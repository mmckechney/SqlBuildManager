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
using System.Linq;
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
        private List<ServerData> lstSrvData = null;
      
        public List<ServerData> ServerData
        {
            get { return GetServerData(); }
            set { lstSrvData = value; }
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
        public MultiDbPage(List<ServerData> lstSrvData, List<string> defaultDatabases)
            : this()
        {
            this.defaultDatabases.AddExistingList(defaultDatabases);
            if (lstSrvData.Count > 0)
            {
                this.serverName = lstSrvData.First().ServerName;
            }
            this.lstSrvData = lstSrvData;
        }
        public void DataBind()
        {
            this.tabControl1.TabPages.Clear();
            if (lstSrvData != null && lstSrvData.Count > 0)
            {
                //if (lstSrvData.Databases != null && lstSrvData.Databases.IsAllManuallyEntered())
                //{
                //    this.errorProvider1.SetError(this.lblServerName,"There was an error connecting to this server.\r\nUnable to generate a database list");
                //}


                foreach (var svData in lstSrvData)
                {
                    foreach(DatabaseOverride ovr in svData.Overrides)
                        if(!this.defaultDatabases.Contains(ovr.DefaultDbTarget))
                            this.defaultDatabases.Add(ovr.DefaultDbTarget,true);
                }

                this.lblServerName.Text = lstSrvData.First().ServerName;

                if (this.defaultDatabases.Count == 0)
                {
                    DatabaseItem item = new DatabaseItem();
                    item.DatabaseName = "";
                    this.defaultDatabases.Add(item);
                }

                var ovrs = new DbOverrides();
                lstSrvData.ForEach(l => ovrs.AddRange(l.Overrides));
                var dbs = new DatabaseList();
                    
                for (int i = 0; i < this.defaultDatabases.Count; i++)
                {
                    MultiDbConfig cfg = new MultiDbConfig(this.defaultDatabases[i], lstSrvData);
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
                    MultiDbConfig cfg = new MultiDbConfig(this.defaultDatabases[i]);
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
                this.ServerRemoved(this, this.serverName,"","", AuthenticationType.Windows);
            }
        }

        internal List<ServerData> GetServerData()
        {
            List<ServerData> lstSrvData = new List<ServerData>();
            //Keep track of the largest override sequence

            int maxSet = 0;
            //Get the configs sorted out into the server data...
            for (int i = 0; i < tabControl1.TabPages.Count; i++)
            {
                foreach (Control ctrl in tabControl1.TabPages[i].Controls)
                {
                    if (string.IsNullOrWhiteSpace(tabControl1.TabPages[i].Text))
                        continue;

                    if (ctrl is MultiDbConfig)
                    {
                        MultiDbConfig cfg = (MultiDbConfig)ctrl;
                        
                        foreach (int? sequenceId in cfg.DatabaseOverrideSequence.Keys)
                        {

                            List<DatabaseOverride> tmp = new List<DatabaseOverride>();
                            tmp.Add(cfg.DatabaseOverrideSequence[sequenceId.Value]);
                            
                            ServerData tmpSv = new ServerData() { ServerName = this.serverName, SequenceId = sequenceId.Value };
                            tmpSv.Overrides.AddRange(tmp);
                            lstSrvData.Add(tmpSv);
                            if (tmp.Count > maxSet)
                            {
                                maxSet = tmp.Count;
                            }
                        }
                    }
                }
            }

            //Next, we need to make sure we have the same number of overrides in each Override sequence collection. If not, add the a default "no override" setting...
            foreach (var srvData in lstSrvData)
            {
                //must be missing an override setting...
                if ( srvData.Overrides.Count < maxSet)
                {
                    foreach (DatabaseItem defaultDb in this.defaultDatabases)
                    {
                        bool found = false;
                        foreach (DatabaseOverride dbO in srvData.Overrides)
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
                            srvData.Overrides.Add(tmp);
                        }   
                        
                    }
                }
            }
            return lstSrvData;
        }

        public event EventHandler ValueChanged;


       
    }
    public delegate void ServerRemovedEventHandler(object sender, string newServerName);
}
