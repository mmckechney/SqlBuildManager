using System;
using System.Collections.Generic;
using System.IO;

namespace SqlSync.SqlBuild
{
    public static class ServerConnectConfigPersistence
    {
        public static ServerConnectConfigModel Load(string path)
        {
            if (File.Exists(path))
            {
                return ServerConnectConfigXmlSerializer.Load(path);
            }
            return new ServerConnectConfigModel(new List<ServerConfiguration>(), new List<LastProgramUpdateCheck>(), new List<LastDirectory>());
        }

        public static void Save(string path, ServerConnectConfigModel model)
        {
            ServerConnectConfigXmlSerializer.Save(path, model);
        }
    }
}
