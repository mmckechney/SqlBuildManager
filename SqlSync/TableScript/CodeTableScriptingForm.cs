using System;
using System.Data;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Text;
using System.Reflection;
using SqlSync.Validator;
using SqlSync.TableScript;
using SqlSync.TableScript.Audit;
using SqlSync.SqlBuild;
using SqlSync.Connection;
using SqlSync.MRU;
using SqlSync.Constants;
using SqlSync.DbInformation;
using System.Linq;
namespace SqlSync
{
	/// <summary>
	/// Summary description for LookUpTable.
	/// </summary>
	public class CodeTableScriptingForm : System.Windows.Forms.Form , IMRUClient
	{
		private MRUManager mruManager;
		private System.ComponentModel.IContainer components;
		ConnectionData data = null;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
		private System.Windows.Forms.StatusBar statusBar1;
		private System.Windows.Forms.StatusBarPanel statStatus;
		private System.Windows.Forms.ContextMenuStrip contextMenu1;
        private System.Windows.Forms.ToolStripMenuItem mnuAddTable;
		private System.Windows.Forms.ToolStripMenuItem mnuDeleteTable;
        private System.Windows.Forms.ToolStripMenuItem mnuWhereClause;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.GroupBox grpScripting;
		private System.Windows.Forms.LinkLabel lnkCheckAll;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListView lstTables;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		SqlSync.TableScript.SQLSyncData tableList = null;
		private System.Windows.Forms.TabControl tcTables;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox ddDatabaseList;
		private System.Windows.Forms.ContextMenuStrip contextDatabase;
		private System.Windows.Forms.ToolStripMenuItem mnuRemoveDatabase;
		private string projectFileName = string.Empty;
		private System.Windows.Forms.MenuStrip mainMenu1;
		private System.Windows.Forms.ToolStripMenuItem mnuActionMain;
		private System.Windows.Forms.ToolStripMenuItem mnuLoadProjectFile;
		private System.Windows.Forms.ToolStripMenuItem mnuChangeSqlServer;
		private System.Windows.Forms.OpenFileDialog openBuildManager;
		private System.Windows.Forms.ToolStripMenuItem mnuScriptMissingColumns;
		private string newDatabaseName = string.Empty;
        //private string[] updateByTables;
		private System.Windows.Forms.ToolStripMenuItem mnuScriptAllMissingColumns;
		private System.Windows.Forms.ToolStripMenuItem mnuScriptTriggers;
		private System.Windows.Forms.DateTimePicker dateTimePicker1;
		private System.Windows.Forms.CheckBox chkSelectByDate;
		private System.Windows.Forms.ToolStripMenuItem mnuScriptTrigger;
		private System.Windows.Forms.ToolStripMenuItem mnuCopyTablesForsql;
		private System.Windows.Forms.ToolStripMenuItem mnuScriptAllColumnDefaults;
		private System.Windows.Forms.ToolStripMenuItem mnuScriptDefault;
		private System.Windows.Forms.ToolStripMenuItem mnuCopyTables;
		private SqlSync.SettingsControl settingsControl1;
		private System.Windows.Forms.ToolStripMenuItem mnuUpdateDateAndId;
		private System.Windows.Forms.ToolStripMenuItem mnuGoSeparators;
		private System.Windows.Forms.ToolStripMenuItem mnuIncludeUpdateStatements;
		private System.Windows.Forms.ToolStripMenuItem mnuSettings;
		private System.Windows.Forms.Button btnGenerateScripts;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Splitter splitter1;
		private System.Windows.Forms.Panel panel2;
		private System.Windows.Forms.GroupBox grpDatabaseInfo;
		private System.Windows.Forms.ToolStripMenuItem mnuFileMRU;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		//private string[] updateDateTables;
		private System.Windows.Forms.ToolBar toolBar1;
		private System.Windows.Forms.ToolBarButton tbbSave;
		private System.Windows.Forms.ToolBarButton tbbExportToBM;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ToolStripMenuItem menuItem7;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private System.Windows.Forms.GroupBox groupBox1;
		private System.Windows.Forms.ToolStripMenuItem mnuScriptTriggersOneFile;
		private System.Windows.Forms.ToolStripMenuItem mnuScriptTriggersPerTable;
		private System.Windows.Forms.ToolStripMenuItem mnuSaveUpdateTrigToBuildFile;
        private System.Windows.Forms.SaveFileDialog saveUpdateTrigBuildFile;
		private System.Windows.Forms.Label lblPK;
        private System.Windows.Forms.Label lblMissingCols;
        private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.Label lblNotInDb;
        private ToolStripContainer toolStripContainer1;
        private ToolStripSeparator menuItem4;
        private ToolStripSeparator menuItem6;
        private ToolStripSeparator menuItem3;
        private ToolStripSeparator menuItem1;
        private ContextMenuStrip ctxCandidate;
        private ToolStripMenuItem mnuAddCandidateTable;
        private ToolStripMenuItem mnuAddNewTable;
        private BackgroundWorker bgTriggerScript;
		private bool allowBuildManagerExport = false;
		public CodeTableScriptingForm(string fileName)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			this.projectFileName = fileName;

		}
		public CodeTableScriptingForm(ConnectionData data)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			this.data = data;

		}
		public CodeTableScriptingForm(ConnectionData data,bool allowBuildManagerExport):this(data)
		{
			this.allowBuildManagerExport = allowBuildManagerExport;	

		}


		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CodeTableScriptingForm));
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Included (with Triggers)", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Included (Missing Triggers)", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup3 = new System.Windows.Forms.ListViewGroup("Excluded (Have Triggers and Columns)", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup4 = new System.Windows.Forms.ListViewGroup("Candidate (Have Some Audit Columns)", System.Windows.Forms.HorizontalAlignment.Left);
            this.contextMenu1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuWhereClause = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem6 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuScriptMissingColumns = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuScriptTrigger = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuScriptDefault = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuAddTable = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuDeleteTable = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuCopyTables = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCopyTablesForsql = new System.Windows.Forms.ToolStripMenuItem();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.statusBar1 = new System.Windows.Forms.StatusBar();
            this.statStatus = new System.Windows.Forms.StatusBarPanel();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.grpScripting = new System.Windows.Forms.GroupBox();
            this.btnGenerateScripts = new System.Windows.Forms.Button();
            this.chkSelectByDate = new System.Windows.Forms.CheckBox();
            this.dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            this.tcTables = new System.Windows.Forms.TabControl();
            this.toolBar1 = new System.Windows.Forms.ToolBar();
            this.tbbSave = new System.Windows.Forms.ToolBarButton();
            this.tbbExportToBM = new System.Windows.Forms.ToolBarButton();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.ddDatabaseList = new System.Windows.Forms.ComboBox();
            this.contextDatabase = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuRemoveDatabase = new System.Windows.Forms.ToolStripMenuItem();
            this.label3 = new System.Windows.Forms.Label();
            this.lnkCheckAll = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.lstTables = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.mainMenu1 = new System.Windows.Forms.MenuStrip();
            this.mnuActionMain = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuLoadProjectFile = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuChangeSqlServer = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem4 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuFileMRU = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSettings = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuGoSeparators = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuIncludeUpdateStatements = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuUpdateDateAndId = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem7 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuScriptTriggers = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSaveUpdateTrigToBuildFile = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuScriptTriggersOneFile = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuScriptTriggersPerTable = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuScriptAllMissingColumns = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuScriptAllColumnDefaults = new System.Windows.Forms.ToolStripMenuItem();
            this.openBuildManager = new System.Windows.Forms.OpenFileDialog();
            this.panel1 = new System.Windows.Forms.Panel();
            this.grpDatabaseInfo = new System.Windows.Forms.GroupBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.lblNotInDb = new System.Windows.Forms.Label();
            this.lblPK = new System.Windows.Forms.Label();
            this.lblMissingCols = new System.Windows.Forms.Label();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.panel2 = new System.Windows.Forms.Panel();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.saveUpdateTrigBuildFile = new System.Windows.Forms.SaveFileDialog();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            this.settingsControl1 = new SqlSync.SettingsControl();
            this.ctxCandidate = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuAddCandidateTable = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAddNewTable = new System.Windows.Forms.ToolStripMenuItem();
            this.bgTriggerScript = new System.ComponentModel.BackgroundWorker();
            this.contextMenu1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.statStatus)).BeginInit();
            this.grpScripting.SuspendLayout();
            this.contextDatabase.SuspendLayout();
            this.mainMenu1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.grpDatabaseInfo.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.toolStripContainer1.ContentPanel.SuspendLayout();
            this.toolStripContainer1.TopToolStripPanel.SuspendLayout();
            this.toolStripContainer1.SuspendLayout();
            this.ctxCandidate.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenu1
            // 
            this.contextMenu1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuWhereClause,
            this.menuItem6,
            this.mnuScriptMissingColumns,
            this.mnuScriptTrigger,
            this.mnuScriptDefault,
            this.menuItem3,
            this.mnuAddTable,
            this.mnuDeleteTable,
            this.menuItem1,
            this.mnuCopyTables,
            this.mnuCopyTablesForsql});
            this.contextMenu1.Name = "contextMenu1";
            this.contextMenu1.Size = new System.Drawing.Size(313, 198);
            this.contextMenu1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenu1_Opening);
            // 
            // mnuWhereClause
            // 
            this.mnuWhereClause.MergeIndex = 0;
            this.mnuWhereClause.Name = "mnuWhereClause";
            this.mnuWhereClause.Size = new System.Drawing.Size(312, 22);
            this.mnuWhereClause.Text = "Add/View \"Where\" Clause";
            this.mnuWhereClause.Click += new System.EventHandler(this.mnuWhereClause_Click);
            // 
            // menuItem6
            // 
            this.menuItem6.MergeIndex = 1;
            this.menuItem6.Name = "menuItem6";
            this.menuItem6.Size = new System.Drawing.Size(309, 6);
            // 
            // mnuScriptMissingColumns
            // 
            this.mnuScriptMissingColumns.Enabled = false;
            this.mnuScriptMissingColumns.MergeIndex = 2;
            this.mnuScriptMissingColumns.Name = "mnuScriptMissingColumns";
            this.mnuScriptMissingColumns.Size = new System.Drawing.Size(312, 22);
            this.mnuScriptMissingColumns.Text = "Script For Missing Create/Update Columns";
            this.mnuScriptMissingColumns.Click += new System.EventHandler(this.mnuScriptMissingColumns_Click);
            // 
            // mnuScriptTrigger
            // 
            this.mnuScriptTrigger.Enabled = false;
            this.mnuScriptTrigger.MergeIndex = 3;
            this.mnuScriptTrigger.Name = "mnuScriptTrigger";
            this.mnuScriptTrigger.Size = new System.Drawing.Size(312, 22);
            this.mnuScriptTrigger.Text = "Script For Update Trigger";
            this.mnuScriptTrigger.Click += new System.EventHandler(this.mnuScriptTrigger_Click);
            // 
            // mnuScriptDefault
            // 
            this.mnuScriptDefault.MergeIndex = 4;
            this.mnuScriptDefault.Name = "mnuScriptDefault";
            this.mnuScriptDefault.Size = new System.Drawing.Size(312, 22);
            this.mnuScriptDefault.Text = "Script to Reset Create/Update Column Defaults";
            this.mnuScriptDefault.Click += new System.EventHandler(this.mnuScriptDefault_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.MergeIndex = 5;
            this.menuItem3.Name = "menuItem3";
            this.menuItem3.Size = new System.Drawing.Size(309, 6);
            // 
            // mnuAddTable
            // 
            this.mnuAddTable.MergeIndex = 6;
            this.mnuAddTable.Name = "mnuAddTable";
            this.mnuAddTable.Size = new System.Drawing.Size(312, 22);
            this.mnuAddTable.Text = "Add New Table to List";
            this.mnuAddTable.Click += new System.EventHandler(this.mnuAddNewTable_Click);
            // 
            // mnuDeleteTable
            // 
            this.mnuDeleteTable.MergeIndex = 7;
            this.mnuDeleteTable.Name = "mnuDeleteTable";
            this.mnuDeleteTable.Size = new System.Drawing.Size(312, 22);
            this.mnuDeleteTable.Text = "Remove Selected Table";
            this.mnuDeleteTable.Click += new System.EventHandler(this.mnuDeleteTable_Click);
            // 
            // menuItem1
            // 
            this.menuItem1.MergeIndex = 8;
            this.menuItem1.Name = "menuItem1";
            this.menuItem1.Size = new System.Drawing.Size(309, 6);
            // 
            // mnuCopyTables
            // 
            this.mnuCopyTables.MergeIndex = 9;
            this.mnuCopyTables.Name = "mnuCopyTables";
            this.mnuCopyTables.Size = new System.Drawing.Size(312, 22);
            this.mnuCopyTables.Text = "Copy Table List";
            this.mnuCopyTables.Click += new System.EventHandler(this.mnuCopyTables_Click);
            // 
            // mnuCopyTablesForsql
            // 
            this.mnuCopyTablesForsql.MergeIndex = 10;
            this.mnuCopyTablesForsql.Name = "mnuCopyTablesForsql";
            this.mnuCopyTablesForsql.Size = new System.Drawing.Size(312, 22);
            this.mnuCopyTablesForsql.Text = "Copy Table List for Sql";
            this.mnuCopyTablesForsql.Click += new System.EventHandler(this.mnuCopyTablesForsql_Click);
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.Description = "Select a folder to save your scripts";
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(0, 624);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
            this.statStatus});
            this.statusBar1.ShowPanels = true;
            this.statusBar1.Size = new System.Drawing.Size(944, 22);
            this.statusBar1.TabIndex = 6;
            this.statusBar1.Text = "statusBar1";
            // 
            // statStatus
            // 
            this.statStatus.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
            this.statStatus.Name = "statStatus";
            this.statStatus.Width = 927;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.CheckFileExists = false;
            this.openFileDialog1.DefaultExt = "xml";
            this.openFileDialog1.Filter = "Sql Table Script Files *.sts|*.sts|XML Files|*.xml|All Files|*.*";
            this.openFileDialog1.Title = "Open SQL Sync Project File";
            // 
            // grpScripting
            // 
            this.grpScripting.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grpScripting.Controls.Add(this.btnGenerateScripts);
            this.grpScripting.Controls.Add(this.chkSelectByDate);
            this.grpScripting.Controls.Add(this.dateTimePicker1);
            this.grpScripting.Controls.Add(this.tcTables);
            this.grpScripting.Controls.Add(this.toolBar1);
            this.grpScripting.Enabled = false;
            this.grpScripting.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.grpScripting.Location = new System.Drawing.Point(2, 2);
            this.grpScripting.Name = "grpScripting";
            this.grpScripting.Size = new System.Drawing.Size(631, 560);
            this.grpScripting.TabIndex = 11;
            this.grpScripting.TabStop = false;
            this.grpScripting.Enter += new System.EventHandler(this.grpScripting_Enter);
            // 
            // btnGenerateScripts
            // 
            this.btnGenerateScripts.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnGenerateScripts.Location = new System.Drawing.Point(24, 16);
            this.btnGenerateScripts.Name = "btnGenerateScripts";
            this.btnGenerateScripts.Size = new System.Drawing.Size(128, 23);
            this.btnGenerateScripts.TabIndex = 21;
            this.btnGenerateScripts.Text = "Generate Scripts";
            this.btnGenerateScripts.Click += new System.EventHandler(this.btnGenerateScripts_Click);
            // 
            // chkSelectByDate
            // 
            this.chkSelectByDate.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.chkSelectByDate.Font = new System.Drawing.Font("Verdana", 7.5F);
            this.chkSelectByDate.Location = new System.Drawing.Point(176, 17);
            this.chkSelectByDate.Name = "chkSelectByDate";
            this.chkSelectByDate.Size = new System.Drawing.Size(200, 20);
            this.chkSelectByDate.TabIndex = 20;
            this.chkSelectByDate.Text = "Select Rows Updated On or After:";
            this.chkSelectByDate.CheckedChanged += new System.EventHandler(this.chkSelectByDate_CheckedChanged);
            // 
            // dateTimePicker1
            // 
            this.dateTimePicker1.Cursor = System.Windows.Forms.Cursors.IBeam;
            this.dateTimePicker1.Enabled = false;
            this.dateTimePicker1.Font = new System.Drawing.Font("Verdana", 7.5F);
            this.dateTimePicker1.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            this.dateTimePicker1.Location = new System.Drawing.Point(376, 17);
            this.dateTimePicker1.MaxDate = new System.DateTime(3000, 12, 31, 0, 0, 0, 0);
            this.dateTimePicker1.MinDate = new System.DateTime(2005, 1, 1, 0, 0, 0, 0);
            this.dateTimePicker1.Name = "dateTimePicker1";
            this.dateTimePicker1.Size = new System.Drawing.Size(112, 20);
            this.dateTimePicker1.TabIndex = 19;
            // 
            // tcTables
            // 
            this.tcTables.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tcTables.Location = new System.Drawing.Point(8, 56);
            this.tcTables.Multiline = true;
            this.tcTables.Name = "tcTables";
            this.tcTables.SelectedIndex = 0;
            this.tcTables.Size = new System.Drawing.Size(617, 496);
            this.tcTables.TabIndex = 11;
            this.tcTables.SelectedIndexChanged += new System.EventHandler(this.tcTables_SelectedIndexChanged);
            // 
            // toolBar1
            // 
            this.toolBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.toolBar1.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
            this.toolBar1.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
            this.tbbSave,
            this.tbbExportToBM});
            this.toolBar1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolBar1.DropDownArrows = true;
            this.toolBar1.ImageList = this.imageList1;
            this.toolBar1.Location = new System.Drawing.Point(561, 31);
            this.toolBar1.Name = "toolBar1";
            this.toolBar1.ShowToolTips = true;
            this.toolBar1.Size = new System.Drawing.Size(56, 28);
            this.toolBar1.TabIndex = 22;
            this.toolBar1.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar1_ButtonClick);
            // 
            // tbbSave
            // 
            this.tbbSave.ImageIndex = 6;
            this.tbbSave.Name = "tbbSave";
            this.tbbSave.Tag = "SaveAll";
            this.tbbSave.ToolTipText = "Save to File";
            // 
            // tbbExportToBM
            // 
            this.tbbExportToBM.ImageIndex = 5;
            this.tbbExportToBM.Name = "tbbExportToBM";
            this.tbbExportToBM.Tag = "export";
            this.tbbExportToBM.ToolTipText = "Export All Scripts to New Sql Build Manager File";
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "");
            this.imageList1.Images.SetKeyName(1, "Cut-2.png");
            this.imageList1.Images.SetKeyName(2, "Save.png");
            this.imageList1.Images.SetKeyName(3, "");
            this.imageList1.Images.SetKeyName(4, "");
            this.imageList1.Images.SetKeyName(5, "");
            this.imageList1.Images.SetKeyName(6, "Save All.png");
            // 
            // ddDatabaseList
            // 
            this.ddDatabaseList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.ddDatabaseList.ContextMenuStrip = this.contextDatabase;
            this.ddDatabaseList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddDatabaseList.Location = new System.Drawing.Point(12, 30);
            this.ddDatabaseList.Name = "ddDatabaseList";
            this.ddDatabaseList.Size = new System.Drawing.Size(271, 21);
            this.ddDatabaseList.TabIndex = 0;
            this.ddDatabaseList.SelectionChangeCommitted += new System.EventHandler(this.ddDatabaseList_SelectionChangeCommitted);
            // 
            // contextDatabase
            // 
            this.contextDatabase.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuRemoveDatabase});
            this.contextDatabase.Name = "contextDatabase";
            this.contextDatabase.Size = new System.Drawing.Size(237, 26);
            // 
            // mnuRemoveDatabase
            // 
            this.mnuRemoveDatabase.MergeIndex = 0;
            this.mnuRemoveDatabase.Name = "mnuRemoveDatabase";
            this.mnuRemoveDatabase.Size = new System.Drawing.Size(236, 22);
            this.mnuRemoveDatabase.Text = "Remove Database (and Tables)";
            this.mnuRemoveDatabase.Click += new System.EventHandler(this.mnuRemoveDatabase_Click);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(12, 14);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(116, 16);
            this.label3.TabIndex = 15;
            this.label3.Text = "Select Database:";
            // 
            // lnkCheckAll
            // 
            this.lnkCheckAll.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkCheckAll.Location = new System.Drawing.Point(116, 58);
            this.lnkCheckAll.Name = "lnkCheckAll";
            this.lnkCheckAll.Size = new System.Drawing.Size(171, 16);
            this.lnkCheckAll.TabIndex = 1;
            this.lnkCheckAll.TabStop = true;
            this.lnkCheckAll.Text = "Check All";
            this.lnkCheckAll.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkCheckAll_LinkClicked);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 58);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 16);
            this.label1.TabIndex = 9;
            this.label1.Text = "Tables to Script";
            // 
            // lstTables
            // 
            this.lstTables.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.lstTables.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstTables.CheckBoxes = true;
            this.lstTables.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.lstTables.ContextMenuStrip = this.contextMenu1;
            this.lstTables.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstTables.FullRowSelect = true;
            listViewGroup1.Header = "Included (with Triggers)";
            listViewGroup1.Name = "Included";
            listViewGroup2.Header = "Included (Missing Triggers)";
            listViewGroup2.Name = "IncludedNoTrigger";
            listViewGroup3.Header = "Excluded (Have Triggers and Columns)";
            listViewGroup3.Name = "Excluded";
            listViewGroup4.Header = "Candidate (Have Some Audit Columns)";
            listViewGroup4.Name = "Candidate";
            this.lstTables.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2,
            listViewGroup3,
            listViewGroup4});
            this.lstTables.Location = new System.Drawing.Point(14, 80);
            this.lstTables.Name = "lstTables";
            this.lstTables.ShowItemToolTips = true;
            this.lstTables.Size = new System.Drawing.Size(271, 442);
            this.lstTables.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lstTables.TabIndex = 8;
            this.lstTables.UseCompatibleStateImageBehavior = false;
            this.lstTables.View = System.Windows.Forms.View.Details;
            this.lstTables.MouseUp += new System.Windows.Forms.MouseEventHandler(this.lstTables_MouseUp);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Table Name";
            this.columnHeader1.Width = 189;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Rows";
            // 
            // mainMenu1
            // 
            this.mainMenu1.Dock = System.Windows.Forms.DockStyle.None;
            this.mainMenu1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuActionMain,
            this.mnuSettings,
            this.menuItem7});
            this.mainMenu1.Location = new System.Drawing.Point(0, 0);
            this.mainMenu1.Name = "mainMenu1";
            this.mainMenu1.Size = new System.Drawing.Size(944, 24);
            this.mainMenu1.TabIndex = 0;
            // 
            // mnuActionMain
            // 
            this.mnuActionMain.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuLoadProjectFile,
            this.mnuChangeSqlServer,
            this.menuItem4,
            this.mnuFileMRU});
            this.mnuActionMain.Image = global::SqlSync.Properties.Resources.Execute;
            this.mnuActionMain.MergeIndex = 0;
            this.mnuActionMain.Name = "mnuActionMain";
            this.mnuActionMain.Size = new System.Drawing.Size(65, 20);
            this.mnuActionMain.Text = "&Action";
            // 
            // mnuLoadProjectFile
            // 
            this.mnuLoadProjectFile.Image = global::SqlSync.Properties.Resources.Open;
            this.mnuLoadProjectFile.MergeIndex = 0;
            this.mnuLoadProjectFile.Name = "mnuLoadProjectFile";
            this.mnuLoadProjectFile.Size = new System.Drawing.Size(231, 22);
            this.mnuLoadProjectFile.Text = "&Load/New  Configuration File";
            this.mnuLoadProjectFile.Click += new System.EventHandler(this.mnuLoadProjectFile_Click);
            // 
            // mnuChangeSqlServer
            // 
            this.mnuChangeSqlServer.Image = global::SqlSync.Properties.Resources.Server1;
            this.mnuChangeSqlServer.MergeIndex = 1;
            this.mnuChangeSqlServer.Name = "mnuChangeSqlServer";
            this.mnuChangeSqlServer.Size = new System.Drawing.Size(231, 22);
            this.mnuChangeSqlServer.Text = "&Change Sql Server Connection";
            this.mnuChangeSqlServer.Click += new System.EventHandler(this.mnuChangeSqlServer_Click);
            // 
            // menuItem4
            // 
            this.menuItem4.MergeIndex = 2;
            this.menuItem4.Name = "menuItem4";
            this.menuItem4.Size = new System.Drawing.Size(228, 6);
            // 
            // mnuFileMRU
            // 
            this.mnuFileMRU.MergeIndex = 3;
            this.mnuFileMRU.Name = "mnuFileMRU";
            this.mnuFileMRU.Size = new System.Drawing.Size(231, 22);
            this.mnuFileMRU.Text = "Recent Files";
            // 
            // mnuSettings
            // 
            this.mnuSettings.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuGoSeparators,
            this.mnuIncludeUpdateStatements,
            this.mnuUpdateDateAndId});
            this.mnuSettings.Image = global::SqlSync.Properties.Resources.Wizard;
            this.mnuSettings.MergeIndex = 1;
            this.mnuSettings.Name = "mnuSettings";
            this.mnuSettings.Size = new System.Drawing.Size(74, 20);
            this.mnuSettings.Text = "Settings";
            // 
            // mnuGoSeparators
            // 
            this.mnuGoSeparators.Checked = true;
            this.mnuGoSeparators.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mnuGoSeparators.MergeIndex = 0;
            this.mnuGoSeparators.Name = "mnuGoSeparators";
            this.mnuGoSeparators.Size = new System.Drawing.Size(298, 22);
            this.mnuGoSeparators.Text = "Add Batch \"GO\" Separators";
            this.mnuGoSeparators.Click += new System.EventHandler(this.SettingsItem_Click);
            // 
            // mnuIncludeUpdateStatements
            // 
            this.mnuIncludeUpdateStatements.Checked = true;
            this.mnuIncludeUpdateStatements.CheckState = System.Windows.Forms.CheckState.Checked;
            this.mnuIncludeUpdateStatements.MergeIndex = 1;
            this.mnuIncludeUpdateStatements.Name = "mnuIncludeUpdateStatements";
            this.mnuIncludeUpdateStatements.Size = new System.Drawing.Size(298, 22);
            this.mnuIncludeUpdateStatements.Text = "Include Update Statements When Applicable";
            this.mnuIncludeUpdateStatements.Click += new System.EventHandler(this.SettingsItem_Click);
            // 
            // mnuUpdateDateAndId
            // 
            this.mnuUpdateDateAndId.MergeIndex = 2;
            this.mnuUpdateDateAndId.Name = "mnuUpdateDateAndId";
            this.mnuUpdateDateAndId.Size = new System.Drawing.Size(298, 22);
            this.mnuUpdateDateAndId.Text = "Replace UpdateDate and UpdateId Values";
            this.mnuUpdateDateAndId.Click += new System.EventHandler(this.SettingsItem_Click);
            // 
            // menuItem7
            // 
            this.menuItem7.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuScriptTriggers,
            this.mnuScriptAllMissingColumns,
            this.mnuScriptAllColumnDefaults});
            this.menuItem7.MergeIndex = 2;
            this.menuItem7.Name = "menuItem7";
            this.menuItem7.Size = new System.Drawing.Size(96, 20);
            this.menuItem7.Text = "Update Triggers";
            // 
            // mnuScriptTriggers
            // 
            this.mnuScriptTriggers.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuSaveUpdateTrigToBuildFile,
            this.mnuScriptTriggersOneFile,
            this.mnuScriptTriggersPerTable});
            this.mnuScriptTriggers.MergeIndex = 0;
            this.mnuScriptTriggers.Name = "mnuScriptTriggers";
            this.mnuScriptTriggers.Size = new System.Drawing.Size(314, 22);
            this.mnuScriptTriggers.Text = "Script Checked For Update Column Triggers";
            // 
            // mnuSaveUpdateTrigToBuildFile
            // 
            this.mnuSaveUpdateTrigToBuildFile.MergeIndex = 0;
            this.mnuSaveUpdateTrigToBuildFile.Name = "mnuSaveUpdateTrigToBuildFile";
            this.mnuSaveUpdateTrigToBuildFile.Size = new System.Drawing.Size(190, 22);
            this.mnuSaveUpdateTrigToBuildFile.Text = "Save to New Build File";
            this.mnuSaveUpdateTrigToBuildFile.Click += new System.EventHandler(this.mnuSaveUpdateTrigToBuildFile_Click);
            // 
            // mnuScriptTriggersOneFile
            // 
            this.mnuScriptTriggersOneFile.MergeIndex = 1;
            this.mnuScriptTriggersOneFile.Name = "mnuScriptTriggersOneFile";
            this.mnuScriptTriggersOneFile.Size = new System.Drawing.Size(190, 22);
            this.mnuScriptTriggersOneFile.Text = "Single Sql File";
            this.mnuScriptTriggersOneFile.Click += new System.EventHandler(this.mnuScriptTriggersOneFile_Click);
            // 
            // mnuScriptTriggersPerTable
            // 
            this.mnuScriptTriggersPerTable.MergeIndex = 2;
            this.mnuScriptTriggersPerTable.Name = "mnuScriptTriggersPerTable";
            this.mnuScriptTriggersPerTable.Size = new System.Drawing.Size(190, 22);
            this.mnuScriptTriggersPerTable.Text = "One File Per Table";
            this.mnuScriptTriggersPerTable.Click += new System.EventHandler(this.mnuScriptTriggers_Click);
            // 
            // mnuScriptAllMissingColumns
            // 
            this.mnuScriptAllMissingColumns.MergeIndex = 1;
            this.mnuScriptAllMissingColumns.Name = "mnuScriptAllMissingColumns";
            this.mnuScriptAllMissingColumns.Size = new System.Drawing.Size(314, 22);
            this.mnuScriptAllMissingColumns.Text = "Script All For Missing Create/Update Columns";
            this.mnuScriptAllMissingColumns.Click += new System.EventHandler(this.mnuScriptAllMissingColumns_Click);
            // 
            // mnuScriptAllColumnDefaults
            // 
            this.mnuScriptAllColumnDefaults.MergeIndex = 2;
            this.mnuScriptAllColumnDefaults.Name = "mnuScriptAllColumnDefaults";
            this.mnuScriptAllColumnDefaults.Size = new System.Drawing.Size(314, 22);
            this.mnuScriptAllColumnDefaults.Text = "Script To Reset Create/Update Column Defaults";
            this.mnuScriptAllColumnDefaults.Click += new System.EventHandler(this.mnuScriptAllColumnDefaults_Click);
            // 
            // openBuildManager
            // 
            this.openBuildManager.CheckFileExists = false;
            this.openBuildManager.DefaultExt = "xml";
            this.openBuildManager.Filter = "Sql Build Manager Project (*.sbm)|*.sbm|Sql Build Export File (*.sbe)|*.sbe|Zip F" +
                "iles (*.zip)|*.zip|All Files|*.*";
            this.openBuildManager.Title = "Open SQL Sync Build Project File";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.grpDatabaseInfo);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 56);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(303, 568);
            this.panel1.TabIndex = 13;
            // 
            // grpDatabaseInfo
            // 
            this.grpDatabaseInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grpDatabaseInfo.Controls.Add(this.groupBox1);
            this.grpDatabaseInfo.Controls.Add(this.lnkCheckAll);
            this.grpDatabaseInfo.Controls.Add(this.label3);
            this.grpDatabaseInfo.Controls.Add(this.lstTables);
            this.grpDatabaseInfo.Controls.Add(this.ddDatabaseList);
            this.grpDatabaseInfo.Controls.Add(this.label1);
            this.grpDatabaseInfo.Enabled = false;
            this.grpDatabaseInfo.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.grpDatabaseInfo.Location = new System.Drawing.Point(2, 2);
            this.grpDatabaseInfo.Name = "grpDatabaseInfo";
            this.grpDatabaseInfo.Size = new System.Drawing.Size(295, 560);
            this.grpDatabaseInfo.TabIndex = 0;
            this.grpDatabaseInfo.TabStop = false;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox1.Controls.Add(this.lblNotInDb);
            this.groupBox1.Controls.Add(this.lblPK);
            this.groupBox1.Controls.Add(this.lblMissingCols);
            this.groupBox1.Location = new System.Drawing.Point(8, 522);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(260, 32);
            this.groupBox1.TabIndex = 16;
            this.groupBox1.TabStop = false;
            // 
            // lblNotInDb
            // 
            this.lblNotInDb.BackColor = System.Drawing.Color.White;
            this.lblNotInDb.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblNotInDb.ForeColor = System.Drawing.Color.LightGray;
            this.lblNotInDb.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblNotInDb.Location = new System.Drawing.Point(154, 12);
            this.lblNotInDb.Name = "lblNotInDb";
            this.lblNotInDb.Size = new System.Drawing.Size(93, 16);
            this.lblNotInDb.TabIndex = 6;
            this.lblNotInDb.Text = "Missing from DB";
            this.lblNotInDb.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolTip1.SetToolTip(this.lblNotInDb, "Project table is missing from Database on the current server.");
            // 
            // lblPK
            // 
            this.lblPK.BackColor = System.Drawing.SystemColors.Control;
            this.lblPK.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblPK.ForeColor = System.Drawing.Color.Red;
            this.lblPK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblPK.Location = new System.Drawing.Point(85, 12);
            this.lblPK.Name = "lblPK";
            this.lblPK.Size = new System.Drawing.Size(69, 16);
            this.lblPK.TabIndex = 2;
            this.lblPK.Text = "Missing PK";
            this.lblPK.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolTip1.SetToolTip(this.lblPK, "Included Table is missing either a PK or \"WHERE\" clause to allow for specific dat" +
                    "a matches");
            // 
            // lblMissingCols
            // 
            this.lblMissingCols.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblMissingCols.ForeColor = System.Drawing.Color.Orange;
            this.lblMissingCols.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.lblMissingCols.Location = new System.Drawing.Point(14, 12);
            this.lblMissingCols.Name = "lblMissingCols";
            this.lblMissingCols.Size = new System.Drawing.Size(71, 16);
            this.lblMissingCols.TabIndex = 1;
            this.lblMissingCols.Text = "Missing Cols";
            this.lblMissingCols.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.toolTip1.SetToolTip(this.lblMissingCols, "Included table is missing at least 1 audit column.\r\n(Refer to item tool tip for s" +
                    "pecifics)");
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(303, 56);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 568);
            this.splitter1.TabIndex = 14;
            this.splitter1.TabStop = false;
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.grpScripting);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(306, 56);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(638, 568);
            this.panel2.TabIndex = 15;
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "Sql Files *.sql|*.sql|All Files *.*|*.*";
            this.saveFileDialog1.Title = "Select File For Full Audit";
            // 
            // saveUpdateTrigBuildFile
            // 
            this.saveUpdateTrigBuildFile.DefaultExt = "sbm";
            this.saveUpdateTrigBuildFile.Filter = "Sql Manager Build File *.sbm|*.sbm|All Files *.*|*.*";
            this.saveUpdateTrigBuildFile.Title = "Create New Sql Manager Build File";
            // 
            // toolStripContainer1
            // 
            // 
            // toolStripContainer1.ContentPanel
            // 
            this.toolStripContainer1.ContentPanel.AutoScroll = true;
            this.toolStripContainer1.ContentPanel.Controls.Add(this.panel2);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.splitter1);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.panel1);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.statusBar1);
            this.toolStripContainer1.ContentPanel.Controls.Add(this.settingsControl1);
            this.toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(944, 646);
            this.toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            this.toolStripContainer1.Name = "toolStripContainer1";
            this.toolStripContainer1.Size = new System.Drawing.Size(944, 670);
            this.toolStripContainer1.TabIndex = 16;
            this.toolStripContainer1.Text = "toolStripContainer1";
            // 
            // toolStripContainer1.TopToolStripPanel
            // 
            this.toolStripContainer1.TopToolStripPanel.Controls.Add(this.mainMenu1);
            // 
            // settingsControl1
            // 
            this.settingsControl1.BackColor = System.Drawing.Color.White;
            this.settingsControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.settingsControl1.Location = new System.Drawing.Point(0, 0);
            this.settingsControl1.Name = "settingsControl1";
            this.settingsControl1.Project = "";
            this.settingsControl1.ProjectLabelText = "Configuration File:";
            this.settingsControl1.Server = "";
            this.settingsControl1.Size = new System.Drawing.Size(944, 56);
            this.settingsControl1.TabIndex = 12;
            this.settingsControl1.ServerChanged += new SqlSync.ServerChangedEventHandler(this.settingsControl1_ServerChanged);
            // 
            // ctxCandidate
            // 
            this.ctxCandidate.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuAddCandidateTable,
            this.mnuAddNewTable});
            this.ctxCandidate.Name = "ctxCandidate";
            this.ctxCandidate.Size = new System.Drawing.Size(290, 48);
            // 
            // mnuAddCandidateTable
            // 
            this.mnuAddCandidateTable.MergeIndex = 11;
            this.mnuAddCandidateTable.Name = "mnuAddCandidateTable";
            this.mnuAddCandidateTable.Size = new System.Drawing.Size(289, 22);
            this.mnuAddCandidateTable.Text = "Add Candiate/Excluded Table(s) to Project";
            this.mnuAddCandidateTable.Click += new System.EventHandler(this.mnuAddCandidateTable_Click);
            // 
            // mnuAddNewTable
            // 
            this.mnuAddNewTable.MergeIndex = 6;
            this.mnuAddNewTable.Name = "mnuAddNewTable";
            this.mnuAddNewTable.Size = new System.Drawing.Size(289, 22);
            this.mnuAddNewTable.Text = "Add New Table to List";
            this.mnuAddNewTable.Click += new System.EventHandler(this.mnuAddNewTable_Click);
            // 
            // bgTriggerScript
            // 
            this.bgTriggerScript.WorkerReportsProgress = true;
            this.bgTriggerScript.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgTriggerScript_DoWork);
            this.bgTriggerScript.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgTriggerScript_RunWorkerCompleted);
            this.bgTriggerScript.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgTriggerScript_ProgressChanged);
            // 
            // CodeTableScriptingForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(944, 670);
            this.Controls.Add(this.toolStripContainer1);
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.mainMenu1;
            this.Name = "CodeTableScriptingForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sql Build Manager ::  Code Table Scripting and Auditing";
            this.Load += new System.EventHandler(this.LookUpTable_Load);
            this.contextMenu1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.statStatus)).EndInit();
            this.grpScripting.ResumeLayout(false);
            this.grpScripting.PerformLayout();
            this.contextDatabase.ResumeLayout(false);
            this.mainMenu1.ResumeLayout(false);
            this.mainMenu1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.grpDatabaseInfo.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.toolStripContainer1.ContentPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            this.toolStripContainer1.TopToolStripPanel.PerformLayout();
            this.toolStripContainer1.ResumeLayout(false);
            this.toolStripContainer1.PerformLayout();
            this.ctxCandidate.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		private void LookUpTable_Load(object sender, System.EventArgs e)
		{
			if(this.data == null)
			{
				ConnectionForm frmConnect = new ConnectionForm("Sql Table Script");
				DialogResult result =  frmConnect.ShowDialog();
				if(result == DialogResult.OK)
				{
					this.data = frmConnect.SqlConnection;
				}
				else
				{
					MessageBox.Show("Sql Table Script can not continue without a valid Sql Connection","Unable to Load",MessageBoxButtons.OK,MessageBoxIcon.Hand);
					this.Close();
				}

			}

			this.mruManager = new MRUManager();
			this.mruManager.Initialize(
				this,                              // owner form
                mnuActionMain,
				mnuFileMRU,                        // Recent Files menu item
				@"Software\Michael McKechney\Sql Sync\Sql Table Scripting"); // Registry path to keep MRU list
			this.mruManager.MaxDisplayNameLength = 40;

			this.dateTimePicker1.Value = DateTime.Now.AddDays(-30);
			this.settingsControl1.Server =  this.data.SQLServerName;
			if(this.projectFileName.Length > 0)
				this.LoadLookUpTableData(this.projectFileName,true);

			if(this.allowBuildManagerExport)
				this.tbbExportToBM.ToolTipText = "Export All Scripts to current Sql Build Manager File";


		}

		private void LoadLookUpTableData(string fileName,bool validateSchema)
		{
			bool successfulLoad = true;
			this.Cursor = Cursors.WaitCursor;
			try
			{
				string configFile = fileName; //Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) +@"\LookUpTables.xml";
				this.tableList = new SqlSync.TableScript.SQLSyncData();
				if(File.Exists(configFile))
				{
					//Read the table list
					try
					{
						bool isValid = true;
						if(validateSchema)
						{
							string path = Path.GetDirectoryName( System.Reflection.Assembly.GetEntryAssembly().Location );
							SchemaValidator validator = new SchemaValidator();
							isValid =  validator.ValidateAgainstSchema(fileName,
								path + @"\"+ SchemaEnums.LookUpTableProjectXsd,
								SchemaEnums.LookUpTableProjectNamespace);
						}

						if(isValid)
						{
							this.tableList.ReadXml(configFile);
						}
						else
						{
							successfulLoad = false;
						}
					}
					catch
					{
						successfulLoad = false;
					}

				}
				else
				{
					this.tableList.Database.AddDatabaseRow(this.data.DatabaseName);
					this.tableList.LookUpTable.AddLookUpTableRow("","",false,"",(SqlSync.TableScript.SQLSyncData.DatabaseRow)tableList.Database.Rows[0]);
					this.tableList.WriteXml(configFile);
				}
				if(successfulLoad)
				{
					this.projectFileName = configFile;
					this.settingsControl1.Project = configFile;
					grpScripting.Enabled = true;
					grpDatabaseInfo.Enabled = true;
					this.mruManager.Add(configFile);
					PopulateDatabaseList(this.tableList.Database);
				}
				else
				{
					MessageBox.Show("Unable to Read the selected file.\r\nIt is not a valid table scripting file","Invalid File",MessageBoxButtons.OK,MessageBoxIcon.Error);
				}
			}
			finally
			{
				this.Cursor = Cursors.Default;
			}
		}
		private void PopulateDatabaseList(SqlSync.TableScript.SQLSyncData.DatabaseDataTable table)
		{
			string currentSelection = string.Empty;
			if(this.ddDatabaseList.Items.Count > 0 && this.ddDatabaseList.SelectedItem != null &&
				this.ddDatabaseList.SelectedItem.ToString() != DropDownConstants.AddNew)
			{
				currentSelection = this.ddDatabaseList.SelectedItem.ToString();
			}
			this.ddDatabaseList.Items.Clear();
			this.lstTables.Items.Clear();
			DataView view = table.DefaultView;
			view.Sort = table.NameColumn.ColumnName + " ASC";
			ddDatabaseList.Items.Add(DropDownConstants.SelectOne);
			this.ddDatabaseList.SelectedIndex = 0;
			for(int i=0;i<view.Count;i++)
			{
				ddDatabaseList.Items.Add(view[i].Row[table.NameColumn.ColumnName].ToString());
				if(currentSelection.Length > 0 && 
					currentSelection == view[i].Row[table.NameColumn.ColumnName].ToString())
				{
					ddDatabaseList.SelectedIndex = i+1;
				}
			}
			//If there's only 1 database, select it.
			if(view.Count == 1)
				ddDatabaseList.SelectedIndex = 1;

			ddDatabaseList.Items.Add(DropDownConstants.AddNew);

			if(ddDatabaseList.SelectedIndex > 0)
			{
				this.PopulateTableList(ddDatabaseList.SelectedItem.ToString());
			}
			
		}
        private void CombineTableData(ref SortedDictionary<string, CodeTableAudit> auditTables, SqlSync.DbInformation.TableSize[] tables, UpdateAutoDetectData[] updateDetect)
        {
            CodeTableAudit aud;
            for (int i = 0; i < tables.Length; i++)
                if(auditTables.TryGetValue(tables[i].TableName,out aud))
                    aud.RowCount = tables[i].RowCount;

            for (int i = 0; i < updateDetect.Length; i++)
            {
                if (auditTables.TryGetValue(updateDetect[i].TableName, out aud))
                {
                    aud.HasUpdateTrigger = updateDetect[i].HasUpdateTrigger;
                }
                else
                {
                    aud = new CodeTableAudit();
                    aud.TableName = updateDetect[i].TableName;
                    aud.HasUpdateTrigger = updateDetect[i].HasUpdateTrigger;
                    auditTables.Add(updateDetect[i].TableName,aud);
                }
            }
        }
        private void PopulateTableList(string databaseName)
		{
			this.statStatus.Text = "Retrieving Table List and Row Count";
			this.lstTables.Items.Clear();
			this.data.DatabaseName = databaseName;
			PopulateHelper helper = new PopulateHelper(this.data,null);

            //Pull the row for the database config, if available
			DataRow[] dbRows =  this.tableList.Database.Select(tableList.Database.NameColumn.ColumnName + " = '"+this.data.DatabaseName+"'");
			
            //Get table size data for all the tables in the DB. 
            SqlSync.DbInformation.TableSize[] tableSizeList = SqlSync.DbInformation.InfoHelper.GetDatabaseTableListWithRowCount(this.data);	
			
            //Auto detect audit tables by virtue of the auditing triggers
            UpdateAutoDetectData[] updateDetect = SqlSync.TableScript.PopulateHelper.AutoDetectUpdateTriggers(this.data, tableSizeList);
            
            //Get tables with potential audit columns
			System.Collections.Generic.SortedDictionary<string, CodeTableAudit> auditTables = SqlSync.DbInformation.InfoHelper.GetTablesWithAuditColumns(this.data);
			
            //Combine all of the data into one object for easier handling.
            CombineTableData(ref auditTables, tableSizeList, updateDetect);
            
            //Create a new table object to store the rows. 
            SqlSync.TableScript.SQLSyncData.LookUpTableDataTable lookTable = null;
			//int rowCount = -1;
			//If the current database has been configed, get tables defined in the config file.
			if(dbRows.Length > 0)
			{
				SqlSync.TableScript.SQLSyncData.LookUpTableRow[] lookUpRows =  ((SQLSyncData.DatabaseRow)dbRows[0]).GetLookUpTableRows();
				//Add the rows to a table so we can sort by Name
				lookTable = new SqlSync.TableScript.SQLSyncData.LookUpTableDataTable();
                CodeTableAudit aud;
				for(int i=0;i<lookUpRows.Length;i++)
                {
					lookTable.ImportRow(lookUpRows[i]);
                    if(auditTables.TryGetValue(lookUpRows[i].Name,out aud))
                        aud.LookUpTableRow = lookUpRows[i];
                    else
                    {
                        aud = new CodeTableAudit();
                        aud.TableName = lookUpRows[i].Name;
                        aud.LookUpTableRow = lookUpRows[i];
                        auditTables.Add(lookUpRows[i].Name,aud);
                    }
                }
			}
            foreach(KeyValuePair<string,CodeTableAudit> auditTable in auditTables)
			{
                //Make sure we set the row count.
                if (auditTable.Value.RowCount < 0)
                    for (int i = 0; i < tableSizeList.Length; i++)
                        if (auditTable.Value.TableName.ToLower() == tableSizeList[i].TableName.ToLower())
                        {
                            auditTable.Value.RowCount = tableSizeList[i].RowCount;
                            break;
                        }

				ListViewItem item;
                item = new ListViewItem(new string[] { auditTable.Value.TableName, auditTable.Value.StrRowCount });
                item.Tag = auditTable.Value;
                item.ToolTipText = auditTable.Value.TableName + "\r\n" +
                    "Create ID:\t" + (auditTable.Value.CreateIdColumn.Length > 0 ? auditTable.Value.CreateIdColumn : "<Missing>") +
                    "\r\nCreate Date:\t" + (auditTable.Value.CreateDateColumn.Length > 0 ? auditTable.Value.CreateDateColumn : "<Missing>") +
                    "\r\nUpdate ID:\t" + (auditTable.Value.UpdateIdColumn.Length > 0 ? auditTable.Value.UpdateIdColumn : "<Missing>") +
                    "\r\nUpdate Date:\t" + (auditTable.Value.UpdateDateColumn.Length > 0 ? auditTable.Value.UpdateDateColumn : "<Missing>") +
                    "\r\nAudit Trigger?\t" + auditTable.Value.HasUpdateTrigger.ToString();

                //Set color
				//Table is missing!
                if (auditTable.Value.RowCount == -1)
				{
					item.ForeColor = lblNotInDb.ForeColor;
					item.SubItems[1].Text = "0";
				}
                else if (auditTable.Value.UpdateDateColumn.Length == 0 ||
                    auditTable.Value.UpdateIdColumn.Length == 0 ||
                    auditTable.Value.CreateIdColumn.Length == 0 ||
                    auditTable.Value.CreateDateColumn.Length == 0) //table is missing updateid or UpdateDate columns
                {
                    item.ForeColor = lblMissingCols.ForeColor;
                }
                else if (auditTable.Value.LookUpTableRow != null)
                {
                    SqlSync.TableScript.SQLSyncData.LookUpTableRow configRow = (SqlSync.TableScript.SQLSyncData.LookUpTableRow)auditTable.Value.LookUpTableRow;
                    if (configRow.WhereClause.Length == 0 && configRow.CheckKeyColumns.Length == 0)
                        item.ForeColor = lblPK.ForeColor; //Missing where clause or unique key values
                }


                if (auditTable.Value.HasUpdateTrigger && auditTable.Value.LookUpTableRow == null)
                    item.Group = lstTables.Groups["Excluded"];
                else if (auditTable.Value.LookUpTableRow == null)
                    item.Group = lstTables.Groups["Candidate"];
                else if(auditTable.Value.HasUpdateTrigger)
                    item.Group = lstTables.Groups["Included"];
                else
                    item.Group = lstTables.Groups["IncludedNoTrigger"];

                lstTables.Items.Add(item);
			
			}
				
            ////Add "candidate" tables
            //for(int i=0;i<updateByTables.Length;i++)
            //{
            //    for(int j=0;j<updateDateTables.Length;j++)
            //    {
            //        bool found = false;
            //        if(updateByTables[i] == updateDateTables[j])
            //        {
            //            for(int y=0;y<lstTables.Items.Count;y++)
            //            {
            //                if(lstTables.Items[y].SubItems[0].Text == updateByTables[i])
            //                {
            //                    found = true;
            //                    break;
            //                }
            //            }
            //            if(!found)
            //            {
            //                ListViewItem candItem = new ListViewItem(new string[]{updateByTables[i],""});
            //                candItem.Group = lstTables.Groups["Candidate"];
            //                lstTables.Items.Add(candItem);

            //            }
            //            break;
            //        }
            //    }
            //}
			this.statStatus.Text = "Ready.";
		}
		private void AddNewDatabase()
		{
			string[] listedDatabases = new string[this.ddDatabaseList.Items.Count -2];
			for(int i=1;i<this.ddDatabaseList.Items.Count-1;i++)
			{
				listedDatabases[i-1] = this.ddDatabaseList.Items[i].ToString();
			}

            NewLookUpDatabaseForm frmNewDb = new NewLookUpDatabaseForm(new PopulateHelper(this.data, null).RemainingDatabases(listedDatabases));
			DialogResult result = frmNewDb.ShowDialog();
			if(result == DialogResult.OK)
			{
				string[] newDatabases = frmNewDb.DatabaseList;
				SaveNewDatabaseNames(newDatabases);
			}
		}
		private void SaveNewDatabaseNames(string[] newDatabases)
		{
			for(int i=0;i<newDatabases.Length;i++)
			{
				this.tableList.Database.AddDatabaseRow(newDatabases[i]);
			}
			this.SaveXmlTemplate(this.projectFileName);
			this.LoadLookUpTableData(this.projectFileName,false);
		}

		private void SaveXmlTemplate(string fileName)
		{
	
			try
			{
				this.tableList.WriteXml(fileName);
			}
			catch(System.UnauthorizedAccessException)
			{
				MessageBox.Show("Unable to save project file to:\r\n"+fileName,"Unable to Save",MessageBoxButtons.OK,MessageBoxIcon.Error);
			}
		}

		private void RemoveDatabase(string databaseName)
		{
			DataRow[] deleteRows = this.tableList.Database.Select(this.tableList.Database.NameColumn.ColumnName +"='"+databaseName+"'");
			if(deleteRows.Length > 0)
			{
				SQLSyncData.DatabaseRow dbRow = (SQLSyncData.DatabaseRow)deleteRows[0];
				int childRowsCount = dbRow.GetLookUpTableRows().Length;
				if( childRowsCount > 0)
				{
					string entry = (childRowsCount > 1)? "entries":"entry";
					string message = "The Database "+dbRow.Name +" has "+childRowsCount.ToString()+" table "+entry+".\r\nAre you sure you want to remove it?";
					DialogResult result = MessageBox.Show(message,"Confirm Removal",MessageBoxButtons.YesNo,MessageBoxIcon.Question);
					if(result == DialogResult.No) return;
				}

				this.tableList.Database.RemoveDatabaseRow(dbRow);
				this.tableList.Database.AcceptChanges();
				this.SaveXmlTemplate(this.projectFileName);
				this.LoadLookUpTableData(this.projectFileName,false);
			}
		}
		private void RemoveTable(string tableName)
		{
			DataRow[] deleteRows =  this.tableList.LookUpTable.Select(this.tableList.LookUpTable.NameColumn.ColumnName + "='"+tableName+"'");
			if(deleteRows.Length > 0)
			{
				SQLSyncData.LookUpTableRow tblRow = (SQLSyncData.LookUpTableRow)deleteRows[0];
				if(tblRow.WhereClause.Length > 0)
				{
					string message = "The table "+tblRow.Name+" has a WHERE clause.\r\nAre you sure you want to remove it?";
					DialogResult result = MessageBox.Show(message,"Confirm Removal",MessageBoxButtons.YesNo,MessageBoxIcon.Question);
					if(result == DialogResult.No) return;
				}

				this.tableList.LookUpTable.RemoveLookUpTableRow(tblRow);
				this.tableList.LookUpTable.AcceptChanges();
				this.SaveXmlTemplate(this.projectFileName);
				this.LoadLookUpTableData(this.projectFileName,false);

			}
		}
		private void AddNewTables(List<string> tables)
		{
			DataRow[] rows =  tableList.Database.Select(tableList.Database.NameColumn.ColumnName +" ='"+this.data.DatabaseName +"'");
			if(rows.Length ==  0)
			{

				tableList.Database.AddDatabaseRow(this.data.DatabaseName);
				rows =  tableList.Database.Select(tableList.Database.NameColumn.ColumnName +" ='"+this.data.DatabaseName +"'");
			}
			for(int  i=0;i<tables.Count;i++)
			{
				string[] pks = SqlSync.DbInformation.InfoHelper.GetPrimaryKeyColumns(tables[i],this.data);
				string pklist = String.Join(",",pks);
				this.tableList.LookUpTable.AddLookUpTableRow(tables[i],"",false,pklist,(SQLSyncData.DatabaseRow)rows[0]);
			}
			this.SaveXmlTemplate(this.projectFileName);
			this.LoadLookUpTableData(this.projectFileName,false);
		}
		private void lnkCheckAll_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			if(lnkCheckAll.Text == "Check All")
			{
				for(int i=0;i<lstTables.Items.Count;i++)
					lstTables.Items[i].Checked = true;

				lnkCheckAll.Text = "Uncheck All";
			}
			else
			{
				while(this.lstTables.CheckedItems.Count > 0)
					this.lstTables.CheckedItems[0].Checked = false;

				lnkCheckAll.Text = "Check All";
			}
		}

		private void lnkGenerateScripts_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			
		}

		private void lnkSaveScripts_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			DialogResult result = folderBrowserDialog1.ShowDialog();
			if(result == DialogResult.OK)
			{
				this.Cursor = Cursors.WaitCursor;
				try
				{
					statStatus.Text = "Saving Scripts";
					this.Cursor = Cursors.WaitCursor;

					foreach(TabPage page in tcTables.TabPages)
					{
						foreach(Control ctrl in page.Controls)
						{
							if(ctrl.GetType() == typeof(PopulateScriptDisplay))
							{

								statStatus.Text = "Saving:"+ ((PopulateScriptDisplay)ctrl).ScriptName;
								((PopulateScriptDisplay)ctrl).SaveScript(folderBrowserDialog1.SelectedPath);
								break;
							}
						}
					}
				}
				finally
				{
					this.Cursor = Cursors.WaitCursor;
				}
			
			}
			statStatus.Text = "Save complete. Ready.";

			this.Cursor = Cursors.Default;
		}


		private void mnuDeleteTable_Click(object sender, System.EventArgs e)
		{
			if(lstTables.SelectedItems.Count > 0)
			{
				ListViewItem item = lstTables.SelectedItems[0];
				this.RemoveTable(item.Text);
			}
		}

		private void mnuWhereClause_Click(object sender, System.EventArgs e)
		{
			if(lstTables.SelectedItems.Count > 0)
			{
				ListViewItem item = lstTables.SelectedItems[0];
				SQLSyncData.LookUpTableRow lookUpRow;
				DataRow[] rows =  tableList.Database.Select(tableList.Database.NameColumn.ColumnName +" ='"+this.data.DatabaseName +"'");
				if(rows.Length > 0)
				{
					DataRow[] lookUpRows = ((SQLSyncData.DatabaseRow)rows[0]).GetLookUpTableRows();
					for(int i=0;i<lookUpRows.Length;i++)
					{
						SQLSyncData.DatabaseRow dbRow = (SQLSyncData.DatabaseRow)rows[0];
						if(((SQLSyncData.LookUpTableRow)lookUpRows[i]).Name.ToUpper() == item.Text.ToUpper())
						{
							lookUpRow = (SQLSyncData.LookUpTableRow)lookUpRows[i];
							string[] updateKeycols = lookUpRow.CheckKeyColumns.Split(',');
							WhereClauseForm frmWhere = new WhereClauseForm(this.data, item.Text,lookUpRow.WhereClause,lookUpRow.UseAsFullSelect,updateKeycols);
							DialogResult result = frmWhere.ShowDialog();
							if(result == DialogResult.OK)
							{
								lookUpRow.WhereClause = frmWhere.WhereClause;
								lookUpRow.UseAsFullSelect = frmWhere.UseAsFullSelect;
								lookUpRow.CheckKeyColumns = String.Join(",",frmWhere.CheckKeyColumns);
								lookUpRow.AcceptChanges();
								this.SaveXmlTemplate(this.projectFileName);
								break;
							}
						}
					}
				}
			}
		}
		

		private void ddDatabaseList_SelectionChangeCommitted(object sender, System.EventArgs e)
		{
			this.Cursor = Cursors.WaitCursor;
			try
			{
				switch(ddDatabaseList.SelectedItem.ToString())
				{
					case DropDownConstants.SelectOne:
						this.lstTables.Items.Clear();
						break;
					case DropDownConstants.AddNew:
						this.AddNewDatabase();
						break;
					default:
						this.PopulateTableList(ddDatabaseList.Text);
						break;
				}
			}
			finally
			{
				this.Cursor = Cursors.Default;
			}
			
		}

		private void mnuRemoveDatabase_Click(object sender, System.EventArgs e)
		{
			this.RemoveDatabase(this.ddDatabaseList.SelectedItem.ToString());
		}

		private void tcTables_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			this.Size = new System.Drawing.Size(this.Width+1,this.Height);
			this.Size = new System.Drawing.Size(this.Width-1,this.Height);

		}

	

		private void mnuLoadProjectFile_Click(object sender, System.EventArgs e)
		{
			DialogResult result = openFileDialog1.ShowDialog();
			if(result == DialogResult.OK)
			{
				if(File.Exists( openFileDialog1.FileName) == false)
				{
					this.tableList = new SQLSyncData();
					this.SaveXmlTemplate(openFileDialog1.FileName);
				}
				OpenMRUFile(openFileDialog1.FileName);
				
			}
		}
		private void mnuChangeSqlServer_Click(object sender, System.EventArgs e)
		{
			ConnectionForm frmConnect = new ConnectionForm("Sql Table Script");
			DialogResult result = frmConnect.ShowDialog();
			if(result == DialogResult.OK)
			{
				this.data = frmConnect.SqlConnection;
				this.settingsControl1.Server = this.data.SQLServerName;
			}
		}

		private void SaveSqlFilesToNewBuildFile(string buildFileName, string directory)
		{
			string[] files = Directory.GetFiles(directory);

			if(File.Exists(buildFileName))
			{
				SqlBuild.SqlBuildForm frmBuild = new SqlBuildForm(buildFileName, this.data);
				frmBuild.Show();
				frmBuild.BulkAdd(files);
			}
			else
			{

                SqlBuildFileHelper.SaveSqlFilesToNewBuildFile(buildFileName, files.ToList(), this.data.DatabaseName);
				
				files = Directory.GetFiles(directory);
				for(int i=0;i<files.Length;i++)
					File.Delete(files[i]);
				Directory.Delete(directory,true);
				
			}
		}
		private void SaveToNewBuildFile()
		{
			DialogResult result =  this.openBuildManager.ShowDialog();
			if(result == DialogResult.OK)
			{
				string buildFileName = this.openBuildManager.FileName;
				string directory = Path.GetDirectoryName(this.openBuildManager.FileName)+@"\~sts";
				Directory.CreateDirectory(directory);

				foreach(TabPage page in tcTables.TabPages)
				{
					foreach(Control ctrl in page.Controls)
					{
						if(ctrl.GetType() == typeof(PopulateScriptDisplay))
						{

							statStatus.Text = "Saving:"+ ((PopulateScriptDisplay)ctrl).ScriptName;
							((PopulateScriptDisplay)ctrl).SaveScript(directory);
							break;
						}
					}
				}
				SaveSqlFilesToNewBuildFile(buildFileName,directory);
				
					
			}
		}
		

		private void ExportToAttachedBuildFile()
		{
			if(this.SqlBuildManagerFileExport != null)
			{
				ArrayList lst = new ArrayList();
				string name;
				string path = Path.GetTempPath();
				foreach(TabPage page in tcTables.TabPages)
				{
					foreach(Control ctrl in page.Controls)
					{
						if(ctrl.GetType() == typeof(PopulateScriptDisplay))
						{

							statStatus.Text = "Saving:"+ ((PopulateScriptDisplay)ctrl).ScriptName;
							name = ((PopulateScriptDisplay)ctrl).SaveScript(path);
							lst.Add(name);
							break;
						}
					}
				}
				string[] fileList = new string[lst.Count];
				lst.CopyTo(fileList);
				this.SqlBuildManagerFileExport(this,new SqlBuildManagerFileExportEventArgs(fileList));
			}


		}

		private void mnuScriptDefault_Click(object sender, System.EventArgs e)
		{

            CodeTableAudit codeTable = (CodeTableAudit)this.lstTables.SelectedItems[0].Tag;
            string script = GetDefaultReset(codeTable);
            ScriptDisplayForm frmDisplay = new ScriptDisplayForm(script, this.data.SQLServerName, "Default Name Resets for " + codeTable.TableName);
            frmDisplay.ShowDialog();

		}
        private string GetDefaultReset(CodeTableAudit codeTable)
		{
            return PopulateHelper.ScriptColumnDefaultsReset(codeTable);
		}
		private void mnuScriptAllColumnDefaults_Click(object sender, System.EventArgs e)
		{
			if(DialogResult.OK == folderBrowserDialog1.ShowDialog())
			{
				string script;
				string folder = folderBrowserDialog1.SelectedPath;
				for(int i=0;i<this.lstTables.Items.Count;i++)
				{
                    CodeTableAudit codeTable = (CodeTableAudit)this.lstTables.Items[i].Tag;
                    script = GetDefaultReset(codeTable);
					if(script.Length == 0)
						continue;

                    statStatus.Text = "Creating default reset scripts for " + codeTable.TableName;
                    using (StreamWriter sw = File.CreateText(folder + @"\" + codeTable.TableName + " - Default Reset.sql"))
					{
						sw.WriteLine(script);
						sw.Flush();
						sw.Close();
					}
				}
				statStatus.Text = "Complete";
			}
		}
		private void mnuScriptMissingColumns_Click(object sender, System.EventArgs e)
		{
            CodeTableAudit codeTable = (CodeTableAudit)this.lstTables.SelectedItems[0].Tag;
            string script = GetMissingScript(codeTable);
            ScriptDisplayForm frmDisplay = new ScriptDisplayForm(script, this.data.SQLServerName, "Missing Audit Columns for " + codeTable.TableName);
            frmDisplay.ShowDialog();

		}
		private string GetMissingScript(CodeTableAudit codeTable)
		{
            return PopulateHelper.ScriptForMissingColumns(codeTable);
		}
		private void mnuScriptAllMissingColumns_Click(object sender, System.EventArgs e)
		{
			if(DialogResult.OK == folderBrowserDialog1.ShowDialog())
			{
				string script;
				string folder = folderBrowserDialog1.SelectedPath;
				for(int i=0;i<this.lstTables.Items.Count;i++)
				{
                    CodeTableAudit codeTable = (CodeTableAudit)this.lstTables.Items[i].Tag;
                    script = GetMissingScript(codeTable);
					if(script.Length == 0)
						continue;

                    statStatus.Text = "Creating alter table scripts for " + codeTable.TableName;
                    using (StreamWriter sw = File.CreateText(folder + @"\" + codeTable.TableName + " - Update Columns.sql"))
					{
						sw.WriteLine(script);
						sw.Flush();
						sw.Close();
					}
				}
				statStatus.Text = "Complete";
			}
		}

		private bool IncludeTable(ListViewItem listItem, bool needsChecked)
		{
            if ((listItem.Group.Name != "Included" && listItem.Group.Name != "IncludedNoTrigger") ||
				listItem.ForeColor == lblNotInDb.ForeColor ||
				(needsChecked && listItem.Checked == false))
				return false;
			else
				return true;
		}
		private void chkSelectByDate_CheckedChanged(object sender, System.EventArgs e)
		{
			if(chkSelectByDate.Checked)
				this.dateTimePicker1.Enabled = true;
			else
				this.dateTimePicker1.Enabled = false;
		}


		private void mnuCopyTablesForsql_Click(object sender, System.EventArgs e)
		{
			StringBuilder sb = new StringBuilder();
			for(int i=0;i<this.lstTables.Items.Count;i++)
			{
				sb.Append("'"+this.lstTables.Items[i].Text+"',");
			}
			sb.Length = sb.Length -1;
			Clipboard.SetDataObject(sb.ToString(),true);
		}

		private void mnuCopyTables_Click(object sender, System.EventArgs e)
		{
			StringBuilder sb = new StringBuilder();
			for(int i=0;i<this.lstTables.Items.Count;i++)
			{
				sb.Append(this.lstTables.Items[i].Text+"\r\n");
			}
			sb.Length = sb.Length -2;
			Clipboard.SetDataObject(sb.ToString(),true);
		}

		private void SettingsItem_Click(object sender, System.EventArgs e)
		{
			((ToolStripMenuItem)sender).Checked = !((ToolStripMenuItem)sender).Checked;
		}

		private void btnGenerateScripts_Click(object sender, System.EventArgs e)
		{
			this.Cursor = Cursors.WaitCursor;
			bool alertNonIncuded = false;
			this.statStatus.Text = "Retrieving Data; Generating Scripts";
			try
			{
				//Clear out any old controls
				tcTables.Controls.Clear();

				//Get the list of selected tables
				StringBuilder sb = new StringBuilder();
				for(int i=0;i<lstTables.Items.Count;i++)
				{
					if(lstTables.Items[i].Checked && lstTables.Items[i].ForeColor != lblNotInDb.ForeColor)
					{
						if(lstTables.Items[i].Group.Name == "Candidate")
							alertNonIncuded = true;

						sb.Append("'"+lstTables.Items[i].Text+"'");
						if(i<lstTables.Items.Count-1) sb.Append(",");
					}
				}
				
				if(alertNonIncuded)
					MessageBox.Show("Note: The Checked candidate tables will not be scripted.\r\nPlease add them to the configuration file before generating scripts","Candidate Tables Selected",MessageBoxButtons.OK,MessageBoxIcon.Information);
                if (sb.Length == 0)
                {
                    MessageBox.Show("Note: Please check at least one table from the list on the left.", "No Tables Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

				//Get their "ExistsExcludeColumns" if any
				DataRow[] rows = this.tableList.LookUpTable.Select(this.tableList.LookUpTable.NameColumn.ColumnName +" IN ("+sb.ToString()+")");
				DataRow parentRow = this.tableList.Database.Select(tableList.Database.NameColumn.ColumnName +" ='"+this.data.DatabaseName +"'")[0];
				ArrayList ruleList = new ArrayList();
				for(int i=0;i<rows.Length;i++)
				{
					SqlSync.TableScript.SQLSyncData.LookUpTableRow current = (SqlSync.TableScript.SQLSyncData.LookUpTableRow)rows[i];
					if(current.DatabaseRow.Equals(parentRow))
					{
						TableScriptingRule rule = new TableScriptingRule();
						rule.TableName = current.Name;
						rule.CheckKeyColumns = current.CheckKeyColumns.Split(',');
						ruleList.Add(rule);
					}
				}
				TableScriptingRule[] rules = new TableScriptingRule[ruleList.Count];
				ruleList.CopyTo(rules);

				//Run the scripting
				PopulateHelper helper = new PopulateHelper(this.data,rules);
				helper.SyncData = this.tableList;
				helper.ReplaceDateAndId = mnuUpdateDateAndId.Checked;
				helper.IncludeUpdates = mnuIncludeUpdateStatements.Checked;
				helper.AddBatchGoSeparators = mnuGoSeparators.Checked;

				if(chkSelectByDate.Checked)
					helper.SelectByUpdateDate = dateTimePicker1.Value;
			
				TableScriptData[] scriptedTables =  helper.GeneratePopulateScripts();
			
				//Add a tab page per generated script
				this.statStatus.Text = "Generating script displays.";
				for(int i=0;i<scriptedTables.Length;i++)
				{
					TabPage page = new TabPage();
					page.BorderStyle = BorderStyle.Fixed3D;
					page.Text = scriptedTables[i].TableName;
					PopulateScriptDisplay disp = new PopulateScriptDisplay(scriptedTables[i],this.allowBuildManagerExport);
					disp.SqlBuildManagerFileExport +=new SqlBuildManagerFileExportHandler(disp_SqlBuildManagerFileExport);
					disp.ScriptText = scriptedTables[i].InsertScript;
					disp.ScriptName = scriptedTables[i].TableName;
					disp.ScriptDataTable = scriptedTables[i].ValuesTable;
					disp.Size = page.Size;
					disp.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
					page.Controls.Add(disp);
					tcTables.Controls.Add(page);

				}
				this.Size = new System.Drawing.Size(this.Width+1,this.Height);
			}
			finally
			{
				this.Cursor = Cursors.Default;
				this.statStatus.Text = "Ready.";

			}
		}

		private void grpScripting_Enter(object sender, System.EventArgs e)
		{
		
		}
		#region IMRUClient Members

		public void OpenMRUFile(string fileName)
		{
			this.Cursor = Cursors.WaitCursor;
			try
			{
				if(System.IO.File.Exists(fileName))
				{
					this.projectFileName = fileName;
					this.LoadLookUpTableData(this.projectFileName,false);
				}
			}
			finally
			{
				this.Cursor= Cursors.Default;
			}
		}

		#endregion

		public event SqlBuildManagerFileExportHandler SqlBuildManagerFileExport;

		private void disp_SqlBuildManagerFileExport(object sender, SqlBuildManagerFileExportEventArgs e)
		{
			if(this.SqlBuildManagerFileExport != null)
				this.SqlBuildManagerFileExport(sender,e);
		}

		private void toolBar1_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			switch(e.Button.Tag.ToString().ToLower())
			{
				case "saveall":
					DialogResult result = folderBrowserDialog1.ShowDialog();
					if(result == DialogResult.OK)
					{
						this.Cursor = Cursors.WaitCursor;
						try
						{
							statStatus.Text = "Saving Scripts";
							this.Cursor = Cursors.WaitCursor;

							foreach(TabPage page in tcTables.TabPages)
							{
								foreach(Control ctrl in page.Controls)
								{
									if(ctrl.GetType() == typeof(PopulateScriptDisplay))
									{

										statStatus.Text = "Saving:"+ ((PopulateScriptDisplay)ctrl).ScriptName;
										((PopulateScriptDisplay)ctrl).SaveScript(folderBrowserDialog1.SelectedPath);
										break;
									}
								}
							}
						}
						finally
						{
							this.Cursor = Cursors.WaitCursor;
						}
			
					}
					statStatus.Text = "Save complete. Ready.";

					this.Cursor = Cursors.Default;
					break;
				case "export":
					if(this.allowBuildManagerExport)
						ExportToAttachedBuildFile();
					else
						SaveToNewBuildFile();
					break;
			}
		}

		private void mnuAuditTable_Click(object sender, System.EventArgs e)
		{
			if(this.lstTables.SelectedItems.Count > 0)
			{
                TableConfig table = new TableConfig(this.lstTables.SelectedItems[0].Text, this.lstTables.SelectedItems[0].Tag as SQLSyncAuditingDatabaseTableToAudit);
                string script = AuditHelper.GetAuditScript(table, AuditScriptType.CreateAuditTable, this.data);
				if(DialogResult.Yes == MessageBox.Show(script+"\r\nCopy to Clipboard?","Copy?",MessageBoxButtons.YesNo,MessageBoxIcon.Question))
					Clipboard.SetDataObject(script,true);
			}
		}

		private void mnuMasterTrx_Click(object sender, System.EventArgs e)
		{
		
		}

		private void mnuScriptAllAudit_Click(object sender, System.EventArgs e)
		{
			if(DialogResult.OK == folderBrowserDialog1.ShowDialog())
			{
				string script;
				TableConfig table;
				string folder = folderBrowserDialog1.SelectedPath;
				for(int i=0;i<this.lstTables.Items.Count;i++)
				{
					table = new TableConfig(this.lstTables.Items[i].Text, this.lstTables.Items[i].Tag as SQLSyncAuditingDatabaseTableToAudit);
					script = SqlSync.TableScript.Audit.AuditHelper.GetAuditScript(table,AuditScriptType.CreateAuditTable, this.data);
					if(script.Length == 0)
						continue;

					statStatus.Text = "Creating audit table scripts for "+table;
					using(StreamWriter sw = File.CreateText(folder + @"\"+table+" - Audit Table.sql"))
					{
						sw.WriteLine(script);
						sw.Flush();
						sw.Close();
					}
				}
				statStatus.Text = "Complete";
			}
		}

        //private void mnuAuditTrigger_Click(object sender, System.EventArgs e)
        //{
        //    if(DialogResult.OK == folderBrowserDialog1.ShowDialog())
        //    {
        //        string script;
        //        string table;
        //        string folder = folderBrowserDialog1.SelectedPath;
        //        for(int i=0;i<this.lstTables.Items.Count;i++)
        //        {
        //            table = this.lstTables.Items[i].Text;
        //            script = SqlSync.TableScript.Audit.AuditHelper.ScriptForAuditTriggers(table,this.data,this.use);
        //            if(script.Length == 0)
        //                continue;

        //            statStatus.Text = "Creating audit trigger scripts for "+table;
        //            using(StreamWriter sw = File.CreateText(folder + @"\"+table+" - Audit Trigger.sql"))
        //            {
        //                sw.WriteLine(script);
        //                sw.Flush();
        //                sw.Close();
        //            }
        //        }
        //        statStatus.Text = "Complete";
        //    }
        //}

        //private void mnuSingleAuditTrigger_Click(object sender, System.EventArgs e)
        //{
        //    if(this.lstTables.SelectedItems.Count > 0)
        //    {
        //        string table = this.lstTables.SelectedItems[0].Text;
        //        string script = AuditHelper.ScriptForAuditTriggers(table,this.data);
        //        if(DialogResult.Yes == MessageBox.Show(script+"\r\nCopy to Clipboard?","Copy?",MessageBoxButtons.YesNo,MessageBoxIcon.Question))
        //            Clipboard.SetDataObject(script,true);
        //    }
        //}

		private void mnuCompleteAudit_Click(object sender, System.EventArgs e)
		{
//			saveFileDialog1.FileName = this.data.DatabaseName +" Audit.sql";
//			if(DialogResult.OK == saveFileDialog1.ShowDialog())
//			{
//				this.statStatus.Text = "Creating Audit Script";
//				string[] tables = new string[this.lstTables.Items.Count];
//				for(int i=0;i<this.lstTables.Items.Count;i++)
//					tables[i] = this.lstTables.Items[i].Text;
//					
//				string audit = SqlSync.TableScript.Audit.AuditHelper.ScriptForCompleteAudit(tables,this.data);
//				using(StreamWriter sw = File.CreateText(saveFileDialog1.FileName))
//				{
//					sw.WriteLine(audit);
//					sw.Flush();
//					sw.Close();
//				}
//				this.statStatus.Text = "Ready.";
//			}
		}

		private void mnuDisableAuditTrigs_Click(object sender, System.EventArgs e)
		{
//			saveFileDialog1.FileName = this.data.DatabaseName +" Disable Triggers.sql";
//			if(DialogResult.OK == saveFileDialog1.ShowDialog())
//			{
//				this.statStatus.Text = "Creating Disable Triggers Script";
//				string[] tables = new string[this.lstTables.Items.Count];
//				for(int i=0;i<this.lstTables.Items.Count;i++)
//					tables[i] = this.lstTables.Items[i].Text;
//					
//				string audit = SqlSync.TableScript.Audit.AuditHelper.ScriptForCompleteTriggerEnableDisable(tables,this.data,false);
//				using(StreamWriter sw = File.CreateText(saveFileDialog1.FileName))
//				{
//					sw.WriteLine(audit);
//					sw.Flush();
//					sw.Close();
//				}
//				this.statStatus.Text = "Ready.";
//			}
		}

		private void mnuEnableAuditTrigs_Click(object sender, System.EventArgs e)
		{
//			saveFileDialog1.FileName = this.data.DatabaseName +" Enable Triggers.sql";
//			if(DialogResult.OK == saveFileDialog1.ShowDialog())
//			{
//				this.statStatus.Text = "Creating Enable Triggers Script";
//				string[] tables = new string[this.lstTables.Items.Count];
//				for(int i=0;i<this.lstTables.Items.Count;i++)
//					tables[i] = this.lstTables.Items[i].Text;
//					
//				string audit = SqlSync.TableScript.Audit.AuditHelper.ScriptForCompleteTriggerEnableDisable(tables,this.data,true);
//				using(StreamWriter sw = File.CreateText(saveFileDialog1.FileName))
//				{
//					sw.WriteLine(audit);
//					sw.Flush();
//					sw.Close();
//				}
//				this.statStatus.Text = "Ready.";
//			}
		}

		private void OpenResultMessage(string fileName, bool triggerWarning)
		{
            string message;
            if (triggerWarning)
                message = "Scripting Complete.\r\nNOTE: Any tables missing update columns were not scripted.\r\n Open Results?";
            else
                message = "Scripting Complete. Open Results?";
            try
            {

                if (DialogResult.Yes == MessageBox.Show(message, "Open Results", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                {
                    System.Diagnostics.Process prc = new System.Diagnostics.Process();
                    prc.StartInfo.FileName = fileName;
                    prc.Start();
                }
            }
            catch (Exception exe)
            {
                MessageBox.Show("Unable to open results.\r\n" + exe.Message, "Open Results Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
		}

		private void mnuAddCandidateTable_Click(object sender, System.EventArgs e)
		{
			if(lstTables.SelectedItems.Count == 0)
				return;

            List<String> tables = new List<String>();
            for (int i = 0; i < lstTables.SelectedItems.Count; i++)
                tables.Add(lstTables.SelectedItems[i].Text);

			AddNewTables(tables);
		}

        private void settingsControl1_ServerChanged(object sender, string serverName, string username, string password, AuthenticationType authType)
        {
            Connection.ConnectionData oldConnData = new Connection.ConnectionData();
            this.data.Fill(oldConnData);
            this.Cursor = Cursors.WaitCursor;

            this.data.SQLServerName = serverName;
            if (!string.IsNullOrWhiteSpace(username) && (!string.IsNullOrWhiteSpace(password)))
            {
                this.data.UserId = username;
                this.data.Password = password;
            }
            this.data.AuthenticationType = authType;
            this.data.ScriptTimeout = 5;
            try
            {
                DatabaseList dbList = SqlSync.DbInformation.InfoHelper.GetDatabaseList(this.data);
                //this.LookUpTable_Load(null, EventArgs.Empty);
            }
            catch
            {
                MessageBox.Show("Error retrieving database list. Is the server running?", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.data = oldConnData;
                this.settingsControl1.Server = oldConnData.SQLServerName;
            }


            this.Cursor = Cursors.Default;
        }


        private void mnuAddNewTable_Click(object sender, EventArgs e)
        {
            List<string> listedTables = new List<string>();
            for (int i = 0; i < this.lstTables.Items.Count; i++)
                listedTables.Add(this.lstTables.Items[i].Text);

            NewLookUpForm frmNew = new NewLookUpForm(new PopulateHelper(this.data, null).RemainingTables(listedTables));
            DialogResult result = frmNew.ShowDialog();
           
            if (result == DialogResult.OK)
                this.AddNewTables(frmNew.TableList);
        }

        private void lstTables_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Right)
            {
                if (lstTables.SelectedItems.Count > 0)
                    if (lstTables.SelectedItems[0].Group.Name != "Included" && 
                        lstTables.SelectedItems[0].Group.Name != "IncludedNoTrigger")
                        lstTables.ContextMenuStrip = ctxCandidate;
                    else
                        lstTables.ContextMenuStrip = contextMenu1;
            }
        }

        private void contextMenu1_Opening(object sender, CancelEventArgs e)
        {
            if (this.lstTables.SelectedItems.Count > 0)
            {
                CodeTableAudit codeTable = (CodeTableAudit)this.lstTables.SelectedItems[0].Tag;
                if(codeTable.UpdateDateColumn.Length == 0 || codeTable.UpdateIdColumn.Length == 0 ||
                    codeTable.CreateDateColumn.Length == 0 || codeTable.CreateIdColumn.Length == 0)
                    mnuScriptMissingColumns.Enabled = true;
                else
                    mnuScriptMissingColumns.Enabled = false;

                if(codeTable.HasUpdateTrigger == false)
                    mnuScriptTrigger.Enabled = true;
                else
                    mnuScriptTrigger.Enabled = false;
               
            }
        }

        #region .: Trigger Scripting :.
        private void mnuScriptTriggersOneFile_Click(object sender, System.EventArgs e)
        {
            if (DialogResult.OK == saveFileDialog1.ShowDialog())
            {
                 string file = saveFileDialog1.FileName;
                List<CodeTableAudit> codeTables = new List<CodeTableAudit>();
                for (int i = 0; i < this.lstTables.CheckedItems.Count; i++)
                    codeTables.Add((CodeTableAudit)this.lstTables.CheckedItems[i].Tag);

                TriggerScriptData args = new TriggerScriptData(TriggerScriptType.SingleFile, file, codeTables);

                this.Cursor = Cursors.AppStarting;
                bgTriggerScript.RunWorkerAsync(args);
            }
        }
        private void mnuScriptTrigger_Click(object sender, System.EventArgs e)
        {
            if (this.lstTables.SelectedItems.Count > 0)
            {
                CodeTableAudit codeTable = (CodeTableAudit)this.lstTables.SelectedItems[0].Tag;
                string script = PopulateHelper.ScriptForUpdateTrigger(codeTable, this.data);
                ScriptDisplayForm frmDisplay = new ScriptDisplayForm(script, this.data.SQLServerName, "Audit Trigger for " + codeTable.TableName);
                frmDisplay.ShowDialog();
            }
        }
        private void mnuScriptTriggers_Click(object sender, System.EventArgs e)
        {
            if (DialogResult.OK == folderBrowserDialog1.ShowDialog())
            {
                string folder = folderBrowserDialog1.SelectedPath;

                List<CodeTableAudit> codeTables = new List<CodeTableAudit>();
                for (int i = 0; i < this.lstTables.CheckedItems.Count; i++)
                    codeTables.Add((CodeTableAudit)this.lstTables.CheckedItems[i].Tag);

                TriggerScriptData args = new TriggerScriptData(TriggerScriptType.FilePerScript, folder, codeTables);

                this.Cursor = Cursors.AppStarting;
                bgTriggerScript.RunWorkerAsync(args);


            }
        }
        private void mnuSaveUpdateTrigToBuildFile_Click(object sender, System.EventArgs e)
        {

            if (DialogResult.OK == saveUpdateTrigBuildFile.ShowDialog())
            {
                List<CodeTableAudit> codeTables = new List<CodeTableAudit>();
                for (int i = 0; i < this.lstTables.CheckedItems.Count; i++)
                    codeTables.Add((CodeTableAudit)this.lstTables.CheckedItems[i].Tag);

                TriggerScriptData args = new TriggerScriptData(TriggerScriptType.NewSqlBuildFile, saveUpdateTrigBuildFile.FileName, codeTables);

                this.Cursor = Cursors.AppStarting;
                bgTriggerScript.RunWorkerAsync(args);
            }
        }

      

        private void bgTriggerScript_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bg = (BackgroundWorker)sender;
            TriggerScriptData args = (TriggerScriptData)e.Argument;
            List<CodeTableAudit> codeTables = args.CodeTables;
            StringBuilder sb = new StringBuilder();
            string destination = args.Destination;
            string folder = Path.GetTempPath() + @"\" + System.Guid.NewGuid().ToString();
            if (args.ScriptType == TriggerScriptType.NewSqlBuildFile)
                Directory.CreateDirectory(folder);
            string fileName;
            string script;
            for (int i = 0; i < codeTables.Count; i++)
            {

                bg.ReportProgress(10, "Creating trigger scripts for " + codeTables[i].TableName);
                script = PopulateHelper.ScriptForUpdateTrigger(codeTables[i], this.data);
                if (script.Length == 0)
                    continue;

                switch (args.ScriptType)
                {
                    case TriggerScriptType.SingleFile:
                        sb.Append(script + "\r\n");
                        break;
                    case TriggerScriptType.FilePerScript:
                    case TriggerScriptType.NewSqlBuildFile:
                        if (args.ScriptType == TriggerScriptType.NewSqlBuildFile)
                            fileName = folder + @"\" + codeTables[i].TableName + " - Update Trigger.sql";
                        else
                            fileName = destination + @"\" + codeTables[i].TableName + " - Update Trigger.sql";

                        bg.ReportProgress(10, "Writing file  " + codeTables[i].TableName + " - Update Trigger.sql");
                        using (StreamWriter sw = File.CreateText(fileName))
                        {
                            sw.WriteLine(script);
                            sw.Flush();
                            sw.Close();
                        }
                        break;
                }
            }

            if (codeTables.Count > 0)
            {
                switch (args.ScriptType)
                {
                    case TriggerScriptType.NewSqlBuildFile:

                        SaveSqlFilesToNewBuildFile(destination, folder);
                        bg.ReportProgress(10, "Build file saved to "+destination);

                        break;
                    case TriggerScriptType.SingleFile:
                        using (StreamWriter sw = File.CreateText(destination))
                        {
                            sw.WriteLine(sb.ToString());
                            sw.Flush();
                            sw.Close();
                        }
                        bg.ReportProgress(10, "Script saved to " + destination);
                        break;
                    case TriggerScriptType.FilePerScript:
                        bg.ReportProgress(10, "Scripts saved to " + destination);
                        break;
                }
            }

            e.Result = destination;

        }

        private void bgTriggerScript_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            statStatus.Text = e.UserState.ToString();
        }

        private void bgTriggerScript_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Cursor = Cursors.Default;
            string destination = e.Result.ToString();
            this.OpenResultMessage(destination, true);
        }
        #endregion

        class TriggerScriptData
        {
            public TriggerScriptType ScriptType;
            public string Destination;
            public List<CodeTableAudit> CodeTables;
            public TriggerScriptData(TriggerScriptType scriptType, string destination, List<CodeTableAudit> codeTables)
            {
                this.CodeTables = codeTables;
                this.Destination = destination;
                this.ScriptType = scriptType;
            }
        }


    }
    public enum TriggerScriptType
    {
        SingleFile,
        FilePerScript,
        NewSqlBuildFile
    }
	public delegate void SqlBuildManagerFileExportHandler(object sender, SqlBuildManagerFileExportEventArgs e);
	public class SqlBuildManagerFileExportEventArgs
	{
		public readonly string[] FileNames;
		public SqlBuildManagerFileExportEventArgs(string[] fileNames)
		{
			this.FileNames = fileNames;
		}
	}
		
}

