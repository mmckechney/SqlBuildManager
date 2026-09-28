using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.Threaded;
using SqlBuildManager.Connection;
using SqlBuildManager.SqlBuild;
using SqlBuildManager.SqlBuild.Models;
using SqlBuildManager.Test.Common;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.IntegrationTest
{
    // Linked into each native integration test assembly; uses that platform's existing test databases.
    [TestClass]
    [DoNotParallelize]
    public class SharedThreadedPackageTest
    {
        [TestMethod]
        [DataRow(true, false, false)]
        [DataRow(true, true, false)]
        [DataRow(false, false, false)]
        [DataRow(false, true, false)]
        [DataRow(true, false, true)]
        [DataRow(true, true, true)]
        [DataRow(false, false, true)]
        [DataRow(false, true, true)]
        public async Task SharedScripts_FinalizeWithIndependentHistories(bool transactional, bool doubleDatabase, bool scriptError)
        {
            var platform = typeof(SharedThreadedPackageTest).Assembly.GetName().Name switch
            {
                "SqlBuildManager.Console.SqlServer.IntegrationTest" => DatabasePlatform.SqlServer,
                "SqlBuildManager.Console.PostgreSQL.IntegrationTest" => DatabasePlatform.PostgreSQL,
                "SqlBuildManager.Console.MySQL.IntegrationTest" => DatabasePlatform.MySQL,
                var name => throw new InvalidOperationException($"Unknown integration test platform: {name}")
            };
            var (server, databases, auth) = platform switch
            {
                DatabasePlatform.SqlServer => (TestEnvironment.SqlServer, new[] { "SqlBuildTest1", "SqlBuildTest2", "SqlBuildTest3", "SqlBuildTest4" }, TestEnvironment.GetSqlAuthArgs()),
                DatabasePlatform.PostgreSQL => (TestEnvironment.PostgresServer, new[] { "sbm_pg_test", "sbm_pg_test1", "sbm_pg_test2", "sbm_pg_test3" }, TestEnvironment.GetPostgresAuthArgs()),
                DatabasePlatform.MySQL => (TestEnvironment.MySqlServer, new[] { "sbm_mysql_test", "sbm_mysql_test1", "sbm_mysql_test2", "sbm_mysql_test3" }, TestEnvironment.GetMySqlAuthArgs()),
                _ => throw new InvalidOperationException("Unsupported test platform.")
            };
            var root = Path.Combine(Path.GetTempPath(), $"sbm-shared-native-{Guid.NewGuid():N}");
            Directory.CreateDirectory(root);
            try
            {
                var source = Path.Combine(root, "source");
                Directory.CreateDirectory(source);
                var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
                for (int i = 0; i < 2; i++)
                {
                    var name = $"script-{i}.sql";
                    File.WriteAllText(Path.Combine(source, name), scriptError && i == 1
                        ? $"SELECT * FROM sbm_missing_{Guid.NewGuid():N};" : "SELECT 1;");
                    model.Script.Add(new Script
                    {
                        FileName = name, ScriptId = Guid.NewGuid().ToString(), BuildOrder = i + 1,
                        Database = doubleDatabase && i == 1 ? "client2" : "client",
                        RollBackOnError = true, CausesBuildFailure = true, AllowMultipleRuns = true
                    });
                }
                var package = Path.Combine(root, "input.sbm");
                await SqlBuildFileHelper.SaveSqlBuildProjectFileAsync(model, Path.Combine(source, "SqlSyncBuildProject.xml"), package, false);
                var original = File.ReadAllBytes(package);
                var overrides = Path.Combine(root, "targets.cfg");
                File.WriteAllLines(overrides, doubleDatabase
                    ? new[] { $"{server}:client,{databases[0]};client2,{databases[1]}", $"{server}:client,{databases[2]};client2,{databases[3]}" }
                    : databases.Select(db => $"{server}:client,{db}"));
                var args = new[]
                {
                    "threaded", "run", "--platform", platform.ToString(), "--packagename", package,
                    "--override", overrides, "--rootloggingpath", Path.Combine(root, "logs"),
                    "--transactional", transactional.ToString(), "--trial", "false", "--timeoutretrycount", "0",
                    "--concurrency", "4", "--concurrencytype", "Count"
                }.Concat(auth).ToArray();
                var manager = new ThreadedManager(CommandLineBuilder.ParseArguments(args));
                var result = await manager.ExecuteAsync();
                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                var workingDirectory = Path.Combine(root, "logs", "Working");
                var sharedProject = Path.Combine(workingDirectory, "SqlSyncBuildProject.xml");
                var errors = Directory.GetFiles(workingDirectory, "*Error.log", SearchOption.AllDirectories)
                    .Select(File.ReadAllText);
                Assert.AreEqual(!scriptError, result == 0, $"Result {result}:\n{string.Join("\n", errors)}");
                Assert.AreEqual(2, Directory.GetFiles(workingDirectory, "*.sql", SearchOption.AllDirectories).Length);
                Assert.AreEqual(0, Directory.GetFiles(workingDirectory, "*.sbm", SearchOption.AllDirectories).Length);
                CollectionAssert.AreEqual(original, File.ReadAllBytes(package));
                var targetProjects = Directory.GetFiles(workingDirectory, Path.GetFileName(sharedProject), SearchOption.AllDirectories)
                    .Where(path => !path.Equals(sharedProject, StringComparison.OrdinalIgnoreCase)).ToArray();
                Assert.AreEqual(doubleDatabase ? 2 : 4, targetProjects.Length);
                foreach (var path in targetProjects)
                {
                    var target = await SqlSyncBuildDataXmlSerializer.LoadAsync(path);
                    Assert.AreEqual(1, target.Build.Count);
                    Assert.AreEqual(2, target.ScriptRun.Count);
                    Assert.IsTrue(Guid.TryParse(target.Build[0].BuildId, out _));
                    foreach (var run in target.ScriptRun)
                        Assert.AreEqual(target.Build[0].BuildId, run.BuildId);
                    Assert.AreEqual(scriptError
                        ? (transactional ? BuildItemStatus.RolledBack : BuildItemStatus.FailedNoTransaction)
                        : BuildItemStatus.Committed, target.Build[0].FinalStatus);
                    Assert.AreEqual(doubleDatabase ? 2 : 1, target.ScriptRun.Select(run => run.Database).Distinct().Count());
                }
            }
            finally
            {
                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                Directory.Delete(root, true);
            }
        }
    }
}
