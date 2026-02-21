using SqlSync.Connection;
using SqlSync.SqlBuild.MultiDb;
using SqlSync.SqlBuild.SqlLogging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlSync.SqlBuild.Models
{
    /// <summary>
    /// Contains all mutable state for a build execution.
    /// Consolidates fields that were previously spread across SqlBuildHelper.
    /// </summary>
    public class BuildExecutionState
    {
        #region Core Build State

        /// <summary>Whether the build runs in a transaction.</summary>
        public bool IsTransactional { get; set; } = true;

        /// <summary>Whether this is a trial (dry-run) build.</summary>
        public bool IsTrialBuild { get; set; } = false;

        /// <summary>Whether to run script only without committing.</summary>
        public bool RunScriptOnly { get; set; } = false;

        /// <summary>Database override mappings.</summary>
        public List<DatabaseOverride> TargetDatabaseOverrides { get; set; }

        /// <summary>The current build data model.</summary>
        public SqlSyncBuildDataModel BuildDataModel { get; set; } = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

        /// <summary>Hash of the build package for integrity verification.</summary>
        public string BuildPackageHash { get; set; } = string.Empty;

        #endregion

        #region Build Configuration

        /// <summary>Type/category of the build.</summary>
        public string BuildType { get; set; } = string.Empty;

        /// <summary>Human-readable description of the build.</summary>
        public string BuildDescription { get; set; } = string.Empty;

        /// <summary>Starting index for script execution.</summary>
        public double StartIndex { get; set; } = 0;

        /// <summary>Path to the project file.</summary>
        public string ProjectFileName { get; set; } = string.Empty;

        /// <summary>Directory containing the project file.</summary>
        public string ProjectFilePath { get; set; } = string.Empty;

        /// <summary>Name of the build file (sbm/sbx).</summary>
        public string BuildFileName { get; set; } = string.Empty;

        /// <summary>Path to the script execution log file.</summary>
        public string ScriptLogFileName { get; set; }

        /// <summary>Path to external log file for copying results.</summary>
        public string ExternalScriptLogFileName { get; set; } = string.Empty;

        /// <summary>Path to build history XML file.</summary>
        public string BuildHistoryXmlFile { get; set; } = string.Empty;

        /// <summary>Database name for logging build results.</summary>
        public string LogToDatabaseName { get; set; } = string.Empty;

        /// <summary>User who requested the build.</summary>
        public string BuildRequestedBy { get; set; } = string.Empty;

        /// <summary>Specific script indexes to run (empty = all).</summary>
        public double[] RunItemIndexes { get; set; } = Array.Empty<double>();

        /// <summary>Whether an error occurred during the build.</summary>
        public bool ErrorOccurred { get; set; } = false;

        #endregion

        #region Runtime State

        /// <summary>Multi-database run data when running across multiple databases.</summary>
        public MultiDbData MultiDbRunData { get; set; }

        /// <summary>Accumulated SQL informational messages.</summary>
        public string SqlInfoMessage { get; set; } = string.Empty;

        /// <summary>Last SQL message received.</summary>
        public string LastSqlMessage { get; set; } = string.Empty;

        /// <summary>Current build's unique identifier.</summary>
        public Guid CurrentBuildId { get; set; } = Guid.Empty;

        /// <summary>Scripts that have been committed during this build.</summary>
        public List<SqlLogging.CommittedScript> CommittedScripts { get; } = new List<SqlLogging.CommittedScript>();

        #endregion

        /// <summary>
        /// Resets the state for a new build execution.
        /// </summary>
        public void Reset()
        {
            IsTransactional = true;
            IsTrialBuild = false;
            RunScriptOnly = false;
            TargetDatabaseOverrides = null;
            BuildDataModel = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            BuildPackageHash = string.Empty;
            BuildType = string.Empty;
            BuildDescription = string.Empty;
            StartIndex = 0;
            ProjectFileName = string.Empty;
            ProjectFilePath = string.Empty;
            BuildFileName = string.Empty;
            ScriptLogFileName = null;
            ExternalScriptLogFileName = string.Empty;
            BuildHistoryXmlFile = string.Empty;
            LogToDatabaseName = string.Empty;
            BuildRequestedBy = string.Empty;
            RunItemIndexes = Array.Empty<double>();
            ErrorOccurred = false;
            MultiDbRunData = null;
            SqlInfoMessage = string.Empty;
            LastSqlMessage = string.Empty;
            CurrentBuildId = Guid.Empty;
            CommittedScripts.Clear();
        }

        /// <summary>
        /// Populates state from a SqlBuildRunDataModel.
        /// </summary>
        public void PopulateFromRunData(SqlBuildRunDataModel runData)
        {
            if (runData == null) return;

            BuildDataModel = runData.BuildDataModel ?? SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            BuildType = runData.BuildType ?? string.Empty;
            BuildDescription = runData.BuildDescription ?? string.Empty;
            StartIndex = runData.StartIndex ?? 0;
            ProjectFileName = runData.ProjectFileName ?? string.Empty;
            if (!string.IsNullOrWhiteSpace(runData.ProjectFileName))
            {
                int lastSep = Math.Max(runData.ProjectFileName.LastIndexOf('/'), runData.ProjectFileName.LastIndexOf('\\'));
                ProjectFilePath = lastSep >= 0 ? runData.ProjectFileName.Substring(0, lastSep) : string.Empty;
            }
            else
            {
                ProjectFilePath = string.Empty;
            }
            IsTrialBuild = runData.IsTrial ?? false;
            RunItemIndexes = runData.RunItemIndexes?.ToArray() ?? Array.Empty<double>();
            RunScriptOnly = runData.RunScriptOnly ?? false;
            var buildFile = runData.BuildFileName ?? string.Empty;
            int buildLastSep = Math.Max(buildFile.LastIndexOf('/'), buildFile.LastIndexOf('\\'));
            BuildFileName = buildLastSep >= 0 ? buildFile.Substring(buildLastSep + 1) : buildFile;
            TargetDatabaseOverrides = runData.TargetDatabaseOverrides?.ToList();
            LogToDatabaseName = runData.LogToDatabaseName ?? string.Empty;
            IsTransactional = runData.IsTransactional ?? true;
        }
    }
}
