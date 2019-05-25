using System;
using System.Collections.Generic;
using System.Text;
using SqlSync.ObjectScript;
using SqlSync.Connection;
namespace SqlSync.ObjectScript.Hash
{
    public class HashCollectionRunner
    {
        private string serverName;
        private string databaseName;
        private bool isBaseLine;

        public bool IsBaseLine
        {
            get { return isBaseLine; }
            set { isBaseLine = value; }
        }
        private ObjectScriptHashData hashData;

        public ObjectScriptHashData HashData
        {
            get { return hashData; }
            set { hashData = value; }
        }
        public HashCollectionRunner(string serverName, string databaseName)
        {
            this.databaseName = databaseName;
            this.serverName = serverName;
         }

        public void CollectHashes()
        {
            if(this.HashCollectionRunnerUpdate != null)
               HashCollectionRunnerUpdate(this,new HashCollectionRunnerUpdateEventArgs(this.serverName,this.databaseName,"Starting"));

            ConnectionData connData = new ConnectionData(serverName, databaseName);
            ObjectScriptHelper helper = new ObjectScriptHelper(connData);
            helper.HashScriptingEvent += new ObjectScriptHelper.HashScriptingEventHandler(helper_HashScriptingEvent);
            this.hashData = helper.GetDatabaseObjectHashes();
            if (hashData != null)
            {
                this.hashData.IsBaseLine = isBaseLine;
            }
            else
            {
                hashData = new ObjectScriptHashData();
            }
            this.hashData.Database = this.databaseName;
            this.hashData.Server = this.serverName;
        }

        void helper_HashScriptingEvent(object sender, HashScriptingEventArgs e)
        {
            if (this.HashCollectionRunnerUpdate != null)
                HashCollectionRunnerUpdate(this, new HashCollectionRunnerUpdateEventArgs(this.serverName, this.databaseName, e.Message));

        }
        public event HashCollectionRunnerUpdateEventHandler HashCollectionRunnerUpdate;
        public delegate void HashCollectionRunnerUpdateEventHandler(object sender, HashCollectionRunnerUpdateEventArgs e);
    }
    public class HashCollectionRunnerUpdateEventArgs
    {
        public readonly string Server;
        public readonly string Database;
        public readonly string Message;
        public HashCollectionRunnerUpdateEventArgs(string server, string database, string message)
        {
            this.Server = server;
            this.Database = database;
            this.Message = message;
        }
    }
}
