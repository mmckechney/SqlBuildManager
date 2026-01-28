using SqlBuildManager.Interfaces.Console;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Services;
using System.Collections.Generic;
using System.ComponentModel;

namespace SqlSync.SqlBuild.Services
{
    public interface IBuildFinalizer
    {
        
        bool CommitBuild(IConnectionsService connectionsService, bool isTransactional);
        bool RollbackBuild(IConnectionsService connectionsService, bool isTransactional);
        void SaveBuildDataModel(ISqlBuildRunnerProperties context, bool finalSave);
        public (Build updatedBuild, SqlSyncBuildDataModel updatedModel, BuildResultStatus buildResult) PerformRunScriptFinalization(ISqlBuildRunnerProperties context, IConnectionsService connectionsService, IBuildFinalizerContext finalizerContext, bool buildFailure, Build myBuild);
        public SqlSyncBuildDataModel RecordCommittedScripts(List<SqlSync.SqlBuild.SqlLogging.CommittedScript> committedScripts, SqlSyncBuildDataModel buildDataModel);
        public BuildResultStatus CalculateFinalStatus(IList<BuildResultStatus> buildResults);
        // Event invocation helpers

        //Build PerformRunScriptFinalization(
        //    IBuildFinalizerContext context,
        //     ISqlLoggingService sqlLoggingService,
        //    bool buildFailure, 
        //    Build myBuild, 
        //    SqlSyncBuildDataModel buildDataModel, 
        //    ref DoWorkEventArgs workEventArgs);
    }
}
