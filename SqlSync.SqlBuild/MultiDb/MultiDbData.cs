using System;
using System.Collections.Generic;
using System.Collections;
using System.Text;
using System.Xml.Serialization;
using SqlSync.Connection;
using SqlSync.DbInformation;
using System.Linq;
namespace SqlSync.SqlBuild.MultiDb
{
    public class MultiDbData : List<ServerData>
    {
        private int allowableTimeoutRetries = 0;

        public int AllowableTimeoutRetries
        {
            get { return allowableTimeoutRetries; }
            set { allowableTimeoutRetries = value; }
        }
        public ServerData this[string serverName]
        {
            get
            {

                for (int i = 0; i < this.Count; i++)
                {
                    if (this[i].ServerName.ToLower() == serverName.ToLower())
                        return this[i];
                }
                return null;
            }
            set
            {
                bool found = false;
                for (int i = 0; i < this.Count; i++)
                {
                    if (this[i].ServerName.ToLower() == serverName.ToLower())
                    {
                        this[i] = value;
                        found = true;
                    }
                }
                if (!found)
                    this.Add(value);
            }
         }
        private bool isTransactional = true;

        public bool IsTransactional
        {
            get { return isTransactional; }
            set { isTransactional = value; }
        }
        private bool runAsTrial = true;

        public bool RunAsTrial
        {
            get { return runAsTrial; }
            set { runAsTrial = value; }
        }

        [XmlIgnore()]
        private string buildFileName = string.Empty;
        [XmlIgnore()]
        public string BuildFileName
        {
            get { return buildFileName; }
            set { buildFileName = value; }
        }
        [XmlIgnore()]
        private string projectFileName = string.Empty;
        [XmlIgnore()]
        public string ProjectFileName
        {
            get { return projectFileName; }
            set { projectFileName = value; }
        }
        [XmlIgnore()]
        private SqlSyncBuildData buildData = null;
        [XmlIgnore()]
        public SqlSyncBuildData BuildData
        {
            get { return buildData; }
            set { buildData = value; }
        }
        [XmlIgnore()]
        private string multiRunId = string.Empty;
        [XmlIgnore()]
        public string MultiRunId
        {
            get { return multiRunId; }
            set { multiRunId = value; }
        }
        [XmlIgnore()]
        public string BuildDescription
        {
            get;
            set;
        }
        [XmlIgnore()]

        private string username = string.Empty;
        public string UserName
        {
            get
            {
                return username;
            }
             set
            {
                username = value;
            }
        }
        private string password = string.Empty;
        [XmlIgnore()]
        public string Password
        {
            get
            {
                return password;
            }
            set
            {
                password = value;
            }
        }
        private AuthenticationType authenticationType = AuthenticationType.Password;
        public AuthenticationType AuthenticationType
        {
            get
            {
                return this.authenticationType;
            }
            set
            {
                this.authenticationType = value;
            }
        }
        [XmlIgnore()]
        public string BuildRevision
        {
            get;
            set;
        }
    }
    public class ServerData 
    {
        string serverName = string.Empty;

        [XmlIgnore()]
        DatabaseList databases = new DatabaseList();
        [XmlIgnore()]
        public DatabaseList Databases
        {
            get { return databases; }
            set { databases = value; }
        }


        public string ServerName
        {
            get { return serverName; }
            set { serverName = value; }
        }
        DbOverrideSequence overrideSequence = new DbOverrideSequence();

        public DbOverrideSequence OverrideSequence
        {
            get { return overrideSequence;  }
            set { overrideSequence = value; }
        }

    }
    public class DbOverrideSequence : SerializableDictionary<string, List<DatabaseOverride>>  
    {
        public void Add(string sequenceId, DatabaseOverride ovrRide)
        {
            List<DatabaseOverride> ovr = new List<DatabaseOverride>();
            ovr.Add(ovrRide);
            base.Add(sequenceId, ovr);
        }
        public new void Add (string sequenceId, List<DatabaseOverride> overrides)
        {
            base.Add(sequenceId, overrides);
        }
        public void Sort()
        {
            List<KeyValuePair<string, List<DatabaseOverride>>> tmpLst = new List<KeyValuePair<string, List<DatabaseOverride>>>();
            foreach (KeyValuePair<string, List<DatabaseOverride>> tmp in this)
            {
                tmpLst.Add(tmp);
            }
            tmpLst.Sort(new MultiDbDataSorter());
            this.Clear();
            for (int i = 0; i < tmpLst.Count; i++)
            {
                this.Add(tmpLst[i].Key, tmpLst[i].Value);
            }
        }

        public string GetSequenceId(string defaultDb, string overrideDb)
        {
            defaultDb = defaultDb.ToLower();
            overrideDb = overrideDb.ToLower();
            foreach (KeyValuePair<string, List<DatabaseOverride>> set in this)
            {
                foreach (DatabaseOverride ovr in set.Value)
                {
                    if (ovr.OverrideDbTarget.ToLower() == overrideDb && ovr.DefaultDbTarget.ToLower() == defaultDb)
                        return set.Key;
                }
            }


            return string.Empty;
        }
        public List<QueryRowItem> GetQueryRowData(string defaultDb, string overrideDb)
        {
            try
            {
                if (this.Count == 0)
                    return new List<QueryRowItem>();

                defaultDb = defaultDb.ToLower().Trim();
                overrideDb = overrideDb.ToLower().Trim();
                List<QueryRowItem> queryRow = (from m in this
                                         from o in m.Value
                                         where m.Value.Count() > 0 && o.DefaultDbTarget.ToLower().Trim() == defaultDb &&
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
            foreach (KeyValuePair<string, List<DatabaseOverride>> set in this)
                foreach (DatabaseOverride ovr in set.Value)
                    if(!dbList.Contains(ovr.OverrideDbTarget))
                        dbList.Add(ovr.OverrideDbTarget);

            dbList.Sort();
            return dbList;
        }


    }

    public class RuntimeServerData
    {
        public RuntimeServerData(ServerData srvData)
        {

        }

    }
    
}
