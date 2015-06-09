using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using SqlSync.Connection;
using System.Data;
using log4net;
using SqlBuildManager.Interfaces.Console;
namespace SqlSync.SqlBuild.Syncronizer
{
    public class DatabaseSyncer
    {
        private static ILog log = log4net.LogManager.GetLogger(typeof(DatabaseSyncer));
        private bool lastBuildSuccessful = true;

        public Boolean SyncronizeDatabases(string goldServer, string goldDatabase, string toUpdateServer, string toUpdateDatabase, bool continueOnFailure)
        {
            ConnectionData gold = new ConnectionData()
            {
                DatabaseName = goldDatabase,
                SQLServerName = goldServer,
                UseWindowAuthentication = true
            };
            ConnectionData toUpdate = new ConnectionData()
            {
                DatabaseName = toUpdateDatabase,
                SQLServerName = toUpdateServer,
                UseWindowAuthentication = true
            };

            return SyncronizeDatabases(gold, toUpdate,continueOnFailure);
        }
        public Boolean SyncronizeDatabases(ConnectionData gold, ConnectionData toUpdate, bool continueOnFailure)
        {
            DatabaseDiffer diff = new DatabaseDiffer();


            DatabaseRunHistory toBeRun = diff.GetDatabaseHistoryDifference(gold, toUpdate);

            PushInfo(string.Format("{0} database packages found to run on {1}.{2}", toBeRun.BuildFileHistory.Count,toUpdate.SQLServerName, toUpdate.DatabaseName));
            
            if (toBeRun.BuildFileHistory.Count == 0) //already in sync
                return true;

            //Make temp directory for rebuild packages...
            string tempPath = System.IO.Path.GetTempPath() + System.Guid.NewGuid();
            Directory.CreateDirectory(tempPath);
            List<string> rebuiltPackages = new List<string>();

            //Create SBM packages for each
            foreach (var buildFileHistory in toBeRun.BuildFileHistory)
            {
                PushInfo(string.Format("Rebuilding Package {0} (Hash:{1})", buildFileHistory.BuildFileName, buildFileHistory.BuildFileHash));

                var fileName = tempPath + "\\" + Path.GetFileNameWithoutExtension(buildFileHistory.BuildFileName) + ".sbm"; //Make sure it creates and SBM and not an SBX
                var rebuildData = Rebuilder.RetreiveBuildData(gold, buildFileHistory.BuildFileHash, buildFileHistory.CommitDate);
                rebuildData.ForEach(h => h.ScriptFileName = Path.GetFileName(h.ScriptFileName)); //trim off the path, we just want the file name

                bool success = Rebuilder.RebuildBuildManagerFile(500, fileName, rebuildData);
                if (!success)
                {
                    PushInfo(string.Format("Error creating package {0} (Hash:{1}) see error log for details.", buildFileHistory.BuildFileName, buildFileHistory.BuildFileHash));
                    ProcessDirectoryCleanup(tempPath);
                    return false;
                }
                rebuiltPackages.Add(fileName);
            }

            bool syncronized = ProcessSyncronizationPackages(rebuiltPackages, toUpdate, false, continueOnFailure);
            
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

        private bool ProcessSyncronizationPackages(IEnumerable<string> sbmPackages, ConnectionData toUpdate, bool runAsTrial, bool continueOnFailure)
        {
            log.InfoFormat("Starting synchronization of {0} with {1} packages...", toUpdate.DatabaseName,sbmPackages.Count());

            string projFileName = string.Empty;
            string projectFilePath = string.Empty;
            string workingDirectory = string.Empty;
            string result = string.Empty;

            foreach (var sbmPackageName in sbmPackages)
            {
                log.InfoFormat("Synchronization run for {0}", Path.GetFileName(sbmPackageName));
                //Unzip and read the package
                SqlSyncBuildData buildData;
                if (!SqlBuildFileHelper.ExtractSqlBuildZipFile(sbmPackageName, ref workingDirectory, ref projectFilePath,
                                                               ref projFileName,
                                                               out result))
                {
                    PushInfo(string.Format("Unable to extract build file {0}. See log for details", sbmPackageName));
                    return false;
                }

                if (!SqlBuildFileHelper.LoadSqlBuildProjectFile(out buildData, projFileName, false))
                {
                    PushInfo(string.Format("Unable to load build file {0}. See log for details", sbmPackageName));
                    return false;
                }

                //set the build data for a new run 
                foreach (SqlSyncBuildData.ScriptRow scriptRow in buildData.Script)
                {
                    scriptRow.Database = "placeholder";
                    scriptRow.AllowMultipleRuns = true;
                }

                List<DatabaseOverride> lstOverride = new List<DatabaseOverride>();
                lstOverride.Add(new DatabaseOverride()
                    {
                        DefaultDbTarget = "placeholder",
                        OverrideDbTarget = toUpdate.DatabaseName,
                    });

                //Set the run meta-data
                SqlSync.SqlBuild.SqlBuildRunData runData = new SqlBuildRunData()
                    {
                        BuildData = buildData,
                        BuildType = BuildType.Other,
                        BuildDescription = new Random().Next(int.MinValue, int.MaxValue).ToString(),
                        //assign random build description
                        StartIndex = -1000, //make sure start a the beginning
                        ProjectFileName = projFileName,
                        IsTrial = runAsTrial,
                        BuildFileName = sbmPackageName,
                        IsTransactional = true,
                        TargetDatabaseOverrides = lstOverride 
                    };

                //Execute the package
                SqlBuildHelper helper = new SqlBuildHelper(toUpdate, false, string.Empty, runData.IsTransactional);
                helper.BuildCommittedEvent += new BuildCommittedEventHandler(helper_BuildCommittedEvent);
                helper.BuildErrorRollBackEvent += new EventHandler(helper_BuildErrorRollBackEvent);
                BackgroundWorker bg = new BackgroundWorker()
                    {
                        WorkerReportsProgress = true,
                        WorkerSupportsCancellation = true
                    };
                DoWorkEventArgs e = new DoWorkEventArgs(null);

                PushInfo(string.Format("Applying {0}", Path.GetFileName(sbmPackageName)));
                helper.ProcessBuild(runData, 0, bg, e);

                if (lastBuildSuccessful)
                {
                    string message = string.Format("Successfully applied {0}", Path.GetFileName(sbmPackageName));
                    PushInfo(message);
                    log.Info(message);
                }
                else
                {
                    string message = string.Format("Failed to apply {0}", Path.GetFileName(sbmPackageName));
                    PushInfo(message);
                    log.Info(message);
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
            if(Directory.Exists(tempPath))
                Directory.Delete(tempPath,true);
        }

        private void PushInfo(string message)
        {
            if(SyncronizationInfoEvent != null)
                SyncronizationInfoEvent(message);

        }
        public delegate void SyncronizationInfoEventHandler(string message);
        public event SyncronizationInfoEventHandler SyncronizationInfoEvent;

        private void helper_BuildCommittedEvent(object sender,RunnerReturn rr )
        {
            lastBuildSuccessful = true;
        }

        private void helper_BuildErrorRollBackEvent(object sender, EventArgs e)
        {
            lastBuildSuccessful = false;
        }

       
    }
    
}
