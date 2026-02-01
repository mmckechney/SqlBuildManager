using Microsoft.Extensions.Logging;
using SqlBuildManager.Interfaces.Console;
using SqlSync.Connection;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SqlSync.SqlBuild.Synchronizer
{
    public class DatabaseSyncer
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private bool lastBuildSuccessful = true;

        public async Task<Boolean> SyncronizeDatabasesAsync(string goldServer, string goldDatabase, string toUpdateServer, string toUpdateDatabase, bool continueOnFailure, CancellationToken cancellationToken = default)
        {
            ConnectionData gold = new ConnectionData()
            {
                DatabaseName = goldDatabase,
                SQLServerName = goldServer,
                AuthenticationType = AuthenticationType.Windows
            };
            ConnectionData toUpdate = new ConnectionData()
            {
                DatabaseName = toUpdateDatabase,
                SQLServerName = toUpdateServer,
                AuthenticationType = AuthenticationType.Windows
            };

            return await SyncronizeDatabasesAsync(gold, toUpdate, continueOnFailure, cancellationToken).ConfigureAwait(false);
        }

        public async Task<Boolean> SyncronizeDatabasesAsync(ConnectionData gold, ConnectionData toUpdate, bool continueOnFailure, CancellationToken cancellationToken = default)
        {
            DatabaseDiffer diff = new DatabaseDiffer();


            DatabaseRunHistory toBeRun = diff.GetDatabaseHistoryDifference(gold, toUpdate);

            PushInfo(string.Format("{0} database packages found to run on {1}.{2}", toBeRun.BuildFileHistory.Count, toUpdate.SQLServerName, toUpdate.DatabaseName));

            if (toBeRun.BuildFileHistory.Count == 0) //already in sync
                return true;

            //Make temp directory for rebuild packages...
            string tempPath = System.IO.Path.GetTempPath() + System.Guid.NewGuid();
            Directory.CreateDirectory(tempPath);
            List<string> rebuiltPackages = new List<string>();

            //Create SBM packages for each
            foreach (var buildFileHistory in toBeRun.BuildFileHistory)
            {
                cancellationToken.ThrowIfCancellationRequested();
                PushInfo(string.Format("Rebuilding Package {0} (Hash:{1})", buildFileHistory.BuildFileName, buildFileHistory.BuildFileHash));

                var fileName = tempPath + "\\" + Path.GetFileNameWithoutExtension(buildFileHistory.BuildFileName) + ".sbm"; //Make sure it creates and SBM and not an SBX
                var rebuildData = Rebuilder.RetreiveBuildData(gold, buildFileHistory.BuildFileHash, buildFileHistory.CommitDate);
                rebuildData.ForEach(h => h.ScriptFileName = Path.GetFileName(h.ScriptFileName)); //trim off the path, we just want the file name

                bool success = await Rebuilder.RebuildBuildManagerFileAsync(500, fileName, rebuildData, cancellationToken).ConfigureAwait(false);
                if (!success)
                {
                    PushInfo(string.Format("Error creating package {0} (Hash:{1}) see error log for details.", buildFileHistory.BuildFileName, buildFileHistory.BuildFileHash));
                    ProcessDirectoryCleanup(tempPath);
                    return false;
                }
                rebuiltPackages.Add(fileName);
            }

            bool syncronized = await ProcessSyncronizationPackagesAsync(rebuiltPackages, toUpdate, false, continueOnFailure, cancellationToken).ConfigureAwait(false);

            if (syncronized)
            {
                PushInfo(string.Format("Syncronized database {0}.{1} to source {2}.{3}", toUpdate.SQLServerName,
                                       toUpdate.DatabaseName, gold.SQLServerName, gold.DatabaseName));
            }
            else
            {
                PushInfo(string.Format("Syncronize failed to {0}.{1} from source {2}.{3}. See log for details.", toUpdate.SQLServerName,
                                       toUpdate.DatabaseName, gold.SQLServerName, gold.DatabaseName));
            }
            ProcessDirectoryCleanup(tempPath);

            return syncronized;

        }

        private async Task<bool> ProcessSyncronizationPackagesAsync(IEnumerable<string> sbmPackages, ConnectionData toUpdate, bool runAsTrial, bool continueOnFailure, CancellationToken cancellationToken = default)
        {
            log.LogInformation($"Starting synchronization of {toUpdate.DatabaseName} with {sbmPackages.Count()} packages...");

            string projFileName = string.Empty;
            string projectFilePath = string.Empty;
            string workingDirectory = string.Empty;
            string result = string.Empty;

            foreach (var sbmPackageName in sbmPackages)
            {
                cancellationToken.ThrowIfCancellationRequested();
                log.LogInformation($"Synchronization run for {Path.GetFileName(sbmPackageName)}");
                //Unzip and read the package
                var extractResult = await SqlBuildFileHelper.ExtractSqlBuildZipFileAsync(sbmPackageName, workingDirectory, resetWorkingDirectory: false, cancellationToken: cancellationToken).ConfigureAwait(false);
                if (!extractResult.success)
                {
                    PushInfo(string.Format("Unable to extract build file {0}. See log for details", sbmPackageName));
                    return false;
                }
                workingDirectory = extractResult.workingDirectory;
                projectFilePath = extractResult.projectFilePath;
                projFileName = extractResult.projectFileName;

                var (loadSuccess, buildModel) = await SqlBuildFileHelper.LoadSqlBuildProjectFileAsync(projFileName, false, cancellationToken).ConfigureAwait(false);
                if (!loadSuccess)
                {
                    PushInfo(string.Format("Unable to load build file {0}. See log for details", sbmPackageName));
                    return false;
                }

                //set the build data for a new run 
                var updatedScripts = new List<Models.Script>();
                foreach (var s in buildModel.Script)
                {
                    var updated = new Models.Script(
                        fileName: s.FileName,
                        buildOrder: s.BuildOrder,
                        description: s.Description,
                        rollBackOnError: s.RollBackOnError,
                        causesBuildFailure: s.CausesBuildFailure,
                        dateAdded: s.DateAdded,
                        scriptId: s.ScriptId,
                        database: "placeholder",
                        stripTransactionText: s.StripTransactionText,
                        allowMultipleRuns: true,
                        addedBy: s.AddedBy,
                        scriptTimeOut: s.ScriptTimeOut,
                        dateModified: s.DateModified,
                        modifiedBy: s.ModifiedBy,
                        tag: s.Tag);
                    updatedScripts.Add(updated);
                }
                buildModel = new Models.SqlSyncBuildDataModel(
                    sqlSyncBuildProject: buildModel.SqlSyncBuildProject,
                    script: updatedScripts,
                    build: buildModel.Build,
                    scriptRun: buildModel.ScriptRun,
                    committedScript: buildModel.CommittedScript);

                List<DatabaseOverride> lstOverride = new List<DatabaseOverride>();
                lstOverride.Add(new DatabaseOverride()
                {
                    DefaultDbTarget = "placeholder",
                    OverrideDbTarget = toUpdate.DatabaseName,
                });

                //Set the run meta-data
                var runDataModel = new SqlSync.SqlBuild.Models.SqlBuildRunDataModel(
                    buildDataModel: buildModel,
                    buildType: BuildType.Other,
                    server: toUpdate.SQLServerName,
                    buildDescription: new Random().Next(int.MinValue, int.MaxValue).ToString(),
                    startIndex: -1000,
                    projectFileName: projFileName,
                    isTrial: runAsTrial,
                    runItemIndexes: Array.Empty<double>(),
                    runScriptOnly: false,
                    buildFileName: sbmPackageName,
                    logToDatabaseName: string.Empty,
                    isTransactional: true,
                    platinumDacPacFileName: string.Empty,
                    targetDatabaseOverrides: lstOverride,
                    forceCustomDacpac: false,
                    buildRevision: string.Empty,
                    defaultScriptTimeout: 0,
                    allowObjectDelete: false);

                //Execute the package
                SqlBuildHelper helper = new SqlBuildHelper(toUpdate, false, string.Empty, runDataModel.IsTransactional ?? true);
                helper.BuildCommittedEvent += new BuildCommittedEventHandler(helper_BuildCommittedEvent);
                helper.BuildErrorRollBackEvent += new EventHandler(helper_BuildErrorRollBackEvent);

                PushInfo(string.Format("Applying {0}", Path.GetFileName(sbmPackageName)));
                await helper.ProcessBuildAsync(runData: runDataModel, allowableTimeoutRetries: 0).ConfigureAwait(false);

                if (lastBuildSuccessful)
                {
                    string message = string.Format("Successfully applied {0}", Path.GetFileName(sbmPackageName));
                    PushInfo(message);
                    log.LogInformation(message);
                }
                else
                {
                    string message = string.Format("Failed to apply {0}", Path.GetFileName(sbmPackageName));
                    PushInfo(message);
                    log.LogInformation(message);
                    if (!continueOnFailure)
                    {
                        PushInfo("Cancelling sync.");
                        return false;
                    }
                    else
                    {
                        PushInfo("Continue On Failure set. Continuing sync process...");
                    }
                }

            }
            return true;
        }


        private void ProcessDirectoryCleanup(string tempPath)
        {
            if (Directory.Exists(tempPath))
                Directory.Delete(tempPath, true);
        }

        private void PushInfo(string message)
        {
            if (SyncronizationInfoEvent != null)
                SyncronizationInfoEvent(message);

        }
        public delegate void SyncronizationInfoEventHandler(string message);
        public event SyncronizationInfoEventHandler SyncronizationInfoEvent;

        private void helper_BuildCommittedEvent(object sender, RunnerReturn rr)
        {
            lastBuildSuccessful = true;
        }

        private void helper_BuildErrorRollBackEvent(object sender, EventArgs e)
        {
            lastBuildSuccessful = false;
        }


    }

}
