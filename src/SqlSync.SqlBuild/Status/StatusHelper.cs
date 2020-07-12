using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using SqlSync.DbInformation;
using SqlSync.DbInformation.ChangeDates;
using SqlSync.Connection;
using SqlSync.Constants;
namespace SqlSync.SqlBuild.Status
{
    public class StatusHelper
    {
        public static ScriptStatusType DetermineScriptRunStatus(SqlSyncBuildData.ScriptRow row, ConnectionData connData, string projectFilePath, bool checkForChanges, List<DatabaseOverride> overrides,  out DateTime commitDate, out DateTime serverChangeDate)
        {
            string targetDatabase = ConnectionHelper.GetTargetDatabase(row.Database, overrides);

            //Update the routine (sp and functions) change date cache
            if (DatabaseObjectChangeDates.Servers[connData.SQLServerName][targetDatabase].LastRefreshTime < DateTime.Now.AddSeconds(-30))
            {
                //InfoHelper.DatabaseRoutineChangeDates = InfoHelper.GetRoutineChangeDates(connData, this.targetDatabaseOverrideCtrl1.GetOverrideData());
                InfoHelper.UpdateRoutineAndViewChangeDates(connData, overrides);
            }
            bool preRun = false;
            bool hashChanged = false;
            //Determine icon
            string scriptHash = string.Empty;
            string scriptTextHash = string.Empty;
            commitDate = DateTime.MinValue;
            serverChangeDate = DateTime.MinValue;
            // preRun = (committedScriptView.Find(row.ScriptId) > -1 || helper.HasBlockingSqlLog(new Guid(row.ScriptId), this.data, targetDatabase, out scriptHash, out scriptTextHash) == true);
            preRun = (SqlBuildHelper.HasBlockingSqlLog(new Guid(row.ScriptId), connData, targetDatabase, out scriptHash, out scriptTextHash, out commitDate) == true);
            //Check that the file exists
            if (!File.Exists(Path.Combine(projectFilePath, row.FileName)))
            {
                return ScriptStatusType.FileMissing;
            }

            //Get the latest hash from the Db only (don't care if the build file is out of date!)
            if (preRun && checkForChanges)
            {
                if (scriptHash == string.Empty || scriptTextHash == string.Empty)
                    SqlBuildHelper.HasBlockingSqlLog(new Guid(row.ScriptId), connData, targetDatabase, out scriptHash, out scriptTextHash, out commitDate);

                /*If the scriptHash is STILL empty, then the file and the Db are out of sync.
                 *	This could be due to a Db refresh, in which case we'll mark it as changed
                 *	by default
                */
                if (scriptHash != string.Empty || scriptTextHash != string.Empty)
                {
                    string fileTextHash;
                    string fileHash;
                    SqlBuildFileHelper.GetSHA1Hash(Path.Combine(projectFilePath, row.FileName), out fileHash, out fileTextHash, row.StripTransactionText);
                    if (fileHash != scriptHash && fileTextHash != scriptHash && fileHash != scriptTextHash && fileTextHash != scriptTextHash)
                    {
                        if (fileHash == SqlBuildFileHelper.FileMissing)
                        {
                            return ScriptStatusType.FileMissing;
                        }
                        hashChanged = true;
                    }
                }
                else
                {
                    hashChanged = true;
                }

            }
            string routineName = row.FileName.Substring(0, row.FileName.Length - 4).ToLower();
            if (row.FileName.EndsWith(DbObjectType.Trigger, StringComparison.CurrentCultureIgnoreCase) && routineName.IndexOf(" - ") > -1)
                routineName = routineName.Split(new char[] { '-' })[1].Trim();

            if (!preRun) //not run at all 
            {
                commitDate = (row.IsDateModifiedNull() || row.DateModified < new DateTime(1980, 1, 1)) ? row.DateAdded : row.DateModified;

                if (row.FileName.EndsWith(DbObjectType.StoredProcedure, StringComparison.CurrentCultureIgnoreCase) ||
                        row.FileName.EndsWith(DbObjectType.UserDefinedFunction, StringComparison.CurrentCultureIgnoreCase) ||
                        row.FileName.EndsWith(DbObjectType.View, StringComparison.CurrentCultureIgnoreCase) ||
                        row.FileName.EndsWith(DbObjectType.Table, StringComparison.CurrentCultureIgnoreCase) ||
                        row.FileName.EndsWith(DbObjectType.Trigger, StringComparison.CurrentCultureIgnoreCase))
                {
                    serverChangeDate = DatabaseObjectChangeDates.Servers[connData.SQLServerName][targetDatabase][routineName];

                    if (commitDate < serverChangeDate)
                        return ScriptStatusType.NotRunButOlderVersion; // question mark
                }

                return ScriptStatusType.NotRun;
            }
            else
            {
                if (row.AllowMultipleRuns == false) //(committedScriptView.Find(row.ScriptId) > -1 || helper.HasBlockingSqlLog(new Guid(row.ScriptId),this.data,row.Database) == true))
                {
                    if (!hashChanged)
                        return ScriptStatusType.Locked; // "locked"
                    else
                        return ScriptStatusType.ChangedSinceCommit; //the caution icon!
                }
                else
                {

                    if (!hashChanged) //if "OK" from a SBM status, need to check the DB next for SP's and Functions
                    {
                        if (row.FileName.EndsWith(DbObjectType.StoredProcedure, StringComparison.CurrentCultureIgnoreCase) ||
                          row.FileName.EndsWith(DbObjectType.UserDefinedFunction, StringComparison.CurrentCultureIgnoreCase) ||
                          row.FileName.EndsWith(DbObjectType.View, StringComparison.CurrentCultureIgnoreCase) ||
                          row.FileName.EndsWith(DbObjectType.Table, StringComparison.CurrentCultureIgnoreCase) ||
                          row.FileName.EndsWith(DbObjectType.Trigger, StringComparison.CurrentCultureIgnoreCase))
                        {
                            serverChangeDate = DatabaseObjectChangeDates.Servers[connData.SQLServerName][targetDatabase][routineName];

                            //Add in 5 second threshold
                            if (commitDate.Ticks + 50000000 < serverChangeDate.Ticks)
                                return ScriptStatusType.ServerChange; // magnifying glass

                            //if the serverChangeDate here is MinValue, it means that the routine is not in this DB, therefore, we need to set as not run.
                            if (serverChangeDate == DateTime.MinValue) 
                                return ScriptStatusType.NotRun; //"gray server icon"
                        }
                        return ScriptStatusType.UpToDate; //green "OK"
                    }
                    else
                    {
                        return ScriptStatusType.ChangedSinceCommit; //the caution icon!
                    }
                }
            }
        }

        public static void SetScriptRunStatusAndDates(ref SqlSyncBuildData buildData, ConnectionData connData, string projectFilePath)
        {
            DateTime commitDate;
            DateTime serverChangeDate;
            foreach (SqlSyncBuildData.ScriptRow row in buildData.Script)
            {
                row.ScriptRunStatus = DetermineScriptRunStatus(row, connData, projectFilePath, true, OverrideData.TargetDatabaseOverrides, out commitDate, out serverChangeDate);
                row.LastCommitDate = commitDate;
                row.ServerChangeDate = serverChangeDate;
            }
        }
    }
}
