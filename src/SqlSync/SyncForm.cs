using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
using System.Diagnostics;
using SqlSync.SqlBuild;
using System.Threading;
using System.Text;
using System.IO;
using SqlSync.Connection;
using SqlSync.ObjectScript;
using SqlSync.Constants;
using SqlSync.DbInformation;
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
		private System.Windows.Forms.StatusBar statusBar1;
        private System.Windows.Forms.StatusBarPanel statStatus;
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
		private System.Windows.Forms.ToolStripMenuItem mnuCompareTables;
		private System.Windows.Forms.ToolStripMenuItem mnuCompareViews;
		private System.Windows.Forms.ToolStripMenuItem mnuCompareStoredProcs;
		private System.Windows.Forms.ToolStripMenuItem mnuCompareFunctions;
		private System.Windows.Forms.ToolStripMenuItem mnuCompareUsers;
		private System.Windows.Forms.ToolStripMenuItem mnuServerLogins;
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
		private System.Windows.Forms.ToolStripMenuItem mnuComparisons;

		public SyncForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			
			//
		}

		public SyncForm(ConnectionData connData):this()
		{
			this.connData = connData;
		}

		public SyncForm(string server, string userid, string password, string databaseName, string destinationDirectory)
		{
			InitializeComponent();

			this.mnuComparisons.Enabled = true;

			//Set the connection data
			this.connData = new ConnectionData();
			this.connData.DatabaseName = databaseName;
			this.connData.SQLServerName = server;
			this.connData.Password = password;
			this.connData.UserId = userid;
            this.connData.AuthenticationType = AuthenticationType.Password;
			this.connData.StartingDirectory = destinationDirectory;

			//Set the messagebox
			StringBuilder sb = new StringBuilder();
			sb.Append("Server:"+this.connData.SQLServerName+"\r\n");
			sb.Append("DatabaseName:"+this.connData.DatabaseName+"\r\n");
			sb.Append("UserId:"+this.connData.UserId+"\r\n");
			sb.Append("Password:"+this.connData.Password+"\r\n");
			sb.Append("StartingDirectory:"+this.connData.StartingDirectory+"\r\n");
			MessageBox.Show(sb.ToString(),"Initilized");



            ObjectScriptingConfigData cfgData = new ObjectScriptingConfigData(true, true, true, true,false,this.connData);
            bgScripting.RunWorkerAsync(cfgData);
		}
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SyncForm));
            this.grpDirectory = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ddDatabase = new System.Windows.Forms.ComboBox();
            this.fldrStartingDir = new System.Windows.Forms.FolderBrowserDialog();
            this.grpScripting = new System.Windows.Forms.GroupBox();
            this.chkIncludeHeaders = new System.Windows.Forms.CheckBox();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnGo = new System.Windows.Forms.Button();
            this.ddScriptType = new System.Windows.Forms.ComboBox();
            this.chkZip = new System.Windows.Forms.CheckBox();
            this.chkCombineTableObjects = new System.Windows.Forms.CheckBox();
            this.lstStatus = new System.Windows.Forms.ListView();
            this.colFile = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colStatus = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colFullPath = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenu1 = new System.Windows.Forms.ContextMenuStrip();
            this.mnuNotePad = new System.Windows.Forms.ToolStripMenuItem();
            this.statusBar1 = new System.Windows.Forms.StatusBar();
            this.statStatus = new System.Windows.Forms.StatusBarPanel();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.saveAutoScriptFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.mainMenu1 = new System.Windows.Forms.MenuStrip();
            this.menuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuDestination = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuChangeConnection = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem3 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAutoScript = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem5 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuLookUpTables = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSqlBuildManager = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuDataDump = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuComparisons = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCompareTables = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCompareViews = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCompareStoredProcs = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCompareFunctions = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCompareUsers = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuServerLogins = new System.Windows.Forms.ToolStripMenuItem();
            this.bgScripting = new System.ComponentModel.BackgroundWorker();
            this.settingsControl1 = new SqlSync.SettingsControl();
            this.grpDirectory.SuspendLayout();
            this.grpScripting.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.statStatus)).BeginInit();
            this.SuspendLayout();
            // 
            // grpDirectory
            // 
            this.grpDirectory.Controls.Add(this.label1);
            this.grpDirectory.Controls.Add(this.ddDatabase);
            this.grpDirectory.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpDirectory.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.grpDirectory.Location = new System.Drawing.Point(0, 60);
            this.grpDirectory.Name = "grpDirectory";
            this.grpDirectory.Size = new System.Drawing.Size(775, 48);
            this.grpDirectory.TabIndex = 0;
            this.grpDirectory.TabStop = false;
            this.grpDirectory.Text = "1) Select a Database";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(16, 18);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(64, 16);
            this.label1.TabIndex = 5;
            this.label1.Text = "Database:";
            // 
            // ddDatabase
            // 
            this.ddDatabase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddDatabase.Location = new System.Drawing.Point(88, 16);
            this.ddDatabase.Name = "ddDatabase";
            this.ddDatabase.Size = new System.Drawing.Size(121, 21);
            this.ddDatabase.TabIndex = 0;
            this.ddDatabase.SelectionChangeCommitted += new System.EventHandler(this.ddDatabase_SelectionChangeCommitted);
            // 
            // grpScripting
            // 
            this.grpScripting.Controls.Add(this.chkIncludeHeaders);
            this.grpScripting.Controls.Add(this.btnCancel);
            this.grpScripting.Controls.Add(this.btnGo);
            this.grpScripting.Controls.Add(this.ddScriptType);
            this.grpScripting.Controls.Add(this.chkZip);
            this.grpScripting.Controls.Add(this.chkCombineTableObjects);
            this.grpScripting.Controls.Add(this.lstStatus);
            this.grpScripting.Dock = System.Windows.Forms.DockStyle.Fill;
            this.grpScripting.Enabled = false;
            this.grpScripting.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.grpScripting.Location = new System.Drawing.Point(0, 108);
            this.grpScripting.Name = "grpScripting";
            this.grpScripting.Size = new System.Drawing.Size(775, 203);
            this.grpScripting.TabIndex = 4;
            this.grpScripting.TabStop = false;
            this.grpScripting.Text = "2) Update Script Files from Database";
            // 
            // chkIncludeHeaders
            // 
            this.chkIncludeHeaders.Checked = true;
            this.chkIncludeHeaders.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkIncludeHeaders.Location = new System.Drawing.Point(499, 16);
            this.chkIncludeHeaders.Name = "chkIncludeHeaders";
            this.chkIncludeHeaders.Size = new System.Drawing.Size(144, 16);
            this.chkIncludeHeaders.TabIndex = 8;
            this.chkIncludeHeaders.Text = "Include Headers";
            this.toolTip1.SetToolTip(this.chkIncludeHeaders, "Adds scripts for Primary Keys, Foreign Keys, Indexes and Defaults in the same fil" +
                    "e as the table create script");
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.Location = new System.Drawing.Point(665, 14);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(102, 23);
            this.btnCancel.TabIndex = 7;
            this.btnCancel.Text = "Cancel Scripting";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnGo
            // 
            this.btnGo.Location = new System.Drawing.Point(218, 14);
            this.btnGo.Name = "btnGo";
            this.btnGo.Size = new System.Drawing.Size(40, 23);
            this.btnGo.TabIndex = 6;
            this.btnGo.Text = "Go";
            this.btnGo.UseVisualStyleBackColor = true;
            this.btnGo.Click += new System.EventHandler(this.btnGo_Click);
            // 
            // ddScriptType
            // 
            this.ddScriptType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddScriptType.Location = new System.Drawing.Point(16, 14);
            this.ddScriptType.Name = "ddScriptType";
            this.ddScriptType.Size = new System.Drawing.Size(192, 21);
            this.ddScriptType.TabIndex = 0;
            // 
            // chkZip
            // 
            this.chkZip.Checked = true;
            this.chkZip.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkZip.Location = new System.Drawing.Point(272, 16);
            this.chkZip.Name = "chkZip";
            this.chkZip.Size = new System.Drawing.Size(80, 16);
            this.chkZip.TabIndex = 2;
            this.chkZip.Text = "Zip Scripts ";
            this.toolTip1.SetToolTip(this.chkZip, "Adds scripts for Primary Keys, Foreign Keys, Indexes and Defaults in the same fil" +
                    "e as the table create script");
            // 
            // chkCombineTableObjects
            // 
            this.chkCombineTableObjects.Checked = true;
            this.chkCombineTableObjects.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkCombineTableObjects.Location = new System.Drawing.Point(360, 16);
            this.chkCombineTableObjects.Name = "chkCombineTableObjects";
            this.chkCombineTableObjects.Size = new System.Drawing.Size(144, 16);
            this.chkCombineTableObjects.TabIndex = 3;
            this.chkCombineTableObjects.Text = "Combine Table Objects";
            this.toolTip1.SetToolTip(this.chkCombineTableObjects, "Adds scripts for Primary Keys, Foreign Keys, Indexes and Defaults in the same fil" +
                    "e as the table create script");
            // 
            // lstStatus
            // 
            this.lstStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstStatus.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colFile,
            this.colStatus,
            this.colFullPath});
            this.lstStatus.ContextMenuStrip = this.contextMenu1;
            this.lstStatus.FullRowSelect = true;
            this.lstStatus.GridLines = true;
            this.lstStatus.Location = new System.Drawing.Point(16, 48);
            this.lstStatus.MultiSelect = false;
            this.lstStatus.Name = "lstStatus";
            this.lstStatus.Size = new System.Drawing.Size(751, 148);
            this.lstStatus.TabIndex = 5;
            this.lstStatus.UseCompatibleStateImageBehavior = false;
            this.lstStatus.View = System.Windows.Forms.View.Details;
            // 
            // colFile
            // 
            this.colFile.Text = "Script File";
            this.colFile.Width = 286;
            // 
            // colStatus
            // 
            this.colStatus.Text = "Status";
            this.colStatus.Width = 142;
            // 
            // colFullPath
            // 
            this.colFullPath.Text = "Full File Path";
            this.colFullPath.Width = 296;
            // 
            // contextMenu1
            // 
            this.contextMenu1.Items.AddRange(new System.Windows.Forms.ToolStripMenuItem[] {
            this.mnuNotePad});
            // 
            // mnuNotePad
            // 
            //this.mnuNotePad.Index = 0;
            this.mnuNotePad.Text = "Open File in NotePad";
            this.mnuNotePad.Click += new System.EventHandler(this.mnuNotePad_Click);
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(0, 311);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
            this.statStatus});
            this.statusBar1.ShowPanels = true;
            this.statusBar1.Size = new System.Drawing.Size(775, 22);
            this.statusBar1.TabIndex = 5;
            this.statusBar1.Text = "statusBar1";
            // 
            // statStatus
            // 
            this.statStatus.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
            this.statStatus.Name = "statStatus";
            this.statStatus.Width = 758;
            // 
            // saveAutoScriptFileDialog1
            // 
            this.saveAutoScriptFileDialog1.DefaultExt = "sqlauto";
            this.saveAutoScriptFileDialog1.Filter = "Auto Scripting *.sqlauto|*.sqlauto|All Files *.*|*.*";
            this.saveAutoScriptFileDialog1.OverwritePrompt = false;
            this.saveAutoScriptFileDialog1.Title = "Create New or Select Existing Auto Scripting file";
            // 
            // mainMenu1
            // 
            this.mainMenu1.Items.AddRange(new System.Windows.Forms.ToolStripMenuItem[] {
            this.menuItem2,
            this.mnuComparisons});
            // 
            // menuItem2
            // 
            //this.menuItem2.Index = 0;
            this.menuItem2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripMenuItem[] {
            this.mnuDestination,
            this.mnuChangeConnection,
            this.menuItem3,
            this.mnuAutoScript,
            this.menuItem5,
            this.mnuLookUpTables,
            this.mnuSqlBuildManager,
            this.mnuDataDump});
            this.menuItem2.Text = "&Action";
            // 
            // mnuDestination
            // 
           // this.mnuDestination.Index = 0;
            this.mnuDestination.Text = "Select Destination Directory";
            this.mnuDestination.Click += new System.EventHandler(this.mnuDestination_Click);
            // 
            // mnuChangeConnection
            // 
            //this.mnuChangeConnection.Index = 1;
            this.mnuChangeConnection.Text = "&Change Sql Server Connection";
            this.mnuChangeConnection.Click += new System.EventHandler(this.mnuChangeConnection_Click);
            // 
            // menuItem3
            // 
            //this.menuItem3.Index = 2;
            this.menuItem3.Text = "-";
            // 
            // mnuAutoScript
            // 
            //this.mnuAutoScript.Index = 3;
            this.mnuAutoScript.Text = "Save to &Auto Script File";
            this.mnuAutoScript.Click += new System.EventHandler(this.mnuAutoScript_Click);
            // 
            // menuItem5
            // 
            //this.menuItem5.Index = 4;
            this.menuItem5.Text = "-";
            this.menuItem5.Visible = false;
            // 
            // mnuLookUpTables
            // 
            //this.mnuLookUpTables.Index = 5;
            this.mnuLookUpTables.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.L;
            this.mnuLookUpTables.Text = "Open &Lookup Table Scripting";
            this.mnuLookUpTables.Visible = false;
            this.mnuLookUpTables.Click += new System.EventHandler(this.mnuLookUpTables_Click);
            // 
            // mnuSqlBuildManager
            // 
            //this.mnuSqlBuildManager.Index = 6;
            this.mnuSqlBuildManager.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.B;
            this.mnuSqlBuildManager.Text = "Open Sql &Build Manager";
            this.mnuSqlBuildManager.Visible = false;
            this.mnuSqlBuildManager.Click += new System.EventHandler(this.mnuSqlBuildManager_Click);
            // 
            // mnuDataDump
            // 
            //this.mnuDataDump.Index = 7;
            this.mnuDataDump.Text = "Open Data Extraction";
            this.mnuDataDump.Visible = false;
            this.mnuDataDump.Click += new System.EventHandler(this.mnuDataDump_Click);
            // 
            // mnuComparisons
            // 
            this.mnuComparisons.Enabled = false;
            //this.mnuComparisons.Index = 1;
            this.mnuComparisons.DropDownItems.AddRange(new System.Windows.Forms.ToolStripMenuItem[] {
            this.mnuCompareTables,
            this.mnuCompareViews,
            this.mnuCompareStoredProcs,
            this.mnuCompareFunctions,
            this.mnuCompareUsers,
            this.mnuServerLogins});
            this.mnuComparisons.Text = "&Comparisons";
            // 
            // mnuCompareTables
            // 
            //this.mnuCompareTables.Index = 0;
            this.mnuCompareTables.Text = "&Tables (with Keys and Indexes)";
            this.mnuCompareTables.Click += new System.EventHandler(this.mnuCompareTables_Click);
            // 
            // mnuCompareViews
            // 
            //this.mnuCompareViews.Index = 1;
            this.mnuCompareViews.Text = "&Views";
            this.mnuCompareViews.Click += new System.EventHandler(this.mnuCompareViews_Click);
            // 
            // mnuCompareStoredProcs
            // 
           // this.mnuCompareStoredProcs.Index = 2;
            this.mnuCompareStoredProcs.Text = "&Stored Procedures";
            this.mnuCompareStoredProcs.Click += new System.EventHandler(this.mnuCompareStoredProcs_Click);
            // 
            // mnuCompareFunctions
            // 
            //this.mnuCompareFunctions.Index = 3;
            this.mnuCompareFunctions.Text = "&User Defined Functions";
            this.mnuCompareFunctions.Click += new System.EventHandler(this.mnuCompareFunctions_Click);
            // 
            // mnuCompareUsers
            // 
            //this.mnuCompareUsers.Index = 4;
            this.mnuCompareUsers.Text = "&Database Users";
            this.mnuCompareUsers.Click += new System.EventHandler(this.mnuCompareUsers_Click);
            // 
            // mnuServerLogins
            // 
            //this.mnuServerLogins.Index = 5;
            this.mnuServerLogins.Text = "Server &Logins";
            this.mnuServerLogins.Click += new System.EventHandler(this.mnuServerLogins_Click);
            // 
            // bgScripting
            // 
            this.bgScripting.WorkerReportsProgress = true;
            this.bgScripting.WorkerSupportsCancellation = true;
            this.bgScripting.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgScripting_DoWork);
            this.bgScripting.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgScripting_ProgressChanged);
            this.bgScripting.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgScripting_RunWorkerCompleted);
            // 
            // settingsControl1
            // 
            this.settingsControl1.BackColor = System.Drawing.Color.White;
            this.settingsControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.settingsControl1.Location = new System.Drawing.Point(0, 0);
            this.settingsControl1.Name = "settingsControl1";
            this.settingsControl1.Project = "(Select Destination)";
            this.settingsControl1.ProjectLabelText = "Destination Folder:";
            this.settingsControl1.Server = "";
            this.settingsControl1.Size = new System.Drawing.Size(775, 60);
            this.settingsControl1.TabIndex = 6;
            this.settingsControl1.ServerChanged += new SqlSync.ServerChangedEventHandler(this.settingsControl1_ServerChanged);
            // 
            // SyncForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(775, 333);
            this.Controls.Add(this.grpScripting);
            this.Controls.Add(this.grpDirectory);
            this.Controls.Add(this.settingsControl1);
            this.Controls.Add(this.statusBar1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.mainMenu1;
            this.Name = "SyncForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sql Build Manager :: Schema Scripting";
            this.Load += new System.EventHandler(this.SyncForm_Load);
            this.grpDirectory.ResumeLayout(false);
            this.grpScripting.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.statStatus)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion



		
		private void SyncForm_Load(object sender, System.EventArgs e)
		{
			if(this.connData == null)
			{
				ConnectionForm frmConnect = new ConnectionForm(connectionTitle);
				DialogResult result =  frmConnect.ShowDialog();
				if(result == DialogResult.OK)
				{
					this.connData = frmConnect.SqlConnection;
				}
				else
				{
					MessageBox.Show("Sql Schema Scripting can not continue without a valid Sql Connection","Unable to Load",MessageBoxButtons.OK,MessageBoxIcon.Hand);
					this.Close();
				}
			}
			
			this.databaseList = SqlSync.DbInformation.InfoHelper.GetDatabaseList(this.connData);
			BindDatabaseListDropDown(this.databaseList);
			BindScriptTypeDropDown();
			this.settingsControl1.Server = this.connData.SQLServerName;

		}

		private void InitilizeHelperClass()
		{
                helper = new ObjectScriptHelper(this.connData);
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
			if(lstStatus.SelectedItems.Count > 0)
			{
				string fullPath = lstStatus.SelectedItems[0].SubItems[2].Text;

				Process prc = new Process();
				prc.StartInfo.FileName = "notepad.exe";
				prc.StartInfo.Arguments = fullPath;
				prc.Start();
			}
			else
			{
				MessageBox.Show("Please select a file to display","Select a File",MessageBoxButtons.OK,MessageBoxIcon.Information);
			}
		}


		private void BindDatabaseListDropDown(DatabaseList databaseList)
		{
			this.ddDatabase.Items.Clear();
            for(int i=0;i<databaseList.Count;i++)
			    this.ddDatabase.Items.Add(databaseList[i].DatabaseName);
			this.ddDatabase.SelectedIndex = 0;
			ddDatabase_SelectionChangeCommitted(null,EventArgs.Empty);

		}
		private void BindScriptTypeDropDown()
		{
			ScriptingType scriptType = new ScriptingType();
			System.Reflection.FieldInfo[] fields = typeof(ScriptingType).GetFields();
			for(int i=0;i<fields.Length;i++)
				ddScriptType.Items.Add(fields[i].GetValue(scriptType));

			ddScriptType.SelectedIndex = 0;
		}

		private void ShowComparisonForm(ObjectSyncData[] syncData)
		{
			if(syncData != null)
			{
				statStatus.Text = "Initilizing Database to File System Object Comparison";
				ObjectComparison frmCompare = new ObjectComparison(syncData,ref helper);
				frmCompare.ShowDialog();
				statStatus.Text = "Ready.";

			}
			else
			{
				statStatus.Text = "Comparison failed. Please try to reconnect to the database";
			}
		}






		
		#region ## Comparison Menu Items ##
		private void mnuServerLogins_Click(object sender, System.EventArgs e)
		{
			InitilizeHelperClass();
			ObjectSyncData[] data =  helper.CompareServerLogins(this.settingsControl1.Project);
			ShowComparisonForm(data);
		}

		private void mnuCompareFunctions_Click(object sender, System.EventArgs e)
		{
			InitilizeHelperClass();
			ObjectSyncData[] data =  helper.CompareUserDefinedFunctions(this.settingsControl1.Project);
			ShowComparisonForm(data);
		}

		private void mnuCompareUsers_Click(object sender, System.EventArgs e)
		{
			InitilizeHelperClass();
			ObjectSyncData[] data =  helper.CompareDatabaseUsers(this.settingsControl1.Project);
			ShowComparisonForm(data);
		}

		private void mnuCompareTables_Click(object sender, System.EventArgs e)
		{
			InitilizeHelperClass();
			ObjectSyncData[] data =  helper.CompareTableObjects(this.settingsControl1.Project);
			ShowComparisonForm(data);

		}

		private void mnuCompareViews_Click(object sender, System.EventArgs e)
		{
			InitilizeHelperClass();
			ObjectSyncData[] data =  helper.CompareViews(this.settingsControl1.Project);
			ShowComparisonForm(data);
		}

		private void mnuCompareStoredProcs_Click(object sender, System.EventArgs e)
		{
			InitilizeHelperClass();
			ObjectSyncData[] data =  helper.CompareStoredProcs(this.settingsControl1.Project);
			ShowComparisonForm(data);

		}

		#endregion

		#region ## Action Menu Items ##
		private void mnuChangeConnection_Click(object sender, System.EventArgs e)
		{
			ConnectionForm frmConnect = new ConnectionForm(connectionTitle);
			DialogResult result = frmConnect.ShowDialog();
			if(result == DialogResult.OK)
			{
				this.Cursor = Cursors.WaitCursor;
				try
				{
					this.connData = frmConnect.SqlConnection;
					this.settingsControl1.Server = this.connData.SQLServerName;
					this.databaseList = frmConnect.DatabaseList;
					this.SyncForm_Load(null,EventArgs.Empty);
					BindDatabaseListDropDown(this.databaseList);
				}
				finally
				{
					this.Cursor = Cursors.Default;
				}
			}
		}

		private void mnuSqlBuildManager_Click(object sender, System.EventArgs e)
		{
			SqlBuildForm frmBuild = new SqlBuildForm(this.connData);
			frmBuild.Show();
		}

		private void mnuLookUpTables_Click(object sender, System.EventArgs e)
		{
			CodeTableScriptingForm frmLookup = new CodeTableScriptingForm(this.connData);
			frmLookup.Show();
		}

		private void mnuAutoScript_Click(object sender, System.EventArgs e)
		{
			if(DialogResult.OK == this.saveAutoScriptFileDialog1.ShowDialog())
			{
				bool newFile = false;
				string fileName = this.saveAutoScriptFileDialog1.FileName;
				SqlSync.ObjectScript.AutoScriptingConfig 	config = new SqlSync.ObjectScript.AutoScriptingConfig();
				SqlSync.ObjectScript.AutoScriptingConfig.AutoScriptingRow parent;
				if(File.Exists(fileName))
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


				config.DatabaseScriptConfig.AddDatabaseScriptConfigRow(this.connData.SQLServerName,
					this.connData.DatabaseName,
					this.connData.UserId,
					this.connData.Password,
					this.connData.AuthenticationType.ToString(),
					this.settingsControl1.Project,
					parent);

				config.WriteXml(this.saveAutoScriptFileDialog1.FileName);

				string message = "Auto Scripting File was successfully {0} @ {1}";

				if(newFile)
					MessageBox.Show(string.Format(message,"created",fileName),"Success",MessageBoxButtons.OK,MessageBoxIcon.Information);
				else
					MessageBox.Show(string.Format(message,"appended",fileName),"Success",MessageBoxButtons.OK,MessageBoxIcon.Information);

			}
		}

		#endregion

		private void ddDatabase_SelectionChangeCommitted(object sender, System.EventArgs e)
		{
			this.connData.DatabaseName = ddDatabase.SelectedItem.ToString();
			if(this.settingsControl1.Project != "(Select Destination)")
			{
				this.mnuCompareFunctions.Enabled = true;
				this.grpScripting.Enabled = true;
			}

		}

		
		private void mnuDataDump_Click(object sender, System.EventArgs e)
		{
			DataDump.DataDumpForm frmDump = new SqlSync.DataDump.DataDumpForm(this.connData);
			frmDump.Show();
		}

		private void mnuDestination_Click(object sender, System.EventArgs e)
		{
			DialogResult result = fldrStartingDir.ShowDialog();
			if(result == DialogResult.OK)
			{
				this.settingsControl1.Project = fldrStartingDir.SelectedPath;
				this.connData.StartingDirectory = this.settingsControl1.Project;

				if(ddDatabase.SelectedItem != null)
				{
					mnuComparisons.Enabled = true;
					grpScripting.Enabled = true;
				}
			}
		
		}

        private void bgScripting_DoWork(object sender, DoWorkEventArgs e)
        {
            ObjectScriptHelper scrHelper;
            if (e.Argument == null)
            {
                scrHelper = new ObjectScriptHelper(this.connData);
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
                this.statStatus.Text = ((StatusEventArgs)e.UserState).StatusMessage;
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
                this.Cursor = Cursors.WaitCursor;
                this.lstStatus.Items.Clear();
                this.InitilizeHelperClass();
                this.btnGo.Enabled = false;

                ObjectScriptingConfigData cfgData = null;
                switch (ddScriptType.SelectedItem.ToString())
                {
                    case ScriptingType.FullSchemaScript:
                        cfgData = new ObjectScriptingConfigData(false, chkCombineTableObjects.Checked, chkZip.Checked, chkIncludeHeaders.Checked,true,this.connData);
                        break;
                    case ScriptingType.FullWithDelete:
                        cfgData = new ObjectScriptingConfigData(true, chkCombineTableObjects.Checked, chkZip.Checked, chkIncludeHeaders.Checked, true, this.connData);
                        break;
                    case ScriptingType.UpdateExistingFiles:
                        break;
                }
                bgScripting.RunWorkerAsync(cfgData);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
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

            this.btnGo.Enabled = true;
            btnCancel.Enabled = true;
            this.Cursor = Cursors.Default;
        }

        private void settingsControl1_ServerChanged(object sender, string serverName, string username, string password, AuthenticationType authType)
        {
            Connection.ConnectionData oldConnData = new Connection.ConnectionData();
            this.connData.Fill(oldConnData);
            this.Cursor = Cursors.WaitCursor;

            this.connData.SQLServerName = serverName;
            if (!string.IsNullOrWhiteSpace(username) && (!string.IsNullOrWhiteSpace(password)))
            {
                this.connData.UserId = username;
                this.connData.Password = password;
            }
            this.connData.AuthenticationType = authType;
            this.connData.ScriptTimeout = 5;

            try
            {
                this.databaseList = SqlSync.DbInformation.InfoHelper.GetDatabaseList(this.connData);
                this.SyncForm_Load(null, EventArgs.Empty);
                BindDatabaseListDropDown(this.databaseList);

            }
            catch
            {
                MessageBox.Show("Error retrieving database list. Is the server running?", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.connData = oldConnData;
                this.settingsControl1.Server = oldConnData.SQLServerName;
            }


            this.Cursor = Cursors.Default;
        }

   

	}
}
