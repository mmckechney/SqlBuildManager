using SqlSync.Connection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace SqlSync.SqlBuild.MultiDb
{
    public class MultiDbData : List<ServerData>
    {
        public int AllowableTimeoutRetries { get; set; } = 0;
        public bool IsTransactional { get; set; } = true;
        public bool RunAsTrial { get; set; } = true;
        [XmlIgnore()]
        public string BuildFileName { get; set; } = string.Empty;
        [XmlIgnore()]
        public string ProjectFileName { get; set; } = string.Empty;
        [XmlIgnore()]
        public SqlSyncBuildData BuildData { get; set; } = null;
        [XmlIgnore()]
        public string MultiRunId { get; set; } = string.Empty;
        [XmlIgnore()]
        public string BuildDescription
        {
            get;
            set;
        }
        [XmlIgnore()]
        public string UserName { get; set; } = string.Empty;
        [XmlIgnore()]
        public string Password { get; set; } = string.Empty;
        public AuthenticationType AuthenticationType { get; set; } = AuthenticationType.Password;
        [XmlIgnore()]
        public string BuildRevision { get; set; } = string.Empty;

    }
    public class ServerData
    {

        public string ServerName { get; set; } = string.Empty;
        public DbOverrides Overrides { get; set; } = new DbOverrides();

        [XmlIgnore()]
        [JsonIgnore()]
        public int? SequenceId { get; set; } = null;

        public bool Equals(ServerData other)
        {
            if (ServerName != other.ServerName)
            {
                return false;
            }
            return Overrides.Equals(other.Overrides);
        }

    }

    [Serializable]
    public class DbOverrides : List<DatabaseOverride>
    {
        public DbOverrides() { }
        public DbOverrides(params DatabaseOverride[] ovr)
        {
            AddRange(ovr);
        }
        public List<QueryRowItem> GetQueryRowData(string defaultDb, string overrideDb)
        {
            try
            {
                if (Count == 0)
                    return new List<QueryRowItem>();

                defaultDb = defaultDb.ToLower().Trim();
                overrideDb = overrideDb.ToLower().Trim();
                List<QueryRowItem> queryRow = (from o in this
                                               where o.DefaultDbTarget.ToLower().Trim() == defaultDb &&
                                               o.OverrideDbTarget.ToLower().Trim() == overrideDb
                                               select o.QueryRowData).First().ToList<QueryRowItem>();

                if (queryRow == null)
                    queryRow = new List<QueryRowItem>();

                return queryRow;
            }
            catch (InvalidOperationException) //when "o" has no values
            {
                return new List<QueryRowItem>();
            }


        }
        public List<string> GetOverrideDatabaseNameList()
        {
            List<string> dbList = new List<string>();
            foreach (DatabaseOverride ovr in this)
            {
                if (!dbList.Contains(ovr.OverrideDbTarget))
                {
                    dbList.Add(ovr.OverrideDbTarget);
                }
            }
            dbList.Sort();
            return dbList;
        }

        public bool Equals(DbOverrides other)
        {
            if (Count != other.Count)
            {
                return false;
            }
            Sort();
            other.Sort();

            for (int i = 0; i < Count; i++)
            {
                if (this[i].DefaultDbTarget != other[i].DefaultDbTarget)
                {
                    return false;
                }
                if (this[i].OverrideDbTarget != other[i].OverrideDbTarget)
                {
                    return false;
                }
            }

            return true;
        }


    }

}
