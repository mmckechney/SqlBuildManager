using SqlSync.Connection;
using SqlSync.SqlBuild.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSync.SqlBuild.Services
{
    /// <summary>
    /// Managed the connections used during a build
    /// </summary>
    public interface IConnectionsService
    {
        public BuildConnectData GetOrAddBuildConnectionDataClass(ConnectionData connData, string serverName, string databaseName, bool isTransactional);
        public BuildConnectData GetBuildConnectionDataClass(string serverName, string databaseName, bool isTransactional);
        public BuildConnectData GetOrAddBuildConnectionDataClassWithLocalAuth(string serverName, string databaseName, bool isTransactional);
        Dictionary<string, BuildConnectData> Connections { get; }
        void ResetConnectionsForRetry();
    }
}
