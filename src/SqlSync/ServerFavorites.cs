using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
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
                if (this.SelectedItem != null)
                    return this.SelectedItem.ToString();
                else
                    return "";
            }
        }

        public void InitServers(bool forceRefresh)
        {
            if (this.Items.Count != 0 && !forceRefresh)
                return;

            string homePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\";
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
            this.Items.AddRange(recentDbs);
        }
               
    }
}
