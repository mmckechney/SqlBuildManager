using System;
using System.Collections.Generic;
using System.Linq;
using SqlSync.SqlBuild.Legacy;

namespace SqlSync.SqlBuild.Models
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
