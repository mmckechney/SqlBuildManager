using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using SqlSync.Connection;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.MultiDb;
using SqlSync.SqlBuild.Services;
using SqlSync.SqlBuild.SqlLogging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace SqlSync.SqlBuild.UnitTest
{

    public sealed class NoopProgressReporter : IProgressReporter
    {
        public bool CancellationPending => false;
        public void ReportProgress(int percent, object userState) { }
    }

    public sealed class FakeRunnerContext : FakeRunnerProperties, ISqlBuildRunnerContext, ISqlBuildRunnerProperties
    {
        private readonly BackgroundWorker _bgWorker;
        
        public FakeRunnerContext() => _bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
        public FakeRunnerContext(BackgroundWorker bgWorker) => _bgWorker = bgWorker;
        
        public string[] ReadBatchReturn { get; set; } = Array.Empty<string>();
        public bool IsTransactionalValue { get; set; } = true;

        public override bool IsTransactional => IsTransactionalValue;

        public Microsoft.Extensions.Logging.ILogger Log => NullLogger.Instance;
        public BackgroundWorker BgWorker => _bgWorker;
        public IProgressReporter ProgressReporter => new NoopProgressReporter();
        
        public string GetTargetDatabase(string defaultDatabase) => defaultDatabase;
        public Task<string[]> ReadBatchFromScriptFileAsync(string path, bool stripTransaction, bool useRegex, CancellationToken cancellationToken = default) => Task.FromResult(ReadBatchReturn);
        public string PerformScriptTokenReplacement(string script) => script;
        public Task<string> PerformScriptTokenReplacementAsync(string script, CancellationToken cancellationToken = default) => Task.FromResult(script);
        public void AddScriptRunToHistory(ScriptRun run, Build myBuild) { }
        public void RollbackBuild() { }
        public void SaveBuildDataSet(bool fireSavedEvent) { }
        public void PublishScriptLog(bool isError, ScriptLogEventArgs args) { }
    }

    public class FakeRunnerProperties : ISqlBuildRunnerProperties
    {
        private readonly string _projectFilePath;

        public FakeRunnerProperties()
        {
            _projectFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "FakeProject");
            if (!System.IO.Directory.Exists(_projectFilePath))
                System.IO.Directory.CreateDirectory(_projectFilePath);
        }

        public virtual bool IsTransactional => true;

        public bool IsTrialBuild => false;

        public bool RunScriptOnly => false;

        public string BuildPackageHash => "FAKEHASH-WERWEFWEFHWHFWRHC342JWIOEJWIECUJWEUFWIJFWOIEJCWIECJWE";

        public string ProjectFilePath => _projectFilePath;

        public string ProjectFileName => "FakeProjectName.xml";

        public string BuildFileName => "FakeBuildFileName.sbm";

        public List<SqlSync.SqlBuild.SqlLogging.CommittedScript> CommittedScripts => new();

        public bool ErrorOccured { get => false; set => value = false; }
        public string SqlInfoMessage { get => string.Empty; set => value = string.Empty; }

        public int DefaultScriptTimeout => 30;

        public SqlSyncBuildDataModel BuildDataModel {get => SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel(); set => _ = value; }
        public SqlSyncBuildDataModel BuildHistoryModel { get => SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel(); set => _ = value; }

        public MultiDbData MultiDbRunData => new MultiDbData();

        public string BuildRequestedBy => "FakeUser";

        public string BuildDescription => "FakeBuildDescription";

        public string LogToDatabaseName { get => string.Empty; set => value = string.Empty; }

        public string BuildHistoryXmlFile => "FakeBuildHistory.xml";

        public ConnectionData ConnectionData => new();
        public List<DatabaseOverride> TargetDatabaseOverrides => new();

        public event ScriptLogWriteEventHandler ScriptLogWriteEvent;
        public event BuildCommittedEventHandler BuildCommittedEvent;
        public event EventHandler BuildSuccessTrialRolledBackEvent;
        public event EventHandler BuildErrorRollBackEvent;
    }


    public sealed class LocalFakeRunnerContext : ISqlBuildRunnerContext
    {
        private static readonly IProgressReporter NoopProgress = new NoopProgressReporter();
        private readonly BackgroundWorker _bg = new BackgroundWorker { WorkerReportsProgress = true };
        private readonly string _projectFilePath;

        public event ScriptLogWriteEventHandler ScriptLogWriteEvent;
        public event BuildCommittedEventHandler BuildCommittedEvent;
        public event EventHandler BuildSuccessTrialRolledBackEvent;
        public event EventHandler BuildErrorRollBackEvent;

        public LocalFakeRunnerContext()
        {
            _projectFilePath = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "LocalFakeProject");
            if (!System.IO.Directory.Exists(_projectFilePath))
                System.IO.Directory.CreateDirectory(_projectFilePath);
        }

        public Microsoft.Extensions.Logging.ILogger Log => Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance;
        public BackgroundWorker BgWorker => _bg;
        public IProgressReporter ProgressReporter => NoopProgress;
        public bool IsTransactional => false;
        public bool IsTrialBuild => false;
        public bool RunScriptOnly => false;
        public string BuildPackageHash => string.Empty;
        public string ProjectFilePath => _projectFilePath;
        public string ProjectFileName => "LocalFakeProject.xml";
        public string BuildFileName => "LocalFakeBuild.sbm";
        public List<SqlSync.SqlBuild.SqlLogging.CommittedScript> CommittedScripts { get; } = new();
        public bool ErrorOccured { get; set; }
        public string SqlInfoMessage { get; set; } = string.Empty;
        public int DefaultScriptTimeout => 30;
        public SqlSyncBuildDataModel BuildDataModel { get => SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel(); set => _ = value; }
        public SqlSyncBuildDataModel BuildHistoryModel { get => SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel(); set => _ = value; }
        public MultiDbData MultiDbRunData => new MultiDbData();
        public string BuildRequestedBy => "FakeUser";
        public string BuildDescription => "FakeBuildDescription";
        public string LogToDatabaseName { get => string.Empty; set => value = string.Empty; }
        public string BuildHistoryXmlFile => "FakeBuildHistory.xml";
        public ConnectionData ConnectionData => new();

        public string GetTargetDatabase(string defaultDatabase) => defaultDatabase;
        public Task<string[]> ReadBatchFromScriptFileAsync(string path, bool stripTransaction, bool useRegex, CancellationToken cancellationToken = default) => Task.FromResult(new[] { "SELECT 1" });
        public string PerformScriptTokenReplacement(string script) => script;
        public Task<string> PerformScriptTokenReplacementAsync(string script, CancellationToken cancellationToken = default) => Task.FromResult(script);
        public void AddScriptRunToHistory(ScriptRun run, Build myBuild) { }
        public void RollbackBuild() { }
        public void SaveBuildDataSet(bool fireSavedEvent) { }
        public void PublishScriptLog(bool isError, ScriptLogEventArgs args) { }
        public List<DatabaseOverride> TargetDatabaseOverrides => new();
    }

    /// <summary>
    /// Helper class to create Mock instances of common services
    /// </summary>
    internal static class MockFactory
    {
        public static Mock<IConnectionsService> CreateMockConnectionsService()
        {
            var mock = new Mock<IConnectionsService>();
            mock.Setup(x => x.GetBuildConnectionDataClass(It.IsAny<string>(), It.IsAny<string>(),It.IsAny<bool>()))
                .Returns((string server, string database, bool isTransactional) => new BuildConnectData
                {
                    ServerName = server,
                    DatabaseName = database
                });
            mock.Setup(x => x.GetOrAddBuildConnectionDataClass(It.IsAny<ConnectionData>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>()))
                .Returns((ConnectionData connData, string server, string database, bool isTransactional) => new BuildConnectData
                {
                    ServerName = server,
                    DatabaseName = database
                });
            mock.Setup(x => x.Connections).Returns(new Dictionary<string, BuildConnectData>());
            return mock;
        }

        public static Mock<ISqlBuildRunnerContext> CreateMockRunnerContext()
        {
            var mock = new Mock<ISqlBuildRunnerContext>();
            mock.Setup(x => x.Log).Returns(NullLogger.Instance);
            mock.Setup(x => x.ProgressReporter).Returns(new NoopProgressReporter());
            mock.Setup(x => x.IsTransactional).Returns(true);
            mock.Setup(x => x.IsTrialBuild).Returns(false);
            mock.Setup(x => x.RunScriptOnly).Returns(false);
            mock.Setup(x => x.BuildPackageHash).Returns("MOCK_HASH");
            mock.Setup(x => x.ProjectFilePath).Returns("MOCK_PATH");
            mock.Setup(x => x.ProjectFileName).Returns("MockProject.xml");
            mock.Setup(x => x.BuildFileName).Returns("MockBuild.sbm");
            mock.Setup(x => x.CommittedScripts).Returns(new List<SqlSync.SqlBuild.SqlLogging.CommittedScript>());
            mock.Setup(x => x.DefaultScriptTimeout).Returns(30);
            mock.Setup(x => x.BuildDataModel).Returns(SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel());
            mock.Setup(x => x.MultiDbRunData).Returns(new MultiDbData());
            mock.Setup(x => x.BuildRequestedBy).Returns("MockUser");
            mock.Setup(x => x.BuildDescription).Returns("Mock Build");
            mock.Setup(x => x.LogToDatabaseName).Returns(string.Empty);
            mock.Setup(x => x.BuildHistoryXmlFile).Returns("MockHistory.xml");
            mock.Setup(x => x.ConnectionData).Returns(new ConnectionData());
            mock.Setup(x => x.GetTargetDatabase(It.IsAny<string>())).Returns((string db) => db);
            mock.Setup(x => x.ReadBatchFromScriptFileAsync(It.IsAny<string>(), It.IsAny<bool>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new[] { "SELECT 1" });
            mock.Setup(x => x.PerformScriptTokenReplacement(It.IsAny<string>())).Returns((string s) => s);
            mock.Setup(x => x.PerformScriptTokenReplacementAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .Returns((string s, CancellationToken ct) => Task.FromResult(s));
            
            return mock;
        }

        public static Mock<ISqlCommandExecutor> CreateMockSqlExecutor()
        {
            var mock = new Mock<ISqlCommandExecutor>();
            mock.Setup(x => x.Execute(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<BuildConnectData>(), It.IsAny<bool>()))
                .Returns(new SqlExecutionResult(true, string.Empty));
            mock.Setup(x => x.ExecuteAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<BuildConnectData>(), It.IsAny<bool>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(new SqlExecutionResult(true, string.Empty));
            return mock;
        }

        public static Mock<ISqlBuildFileHelper> CreateMockFileHelper()
        {
            var mock = new Mock<ISqlBuildFileHelper>();
            return mock;
        }

        public static Mock<IBuildFinalizer> CreateMockBuildFinalizer()
        {
            var mock = new Mock<IBuildFinalizer>();
            mock.Setup(x => x.PerformRunScriptFinalization(
                    It.IsAny<ISqlBuildRunnerProperties>(),
                    It.IsAny<IConnectionsService>(),
                    It.IsAny<IBuildFinalizerContext>(),
                    It.IsAny<bool>(),
                    It.IsAny<Build>()))
                .Returns((ISqlBuildRunnerProperties ctx, IConnectionsService conn, IBuildFinalizerContext finalizerContext, bool buildFailure, Build b) =>
                {
                    // Mimic the real DefaultBuildFinalizer behavior
                    Build finalBuild;
                    BuildResultStatus result;
                    
                    if (buildFailure)
                    {
                        // Set FinalStatus based on transaction state
                        var status = ctx.IsTransactional ? BuildItemStatus.RolledBack : BuildItemStatus.FailedNoTransaction;
                        finalBuild = new Build(
                            name: b.Name,
                            buildType: b.BuildType,
                            buildStart: b.BuildStart,
                            buildEnd: b.BuildEnd,
                            serverName: b.ServerName,
                            finalStatus: status,
                            buildId: b.BuildId,
                            userId: b.UserId);
                        result = ctx.IsTransactional ? BuildResultStatus.BUILD_FAILED_AND_ROLLED_BACK : BuildResultStatus.BUILD_FAILED_NO_TRANSACTION;
                    }
                    else
                    {
                        finalBuild = new Build(
                            name: b.Name,
                            buildType: b.BuildType,
                            buildStart: b.BuildStart,
                            buildEnd: b.BuildEnd,
                            serverName: b.ServerName,
                            finalStatus: BuildItemStatus.Committed,
                            buildId: b.BuildId,
                            userId: b.UserId);
                        result = BuildResultStatus.BUILD_COMMITTED;
                    }
                    
                    return (finalBuild, ctx.BuildDataModel, result);
                });
            return mock;
        }

        public static Mock<ISqlLoggingService> CreateMockSqlLoggingService()
        {
            var mock = new Mock<ISqlLoggingService>();
            return mock;
        }
    }
}
