using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SqlBuildManager.ServiceClient;
using SqlBuildManager.ServiceClient.Sbm.BuildService;
using SqlSync.SqlBuild.MultiDb;
namespace SqlSync.SqlBuild.Remote
{
    public partial class ServerDistributionListForm : Form
    {
        private bool showLabels = true;
        private MultiDbData data = null;
        private ServerDistributionListForm()
        {
            InitializeComponent();
        }
        public ServerDistributionListForm(MultiDbData data, bool showLabels) : this()
        {
            this.showLabels = showLabels;
            this.data = data;
        }

        private SqlSync.ColumnSorter listSorter = new ColumnSorter();
        private ServerConfigData exeServer = null;
        private BuildSettings buildSetting = null;
        public ServerDistributionListForm(ServerConfigData exeServer, BuildSettings buildSetting)
            : this()
        {
            this.exeServer = exeServer;
            this.buildSetting = buildSetting;
        }

        private void ServerDistributionListForm_Load(object sender, EventArgs e)
        {
            if(this.exeServer != null)
                lblExeServer.Text = this.exeServer.ServerName;

            if(this.data == null)
                data = MultiDbHelper.ImportMultiDbTextConfig(this.buildSetting.MultiDbTextConfig);
            
            
            var matchedSet = from s in data
                    select new
                    {
                        s.ServerName,
                        OverrideTarget = (from o in s.OverrideSequence select o.Value[0].OverrideDbTarget)
                    };


            foreach (var set in matchedSet)
            {
                foreach (string over in set.OverrideTarget)
                {
                    ListViewItem item = new ListViewItem();
                    item.Text = set.ServerName;
                    item.SubItems.Add(over);
                    lstLoad.Items.Add(item);
                }
            }

            if (!this.showLabels)
            {
                this.label1.Visible = false;
                this.lblExeServer.Visible = false;
            }

            statCount.Text = "Item Count: " + lstLoad.Items.Count.ToString();
        }

        private void lstLoad_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListViewItem item = null;
            if (lstLoad.SelectedItems.Count > 0)
                item = lstLoad.SelectedItems[0];


            listSorter.CurrentColumn = e.Column;
            lstLoad.ListViewItemSorter = listSorter;
            lstLoad.Sort();
            if (item != null)
                item.EnsureVisible();
        }

        private void ServerDistributionListForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                this.Close();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    
    }
}
