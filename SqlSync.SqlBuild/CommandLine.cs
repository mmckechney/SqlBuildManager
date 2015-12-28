using System;
using System.Collections.Generic;
using System.Text;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
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
            cmdLine.RawArguments = args;

            StringDictionary dict = Arguments.ParseArguments(args);
            cmdLine.ArgumentCollection = dict;
            if(dict.ContainsKey("action"))
            {
                cmdLine.Action = dict["action"].ToLowerInvariant();
            }

            switch (cmdLine.Action)
            {
                case "remote":
                    if (dict.ContainsKey("remoteservers"))
                    {
                        cmdLine.RemoteServers = dict["remoteservers"];
                    }
                    if (dict.ContainsKey("distributiontype"))
                    {
                        cmdLine.DistributionType = dict["distributiontype"];
                    }
                    break;
                case "threaded":
                    
                    break;
                case "package":
                    
                    break;
                case "policycheck":
                    
                    break;
                case "gethash":
                    
                    break;
                case "createbackout":
                    
                    break;
                case "getdifference":
                    cmdLine.GetDifference = true;
                    break;
                case "synchronize":
                    cmdLine.Synchronize = true;
                    break;
                case "build":
                    cmdLine.BuildDesignated = true;
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
                cmdLine.AutoScriptDesignated = true;
                cmdLine.AutoScriptFileName = dict["auto"];
            }

            if (dict.ContainsKey("server"))
                cmdLine.Server = dict["server"];

            if (dict.ContainsKey("log"))
                cmdLine.LogFileName = dict["log"];

            if (dict.ContainsKey("test"))
            {
                cmdLine.SprocTestDesignated = true;
                cmdLine.SpTestFile = dict["test"];
            }

            if (dict.ContainsKey("database"))
                cmdLine.Database = dict["database"];

            if (dict.ContainsKey("scriptlogfile"))
                cmdLine.ScriptLogFileName = dict["scriptlogfile"];

            if (dict.ContainsKey("rootloggingpath"))
                cmdLine.RootLoggingPath = dict["rootloggingpath"];

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
                cmdLine.UserName = dict["username"];

            if (dict.ContainsKey("password"))
                cmdLine.Password = dict["password"];

            if (dict.ContainsKey("logtodatabasename"))
                cmdLine.LogToDatabaseName = dict["logtodatabasename"];

            if (dict.ContainsKey("description"))
                cmdLine.Description = dict["description"];

            if (dict.ContainsKey("packagename"))
            {
                cmdLine.PackageName = dict["packagename"];
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
                cmdLine.GoldDatabase = dict["golddatabase"];


            if (dict.ContainsKey("goldserver"))
                cmdLine.GoldServer = dict["goldserver"];

            bool cont;
            if (dict.ContainsKey("continueonfailure") && Boolean.TryParse(dict["continueonfailure"], out cont))
                cmdLine.ContinueOnFailure = cont;

            if (dict.ContainsKey("platinumdacpac"))
                cmdLine.PlatinumDacpac = dict["platinumdacpac"];

            if (dict.ContainsKey("targetdacpac"))
                cmdLine.TargetDacpac = dict["targetdacpac"];

            bool forceCustom;
            if (dict.ContainsKey("forcecustomdacpac") && Boolean.TryParse(dict["forcecustomdacpac"], out forceCustom))
                cmdLine.ForceCustomDacPac = forceCustom;

            if (dict.ContainsKey("platinumdbsource"))
                cmdLine.PlatinumDbSource = dict["platinumdbsource"];

            if (dict.ContainsKey("platinumserversource"))
                cmdLine.PlatinumServerSource = dict["platinumserversource"];

            if (dict.ContainsKey("buildrevision"))
                cmdLine.BuildRevision = dict["buildrevision"];

            if (dict.ContainsKey("remotedberrorlist"))
                cmdLine.RemoteDbErrorList = dict["remotedberrorlist"];

            if (dict.ContainsKey("remoteerrordetail"))
                cmdLine.RemoteErrorDetail = dict["remoteerrordetail"];

            if (dict.ContainsKey("outputsbm"))
                cmdLine.OutputSbm = dict["outputsbm"];

            if (dict.ContainsKey("savedcreds"))
                cmdLine.SavedCreds = true;


            if(dict.ContainsKey("authtype"))
            {
                switch(dict["authtype"].ToLower())
                {
                    case "windows":
                        cmdLine.AuthenticationType = Connection.AuthenticationType.WindowsAuthentication;
                        break;
                    case "azuread":
                        cmdLine.AuthenticationType = Connection.AuthenticationType.AzureActiveDirectory;
                        break;
                    case "azurepassword":
                        cmdLine.AuthenticationType = Connection.AuthenticationType.AzureUserNamePassword;
                        break;
                    case "password":
                    default:
                        cmdLine.AuthenticationType = Connection.AuthenticationType.UserNamePassword;
                        break;
                }
            }
            else
            {
                if(!string.IsNullOrWhiteSpace(cmdLine.Password) && !string.IsNullOrWhiteSpace(cmdLine.UserName))
                {
                    cmdLine.AuthenticationType = Connection.AuthenticationType.UserNamePassword;
                }
                else
                {
                    cmdLine.AuthenticationType = Connection.AuthenticationType.WindowsAuthentication;
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
