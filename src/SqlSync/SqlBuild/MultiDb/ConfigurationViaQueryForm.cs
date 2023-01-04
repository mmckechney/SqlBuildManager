using SqlSync.Connection;
using SqlSync.DbInformation;
using SqlSync.MRU;
using System;
using System.IO;
using System.Windows.Forms;
namespace SqlSync.SqlBuild.MultiDb
{
    public partial class ConfigurationViaQueryForm : Form, SqlSync.MRU.IMRUClient
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
            if (connData.SQLServerName != null && connData.SQLServerName.Length > 0)
                this.connData = connData;
            databaseList = dbList;
            showPreviewButton = true;
        }
        public ConfigurationViaQueryForm(ConnectionData connData, DatabaseList dbList, string multiDbQFileName, bool showPreviewButton) : this(connData, dbList)
        {
            this.showPreviewButton = showPreviewButton;
            savedMultiDbQFile = multiDbQFileName;

        }

        private void mnuChangeSqlServer_Click(object sender, EventArgs e)
        {

            ConnectionForm frmConnect = new ConnectionForm("Sql Build Manager");
            DialogResult result = frmConnect.ShowDialog();
            if (result == DialogResult.OK)
            {
                connData = frmConnect.SqlConnection;
                databaseList = frmConnect.DatabaseList;
                lblServer.Text = connData.SQLServerName;


                SetDatabaseList(frmConnect.SqlConnection.SQLServerName, "");


            }
        }

        private void SetDatabaseList(string server, string targetDb)
        {
            databaseList = InfoHelper.GetDatabaseList(new ConnectionData(server, ""));

            ddDatabase.Items.Clear();
            for (int i = 0; i < databaseList.Count; i++)
            {
                if (!databaseList[i].IsManuallyEntered)
                    ddDatabase.Items.Add(databaseList[i].DatabaseName);
            }

            for (int i = 0; i < ddDatabase.Items.Count; i++)
            {
                if (ddDatabase.Items[i].ToString().ToLower() == targetDb.ToLower())
                    ddDatabase.SelectedIndex = i;
            }


        }

        private void button1_Click(object sender, EventArgs e)
        {

            if (PrecompileMultiDbData())
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private bool PrecompileMultiDbData()
        {
            if (connData == null)
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
            multiDbConfig = MultiDbHelper.CreateMultiDbConfigFromQuery(connData, rtbSqlScript.Text, out message);
            if (multiDbConfig == null)
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
                DialogResult = DialogResult.Cancel;
                Close();
            }

        }

        private void ConfigurationViaQueryForm_Load(object sender, EventArgs e)
        {
            if (databaseList != null && databaseList.Count > 0)
                foreach (DatabaseItem item in databaseList)
                    if (!item.IsManuallyEntered)
                        ddDatabase.Items.Add(item.DatabaseName);

            if (ddDatabase.Items.Count > 0)
                ddDatabase.SelectedIndex = 0;

            if (connData != null)
                lblServer.Text = connData.SQLServerName;

            if (showPreviewButton)
            {
                button1.Text = "Close";
                btnPreview.Visible = true;
            }

            InitMRU();
            isDirty = false;

            if (savedMultiDbQFile != null && savedMultiDbQFile.Length > 0)
                LoadQueryConfig(savedMultiDbQFile);
        }

        private void ConfigurationViaQueryForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!isDirty)
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
                SavedMultiDbQFile = openFileDialog1.FileName;
                isDirty = false;
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
                SetDatabaseList(cfg.SourceServer, cfg.Database);
                mruManager.Add(fileName);
                rtbSqlScript.Text = cfg.Query;
            }

            isDirty = false;
        }

        #endregion
        /// <summary>
        /// Initializes the "Recent Files" menu option off the "Actions" menu
        /// </summary>
        private void InitMRU()
        {
            mruManager = new MRUManager();
            mruManager.Initialize(
                this,                              // owner form
                actionToolStripMenuItem,
                recentFilesToolStripMenuItem,                        // Recent Files menu item
                @"Software\Michael McKechney\Sql Sync\Multi Db Config Query"); // Registry path to keep MRU list
            mruManager.MaxDisplayNameLength = 40;
        }

        private void saveQueryConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == saveFileDialog1.ShowDialog())
            {
                MultiDbQueryConfig cfg = new MultiDbQueryConfig(connData.SQLServerName, ddDatabase.SelectedItem.ToString(), rtbSqlScript.Text);
                MultiDbHelper.SaveMultiDbQueryConfiguration(saveFileDialog1.FileName, cfg);

                mruManager.Add(saveFileDialog1.FileName);
                savedMultiDbQFile = saveFileDialog1.FileName;
                isDirty = false;
            }
        }

        private void rtbSqlScript_TextChanged(object sender, EventArgs e)
        {
            isDirty = true;
        }

        private void ddDatabase_SelectionChangeCommitted(object sender, EventArgs e)
        {
            isDirty = true;
        }

        //private void btnPreview_Click(object sender, EventArgs e)
        //{
        //    if (PrecompileMultiDbData())
        //    {
        //        Remote.ServerDistributionListForm frmDist = new SqlSync.SqlBuild.Remote.ServerDistributionListForm(this.multiDbConfig, false);
        //        frmDist.ShowDialog();
        //        frmDist.Dispose();
        //    }

        //}

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Close();
        }


    }
}
