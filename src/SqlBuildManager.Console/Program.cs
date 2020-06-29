using log4net;
using log4net.Core;
using log4net.Repository.Hierarchy;
using Microsoft.Azure.Storage;
using Microsoft.Azure.Storage.Blob;
using Microsoft.Azure.Storage.RetryPolicies;
using Newtonsoft.Json;
using SqlBuildManager.Enterprise.Policy;
using SqlBuildManager.Interfaces.Console;

using SqlSync.SqlBuild;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.Globalization;
using Microsoft.SqlServer.Management.XEvent;
using Microsoft.Azure.Amqp.Framing;
using Microsoft.SqlServer.Management.Smo;
using System.CommandLine.Parsing;
using System.CommandLine.Builder;
using System.CommandLine.IO;

namespace SqlBuildManager.Console
{

    class Program
    {
        private static ILog log = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        internal static string[] AppendLogFiles = new string[] { "commits.log", "errors.log", "successdatabases.cfg", "failuredatabases.cfg" };

        static async Task Main(string[] args)
        {
            var fn = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
            var currentPath = Path.GetDirectoryName(fn);
            var exeName = Path.GetFileNameWithoutExtension(fn);
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());

            log4net.Config.XmlConfigurator.Configure(logRepository, new FileInfo(Path.Combine(currentPath,"log4net.config")));
            SqlBuildManager.Logging.Configure.SetLoggingPath();

            log.Debug("Received Command: " + String.Join(" | ", args));
           

            if (exeName == "sbm")
            {
                #region System.CommandLine options
                var settingsfileOption = new Option(new string[] { "--settingsfile" }, "Saved settings file to load parameters from")
                {
                    Argument = new Argument<string>("settingsfile"),
                    Required = false
                };
                var overrideOption = new Option(new string[] { "--override" }, "File containing the target database settings (usually a formatted .cfg file)")
                {
                    Argument = new Argument<string>("override"),
                    Required = false
                };
                var serverOption = new Option(new string[] { "--server" }, "1) Name of a server for single database run or 2) source server for scripting or runtime configuration")
                {
                    Argument = new Argument<string>("server"),
                    Required = false
                };
                var databaseOption = new Option(new string[] { "--database" }, "1) Name of a single database to run against or 2) source database for scripting or runtime configuration")
                {
                    Argument = new Argument<string>("database"),
                    Required = false
                };
                var rootloggingpathOption = new Option(new string[] { "--rootloggingpath" }, "Directory to save execution logs (for threaded and remote executions)")
                {
                    Argument = new Argument<string>("rootloggingpath"),
                    Required = false
                };
                var trialOption = new Option(new string[] { "--trial" }, "Whether or not to run in trial mode(default is false)")
                {
                    Argument = new Argument<bool>("trial"),
                    Required = false
                };
                var scriptsrcdirOption = new Option(new string[] { "--scriptsrcdir" }, " [Not recommended] Alternative ability to run against a directory of scripts (vs .sbm or .sbx file)")
                {
                    Argument = new Argument<string>("scriptsrcdir"),
                    Required = false
                };
                var usernameOption = new Option(new string[] { "--username" }, "The username to authenticate against the database if not using integrate auth")
                {
                    Argument = new Argument<string>("username"),
                    Required = false
                };
                var passwordOption = new Option(new string[] { "--password" }, "The password to authenticate against the database if not using integrate auth")
                {
                    Argument = new Argument<string>("password"),
                    Required = false
                };
                var logtodatabasenamedOption = new Option(new string[] { "--logtodatabasename" }, "[Not recommended] Specifies that the SqlBuild_logging logs should go to an alternate database (vs. target).")
                {
                    Argument = new Argument<string>("logtodatabasename"),
                    Required = false
                };
                var descriptionOption = new Option(new string[] { "--description" }, "Description of build (logged with build)")
                {
                    Argument = new Argument<string>("description"),
                    Required = false
                };
                var packagenameOption = new Option(new string[] { "--packagename", "--buildfilename" }, "Name of the .sbm or .sbx file to execute")
                {
                    Argument = new Argument<string>("BuildFileName"),
                    Required = false
                };
                var directoryOption = new Option(new string[] { "--directory" }, "Directory containing 1 or more SBX files to package into SBM zip files")
                {
                    Argument = new Argument<string>("directory"),
                    Required = false
                };
                var transactionalOption = new Option(new string[] { "--transactional" }, "Whether or not to run with a wrapping transaction (default is true)")
                {
                    Argument = new Argument<bool>("transactional"),
                    Required = false
                };
                var timeoutretrycountOption = new Option(new string[] { "--timeoutretrycount" }, "How many retries to attempt if a timeout exception occurs")
                {
                    Argument = new Argument<int>("timeoutretrycount"),
                    Required = false
                };
                var golddatabaseOption = new Option(new string[] { "--golddatabase" }, "The \"gold copy\" database that will serve as the model for what the target database should look like")
                {
                    Argument = new Argument<string>("golddatabase"),
                    Required = false
                };
                var goldserverOption = new Option(new string[] { "--goldserver" }, "The server that the \"gold copy\" database can be found")
                {
                    Argument = new Argument<string>("goldserver"),
                    Required = false
                };
                var continueonfailureOption = new Option(new string[] { "--continueonfailure" }, "Whether or not to continue on the failure of a package (default is false)")
                {
                    Argument = new Argument<bool>("continueonfailure"),
                    Required = false
                };
                var platinumdacpacOption = new Option(new string[] { "--platinumdacpac" }, "Name of the dacpac containing the platinum schema")
                {
                    Argument = new Argument<string>("platinumdacpac"),
                    Required = false
                };
                var targetdacpacOption = new Option(new string[] { "--targetdacpac" }, "Name of the dacpac containing the schema of the database to be updated")
                {
                    Argument = new Argument<string>("targetdacpac"),
                    Required = false
                };
                var forcecustomdacpacOption = new Option(new string[] { "--forcecustomdacpac" }, "USE WITH CAUTION! This will force the dacpac extraction and creation of custom scripts for EVERY target database! Your execution will take much longer.")
                {
                    Argument = new Argument<bool>("forcecustomdacpac"),
                    Required = false
                };
                var platinumdbsourceOption = new Option(new string[] { "--platinumdbsource" }, "Instead of a formally built Platinum Dacpac, target this database as having the desired state schema")
                {
                    Argument = new Argument<string>("platinumdbsource"),
                    Required = false
                };
                var platinumserversourceOption = new Option(new string[] { "--platinumserversource" }, "Instead of a formally built Platinum Dacpac, target a database on this server as having the desired state schema")
                {
                    Argument = new Argument<string>("platinumserversource"),
                    Required = false
                };
                var buildrevisionOption = new Option(new string[] { "--buildrevision" }, "If provided, the build will include an update to a \"Versions\" table and this will be the value used to add to a \"VersionNumber\" column (varchar(max))")
                {
                    Argument = new Argument<string>("buildrevision"),
                    Required = false
                };
                var outputsbmOption = new Option(new string[] { "--outputsbm" }, "Name (and path) of the SBM package to create")
                {
                    Argument = new Argument<string>("outputsbm"),
                    Required = false
                };
                var outputcontainersasurlOption = new Option(new string[] { "--outputcontainersasurl" }, "Override of default storage container for batch output")
                {
                    Argument = new Argument<string>("outputcontainersasurl"),
                    Required = false
                };
                var deletebatchpoolOption = new Option(new string[] { "--deletebatchpool" }, "Whether or not to delete the batch pool servers after an execution (default is false)")
                {
                    Argument = new Argument<bool>("deletebatchpool"),
                    Required = false
                };
                var deletebatchjobOption = new Option(new string[] { "--deletebatchjob" }, "Whether or not to delete the batch job after an execution (default is true)")
                {
                    Argument = new Argument<bool>("deletebatchjob"),
                    Required = false
                };
                var batchnodecountOption = new Option(new string[] { "--batchnodecount" }, "Number of nodes to provision to run the batch job  (default is 10)")
                {
                    Argument = new Argument<int>("batchnodecount"),
                    Required = false
                };
                var batchjobnameOption = new Option(new string[] { "--batchjobname" }, "[Optional] User friendly name for the job. This will also be the container name for the stored logs. Any disallowed URL characters will be removed")
                {
                    Argument = new Argument<string>("batchjobname"),
                    Required = false
                };
                var batchaccountnameOption = new Option(new string[] { "--batchaccountname" }, "String name of the Azure Batch account  [can also be set via BatchAccountName app settings key]")
                {
                    Argument = new Argument<string>("batchaccountname"),
                    Required = false
                };
                var batchaccountkeyOption = new Option(new string[] { "--batchaccountkey" }, "Account Key for the Azure Batch account [can also be set via BatchAccountKey app settings key]")
                {
                    Argument = new Argument<string>("batchaccountkey"),
                    Required = false
                };
                var batchaccounturlOption = new Option(new string[] { "--batchaccounturl" }, "URL for the Azure Batch account [can also be set via BatchAccountUrl app settings key]")
                {
                    Argument = new Argument<string>("batchaccounturl"),
                    Required = false
                };
                var storageaccountnameOption = new Option(new string[] { "--storageaccountname" }, "Name of storage account associated with the Azure Batch account  [can also be set via StorageAccountName app settings key]")
                {
                    Argument = new Argument<string>("storageaccountname"),
                    Required = false
                };
                var storageaccountkeyOption = new Option(new string[] { "--storageaccountkey" }, "Account Key for the storage account  [can also be set via StorageAccountKey app settings key]")
                {
                    Argument = new Argument<string>("storageaccountkey"),
                    Required = false
                };
                var batchvmsizeOption = new Option(new string[] { "--batchvmsize" }, "Size key for VM size required (see https://docs.microsoft.com/en-us/azure/virtual-machines/windows/sizes-general) [can also be set via BatchVmSize app settings key]")
                {
                    Argument = new Argument<string>("batchvmsize"),
                    Required = false
                };
                var batchpoolnameOption = new Option(new string[] { "--batchpoolname" }, "Override for the default pool name of \"SqlBuildManagerPool\"")
                {
                    Argument = new Argument<string>("batchpoolname"),
                    Required = false
                };
                var eventhubconnectionOption = new Option(new string[] { "--eventhubconnection" }, "Event Hub connection string for Event Hub logging of batch execution")
                {
                    Argument = new Argument<string>("eventhubconnection"),
                    Required = false
                };
                var pollbatchpoolstatusOption = new Option(new string[] { "--pollbatchpoolstatus" }, "Whether or not you want to get updated status (true, default) or fire and forget (false)")
                {
                    Argument = new Argument<bool>("pollbatchpoolstatus"),
                    Required = false
                };
                var defaultscripttimeoutOption = new Option(new string[] { "--defaultscripttimeout" }, "Override the default script timeouts set when creating a DACPAC (default is 500)")
                {
                    Argument = new Argument<int>("defaultscripttimeout"),
                    Required = false
                };
                var authtypeOption = new Option(new string[] { "--authtype" }, "Values: \"Windows\", \"AzureADIntegrated\", \"AzureADPassword\", \"Password\" (default)")
                {
                    Argument = new Argument<SqlSync.Connection.AuthenticationType>("authtype"),
                    Required = false
                };
                var whatIfOption = new Option(new string[] { "--whatif" }, "Provides commandline validation and some authentication validation")
                {
                    Argument = new Argument<bool>("whatif"),
                    Required = false
                };
                List<Option> authOptions = new List<Option>()
                {
                    passwordOption,
                    usernameOption
                };

                List<Option> generalbuildAndThreadOptions = new List<Option>()
                {
                    packagenameOption,
                    rootloggingpathOption,
                    trialOption,
                    transactionalOption,
                    overrideOption,
                    descriptionOption,
                    buildrevisionOption,
                    logtodatabasenamedOption,
                    scriptsrcdirOption
                };

                List<Option> generalBatchAccountOptions = new List<Option>()
                {
                    settingsfileOption,
                    batchaccountnameOption,
                    batchaccountkeyOption,
                    batchaccounturlOption,

                };

                List<Option> generalBatchNodeOptions = new List<Option>()
                {
                    batchnodecountOption,
                    batchvmsizeOption
                };
                List<Option> generalBatchExecutionOptions = new List<Option>()
                {
                    deletebatchpoolOption,
                    deletebatchjobOption,
                    rootloggingpathOption,
                    storageaccountnameOption,
                    storageaccountkeyOption,
                    eventhubconnectionOption,
                    defaultscripttimeoutOption
                };
                #endregion

                #region System.Commandline commands
                //Build root and sub commands
                RootCommand rootCommand = new RootCommand(description: "Tool to manage your SQL server database updates and releases");
                var threadedCommand = new Command("threaded", "For updating multiple databases simultaneously from the current machine")
                {
                    Handler = CommandHandler.Create<CommandLineArgs>(RunThreadedExecutionAsync)
                };
                var packageCommand = new Command("package", "Creates an SBM package from an SBX configuraiton file and scripts")
                {
                    Handler = CommandHandler.Create<CommandLineArgs>(PackageSbxFilesIntoSbmFiles)
                };
                var policyCheckCommand = new Command("policycheck", "Performs a script policy check on the specified SBM package")
                {
                    Handler = CommandHandler.Create<CommandLineArgs>(ExecutePolicyCheck)
                };
                var getHashCommand = new Command("gethash", "Calculates the SHA-1 hash fingerprint value for the SBM package(scripts + run order)")
                {
                    Handler = CommandHandler.Create<CommandLineArgs>(GetPackageHash)
                };
                var createBackoutCommand = new Command("createbackout", "Generates a backout package (reversing stored procedure and scripted object changes)")
                {
                    Handler = CommandHandler.Create<CommandLineArgs>(CreateBackout)
                };
                var getDifferenceCommand = new Command("getdifference", "Determines the difference between SQL Build run histories for two databases. Calculate and list out packages that need to be run between /Database and /GoldDatabase")
                {
                    Handler = CommandHandler.Create<CommandLineArgs>(GetDifferences)
                };
                var synchronizeCommand = new Command("synchronize", "Performs a database syncronization between between /Database and /GoldDatabase")
                {
                    Handler = CommandHandler.Create<CommandLineArgs>(SyncronizeDatabase)
                };
                var buildCommand = new Command("build", "Performs a standard, local SBM execution via command line")
                {
                    Handler = CommandHandler.Create<string[]>(StandardExecution)
                };
                var scriptExtractCommand = new Command("scriptextract", "Extract a SBM package from a source /PlatinumDacPac")
                {
                    Handler = CommandHandler.Create<CommandLineArgs>(ScriptExtraction)
                };
                var saveSettingsCommand = new Command("savesettings", "Save a settings json file for Batch arguments (see Batch documentation)")
                {
                    Handler = CommandHandler.Create<CommandLineArgs>(SaveAndEncryptSettings)
                };
                var batchCommand = new Command("batch", "Commands for setting and executing a batch run");
                var batchRunCommand = new Command("run", "For updating multiple databases simultaneously using Azure batch services")
                {
                    Handler = CommandHandler.Create<CommandLineArgs>(RunBatchExecution)
                };
                var batchPreStageCommand = new Command("prestage", "Pre-stage the Azure Batch VM nodes")
                {
                    Handler = CommandHandler.Create<CommandLineArgs>(RunBatchPreStage)
                };
                var batchCleanUpCommand = new Command("cleanup", "Azure Batch Clean Up - remove VM nodes")
                {
                    Handler = CommandHandler.Create<CommandLineArgs>(RunBatchCleanUp)
                };

                
                #endregion

                rootCommand.Add(buildCommand);
                rootCommand.Add(threadedCommand);
                rootCommand.Add(batchCommand);
                rootCommand.Add(packageCommand);
                rootCommand.Add(policyCheckCommand);
                rootCommand.Add(getHashCommand);
                rootCommand.Add(createBackoutCommand);
                rootCommand.Add(getDifferenceCommand);
                rootCommand.Add(synchronizeCommand);
                rootCommand.Add(scriptExtractCommand);

                batchCommand.Add(saveSettingsCommand);
                batchCommand.Add(batchPreStageCommand);
                batchCommand.Add(batchCleanUpCommand);
                batchCommand.Add(batchRunCommand);
                //batchCommand.Add(whatIfOption);

                //General Building options
                authOptions.ForEach(a => buildCommand.Add(a));
                buildCommand.Add(authtypeOption);
                generalbuildAndThreadOptions.ForEach(a => buildCommand.Add(a));
                buildCommand.Add(databaseOption);
                buildCommand.Add(serverOption);

                //Threaded Building options
                authOptions.ForEach(a => threadedCommand.Add(a));
                threadedCommand.Add(authtypeOption);
                generalbuildAndThreadOptions.ForEach(a => threadedCommand.Add(a));
                threadedCommand.Add(platinumdacpacOption);
                threadedCommand.Add(targetdacpacOption);
                threadedCommand.Add(forcecustomdacpacOption);
                threadedCommand.Add(platinumdbsourceOption);
                threadedCommand.Add(platinumserversourceOption);

                //Batch running
                authOptions.ForEach(a => batchRunCommand.Add(a));
                batchRunCommand.Add(overrideOption);
                generalBatchAccountOptions.ForEach(a => batchRunCommand.Add(a));
                generalBatchNodeOptions.ForEach(a => batchRunCommand.Add(a));
                generalBatchExecutionOptions.ForEach(a => batchRunCommand.Add(a));
                batchRunCommand.Add(platinumdacpacOption);
                batchRunCommand.Add(packagenameOption);
                batchRunCommand.Add(batchjobnameOption);
                batchRunCommand.Add(targetdacpacOption);
                batchRunCommand.Add(forcecustomdacpacOption);
                batchRunCommand.Add(platinumdbsourceOption);
                batchRunCommand.Add(platinumserversourceOption);
                

                //Batch pre-stage
                generalBatchAccountOptions.ForEach(a => batchPreStageCommand.Add(a));
                generalBatchNodeOptions.ForEach(a => batchPreStageCommand.Add(a));
                batchPreStageCommand.Add(pollbatchpoolstatusOption);

                //Batch node cleanup
                generalBatchAccountOptions.ForEach(a => batchCleanUpCommand.Add(a));
                batchCleanUpCommand.Add(pollbatchpoolstatusOption);

                //Batch Save settings file
                authOptions.ForEach(a => saveSettingsCommand.Add(a));
                generalBatchAccountOptions.ForEach(a => saveSettingsCommand.Add(a));
                generalBatchNodeOptions.ForEach(a => saveSettingsCommand.Add(a));
                generalBatchExecutionOptions.ForEach(a => saveSettingsCommand.Add(a));
                saveSettingsCommand.Add(timeoutretrycountOption);
                saveSettingsCommand.Add(pollbatchpoolstatusOption);


                scriptExtractCommand.Add(platinumdacpacOption);
                scriptExtractCommand.Add(outputsbmOption);

                authOptions.ForEach(a => synchronizeCommand.Add(a));
                synchronizeCommand.Add(golddatabaseOption);
                synchronizeCommand.Add(goldserverOption);
                synchronizeCommand.Add(continueonfailureOption);

                authOptions.ForEach(a => getDifferenceCommand.Add(a));
                getDifferenceCommand.Add(authtypeOption);
                getDifferenceCommand.Add(golddatabaseOption);
                getDifferenceCommand.Add(goldserverOption);
                getDifferenceCommand.Add(continueonfailureOption);

                packageCommand.Add(directoryOption);

                policyCheckCommand.Add(packagenameOption);

                getHashCommand.Add(packagenameOption);

                authOptions.ForEach(a => createBackoutCommand.Add(a));
                createBackoutCommand.Add(authtypeOption);
                createBackoutCommand.Add(packagenameOption);
                createBackoutCommand.Add(serverOption);
                createBackoutCommand.Add(databaseOption);

                CommandLineBuilder clb = new CommandLineBuilder(rootCommand);
                clb.UseMiddleware(async (context, next) =>
                {
                    if (context.ParseResult.Directives.Count() != 0 && context.ParseResult.Directives.Contains("debug"))
                    {
                    }
                    if (context.ParseResult.Directives.Count() != 0 && context.ParseResult.Directives.Contains("whatif"))
                    {

                    }
                    await next(context);
                });

                Task<int> val = rootCommand.InvokeAsync(args);
                val.Wait();

                LogManager.Flush(10000);
                System.Environment.Exit(val.Result);
            }
            else
            {

                DateTime start = DateTime.Now;
                int retVal = 0;
                try
                {
                    string joinedArgs = string.Join(",", args).ToLower();
                    string[] helpRequest = new string[] { "/?", "-?", "--?", "-h", "-help", "/help", "--help", "--h", "/h" };

                    if (args.Length == 0 || helpRequest.Any(h => joinedArgs.Contains(h)))
                    {
                        log.Info(Properties.Resources.ConsoleHelp2);
                        Environment.Exit(0);
                    }

                    var cmdLine = CommandLine.ParseCommandLineArg(args);
                    switch (cmdLine.Action)
                    {
                        case CommandLineArgs.ActionType.Threaded:
                            retVal = await RunThreadedExecutionAsync(cmdLine);
                            break;
                        case CommandLineArgs.ActionType.Package:
                            PackageSbxFilesIntoSbmFiles(cmdLine);
                            break;
                        case CommandLineArgs.ActionType.PolicyCheck:
                            ExecutePolicyCheck(cmdLine);
                            break;
                        case CommandLineArgs.ActionType.GetHash:
                            GetPackageHash(cmdLine);
                            break;
                        case CommandLineArgs.ActionType.CreateBackout:
                            CreateBackout(cmdLine);
                            break;
                        case CommandLineArgs.ActionType.GetDifference:
                            GetDifferences(cmdLine);
                            break;
                        case CommandLineArgs.ActionType.Synchronize:
                            SyncronizeDatabase(cmdLine);
                            break;
                        case CommandLineArgs.ActionType.Build:
                            StandardExecution(args);
                            break;
                        case CommandLineArgs.ActionType.ScriptExtract:
                            ScriptExtraction(cmdLine);
                            break;
                        case CommandLineArgs.ActionType.SaveSettings:
                            SaveAndEncryptSettings(cmdLine);
                            break;
                        case CommandLineArgs.ActionType.Batch:
                            retVal = await RunBatchExecution(cmdLine);
                            break;
                        case CommandLineArgs.ActionType.BatchPreStage:
                            retVal = await RunBatchPreStage(cmdLine);
                            break;
                        case CommandLineArgs.ActionType.BatchCleanUp:
                            retVal = await RunBatchCleanUp(cmdLine);
                            break;
                        default:
                            log.Error("A valid /Action arument was not found. Please check the help documentation for valid settings (/help or /?)");
                            System.Environment.Exit(8675309);
                            break;

                    }

                    LogManager.Flush(10000);
                    System.Environment.Exit(retVal);
                }
                catch (Exception exe)
                {
                    log.ErrorFormat($"Something went wrong!\r\n{exe.ToString()}");
                }
            }

        }

        private static async Task<int> RunBatchCleanUp(CommandLineArgs cmdLine)
        {
            DateTime start = DateTime.Now;
            Batch.Execution batchExe = new Batch.Execution(cmdLine);
            var retVal = batchExe.CleanUpBatchNodes();

            TimeSpan span = DateTime.Now - start;
            string msg = "Total Run time: " + span.ToString();
            log.Info(msg);

            return retVal;
        }

        private static async Task<int> RunBatchPreStage(CommandLineArgs cmdLine)
        {
            DateTime start = DateTime.Now;
            Batch.Execution batchExe = new Batch.Execution(cmdLine);
            var retVal = batchExe.PreStageBatchNodes();

            TimeSpan span = DateTime.Now - start;
            string msg = "Total Run time: " + span.ToString();
            log.Info(msg);

            return retVal;
        }

        private static async Task<int> RunBatchExecution(CommandLineArgs cmdLine)
        {
            DateTime start = DateTime.Now;
            Batch.Execution batchExe = new Batch.Execution(cmdLine);
            SetWorkingDirectoryLogger(cmdLine.RootLoggingPath);
            log.Debug("Entering Batch Execution");
            log.Info("Running...");
            int retVal =  batchExe.StartBatch();
            if (retVal == (int)ExecutionReturn.Successful)
            {
                log.Info("Completed Successfully");
            }
            else if (retVal == (int)ExecutionReturn.DacpacDatabasesInSync)
            {
                log.Info("Datbases already in sync");
                retVal = (int)ExecutionReturn.Successful;
            }
            else
            {
                log.Warn("Completed with Errors - check log");
            }

            TimeSpan span = DateTime.Now - start;
            string msg = "Total Run time: " + span.ToString();
            log.Info(msg);

            log.Debug("Exiting Batch Execution");

            return retVal;
        }

        private static void SaveAndEncryptSettings(CommandLineArgs cmdLine)
        {

            if(string.IsNullOrWhiteSpace(cmdLine.SettingsFile))
            {
                log.Error("When /Action=SaveSettings or 'sbm batch savesettings' is specified the /SettingsFile argument is also required");
            }

            cmdLine = Cryptography.EncryptSensitiveFields(cmdLine);
            var mystuff = JsonConvert.SerializeObject(cmdLine, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            try
            {
                string write = "y";
                if(File.Exists(cmdLine.SettingsFile))
                {
                    System.Console.WriteLine($"The settings file '{cmdLine.SettingsFile}' already exists. Overwrite (Y/N)?");
                    write = System.Console.ReadLine();
                }

                if (write.ToLower() == "y")
                {
                    File.WriteAllText(cmdLine.SettingsFile, mystuff);
                    log.Info($"Settings file saved to '{cmdLine.SettingsFile}'");
                }
                else
                {
                    log.Info("Settings file not saved");
                }
            }
            catch (Exception exe)
            {
                log.Error($"Unable to save settings file.\r\n{exe.ToString()}");
            }
           
        }

        private static void ScriptExtraction(CommandLineArgs cmdLine)
        {
            #region Validate flags
            if (string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumDacpac))
            {
                log.Error("/PlatinumDacpac flag is required");
                System.Environment.Exit(-1);
            }
            if (string.IsNullOrWhiteSpace(cmdLine.Database))
            {
                log.Error("/Database flag is required");
                System.Environment.Exit(-1);
            }
            if (string.IsNullOrWhiteSpace(cmdLine.Server))
            {
                log.Error("/Server flag is required");
                System.Environment.Exit(-1);
            }
            string[] errorMessages;
            int pwVal = Validation.ValidateUserNameAndPassword(ref cmdLine,out errorMessages);
            if(pwVal != 0)
            {
                log.Error(errorMessages.Aggregate((a, b) => a + "; " + b));
            }

            if (string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.UserName) || string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.Password))
            {
                log.Error("/Username and /Password flags are required");
                System.Environment.Exit(-1);
            }

            if (string.IsNullOrWhiteSpace(cmdLine.OutputSbm))
            {
                log.Error("/OutputSbm flag is required");
                System.Environment.Exit(-1);
            }

            if(File.Exists(cmdLine.OutputSbm))
            {
                log.ErrorFormat("The /OutputSbm file already exists at {0}. Please choose another name or delete the existing file.",cmdLine.OutputSbm);
                System.Environment.Exit(-1);
            }
            #endregion

            string name;
            cmdLine.RootLoggingPath = Path.GetDirectoryName(cmdLine.OutputSbm);

            var status = DacPacHelper.GetSbmFromDacPac(cmdLine, new SqlSync.SqlBuild.MultiDb.MultiDbData(), out name);
            if(status == DacpacDeltasStatus.Success)
            {
                File.Move(name, cmdLine.OutputSbm);
                log.InfoFormat("SBM package successfully created at {0}", cmdLine.OutputSbm);
            }
            else
            {
                log.ErrorFormat("Error creating SBM package: {0}", status.ToString());
            }
        }


        #region Deprecated -- Remote Exectution via Azure Cloud Service deployment
        //private static void RunRemoteExecution(string[] args, CommandLineArgs cmdLine, DateTime start)
        //{
        //    try
        //    {
        //        //string joinedArgs = string.Join(",", args).ToLower();

        //        if(cmdLine.RemoteArgs.TestConnectivity == true)
        //        {
        //            RemoteExecutionTestConnectivity(args);
        //        }
        //        else if(cmdLine.RemoteArgs.AzureRemoteStatus == true)
        //        {
        //            GetAzureRemoteStatus(args);
        //        }
        //        else if (!string.IsNullOrWhiteSpace(cmdLine.RemoteArgs.RemoteDbErrorList))
        //        {
        //            var dbsInError = RemoteAzureHealth.GetDatabaseErrorList(cmdLine.RemoteArgs.RemoteDbErrorList);
        //           if (dbsInError != null)
        //           {
        //               log.Info("\r\n" + string.Join("\r\n", dbsInError.ToArray()));
        //           }
        //        }
        //        else if (!string.IsNullOrWhiteSpace(cmdLine.RemoteArgs.RemoteErrorDetail))
        //        {
        //            var errorMessages = RemoteAzureHealth.GetErrorDetail(cmdLine.RemoteArgs.RemoteErrorDetail);
        //            log.Info("Returned error messages:");
        //            log.Info("\r\n" + errorMessages);
        //        }
        //        else if(cmdLine.DacPacArgs.ForceCustomDacPac == true)
        //        {
        //            log.Error("The /ForceCustomDacPac flag is not compatible with the /Action=Remote action");
        //            System.Environment.Exit(681);
        //        }
        //        else
        //        {


        //            log.Info("Entering Remote Server Execution - command flag option");
        //            log.Info("Running remote execution...");
        //            RemoteExecution remote = new RemoteExecution(args);

        //            int retVal = remote.Execute();
        //            if (retVal != 0)
        //                log.Warn("Completed with Errors - check log. Exiting with code: " + retVal.ToString());
        //            else
        //                log.Info("Completed Successfully. Exiting with code: " + retVal.ToString());

        //            TimeSpan span = DateTime.Now - start;
        //            string msg = "Total Run time: " + span.ToString();
        //            log.Info(msg);

        //            log.Info("Exiting Remote Execution");
        //            System.Environment.Exit(retVal);

        //        }
        //    }
        //    catch (Exception exe)
        //    {
        //        log.Warn("Exiting Remote Execution with 603: " + exe.ToString());

        //        log.Error("Execution error - check logs");
        //        System.Environment.Exit(603);
        //    }
        //}

        //#region .: Remote Health Check :.
        //private static void RemoteExecutionTestConnectivity(string[] args)
        //{
        //    log.Info("Entering Remote Server Connectivity Testing: agent and database connectivity");
        //    log.Info("Entering Remote Server Connectivity Testing...");
        //    RemoteExecution remote = new RemoteExecution(args);

        //    int retVal = remote.TestConnectivity();
        //    if (retVal != 0)
        //        log.Error(
        //            string.Format("Test Connectivity Failed for {0} server/databases. - check log.",
        //                          retVal.ToString()));
        //    else
        //        log.Info("Test Connectivity Completed Successfully. Exiting with code: " + retVal.ToString());
        //}
        //private static void GetAzureRemoteStatus(string[] args)
        //{
        //    try
        //    {
        //        string format = "{0}{1}{2}{3}";
        //        log.Info("Getting list of Azure instances...");
        //        BuildServiceManager manager = new BuildServiceManager();
        //        List<ServerConfigData> serverData = manager.GetListOfAzureInstancePublicUrls();
        //        var remote = serverData.Select(s => s.ServerName).ToList();
        //        if (remote.Count() > 0)
        //        {
        //            log.InfoFormat("{0} instances available at {1}", remote.Count(), Regex.Replace(serverData[0].ActiveServiceEndpoint, @":\d{5}", ""));
        //        }
        //        List<ServerConfigData> remoteServer = null;
        //        string[] errorMessages;
        //        log.Info("Retrieving status of each instance...");



        //        int statReturn = RemoteExecution.ValidateRemoteServerAvailability(remote, Protocol.AzureHttp, out remoteServer, out errorMessages);

        //        int serverPad = remoteServer.Max(s => s.ServerName.Length) + 2;
        //        int statusPad = remoteServer.Max(s => s.ServiceReadiness.ToString().Length) + 2;
        //        int exePad = remoteServer.Max(s => s.ExecutionReturn.ToString().Length) + 2;
        //        if (exePad < "Last Status".Length + 2)
        //            exePad = "Last Status".Length + 2;
        //        int versionPad = remoteServer.Max(s => s.ServiceVersion.ToString().Length) + 2;

        //        log.InfoFormat(format, "Service".PadRight(serverPad, ' '), "Status".PadRight(statusPad, ' '), "Last Status".PadRight(exePad, ' '), "Version".PadRight(versionPad, ' '));
        //        log.InfoFormat(format, "-".PadRight(serverPad-2, '-'), "-".PadRight(statusPad-2, '-'), "-".PadRight(exePad-2, '-'), "--".PadRight(versionPad-2, '-'));
        //        remoteServer.ForEach(s =>
        //            log.InfoFormat(format, s.ServerName.PadRight(serverPad, ' '), s.ServiceReadiness.ToString().PadRight(statusPad, ' '), s.ExecutionReturn.ToString().PadRight(exePad, ' '), s.ServiceVersion.PadRight(versionPad, ' ')));

        //        if(errorMessages.Length > 0)
        //        {
        //            errorMessages.ToList().ForEach(e => log.Error(e));
        //        }

        //    }
        //    catch (Exception exe)
        //    {
        //        log.Error("Unable to get list of Azure instances", exe);
        //    }
        //}

        //#endregion

        //private static void RemoteExecutionWithRespFile(string[] args, DateTime start)
        //{
        //    log.Info("Entering Remote Server Execution - single config file option.");
        //    try
        //    {
        //        log.Info("Starting Remote Execution...");

        //        RemoteExecution remote = new RemoteExecution(args[0]);
        //        int retVal = remote.Execute();
        //        if (retVal != 0)
        //            log.Warn("Completed with Errors - check logs");
        //        else
        //            log.Info("Completed Successfully");


        //        TimeSpan span = DateTime.Now - start;
        //        string msg = "Total Run time: " + span.ToString();
        //        log.Info(msg);

        //        log.Debug("Exiting Remote Execution with " + retVal.ToString());

        //        System.Environment.Exit(retVal);
        //    }
        //    catch (Exception exe)
        //    {
        //        log.Debug("Exiting Remote Execution with 602: " + exe.ToString());

        //        log.Error("Execution error - check logs");
        //        System.Environment.Exit(602);
        //    }
        //}
        #endregion

        private static async Task<int> RunThreadedExecutionAsync(CommandLineArgs cmdLine)
        {
            DateTime start = DateTime.Now;
            SetWorkingDirectoryLogger(cmdLine.RootLoggingPath);
            log.Debug("Entering Threaded Execution");
            log.Info("Running...");
            ThreadedExecution runner = new ThreadedExecution(cmdLine);
            int retVal = runner.Execute();
            if (retVal == (int)ExecutionReturn.Successful)
            {
                log.Info("Completed Successfully");
            }
            else if (retVal == (int)ExecutionReturn.DacpacDatabasesInSync)
            {
                log.Info("Datbases already in sync");
                retVal = (int)ExecutionReturn.Successful;
            }
            else
            {
                log.Warn("Completed with Errors - check log");
            }     

            TimeSpan span = DateTime.Now - start;
            string msg = "Total Run time: " + span.ToString();
            log.Info(msg);

            if(!String.IsNullOrEmpty(cmdLine.BatchArgs.OutputContainerSasUrl))
            {
                log.Info("Writing log files to storage...");
                bool success = await WriteLogsToBlobContainer(cmdLine.BatchArgs.OutputContainerSasUrl, cmdLine.RootLoggingPath);
            }
          
            log.Debug("Exiting Threaded Execution");
  
            return retVal;

        }

        private static async Task<bool> WriteLogsToBlobContainer(string outputContainerSasUrl, string rootLoggingPath)
        {
            try
            {
                var writeTasks = new List<Task>();
                
                var renameLogFiles = new string[] { "execution.log" };
                CloudBlobContainer container = new CloudBlobContainer(new Uri(outputContainerSasUrl));
                var fileList = Directory.GetFiles(rootLoggingPath, "*.*", SearchOption.AllDirectories);
                string machine = Environment.MachineName;
                fileList.ToList().ForEach(f =>
                {
                    try
                    {
                        var tmp = f.Replace(rootLoggingPath + @"\", "");

                        if (Program.AppendLogFiles.Any(a => f.ToLower().IndexOf(a) > -1))
                        {

                            tmp = machine + "-" + tmp;
                            log.InfoFormat($"Saving File '{f}' as '{tmp}'");
                            var rename = container.GetBlockBlobReference(tmp);

                            writeTasks.Add(
                                rename.UploadFromFileAsync(f,
                                AccessCondition.GenerateIfNotExistsCondition(),
                                new BlobRequestOptions() { RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 20) }, null));

                        }
                        else if (renameLogFiles.Any(a => f.ToLower().IndexOf(a) > -1))
                        {
                            tmp = machine + "-" + tmp;
                            log.InfoFormat($"Saving File '{f}' as '{tmp}'");
                            var rename = container.GetBlockBlobReference(tmp);

                            writeTasks.Add(
                                rename.UploadFromFileAsync(f,
                                AccessCondition.GenerateIfNotExistsCondition(),
                                new BlobRequestOptions() { RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 20) }, null));
                        }
                        else
                        {
                            log.InfoFormat($"Saving File '{f}' as '{tmp}'");
                            var b = container.GetBlockBlobReference(tmp);
                            writeTasks.Add(
                                b.UploadFromFileAsync(f,
                                AccessCondition.GenerateIfNotExistsCondition(),
                                new BlobRequestOptions() { RetryPolicy = new ExponentialRetry(TimeSpan.FromSeconds(1), 20) }, null));
                        }
                    }
                    catch (Exception e)
                    {
                        log.ErrorFormat($"Unable to upload log file '{f}' to blob storage: {e.ToString()}");
                    }
                });

                await Task.WhenAll(writeTasks);
                return true;
            }
            catch (Exception exe)
            {
                log.Error("Unable to upload log files to blob storage", exe);
                return false;
            }
        }

        private static void SetWorkingDirectoryLogger(string rootLoggingPath)
        {
   
            try
            {

                if (!string.IsNullOrEmpty(rootLoggingPath))
                {
                    //if (!cmdLine.RootLoggingPath.EndsWith("\\"))
                    //{
                    //    cmdLine.RootLoggingPath = cmdLine.RootLoggingPath + "\\";
                    //}
                    if (!Directory.Exists(rootLoggingPath))
                    {
                        Directory.CreateDirectory(rootLoggingPath);
                    }

                    var appender = LogManager.GetRepository(Assembly.GetEntryAssembly()).GetAppenders().Where(a => a.Name == "ThreadedExecutionWorkingAppender").FirstOrDefault();
                    if (appender != null)
                    {
                        var thr = appender as log4net.Appender.FileAppender;
                        thr.File = Path.Combine(rootLoggingPath, Path.GetFileName(thr.File));
                        thr.ActivateOptions();
                    }
                }
            }catch(Exception exe)
            {
                log.Error(string.Format("Unable to set local root logging path to {0}", rootLoggingPath), exe);
            }

            
        }

        private static void StandardExecution(string[] args )
        {
            DateTime start = DateTime.Now;
            log.Debug("Entering Standard Execution");

            //Get the path of the Sql Build Manager executable - need to be co-resident
            string sbmExe = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +
                            @"\Sql Build Manager.exe";

            //Put any arguments that have spaces into quotes
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i].IndexOf(" ") > -1)
                    args[i] = "\"" + args[i] + "\"";
            }
            //Rejoin the args to pass over to Sql Build Manager
            string arg = String.Join(" ", args);
            ProcessHelper prcHelper = new ProcessHelper();
            int exitCode = prcHelper.ExecuteProcess(sbmExe, arg);

            //Send the console output to the console window
            if (prcHelper.Output.Length > 0)
                System.Console.Out.WriteLine(prcHelper.Output);

            //Send the error output to the error stream
            if (prcHelper.Error.Length > 0)
                log.Error(prcHelper.Output);

            TimeSpan span = DateTime.Now - start;
            string msg = "Total Run time: " + span.ToString();
            log.Info(msg);

            log.Debug("Exiting Standard Execution");
            log4net.LogManager.Shutdown();
            System.Environment.Exit(exitCode);
        }

        private static void PackageSbxFilesIntoSbmFiles(CommandLineArgs cmdLine)
        {
            if(string.IsNullOrWhiteSpace(cmdLine.Directory))
            {
                log.Error("The /Directory argument is required for /Action=Package");
                System.Environment.Exit(9835);
            }
            string directory = cmdLine.Directory;
            string message;
            List<string> sbmFiles = SqlBuildFileHelper.PackageSbxFilesIntoSbmFiles(directory, out message);
            if (sbmFiles.Count > 0)
            {
                foreach (string sbm in sbmFiles)
                    log.Info(sbm);

                System.Environment.Exit(0);
            }
            else if (message.Length > 0)
            {
                log.Warn (message);
                System.Environment.Exit(604);
            }
            else
            {
                System.Environment.Exit(0);
            }
        }

   


        #region .: Helper Processes :.
        private static void SyncronizeDatabase(CommandLineArgs cmdLine)
        {
            bool success = Synchronize.SyncDatabases(cmdLine);
            if (success)
                System.Environment.Exit(0);
            else
                System.Environment.Exit(954);
        }

        private static void GetDifferences(CommandLineArgs cmdLine)
        {
            string history = Synchronize.GetDatabaseRunHistoryTextDifference(cmdLine);
            log.Info(history);
            System.Environment.Exit(0);
        }

        private static void CreateBackout(CommandLineArgs cmdLine)
        {
            string packageName = BackoutCommandLine.CreateBackoutPackage(cmdLine);
            if (!String.IsNullOrEmpty(packageName))
            {
                log.Info(packageName);
                System.Environment.Exit(0);
            }
            else
            {
                System.Environment.Exit(856);
            }
        }

        private static void GetPackageHash(CommandLineArgs cmdLine)
        {
            if(string.IsNullOrWhiteSpace(cmdLine.BuildFileName))
            {
                log.Error("No /PackageName was specified. This is required for /Action=GetHash");
                System.Environment.Exit(626);

            }
            string packageName = cmdLine.BuildFileName;
            string hash = SqlBuildFileHelper.CalculateSha1HashFromPackage(packageName);
            if (!String.IsNullOrEmpty(hash))
            {
                log.Info(hash);
                System.Environment.Exit(0);
            }
            else
            {
                System.Environment.Exit(621);
            }
        }

        private static void ExecutePolicyCheck(CommandLineArgs cmdLine)
        {
            if (string.IsNullOrWhiteSpace(cmdLine.BuildFileName))
            {
                log.Error("No /PackageName was specified. This is required for /Action=PolicyCheck");
                System.Environment.Exit(34536);

            }
            string packageName = cmdLine.BuildFileName;
            PolicyHelper helper = new PolicyHelper();
            bool passed;
            List<string> policyMessages = helper.CommandLinePolicyCheck(packageName, out passed);
            if (policyMessages.Count > 0)
            {
                log.Info("Script Policy Messages:");
                foreach (var policyMessage in policyMessages)
                {
                    log.Info(policyMessage);
                }
            }

            if (passed)
            {
                System.Environment.Exit(0);
            }
            else
            {
                System.Environment.Exit(739);
            }
        }
        #endregion
    }


}
