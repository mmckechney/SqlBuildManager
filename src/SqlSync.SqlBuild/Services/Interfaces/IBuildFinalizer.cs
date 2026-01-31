using SqlBuildManager.Interfaces.Console;
using SqlSync.SqlBuild.Models;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading.Tasks;

namespace SqlSync.SqlBuild.Services
{
    public interface IBuildFinalizer
    {
        
        bool CommitBuild(IConnectionsService connectionsService, bool isTransactional);
        bool RollbackBuild(IConnectionsService connectionsService, bool isTransactional);
        void SaveBuildDataModel(ISqlBuildRunnerProperties context, bool finalSave);
        public Task<(Build updatedBuild, SqlSyncBuildDataModel updatedModel, BuildResultStatus buildResult)> PerformRunScriptFinalizationAsync(ISqlBuildRunnerProperties context, IConnectionsService connectionsService, IBuildFinalizerContext finalizerContext, bool buildFailure, Build myBuild);
        public SqlSyncBuildDataModel RecordCommittedScripts(List<SqlSync.SqlBuild.SqlLogging.CommittedScript> committedScripts, SqlSyncBuildDataModel buildDataModel);
        public BuildResultStatus CalculateFinalStatus(IList<BuildResultStatus> buildResults);
    }
}
