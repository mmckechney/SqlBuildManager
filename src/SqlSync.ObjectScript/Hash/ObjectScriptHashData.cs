using System;
using System.Collections.Generic;
using System.Text;

namespace SqlSync.ObjectScript.Hash
{
    public class ObjectScriptHashData
    {
        public void ResetComparisonValues()
        {
            this.storedProcedures.ResetComparisonValues();
            this.tables.ResetComparisonValues();
            this.views.ResetComparisonValues();
            this.functions.ResetComparisonValues();
            this.keysAndIndexes.ResetComparisonValues();
            this.logins.ResetComparisonValues();
            this.roles.ResetComparisonValues();
            this.schemas.ResetComparisonValues();
            this.users.ResetComparisonValues();
        }
        public override string ToString()
        {
            return this.server + "." + this.database;
        }
        private bool isBaseLine;

        public bool IsBaseLine
        {
            get { return isBaseLine; }
            set { isBaseLine = value; }
        }

        private string server;

        public string Server
        {
            get { return server; }
            set { server = value; }
        }
        private string database;

        public string Database
        {
            get { return database; }
            set { database = value; }
        }
        public ObjectScriptHashData()
        {
        }

        private Tables tables = new Tables();

        public Tables Tables
        {
            get { return tables; }
            set { tables = value; }
        }
        private StoredProcedures storedProcedures = new StoredProcedures();

        public StoredProcedures StoredProcedures
        {
            get { return storedProcedures; }
            set { storedProcedures = value; }
        }
        private Functions functions = new Functions();

        public Functions Functions
        {
            get { return functions; }
            set { functions = value; }
        }
        private Views views = new Views();

        public Views Views
        {
            get { return views; }
            set { views = value; }
        }
        private KeysAndIndexes keysAndIndexes = new KeysAndIndexes();

        public KeysAndIndexes KeysAndIndexes
        {
            get { return keysAndIndexes; }
            set { keysAndIndexes = value; }
        }
        private Logins logins = new Logins();

        public Logins Logins
        {
            get { return logins; }
            set { logins = value; }
        }
        private Roles roles = new Roles();

        public Roles Roles
        {
            get { return roles; }
            set { roles = value; }
        }
        private Schemas schemas = new Schemas();

        public Schemas Schemas
        {
            get { return schemas; }
            set { schemas = value; }
        }

        private Users users = new Users();
        public Users Users
        {
            get { return users; }
            set { users = value; }
        }

    }

    public class Tables : ObjectHashDictionary
    {
        public Tables()
            : base("Table")
        {
        }
    }
    public class StoredProcedures : ObjectHashDictionary
    {
        public StoredProcedures()
            : base("StoredProcedure")
        {
        }
    }
    public class Functions : ObjectHashDictionary
    {
        public Functions()
            : base("Function")
        {
        }
    }
    public class Views : ObjectHashDictionary
    {
        public Views()
            : base("View")
        {
        }
    }
    public class KeysAndIndexes : ObjectHashDictionary
    {
        public KeysAndIndexes()
            : base("KeysAndIndex")
        {
        }
    }
    public class Logins : ObjectHashDictionary
    {
        public Logins()
            : base("Login")
        {
        }
    }
    public class Roles : ObjectHashDictionary
    {
        public Roles()
            : base("Role")
        {
        }
    }
    public class Schemas : ObjectHashDictionary
    {
        public Schemas()
            : base("Schema")
        {
        }
    }
    public class Users : ObjectHashDictionary
    {
        public Users()
            : base("User")
        {
        }
    }
}
