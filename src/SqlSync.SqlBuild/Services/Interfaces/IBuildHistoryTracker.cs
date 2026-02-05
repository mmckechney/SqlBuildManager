using SqlSync.SqlBuild.Models;

namespace SqlSync.SqlBuild.Services
{
    /// <summary>
    /// Tracks build history by recording script runs and builds.
    /// </summary>
    internal interface IBuildHistoryTracker
    {
        /// <summary>
        /// Gets the current build history model.
        /// </summary>
        SqlSyncBuildDataModel BuildHistoryModel { get; }

        /// <summary>
        /// Adds a script run to the build history.
        /// </summary>
        void AddScriptRunToHistory(ScriptRun run, Build build);

        /// <summary>
        /// Resets the build history model to a clean state.
        /// </summary>
        void Reset();
    }
}
