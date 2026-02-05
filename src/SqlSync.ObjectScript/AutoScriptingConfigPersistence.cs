using System.Collections.Generic;
using System.IO;
using SqlSync.ObjectScript.Models;

namespace SqlSync.ObjectScript
{
    public static class AutoScriptingConfigPersistence
    {
        public static AutoScriptingConfigModel Load(string path)
        {
            if (File.Exists(path))
            {
                return AutoScriptingConfigXmlSerializer.Load(path);
            }
            return new AutoScriptingConfigModel(new List<AutoScripting>(), new List<DatabaseScriptConfig>(), new List<PostScriptingAction>());
        }

        public static void Save(string path, AutoScriptingConfigModel model)
        {
            AutoScriptingConfigXmlSerializer.Save(path, model);
        }
    }
}
