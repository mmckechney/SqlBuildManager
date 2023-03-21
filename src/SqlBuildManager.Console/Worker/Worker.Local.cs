using Microsoft.Extensions.Logging;
using SqlBuildManager.Console.Aad;
using SqlBuildManager.Console.CloudStorage;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.KeyVault;
using SqlBuildManager.Console.Threaded;
using SqlBuildManager.Interfaces.Console;
using SqlSync.Connection;
using SqlSync.SqlBuild.MultiDb;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using sb = SqlSync.SqlBuild;

namespace SqlBuildManager.Console
{
    internal partial class Worker
    {
        internal static int RunLocalBuildAsync(CommandLineArgs cmdLine)
        {
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);
            log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger<Worker>(Program.applicationLogFileName, cmdLine.RootLoggingPath);


            bool decryptSuccess;
            string workingDir = "";
            DateTime start = DateTime.Now;
            (decryptSuccess, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
            if (!decryptSuccess)
            {
                log.LogError("There was an error decrypting one or more value from the --settingsfile. Please check that you are using the correct --settingsfilekey value");
                return -8675;
            }

            try
            {
                cmdLine.BuildFileName = Path.GetFullPath(cmdLine.BuildFileName);
                log.LogDebug("Entering Local Build Execution");
                log.LogInformation($"Running Local Build with: {cmdLine.BuildFileName}");


                //We need an override setting. if not provided, we need to glean it from the SqlSyncBuildProject.xml file 
                List<DatabaseOverride> overrides = new List<DatabaseOverride>();
                MultiDbData multiDbData = null;
                if (!string.IsNullOrWhiteSpace(cmdLine.MultiDbRunConfigFileName) && !string.IsNullOrWhiteSpace(cmdLine.BuildFileName))
                {
                    var x = Validation.ValidateAndLoadMultiDbData(cmdLine.MultiDbRunConfigFileName, cmdLine, out MultiDbData multiData, out string[] errorMessages);
                    if (string.IsNullOrWhiteSpace(cmdLine.Server))
                    {
                        cmdLine.Server = multiData.First().ServerName;
                        overrides = multiData.First().Overrides;
                    }
                    if (string.IsNullOrWhiteSpace(cmdLine.Database))
                    {
                        cmdLine.Database = overrides.First().OverrideDbTarget;
                    }

                    if (multiData.Count > 1)
                    {
                        multiDbData = multiData;
                    }

                }
                else if (string.IsNullOrWhiteSpace(cmdLine.ManualOverRideSets) && !string.IsNullOrWhiteSpace(cmdLine.BuildFileName))
                {
                    cmdLine.ManualOverRideSets = sb.SqlBuildFileHelper.InferOverridesFromPackage(cmdLine.BuildFileName, cmdLine.Database);
                    var ovrRide = $"{cmdLine.Server}:{cmdLine.ManualOverRideSets}";
                    var def = ovrRide.Split(':')[1].Split(',')[0];
                    var target = ovrRide.Split(':')[1].Split(',')[1];
                    overrides = new List<DatabaseOverride>() { new DatabaseOverride() { DefaultDbTarget = def, OverrideDbTarget = target } };
                }
                else if (!string.IsNullOrWhiteSpace(cmdLine.ManualOverRideSets))
                {
                    var ovrRide = $"{cmdLine.Server}:{cmdLine.ManualOverRideSets}";
                    var def = ovrRide.Split(':')[1].Split(',')[0];
                    var target = ovrRide.Split(':')[1].Split(',')[1];
                    overrides = new List<DatabaseOverride>() { new DatabaseOverride() { DefaultDbTarget = def, OverrideDbTarget = target } };
                }


                if (string.IsNullOrEmpty(cmdLine.RootLoggingPath))
                {
                    cmdLine.RootLoggingPath = Directory.GetCurrentDirectory();
                }

                string projFilePath = "", projectFileName = "";
                sb.SqlBuildFileHelper.ExtractSqlBuildZipFile(cmdLine.BuildFileName, ref workingDir, ref projFilePath, ref projectFileName, true, true, out string result);

                bool success = sb.SqlBuildFileHelper.LoadSqlBuildProjectFile(out sb.SqlSyncBuildData buildData, projectFileName, true);
                if (!success)
                {
                    log.LogError($"Unable to load and extract build file: {cmdLine.BuildFileName}");
                    return -1;
                }

                sb.SqlBuildRunData sqlBuildRunData = new sb.SqlBuildRunData()
                {
                    ForceCustomDacpac = false,
                    BuildData = buildData,
                    IsTransactional = cmdLine.Transactional,
                    BuildDescription = cmdLine.Description,
                    BuildRevision = cmdLine.BuildRevision,
                    LogToDatabaseName = cmdLine.LogToDatabaseName,
                    TargetDatabaseOverrides = overrides,
                    ProjectFileName = projectFileName,
                    BuildFileName = cmdLine.BuildFileName,
                    AllowObjectDelete = cmdLine.AllowObjectDelete,


                };
                ConnectionData connData = new ConnectionData()
                {
                    SQLServerName = cmdLine.Server,
                    DatabaseName = cmdLine.Database,
                    AuthenticationType = cmdLine.AuthenticationArgs.AuthenticationType,
                    UserId = cmdLine.AuthenticationArgs.UserName,
                    Password = cmdLine.AuthenticationArgs.Password
                };
                sb.SqlBuildHelper helper = new sb.SqlBuildHelper(connData, true, "", cmdLine.Transactional);
                BackgroundWorker bg = new BackgroundWorker()
                {
                    WorkerReportsProgress = true,
                };
                bg.ProgressChanged += Bg_ProgressChanged;
                DoWorkEventArgs workArgs = new DoWorkEventArgs(sqlBuildRunData);
                LocalRunInfo.Sq1SyncBuildData = buildData;
                LocalRunInfo.BuildZipFileName = cmdLine.BuildFileName;
                LocalRunInfo.WorkingDirectory = workingDir;

                if (multiDbData == null)
                {
                    helper.ProcessBuild(sqlBuildRunData, cmdLine.TimeoutRetryCount, bg, workArgs);
                }
                else
                {
                    multiDbData.ProjectFileName = projectFileName;
                    multiDbData.RunAsTrial = cmdLine.Trial;
                    multiDbData.BuildFileName = cmdLine.BuildFileName;
                    multiDbData.BuildDescription = cmdLine.Description;
                    multiDbData.BuildData = buildData;
                    helper.ProcessMultiDbBuild(multiDbData, projectFileName, bg, workArgs);
                }
            }
            finally
            {
                sb.SqlBuildFileHelper.CleanUpAndDeleteWorkingDirectory(workingDir);
            }
            TimeSpan span = DateTime.Now - start;
            string msg = "Total Run time: " + span.ToString();
            log.LogInformation(msg);
            log.LogDebug("Exiting Single Build Execution");

            if (LocalRunInfo.Success)
                return 0;
            else
                return -2;
        }

        private static void Bg_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

            if (e.UserState is sb.GeneralStatusEventArgs) //Update the general run status
            {
                var stat = (sb.GeneralStatusEventArgs)e.UserState;
                log.LogInformation(stat.StatusMessage);
                if (stat.StatusMessage.ToLower().Contains("build failure") || stat.StatusMessage.ToLower().Contains("build failed"))
                {
                    LocalRunInfo.Success = false;
                }
            }

            else if (e.UserState is sb.CommitFailureEventArgs)
            {
                log.LogError("Failed to Commit Build " + ((sb.CommitFailureEventArgs)e.UserState).ErrorMessage);
                LocalRunInfo.Success = false;
            }
            else if (e.UserState is sb.ScriptRunStatusEventArgs)
            {
                log.LogInformation(((sb.ScriptRunStatusEventArgs)e.UserState).Status);
            }
            else if (e.UserState is sb.ScriptRunProjectFileSavedEventArgs)
            {
                log.LogInformation("Saving updated build file to disk");
                try
                {
                    sb.SqlBuildFileHelper.PackageProjectFileIntoZip(LocalRunInfo.Sq1SyncBuildData, LocalRunInfo.WorkingDirectory, LocalRunInfo.BuildZipFileName);
                    log.LogInformation("Build file saved to disk");
                }
                catch (Exception exe)
                {
                    log.LogError(exe.ToString());
                }
            }
            else if (e.UserState is Exception)
            {
                log.LogError("ERROR!" + ((Exception)e.UserState).Message);
            }
        }

        internal static int RunThreadedExecution(CommandLineArgs cmdLine, bool unittest = false)
        {
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);
            if (string.IsNullOrWhiteSpace(cmdLine.RootLoggingPath))
            {
                cmdLine.RootLoggingPath = Directory.GetCurrentDirectory();
            }
            log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger<Worker>(Program.applicationLogFileName, cmdLine.RootLoggingPath);

            (var success, cmdLine) = Init(cmdLine);
            if(!success)
            {
                log.LogError("There was a problem initilizing command settings");
                return -322;
            }
    
            if (cmdLine.IdentityArgs != null)
            {
                AadHelper.ManagedIdentityClientId = cmdLine.IdentityArgs.ClientId;
                AadHelper.TenantId = cmdLine.IdentityArgs.TenantId;
            }
            DateTime start = DateTime.Now;
            log.LogDebug("Entering Threaded Execution");
            log.LogDebug(cmdLine.ToStringExtension(StringType.Basic));
            log.LogDebug(cmdLine.ToStringExtension(StringType.Batch));
            log.LogInformation("Running Threaded Execution...");
            ThreadedManager tManager = new ThreadedManager(cmdLine);
            int retVal = tManager.Execute();
            ExecutionReturn exeResult;
            if (Enum.TryParse<ExecutionReturn>(retVal.ToString(), out exeResult))
            {
                switch (exeResult)
                {
                    case ExecutionReturn.Successful:
                        log.LogInformation("Completed Successfully");
                        break;
                    case ExecutionReturn.DacpacDatabasesInSync:
                        log.LogInformation("Datbases already in sync");
                        retVal = (int)ExecutionReturn.Successful;
                        break;
                    default:
                        log.LogWarning($"Completed with Errors - check log [{exeResult.ToString()}]");
                        break;
                }

            }
            else
            {
                log.LogWarning($"Completed with Errors - check log. Return code [{retVal.ToString()}]");
            }

            TimeSpan span = DateTime.Now - start;
            string msg = "Total Run time: " + span.ToString();
            log.LogInformation(msg);

            if (!String.IsNullOrEmpty(cmdLine.BatchArgs.OutputContainerSasUrl))
            {
                log.LogInformation("Writing log files to storage...");
                var blobTask = StorageManager.WriteLogsToBlobContainer(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, cmdLine.JobName, cmdLine.RootLoggingPath);
                blobTask.Wait();
            }

            log.LogDebug("Exiting Threaded Execution");
            return retVal;

        }

        private static class LocalRunInfo
        {
            public static sb.SqlSyncBuildData Sq1SyncBuildData { get; set; }
            public static string WorkingDirectory { get; set; }
            public static string BuildZipFileName { get; set; }
            public static bool Success { get; set; } = true;
        }
    }
}
