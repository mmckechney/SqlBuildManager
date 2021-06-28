using Microsoft.Extensions.Logging;
using System.CommandLine;
using System.CommandLine.Binding;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;

namespace SqlBuildManager.Console.CommandLine
{
    public class CommandLineBuilder

    {
        public static RootCommand SetUp()
        {
            var settingsfileOption = new Option<FileInfo>(new string[] { "--settingsfile" }, "Saved settings file to load parameters from") { Name = "FileInfoSettingsFile" }.ExistingOnly();
            var settingsfileNewOption = new Option<FileInfo>(new string[] { "--settingsfile" }, "Saved settings file to load parameters from") { Name = "FileInfoSettingsFile" };
            var settingsfileKeyOption = new Option<string>(new string[] { "--settingsfilekey" }, "Key for the encryption of sensitive informtation in the settings file (must be at least 16 characters). It can be either the key string or a file path to a key file. The key may also provided by setting a 'sbm-settingsfilekey' Environment variable. If not provided a machine value will be used.");
            var overrideOption = new Option<string>(new string[] { "--override" }, "File containing the target database settings (usually a formatted .cfg file)");
            var serverOption = new Option<string>(new string[] { "-s", "--server" }, "1) Name of a server for single database run or 2) source server for scripting or runtime configuration");
            var databaseOption = new Option<string>(new string[] { "-d", "--database" }, "1) Name of a single database to run against or 2) source database for scripting or runtime configuration");
            var rootloggingpathOption = new Option<string>(new string[] { "--rootloggingpath" }, "Directory to save execution logs (for threaded and remote executions)");
            var trialOption = new Option<bool>(new string[] { "--trial" }, () => false, "Whether or not to run in trial mode (default is false)");
            var scriptsrcdirOption = new Option<string>(new string[] { "--scriptsrcdir" }, " [Not recommended] Alternative ability to run against a directory of scripts (vs .sbm or .sbx file)");
            var usernameOption = new Option<string>(new string[] { "-u", "--username" }, "The username to authenticate against the database if not using integrate auth");
            var passwordOption = new Option<string>(new string[] { "-p", "--password" }, "The password to authenticate against the database if not using integrate auth");
            var logtodatabasenamedOption = new Option<string>(new string[] { "--logtodatabasename" }, "[Not recommended] Specifies that the SqlBuild_logging logs should go to an alternate database (vs. target).");
            var descriptionOption = new Option<string>(new string[] { "--description" }, "Description of build (logged with build)");
            var packagenameOption = new Option<string>(new string[] { "--packagename", "--buildfilename" }, "Name of the .sbm or .sbx file to execute") { Name = "BuildFileName", IsRequired = true };
            var directoryOption = new Option<string>(new string[] { "--directory" }, "Directory containing 1 or more SBX files to package into SBM zip files") { IsRequired = true };
            var transactionalOption = new Option<bool>(new string[] { "--transactional" }, () => true, "Whether or not to run with a wrapping transaction (default is true)");
            var timeoutretrycountOption = new Option<int>(new string[] { "--timeoutretrycount" }, "How many retries to attempt if a timeout exception occurs");
            var golddatabaseOption = new Option<string>(new string[] {"-gd", "--golddatabase" }, "The \"gold copy\" database that will serve as the model for what the target database should look like") { IsRequired = true };
            var goldserverOption = new Option<string>(new string[] { "-gs", "--goldserver" }, "The server that the \"gold copy\" database can be found") { IsRequired = true };
            var continueonfailureOption = new Option<bool>(new string[] { "--continueonfailure" }, "Whether or not to continue on the failure of a package (default is false)");
            var platinumdacpacOption = new Option<string>(new string[] { "-pd","--platinumdacpac" }, "Name of the dacpac containing the platinum schema");
            var targetdacpacOption = new Option<string>(new string[] { "-td", "--targetdacpac" }, "Name of the dacpac containing the schema of the database to be updated");
            var forcecustomdacpacOption = new Option<bool>(new string[] { "--forcecustomdacpac" }, "USE WITH CAUTION! This will force the dacpac extraction and creation of custom scripts for EVERY target database! Your execution will take much longer.");
            var platinumdbsourceOption = new Option<string>(new string[] { "--platinumdbsource" }, "Instead of a formally built Platinum Dacpac, target this database as having the desired state schema");
            var platinumserversourceOption = new Option<string>(new string[] { "--platinumserversource" }, "Instead of a formally built Platinum Dacpac, target a database on this server as having the desired state schema");
            var buildrevisionOption = new Option<string>(new string[] { "--buildrevision" }, "If provided, the build will include an update to a \"Versions\" table and this will be the value used to add to a \"VersionNumber\" column (varchar(max))");
            var outputsbmOption = new Option<string>(new string[] { "-o", "--outputsbm" }, "Name (and path) of the SBM package or SBX file to create");
            var deletebatchpoolOption = new Option<bool>(new string[] { "--deletebatchpool" }, () => false, "Whether or not to delete the batch pool servers after an execution");
            var deletebatchjobOption = new Option<bool>(new string[] { "--deletebatchjob" }, () => false, "Whether or not to delete the batch job after an execution");
            var batchnodecountOption = new Option<int>(new string[] { "--nodecount", "--batchnodecount" }, "Number of nodes to provision to run the batch job");
            var batchjobnameOption = new Option<string>(new string[] { "--jobname", "--batchjobname" }, "User friendly name for the job. This will also be the container name for the stored logs. Any disallowed URL characters will be removed");
            var batchaccountnameOption = new Option<string>(new string[] { "--batchaccountname" }, "String name of the Azure Batch account");
            var batchaccountkeyOption = new Option<string>(new string[] { "--batchaccountkey" }, "Account Key for the Azure Batch account");
            var batchaccounturlOption = new Option<string>(new string[] { "--batchaccounturl" }, "URL for the Azure Batch account");
            var storageaccountnameOption = new Option<string>(new string[] { "--storageaccountname" }, "Name of storage account associated with the Azure Batch account");
            var storageaccountkeyOption = new Option<string>(new string[] { "--storageaccountkey" }, "Account Key for the storage account");
            var batchvmsizeOption = new Option<string>(new string[] { "--vmsize", "--batchvmsize" }, "Size key for VM size required (see https://docs.microsoft.com/en-us/azure/virtual-machines/windows/sizes-general) ");
            var batchpoolnameOption = new Option<string>(new string[] { "--poolname", "--batchpoolname" }, "Override for the default pool name of \"SqlBuildManagerPool\"");
            var batchpoolOsOption = new Option<OsType>(new string[] { "--os", "--batchpoolos" }, "Operating system for the Azure Batch nodes. Windows is default");
            var batchApplicationOption = new Option<string>(new string[] { "--apppackage", "--applicationpackage" }, "The Azure Batch application package name. (Default is 'SqlBuildManagerWindows' for Windows and 'SqlBuildManagerLinux' for Linux");
            var eventhubconnectionOption = new Option<string>(new string[] { "--eventhubconnection" }, "Event Hub connection string for Event Hub logging of batch execution");
            var serviceBusconnectionOption = new Option<string>(new string[] { "--servicebustopicconnection" }, "Service Bus connection string for Service Bus topic distribution of batch execution");
            var pollbatchpoolstatusOption = new Option<bool>(new string[] { "--pollbatchpoolstatus" }, "Whether or not you want to get updated status (true) or fire and forget (false)");
            var defaultscripttimeoutOption = new Option<int>(new string[] { "--defaultscripttimeout" }, "Override the default script timeouts set when creating a DACPAC");
            var authtypeOption = new Option<SqlSync.Connection.AuthenticationType>(new string[] { "--authtype" }, "SQL Authentication type to use.") { Name = "AuthenticationType" };
            var silentOption = new Option<bool>(new string[] { "--silent" }, () => false, "Suppresses overwrite prompt if file already exists");
            var outputcontainersasurlOption = new Option<string>(new string[] { "--outputcontainersasurl" }, "[Internal only] Runtime storage SAS url (auto-generated from `sbm batch run` command");
            var dacpacOutputOption = new Option<string>(new string[] { "--dacpacname" }, "Name of the dacpac that you want to create") { IsRequired = true };
            var cleartextOption = new Option<bool>(new string[] { "--cleartext" }, () => false, "Flag to save settings file in clear text (vs. encrypted)");
            var queryFileOption = new Option<FileInfo>(new string[] { "--queryfile" }, "File containing the SELECT query to run across the databases").ExistingOnly();
            var outputFileOption = new Option<FileInfo>(new string[] { "--outputfile" }, "Results output file to create");
            var threadedConcurrencyOption = new Option<int>(new string[] { "--concurrency" }, "Maximum concurrency for threaded executions");
            var threadedConcurrencyTypeOption = new Option<ConcurrencyType>(new string[] { "--concurrencytype" }, "Type of concurrency, used in conjunction with --concurrency ");
            var logLevelOption = new Option<LogLevel>(new string[] { "--loglevel" }, "Logging level for console and log file");
            var scriptListOption = new Option<FileInfo[]>(new string[] {"-s", "--scripts"}, "List of script files to create SBM package from - separate names with spaces (will be added in order provided)") { IsRequired = true }.ExistingOnly();
            var withHashOption = new Option<bool>(new string[] { "-w", "--withhash" }, () => true, "Also include the SHA1 hash of the script files in the package");
            var packagesOption = new Option<FileInfo[]>(new string[] {"-p","--packages"}, "One or more SBM packages to get contents for") { IsRequired = true }.ExistingOnly();

            var platinumdacpacSourceOption = new Option<FileInfo>(new string[] { "-pd", "--platinumdacpac" }, "Name of the dacpac containing the platinum schema") { IsRequired = true }.ExistingOnly();
            var targetdacpacSourceOption = new Option<FileInfo>(new string[] { "-td", "--targetdacpac" }, "Name of the dacpac containing the schema of the database to be updated") { IsRequired = true }.ExistingOnly();
            //Create DACPAC from target database
            var dacpacCommand = new Command("dacpac", "Creates a DACPAC file from the target database")
            {
                passwordOption,
                usernameOption,
                authtypeOption,
                databaseOption.Copy(true),
                serverOption.Copy(true),
                dacpacOutputOption
            };
            dacpacCommand.Handler = CommandHandler.Create<CommandLineArgs>(Program.CreateDacpac);


            //General Local building options
            var buildCommand = new Command("build", "Performs a standard, local SBM execution via command line")
            {
                passwordOption,
                usernameOption,
                authtypeOption,
                packagenameOption.Copy(true),
                serverOption.Copy(true),
                databaseOption,
                rootloggingpathOption,
                trialOption,
                transactionalOption,
                overrideOption,
                descriptionOption,
                buildrevisionOption,
                logtodatabasenamedOption,
                scriptsrcdirOption,
                timeoutretrycountOption
            };
            buildCommand.Handler = CommandHandler.Create<CommandLineArgs>(Program.RunLocalBuildAsync);

            /****************************************
             * Threaded 
             ***************************************/
            //Threaded run command
            var threadedRunCommand = new Command("run", "For updating multiple databases simultaneously from the current machine")
            {
                passwordOption,
                usernameOption,
                authtypeOption,
                packagenameOption,
                rootloggingpathOption,
                trialOption,
                transactionalOption,
                overrideOption,
                descriptionOption,
                buildrevisionOption,
                logtodatabasenamedOption,
                scriptsrcdirOption,
                platinumdacpacOption,
                targetdacpacOption,
                forcecustomdacpacOption,
                platinumdbsourceOption,
                platinumserversourceOption,
                timeoutretrycountOption,
                defaultscripttimeoutOption,
                threadedConcurrencyOption,
                threadedConcurrencyTypeOption
            };
            threadedRunCommand.Handler = CommandHandler.Create<CommandLineArgs>(Program.RunThreadedExecution);

            //Threaded query options
            var threadedQueryCommand = new Command("query", "Run a SELECT query across multiple databases")
            {
                passwordOption,
                usernameOption,
                authtypeOption,
                queryFileOption.Copy(true),
                overrideOption.Copy(true),
                outputFileOption.Copy(true),
                defaultscripttimeoutOption,
                silentOption,
                threadedConcurrencyOption,
                threadedConcurrencyTypeOption
            };
            threadedQueryCommand.Handler = CommandHandler.Create<CommandLineArgs>(Program.QueryDatabases);

            //Threaded base commands
            var threadedCommand = new Command("threaded", "For updating multiple or querying databases simultaneously from the current machine");
            threadedCommand.Add(threadedQueryCommand);
            threadedCommand.Add(threadedRunCommand);


            /****************************************
             * Batch 
             ***************************************/

            //Batch running
            var batchRunCommand = new Command("run", "For updating multiple databases simultaneously using Azure batch services")
            {
                passwordOption,
                usernameOption,
                overrideOption.Copy(true),
                settingsfileKeyOption,
                settingsfileOption,
                //Batch account options
                batchaccountnameOption,
                batchaccountkeyOption,
                batchaccounturlOption,
                //Batch node options
                batchnodecountOption,
                batchvmsizeOption,
                batchpoolOsOption,
                batchpoolnameOption,
                batchApplicationOption,
                //Batch execution options
                deletebatchpoolOption,
                deletebatchjobOption,
                rootloggingpathOption,
                storageaccountnameOption,
                storageaccountkeyOption,
                eventhubconnectionOption,
                defaultscripttimeoutOption,
                threadedConcurrencyOption,
                threadedConcurrencyTypeOption,
                //Service Bus queue options
                serviceBusconnectionOption,
                //Other run command options
                platinumdacpacOption,
                packagenameOption.Copy(false),
                batchjobnameOption,
                targetdacpacOption,
                forcecustomdacpacOption,
                platinumdbsourceOption,
                platinumserversourceOption
            };
            batchRunCommand.Handler = CommandHandler.Create<CommandLineArgs>(Program.RunBatchExecution);


            //Batch threading run -- used to run on Batch node
            var batchRunThreadedCommand = new Command("runthreaded", "[Internal use only] - this commmand is used to send threaded commands to Azure Batch Nodes")
            {
                passwordOption,
                usernameOption,
                authtypeOption,
                overrideOption,
                settingsfileKeyOption,
                settingsfileOption,
                //Batch account options
                batchaccountnameOption,
                batchaccountkeyOption,
                batchaccounturlOption,
                //Batch node options
                batchnodecountOption,
                batchvmsizeOption,
                batchpoolOsOption,
                batchpoolnameOption,
                batchApplicationOption,
                //Batch execution options
                deletebatchpoolOption,
                deletebatchjobOption,
                rootloggingpathOption,
                storageaccountnameOption,
                storageaccountkeyOption,
                eventhubconnectionOption,
                defaultscripttimeoutOption,
                threadedConcurrencyOption,
                threadedConcurrencyTypeOption,
                //Service Bus queue options
                serviceBusconnectionOption,
                //Batch to threaded node options
                platinumdacpacOption,
                packagenameOption.Copy(false),
                batchjobnameOption,
                targetdacpacOption,
                forcecustomdacpacOption,
                platinumdbsourceOption,
                platinumserversourceOption,
                outputcontainersasurlOption,
                transactionalOption,
                timeoutretrycountOption

            };
            batchRunThreadedCommand.Handler = CommandHandler.Create<CommandLineArgs>(Program.RunThreadedExecution);
            batchRunThreadedCommand.IsHidden = true;


            //Batch pre-stage
            var batchPreStageCommand = new Command("prestage", "Pre-stage the Azure Batch VM nodes")
            {
                settingsfileKeyOption,
                settingsfileOption,
                //Batch account options
                batchaccountnameOption,
                batchaccountkeyOption,
                batchaccounturlOption,
                //Batch node options
                batchnodecountOption,
                batchvmsizeOption,
                batchpoolOsOption,
                batchpoolnameOption,
                batchApplicationOption,
                pollbatchpoolstatusOption
            };
            batchPreStageCommand.Handler = CommandHandler.Create<CommandLineArgs>(Program.RunBatchPreStage);


            //Batch node cleanup
            var batchCleanUpCommand = new Command("cleanup", "Azure Batch Clean Up - remove VM nodes")
            {
                settingsfileKeyOption,
                settingsfileOption,
                batchaccountnameOption,
                batchaccountkeyOption,
                batchaccounturlOption,
                pollbatchpoolstatusOption
            };
            batchCleanUpCommand.Handler = CommandHandler.Create<CommandLineArgs>(Program.RunBatchCleanUp);

            //Batch delete jobs
            var batchDeleteJobsCommand = new Command("deletejobs", "Delete Jobs from the Azure batch account")
            {
                settingsfileKeyOption,
                settingsfileOption,
                batchaccountnameOption,
                batchaccountkeyOption,
                batchaccounturlOption
            };
            batchDeleteJobsCommand.IsHidden = true;
            batchDeleteJobsCommand.Handler = CommandHandler.Create<CommandLineArgs>(Program.RunBatchJobDelete);


            //Batch Save settings file
            var saveSettingsCommand = new Command("savesettings", "Save a settings json file for Batch arguments (see Batch documentation)")
            {
                passwordOption,
                usernameOption,
                settingsfileKeyOption,
                settingsfileNewOption,
                //Batch account options
                batchaccountnameOption,
                batchaccountkeyOption,
                batchaccounturlOption,
                //Batch node options
                batchnodecountOption,
                batchvmsizeOption,
                batchpoolOsOption,
                batchpoolnameOption,
                batchApplicationOption,
                //Batch execution options
                deletebatchpoolOption,
                deletebatchjobOption,
                rootloggingpathOption,
                storageaccountnameOption,
                storageaccountkeyOption,
                eventhubconnectionOption,
                defaultscripttimeoutOption,
                threadedConcurrencyOption,
                threadedConcurrencyTypeOption,
                //Service Bus queue options
                serviceBusconnectionOption,
                //Additional settings
                timeoutretrycountOption,
                pollbatchpoolstatusOption,
                silentOption,
                cleartextOption
            };
            saveSettingsCommand.Handler = CommandHandler.Create<CommandLineArgs, bool>(Program.SaveAndEncryptSettings);

            var batchEnqueueTargetsCommand = new Command("enqueue", "Sends database override targets to Service Bus Topic")
            {
                batchjobnameOption.Copy(true),
                threadedConcurrencyTypeOption.Copy(true),
                settingsfileOption,
                settingsfileKeyOption,
                serviceBusconnectionOption,
                overrideOption.Copy(true)
            };
            batchEnqueueTargetsCommand.Handler = CommandHandler.Create<CommandLineArgs>(Program.QueueOverrideTargets);

            var batchDequeueTargetsCommand = new Command("dequeue", "Careful! Removes all messages from the Service Bue Topics and Deadletters without processing them")
            {
                settingsfileOption,
                settingsfileKeyOption,
                serviceBusconnectionOption
            };
            batchDequeueTargetsCommand.Handler = CommandHandler.Create<CommandLineArgs>(Program.DeQueueOverrideTargets);

            
            //Batch query 
            var batchQueryCommand = new Command("query", "Run a SELECT query across multiple databases using Azure Batch")
            {
                passwordOption,
                usernameOption,
                authtypeOption,
                overrideOption.Copy(true),
                queryFileOption.Copy(true),
                outputFileOption.Copy(true),
                silentOption,
                settingsfileKeyOption,
                settingsfileOption,
                //Batch account options
                batchaccountnameOption,
                batchaccountkeyOption,
                batchaccounturlOption,
                //Batch node options
                batchnodecountOption,
                batchvmsizeOption,
                batchpoolOsOption,
                batchpoolnameOption,
                batchApplicationOption,
                //Batch execution options
                deletebatchpoolOption,
                deletebatchjobOption,
                rootloggingpathOption,
                storageaccountnameOption,
                storageaccountkeyOption,
                eventhubconnectionOption,
                defaultscripttimeoutOption,
                threadedConcurrencyOption,
                threadedConcurrencyTypeOption,
                //Service Bus queue options
                serviceBusconnectionOption,

            };
            batchQueryCommand.Handler = CommandHandler.Create<CommandLineArgs>(Program.RunBatchQuery);


            // Batch query -- threaded
            var batchQueryThreadedCommand = new Command("querythreaded", "[Internal use only] - this commmand is used to send query commands to Azure Batch Nodes")
            {
                passwordOption,
                usernameOption,
                authtypeOption,
                overrideOption.Copy(true),
                queryFileOption.Copy(true),
                outputFileOption.Copy(true),
                settingsfileKeyOption,
                settingsfileOption,
                //Batch account options
                batchaccountnameOption,
                batchaccountkeyOption,
                batchaccounturlOption,
                //Batch node options
                batchnodecountOption,
                batchvmsizeOption,
                batchpoolOsOption,
                batchpoolnameOption,
                batchApplicationOption,
                //Batch execution options
                deletebatchpoolOption,
                deletebatchjobOption,
                rootloggingpathOption,
                storageaccountnameOption,
                storageaccountkeyOption,
                eventhubconnectionOption,
                defaultscripttimeoutOption,
                threadedConcurrencyOption,
                threadedConcurrencyTypeOption,
                //Batch to Threaded node options
                outputcontainersasurlOption,
                transactionalOption,
                timeoutretrycountOption,
                silentOption

            };
            batchQueryThreadedCommand.Handler = CommandHandler.Create<CommandLineArgs>(Program.QueryDatabases);
            batchQueryThreadedCommand.IsHidden = true;

            //Azure Batch base command
            var batchCommand = new Command("batch", "Commands for setting and executing a batch run or batch query");
            batchCommand.Add(saveSettingsCommand);
            batchCommand.Add(batchPreStageCommand);
            batchCommand.Add(batchEnqueueTargetsCommand);
            batchCommand.Add(batchRunCommand);
            batchCommand.Add(batchQueryCommand);
            batchCommand.Add(batchCleanUpCommand);
            batchCommand.Add(batchDequeueTargetsCommand);
            batchCommand.Add(batchRunThreadedCommand);
            batchCommand.Add(batchQueryThreadedCommand);
            batchCommand.Add(batchDeleteJobsCommand);
            


            /****************************************
             * Utility commands 
             ***************************************/

            //Sync two databases
            var synchronizeCommand = new Command("synchronize", "Performs a database synchronization between between --database and --golddatabase. Can only be used for Windows Auth database targets")
            {
                passwordOption,
                usernameOption,
                golddatabaseOption,
                goldserverOption,
                databaseOption.Copy(true),
                serverOption.Copy(true),
                continueonfailureOption
            };
            synchronizeCommand.Handler = CommandHandler.Create<CommandLineArgs>(Program.SyncronizeDatabase);

            //Get SBM run differences between two databases
            var getDifferenceCommand = new Command("getdifference", "Determines the difference between SQL Build run histories for two databases. Calculate and list out packages that need to be run between --database and --golddatabase. Only supports Windows Auth")
            {
                golddatabaseOption,
                goldserverOption,
                databaseOption.Copy(true),
                serverOption.Copy(true)
            };
            getDifferenceCommand.Handler = CommandHandler.Create<CommandLineArgs>(Program.GetDifferences);

            //Create and SBM from and SBX and script files
            var packageCommand = new Command("package", "Creates an SBM package from an SBX configuration file and scripts")
            {
                directoryOption
            };
            packageCommand.Handler = CommandHandler.Create<CommandLineArgs>(Program.PackageSbxFilesIntoSbmFiles);

           

            //Create from diff
            var createFromDiffCommand = new Command("fromdiff", "Creates an SBM package from a calculated diff between two databases")
            {
                outputsbmOption.Copy(true),
                golddatabaseOption.Copy(true),
                goldserverOption.Copy(true),
                databaseOption.Copy(true),
                serverOption.Copy(true),
                authtypeOption

            };
            createFromDiffCommand.Handler = CommandHandler.Create<CommandLineArgs>(Program.CreatePackageFromDiff);

            //Create from diff
            var createFromScriptsCommand = new Command("fromscripts", "Creates an SBM package or SBX project file from a list of scripts (type is determined by file extension- .sbm or .sbx)")
            {
                outputsbmOption.Copy(true),
                scriptListOption

            };
            createFromScriptsCommand.Handler = CommandHandler.Create<CommandLineArgs>(Program.CreatePackageFromScripts);

            //Create from diff
            var createFromDacpacsCommand = new Command("fromdacpacs", "Creates an SBM package from differences between two DACPAC files")
            {
                outputsbmOption.Copy(true),
                platinumdacpacSourceOption, 
                targetdacpacSourceOption

            };
            createFromDacpacsCommand.Handler = CommandHandler.Create<string, FileInfo, FileInfo>(Program.CreatePackageFromDacpacs);

            //Create an SBM from a platium DACPAC file
            var createFromDacpacDiffCommand = new Command("fromdacpacdiff", "Extract a SBM package from a source --platinumdacpac and a target database connection")
            {
                platinumdacpacOption.Copy(true),
                outputsbmOption.Copy(true),
                databaseOption.Copy(true),
                serverOption.Copy(true),
                usernameOption,
                passwordOption,
                authtypeOption
            };
            createFromDacpacDiffCommand.Handler = CommandHandler.Create<CommandLineArgs>(Program.CreateFromDacpacDiff);

            //Create an SBM from a platium DACPAC file (deprecated, but keeping in sync with fromdacpacdiff for now
            var scriptExtractCommand = new Command("scriptextract", "[*Deprecated - please use `sbm create fromdacpacdiff`] Extract a SBM package from a source --platinumdacpac");
            createFromDacpacDiffCommand.Options.ToList().ForEach(o => scriptExtractCommand.AddOption(o));
            scriptExtractCommand.Handler = CommandHandler.Create<CommandLineArgs>(Program.CreateFromDacpacDiff);

            //Create
            var createCommand = new Command("create", "Creates an SBM package from script files (fromscripts),  calculated database differences (fromdiff) or diffs between two DACPAC files (fromdacpacs)");
            createCommand.Add(createFromScriptsCommand);
            createCommand.Add(createFromDiffCommand);
            createCommand.Add(createFromDacpacsCommand);
            createCommand.Add(createFromDacpacDiffCommand);


            //Add
            var addCommand = new Command("add", "Adds one or more scripts to an SBM package or SBX project file from a list of scripts")
            {
                outputsbmOption.Copy(true),
                scriptListOption
            };
            addCommand.Handler = CommandHandler.Create<CommandLineArgs>(Program.AddScriptsToPackage);

            //List
            var listCommand = new Command("list", "List the script contents (order, script name, date added/modified, user info, script ids, script hashes) for SBM packages. (For SBX, just open the XML file!)")
            {
                packagesOption,
                withHashOption
            };
            listCommand.Handler = CommandHandler.Create<FileInfo[], bool>(Program.ListPackageScripts);

            //Run a policy check
            var policyCheckCommand = new Command("policycheck", "Performs a script policy check on the specified SBM package")
            {
                packagenameOption.Copy(true)
            };
            policyCheckCommand.Handler = CommandHandler.Create<CommandLineArgs>(Program.ExecutePolicyCheck);

            //Get the hash of an SBM package
            var getHashCommand = new Command("gethash", "Calculates the SHA-1 hash fingerprint value for the SBM package(scripts + run order)")
            {
                packagenameOption
            };
            getHashCommand.Handler = CommandHandler.Create<CommandLineArgs>(Program.GetPackageHash);

            //Create a backout SBM
            var createBackoutCommand = new Command("createbackout", "Generates a backout package (reversing stored procedure and scripted object changes)")
            {
                passwordOption,
                usernameOption,
                authtypeOption,
                packagenameOption,
                serverOption.Copy(true),
                databaseOption.Copy(true)
            };
            createBackoutCommand.Handler = CommandHandler.Create<CommandLineArgs>(Program.CreateBackout);



            RootCommand rootCommand = new RootCommand(description: "Tool to manage your SQL server database updates and releases");
            rootCommand.Add(logLevelOption);
            rootCommand.Add(buildCommand);
            rootCommand.Add(threadedCommand);
            rootCommand.Add(batchCommand);
            rootCommand.Add(createCommand);
            rootCommand.Add(addCommand);
            rootCommand.Add(packageCommand);
            rootCommand.Add(listCommand);
            rootCommand.Add(dacpacCommand);
            rootCommand.Add(policyCheckCommand);
            rootCommand.Add(getHashCommand);
            rootCommand.Add(createBackoutCommand);
            rootCommand.Add(getDifferenceCommand);
            rootCommand.Add(synchronizeCommand);
            rootCommand.Add(scriptExtractCommand);
    

            return rootCommand;
        }

        public static (CommandLineArgs, string) ParseArgumentsWithMessage(string[] args)
        {
            RootCommand rootCommand = CommandLineBuilder.SetUp();
            var res = rootCommand.Parse(args);
            if(res.Errors.Count > 0)
            {
                return (null, string.Join<string>(System.Environment.NewLine, res.Errors.Select(e => e.Message).ToArray()));
            }
            var binder = new ModelBinder(typeof(CommandLineArgs));
            var bindingContext = new BindingContext(res);
            var instance = (CommandLineArgs)binder.CreateInstance(bindingContext);

            return (instance, string.Empty);
        }
        public static CommandLineArgs ParseArguments(string[] args)
        {
            (CommandLineArgs cmd, string msg) = ParseArgumentsWithMessage(args);
            if(cmd == null)
            {
                throw new System.Exception($"Unable to parse arguments: {msg}");
            }
            else
            {
                return cmd;
            }
        }
    }
}
