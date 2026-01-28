using Microsoft.Extensions.Logging;
using SqlSync.Connection;
using SqlSync.SqlBuild.MultiDb;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using BuildModels = SqlSync.SqlBuild.Models;

namespace SqlSync.SqlBuild
{
    public interface ISqlBuildRunnerContext : ISqlBuildRunnerProperties
    {
        ILogger Log { get; }
        BackgroundWorker BgWorker { get; }
        IProgressReporter ProgressReporter { get; }


        string GetTargetDatabase(string defaultDatabase);
        Task<string[]> ReadBatchFromScriptFileAsync(string path, bool stripTransaction, bool useRegex, CancellationToken cancellationToken = default);
        string PerformScriptTokenReplacement(string script);
        Task<string> PerformScriptTokenReplacementAsync(string script, CancellationToken cancellationToken = default);
        void AddScriptRunToHistory(BuildModels.ScriptRun run, BuildModels.Build myBuild);
        void SaveBuildDataSet(bool fireSavedEvent);
        void PublishScriptLog(bool isError, ScriptLogEventArgs args);
    }


    public interface ISqlBuildRunnerProperties
    {
        bool IsTransactional { get; }
        bool IsTrialBuild { get; }
        bool RunScriptOnly { get; }
        string BuildPackageHash { get; }
        string ProjectFilePath { get; }
        string ProjectFileName { get; }
        string BuildFileName { get; }
        List<SqlSync.SqlBuild.SqlLogging.CommittedScript> CommittedScripts { get; }
        bool ErrorOccured { get; set; }
        string SqlInfoMessage { get; set; }
        int DefaultScriptTimeout { get; }
        BuildModels.SqlSyncBuildDataModel BuildDataModel { get; }
        MultiDbData MultiDbRunData { get; }

        string BuildRequestedBy { get; }
        string BuildDescription { get; }
        string LogToDataBaseName { get; }
        string BuildHistoryXmlFile { get; }
        ConnectionData ConnectionData { get; }

        event ScriptLogWriteEventHandler ScriptLogWriteEvent;
        event BuildCommittedEventHandler BuildCommittedEvent;
        event EventHandler BuildSuccessTrialRolledBackEvent;
        event EventHandler BuildErrorRollBackEvent;
    }
}
