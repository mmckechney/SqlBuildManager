using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.Threaded;
using SqlBuildManager.Connection;
using SqlBuildManager.SqlBuild;
using SqlBuildManager.SqlBuild.Models;
using SqlBuildManager.SqlBuild.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.UnitTest
{
    [TestClass]
    public class SharedPackagePersistenceTest
    {
        [TestMethod]
        [DataRow(1)]
        [DataRow(32)]
        public async Task SharedPackage_PersistsIndependentTargetMetadata_WithoutCopyingScriptsOrArchives(int targetCount)
        {
            var directory = Path.Combine(Path.GetTempPath(), $"sbm-shared-package-{Guid.NewGuid():N}");
            Directory.CreateDirectory(directory);
            try
            {
                var sourceDirectory = Path.Combine(directory, "source");
                Directory.CreateDirectory(sourceDirectory);
                var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
                for (int script = 0; script < 2; script++)
                {
                    var name = $"script-{script}.sql";
                    await File.WriteAllTextAsync(Path.Combine(sourceDirectory, name), "SELECT 1;");
                    model.Script.Add(new Script { FileName = name, ScriptId = Guid.NewGuid().ToString(), BuildOrder = script + 1, Database = "db" + script });
                }
                model.CommittedScript.Add(new CommittedScript { ScriptId = "original", SqlSyncBuildProjectId = model.SqlSyncBuildProject[0].SqlSyncBuildProjectId });
                model.Build.Add(new Build { BuildId = "original-build", FinalStatus = BuildItemStatus.Committed });
                model.ScriptRun.Add(new ScriptRun { BuildId = "original-build", Database = "original-db" });
                var sourceProject = Path.Combine(sourceDirectory, "SqlSyncBuildProject.xml");
                var package = Path.Combine(directory, "input.sbm");
                await SqlBuildFileHelper.SaveSqlBuildProjectFileAsync(model, sourceProject, package, false);
                var originalArchive = await File.ReadAllBytesAsync(package);
                var extracted = await SqlBuildFileHelper.ExtractSqlBuildZipFileAsync(package, Path.Combine(directory, "Working"), resetWorkingDirectory: false);
                Assert.IsTrue(extracted.success, extracted.result);
                var extractedModel = await SqlSyncBuildDataXmlSerializer.LoadAsync(extracted.projectFileName);
                var context = new BuildExecutionContext
                {
                    WorkingDirectory = extracted.workingDirectory,
                    BuildDataModel = extractedModel,
                    BuildZipFileName = package,
                    ProjectFileName = extracted.projectFileName,
                    BatchCollection = new DefaultScriptBatcher().LoadAndBatchSqlScripts(extractedModel, extracted.projectFilePath)
                };
                var originalProject = await File.ReadAllBytesAsync(context.ProjectFileName);
                var sharedScriptPaths = Directory.GetFiles(context.WorkingDirectory, "*.sql", SearchOption.AllDirectories);
                var originalScripts = sharedScriptPaths.ToDictionary(p => p, File.ReadAllBytes);
                var runs = await Task.WhenAll(Enumerable.Range(0, targetCount).Select(async target =>
                {
                    var targetDirectory = Path.Combine(context.WorkingDirectory, "server", "target-" + target);
                    Directory.CreateDirectory(targetDirectory);
                    var runner = new ThreadedRunner("server", new List<DatabaseOverride>
                    {
                        new DatabaseOverride("server", "db0", "target-" + target)
                    }, new CommandLineArgs(), "test", false, context);
                    var run = new SqlBuildRunDataModel();
                    await runner.PrepareSharedPackageRunAsync(run, targetDirectory);
                    Assert.IsNotNull(run.BuildDataModel);
                    Assert.IsNotNull(run.ProjectFileName);
                    Assert.IsNotNull(run.BuildFileName);
                    Assert.AreEqual(string.Empty, run.BuildFileName);
                    Assert.AreEqual(0, run.BuildDataModel.CommittedScript.Count);
                    Assert.AreNotSame(context.BuildDataModel.Script[0], run.BuildDataModel.Script[0]);
                    Assert.AreNotSame(context.BuildDataModel.Build[0], run.BuildDataModel.Build[0]);
                    Assert.AreNotSame(context.BuildDataModel.ScriptRun[0], run.BuildDataModel.ScriptRun[0]);
                    run.BuildDataModel.Script[0].Description = "target-" + target;
                    run.BuildDataModel.Build.Add(new Build { BuildId = target.ToString(), FinalStatus = BuildItemStatus.Committed });
                    run.BuildDataModel.ScriptRun.Add(new ScriptRun { BuildId = target.ToString(), Database = "target-" + target });
                    await SqlBuildFileHelper.SaveSqlBuildProjectFileAsync(run.BuildDataModel, run.ProjectFileName, run.BuildFileName, true);
                    return await SqlSyncBuildDataXmlSerializer.LoadAsync(run.ProjectFileName);
                }));

                foreach (var run in runs)
                {
                    Assert.AreEqual(2, run.Build.Count);
                    Assert.AreEqual(2, run.ScriptRun.Count);
                    Assert.AreEqual("original-build", run.ScriptRun[0].BuildId);
                    Assert.AreEqual("original-db", run.ScriptRun[0].Database);
                    Assert.AreEqual(run.Build[1].BuildId, run.ScriptRun[1].BuildId);
                    Assert.AreEqual("target-" + run.Build[1].BuildId, run.ScriptRun[1].Database);
                    Assert.AreEqual(run.ScriptRun[1].Database, run.Script[0].Description);
                }
                Assert.AreEqual(1, context.BuildDataModel.Build.Count);
                Assert.AreEqual(1, context.BuildDataModel.ScriptRun.Count);
                Assert.AreEqual("original", context.BuildDataModel.CommittedScript[0].ScriptId);
                CollectionAssert.AreEquivalent(sharedScriptPaths, Directory.GetFiles(context.WorkingDirectory, "*.sql", SearchOption.AllDirectories));
                Assert.AreEqual(0, Directory.GetFiles(context.WorkingDirectory, "*.sbm", SearchOption.AllDirectories).Length);
                Assert.AreEqual(0, Directory.GetFiles(context.WorkingDirectory, "*.zip", SearchOption.AllDirectories).Length);
                CollectionAssert.AreEqual(originalArchive, await File.ReadAllBytesAsync(package));
                CollectionAssert.AreEqual(originalProject, await File.ReadAllBytesAsync(context.ProjectFileName));
                foreach (var (path, contents) in originalScripts)
                    CollectionAssert.AreEqual(contents, await File.ReadAllBytesAsync(path));
            }
            finally
            {
                Directory.Delete(directory, true);
            }
        }
    }
}
