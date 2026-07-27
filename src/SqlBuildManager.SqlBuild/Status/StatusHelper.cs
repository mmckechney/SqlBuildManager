using SqlBuildManager.Connection;
using SqlBuildManager.Constants;
using SqlBuildManager.DbInformation;
using SqlBuildManager.DbInformation.ChangeDates;
using SqlBuildManager.SqlBuild.Models;
using SqlBuildManager.SqlBuild.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SqlBuildManager.SqlBuild.Status
{
    public class StatusHelper
    {
        /// <summary>
        /// Determines the run status of a single script by querying the database directly.
        /// Preserves the original single-script public API for individual callers.
        /// </summary>
        public static ScriptStatusType DetermineScriptRunStatus(IDatabaseUtility dbUtil, Script script, ConnectionData connData, string projectFilePath, bool checkForChanges, List<DatabaseOverride> overrides, out DateTime commitDate, out DateTime serverChangeDate)
        {
            string targetDatabase = ConnectionHelper.GetTargetDatabase(script.Database ?? string.Empty, overrides);

            string scriptIdStr = script.ScriptId ?? string.Empty;
            if (!Guid.TryParse(scriptIdStr, out Guid scriptGuid))
                scriptGuid = Guid.Empty;

            bool preRun = (dbUtil.HasBlockingSqlLog(scriptGuid, connData, targetDatabase, out string scriptHash, out string scriptTextHash, out commitDate) == true);

            return DetermineScriptRunStatusCore(
                script, connData, projectFilePath, checkForChanges, overrides, targetDatabase,
                preRun, scriptHash, scriptTextHash, commitDate,
                out commitDate, out serverChangeDate);
        }

        /// <summary>
        /// Core status determination logic using pre-fetched DB values.
        /// Called by both <see cref="DetermineScriptRunStatus"/> (single-script path)
        /// and <see cref="SetScriptRunStatusAndDates"/> (set-based batch path).
        /// </summary>
        internal static ScriptStatusType DetermineScriptRunStatusCore(
            Script script, ConnectionData connData, string projectFilePath, bool checkForChanges,
            List<DatabaseOverride> overrides, string targetDatabase,
            bool preRun, string scriptHash, string scriptTextHash, DateTime dbCommitDate,
            out DateTime commitDate, out DateTime serverChangeDate)
        {
            commitDate = dbCommitDate;
            serverChangeDate = DateTime.MinValue;

            //Update the routine (sp and functions) change date cache
            if (DatabaseObjectChangeDates.Servers[connData.SQLServerName][targetDatabase].LastRefreshTime < DateTime.Now.AddSeconds(-30))
            {
                InfoHelper.UpdateRoutineAndViewChangeDates(connData, overrides);
            }

            bool hashChanged = false;
            string fileName = script.FileName ?? string.Empty;

            if (!File.Exists(Path.Combine(projectFilePath, fileName)))
            {
                return ScriptStatusType.FileMissing;
            }

            if (preRun && checkForChanges)
            {
                // PERF-003: The first call already returned all available hash data.
                // A second identical call cannot produce different results and is unconditionally removed.

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

        /// <summary>
        /// PERF-003: Sets run status for all scripts using one set-based DB query per target database.
        /// Groups scripts by resolved target database, calls <see cref="IDatabaseUtility.GetBatchBlockingSqlLog"/>
        /// once per database (the implementation handles chunking within param limits), then applies status
        /// from the cache. <see cref="IDatabaseUtility.HasBlockingSqlLog"/> is never called.
        /// </summary>
        public static void SetScriptRunStatusAndDates(SqlSyncBuildDataModel model, IDatabaseUtility dbUtil, ConnectionData connData, string projectFilePath)
        {
            var overrides = OverrideData.TargetDatabaseOverrides?.ToList() ?? new List<DatabaseOverride>();

            // Group scripts by their resolved target database
            var scriptsByDb = new Dictionary<string, List<(Script script, Guid guid)>>(StringComparer.OrdinalIgnoreCase);
            foreach (Script script in model.Script)
            {
                string targetDb = ConnectionHelper.GetTargetDatabase(script.Database ?? string.Empty, overrides);
                Guid.TryParse(script.ScriptId ?? string.Empty, out Guid scriptGuid);
                if (!scriptsByDb.TryGetValue(targetDb, out var list))
                    scriptsByDb[targetDb] = list = new List<(Script, Guid)>();
                list.Add((script, scriptGuid));
            }

            // PERF-003: One set-based query per target database instead of N per-script HasBlockingSqlLog calls
            var statusByDatabase =
                new Dictionary<string, IReadOnlyDictionary<Guid, SqlLogStatus>>(StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in scriptsByDb)
            {
                var ids = kvp.Value
                    .Select(x => x.guid)
                    .Where(g => g != Guid.Empty)
                    .Distinct()
                    .ToList();
                if (ids.Count == 0) continue;

                statusByDatabase[kvp.Key] = dbUtil.GetBatchBlockingSqlLog(ids, connData, kvp.Key);
            }

            // Apply cached status to each script
            foreach (Script script in model.Script)
            {
                string targetDb = ConnectionHelper.GetTargetDatabase(script.Database ?? string.Empty, overrides);
                Guid.TryParse(script.ScriptId ?? string.Empty, out Guid scriptGuid);

                bool preRun;
                string scriptHash;
                string scriptTextHash;
                DateTime dbCommitDate;

                if (scriptGuid != Guid.Empty &&
                    statusByDatabase.TryGetValue(targetDb, out var databaseStatuses) &&
                    databaseStatuses.TryGetValue(scriptGuid, out SqlLogStatus cached))
                {
                    preRun = cached.HasBlock;
                    scriptHash = cached.ScriptHash;
                    scriptTextHash = cached.ScriptTextHash;
                    dbCommitDate = cached.CommitDate;
                }
                else
                {
                    preRun = false;
                    scriptHash = string.Empty;
                    scriptTextHash = string.Empty;
                    dbCommitDate = DateTime.MinValue;
                }

                script.ScriptRunStatus = DetermineScriptRunStatusCore(
                    script, connData, projectFilePath, true, overrides, targetDb,
                    preRun, scriptHash, scriptTextHash, dbCommitDate,
                    out DateTime commitDate, out DateTime serverChangeDate);
                script.LastCommitDate = commitDate;
                script.ServerChangeDate = serverChangeDate;
            }
        }
    }
}
