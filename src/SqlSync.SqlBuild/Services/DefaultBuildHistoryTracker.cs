using SqlSync.SqlBuild.Models;
using System.Linq;

namespace SqlSync.SqlBuild.Services
{
    /// <summary>
    /// Default implementation of IBuildHistoryTracker that maintains build history in memory.
    /// </summary>
    internal sealed class DefaultBuildHistoryTracker : IBuildHistoryTracker
    {
        private SqlSyncBuildDataModel _buildHistoryModel;

        public DefaultBuildHistoryTracker()
        {
            _buildHistoryModel = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
        }

        public SqlSyncBuildDataModel BuildHistoryModel => _buildHistoryModel;

        public void AddScriptRunToHistory(ScriptRun run, Build build)
        {
            if (run == null) return;

            // Add the script run
            _buildHistoryModel = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: _buildHistoryModel.SqlSyncBuildProject,
                script: _buildHistoryModel.Script,
                build: _buildHistoryModel.Build,
                scriptRun: _buildHistoryModel.ScriptRun.Concat(new[] { run }).ToList(),
                committedScript: _buildHistoryModel.CommittedScript,
                codeReview: _buildHistoryModel.CodeReview);

            // Add the build if not already present
            if (!_buildHistoryModel.Build.Contains(build))
            {
                _buildHistoryModel = new SqlSyncBuildDataModel(
                    sqlSyncBuildProject: _buildHistoryModel.SqlSyncBuildProject,
                    script: _buildHistoryModel.Script,
                    build: _buildHistoryModel.Build.Concat(new[] { build }).ToList(),
                    scriptRun: _buildHistoryModel.ScriptRun,
                    committedScript: _buildHistoryModel.CommittedScript,
                    codeReview: _buildHistoryModel.CodeReview);
            }
        }

        public void Reset()
        {
            _buildHistoryModel = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
        }
    }
}
