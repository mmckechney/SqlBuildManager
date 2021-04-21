using System;
using System.Collections.Generic;
using System.Text;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.CommandLine.Parsing;
using SqlSync.SqlBuild;
using System.IO;
using Microsoft.Extensions.Logging;

namespace SqlBuildManager.Console
{
    class CommandLineConfig
    {
        internal static RootCommand SetUp()
        {
            var settingsfileOption = new Option(new string[] { "--settingsfile" }, "Saved settings file to load parameters from")
            {
                Argument = new Argument<FileInfo>("settingsfile").ExistingOnly(),
                Name = "SettingsFileInfo",
            };
            var settingsfileKeyOption = new Option(new string[] { "--settingsfilekey" }, "Key for the encryption of sensitive informtation in the settings file (must be at least 16 characters). It can be either the key string or a file path to a key file. The key may also provided by setting a 'sbm-settingsfilekey' Environment variable. If not provided a machine value will be used.")
            {
                Argument = new Argument<string>("settingsfilekey"),
            };
            var overrideOption = new Option(new string[] { "--override" }, "File containing the target database settings (usually a formatted .cfg file)")
            {
                Argument = new Argument<string>("override")
            };
            var serverOption = new Option(new string[] { "-s", "--server" }, "1) Name of a server for single database run or 2) source server for scripting or runtime configuration")
            {
                Argument = new Argument<string>("server")
            };
            var databaseOption = new Option(new string[] { "-d", "--database" }, "1) Name of a single database to run against or 2) source database for scripting or runtime configuration")
            {
                Argument = new Argument<string>("database")
            };
            var rootloggingpathOption = new Option(new string[] { "--rootloggingpath" }, "Directory to save execution logs (for threaded and remote executions)")
            {
                Argument = new Argument<string>("rootloggingpath")
            };
            var trialOption = new Option(new string[] { "--trial" }, "Whether or not to run in trial mode(default is false)")
            {
                Argument = new Argument<bool>("trial")
            };
            var scriptsrcdirOption = new Option(new string[] { "--scriptsrcdir" }, " [Not recommended] Alternative ability to run against a directory of scripts (vs .sbm or .sbx file)")
            {
                Argument = new Argument<string>("scriptsrcdir")
            };
            var usernameOption = new Option(new string[] { "-u", "--username" }, "The username to authenticate against the database if not using integrate auth")
            {
                Argument = new Argument<string>("username")
            };
            var passwordOption = new Option(new string[] { "-p", "--password" }, "The password to authenticate against the database if not using integrate auth")
            {
                Argument = new Argument<string>("password")
            };
            var logtodatabasenamedOption = new Option(new string[] { "--logtodatabasename" }, "[Not recommended] Specifies that the SqlBuild_logging logs should go to an alternate database (vs. target).")
            {
                Argument = new Argument<string>("logtodatabasename")
            };
            var descriptionOption = new Option(new string[] { "--description" }, "Description of build (logged with build)")
            {
                Argument = new Argument<string>("description")
            };
            var packagenameOption = new Option(new string[] { "--packagename", "--buildfilename" }, "Name of the .sbm or .sbx file to execute")
            {
                Argument = new Argument<string>("BuildFileName"),
                Required = true
            };
            var directoryOption = new Option(new string[] { "--directory" }, "Directory containing 1 or more SBX files to package into SBM zip files")
            {
                Argument = new Argument<string>("directory"),
                Required = true
            };
            var transactionalOption = new Option(new string[] { "--transactional" }, "Whether or not to run with a wrapping transaction (default is true)")
            {
                Argument = new Argument<bool>("transactional")
            };
            var timeoutretrycountOption = new Option(new string[] { "--timeoutretrycount" }, "How many retries to attempt if a timeout exception occurs")
            {
                Argument = new Argument<int>("timeoutretrycount")
            };
            var golddatabaseOption = new Option(new string[] { "--golddatabase" }, "The \"gold copy\" database that will serve as the model for what the target database should look like")
            {
                Argument = new Argument<string>("golddatabase"),
                Required = true
            };
            var goldserverOption = new Option(new string[] { "--goldserver" }, "The server that the \"gold copy\" database can be found")
            {
                Argument = new Argument<string>("goldserver"),
                Required = true
            };
            var continueonfailureOption = new Option(new string[] { "--continueonfailure" }, "Whether or not to continue on the failure of a package (default is false)")
            {
                Argument = new Argument<bool>("continueonfailure")
            };
            var platinumdacpacOption = new Option(new string[] { "--platinumdacpac" }, "Name of the dacpac containing the platinum schema")
            {
                Argument = new Argument<string>("platinumdacpac")
            };
            var targetdacpacOption = new Option(new string[] { "--targetdacpac" }, "Name of the dacpac containing the schema of the database to be updated")
            {
                Argument = new Argument<string>("targetdacpac")
            };
            var forcecustomdacpacOption = new Option(new string[] { "--forcecustomdacpac" }, "USE WITH CAUTION! This will force the dacpac extraction and creation of custom scripts for EVERY target database! Your execution will take much longer.")
            {
                Argument = new Argument<bool>("forcecustomdacpac")
            };
            var platinumdbsourceOption = new Option(new string[] { "--platinumdbsource" }, "Instead of a formally built Platinum Dacpac, target this database as having the desired state schema")
            {
                Argument = new Argument<string>("platinumdbsource")
            };
            var platinumserversourceOption = new Option(new string[] { "--platinumserversource" }, "Instead of a formally built Platinum Dacpac, target a database on this server as having the desired state schema")
            {
                Argument = new Argument<string>("platinumserversource")
            };
            var buildrevisionOption = new Option(new string[] { "--buildrevision" }, "If provided, the build will include an update to a \"Versions\" table and this will be the value used to add to a \"VersionNumber\" column (varchar(max))")
            {
                Argument = new Argument<string>("buildrevision")
            };
            var outputsbmOption = new Option(new string[] { "--outputsbm" }, "Name (and path) of the SBM package to create")
            {
                Argument = new Argument<string>("outputsbm")
            };
            var deletebatchpoolOption = new Option(new string[] { "--deletebatchpool" }, "Whether or not to delete the batch pool servers after an execution (default is false)")
            {
                Argument = new Argument<bool>("deletebatchpool")
            };
            var deletebatchjobOption = new Option(new string[] { "--deletebatchjob" }, "Whether or not to delete the batch job after an execution (default is true)")
            {
                Argument = new Argument<bool>("deletebatchjob")
            };
            var batchnodecountOption = new Option(new string[] { "--nodecount", "--batchnodecount" }, "Number of nodes to provision to run the batch job  (default is 10)")
            {
                Argument = new Argument<int>("batchnodecount")
            };
            var batchjobnameOption = new Option(new string[] { "--jobname", "--batchjobname" }, "[Optional] User friendly name for the job. This will also be the container name for the stored logs. Any disallowed URL characters will be removed")
            {
                Argument = new Argument<string>("batchjobname")
            };
            var batchaccountnameOption = new Option(new string[] { "--batchaccountname" }, "String name of the Azure Batch account  [can also be set via BatchAccountName app settings key]")
            {
                Argument = new Argument<string>("batchaccountname")
            };
            var batchaccountkeyOption = new Option(new string[] { "--batchaccountkey" }, "Account Key for the Azure Batch account [can also be set via BatchAccountKey app settings key]")
            {
                Argument = new Argument<string>("batchaccountkey")
            };
            var batchaccounturlOption = new Option(new string[] { "--batchaccounturl" }, "URL for the Azure Batch account [can also be set via BatchAccountUrl app settings key]")
            {
                Argument = new Argument<string>("batchaccounturl")
            };
            var storageaccountnameOption = new Option(new string[] { "--storageaccountname" }, "Name of storage account associated with the Azure Batch account  [can also be set via StorageAccountName app settings key]")
            {
                Argument = new Argument<string>("storageaccountname")
            };
            var storageaccountkeyOption = new Option(new string[] { "--storageaccountkey" }, "Account Key for the storage account  [can also be set via StorageAccountKey app settings key]")
            {
                Argument = new Argument<string>("storageaccountkey")
            };
            var batchvmsizeOption = new Option(new string[] { "--vmsize", "--batchvmsize" }, "Size key for VM size required (see https://docs.microsoft.com/en-us/azure/virtual-machines/windows/sizes-general) [can also be set via BatchVmSize app settings key]")
            {
                Argument = new Argument<string>("batchvmsize")
            };
            var batchpoolnameOption = new Option(new string[] { "--poolname", "--batchpoolname" }, "Override for the default pool name of \"SqlBuildManagerPool\"")
            {
                Argument = new Argument<string>("batchpoolname")
            };
            var batchpoolOsOption = new Option(new string[] { "--os", "--batchpoolos" }, "Operating system for the Azure Batch nodes. Windows is default")
            {
                Argument = new Argument<OsType>("batchpoolos")
            };
            var batchApplicationOption = new Option(new string[] { "--apppackage", "--applicationpackage" }, "The Azure Batch application package name. (Default is 'SqlBuildManagerWindows' for Windows and 'SqlBuildManagerLinux' for Linux")
            {
                Argument = new Argument<OsType>("applicationpackage")
            };
            var eventhubconnectionOption = new Option(new string[] { "--eventhubconnection" }, "Event Hub connection string for Event Hub logging of batch execution")
            {
                Argument = new Argument<string>("eventhubconnection")
            };
            var pollbatchpoolstatusOption = new Option(new string[] { "--pollbatchpoolstatus" }, "Whether or not you want to get updated status (true, default) or fire and forget (false)")
            {
                Argument = new Argument<bool>("pollbatchpoolstatus")
            };
            var defaultscripttimeoutOption = new Option(new string[] { "--defaultscripttimeout" }, "Override the default script timeouts set when creating a DACPAC (default is 500)")
            {
                Argument = new Argument<int>("defaultscripttimeout")
            };
            var authtypeOption = new Option(new string[] { "--authtype" }, "SQL Authentication type to use.")
            {
                Argument = new Argument<SqlSync.Connection.AuthenticationType>("AuthenticationType", () => SqlSync.Connection.AuthenticationType.Password),
                Name = "AuthenticationType"
            };
            var silentOption = new Option(new string[] { "--silent" }, "Suppresses overwrite prompt if file already exists")
            {
                Argument = new Argument<bool>("silent")
            };
            var outputcontainersasurlOption = new Option(new string[] { "--outputcontainersasurl" }, "[Internal only] Runtime storage SAS url (auto-generated from `sbm batch run` command")
            {
                Argument = new Argument<string>("outputcontainersasurl")
            };
            var dacpacOutputOption = new Option(new string[] { "--dacpacname" }, "Name of the dacpac that you want to create")
            {
                Argument = new Argument<string>("dacpacname"),
                Required = true
            };
            var cleartextOption = new Option(new string[] { "--cleartext" }, "Flag to save settings file in clear text (vs. encrypted)")
            {
                Argument = new Argument<bool>("cleartext")
            };
            var queryFileOption = new Option(new string[] { "--queryfile" }, "File containing the SELECT query to run across the databases")
            {
                Argument = new Argument<FileInfo>("queryfile").ExistingOnly()
            };
            var outputFileOption = new Option(new string[] { "--outputfile" }, "Results output file to create")
            {
                Argument = new Argument<FileInfo>("outputfile")
            };
            var threadedConcurrencyOption = new Option(new string[] { "--concurrency" }, "Maximum concurrency for threaded executions")
            {
                Argument = new Argument<int>("concurrency", () => 8)
            };
            var threadedConcurrencyTypeOption = new Option(new string[] { "--concurrencytype" }, "Type of concurrency, used in conjunction with --concurrency ")
            {
                Argument = new Argument<ConcurrencyType>("concurrencytype", () => ConcurrencyType.Count)
            };
            var logLevelOption = new Option(new string[] { "--loglevel" }, "Logging level for console and log file")
            {
                Argument = new Argument<LogLevel>("loglevel", () => LogLevel.Information)
            };


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
                scriptsrcdirOption
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


            //Batch Save settings file
            var saveSettingsCommand = new Command("savesettings", "Save a settings json file for Batch arguments (see Batch documentation)")
            {
                passwordOption,
                usernameOption,
                settingsfileKeyOption,
                settingsfileOption.Copy(true),
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
                //Additional settings
                timeoutretrycountOption,
                pollbatchpoolstatusOption,
                silentOption,
                cleartextOption
            };
            saveSettingsCommand.Handler = CommandHandler.Create<CommandLineArgs, bool>(Program.SaveAndEncryptSettings);

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
                threadedConcurrencyTypeOption

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

            //Azure Batch base command
            var batchCommand = new Command("batch", "Commands for setting and executing a batch run or batch query");
            batchCommand.Add(saveSettingsCommand);
            batchCommand.Add(batchPreStageCommand);
            batchCommand.Add(batchCleanUpCommand);
            batchCommand.Add(batchRunCommand);
            batchCommand.Add(batchQueryCommand);
            batchCommand.Add(batchRunThreadedCommand);
            batchCommand.Add(batchQueryThreadedCommand);


            /****************************************
             * Utility commands 
             ***************************************/

            //Create an SBM from a platium DACPAC file
            var scriptExtractCommand = new Command("scriptextract", "Extract a SBM package from a source --platinumdacpac")
            {
                platinumdacpacOption.Copy(true),
                outputsbmOption.Copy(true),
                databaseOption.Copy(true),
                serverOption.Copy(true),
                usernameOption.Copy(true),
                passwordOption.Copy(true)
            };
            scriptExtractCommand.Handler = CommandHandler.Create<CommandLineArgs>(Program.ScriptExtraction);

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
            rootCommand.Add(packageCommand);
            rootCommand.Add(policyCheckCommand);
            rootCommand.Add(getHashCommand);
            rootCommand.Add(createBackoutCommand);
            rootCommand.Add(getDifferenceCommand);
            rootCommand.Add(synchronizeCommand);
            rootCommand.Add(scriptExtractCommand);
            rootCommand.Add(dacpacCommand);

            return rootCommand;
        }
    }
}
