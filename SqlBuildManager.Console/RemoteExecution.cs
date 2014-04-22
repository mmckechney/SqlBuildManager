using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using SqlBuildManager.ServiceClient;
using SqlBuildManager.ServiceClient.Sbm.BuildService;
using con = SqlBuildManager.Interfaces.Console;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.MultiDb;
using SqlSync.SqlBuild.Remote;
using System.Text;
namespace SqlBuildManager.Console
{
    class RemoteExecution
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private BuildSettings settings = null;
        private string settingsFile = string.Empty;
        string[] args = new string[0];
        public RemoteExecution(string settingsFile)
        {
            this.settingsFile = settingsFile;
        }
        public RemoteExecution(string[] arguments)
        {
            this.args = arguments;
        }
        public int Execute()
        {
            if (this.settingsFile.Length > 0) //path for using .resp file...
            {
                this.settings = DeserializeBuildSettingsFile(this.settingsFile);

                //If remote servers is set to "local", glean the servers from the multi-database configuration
                if (this.settings.RemoteExecutionServers[0].ServerName.ToLower() == "derive")
                {
                    string[] exeServers = RemoteHelper.GetUniqueServerNamesFromMultiDb(this.settings.MultiDbTextConfig);
                    List<ServerConfigData> tmpSrv = new List<ServerConfigData>();
                    foreach (string serv in exeServers)
                        tmpSrv.Add(new ServerConfigData() { ServerName = serv });

                    this.settings.RemoteExecutionServers = tmpSrv;
                }


                //Go ahead and validate the availability of the designated servers
                if (this.settings != null)
                {
                    string[] errorMessages;
                    int tmp = ValidateRemoteServerAvailability(ref this.settings,out errorMessages);
                    if (tmp != 0)
                    {
                        for (int i = 0; i < errorMessages.Length; i++)
                        {
                            log.Error(errorMessages[i]);
                            System.Console.Error.WriteLine(errorMessages[i]);
                        }
                        return tmp;
                    }
                }
            }
            else if (this.args.Length > 0)
            {
                int result =  DigestAndValidateCommandLineArguments(this.args, out this.settings);
            }
            if (this.settings == null)
            {
                log.Error("Unable to get BuildSettings, returning code 600");
                return (int)SqlBuildManager.Interfaces.Console.ExecutionReturn.UnableToLoadBuildSettings;
            }
            else
            {
                log.Debug("Successfully created BuildSettings object");
            }

            //Validate the load distribution...
            List<string> untaskedExeServers;
            List<string> unassignedDbServers;
            BuildServiceManager manager = new BuildServiceManager();
            manager.ValidateLoadDistribution(settings.DistributionType, settings.RemoteExecutionServers, settings.MultiDbTextConfig, out untaskedExeServers, out unassignedDbServers);
            if (unassignedDbServers.Count > 0)
            {
                string message = String.Format("The following database servers will not be updated with the current distribution type and remote execution server settings:\r\n{0}", String.Join("\r\n", unassignedDbServers.ToArray()));
                log.Error(message);
                System.Console.Error.WriteLine("Some databases will not get updated with the current settings. See \"SqlBuildManager.Console.log\" for details");

                return (int)con.ExecutionReturn.UnassignedDatabaseServers;
            }

            if (untaskedExeServers.Count > 0)
            {

                log.WarnFormat("The following remote execution servers will not be tasked with the current distribution type and remote execution server settings:\r\n{0}", String.Join("\r\n", untaskedExeServers.ToArray()));
                
                List<ServerConfigData> remaining = (from r in this.settings.RemoteExecutionServers
                                                    where !(from u in untaskedExeServers select u).Contains(r.ServerName)
                                                    select r).ToList();
                
                string[] rs = (from r in remaining select r.ServerName).ToArray();
                log.WarnFormat("The remaining execution servers are:\r\n{0}", String.Join("\r\n", rs));

                this.settings.RemoteExecutionServers = remaining;
             }
        
            manager.SubmitBuildRequest(this.settings, this.settings.DistributionType);
            System.Threading.Thread.Sleep(500);
            bool recheckStatus = true;
            List<ServerConfigData> stat = null;
            while (recheckStatus)
            {

               stat =  manager.GetServiceStatus().ToList();
               IEnumerable<ServerConfigData> complete = from s in stat
                                                        where
                                                            s.ServiceReadiness == ServiceReadiness.Error || s.ServiceReadiness == ServiceReadiness.ReadyToAccept ||
                                                            s.ServiceReadiness == ServiceReadiness.ProcessingCompletedSuccessfully || s.ServiceReadiness == ServiceReadiness.Unknown ||
                                                            s.ServiceReadiness == ServiceReadiness.PackageValidationError
                                                        select s;

                if(complete.Count() != this.settings.RemoteExecutionServers.Count())
                {
                    System.Threading.Thread.Sleep(500);
                }
                else
                {
                    recheckStatus = false;
                }
            }


            foreach (ServerConfigData data in stat)
            {
                log.InfoFormat("{0} returned with Execution Return = {1}, Service Readiness = {2}" , data.ServerName, Enum.GetName(typeof(ExecutionReturn), data.ExecutionReturn), Enum.GetName(typeof(ServiceReadiness), data.ServiceReadiness));
            }


            IEnumerable<ServerConfigData> hadError = from s in stat
                                                     where
                                                         s.ServiceReadiness == ServiceReadiness.Error || s.ExecutionReturn !=  ExecutionReturn.Successful
                                                     select s;

            if (hadError.Count() > 0)
            {
                bool success = BuildFailureDatabaseConfig(settings.SqlBuildManagerProjectFileName, hadError, ref manager);
                if (!success)
                    log.Error("Unable to retrieve the failure database configuration data");

                System.Console.WriteLine("One or more remote execution servers encountered an execution error. Check the \"SqlBuildManager.Console.log\" file for details");
                return (int)con.ExecutionReturn.OneOrMoreRemoteServersHadError;
            }
            else
                return (int)con.ExecutionReturn.Successful;

        }

        private static bool BuildFailureDatabaseConfig(string sqlBuildFileName, IEnumerable<ServerConfigData> hadErrorServers, ref BuildServiceManager manager)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                foreach (ServerConfigData cfg in hadErrorServers)
                {
                    sb.AppendLine(manager.GetFailureDatabasesConfig(cfg.TcpServiceEndpoint));
                }
                string fileFormat = @"{0}\{1}-{2}.cfg";
                string fileName = String.Format(fileFormat, Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location),
                    sqlBuildFileName,
                    DateTime.Now.ToString().Replace('/', '-').Replace(':', '.'));
                File.WriteAllText(fileName, sb.ToString());
                return true;
            }
            catch (Exception exe)
            {
                log.Error("Unable to get/save data", exe);
                return false;
            }
            
        }
        private static BuildSettings DeserializeBuildSettingsFile(string fileName)
        {
            BuildSettings tmpSettings = null;
            if (File.Exists(fileName))
            {
                try
                {
                    using (StreamReader sr = new StreamReader(fileName))
                    {
                        System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(BuildSettings));
                        object obj = serializer.Deserialize(sr);
                        tmpSettings = (BuildSettings)obj;
                    }
                }
                catch (Exception e)
                {
                    log.Error("Unable to deserialize .resp file: " + fileName, e);
                }

            }
            else
            {
                log.Error("Unable to find .resp file: " + fileName);
               // this.lastError = "Specified file \"" + fileName + "\" does not exist.";
            }

            return tmpSettings;
        }
        /// <summary>
        /// Accepts the incomming command line arguments array and produces a validated BuildSettings object
        /// </summary>
        /// <param name="args">Command line arguments array</param>
        /// <param name="setting">Populated build settings object</param>
        /// <returns>Zero (0) if validated, otherwise an error code</returns>
        private static int DigestAndValidateCommandLineArguments(string[] args, out BuildSettings setting)
        {
            CommandLineArgs cmd = CommandLine.ParseCommandLineArg(args);
            string[] errorMessages;
            setting = null;
            int tmpReturn = Validation.ValidateCommonCommandLineArgs(cmd,out errorMessages);
            if(tmpReturn != 0)
            {
                for(int i=0;i<errorMessages.Length;i++)
                {
                    log.Error(errorMessages[i]);
                    System.Console.Error.WriteLine(errorMessages[i]);
                }
                return tmpReturn;
            }

            if(cmd.RemoteServers.Length == 0)
            {
                string err = "The command line arguments is missing a value for \"RemoteServers\". This is required for a remote server execution";
                log.Error(err);
                System.Console.Error.WriteLine(err);
                return -700;
            }

            if(!File.Exists(cmd.RemoteServers) && cmd.RemoteServers.ToLower() != "derive")
            {
                string err = "The command line arguments value for \"RemoteServers\" is not a valid file name, nor is the value set to \"derive\"";
                log.Error(err);
                System.Console.Error.WriteLine(err);
                return -701;
            }

            if(cmd.DistributionType.Length == 0)
            {
                string err = "The command line arguments is missing a value for \"DistributionType\". This is required for a remote server execution";
                log.Error(err);
                System.Console.Error.WriteLine(err);
                return -702;
            }

            if(cmd.DistributionType.ToLower() != "equal" && cmd.DistributionType.ToLower() != "local")
            {
                string err = "The command line argument \"DistributionType\" has an invalid value. Allowed values are \"equal\" or \"local\"";
                log.Error(err);
                System.Console.Error.WriteLine(err);
                return -703;
            }

            MultiDbData multiDb;
            int valRet = Validation.ValidateAndLoadMultiDbData(cmd.MultiDbRunConfigFileName, out multiDb, out errorMessages);
            if (valRet != 0)
            {
                for (int i = 0; i < errorMessages.Length; i++)
                {
                    log.Error(errorMessages[i]);
                    System.Console.Error.WriteLine(errorMessages[i]);
                }
                return valRet;
            }

            List<string> remote = RemoteHelper.GetRemoteExecutionServers(cmd.RemoteServers, multiDb);
            
            List<ServerConfigData> remoteServer = null;
            int statReturn = ValidateRemoteServerAvailability(remote, out remoteServer, out errorMessages);
            if(statReturn != 0)
            {
                 for (int i = 0; i < errorMessages.Length; i++)
                {
                    log.Error(errorMessages[i]);
                    System.Console.Error.WriteLine(errorMessages[i]);
                }
                return statReturn;
            }


            //Now that all of the validation is complete... create the settings object to return
            setting = new BuildSettings();
            setting.TimeoutRetryCount = cmd.AllowableTimeoutRetries;
            setting.AlternateLoggingDatabase = cmd.LogToDatabaseName;
            setting.Description = cmd.Description;
            if (cmd.DistributionType.ToLower() == "equal")
                setting.DistributionType = DistributionType.EqualSplit;
            else
                setting.DistributionType = DistributionType.OwnMachineName;
            setting.IsTransactional = cmd.Transactional;
            setting.IsTrialBuild = cmd.Trial;
            setting.LocalRootLoggingPath = cmd.RootLoggingPath;

            string cfg = MultiDbHelper.ConvertMultiDbDataToTextConfig(multiDb);
            setting.MultiDbTextConfig = cfg.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);


            setting.RemoteExecutionServers = remoteServer;
            setting.SqlBuildManagerProjectFileName = Path.GetFileName(cmd.BuildFileName);

            //setting.SqlBuildManagerProjectContents = File.ReadAllBytes(cmd.BuildFileName);
            setting.SqlBuildManagerProjectContents = SqlBuildFileHelper.CleanProjectFileForRemoteExecution(cmd.BuildFileName);
            return 0;
        }

        /// <summary>
        /// Accepts the list of remote server names and outputs a List of ServerConfigData objects and potentially error messages
        /// </summary>
        /// <param name="remoteServers">List of remote server names</param>
        /// <param name="remoteServerData">Output list of ServerConfigData objects</param>
        /// <param name="errorMessages">Output array or error messages (if any)</param>
        /// <returns>Zero (0) if validated, otherwise an error code</returns>
        private static int ValidateRemoteServerAvailability(List<string> remoteServers, out List<ServerConfigData> remoteServerData, out string[] errorMessages)
        {

            List<string> errors = new List<string>();
            int returnVal = 0;

            if (remoteServers == null || remoteServers.Count == 0)
            {
                errors.Add("No remote execution servers were specified.");
                errorMessages = errors.ToArray();
                remoteServerData = null;
                returnVal = -752;
                return returnVal;
            }

            try
            {
                BuildServiceManager buildManager = new BuildServiceManager(remoteServers);
                remoteServerData = buildManager.GetServiceStatus().ToList();

                foreach (ServerConfigData cd in remoteServerData)
                {
                    if (cd.ServiceReadiness != ServiceReadiness.ReadyToAccept)
                    {
                        errors.Add("Remote service status for " + cd.ServerName + " is " + Enum.GetName(typeof(ServiceReadiness), cd.ServiceReadiness));
                        returnVal = -750;
                    }
                }
            }
            catch (Exception exe)
            {
                errors.Add("Unable to gather remote service status");
                errors.Add(exe.ToString());
                remoteServerData = null;
                returnVal = -751;
            }

            errorMessages = errors.ToArray();
            return returnVal;
        }
        private static int ValidateRemoteServerAvailability(ref BuildSettings settings, out string[] errorMessages)
        {
            List<string> remoteServers = (from s in settings.RemoteExecutionServers select s.ServerName).ToList(); ;
            List<ServerConfigData> remoteServerData;

            int returnVal = ValidateRemoteServerAvailability(remoteServers, out remoteServerData, out errorMessages);
            settings.RemoteExecutionServers = remoteServerData;

            return returnVal;
        }
    }
}
