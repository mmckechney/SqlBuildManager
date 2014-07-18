using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using SqlSync.Connection;

namespace SqlSync.SqlBuild.Syncronizer
{
    public class DatabaseSyncer
    {
        private bool lastBuildSuccessful = true;
        public Boolean SyncronizeDatabases(ConnectionData gold, ConnectionData toUpdate)
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
                bool success = Rebuilder.RebuildBuildManagerFile(500, fileName, rebuildData);
                if (!success)
                {
                    PushInfo(string.Format("Error creating package {0} (Hash:{1}) see error log for details.", buildFileHistory.BuildFileName, buildFileHistory.BuildFileHash));
                    ProcessDirectoryCleanup(tempPath);
                    return false;
                }
                rebuiltPackages.Add(fileName);
            }

            if (!ProcessSyncronizationPackages(rebuiltPackages, toUpdate, true))
            {
                ProcessDirectoryCleanup(tempPath);
                return false;
            }
            return true;
        }

        private bool ProcessSyncronizationPackages(IEnumerable<string> sbmPackages, ConnectionData toUpdate, bool runAsTrial)
        {
            string projFileName = string.Empty;
            string projectFilePath = string.Empty;
            string workingDirectory = string.Empty;
            string result = string.Empty;

            foreach (var sbmPackageName in sbmPackages)
            {
                //Unzip and read the package
                SqlSyncBuildData buildData;
                SqlBuildFileHelper.ExtractSqlBuildZipFile(sbmPackageName, ref workingDirectory, ref projectFilePath,
                                                          ref projFileName,
                                                          out result);

                SqlBuildFileHelper.LoadSqlBuildProjectFile(out buildData, projFileName, false);

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
                        IsTransactional = true//,
                       // TargetDatabaseOverrides = this.targetDatabaseOverrideCtrl1.GetOverrideData()
                    };

                //Execute the package
                SqlBuildHelper helper = new SqlBuildHelper(toUpdate, false, string.Empty, runData.IsTransactional);
                helper.BuildCommittedEvent += new EventHandler(helper_BuildCommittedEvent);
                helper.BuildErrorRollBackEvent += new EventHandler(helper_BuildErrorRollBackEvent);
                BackgroundWorker bg = new BackgroundWorker()
                    {
                        WorkerReportsProgress = true,
                        WorkerSupportsCancellation = true
                    };
                DoWorkEventArgs e = new DoWorkEventArgs(null);
                helper.ProcessBuild(runData, 0, bg, e);

                if (lastBuildSuccessful)
                {
                    PushInfo(string.Format("Successfully applied {0}", Path.GetFileName(sbmPackageName)));
                }
                else
                {
                    PushInfo(string.Format("Failed to apply {0}", Path.GetFileName(sbmPackageName)));
                    return false;
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

        private void helper_BuildCommittedEvent(object sender, EventArgs e)
        {
            lastBuildSuccessful = true;
        }

        private void helper_BuildErrorRollBackEvent(object sender, EventArgs e)
        {
            lastBuildSuccessful = false;
        }
    }
    
}
