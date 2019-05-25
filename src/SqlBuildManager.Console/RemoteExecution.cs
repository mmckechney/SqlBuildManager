﻿using System;
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
        internal string settingsFile = string.Empty;
        internal string[] args = new string[0];
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
            BuildServiceManager manager = new BuildServiceManager();

            int valid = this.ValidateAll(this.args, ref manager, out this.settings);
            if (valid != 0)
                return valid;

        
            List<string> taskedEndpoints = manager.SubmitBuildRequest(this.settings, this.settings.DistributionType);
            System.Threading.Thread.Sleep(2000);
            bool recheckStatus = true;
            List<ServiceStatus> stat = null;
            while (recheckStatus)
            {
                
               stat = manager.GetServiceStatus(taskedEndpoints).ToList();
               var complete = from s in stat
                                where
                                    s.Readiness == ServiceReadiness.Error || s.Readiness == ServiceReadiness.ReadyToAccept ||
                                    s.Readiness == ServiceReadiness.ProcessingCompletedSuccessfully || s.Readiness == ServiceReadiness.Unknown ||
                                    s.Readiness == ServiceReadiness.PackageValidationError
                                select s;

                if (complete.Count() != taskedEndpoints.Count())
                {
                    System.Threading.Thread.Sleep(500);
                }
                else
                {
                    recheckStatus = false;
                }
            }


            foreach (var data in stat)
            {
                log.InfoFormat("{0} returned with Execution Return = {1}, Service Readiness = {2}" , 
                    data.ServerName, Enum.GetName(typeof(ExecutionReturn), data.ExecutionStatus), Enum.GetName(typeof(ServiceReadiness), data.Readiness));
            }


            var hadError = from s in stat
                            where
                                s.Readiness == ServiceReadiness.Error || s.ExecutionStatus != ExecutionReturn.Successful
                            select s;

            if (hadError.Any())
            {
                bool success = BuildFailureDatabaseConfig(settings.SqlBuildManagerProjectFileName, hadError, ref manager);
                if (!success)
                {
                    log.Error("Unable to retrieve the failure database configuration data");
                }


                //pull the detailed logs
                
                foreach(var item in hadError)
                {
                    try
                    {
                        string error = RemoteAzureHealth.GetErrorDetail(item.Endpoint);
                        log.Warn("Returned error messages:");
                        log.Warn("\r\n"+error);
                    }
                    catch(Exception exe)
                    {
                        log.Error(string.Format("Unable to get error detail from {0}", item.Endpoint), exe);
                    }
                }

                
                //log.Warn("One or more remote execution servers encountered an execution error. Check the \"SqlBuildManager.Console.log\" file for details");
                return (int)con.ExecutionReturn.OneOrMoreRemoteServersHadError;
            }
            else
            {
                return (int)con.ExecutionReturn.Successful;
            }

        }
        public int TestConnectivity()
        {
            BuildServiceManager manager = new BuildServiceManager();

            int valid = this.ValidateAll(this.args, ref manager, out this.settings);
            if (valid != 0)
                return valid;

            IList<ServerConfigData> connectivityResults = manager.TestDatabaseConnectivity(this.settings, this.settings.DistributionType);

            var err = from s in connectivityResults
                      from c in s.ConnectionTestResults
                      where s.ConnectionTestResults.Count == 0 || c.Successful == false
                      select new {Server = c.ServerName, Database = c.DatabaseName};

            if (err.Any())
            {
                log.Error(
                    String.Format("Connectivity Errors to the following {0} Server/Databases:", err.Count()));
          
                var errorList =
                    err.Select(combined => combined.Server +": "+ combined.Database).Aggregate((start, add) => start + "\r\n" + add);
                log.Error(errorList);
                return err.Count();
            }

            System.Console.Out.WriteLine("TestConnectivity passed for all Server/Databases");
            return 0;

        }
        private static bool BuildFailureDatabaseConfig(string sqlBuildFileName, IEnumerable<ServiceStatus> hadErrorEndpoints, ref BuildServiceManager manager)
        {
            try
            {
                StringBuilder sb = new StringBuilder();
                foreach (ServiceStatus cfg in hadErrorEndpoints)
                {
                    sb.AppendLine(manager.GetFailureDatabasesConfig(cfg.Endpoint));
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
        internal static BuildSettings DeserializeBuildSettingsFile(string fileName)
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
        internal static int DigestAndValidateCommandLineArguments(string[] args, out BuildSettings setting)
        {
            CommandLineArgs cmd = CommandLine.ParseCommandLineArg(args);
            string[] errorMessages;
            setting = null;

            #region .: Simple Validation or settings :.
            int tmpReturn = Validation.ValidateCommonCommandLineArgs(ref cmd,out errorMessages);
            if(tmpReturn != 0)
            {
                for(int i=0;i<errorMessages.Length;i++)
                {
                    log.Error(errorMessages[i]);
                    log.Error(errorMessages[i]);
                }
                return tmpReturn;
            }

            if(cmd.RemoteArgs.RemoteServers.Length == 0)
            {
                string err = "The command line arguments is missing a value for \"RemoteServers\". This is required for a remote server execution";
                log.Error(err);
                log.Error(err);
                return -700;
            }

            if(!File.Exists(cmd.RemoteArgs.RemoteServers) && cmd.RemoteArgs.RemoteServers.ToLower() != "derive" && cmd.RemoteArgs.RemoteServers.ToLower() != "azure")
            {
                string err = "The command line arguments value for \"RemoteServers\" is not a valid file name, nor is the value set to \"derive\" or \"azure\"";
                log.Error(err);
                log.Error(err);
                return -701;
            }

            if (cmd.RemoteArgs.DistributionType.Length == 0 && cmd.RemoteArgs.RemoteServers.ToLower() != "azure")
            {
                string err = "The command line arguments is missing a value for \"DistributionType\". This is required for non-Azure remote server execution";
                log.Error(err);
                log.Error(err);
                return -702;
            }

            if (cmd.RemoteArgs.DistributionType.ToLower() == "local" && cmd.RemoteArgs.RemoteServers.ToLower() == "azure")
            {
                string err = "The command line combination of  DistributionType=local and RemoteServers=azure is not allowed.";
                log.Error(err);
                log.Error(err);
                return -704;
            }

            if (cmd.RemoteArgs.RemoteServers.ToLower() != "azure")
            {
                if (cmd.RemoteArgs.DistributionType.ToLower() != "equal" && cmd.RemoteArgs.DistributionType.ToLower() != "local")
                {
                    string err = "The command line argument \"DistributionType\" has an invalid value. Allowed values are \"equal\" or \"local\"";
                    log.Error(err);
                    log.Error(err);
                    return -703;
                }
            }
            else
            {
                cmd.RemoteArgs.DistributionType = "equal";
            }

             if (cmd.RemoteArgs.RemoteServers.ToLower() == "azure" && (String.IsNullOrWhiteSpace(cmd.AuthenticationArgs.UserName) || string.IsNullOrWhiteSpace(cmd.AuthenticationArgs.Password)))
             {
                  string err = "When running a remote execution on Azure, a username and password are required";
                    log.Error(err);
                    log.Error(err);
                    return -707;
             }

            #endregion

             MultiDbData multiDb;
            int valRet = Validation.ValidateAndLoadMultiDbData(cmd.MultiDbRunConfigFileName,cmd, out multiDb, out errorMessages);
            if (valRet != 0)
            {
                for (int i = 0; i < errorMessages.Length; i++)
                {
                    log.Error(errorMessages[i]);
                    log.Error(errorMessages[i]);
                }
                return valRet;
            }

            if (string.IsNullOrWhiteSpace(cmd.DacPacArgs.PlatinumDacpac) && !string.IsNullOrWhiteSpace(cmd.DacPacArgs.PlatinumDbSource) && !string.IsNullOrWhiteSpace(cmd.DacPacArgs.PlatinumServerSource))
            {
                log.InfoFormat("Extracting Platinum Dacpac from {0} : {1}", cmd.DacPacArgs.PlatinumServerSource, cmd.DacPacArgs.PlatinumDbSource);
                string dacpacName = Path.Combine(cmd.RootLoggingPath, cmd.DacPacArgs.PlatinumDbSource + ".dacpac");

                if (!DacPacHelper.ExtractDacPac(cmd.DacPacArgs.PlatinumDbSource, cmd.DacPacArgs.PlatinumServerSource, cmd.AuthenticationArgs.UserName, cmd.AuthenticationArgs.Password, dacpacName))
                {
                    log.ErrorFormat("Error creating the Platinum dacpac from {0} : {1}", cmd.DacPacArgs.PlatinumServerSource, cmd.DacPacArgs.PlatinumDbSource);
                }
                cmd.DacPacArgs.PlatinumDacpac = dacpacName;
            }

            //If there is a platinum dacpac specified...
            var dacPacReturn = Validation.ValidateAndLoadPlatinumDacpac(ref cmd, ref multiDb);
            if(dacPacReturn == (int)con.ExecutionReturn.DacpacDatabasesInSync)
            {
                log.Error("The dacpac databases are already in sync.");
                return (int)con.ExecutionReturn.DacpacDatabasesInSync;
            }
            
            List<string> remote = null;
            if (cmd.RemoteArgs.RemoteServers.ToLower() != "azure")
            {
                remote = RemoteHelper.GetRemoteExecutionServers(cmd.RemoteArgs.RemoteServers, multiDb);
            }
            else
            {
                BuildServiceManager manager = new BuildServiceManager();
                List<ServerConfigData> serverData = manager.GetListOfAzureInstancePublicUrls();
                remote = serverData.Select(s => s.ServerName).ToList();
            }


            //Validate that all of the Azure servers are accepting commands
            List<ServerConfigData> remoteServer = null;
            Protocol p = (cmd.RemoteArgs.RemoteServers.ToLower() == "azure") ? Protocol.AzureHttp : Protocol.Tcp;
            int statReturn = ValidateRemoteServerAvailability(remote,p, out remoteServer, out errorMessages);
            if(statReturn != 0)
            {
                 for (int i = 0; i < errorMessages.Length; i++)
                {
                    log.Error(errorMessages[i]);
                    log.Error(errorMessages[i]);
                }
                return statReturn;
            }

          

            //Now that all of the validation is complete... create the settings object to return
            setting = new BuildSettings();
            setting.BuildRunGuid = Guid.NewGuid().ToString();
            setting.TimeoutRetryCount = cmd.TimeoutRetryCount;
            setting.AlternateLoggingDatabase = cmd.LogToDatabaseName;
            setting.Description = cmd.Description;
            if (cmd.RemoteArgs.DistributionType.ToLower() == "equal" || cmd.RemoteArgs.RemoteServers.ToLower() == "azure")
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

            setting.SqlBuildManagerProjectContents = SqlBuildFileHelper.CleanProjectFileForRemoteExecution(cmd.BuildFileName);

            setting.DbUserName = cmd.AuthenticationArgs.UserName;
            setting.DbPassword = cmd.AuthenticationArgs.Password;
            setting.AuthenticationType = cmd.AuthenticationArgs.AuthenticationType;

            if (!string.IsNullOrEmpty(cmd.DacPacArgs.PlatinumDacpac))
            {
                setting.PlatinumDacpacContents = File.ReadAllBytes(cmd.DacPacArgs.PlatinumDacpac);
                setting.PlatinumDacpacFileName = Path.GetFileName(cmd.DacPacArgs.PlatinumDacpac);
            }

            setting.BuildRevision = cmd.BuildRevision;
            return 0;
        }

       

        private int ValidateAll(string[] cmdArgs, ref BuildServiceManager manager, out BuildSettings bldSettings)
        {
            int returnVal = 0;
            returnVal = ValidateRemoteArguments(cmdArgs, out bldSettings);
            if (returnVal != 0)
                return returnVal;

            return ValidateLoadDistribution(ref manager, ref bldSettings);
        }
        /// <summary>
        /// Consumes the build command arguments or a .resp file to create a BuildSettings object 
        /// </summary>
        /// <param name="cmdArgs">The command line arguments provided</param>
        /// <param name="bldSettings">Returns a populated BuildSettings object</param>
        /// <returns>Non-zero if BuildSettings object was not successfully created.</returns>
        private int ValidateRemoteArguments(string[] cmdArgs, out BuildSettings bldSettings)
        {
            bldSettings = null;
            int result = 0;
            if (this.settingsFile.Length > 0) //path for using .resp file...
            {
                bldSettings = DeserializeBuildSettingsFile(this.settingsFile);

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
                    int tmp = ValidateRemoteServerAvailability(ref this.settings, out errorMessages);
                    if (tmp != 0)
                    {
                        for (int i = 0; i < errorMessages.Length; i++)
                        {
                            log.Error(errorMessages[i]);
                            log.Error(errorMessages[i]);
                        }
                        return tmp;
                    }
                }
            }
            else if (this.args.Length > 0)
            {
                result = DigestAndValidateCommandLineArguments(cmdArgs, out bldSettings);
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
            return result;
        }
        /// <summary>
        /// Validate to make sure that all of the configured databases/ remote servers will be hit via the settings.
        /// 
        /// </summary>
        /// <param name="manager">BuildServiceManager object</param>
        /// <param name="bldSettings">BuildSettings object</param>
        /// <returns>Return non-zero if there will be skipped databases but will return a zero and log a lost of execution agents that will be idle</returns>
        private int ValidateLoadDistribution(ref BuildServiceManager manager, ref BuildSettings bldSettings)
        {
            List<string> untaskedExeServers;
            List<string> unassignedDbServers;
            manager.ValidateLoadDistribution(settings.DistributionType, settings.RemoteExecutionServers, settings.MultiDbTextConfig, out untaskedExeServers, out unassignedDbServers);
            if (unassignedDbServers.Count > 0)
            {
                string message = String.Format("The following database servers will not be updated with the current distribution type and remote execution server settings:\r\n{0}", String.Join("\r\n", unassignedDbServers.ToArray()));
                log.Error(message);
                log.Error("Some databases will not get updated with the current settings. See \"SqlBuildManager.Console.log\" for details");

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
            return 0;
        }

        /// <summary>
        /// Accepts the list of remote server names and outputs a List of ServerConfigData objects and potentially error messages
        /// </summary>
        /// <param name="remoteServers">List of remote server names</param>
        /// <param name="remoteServerData">Output list of ServerConfigData objects</param>
        /// <param name="errorMessages">Output array or error messages (if any)</param>
        /// <returns>Zero (0) if validated, otherwise an error code</returns>
        internal static int ValidateRemoteServerAvailability(List<string> remoteServers, Protocol protocol, out List<ServerConfigData> remoteServerData, out string[] errorMessages)
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
                BuildServiceManager buildManager = new BuildServiceManager(remoteServers, protocol);
                remoteServerData = buildManager.GetServiceStatus().ToList();

                foreach (ServerConfigData cd in remoteServerData)
                {
                    if (cd.ServiceReadiness != ServiceReadiness.ReadyToAccept)
                    {
                        if (!buildManager.SubmitServiceResetRequest(cd))
                        {
                            errors.Add("Remote service status for " + cd.ServerName + " is " + Enum.GetName(typeof(ServiceReadiness), cd.ServiceReadiness));
                            returnVal = -750;
                        }
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

            int returnVal = ValidateRemoteServerAvailability(remoteServers, Protocol.Tcp, out remoteServerData, out errorMessages);
            settings.RemoteExecutionServers = remoteServerData;

            return returnVal;
        }
    }
}