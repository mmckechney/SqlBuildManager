using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using SqlSync.DbInformation;
using SqlSync.SqlBuild.MultiDb;
using SqlSync.Connection;
namespace SqlSync.SqlBuild.MultiDb
{
    public partial class MultiDbPage : UserControl
    {
        string serverName;

        public string ServerName
        {
            get { return serverName; }
            set { serverName = value; }
        }
        DatabaseList defaultDatabases = new DatabaseList();
        DatabaseList databaseList;
        private ServerData srvData = null;
      
        public ServerData ServerData
        {
            get { return GetServerData(); }
            set { srvData = value; }
        }
        private MultiDbPage()
        {
            InitializeComponent();
        }
        public MultiDbPage(string server, List<string> defaultDatabases, DatabaseList databaseList) : this()
        {
            this.defaultDatabases.AddExistingList(defaultDatabases);
            this.databaseList = databaseList;
            this.serverName = server;
        }
        public MultiDbPage(ServerData srvData, List<string> defaultDatabases)
            : this()
        {
            this.defaultDatabases.AddExistingList(defaultDatabases);
            this.serverName = srvData.ServerName;
            this.srvData = srvData;
        }
        public void DataBind()
        {
            this.tabControl1.TabPages.Clear();
            if (srvData != null)
            {

                if(srvData.Databases != null && srvData.Databases.IsAllManuallyEntered())
                {
                    this.errorProvider1.SetError(this.lblServerName,"There was an error connecting to this server.\r\nUnable to generate a database list");
                }


                foreach (KeyValuePair<string, List<DatabaseOverride>> setting in srvData.OverrideSequence)
                {
                    foreach(DatabaseOverride ovr in setting.Value)
                        if(!this.defaultDatabases.Contains(ovr.DefaultDbTarget))
                            this.defaultDatabases.Add(ovr.DefaultDbTarget,true);
                }

                this.lblServerName.Text = srvData.ServerName;

                if (this.defaultDatabases.Count == 0)
                {
                    DatabaseItem item = new DatabaseItem();
                    item.DatabaseName = "";
                    this.defaultDatabases.Add(item);
                }


                for (int i = 0; i < this.defaultDatabases.Count; i++)
                {
                    MultiDbConfig cfg = new MultiDbConfig(this.defaultDatabases[i], srvData.Databases, srvData.OverrideSequence);
                    cfg.DataBind();
                    cfg.ValueChanged += new EventHandler(cfg_ValueChanged);
                    this.tabControl1.TabPages.Add(SetupTagPage(this.defaultDatabases[i].DatabaseName,cfg));
                }

            }
            else
            {
                this.lblServerName.Text = this.serverName;
                if (this.defaultDatabases.Count == 0)
                {
                    DatabaseItem item = new DatabaseItem();
                    item.DatabaseName = "";
                    this.defaultDatabases.Add(item);
                }
                for (int i = 0; i < this.defaultDatabases.Count; i++)
                {
                    MultiDbConfig cfg = new MultiDbConfig(this.defaultDatabases[i], this.databaseList);
                    cfg.DataBind();
                    cfg.ValueChanged += new EventHandler(cfg_ValueChanged);
                    this.tabControl1.TabPages.Add(SetupTagPage(this.defaultDatabases[i].DatabaseName, cfg));
                }
            }
        }
        private TabPage SetupTagPage(string databaseName, MultiDbConfig dbConfig)
        {
            TabPage tp = new TabPage(databaseName);
            tp.AutoScroll = true;
            tp.Controls.Add(dbConfig);
            return tp;
        }
        void cfg_ValueChanged(object sender, EventArgs e)
        {
            if (this.ValueChanged != null)
                this.ValueChanged(this, EventArgs.Empty);
        }

        public event ServerChangedEventHandler ServerRemoved;


        private void btmRemove_Click(object sender, EventArgs e)
        {
            if (this.ServerRemoved != null)
            {
                this.ServerRemoved(this, this.serverName,"","", AuthenticationType.WindowsAuthentication);
            }
        }

        internal ServerData GetServerData()
        {
            ServerData srvData = new ServerData();
            srvData.ServerName = this.serverName;
            srvData.Databases = this.srvData.Databases;
            
            //Keep track of the largest override sequence

            int maxSet = 0;
            //Get the configs sorted out into the server data...
            for (int i = 0; i < tabControl1.TabPages.Count; i++)
            {
                foreach (Control ctrl in tabControl1.TabPages[i].Controls)
                {
                    if (ctrl is MultiDbConfig)
                    {
                        MultiDbConfig cfg = (MultiDbConfig)ctrl;
                        foreach (string sequenceId in cfg.DatabaseOverrideSequence.Keys)
                        {
                            if (srvData.OverrideSequence.ContainsKey(sequenceId))
                            {
                                srvData.OverrideSequence[sequenceId].Add(cfg.DatabaseOverrideSequence[sequenceId]);
                                if (srvData.OverrideSequence[sequenceId].Count > maxSet)
                                    maxSet = srvData.OverrideSequence[sequenceId].Count;
                            }
                            else
                            {
                                List<DatabaseOverride> tmp = new List<DatabaseOverride>();
                                tmp.Add(cfg.DatabaseOverrideSequence[sequenceId]);
                                srvData.OverrideSequence.Add(sequenceId, tmp);
                                if (maxSet == 0) maxSet = 1;
                            }
                        }
                    }
                }
            }

            //Next, we need to make sure we have the same number of overrides in each Override sequence collection. If not, add the a default "no override" setting...
            foreach (string key in srvData.OverrideSequence.Keys)
            {
                //must be missing an override setting...
                if ( srvData.OverrideSequence[key].Count < maxSet)
                {
                    foreach (DatabaseItem defaultDb in this.defaultDatabases)
                    {
                        bool found = false;
                        foreach (DatabaseOverride dbO in srvData.OverrideSequence[key])
                        {
                            if (dbO.DefaultDbTarget.ToLower() == defaultDb.DatabaseName.ToLower())
                            {
                                found = true;
                                break;
                            }
                        }
                        if(!found)
                        {
                            DatabaseOverride tmp = new DatabaseOverride(defaultDb.DatabaseName, defaultDb.DatabaseName);
                            srvData.OverrideSequence[key].Add(tmp);
                        }   
                        
                    }
                }
            }
            return srvData;
        }

        public event EventHandler ValueChanged;


       
    }
    public delegate void ServerRemovedEventHandler(object sender, string newServerName);
}
