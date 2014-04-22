using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SqlSync.DbInformation;
using System.IO;
using System.Xml.Serialization;
using SqlSync.MRU;
using SqlSync.Connection;
namespace SqlSync.SqlBuild.MultiDb
{
    public partial class MultiDbRunForm : Form, IMRUClient    
    {
        bool isTransactional = true;
        SqlSyncBuildData buildData;
        string projectFilePath;
        bool valuesChanged = false;
        bool runAsTrial = true;
        public bool ValuesChanged
        {
            get { return valuesChanged; }
            set
            {
                valuesChanged = value;
                if (statChanged != null)
                {
                    if (valuesChanged)
                    {
                        statChanged.Text = "Changed";
                        statChanged.ForeColor = Color.Red;
                    }
                    else
                    {
                        statChanged.Text = "Unchanged";
                        statChanged.ForeColor = Color.Green;
                    }
                }

            }
        }
        string loadedFileName = string.Empty;

        public string LoadedFileName
        {
            get { return loadedFileName; }
            set { loadedFileName = value; }
        }
        MRUManager mruManager;
        public MultiDbRunForm()
        {
            InitializeComponent();
        }
         string server;
        List<string> defaultDatabases;
        DatabaseList databaseList;
        private string buildZipFileName = string.Empty;

        public MultiDbRunForm(string server, List<string> defaultDatabases, DatabaseList databaseList, string buildZipFileName, string projectFilePath,ref SqlSyncBuildData buildData)
            : this()
        {
            //perform real copy to prevent data slipping back into main form
            string[] tmp = new string[defaultDatabases.Count];
            defaultDatabases.CopyTo(tmp);
            this.defaultDatabases = new List<string>(tmp);
            this.databaseList = databaseList;
            this.server = server;
            this.DialogResult = DialogResult.Cancel;
            this.buildZipFileName = buildZipFileName;
            this.buildData = buildData;
            this.projectFilePath = projectFilePath;
        }

        private MultiDbData runConfiguration = null;
        public MultiDbData RunConfiguration
        {
            get
            {
                runConfiguration =  GetServerDataCollection();
                runConfiguration.RunAsTrial = this.runAsTrial;
                runConfiguration.IsTransactional = this.isTransactional;
                return runConfiguration;
            }
        }

        private void MultiDbRunForm_Load(object sender, EventArgs e)
        {
            this.InitMRU();
            this.lstServers.Items.Clear();
            this.splitContainer1.Panel2.Controls.Clear();

            if (this.buildData == null)
            {
                tryBuildUsingCurrentConfigurationToolStripMenuItem.Enabled = false;
                runBuildUsingCurrentConfigurationCommitToolStripMenuItem.Enabled = false;
                runBuildUsingCurrentConfigurationWithoutTransactionsToolStripMenuItem.Enabled = false;
            }

            this.AddNewServerItem(this.server, this.databaseList);
            //LoadDatabaseData(this.server, this.databaseList);
        }

        private void PopulateServerList(List<ServerData> srvData)
        {
            this.lstServers.Items.Clear();
            foreach (ServerData dat in srvData)
            {
                ListViewItem item = new ListViewItem(dat.ServerName);
                item.Tag = dat;
                lstServers.Items.Add(item);
            }

        }

        void pg_ValueChanged(object sender, EventArgs e)
        {
            this.ValuesChanged = true;
        }

        void pg_ServerRemoved(object sender, string newServerName)
        {
            foreach (ListViewItem item in lstServers.Items)
            {
                if (item.Text == newServerName)
                {
                    lstServers.Items.Remove(item);
                     this.ValuesChanged = true;
                    break;
                }
            }
            this.splitContainer1.Panel2.Controls.Remove((Control)sender);
        }

      
      private void addAnotherServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConnectionForm frmConnect = new ConnectionForm("Multi-Database Config");
            if(DialogResult.OK == frmConnect.ShowDialog())
            {
                this.AddNewServerItem(frmConnect.SqlConnection.SQLServerName, frmConnect.DatabaseList);
            }
           
        }

        private void runBuildUsingCurrentConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.runConfiguration = GetServerDataCollection();
            if (this.runConfiguration == null)
                return;


            if (!MultiDbHelper.ValidateMultiDatabaseData(this.runConfiguration))
            {
                MessageBox.Show("One or more scripts is missing a default or target override database setting.\r\nRun has been halted. Please correct the error and try again", "Missing Database setting", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (this.runConfiguration.Count == 1 && this.runConfiguration[0].OverrideSequence.Count == 0)
            {
                MessageBox.Show("Please configure a run order for at least one database item", "Unable to run", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            this.runConfiguration.RunAsTrial = true;
            this.runAsTrial = true;
            if (this.valuesChanged && ConfirmSave(ConfirmType.Run))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void runBuildUsingCurrentConfigurationCommitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.runConfiguration = GetServerDataCollection();
            if (this.runConfiguration == null)
                return;

            if (this.runConfiguration.Count == 1 && this.runConfiguration[0].OverrideSequence.Count == 0)
            {
                MessageBox.Show("Please configure a run order for at least one database item", "Unable to run", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            this.runConfiguration.RunAsTrial = false;
            this.runAsTrial = false;
            if (this.valuesChanged && ConfirmSave(ConfirmType.Run))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }

        }
        private bool ConfirmSave(ConfirmType type)
        {
            string msg = string.Empty;
            switch (type)
            {
                case ConfirmType.Close:
                    msg = "Save configuration prior to closing form?";
                    break;
                case ConfirmType.Run:
                    msg = "Save configuration prior to closing form and running build?";
                    break;
                case ConfirmType.CommandLine:
                    msg = "Save configuration prior to constructing Command Line?";
                    break;

            }
            DialogResult result = MessageBox.Show(msg, "Save Configuration", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
            if(result == DialogResult.Yes)
            {
                saveConfigurationToolStripMenuItem_Click(null, EventArgs.Empty);
            }
            if (result == DialogResult.Cancel)
                return false;
            else
                return true;
        }
        private void saveConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (null == GetServerDataCollection())
                return;

             
            if (this.loadedFileName.Length > 0)
                saveFileDialog1.FileName = this.loadedFileName;

            if (DialogResult.OK == saveFileDialog1.ShowDialog())
            {
                MultiDbData data = this.RunConfiguration;
                using (TextWriter writer = new StreamWriter(saveFileDialog1.FileName))
                {
                    XmlSerializer xmlS = new XmlSerializer(typeof(MultiDbData));
                    xmlS.Serialize(writer, data);
                }
                this.mruManager.Add(saveFileDialog1.FileName);
                this.loadedFileName = saveFileDialog1.FileName;

            }
            this.ValuesChanged = false;

        }
        private MultiDbData GetServerDataCollection()
        {
            try
            {
                SyncFormMultiDbPageData();
                MultiDbData mult = new MultiDbData();
                foreach (ListViewItem item in this.lstServers.Items)
                {
                    mult.Add((ServerData)item.Tag);
                }
                return mult;
            }
            catch (Exception)
            {
                MessageBox.Show("There is an error in your configuration.\r\nPlease make sure that you do not have any duplicate run order identifiers for the same default database", "Configration Error", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return null;
            }
        }

        private void loadConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.FileName = "";
            if (DialogResult.OK == openFileDialog1.ShowDialog())
            {
                string fileName = openFileDialog1.FileName;
                this.loadedFileName = fileName;
                this.mruManager.Add(fileName);
                bgLoadCfg.RunWorkerAsync(fileName);
                this.ValuesChanged = false;
            }
            openFileDialog1.Dispose();
        }
       

        #region IMRUClient Members

        public void OpenMRUFile(string fileName)
        {
            if (File.Exists(fileName))
                bgLoadCfg.RunWorkerAsync(fileName);
        }
        private void InitMRU()
        {
            this.mruManager = new MRUManager();
            this.mruManager.Initialize(
                this,                              // owner form
                mnuActionMain,
                mnuFileMRU,                        // Recent Files menu item
                @"Software\Michael McKechney\Sql Sync\Multi Db Run"); // Registry path to keep MRU list
            this.mruManager.MaxDisplayNameLength = 40;
        }
        #endregion


        private void MultiDbRunForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (this.ValuesChanged)
            {
                ConfirmSave(ConfirmType.CommandLine);
            }
        }

        private void bgLoadCfg_DoWork(object sender, DoWorkEventArgs e)
        {

            List<ServerData> svrDataList = new List<ServerData>();

            MultiDbData data = null;
            BackgroundWorker bg = (BackgroundWorker)sender;
            if (e.Argument is string)
            {

                bg.ReportProgress(-10, "Initializing");
                string fileName = e.Argument.ToString();
                bg.ReportProgress(10, "Loading " + Path.GetFileName(fileName) + "...");

                data = MultiDbHelper.DeserializeMultiDbConfiguration(fileName);

                if (data == null) //maybe have a flat .cfg file??
                {
                    data = MultiDbHelper.ImportMultiDbTextConfig(fileName);
                }
            }
            else if (e.Argument is MultiDbData)
                data = (MultiDbData)e.Argument;

            if (data != null)
            {
                bg.ReportProgress(0, "Applying configuration... ");
                foreach (ServerData srv in data)
                {
                    //We need to get the list of databases for this server if it doesn't match the current connection
                    if (srv.ServerName != this.server)
                    {
                        bg.ReportProgress(10, "Retrieving database list from " + srv.ServerName);
                        srv.Databases = InfoHelper.GetDatabaseList(new ConnectionData(srv.ServerName, "master"));
                    }
                    else
                    {
                        srv.Databases = this.databaseList;
                    }
                    svrDataList.Add(srv);
                }
                e.Result = svrDataList;
            }
        }

        private void bgLoadCfg_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;   

            if(e.UserState is string)
                statGeneral.Text = e.UserState.ToString();
        }

        private void bgLoadCfg_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {

            if (e.Result is List<ServerData>)
            {
                statGeneral.Text = "Configuring server data...";
                List<ServerData> lst = (List<ServerData>)e.Result;

                if (lst.Count == 0)
                {
                    MessageBox.Show("No server data loaded. Please check your configuration and try loading again", "No Servers found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    statGeneral.Text = "Unable to load configuration.";
                    this.splitContainer1.Panel2.Controls.Clear();
                    this.Cursor = Cursors.Default;
                    return;
                }

                this.splitContainer1.Panel2.Controls.Clear();
                this.PopulateServerList(lst);
                this.lstServers.Items[0].Selected = true;
            }

            if (this.loadedFileName.Length < 50)
                statGeneral.Text = this.loadedFileName;
            else
                statGeneral.Text = "..." + this.loadedFileName.Substring(this.loadedFileName.Length - 50);

            this.Cursor = Cursors.Default;
            this.ValuesChanged = false;
        }

        enum ConfirmType
        {
            Run,
            Close,
            CommandLine
        }

        private void constructCommandLineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (valuesChanged)
                ConfirmSave(ConfirmType.CommandLine);

            CommandLineBuilderForm frmCmd = new CommandLineBuilderForm(this.buildZipFileName, this.loadedFileName);
            frmCmd.Show();
        }

        private void runBuildUsingCurrentConfigurationWithoutTransactionsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.runConfiguration = GetServerDataCollection();
            if (this.runConfiguration == null)
                return;

            if (this.runConfiguration.Count == 1 && this.runConfiguration[0].OverrideSequence.Count == 0)
            {
                MessageBox.Show("Please configure a run order for at least one database item", "Unable to run", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string message = "WARNING!\r\nWith this selection, you are disabling the transaction handling of Sql Build Manager.\r\nIn the event of a script failure, your scripts will NOT be rolled back\r\nand yout database will be left in an inconsistent state!\r\nAre you certain you want to continue?";
            if (DialogResult.No == MessageBox.Show(message, "Are you sure you want this?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2))
            {
                return;
            }

            this.runConfiguration.RunAsTrial = false;
            this.runAsTrial = false;

            this.runConfiguration.IsTransactional = false;
            this.isTransactional = false;
            if (this.valuesChanged && ConfirmSave(ConfirmType.Run))
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            else
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
        }

        private void generateScriptStatusReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MultiDbData multiDbData = GetServerDataCollection();
            if (this.buildData == null)
            {
                MessageBox.Show("You do not have a Sql Build Project loaded. There's nothing to get status for.\r\nGo back and load an SBM or SBX file and try again.", "Nothing to Check", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            StatusReportForm frmStat = new StatusReportForm(this.buildData, multiDbData, this.projectFilePath,this.buildZipFileName);
            frmStat.ShowDialog();

        }

        private void generateObjectComparisonReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MultiDbData multiDbData = GetServerDataCollection();

            ObjectComparisonReportForm frmStat = new ObjectComparisonReportForm(multiDbData);
            frmStat.ShowDialog();

        }

        private void adHocQueryExecutionToolStripMenuItem_Click(object sender, EventArgs e)
        {
              MultiDbData multiDbData = GetServerDataCollection();
              AdHocQueryExecution frmAdHoc = new AdHocQueryExecution(multiDbData);
              frmAdHoc.ShowDialog();
        }

        private void createConfigurationViaQueryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try
            {
                ConnectionData connData = new ConnectionData(this.server, "");
                ConfigurationViaQueryForm frmQuery = new ConfigurationViaQueryForm(connData, this.databaseList);
                if (DialogResult.OK == frmQuery.ShowDialog())
                {
                    if (frmQuery.MultiDbConfig != null)
                    {
                        bgLoadCfg.RunWorkerAsync(frmQuery.MultiDbConfig);
                    }
                }

                frmQuery.Dispose();
            }
            catch (Exception)
            {
            }

        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SqlSync.Utility.OpenManual("TargetingMultipleServersandDatabases");
        }

        private void lstServers_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (lstServers.SelectedItems.Count == 0)
                return;

            SwitchSelectedServer(lstServers.SelectedItems[0].Index);
          
        }
        private void AddNewServerItem(string serverName, DatabaseList dbList)
        {
            ListViewItem item = new ListViewItem(serverName);
            ServerData dat = new ServerData();
            dat.ServerName = serverName;
            dat.Databases = dbList;
            item.Tag = dat;

            lstServers.Items.Add(item);
            SwitchSelectedServer(item.Index);
        }
        private void SwitchSelectedServer(int listViewItemIndex)
        {
            //Sync any updated data with the ListView item tag
            if (this.splitContainer1.Panel2.Controls.Count > 0)
            {
                SyncFormMultiDbPageData();
                this.splitContainer1.Panel2.Controls[0].Dispose();
                this.splitContainer1.Panel2.Controls.Clear();
                
            }

            for (int i = 0; i < lstServers.Items.Count; i++)
            {
                if (i == listViewItemIndex)
                    lstServers.Items[i].BackColor = Color.LightSteelBlue;
                else
                    lstServers.Items[i].BackColor = SystemColors.Window;
            }

            //Change the MultiDbPage Control
            ServerData dat = (ServerData)lstServers.Items[listViewItemIndex].Tag;
            MultiDbPage page = new MultiDbPage(dat, this.defaultDatabases);
            page.ServerRemoved += new ServerChangedEventHandler(pg_ServerRemoved);
            page.DataBind();
            page.ValueChanged += new EventHandler(pg_ValueChanged);
            page.Dock = DockStyle.Fill;
            this.splitContainer1.Panel2.Controls.Add(page);
        }
        private void SyncFormMultiDbPageData()
        {
            if (this.splitContainer1.Panel2.Controls[0] is MultiDbPage)
            {
                MultiDbPage current = (MultiDbPage)this.splitContainer1.Panel2.Controls[0];
                foreach (ListViewItem item in lstServers.Items)
                {
                    if (item.Text == current.ServerName)
                    {
                        item.Tag = current.ServerData;
                        break;
                    }
                }
            }
        }

        private void buildValidationReportToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MultiDbData multiDbData = GetServerDataCollection();
            BuildValidationForm fbmBldValid = new BuildValidationForm(multiDbData);
            fbmBldValid.ShowDialog();
        }

    }
}
