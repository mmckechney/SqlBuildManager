using SqlSync.Connection;
using SqlSync.Constants;
using SqlSync.DbInformation;
using SqlSync.DbInformation.ChangeDates;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Services;
using System;
using System.Collections.Generic;
using System.IO;
namespace SqlSync.SqlBuild.Status
{
    public class StatusHelper
    {
        public static ScriptStatusType DetermineScriptRunStatus(IDatabaseUtility dbUtil, Script script, ConnectionData connData, string projectFilePath, bool checkForChanges, List<DatabaseOverride> overrides, out DateTime commitDate, out DateTime serverChangeDate)
        {
            string targetDatabase = ConnectionHelper.GetTargetDatabase(script.Database ?? string.Empty, overrides);

            //Update the routine (sp and functions) change date cache
            if (DatabaseObjectChangeDates.Servers[connData.SQLServerName][targetDatabase].LastRefreshTime < DateTime.Now.AddSeconds(-30))
            {
                InfoHelper.UpdateRoutineAndViewChangeDates(connData, overrides);
            }
            bool preRun = false;
            bool hashChanged = false;
            string scriptHash = string.Empty;
            string scriptTextHash = string.Empty;
            commitDate = DateTime.MinValue;
            serverChangeDate = DateTime.MinValue;
            
            string scriptIdStr = script.ScriptId ?? string.Empty;
            if (!Guid.TryParse(scriptIdStr, out Guid scriptGuid))
                scriptGuid = Guid.Empty;
                
            preRun = (dbUtil.HasBlockingSqlLog(scriptGuid, connData, targetDatabase, out scriptHash, out scriptTextHash, out commitDate) == true);
            
            string fileName = script.FileName ?? string.Empty;
            if (!File.Exists(Path.Combine(projectFilePath, fileName)))
            {
                return ScriptStatusType.FileMissing;
            }

            if (preRun && checkForChanges)
            {
                if (scriptHash == string.Empty || scriptTextHash == string.Empty)
                    dbUtil.HasBlockingSqlLog(scriptGuid, connData, targetDatabase, out scriptHash, out scriptTextHash, out commitDate);

                if (scriptHash != string.Empty || scriptTextHash != string.Empty)
                {
                    string fileTextHash;
                    string fileHash;
                    SqlBuildFileHelper.GetSHA1Hash(Path.Combine(projectFilePath, fileName), out fileHash, out fileTextHash, script.StripTransactionText ?? false);
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
            
            string routineName = fileName.Length > 4 ? fileName.Substring(0, fileName.Length - 4).ToLower() : fileName.ToLower();
            if (fileName.EndsWith(DbObjectType.Trigger, StringComparison.CurrentCultureIgnoreCase) && routineName.IndexOf(" - ") > -1)
                routineName = routineName.Split(new char[] { '-' })[1].Trim();

            if (!preRun)
            {
                commitDate = (script.DateModified == null || script.DateModified < new DateTime(1980, 1, 1)) ? (script.DateAdded ?? DateTime.MinValue) : script.DateModified.Value;

                if (fileName.EndsWith(DbObjectType.StoredProcedure, StringComparison.CurrentCultureIgnoreCase) ||
                        fileName.EndsWith(DbObjectType.UserDefinedFunction, StringComparison.CurrentCultureIgnoreCase) ||
                        fileName.EndsWith(DbObjectType.View, StringComparison.CurrentCultureIgnoreCase) ||
                        fileName.EndsWith(DbObjectType.Table, StringComparison.CurrentCultureIgnoreCase) ||
                        fileName.EndsWith(DbObjectType.Trigger, StringComparison.CurrentCultureIgnoreCase))
                {
                    serverChangeDate = DatabaseObjectChangeDates.Servers[connData.SQLServerName][targetDatabase][routineName];

                    if (commitDate < serverChangeDate)
                        return ScriptStatusType.NotRunButOlderVersion;
                }

                return ScriptStatusType.NotRun;
            }
            else
            {
                if (script.AllowMultipleRuns == false)
                {
                    if (!hashChanged)
                        return ScriptStatusType.Locked;
                    else
                        return ScriptStatusType.ChangedSinceCommit;
                }
                else
                {
                    if (!hashChanged)
                    {
                        if (fileName.EndsWith(DbObjectType.StoredProcedure, StringComparison.CurrentCultureIgnoreCase) ||
                          fileName.EndsWith(DbObjectType.UserDefinedFunction, StringComparison.CurrentCultureIgnoreCase) ||
                          fileName.EndsWith(DbObjectType.View, StringComparison.CurrentCultureIgnoreCase) ||
                          fileName.EndsWith(DbObjectType.Table, StringComparison.CurrentCultureIgnoreCase) ||
                          fileName.EndsWith(DbObjectType.Trigger, StringComparison.CurrentCultureIgnoreCase))
                        {
                            serverChangeDate = DatabaseObjectChangeDates.Servers[connData.SQLServerName][targetDatabase][routineName];

                            if (commitDate.Ticks + 50000000 < serverChangeDate.Ticks)
                                return ScriptStatusType.ServerChange;

                            if (serverChangeDate == DateTime.MinValue)
                                return ScriptStatusType.NotRun;
                        }
                        return ScriptStatusType.UpToDate;
                    }
                    else
                    {
                        return ScriptStatusType.ChangedSinceCommit;
                    }
                }
            }
        }

        public static void SetScriptRunStatusAndDates(SqlSyncBuildDataModel model, IDatabaseUtility dbUtil, ConnectionData connData, string projectFilePath)
        {
            DateTime commitDate;
            DateTime serverChangeDate;
            foreach (Script script in model.Script)
            {
                script.ScriptRunStatus = DetermineScriptRunStatus(dbUtil, script, connData, projectFilePath, true, OverrideData.TargetDatabaseOverrides, out commitDate, out serverChangeDate);
                script.LastCommitDate = commitDate;
                script.ServerChangeDate = serverChangeDate;
            }
        }
    }
}
