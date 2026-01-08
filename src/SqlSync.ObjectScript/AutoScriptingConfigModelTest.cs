using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.ObjectScript;
using SqlSync.ObjectScript.Models;
using System;
using System.IO;

namespace SqlSync.ObjectScript.UnitTest
{
    [TestClass]
    public class AutoScriptingConfigModelTest
    {
        [TestMethod]
        public void LoadSave_RoundTrip_Model()
        {
            var model = new AutoScriptingConfigModel(
                AutoScripting: new[] { new AutoScripting(true, true, false, true, 1) },
                DatabaseScriptConfig: new[] { new DatabaseScriptConfig("srv", "db", "user", "pwd", "auth", "path", 1) },
                PostScriptingAction: new[] { new PostScriptingAction("name", "cmd", "args", 1) });

            var tmp = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tmp);
            var file = Path.Combine(tmp, "auto.xml");
            try
            {
                AutoScriptingConfigPersistence.Save(file, model);
                var loaded = AutoScriptingConfigPersistence.Load(file);
                Assert.AreEqual(model.AutoScripting.Count, loaded.AutoScripting.Count);
                Assert.AreEqual(model.DatabaseScriptConfig[0].ScriptToPath, loaded.DatabaseScriptConfig[0].ScriptToPath);
                Assert.AreEqual(model.PostScriptingAction[0].Name, loaded.PostScriptingAction[0].Name);
            }
            finally
            {
                if (Directory.Exists(tmp)) Directory.Delete(tmp, true);
            }
        }

        [TestMethod]
        public void Mapper_DataSet_RoundTrip()
        {
            var ds = new AutoScriptingConfig();
            ds.AutoScripting.AddAutoScriptingRow(true, true, false, true, 1);
            ds.DatabaseScriptConfig.AddDatabaseScriptConfigRow("srv", "db", "user", "pwd", "auth", "path", null);
            ds.PostScriptingAction.AddPostScriptingActionRow("name", "cmd", "args", null);

            var model = ds.ToModel();
            var ds2 = model.ToDataSet();
            Assert.AreEqual(ds.AutoScripting.Count, ds2.AutoScripting.Count);
            Assert.AreEqual(ds.DatabaseScriptConfig[0].ScriptToPath, ds2.DatabaseScriptConfig[0].ScriptToPath);
            Assert.AreEqual(ds.PostScriptingAction[0].Name, ds2.PostScriptingAction[0].Name);
        }
    }
}
