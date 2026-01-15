using System;
using System.Collections.Generic;
using System.ComponentModel;
using LoggingCommittedScript = SqlSync.SqlBuild.SqlLogging.CommittedScript;
using SqlSync.SqlBuild.MultiDb;
using SqlBuildManager.Interfaces.Console;

namespace SqlSync.SqlBuild
{
    /// <summary>
    /// Provides the context needed by IBuildFinalizer to perform finalization operations.
    /// </summary>
    public interface IBuildFinalizerContext
    {
        bool IsTransactional { get; }
        bool IsTrialBuild { get; }
        bool RunScriptOnly { get; }
        BackgroundWorker BgWorker { get; }

        bool CommitBuild();
        void RollbackBuild();
        void SaveBuildDataSet(bool finalSave);
        bool RecordCommittedScripts(List<LoggingCommittedScript> committedScripts, Models.SqlSyncBuildDataModel buildDataModel, out Models.SqlSyncBuildDataModel updatedModel);
        
        List<LoggingCommittedScript> CommittedScripts { get; }
        void SetErrorOccurred(bool value);
        
        // Event invocation helpers
        void RaiseBuildCommittedEvent(object sender, RunnerReturn rr);
        void RaiseBuildSuccessTrialRolledBackEvent(object sender);
        void RaiseBuildErrorRollBackEvent(object sender);
    }
}
