using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using SqlBuildManager.ServiceClient;
namespace SqlSync.SqlBuild.Remote
{
    public partial class ExecutionServerForm : Form
    {
        private string currentCfgFileName = string.Empty;
        private RemoteExecutionServerConfig currentConfig = new RemoteExecutionServerConfig();
        BindingList<string> currentServers = new BindingList<string>();
        public ExecutionServerForm()
        {
            InitializeComponent();
        }

        public BindingList<string> SelectedExecutionServers
        {
            get
            {
                if (currentServers != null)
                    return currentServers;
                else
                    return new BindingList<string>();
            }
        }

        private void ExecutionServerForm_Load(object sender, EventArgs e)
        {
            //string homePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\";
            //if (File.Exists(homePath + RemoteServerManager.DefaultConfigName))
            //{
            //    LoadConfiguration(homePath + RemoteServerManager.DefaultConfigName);
            //}
            if (SqlSync.Properties.Settings.Default.LastRemoteExecutionConfigFile != null && 
                File.Exists(SqlSync.Properties.Settings.Default.LastRemoteExecutionConfigFile))
            {
                if (LoadConfiguration(SqlSync.Properties.Settings.Default.LastRemoteExecutionConfigFile))
                    BindConfigToList();
            }
            else
            {
                splitContainer1.Enabled = false;
                btnUseGroup.Enabled = false;
                toolTip1.SetToolTip(splitContainer1, "Open an existing configuration or create a new one to start editing");
            }

        }
        private bool LoadConfiguration(string path)
        {
            try
            {
                using (StreamReader sr = new StreamReader(path))
                {
                    XmlSerializer serializer = new XmlSerializer(typeof(RemoteExecutionServerConfig));
                    object obj = serializer.Deserialize(sr);
                    this.currentConfig = (RemoteExecutionServerConfig)obj;
                }
                return true;    
            }
            catch
            {
                return false;
            }
        }
        private bool SaveConfiguration(string path)
        {
            try
            {
                XmlSerializer xmlS = new XmlSerializer(typeof(RemoteExecutionServerConfig));
                using (XmlTextWriter tw = new XmlTextWriter(path, Encoding.UTF8))
                {
                    tw.Indentation = 4;
                    tw.Formatting = System.Xml.Formatting.Indented;
                    xmlS.Serialize(tw, currentConfig);
                    tw.Close();
                }
            }
            catch
            {
                return false;
            }
            return true;
        }
        private void BindConfigToList()
        {
            lstServers.Items.Clear();
            dgvRemoteServers.Rows.Clear();
            dgvRemoteServers.Invalidate();
            txtDescription.Text = string.Empty;

            if (this.currentConfig == null || this.currentConfig.ServerSet == null && this.currentConfig.ServerSet.Length == 0)
                return;


            foreach (ServerSet server in this.currentConfig.ServerSet)
            {
                ListViewItem item = new ListViewItem(server.Name);
                item.Tag = server;
                lstServers.Items.Add(item);
            }
            btnUseGroup.Enabled = false;
            splitContainer1.Enabled = true;
            splitContainer1.Panel2.Enabled = false;
            toolTip1.SetToolTip(splitContainer1.Panel2, "Select a server set to the left to allow editing");


        }

        private void lstServers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstServers.SelectedItems.Count == 0)
                return;

            toolTip1.SetToolTip(splitContainer1.Panel2, "");

            SwitchSelectedServer(lstServers.SelectedItems[0].Index);

        }
        private void SwitchSelectedServer(int listViewItemIndex)
        {
            splitContainer1.Panel2.Enabled = true;
            btnUseGroup.Enabled = true;
            dgvRemoteServers.Rows.Clear();
            for (int i = 0; i < lstServers.Items.Count; i++)
            {
                if (i == listViewItemIndex)
                    lstServers.Items[i].BackColor = Color.LightSteelBlue;
                else
                    lstServers.Items[i].BackColor = SystemColors.Window;
            }

            //Change the MultiDbPage Control
            ServerSet server = (ServerSet)lstServers.Items[listViewItemIndex].Tag;
            currentServers.Clear();

            if (server.ServerName != null)
            {
                foreach (string srv in server.ServerName)
                    currentServers.Add(srv);
            }

            foreach (string val in currentServers)
                dgvRemoteServers.Rows.Add(val);
            //dgvRemoteServers.DataSource = currentServers;
            dgvRemoteServers.Invalidate();
            txtDescription.Text = server.Description;
       }

        private void btnAddSet_Click(object sender, EventArgs e)
        {
            ServerSetNamePrompt frmPrompt = new ServerSetNamePrompt();
            if(DialogResult.OK == frmPrompt.ShowDialog())
            {
                string setName =frmPrompt.ServerSetName;
                List<ServerSet> tmp;
                if (this.currentConfig.ServerSet == null)
                    tmp = new List<ServerSet>();
                else
                    tmp = this.currentConfig.ServerSet.ToList();
                ServerSet x = new ServerSet();
                x.Name = setName;
                tmp.Add(x);
                this.currentConfig.ServerSet = tmp.ToArray();
                BindConfigToList();

            }
            frmPrompt.Dispose();
        }

        private void newConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            currentConfig = new RemoteExecutionServerConfig();
            lstServers.Items.Clear();
            dgvRemoteServers.DataSource = null;
            dgvRemoteServers.Invalidate();

            btnAddSet_Click(this, EventArgs.Empty);
        }

        private void loadConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == openFileDialog1.ShowDialog())
            {
                if (!LoadConfiguration(openFileDialog1.FileName))
                {
                    MessageBox.Show("I'm sorry, that does not appear to be a valid Remote Server Set Configuration File.\r\nPlease select another file and try again.", "Whoops!", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
                else
                {
                    this.currentCfgFileName = openFileDialog1.FileName;
                    BindConfigToList();
                    toolTip1.SetToolTip(splitContainer1, "");
                }
            }
            openFileDialog1.Dispose();
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.currentCfgFileName.Length > 0)
            {
                SaveConfiguration(this.currentCfgFileName);
            }
            else
            {
                saveConfigurationToolStripMenuItem_Click(sender, e);
            }
        }

        private void saveConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(DialogResult.OK == saveFileDialog1.ShowDialog())
            {
                SaveConfiguration(saveFileDialog1.FileName);
                this.currentCfgFileName = saveFileDialog1.FileName;
            }
            saveFileDialog1.Dispose();
        }

        private void dgvRemoteServers_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            BindingList<string> tmp = new BindingList<string>();
            foreach (DataGridViewRow row in dgvRemoteServers.Rows)
            {
                if(row.Cells[0].Value != null && row.Cells[0].Value.ToString().Length > 0)
                    tmp.Add(row.Cells[0].Value.ToString());
            }

            this.currentServers = tmp;
            if (lstServers.SelectedItems.Count > 0)
            {
                ((ServerSet)lstServers.SelectedItems[0].Tag).ServerName = this.currentServers.ToArray();
            }
        }

        private void btnUseGroup_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.OK;
            this.Close();

        }

        private void button2_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        private void txtDescription_TextChanged(object sender, EventArgs e)
        {
            if (lstServers.SelectedItems.Count == 0)
                return;

            ((ServerSet)lstServers.SelectedItems[0].Tag).Description = txtDescription.Text;
        }

        private void ExecutionServerForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.currentCfgFileName.Length > 0)
            {
                SqlSync.Properties.Settings.Default.LastRemoteExecutionConfigFile = this.currentCfgFileName;
                SqlSync.Properties.Settings.Default.Save();
            }
        }

        private void addNewGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnAddSet_Click(sender, e);
        }

        private void removeGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lstServers.SelectedItems.Count == 0)
                return;

            ServerSet server = (ServerSet)lstServers.SelectedItems[0].Tag;

            var toDelete = from s in this.currentConfig.ServerSet where s.Name == server.Name select s;

            this.currentConfig.ServerSet.ToList().Remove(toDelete.First());

            lstServers.Items.Remove(lstServers.SelectedItems[0]);
            lstServers.Invalidate();
        }


    }
}
