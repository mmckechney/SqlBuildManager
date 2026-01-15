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
        (List<Build> buildRecords, SqlSyncBuildDataModel updatedModel, bool errorOccurred) PerformRunScriptFinalization(ISqlBuildRunnerProperties context, IConnectionsService connectionsService, bool buildFailure, List<Build> myBuild, SqlSyncBuildDataModel buildDataModel, ref DoWorkEventArgs workEventArgs)
        public SqlSyncBuildDataModel RecordCommittedScripts(List<SqlSync.SqlBuild.SqlLogging.CommittedScript> committedScripts, SqlSyncBuildDataModel buildDataModel);

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
