using SqlSync.SqlBuild;
using SqlSync.SqlBuild.Models;
using System;

namespace SqlBuildManager.Console.Threaded
{
    /// <summary>
    /// Contains shared execution state for a threaded build run.
    /// Replaces static fields that were previously in ThreadedManager.
    /// </summary>
    public class BuildExecutionContext
    {
        private string runId = string.Empty;

        /// <summary>
        /// Unique identifier for the run.
        /// Auto-generated on first access if not set.
        /// </summary>
        public string RunId
        {
            get
            {
                if (string.IsNullOrEmpty(runId))
                {
                    runId = Guid.NewGuid().ToString("N");
                }
                return runId;
            }
            set => runId = value;
        }

        /// <summary>
        /// Path and file name to the XML metadata configuration project file (SqlSyncBuildProject.xml)
        /// </summary>
        public string ProjectFileName { get; set; } = string.Empty;

        /// <summary>
        /// The name of the zipped build file (.sbm)
        /// </summary>
        public string BuildZipFileName { get; set; } = string.Empty;

        /// <summary>
        /// The platinum dacpac file name for schema comparison
        /// </summary>
        public string PlatinumDacPacFileName { get; set; } = string.Empty;

        /// <summary>
        /// The root folder where logging files are written
        /// </summary>
        public string RootLoggingPath { get; set; } = string.Empty;

        /// <summary>
        /// The working directory for extracted build files
        /// </summary>
        public string WorkingDirectory { get; set; } = string.Empty;

        /// <summary>
        /// The pre-batched set of scripts to be run
        /// </summary>
        public ScriptBatchCollection BatchCollection { get; set; } = null!;

        /// <summary>
        /// The build data model containing script metadata
        /// </summary>
        public SqlSyncBuildDataModel BuildDataModel { get; set; } = null!;

        /// <summary>
        /// Resets all context values for a new run.
        /// </summary>
        public void Reset()
        {
            runId = string.Empty;
            ProjectFileName = string.Empty;
            BuildZipFileName = string.Empty;
            PlatinumDacPacFileName = string.Empty;
            RootLoggingPath = string.Empty;
            WorkingDirectory = string.Empty;
            BatchCollection = null!;
            BuildDataModel = null!;
        }
    }
}
