using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Configuration;

namespace SqlSync.SqlBuild
{
    public class CommandLine
    {
        /// <summary>
        /// Parses out the command line arguments array and populates a CommandLineArgs object
        /// </summary>
        /// <param name="args">command line arguments array</param>
        /// <returns>Populated CommandLineArgs object</returns>
        public static CommandLineArgs ParseCommandLineArg(string[] args)
        {
            CommandLineArgs cmdLine = new CommandLineArgs();

            StringDictionary dict = Arguments.ParseArguments(args);
            if(dict.ContainsKey("action"))
            {
                try
                {
                    var actionType = (CommandLineArgs.ActionType)Enum.Parse(typeof(CommandLineArgs.ActionType), dict["action"].ToString(), true);
                    cmdLine.Action = actionType;
                }catch
                {
                    cmdLine.Action = CommandLineArgs.ActionType.Error;
                }
            }

            switch (cmdLine.Action)
            {
                case CommandLineArgs.ActionType.Remote:
                    if (dict.ContainsKey("remoteservers"))
                    {
                        cmdLine.RemoteArgs.RemoteServers = dict["remoteservers"];
                    }
                    if (dict.ContainsKey("distributiontype"))
                    {
                        cmdLine.RemoteArgs.DistributionType = dict["distributiontype"];
                    }
                    break;
                case CommandLineArgs.ActionType.Threaded:
                    break;
                case CommandLineArgs.ActionType.Package:
                    break;
                case CommandLineArgs.ActionType.PolicyCheck:
                    break;
                case CommandLineArgs.ActionType.GetHash:
                    break;
                case CommandLineArgs.ActionType.CreateBackout:
                    break;
                case CommandLineArgs.ActionType.GetDifference:
                    break;
                case CommandLineArgs.ActionType.Synchronize:
                    break;
                case CommandLineArgs.ActionType.Build:
                    cmdLine.BuildFileName = dict["packagename"];
                    break;
                default:
                  
                    break;

            }

            if (dict.ContainsKey("override"))
            {
                cmdLine.OverrideDesignated = true;
                if (dict["override"].ToLower().Trim().EndsWith(".multidb") || dict["override"].ToLower().Trim().EndsWith(".cfg") || dict["override"].ToLower().EndsWith("multidbq") || dict["override"].ToLower().EndsWith(".sql"))
                    cmdLine.MultiDbRunConfigFileName = dict["override"];
                else
                    cmdLine.ManualOverRideSets = dict["override"];
            }

            if (dict.ContainsKey("auto"))
            {
                cmdLine.AutoScriptingArgs.AutoScriptDesignated = true;
                cmdLine.AutoScriptingArgs.AutoScriptFileName = dict["auto"];
            }

            if (dict.ContainsKey("server"))
                cmdLine.Server = dict["server"];

            if (dict.ContainsKey("log"))
                cmdLine.LogFileName = dict["log"];

            if (dict.ContainsKey("test"))
            {
                cmdLine.StoredProcTestingArgs.SprocTestDesignated = true;
                cmdLine.StoredProcTestingArgs.SpTestFile = dict["test"];
            }

            if (dict.ContainsKey("database"))
                cmdLine.Database = dict["database"];

            if (dict.ContainsKey("scriptlogfile"))
                cmdLine.ScriptLogFileName = dict["scriptlogfile"];

            if (dict.ContainsKey("rootloggingpath"))
                cmdLine.RootLoggingPath = dict["rootloggingpath"].Trim();

            bool val;
            if (dict.ContainsKey("logastext") && Boolean.TryParse(dict["logastext"], out val))
            {
                cmdLine.LogAsText = val;
            }

            bool trial;
            if (dict.ContainsKey("trial") && Boolean.TryParse(dict["trial"], out trial))
            {
                cmdLine.Trial = trial;
            }

            if (dict.ContainsKey("scriptsrcdir"))
                cmdLine.ScriptSrcDir = dict["scriptsrcdir"];

            if (dict.ContainsKey("username"))
                cmdLine.AuthenticationArgs.UserName = dict["username"];

            if (dict.ContainsKey("password"))
                cmdLine.AuthenticationArgs.Password = dict["password"];

            if (dict.ContainsKey("logtodatabasename"))
                cmdLine.LogToDatabaseName = dict["logtodatabasename"];

            if (dict.ContainsKey("description"))
                cmdLine.Description = dict["description"];

            if (dict.ContainsKey("packagename"))
            {
                cmdLine.BuildFileName = dict["packagename"];
            }

            if (dict.ContainsKey("directory"))
                cmdLine.Directory = dict["directory"];

            bool isTrans;
            if (dict.ContainsKey("transactional") && Boolean.TryParse(dict["transactional"], out isTrans))
            {
                cmdLine.Transactional = isTrans;
            }

            int allowableTimeoutRetries = 0;
            if(dict.ContainsKey("timeoutretrycount"))
            {
                if(int.TryParse(dict["timeoutretrycount"],out allowableTimeoutRetries))
                    cmdLine.AllowableTimeoutRetries = allowableTimeoutRetries;
            }

            if (dict.ContainsKey("golddatabase"))
                cmdLine.SynchronizeArgs.GoldDatabase = dict["golddatabase"];


            if (dict.ContainsKey("goldserver"))
                cmdLine.SynchronizeArgs.GoldServer = dict["goldserver"];

            bool cont;
            if (dict.ContainsKey("continueonfailure") && Boolean.TryParse(dict["continueonfailure"], out cont))
                cmdLine.ContinueOnFailure = cont;

            if (dict.ContainsKey("platinumdacpac"))
                cmdLine.DacPacArgs.PlatinumDacpac = dict["platinumdacpac"];

            if (dict.ContainsKey("targetdacpac"))
                cmdLine.DacPacArgs.TargetDacpac = dict["targetdacpac"];

            bool forceCustom;
            if (dict.ContainsKey("forcecustomdacpac") && Boolean.TryParse(dict["forcecustomdacpac"], out forceCustom))
                cmdLine.DacPacArgs.ForceCustomDacPac = forceCustom;

            if (dict.ContainsKey("platinumdbsource"))
                cmdLine.DacPacArgs.PlatinumDbSource = dict["platinumdbsource"];

            if (dict.ContainsKey("platinumserversource"))
                cmdLine.DacPacArgs.PlatinumServerSource = dict["platinumserversource"];

            if (dict.ContainsKey("buildrevision"))
                cmdLine.BuildRevision = dict["buildrevision"];

            if (dict.ContainsKey("remotedberrorlist"))
                cmdLine.RemoteArgs.RemoteDbErrorList = dict["remotedberrorlist"];

            if (dict.ContainsKey("remoteerrordetail"))
                cmdLine.RemoteArgs.RemoteErrorDetail = dict["remoteerrordetail"];

            if (dict.ContainsKey("outputsbm"))
                cmdLine.OutputSbm = dict["outputsbm"];

            if (dict.ContainsKey("savedcreds"))
                cmdLine.AuthenticationArgs.SavedCreds = true;

            if (dict.ContainsKey("testconnectivity"))
                cmdLine.RemoteArgs.TestConnectivity = true;

            if (dict.ContainsKey("azureremotestatus"))
                cmdLine.RemoteArgs.AzureRemoteStatus = true;

            if (dict.ContainsKey("outputcontainersasurl"))
                cmdLine.BatchArgs.OutputContainerSasUrl = dict["outputcontainersasurl"];

            bool del;
            if (dict.ContainsKey("deletebatchpool") && Boolean.TryParse(dict["deletebatchpool"], out del))
            {
                cmdLine.BatchArgs.DeleteBatchPool = del;
            }

            int node;
            if (dict.ContainsKey("batchnodecount") && Int32.TryParse(dict["batchnodecount"], out node))
            {
                cmdLine.BatchArgs.BatchNodeCount = node;
            }

            if (dict.ContainsKey("batchaccountname"))
                cmdLine.BatchArgs.BatchAccountName = dict["batchaccountname"];

            if (String.IsNullOrEmpty(cmdLine.BatchArgs.BatchAccountName))
                cmdLine.BatchArgs.BatchAccountName = ConfigurationManager.AppSettings["BatchAccountName"];

            if (dict.ContainsKey("batchaccountkey"))
                cmdLine.BatchArgs.BatchAccountKey = dict["batchaccountkey"];

            if (String.IsNullOrEmpty(cmdLine.BatchArgs.BatchAccountKey))
                cmdLine.BatchArgs.BatchAccountKey = ConfigurationManager.AppSettings["BatchAccountKey"];

            if (dict.ContainsKey("batchaccounturl"))
                cmdLine.BatchArgs.BatchAccountUrl = dict["batchaccounturl"];

            if (String.IsNullOrEmpty(cmdLine.BatchArgs.BatchAccountUrl))
                cmdLine.BatchArgs.BatchAccountUrl = ConfigurationManager.AppSettings["BatchAccountUrl"];

            if (dict.ContainsKey("storageaccountname"))
                cmdLine.BatchArgs.StorageAccountName = dict["storageaccountname"];

            if (String.IsNullOrEmpty(cmdLine.BatchArgs.StorageAccountName))
                cmdLine.BatchArgs.StorageAccountName = ConfigurationManager.AppSettings["StorageAccountName"];

            if (dict.ContainsKey("storageaccountkey"))
                cmdLine.BatchArgs.StorageAccountKey = dict["storageaccountkey"];

            if (String.IsNullOrEmpty(cmdLine.BatchArgs.StorageAccountKey))
                cmdLine.BatchArgs.StorageAccountKey = ConfigurationManager.AppSettings["StorageAccountKey"];

            if (dict.ContainsKey("batchvmsize"))
                cmdLine.BatchArgs.BatchVmSize = dict["batchvmsize"];

            if (String.IsNullOrEmpty(cmdLine.BatchArgs.BatchVmSize))
                cmdLine.BatchArgs.BatchVmSize = ConfigurationManager.AppSettings["BatchVmSize"];





            if (dict.ContainsKey("authtype"))
            {
                switch(dict["authtype"].ToLower())
                {
                    case "windows":
                        cmdLine.AuthenticationArgs.AuthenticationType = Connection.AuthenticationType.Windows;
                        break;
                    case "azureadintegrated":
                        cmdLine.AuthenticationArgs.AuthenticationType = Connection.AuthenticationType.AzureADIntegrated;
                        break;
                    case "azureadpassword":
                        cmdLine.AuthenticationArgs.AuthenticationType = Connection.AuthenticationType.AzureADPassword;
                        break;
                    case "password":
                    default:
                        cmdLine.AuthenticationArgs.AuthenticationType = Connection.AuthenticationType.Password;
                        break;
                }
            }
            else
            {
                if(!string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.Password) && !string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.UserName))
                {
                    cmdLine.AuthenticationArgs.AuthenticationType = Connection.AuthenticationType.Password;
                }
                else
                {
                    cmdLine.AuthenticationArgs.AuthenticationType = Connection.AuthenticationType.Windows;
                }
            }
            
            return cmdLine;
        }

        /// <summary>
        /// Arguments class
        /// /*
        ///* Arguments class: application arguments interpreter
        ///*
        ///* Authors:		R. LOPES
        ///* Contributors:	R. LOPES
        ///* Created:		25 October 2002
        ///* Modified:		28 October 2002
        ///*
        ///* Version:		1.0
        ///*/
        /// </summary>
        public class Arguments
        {


            public static StringDictionary ParseArguments(string[] Args)
            {
                StringDictionary Parameters = new StringDictionary();
                Regex Spliter = new Regex(@"^-{1,2}|^/|=|:", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                Regex Remover = new Regex(@"^['""]?(.*?)['""]?$", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                string Parameter = null;
                string[] Parts;

                // Valid parameters forms:
                // {-,/,--}param{ ,=,:}((",')value(",'))
                // Examples: -param1 value1 --param2 /param3:"Test-:-work" /param4=happy -param5 '--=nice=--'
                foreach (string Txt in Args)
                {
                    // Look for new parameters (-,/ or --) and a possible enclosed value (=,:)
                     Parts = Spliter.Split(Txt, 3);
                    switch (Parts.Length)
                    {
                        // Found a value (for the last parameter found (space separator))
                        case 1:
                            if (Parameter != null)
                            {
                                if (!Parameters.ContainsKey(Parameter))
                                {
                                    Parts[0] = Remover.Replace(Parts[0], "$1");
                                    Parameters.Add(Parameter, Parts[0]);
                                }
                                Parameter = null;
                            }
                            // else Error: no parameter waiting for a value (skipped)
                            break;
                        // Found just a parameter
                        case 2:
                            // The last parameter is still waiting. With no value, set it to true.
                            if (Parameter != null)
                                if (!Parameters.ContainsKey(Parameter))
                                    Parameters.Add(Parameter, "true");

                            Parameter = Parts[1];
                            break;
                        // Parameter with enclosed value
                        case 3:
                            // The last parameter is still waiting. With no value, set it to true.
                            if (Parameter != null)
                            {
                                if (!Parameters.ContainsKey(Parameter))
                                    Parameters.Add(Parameter, "true");
                            }
                            Parameter = Parts[1].ToLowerInvariant();
                            // Remove possible enclosing characters (",')
                            if (!Parameters.ContainsKey(Parameter))
                            {
                                Parts[2] = Remover.Replace(Parts[2], "$1");
                                Parameters.Add(Parameter, Parts[2]);
                            }
                            Parameter = null;
                            break;
                    }
                }
                // In case a parameter is still waiting
                if (Parameter != null)
                {
                    if (!Parameters.ContainsKey(Parameter)) Parameters.Add(Parameter, "true");
                }

                return Parameters;
            }


        }
    }
}
