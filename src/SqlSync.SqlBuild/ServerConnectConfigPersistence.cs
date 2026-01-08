using System;
using System.IO;

namespace SqlSync.SqlBuild
{
    public static class ServerConnectConfigPersistence
    {
        public static ServerConnectConfigModel Load(string path)
        {
            var ds = new ServerConnectConfig();
            if (File.Exists(path))
            {
                ds.ReadXml(path);
            }
            return ds.ToModel();
        }

        public static void Save(string path, ServerConnectConfigModel model)
        {
            var ds = model.ToDataSet();
            ds.WriteXml(path);
        }
    }
}
