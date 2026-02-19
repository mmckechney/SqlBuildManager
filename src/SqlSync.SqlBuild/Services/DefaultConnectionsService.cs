using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Polly;
using SqlSync.Connection;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.SqlLogging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSync.SqlBuild.Services
{
    internal class DefaultConnectionsService : IConnectionsService
    {
        private readonly IDbConnectionFactory _connectionFactory;
        private readonly ITransactionManager _transactionManager;

        public DefaultConnectionsService() : this(null, null)
        {
        }

        public DefaultConnectionsService(IDbConnectionFactory connectionFactory = null, ITransactionManager transactionManager = null)
        {
            _connectionFactory = connectionFactory ?? new SqlSync.Connection.SqlServerConnectionFactory();
            _transactionManager = transactionManager ?? new SqlServerTransactionManager();
        }
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string GetDatabaseKey(string serverName, string databaseName)
        {
            return serverName.ToUpper() + ":" + databaseName.ToUpper();
        }
        private BuildConnectData PrepConnectionAndTransaction(BuildConnectData buildConnectData, bool isTransactional)
        {
            try
            {
                if (buildConnectData.Connection.State != System.Data.ConnectionState.Open)
                {
                    var pollyConnection = Policy.Handle<System.Data.Common.DbException>().WaitAndRetry(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.3, retryAttempt)));
                    pollyConnection.Execute(() => buildConnectData.Connection.Open());
                }
                if (isTransactional)
                {
                    buildConnectData.Transaction = buildConnectData.Transaction ?? _transactionManager.BeginTransaction(buildConnectData.Connection);
                }

                return buildConnectData;
            }
            catch (Exception exe)
            {
                log.LogError("Error preparing connection and transaction for " + buildConnectData.ServerName + "." + buildConnectData.DatabaseName + " : " + exe.Message);
                throw;
            }
        }
        public BuildConnectData GetBuildConnectionDataClass(string serverName, string databaseName, bool isTransactional)
        {
            var dbKey = GetDatabaseKey(serverName, databaseName);
            if (!Connections.ContainsKey(dbKey))
            {
               throw new ArgumentNullException("Connection data not found for " + serverName + "." + databaseName);
            }

            var buildConn = Connections[dbKey];
            buildConn = PrepConnectionAndTransaction(buildConn, isTransactional);
            return buildConn;
        }

        [Description("This should only be used for local windows authentication connections and test scenarios")]
        public BuildConnectData GetOrAddBuildConnectionDataClassWithLocalAuth(string serverName, string databaseName, bool isTransactional)
        {
            try
            {
                //always Upper case the database name
                string databaseKey = serverName.ToUpper() + ":" + databaseName.ToUpper();
                if (Connections.ContainsKey(databaseKey) == false)
                {
                    BuildConnectData cData = new BuildConnectData();
                    cData.Connection = _connectionFactory.CreateConnection(databaseName, serverName,"","",AuthenticationType.Windows,20,"");
                    cData = PrepConnectionAndTransaction(cData, isTransactional);
                    cData.DatabaseName = databaseName;
                    cData.ServerName = serverName;

                    Connections.Add(databaseKey, cData);
                    return cData;
                }
                else
                {
                    return (BuildConnectData)Connections[databaseKey];
                }
            }
            catch (Exception exe)
            {
                log.LogError("Error getting connection data for " + serverName + "." + databaseName + " : " + exe.Message);
                throw;
            }
        }

        public BuildConnectData GetOrAddBuildConnectionDataClass(ConnectionData connData, string serverName, string databaseName, bool isTransactional)
        {
            try
            {
                //always Upper case the database name
                string databaseKey = serverName.ToUpper() + ":" + databaseName.ToUpper();
                if (Connections.ContainsKey(databaseKey) == false)
                {
                    BuildConnectData cData = new BuildConnectData();
                    cData.Connection = _connectionFactory.CreateConnection(databaseName, serverName, connData.UserId, connData.Password, connData.AuthenticationType, connData.ScriptTimeout, connData.ManagedIdentityClientId);
                    cData = PrepConnectionAndTransaction(cData, isTransactional);
                    cData.DatabaseName = databaseName;
                    cData.ServerName = serverName;

                    Connections.Add(databaseKey, cData);
                    return cData;
                }
                else
                {
                    return (BuildConnectData)Connections[databaseKey];
                }
            }
            catch (Exception exe)
            {
                log.LogError("Error getting connection data for " + serverName + "." + databaseName + " : " + exe.Message);
                throw;
            }
        }
        public void ResetConnectionsForRetry()
        {
            try
            {
                foreach (var kvp in Connections.ToList())
                {
                    try { kvp.Value.Transaction?.Dispose(); } catch { }
                    try { kvp.Value.Connection?.Close(); } catch { }
                    try { kvp.Value.Connection?.Dispose(); } catch { }
                }
            }
            catch { }
            Connections.Clear();
        }
        public Dictionary<string, BuildConnectData> Connections { get; } = new Dictionary<string, BuildConnectData>();
    }
}
