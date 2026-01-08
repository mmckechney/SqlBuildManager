using System.IO;
using SqlSync.ObjectScript.Models;

namespace SqlSync.ObjectScript
{
    public static class AutoScriptingConfigPersistence
    {
        public static AutoScriptingConfigModel Load(string path)
        {
            var ds = new AutoScriptingConfig();
            if (File.Exists(path))
            {
                ds.ReadXml(path);
            }
            return ds.ToModel();
        }

        public static void Save(string path, AutoScriptingConfigModel model)
        {
            var ds = model.ToDataSet();
            ds.WriteXml(path);
        }
    }
}
