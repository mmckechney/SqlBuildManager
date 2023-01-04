using SqlSync.Connection;
using SqlSync.Constants;
using SqlSync.DbInformation;
using SqlSync.ObjectScript;
using SqlSync.SqlBuild;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
namespace SqlSync
{
    /// <summary>
    /// Summary description for Form1.
    /// </summary>
    public class SyncForm : System.Windows.Forms.Form
    {
        private DatabaseList databaseList;
        private const string connectionTitle = "Sql Schema Scripting";
        private ConnectionData connData = null;
        private ObjectScriptHelper helper = null;
        private System.Windows.Forms.FolderBrowserDialog fldrStartingDir;
        private System.Windows.Forms.ColumnHeader colFile;
        private System.Windows.Forms.ColumnHeader colStatus;
        private System.Windows.Forms.StatusStrip statusBar1;
        private System.Windows.Forms.ToolStripStatusLabel statStatus;
        private System.Windows.Forms.ColumnHeader colFullPath;
        private System.Windows.Forms.ContextMenuStrip contextMenu1;
        private System.Windows.Forms.ToolStripMenuItem mnuNotePad;
        private System.Windows.Forms.GroupBox grpScripting;
        private System.Windows.Forms.ListView lstStatus;
        private System.Windows.Forms.GroupBox grpDirectory;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckBox chkCombineTableObjects;
        private System.Windows.Forms.CheckBox chkZip;
        private System.ComponentModel.IContainer components;
        private System.Windows.Forms.SaveFileDialog saveAutoScriptFileDialog1;
        private System.Windows.Forms.MenuStrip mainMenu1;
        private System.Windows.Forms.ToolStripMenuItem menuItem2;
        private System.Windows.Forms.ToolStripMenuItem mnuChangeConnection;
        private System.Windows.Forms.ToolStripMenuItem menuItem3;
        private System.Windows.Forms.ToolStripMenuItem menuItem5;
        private System.Windows.Forms.ToolStripMenuItem mnuLookUpTables;
        private System.Windows.Forms.ToolStripMenuItem mnuSqlBuildManager;
        private System.Windows.Forms.ToolStripMenuItem mnuAutoScript;
        private System.Windows.Forms.ComboBox ddDatabase;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox ddScriptType;
        private System.Windows.Forms.ToolStripMenuItem mnuDataDump;
        private SqlSync.SettingsControl settingsControl1;
        private System.Windows.Forms.ToolStripMenuItem mnuDestination;
        private BackgroundWorker bgScripting;
        private Button btnGo;
        private Button btnCancel;
        private CheckBox chkIncludeHeaders;
        private Panel panel1;

        public SyncForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //

            //
        }

        public SyncForm(ConnectionData connData) : this()
        {
            this.connData = connData;
        }

        public SyncForm(string server, string userid, string password, string databaseName, string destinationDirectory)
        {
            InitializeComponent();


            //Set the connection data
            connData = new ConnectionData();
            connData.DatabaseName = databaseName;
            connData.SQLServerName = server;
            connData.Password = password;
            connData.UserId = userid;
            connData.AuthenticationType = AuthenticationType.Password;
            connData.StartingDirectory = destinationDirectory;

            //Set the messagebox
            StringBuilder sb = new StringBuilder();
            sb.Append("Server:" + connData.SQLServerName + "\r\n");
            sb.Append("DatabaseName:" + connData.DatabaseName + "\r\n");
            sb.Append("UserId:" + connData.UserId + "\r\n");
            sb.Append("Password:" + connData.Password + "\r\n");
            sb.Append("StartingDirectory:" + connData.StartingDirectory + "\r\n");
            MessageBox.Show(sb.ToString(), "Initilized");



            ObjectScriptingConfigData cfgData = new ObjectScriptingConfigData(true, true, true, true, false, connData);
            bgScripting.RunWorkerAsync(cfgData);
        }
        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SyncForm));
            grpDirectory = new System.Windows.Forms.GroupBox();
            label1 = new System.Windows.Forms.Label();
            ddDatabase = new System.Windows.Forms.ComboBox();
            fldrStartingDir = new System.Windows.Forms.FolderBrowserDialog();
            grpScripting = new System.Windows.Forms.GroupBox();
            chkIncludeHeaders = new System.Windows.Forms.CheckBox();
            btnCancel = new System.Windows.Forms.Button();
            btnGo = new System.Windows.Forms.Button();
            ddScriptType = new System.Windows.Forms.ComboBox();
            chkZip = new System.Windows.Forms.CheckBox();
            chkCombineTableObjects = new System.Windows.Forms.CheckBox();
            lstStatus = new System.Windows.Forms.ListView();
            colFile = new System.Windows.Forms.ColumnHeader();
            colStatus = new System.Windows.Forms.ColumnHeader();
            colFullPath = new System.Windows.Forms.ColumnHeader();
            contextMenu1 = new System.Windows.Forms.ContextMenuStrip(components);
            mnuNotePad = new System.Windows.Forms.ToolStripMenuItem();
            statusBar1 = new System.Windows.Forms.StatusStrip();
            statStatus = new System.Windows.Forms.ToolStripStatusLabel();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            saveAutoScriptFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            mainMenu1 = new System.Windows.Forms.MenuStrip();
            menuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            mnuDestination = new System.Windows.Forms.ToolStripMenuItem();
            mnuChangeConnection = new System.Windows.Forms.ToolStripMenuItem();
            menuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            mnuAutoScript = new System.Windows.Forms.ToolStripMenuItem();
            menuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            mnuLookUpTables = new System.Windows.Forms.ToolStripMenuItem();
            mnuSqlBuildManager = new System.Windows.Forms.ToolStripMenuItem();
            mnuDataDump = new System.Windows.Forms.ToolStripMenuItem();
            bgScripting = new System.ComponentModel.BackgroundWorker();
            settingsControl1 = new SqlSync.SettingsControl();
            panel1 = new System.Windows.Forms.Panel();
            grpDirectory.SuspendLayout();
            grpScripting.SuspendLayout();
            contextMenu1.SuspendLayout();
            statusBar1.SuspendLayout();
            mainMenu1.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // grpDirectory
            // 
            grpDirectory.Controls.Add(label1);
            grpDirectory.Controls.Add(ddDatabase);
            grpDirectory.Dock = System.Windows.Forms.DockStyle.Top;
            grpDirectory.FlatStyle = System.Windows.Forms.FlatStyle.System;
            grpDirectory.Location = new System.Drawing.Point(0, 92);
            grpDirectory.Name = "grpDirectory";
            grpDirectory.Size = new System.Drawing.Size(800, 59);
            grpDirectory.TabIndex = 0;
            grpDirectory.TabStop = false;
            grpDirectory.Text = "1) Select a Database";
            // 
            // label1
            // 
            label1.Location = new System.Drawing.Point(19, 22);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(77, 20);
            label1.TabIndex = 5;
            label1.Text = "Database:";
            // 
            // ddDatabase
            // 
            ddDatabase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            ddDatabase.Location = new System.Drawing.Point(106, 20);
            ddDatabase.Name = "ddDatabase";
            ddDatabase.Size = new System.Drawing.Size(145, 23);
            ddDatabase.TabIndex = 0;
            ddDatabase.SelectionChangeCommitted += new System.EventHandler(ddDatabase_SelectionChangeCommitted);
            // 
            // grpScripting
            // 
            grpScripting.Controls.Add(chkIncludeHeaders);
            grpScripting.Controls.Add(btnCancel);
            grpScripting.Controls.Add(btnGo);
            grpScripting.Controls.Add(ddScriptType);
            grpScripting.Controls.Add(chkZip);
            grpScripting.Controls.Add(chkCombineTableObjects);
            grpScripting.Controls.Add(lstStatus);
            grpScripting.Dock = System.Windows.Forms.DockStyle.Fill;
            grpScripting.Enabled = false;
            grpScripting.FlatStyle = System.Windows.Forms.FlatStyle.System;
            grpScripting.Location = new System.Drawing.Point(0, 151);
            grpScripting.Name = "grpScripting";
            grpScripting.Size = new System.Drawing.Size(800, 207);
            grpScripting.TabIndex = 4;
            grpScripting.TabStop = false;
            grpScripting.Text = "2) Update Script Files from Database";
            // 
            // chkIncludeHeaders
            // 
            chkIncludeHeaders.Checked = true;
            chkIncludeHeaders.CheckState = System.Windows.Forms.CheckState.Checked;
            chkIncludeHeaders.Location = new System.Drawing.Point(599, 20);
            chkIncludeHeaders.Name = "chkIncludeHeaders";
            chkIncludeHeaders.Size = new System.Drawing.Size(173, 19);
            chkIncludeHeaders.TabIndex = 8;
            chkIncludeHeaders.Text = "Include Headers";
            toolTip1.SetToolTip(chkIncludeHeaders, "Adds scripts for Primary Keys, Foreign Keys, Indexes and Defaults in the same fil" +
        "e as the table create script");
            // 
            // btnCancel
            // 
            btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            btnCancel.Location = new System.Drawing.Point(668, 17);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(122, 29);
            btnCancel.TabIndex = 7;
            btnCancel.Text = "Cancel Scripting";
            btnCancel.UseVisualStyleBackColor = true;
            btnCancel.Click += new System.EventHandler(btnCancel_Click);
            // 
            // btnGo
            // 
            btnGo.Location = new System.Drawing.Point(262, 17);
            btnGo.Name = "btnGo";
            btnGo.Size = new System.Drawing.Size(48, 29);
            btnGo.TabIndex = 6;
            btnGo.Text = "Go";
            btnGo.UseVisualStyleBackColor = true;
            btnGo.Click += new System.EventHandler(btnGo_Click);
            // 
            // ddScriptType
            // 
            ddScriptType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            ddScriptType.Location = new System.Drawing.Point(19, 17);
            ddScriptType.Name = "ddScriptType";
            ddScriptType.Size = new System.Drawing.Size(231, 23);
            ddScriptType.TabIndex = 0;
            // 
            // chkZip
            // 
            chkZip.Checked = true;
            chkZip.CheckState = System.Windows.Forms.CheckState.Checked;
            chkZip.Location = new System.Drawing.Point(326, 20);
            chkZip.Name = "chkZip";
            chkZip.Size = new System.Drawing.Size(96, 19);
            chkZip.TabIndex = 2;
            chkZip.Text = "Zip Scripts ";
            toolTip1.SetToolTip(chkZip, "Adds scripts for Primary Keys, Foreign Keys, Indexes and Defaults in the same fil" +
        "e as the table create script");
            // 
            // chkCombineTableObjects
            // 
            chkCombineTableObjects.Checked = true;
            chkCombineTableObjects.CheckState = System.Windows.Forms.CheckState.Checked;
            chkCombineTableObjects.Location = new System.Drawing.Point(432, 20);
            chkCombineTableObjects.Name = "chkCombineTableObjects";
            chkCombineTableObjects.Size = new System.Drawing.Size(173, 19);
            chkCombineTableObjects.TabIndex = 3;
            chkCombineTableObjects.Text = "Combine Table Objects";
            toolTip1.SetToolTip(chkCombineTableObjects, "Adds scripts for Primary Keys, Foreign Keys, Indexes and Defaults in the same fil" +
        "e as the table create script");
            // 
            // lstStatus
            // 
            lstStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            lstStatus.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            colFile,
            colStatus,
            colFullPath});
            lstStatus.ContextMenuStrip = contextMenu1;
            lstStatus.FullRowSelect = true;
            lstStatus.GridLines = true;
            lstStatus.HideSelection = false;
            lstStatus.Location = new System.Drawing.Point(19, 59);
            lstStatus.MultiSelect = false;
            lstStatus.Name = "lstStatus";
            lstStatus.Size = new System.Drawing.Size(771, 139);
            lstStatus.TabIndex = 5;
            lstStatus.UseCompatibleStateImageBehavior = false;
            lstStatus.View = System.Windows.Forms.View.Details;
            // 
            // colFile
            // 
            colFile.Text = "Script File";
            colFile.Width = 286;
            // 
            // colStatus
            // 
            colStatus.Text = "Status";
            colStatus.Width = 142;
            // 
            // colFullPath
            // 
            colFullPath.Text = "Full File Path";
            colFullPath.Width = 296;
            // 
            // contextMenu1
            // 
            contextMenu1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            mnuNotePad});
            contextMenu1.Name = "contextMenu1";
            contextMenu1.Size = new System.Drawing.Size(187, 26);
            // 
            // mnuNotePad
            // 
            mnuNotePad.Name = "mnuNotePad";
            mnuNotePad.Size = new System.Drawing.Size(186, 22);
            mnuNotePad.Text = "Open File in NotePad";
            mnuNotePad.Click += new System.EventHandler(mnuNotePad_Click);
            // 
            // statusBar1
            // 
            statusBar1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            statStatus});
            statusBar1.Location = new System.Drawing.Point(0, 358);
            statusBar1.Name = "statusBar1";
            statusBar1.Size = new System.Drawing.Size(800, 22);
            statusBar1.TabIndex = 5;
            statusBar1.Text = "statusBar1";
            // 
            // statStatus
            // 
            statStatus.Name = "statStatus";
            statStatus.Size = new System.Drawing.Size(785, 17);
            statStatus.Spring = true;
            // 
            // saveAutoScriptFileDialog1
            // 
            saveAutoScriptFileDialog1.DefaultExt = "sqlauto";
            saveAutoScriptFileDialog1.Filter = "Auto Scripting *.sqlauto|*.sqlauto|All Files *.*|*.*";
            saveAutoScriptFileDialog1.OverwritePrompt = false;
            saveAutoScriptFileDialog1.Title = "Create New or Select Existing Auto Scripting file";
            // 
            // mainMenu1
            // 
            mainMenu1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            menuItem2});
            mainMenu1.Location = new System.Drawing.Point(0, 0);
            mainMenu1.Name = "mainMenu1";
            mainMenu1.Size = new System.Drawing.Size(800, 24);
            mainMenu1.TabIndex = 0;
            // 
            // menuItem2
            // 
            menuItem2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            mnuDestination,
            mnuChangeConnection,
            menuItem3,
            mnuAutoScript,
            menuItem5,
            mnuLookUpTables,
            mnuSqlBuildManager,
            mnuDataDump});
            menuItem2.Name = "menuItem2";
            menuItem2.Size = new System.Drawing.Size(54, 20);
            menuItem2.Text = "&Action";
            // 
            // mnuDestination
            // 
            mnuDestination.Name = "mnuDestination";
            mnuDestination.Size = new System.Drawing.Size(266, 22);
            mnuDestination.Text = "Select Destination Directory";
            mnuDestination.Click += new System.EventHandler(mnuDestination_Click);
            // 
            // mnuChangeConnection
            // 
            mnuChangeConnection.Name = "mnuChangeConnection";
            mnuChangeConnection.Size = new System.Drawing.Size(266, 22);
            mnuChangeConnection.Text = "&Change Sql Server Connection";
            mnuChangeConnection.Click += new System.EventHandler(mnuChangeConnection_Click);
            // 
            // menuItem3
            // 
            menuItem3.Name = "menuItem3";
            menuItem3.Size = new System.Drawing.Size(266, 22);
            menuItem3.Text = "-";
            // 
            // mnuAutoScript
            // 
            mnuAutoScript.Name = "mnuAutoScript";
            mnuAutoScript.Size = new System.Drawing.Size(266, 22);
            mnuAutoScript.Text = "Save to &Auto Script File";
            mnuAutoScript.Click += new System.EventHandler(mnuAutoScript_Click);
            // 
            // menuItem5
            // 
            menuItem5.Name = "menuItem5";
            menuItem5.Size = new System.Drawing.Size(266, 22);
            menuItem5.Text = "-";
            menuItem5.Visible = false;
            // 
            // mnuLookUpTables
            // 
            mnuLookUpTables.Name = "mnuLookUpTables";
            mnuLookUpTables.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L)));
            mnuLookUpTables.Size = new System.Drawing.Size(266, 22);
            mnuLookUpTables.Text = "Open &Lookup Table Scripting";
            mnuLookUpTables.Visible = false;
            mnuLookUpTables.Click += new System.EventHandler(mnuLookUpTables_Click);
            // 
            // mnuSqlBuildManager
            // 
            mnuSqlBuildManager.Name = "mnuSqlBuildManager";
            mnuSqlBuildManager.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.B)));
            mnuSqlBuildManager.Size = new System.Drawing.Size(266, 22);
            mnuSqlBuildManager.Text = "Open Sql &Build Manager";
            mnuSqlBuildManager.Visible = false;
            mnuSqlBuildManager.Click += new System.EventHandler(mnuSqlBuildManager_Click);
            // 
            // mnuDataDump
            // 
            mnuDataDump.Name = "mnuDataDump";
            mnuDataDump.Size = new System.Drawing.Size(266, 22);
            mnuDataDump.Text = "Open Data Extraction";
            mnuDataDump.Visible = false;
            mnuDataDump.Click += new System.EventHandler(mnuDataDump_Click);

            // 
            // bgScripting
            // 
            bgScripting.WorkerReportsProgress = true;
            bgScripting.WorkerSupportsCancellation = true;
            bgScripting.DoWork += new System.ComponentModel.DoWorkEventHandler(bgScripting_DoWork);
            bgScripting.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(bgScripting_ProgressChanged);
            bgScripting.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bgScripting_RunWorkerCompleted);
            // 
            // settingsControl1
            // 
            settingsControl1.BackColor = System.Drawing.Color.White;
            settingsControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            settingsControl1.Location = new System.Drawing.Point(0, 0);
            settingsControl1.Name = "settingsControl1";
            settingsControl1.Project = "(Select Destination)";
            settingsControl1.ProjectLabelText = "Destination Folder:";
            settingsControl1.Server = "";
            settingsControl1.Size = new System.Drawing.Size(800, 92);
            settingsControl1.TabIndex = 6;
            settingsControl1.ServerChanged += new SqlSync.ServerChangedEventHandler(settingsControl1_ServerChanged);
            // 
            // panel1
            // 
            panel1.Controls.Add(mainMenu1);
            panel1.Controls.Add(settingsControl1);
            panel1.Dock = System.Windows.Forms.DockStyle.Top;
            panel1.Location = new System.Drawing.Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(800, 92);
            panel1.TabIndex = 5;
            // 
            // SyncForm
            // 
            AutoScaleBaseSize = new System.Drawing.Size(6, 16);
            ClientSize = new System.Drawing.Size(800, 380);
            Controls.Add(grpScripting);
            Controls.Add(grpDirectory);
            Controls.Add(panel1);
            Controls.Add(statusBar1);
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            MainMenuStrip = mainMenu1;
            Name = "SyncForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Sql Build Manager :: Schema Scripting";
            Load += new System.EventHandler(SyncForm_Load);
            grpDirectory.ResumeLayout(false);
            grpScripting.ResumeLayout(false);
            contextMenu1.ResumeLayout(false);
            statusBar1.ResumeLayout(false);
            statusBar1.PerformLayout();
            mainMenu1.ResumeLayout(false);
            mainMenu1.PerformLayout();
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }
        #endregion




        private void SyncForm_Load(object sender, System.EventArgs e)
        {
            if (connData == null)
            {
                ConnectionForm frmConnect = new ConnectionForm(connectionTitle);
                DialogResult result = frmConnect.ShowDialog();
                if (result == DialogResult.OK)
                {
                    connData = frmConnect.SqlConnection;
                }
                else
                {
                    MessageBox.Show("Sql Schema Scripting can not continue without a valid Sql Connection", "Unable to Load", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    Close();
                }
            }

            databaseList = SqlSync.DbInformation.InfoHelper.GetDatabaseList(connData);
            BindDatabaseListDropDown(databaseList);
            BindScriptTypeDropDown();
            settingsControl1.Server = connData.SQLServerName;

        }

        private void InitilizeHelperClass()
        {
            helper = new ObjectScriptHelper(connData);
            //helper.DatabaseScriptEvent += new DatabaseScriptEventHandler(UpdateScriptStatus);
            //helper.StatusEvent += new StatusEventHandler(helper_StatusEvent);
        }

        #region ## Link Button Event Handlers ##
        private void lstStatus_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {


        }

        #endregion

        #region ## SyncHelper Event Handlers ##
        //private void UpdateScriptStatus(object sender, DatabaseScriptEventArgs e)
        //{
        //    if(e.IsNew)
        //    {
        //        lstStatus.Items.Insert(0,new ListViewItem(new string[]{e.SourceFile,e.Status,e.FullPath}));
        //    }
        //    else
        //    {
        //        for(int i=0;i<15;i++)
        //        {
        //            if(lstStatus.Items[i].SubItems[0].Text == e.SourceFile)
        //            {
        //                lstStatus.Items[i].SubItems[1].Text = e.Status;
        //                if(e.Status == "Object not in Db")
        //                {
        //                    lstStatus.Items[i].BackColor = Color.Orange;
        //                }
        //                break;
        //            }		
        //        }
        //    }

        //}

        //private void helper_StatusEvent(object sender, StatusEventArgs e)
        //{
        //    this.statStatus.Text = e.StatusMessage;
        //}
        #endregion

        private void mnuNotePad_Click(object sender, System.EventArgs e)
        {
            if (lstStatus.SelectedItems.Count > 0)
            {
                string fullPath = lstStatus.SelectedItems[0].SubItems[2].Text;

                Process prc = new Process();
                prc.StartInfo.FileName = "notepad.exe";
                prc.StartInfo.Arguments = fullPath;
                prc.Start();
            }
            else
            {
                MessageBox.Show("Please select a file to display", "Select a File", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }


        private void BindDatabaseListDropDown(DatabaseList databaseList)
        {
            ddDatabase.Items.Clear();
            for (int i = 0; i < databaseList.Count; i++)
                ddDatabase.Items.Add(databaseList[i].DatabaseName);
            ddDatabase.SelectedIndex = 0;
            ddDatabase_SelectionChangeCommitted(null, EventArgs.Empty);

        }
        private void BindScriptTypeDropDown()
        {
            ScriptingType scriptType = new ScriptingType();
            System.Reflection.FieldInfo[] fields = typeof(ScriptingType).GetFields();
            for (int i = 0; i < fields.Length; i++)
                ddScriptType.Items.Add(fields[i].GetValue(scriptType));

            ddScriptType.SelectedIndex = 0;
        }

        #region ## Action Menu Items ##
        private void mnuChangeConnection_Click(object sender, System.EventArgs e)
        {
            ConnectionForm frmConnect = new ConnectionForm(connectionTitle);
            DialogResult result = frmConnect.ShowDialog();
            if (result == DialogResult.OK)
            {
                Cursor = Cursors.WaitCursor;
                try
                {
                    connData = frmConnect.SqlConnection;
                    settingsControl1.Server = connData.SQLServerName;
                    databaseList = frmConnect.DatabaseList;
                    SyncForm_Load(null, EventArgs.Empty);
                    BindDatabaseListDropDown(databaseList);
                }
                finally
                {
                    Cursor = Cursors.Default;
                }
            }
        }

        private void mnuSqlBuildManager_Click(object sender, System.EventArgs e)
        {
            SqlBuildForm frmBuild = new SqlBuildForm(connData);
            frmBuild.Show();
        }

        private void mnuLookUpTables_Click(object sender, System.EventArgs e)
        {
            CodeTableScriptingForm frmLookup = new CodeTableScriptingForm(connData);
            frmLookup.Show();
        }

        private void mnuAutoScript_Click(object sender, System.EventArgs e)
        {
            if (DialogResult.OK == saveAutoScriptFileDialog1.ShowDialog())
            {
                bool newFile = false;
                string fileName = saveAutoScriptFileDialog1.FileName;
                SqlSync.ObjectScript.AutoScriptingConfig config = new SqlSync.ObjectScript.AutoScriptingConfig();
                SqlSync.ObjectScript.AutoScriptingConfig.AutoScriptingRow parent;
                if (File.Exists(fileName))
                {
                    config.ReadXml(fileName);
                    parent = config.AutoScripting[0];
                }
                else
                {
                    parent = config.AutoScripting.NewAutoScriptingRow();
                    parent.AllowManualSelection = false;
                    parent.IncludeFileHeaders = chkIncludeHeaders.Checked;
                    parent.ZipScripts = chkZip.Checked;
                    config.AutoScripting.AddAutoScriptingRow(parent);
                    newFile = true;
                }


                config.DatabaseScriptConfig.AddDatabaseScriptConfigRow(connData.SQLServerName,
                    connData.DatabaseName,
                    connData.UserId,
                    connData.Password,
                    connData.AuthenticationType.ToString(),
                    settingsControl1.Project,
                    parent);

                config.WriteXml(saveAutoScriptFileDialog1.FileName);

                string message = "Auto Scripting File was successfully {0} @ {1}";

                if (newFile)
                    MessageBox.Show(string.Format(message, "created", fileName), "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                else
                    MessageBox.Show(string.Format(message, "appended", fileName), "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);

            }
        }

        #endregion

        private void ddDatabase_SelectionChangeCommitted(object sender, System.EventArgs e)
        {
            connData.DatabaseName = ddDatabase.SelectedItem.ToString();
            if (settingsControl1.Project != "(Select Destination)")
            {
                grpScripting.Enabled = true;
            }

        }


        private void mnuDataDump_Click(object sender, System.EventArgs e)
        {
            DataDump.DataDumpForm frmDump = new SqlSync.DataDump.DataDumpForm(connData);
            frmDump.Show();
        }

        private void mnuDestination_Click(object sender, System.EventArgs e)
        {
            DialogResult result = fldrStartingDir.ShowDialog();
            if (result == DialogResult.OK)
            {
                settingsControl1.Project = fldrStartingDir.SelectedPath;
                connData.StartingDirectory = settingsControl1.Project;

                if (ddDatabase.SelectedItem != null)
                {
                    //mnuComparisons.Enabled = true;
                    grpScripting.Enabled = true;
                }
            }

        }

        private void bgScripting_DoWork(object sender, DoWorkEventArgs e)
        {
            ObjectScriptHelper scrHelper;
            if (e.Argument == null)
            {
                scrHelper = new ObjectScriptHelper(connData);
                scrHelper.ProcessScripts(sender as BackgroundWorker, e);
            }
            else
            {
                scrHelper = new ObjectScriptHelper(((ObjectScriptingConfigData)e.Argument).ConnData);
                scrHelper.ProcessFullScripting(e.Argument as ObjectScriptingConfigData, sender as BackgroundWorker, e);
            }
        }

        private void bgScripting_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is StatusEventArgs)
            {
                statStatus.Text = ((StatusEventArgs)e.UserState).StatusMessage;
            }
            else if (e.UserState is DatabaseScriptEventArgs)
            {
                DatabaseScriptEventArgs args = e.UserState as DatabaseScriptEventArgs;
                if (args.IsNew)
                {
                    lstStatus.Items.Insert(0, new ListViewItem(new string[] { args.SourceFile, args.Status, args.FullPath }));
                }
                else
                {
                    for (int i = 0; i < 15; i++)
                    {
                        if (lstStatus.Items[i].SubItems[0].Text == args.SourceFile)
                        {
                            lstStatus.Items[i].SubItems[1].Text = args.Status;
                            if (args.Status == "Object not in Db")
                            {
                                lstStatus.Items[i].BackColor = Color.Orange;
                            }
                            break;
                        }
                    }
                }
            }

        }

        private void btnGo_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                lstStatus.Items.Clear();
                InitilizeHelperClass();
                btnGo.Enabled = false;

                ObjectScriptingConfigData cfgData = null;
                switch (ddScriptType.SelectedItem.ToString())
                {
                    case ScriptingType.FullSchemaScript:
                        cfgData = new ObjectScriptingConfigData(false, chkCombineTableObjects.Checked, chkZip.Checked, chkIncludeHeaders.Checked, true, connData);
                        break;
                    case ScriptingType.FullWithDelete:
                        cfgData = new ObjectScriptingConfigData(true, chkCombineTableObjects.Checked, chkZip.Checked, chkIncludeHeaders.Checked, true, connData);
                        break;
                    case ScriptingType.UpdateExistingFiles:
                        break;
                }
                bgScripting.RunWorkerAsync(cfgData);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            statStatus.Text = "Attempting to Cancel Scripting";
            btnCancel.Enabled = false;
            bgScripting.CancelAsync();

        }

        private void bgScripting_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                statStatus.Text = "Scripting Canceled";
            }

            btnGo.Enabled = true;
            btnCancel.Enabled = true;
            Cursor = Cursors.Default;
        }

        private void settingsControl1_ServerChanged(object sender, string serverName, string username, string password, AuthenticationType authType)
        {
            Connection.ConnectionData oldConnData = new Connection.ConnectionData();
            connData.Fill(oldConnData);
            Cursor = Cursors.WaitCursor;

            connData.SQLServerName = serverName;
            if (!string.IsNullOrWhiteSpace(username) && (!string.IsNullOrWhiteSpace(password)))
            {
                connData.UserId = username;
                connData.Password = password;
            }
            connData.AuthenticationType = authType;
            connData.ScriptTimeout = 5;

            try
            {
                databaseList = SqlSync.DbInformation.InfoHelper.GetDatabaseList(connData);
                SyncForm_Load(null, EventArgs.Empty);
                BindDatabaseListDropDown(databaseList);

            }
            catch
            {
                MessageBox.Show("Error retrieving database list. Is the server running?", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                connData = oldConnData;
                settingsControl1.Server = oldConnData.SQLServerName;
            }


            Cursor = Cursors.Default;
        }



    }
}
