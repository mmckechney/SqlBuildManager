using System.IO;

namespace SqlSync
{
    public partial class ServerFavorites : System.Windows.Forms.ToolStripComboBox
    {
        private const string ConfigFileName = "SqlSync.cfg";

        public ServerFavorites()
        {
            try
            {
                InitServers(false);
            }
            catch { }
        }

        public string ServerName
        {
            get
            {
                if (SelectedItem != null)
                    return SelectedItem.ToString();
                else
                    return "";
            }
        }

        public void InitServers(bool forceRefresh)
        {
            if (Items.Count != 0 && !forceRefresh)
                return;

            string homePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string[] recentDbs = new string[6];

            //if (File.Exists(homePath + ConfigFileName))
            //{
            //    try
            //    {
            //        SqlSyncConfig config = new SqlSyncConfig();
            //        config.ReadXml(homePath + "SqlSync.cfg");
            //        DataView view = config.RecentDatabase.DefaultView;
            //        view.Sort = config.RecentDatabase.LastAccessedColumn.ColumnName;
            //        for (int i = 0; i < view.Count; i++)
            //        {
            //            if (i < 6)
            //                recentDbs[i] = ((SqlSyncConfig.RecentDatabaseRow)view[i].Row).Name;
            //        }
            //        Array.Sort(recentDbs);
            //    }
            //    catch
            //    {

            //    }
            //}
            Items.AddRange(recentDbs);
        }

    }
}
