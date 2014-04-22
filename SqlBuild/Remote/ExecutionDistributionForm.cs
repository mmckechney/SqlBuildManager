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
namespace SqlSync.SqlBuild.Remote
{
    public partial class ExecutionDistributionForm : Form
    {
        private ExecutionDistributionForm()
        {
            InitializeComponent();
        }
        private DistributionType disType;
        private string[] multiDbLines;
        List<ServerConfigData> activeExecutionServers;
        public ExecutionDistributionForm(DistributionType disType, string[] multiDbLines, List<ServerConfigData> activeExecutionServers)
            : this()
        {
            this.disType = disType;
            this.multiDbLines = multiDbLines;
            this.activeExecutionServers = activeExecutionServers;
        }

        private void ExecutionDistributionForm_Load(object sender, EventArgs e)
        {
            BuildServiceManager buildManager = new BuildServiceManager();
            List<string> lstUntaskedExecutionServers;
            List<string> lstUnassignedDatabaseServers;
            buildManager.ValidateLoadDistribution(this.disType, this.activeExecutionServers, this.multiDbLines, out lstUntaskedExecutionServers, out lstUnassignedDatabaseServers);

            BuildSettings settings = new BuildSettings();
            settings.MultiDbTextConfig = this.multiDbLines;

            IDictionary<ServerConfigData, BuildSettings> distributedLoad = buildManager.DistributeBuildLoad(settings, this.disType, this.activeExecutionServers);

            foreach (KeyValuePair<ServerConfigData, BuildSettings> set in distributedLoad)
            {
                ListViewItem item = new ListViewItem(set.Key.ServerName);
                item.Tag = set;
                //item.SubItems.Add(set.Key.ServerName);
                item.SubItems.Add(set.Value.MultiDbTextConfig.Length.ToString());
                lstLoad.Items.Add(item);

            }

            foreach (string val in lstUnassignedDatabaseServers)
                lstDatabase.Items.Add(val);

            foreach (string val in lstUntaskedExecutionServers)
                lstUntasked.Items.Add(val);

        }

        private void ExecutionDistributionForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                this.Close();
        }

        private void viewAssignedServersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lstLoad.SelectedItems.Count == 0)
                return;

            KeyValuePair<ServerConfigData, BuildSettings> set = (KeyValuePair<ServerConfigData, BuildSettings>)lstLoad.SelectedItems[0].Tag;
            ServerDistributionListForm frmDist = new ServerDistributionListForm(set.Key, set.Value);
            frmDist.ShowDialog();

        }
    }
}
