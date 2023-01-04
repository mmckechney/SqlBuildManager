using SqlSync.Connection;
using System;
using System.Collections.Generic;
namespace SqlSync.SqlBuild.Status
{
    class StatusReportRunner
    {
        SqlSyncBuildData buildData;
        string serverName;

        public string ServerName
        {
            get { return serverName; }
            set { serverName = value; }
        }
        private string baseDatabase;

        public string BaseDatabase
        {
            get { return baseDatabase; }
            set { baseDatabase = value; }
        }

        List<DatabaseOverride> dbOverrides;
        private StatusDataCollection status = new StatusDataCollection();
        private string projectFilePath;
        public StatusDataCollection Status
        {
            get { return status; }
            set { status = value; }
        }

        public StatusReportRunner(SqlSyncBuildData buildData, string serverName, List<DatabaseOverride> dbOverrides, string projectFilePath)
        {
            this.buildData = buildData;
            this.serverName = serverName;
            this.dbOverrides = dbOverrides;
            this.projectFilePath = projectFilePath;
        }
        public void RetrieveStatus()
        {
            DateTime commitDate;
            DateTime serverChangeDate;
            Connection.ConnectionData connData;
            string databaseName;
            foreach (SqlSyncBuildData.ScriptRow row in buildData.Script)
            {
                databaseName = ConnectionHelper.GetTargetDatabase(row.Database, dbOverrides);

                if (baseDatabase == null)
                    baseDatabase = databaseName;

                ScriptStatusData dat = new ScriptStatusData();
                connData = new SqlSync.Connection.ConnectionData(serverName, databaseName);

                ScriptStatusType stat = StatusHelper.DetermineScriptRunStatus(row, connData, projectFilePath, true, dbOverrides, out commitDate, out serverChangeDate);
                dat.Fill(row);
                dat.DatabaseName = databaseName;
                dat.ServerName = serverName;
                dat.ServerChangeDate = serverChangeDate;
                dat.LastCommitDate = commitDate;
                dat.ScriptStatus = stat;

                status.Add(dat);

            }
        }

    }
}
