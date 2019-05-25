using System;
using System.Collections.Generic;
using System.Text;

namespace SqlSync.DbInformation.ChangeDates
{
    public static class DatabaseObjectChangeDates
    {
        static Servers servers;
        static DatabaseObjectChangeDates()
        {
            servers = new Servers();
        }

        public static Servers Servers
        {
            get
            {
                return servers;
            }
        }
    }

    public class Servers
    {
        Dictionary<string, Databases> serverCollection;
        public Servers()
        {
            this.serverCollection = new Dictionary<string, Databases>();
        }

        public Dictionary<string, Databases> ServerCollection
        {
            get { return serverCollection; }
            set { serverCollection = value; }
        }

        public Databases this[string serverName]
        {
            get
            {
                if (!serverCollection.ContainsKey(serverName))
                    serverCollection.Add(serverName, new Databases());

                return serverCollection[serverName];
            }
            set
            {
                if (serverCollection.ContainsKey(serverName))
                    serverCollection[serverName] = value;
                else
                    serverCollection.Add(serverName, value);
            }
        }
    }


    public class Databases
    {
        public Databases()
        {
            databaseCollection = new Dictionary<string, DatabaseObject>();
        }
        Dictionary<string, DatabaseObject> databaseCollection;

        public Dictionary<string, DatabaseObject> DatabaseCollection
        {
            get { return databaseCollection; }
            set { databaseCollection = value; }
        }
        public DatabaseObject this[string databaseName]
        {
            get
            {
                if (!databaseCollection.ContainsKey(databaseName))
                    databaseCollection.Add(databaseName, new DatabaseObject());

                return databaseCollection[databaseName];
            }
            set
            {
                if (databaseCollection.ContainsKey(databaseName))
                    databaseCollection[databaseName] = value;
                else
                    databaseCollection.Add(databaseName, value);

            }
        }
       


    }


    public class DatabaseObject
    {
        private DateTime lastRefreshTime = DateTime.MinValue;

        public DateTime LastRefreshTime
        {
            get { return lastRefreshTime; }
            set { lastRefreshTime = value; }
        }
        public DatabaseObject()
        {
            databaseObjectCollection = new Dictionary<string, DateTime>();
        }
        Dictionary<string, DateTime> databaseObjectCollection;

        public Dictionary<string, DateTime> DatabaseObjectCollection
        {
            get { return databaseObjectCollection; }
            set { databaseObjectCollection = value; }
        }

        public DateTime this[string databaseObjectName]
        {
            get
            {
                if (databaseObjectCollection.ContainsKey(databaseObjectName))
                    return databaseObjectCollection[databaseObjectName];
                else
                    return DateTime.MinValue;

            }
            set
            {
                if (databaseObjectCollection.ContainsKey(databaseObjectName))
                    databaseObjectCollection[databaseObjectName] = value;
                else
                    databaseObjectCollection.Add(databaseObjectName, value);
            }
        }

    }
}

