
using System;
using System.Collections.Generic;
using System.Xml.Serialization;
namespace SqlSync.SqlBuild.Status
{
    [Serializable()]
    public class ServerStatusDataCollection
    {
        [XmlElement("BuildFileNameShort")]
        public string BuildFileNameShort
        {
            get { return buildFileNameShort; }
            set { buildFileNameShort = value; }
        }
        string buildFileNameShort = string.Empty;
        string buildFileNameFull = string.Empty;

        [XmlElement("BuildFileNameFull")]
        public string BuildFileNameFull
        {
            get { return buildFileNameFull; }
            set
            {
                buildFileNameFull = value;
                buildFileNameShort = System.IO.Path.GetFileName(value);
            }
        }
        private ServerDictionary serverDict = new ServerDictionary();

        [XmlElement("Servers")]
        public ServerDictionary ServerDict
        {
            get { return serverDict; }
            set { serverDict = value; }
        }


        internal ServerStatusDataCollection()
        {
        }

        public Databases this[string serverName]
        {
            get
            {
                if (!serverDict.ContainsKey(serverName))
                    serverDict.Add(serverName, new Databases());

                return serverDict[serverName];
            }
            set
            {
                if (!serverDict.ContainsKey(serverName))
                    serverDict.Add(serverName, value);
                else
                    serverDict[serverName] = value;

            }
        }

    }

    [Serializable()]
    public class StatusDataCollection : List<ScriptStatusData>
    {

    }

    public class ServerDictionary : StatusDictionary<Databases>
    {
        public ServerDictionary()
            : base("Server", "ServerName", "Databases")
        {
        }
        public KeyCollection Servers
        {
            get
            {
                return base.Keys;
            }
        }
        public ValueCollection Databases
        {
            get
            {
                return base.Values;
            }
        }
    }

    public class Databases : StatusDictionary<StatusDataCollection>
    {
        public Databases()
            : base("Database", "DatabaseName", "Scripts")
        {
        }
        //public KeyCollection Databases
        //{
        //    get
        //    {
        //        return base.Keys;
        //    }
        //}
        //public ValueCollection Scripts
        //{
        //    get
        //    {
        //        return base.Values;
        //    }
        //}
    }

}
