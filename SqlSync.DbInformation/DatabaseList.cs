using System;
using System.Collections.Generic;
using System.Text;

namespace SqlSync.DbInformation
{

    public class DatabaseList :  System.Collections.Generic.List<DatabaseItem>
    {
        public DatabaseList() : base()
        {
        }
        public void Add(string databaseName, bool isManuallyEntered)
        {
            DatabaseItem item = new DatabaseItem();
            item.DatabaseName = databaseName;
            item.IsManuallyEntered = isManuallyEntered;
            base.Add(item);
        }
        public bool Contains(string databaseName)
        {
            foreach (DatabaseItem item in this)
            {
                if (item.DatabaseName.ToLower() == databaseName.ToLower())
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Adds a list of databases to the collection, each being marked as "Manually Entered"
        /// </summary>
        /// <param name="databases"></param>
        public void AddManualList(List<string> databases)
        {
            if (databases == null)
                return;
            for (int i = 0; i < databases.Count; i++)
            {
                DatabaseItem item = new DatabaseItem();
                item.DatabaseName = databases[i];
                item.IsManuallyEntered = true;
                base.Add(item);
            }
        }
        /// <summary>
        /// Adds a list of databases to the collection, each being marked as not "Manually Entered" (i.e. Existing on the server)
        /// </summary>
        /// <param name="databases"></param>
        public void AddExistingList(List<string> databases)
        {
            if (databases == null)
                return;
            for (int i = 0; i < databases.Count; i++)
            {
                DatabaseItem item = new DatabaseItem();
                item.DatabaseName = databases[i];
                item.IsManuallyEntered = false;
                base.Add(item);
            }
        }
        /// <summary>
        /// Adds a list of databases to the collection, checking for a pre-existing.
        /// If it already exists, just updates the IsManuallyEntered property
        /// </summary>
        /// <param name="databases"></param>
        public void AddRangeUnique(List<DatabaseItem> databases)
        {
            if (databases == null)
                return;

            for (int i = 0; i < databases.Count; i++)
            {
                DatabaseItem tmp = this.Find(databases[i].DatabaseName);
                if (tmp != null)
                    tmp.IsManuallyEntered = databases[i].IsManuallyEntered;
                else
                {
                    this.Add(databases[i]);
                }
            }
        }
        public DatabaseItem Find(string databaseName)
        {
            for (int i = 0; i < this.Count; i++)
            {
                if (this[i].DatabaseName.ToLower() == databaseName.ToLower())
                    return this[i];
            }
            return null;
        }
        public bool IsAllManuallyEntered()
        {
            for (int i = 0; i < this.Count; i++)
            {
                if(this[i].IsManuallyEntered == false)
                    return false;
            }
            return true;
        }
    }
    public class DatabaseListComparer : IComparer<DatabaseItem>
    {
        public int Compare(DatabaseItem x, DatabaseItem y)
        {
            return string.Compare(x.DatabaseName, y.DatabaseName);
        }
    }

}
