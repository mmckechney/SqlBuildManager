using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlSync.SqlBuild
{
    public static class ServerConnectConfigMappers
    {
        public static ServerConnectConfigModel ToModelLegacy(ServerConnectConfig ds) => new ServerConnectConfigModel(new List<ServerConfiguration>(), new List<LastProgramUpdateCheck>(), new List<LastDirectory>());

        // legacy dataset converters removed

        // legacy dataset converters removed

        // legacy dataset converters removed

        // legacy dataset converters removed

        // legacy dataset converters removed
    }
}
