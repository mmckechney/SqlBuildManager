using Microsoft.Extensions.Logging;
using SqlBuildManager.Interfaces.Console;
using SqlSync.Connection;
using SqlSync.SqlBuild.Models;
using System.IO;

namespace SqlSync.SqlBuild.Services
{
    /// <summary>
    /// Handles DacPac fallback logic when a build fails and needs to sync via DACPAC.
    /// </summary>
    internal interface IDacPacFallbackHandler
    {
        /// <summary>
        /// Determines if a build result is a candidate for custom DacPac fallback.
        /// </summary>
        bool IsCandidateForDacPacFallback(BuildItemStatus status);

        /// <summary>
        /// Attempts to execute a DacPac-based build recovery.
        /// </summary>
        DacPacFallbackResult TryDacPacFallback(
            DacPacFallbackContext context,
            Build buildResult);
    }

    /// <summary>
    /// Context information needed for DacPac fallback execution.
    /// </summary>
    internal sealed class DacPacFallbackContext
    {
        public SqlBuildRunDataModel RunData { get; set; }
        public BuildPreparationResult Prep { get; set; }
        public string ServerName { get; set; }
        public bool IsMultiDbRun { get; set; }
        public ScriptBatchCollection ScriptBatchColl { get; set; }
        public int AllowableTimeoutRetries { get; set; }
        public ConnectionData ConnectionData { get; set; }
        public string ProjectFilePath { get; set; }
        
        /// <summary>
        /// Callback to recursively execute a build with updated run data.
        /// </summary>
        public System.Func<SqlBuildRunDataModel, string, bool, ScriptBatchCollection, int, Build> ProcessBuildCallback { get; set; }
        
        /// <summary>
        /// Callback to get the target database with overrides.
        /// </summary>
        public System.Func<string, string> GetTargetDatabaseCallback { get; set; }
        
        /// <summary>
        /// Event to raise when build is committed.
        /// </summary>
        public System.Action<RunnerReturn> RaiseBuildCommittedEvent { get; set; }
    }

    /// <summary>
    /// Result of a DacPac fallback attempt.
    /// </summary>
    internal sealed class DacPacFallbackResult
    {
        public bool WasAttempted { get; set; }
        public BuildItemStatus? NewStatus { get; set; }
    }
}
