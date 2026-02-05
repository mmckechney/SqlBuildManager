using Microsoft.Extensions.Logging;
using SqlBuildManager.Interfaces.Console;
using SqlSync.Connection;
using SqlSync.SqlBuild.Models;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

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
        /// Attempts to execute a DacPac-based build recovery asynchronously.
        /// </summary>
        Task<DacPacFallbackResult> TryDacPacFallbackAsync(
            DacPacFallbackContext context,
            Build buildResult,
            CancellationToken cancellationToken = default);
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
        /// Async callback to recursively execute a build with updated run data.
        /// </summary>
        public Func<SqlBuildRunDataModel, int, string, ScriptBatchCollection, CancellationToken, Task<Build>> ProcessBuildCallbackAsync { get; set; }
        
        /// <summary>
        /// Callback to get the target database with overrides.
        /// </summary>
        public Func<string, string> GetTargetDatabaseCallback { get; set; }
        
        /// <summary>
        /// Event to raise when build is committed.
        /// </summary>
        public Action<RunnerReturn> RaiseBuildCommittedEvent { get; set; }
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
