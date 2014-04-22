using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SqlSync.Connection;
using SqlSync.DbInformation;
using System.Xml;
using System.Xml.Serialization;
using SqlSync.MRU;
using System.IO;
namespace SqlSync.SqlBuild.MultiDb
{
    public partial class ConfigurationViaQueryForm : Form , SqlSync.MRU.IMRUClient
    {
        MRUManager mruManager = new MRUManager();
        private ConnectionData connData = null;
        private DatabaseList databaseList = null;
        private MultiDb.MultiDbData multiDbConfig = null;
        private bool showPreviewButton = false;
        private bool isDirty = false;
        private string savedMultiDbQFile = string.Empty;

        public string SavedMultiDbQFile
        {
            get { return savedMultiDbQFile; }
            set { savedMultiDbQFile = value; }
        }
        public MultiDb.MultiDbData MultiDbConfig
        {
            get { return multiDbConfig; }
            set { multiDbConfig = value; }
        }
        public ConfigurationViaQueryForm()
        {
            InitializeComponent();
        }
        public ConfigurationViaQueryForm(ConnectionData connData, DatabaseList dbList) : this()
        {
            if(connData.SQLServerName != null && connData.SQLServerName.Length > 0)
                this.connData = connData;
            this.databaseList = dbList;
            this.showPreviewButton = true;
        }
        public ConfigurationViaQueryForm(ConnectionData connData, DatabaseList dbList, string multiDbQFileName, bool showPreviewButton) : this(connData, dbList)
        {
            this.showPreviewButton = showPreviewButton;
            this.savedMultiDbQFile = multiDbQFileName;

        }

        private void mnuChangeSqlServer_Click(object sender, EventArgs e)
        {

            ConnectionForm frmConnect = new ConnectionForm("Sql Build Manager");
            DialogResult result = frmConnect.ShowDialog();
            if (result == DialogResult.OK)
            {
                this.connData = frmConnect.SqlConnection;
                this.databaseList = frmConnect.DatabaseList;
                this.lblServer.Text = this.connData.SQLServerName;


                this.SetDatabaseList(frmConnect.SqlConnection.SQLServerName, "");

                
            }
        }

        private void SetDatabaseList(string server, string targetDb)
        {
            this.databaseList =  InfoHelper.GetDatabaseList(new ConnectionData(server,""));

            this.ddDatabase.Items.Clear();
            for(int i=0;i<this.databaseList.Count;i++)
            {
                if (!this.databaseList[i].IsManuallyEntered)
                    this.ddDatabase.Items.Add(this.databaseList[i].DatabaseName);
            }

            for (int i = 0; i < this.ddDatabase.Items.Count; i++)
            {
                if (this.ddDatabase.Items[i].ToString().ToLower() == targetDb.ToLower())
                    this.ddDatabase.SelectedIndex = i;
            }


        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (PrecompileMultiDbData())
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private bool PrecompileMultiDbData()
        {
            if (this.connData == null)
            {
                MessageBox.Show("Please select a source server and database to execute against", "Connection Needed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }

            if (ddDatabase.SelectedItem == null)
            {
                MessageBox.Show("Please select a database to execute against", "Database Name Needed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return false;
            }
            connData.DatabaseName = ddDatabase.SelectedItem.ToString();
            connData.SQLServerName = lblServer.Text.Trim();
            string message;
            this.multiDbConfig = MultiDbHelper.CreateMultiDbConfigFromQuery(connData, rtbSqlScript.Text, out message);
            if (this.multiDbConfig == null)
            {
                MessageBox.Show("Unable to create a configuration from your query. Please confirm your database settings and query.\r\n\r\nDatabase Message:\r\n" + message, "Unable to create", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
            return true;
        }
        private void ConfigurationViaQueryForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }

        }

        private void ConfigurationViaQueryForm_Load(object sender, EventArgs e)
        {
            if(this.databaseList != null && this.databaseList.Count > 0)
                foreach (DatabaseItem item in this.databaseList)
                    if (!item.IsManuallyEntered)
                        this.ddDatabase.Items.Add(item.DatabaseName);

            if(this.ddDatabase.Items.Count > 0)
                this.ddDatabase.SelectedIndex = 0;

            if (this.connData != null)
                this.lblServer.Text = this.connData.SQLServerName;

            if (this.showPreviewButton)
            {
                button1.Text = "Close";
                btnPreview.Visible = true;
            }

            this.InitMRU();
            this.isDirty = false;

            if (this.savedMultiDbQFile != null && this.savedMultiDbQFile.Length > 0)
                LoadQueryConfig(this.savedMultiDbQFile);
        }

        private void ConfigurationViaQueryForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!this.isDirty)
                return;

            if (DialogResult.Yes == MessageBox.Show("Do you want to save the query settings?", "Save Query?", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                saveQueryConfigurationToolStripMenuItem_Click(null, EventArgs.Empty);
            }
        }


        private void openSavedQueryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == openFileDialog1.ShowDialog())
            {
                LoadQueryConfig(openFileDialog1.FileName);
                this.SavedMultiDbQFile = openFileDialog1.FileName;
                this.isDirty = false;
            }
        }


         #region IMRUClient Members

        public void OpenMRUFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                LoadQueryConfig(fileName);
            }
        }

        private void LoadQueryConfig(string fileName)
        {
            MultiDbQueryConfig cfg = MultiDbHelper.LoadMultiDbQueryConfiguration(fileName);
            if (cfg != null)
            {
                lblServer.Text = cfg.SourceServer;
                this.SetDatabaseList(cfg.SourceServer, cfg.Database);
                this.mruManager.Add(fileName);
                rtbSqlScript.Text = cfg.Query;
            }

            this.isDirty = false;
        }

        #endregion
        /// <summary>
        /// Initializes the "Recent Files" menu option off the "Actions" menu
        /// </summary>
        private void InitMRU()
        {
            this.mruManager = new MRUManager();
            this.mruManager.Initialize(
                this,                              // owner form
                actionToolStripMenuItem,
                recentFilesToolStripMenuItem,                        // Recent Files menu item
                @"Software\Michael McKechney\Sql Sync\Multi Db Config Query"); // Registry path to keep MRU list
            this.mruManager.MaxDisplayNameLength = 40;
        }

        private void saveQueryConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == saveFileDialog1.ShowDialog())
            {
                MultiDbQueryConfig cfg = new MultiDbQueryConfig(this.connData.SQLServerName, this.ddDatabase.SelectedItem.ToString(), this.rtbSqlScript.Text);
                MultiDbHelper.SaveMultiDbQueryConfiguration(saveFileDialog1.FileName, cfg);

                this.mruManager.Add(saveFileDialog1.FileName);
                this.savedMultiDbQFile = saveFileDialog1.FileName;
                this.isDirty = false;
            }
        }

        private void rtbSqlScript_TextChanged(object sender, EventArgs e)
        {
            this.isDirty = true;
        }

        private void ddDatabase_SelectionChangeCommitted(object sender, EventArgs e)
        {
            this.isDirty = true;
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            if (PrecompileMultiDbData())
            {
                Remote.ServerDistributionListForm frmDist = new SqlSync.SqlBuild.Remote.ServerDistributionListForm(this.multiDbConfig, false);
                frmDist.ShowDialog();
                frmDist.Dispose();
            }

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

       
    }
}
