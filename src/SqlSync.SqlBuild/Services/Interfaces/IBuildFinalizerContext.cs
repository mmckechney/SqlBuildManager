using System;
using System.Collections.Generic;
using System.ComponentModel;
using SqlSync.SqlBuild.MultiDb;
using SqlBuildManager.Interfaces.Console;

namespace SqlSync.SqlBuild.Services
{
    /// <summary>
    /// Provides the context needed by IBuildFinalizer to perform finalization operations.
    /// </summary>
    public interface IBuildFinalizerContext
    {
        bool IsTransactional { get; }
        bool IsTrialBuild { get; }
        bool RunScriptOnly { get; }
       
        List<SqlLogging.CommittedScript> CommittedScripts { get; }
        // Event invocation helpers
        void RaiseBuildCommittedEvent(object sender, RunnerReturn rr);
        void RaiseBuildSuccessTrialRolledBackEvent(object sender);
        void RaiseBuildErrorRollBackEvent(object sender);
    }
}
