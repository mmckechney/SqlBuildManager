using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Reflection;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using SqlBuildManager.Enterprise;
using SqlSync.Connection;
using SqlSync.Constants;
using SqlSync.DbInformation;
using SqlSync.MRU;
using SqlSync.ObjectScript;
using SqlSync.SqlBuild;
using SqlSync.TableScript;
using SqlSync.TableScript.Audit;
using SqlSync.Validator;
namespace SqlSync
{
	/// <summary>
	/// Summary description for LookUpTable.
	/// </summary>
	public class DataAuditForm : System.Windows.Forms.Form , IMRUClient
    {
        #region Fields
        private string tempFolderName = string.Empty;
        private string tempBuildFileName = string.Empty;
		//private bool useDS = false;
		private MRUManager mruManager;
		private System.ComponentModel.IContainer components;
        ConnectionData data = null;
		private System.Windows.Forms.StatusStrip statusBar1;
		private System.Windows.Forms.ToolStripStatusLabel statStatus;
		private System.Windows.Forms.ContextMenuStrip contextMenu1;
        private System.Windows.Forms.ToolStripMenuItem mnuAddTable;
        private System.Windows.Forms.ToolStripMenuItem mnuDeleteTable;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.LinkLabel lnkCheckAll;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListView lstTables;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		SqlSync.TableScript.Audit.SQLSyncAuditing auditConfig = null;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox ddDatabaseList;
		private string projectFileName = string.Empty;
		private System.Windows.Forms.MenuStrip mainMenu1;
		private System.Windows.Forms.ToolStripMenuItem mnuActionMain;
		private System.Windows.Forms.ToolStripMenuItem mnuLoadProjectFile;
		private System.Windows.Forms.ToolStripMenuItem mnuChangeSqlServer;
        private string newDatabaseName = string.Empty;
		private System.Windows.Forms.ToolStripMenuItem mnuScriptTrigger;
        private System.Windows.Forms.ToolStripMenuItem mnuCopyTablesForsql;
		private System.Windows.Forms.ToolStripMenuItem mnuScriptDefault;
		private System.Windows.Forms.ToolStripMenuItem mnuCopyTables;
		private SqlSync.SettingsControl settingsControl1;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.GroupBox grpDatabaseInfo;
		private System.Windows.Forms.ToolStripMenuItem mnuFileMRU;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ToolStripMenuItem mnuAuditTable;
        private System.Windows.Forms.ToolStripMenuItem mnuSingleAuditTrigger;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ToolStripMenuItem mnuScriptMissingColumns;
        private System.Windows.Forms.Splitter splitter1;
        private System.Windows.Forms.Panel panel3;
		private System.Windows.Forms.Splitter splitter2;
		private System.Windows.Forms.ContextMenuStrip contextMenu2;
		private System.Windows.Forms.ToolStripMenuItem mnuCopy;
        private System.Windows.Forms.ToolStripMenuItem menuItem11;
        private System.Windows.Forms.ToolTip toolTip1;
		private System.Windows.Forms.RadioButton radSuffix;
        private System.Windows.Forms.RadioButton radPrefix;
        private ToolStripSeparator menuItem4;
        private ToolStripSeparator menuItem6;
        private ToolStripSeparator menuItem3;
        private ToolStripSeparator menuItem1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem backgrounLegendToolStripMenuItem;
        private ToolStripMenuItem allowMultipleRunsOfScriptOnSameServerToolStripMenuItem;
        private ToolStripMenuItem leaveTransactionTextInScriptsdontStripOutToolStripMenuItem;
        private ToolStripMenuItem groupingHelpToolStripMenuItem;
        private ToolStripMenuItem addedToProjectFileYouHaveSelectedThisTableForAuditingToolStripMenuItem;
        private ToolStripMenuItem candidateTablesTheToolHasDetectedThatTheseToolStripMenuItem;
        private UrielGuy.SyntaxHighlighting.SyntaxHighlightingTextBox rtbSqlScript;
        private ToolStrip toolStripCommands;
        private ToolStripSplitButton toolStripSplitButton1;
        private ToolStripMenuItem scriptCompletedAuditScriptSolutionToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem scriptAuditTablesToolStripMenuItem;
        private ToolStripMenuItem scriptAuditTriggersToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator2;
        private ToolStripMenuItem scriptMasterTransactionTableToolStripMenuItem;
        private ToolStripSplitButton toolStripSplitButton2;
        private ToolStripMenuItem disableAuditTriggersToolStripMenuItem;
        private ToolStripMenuItem enableAuditTriggersToolStripMenuItem;
        private ToolStripSplitButton toolStripSplitButton3;
        private ToolStripMenuItem saveToOpenToolStripMenuItem;
        private ToolStripMenuItem saveToNewSBMPackageToolStripMenuItem;
		private bool allowBuildManagerExport = false;
		public DataAuditForm(string fileName)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			this.projectFileName = fileName;

		}
		public DataAuditForm(ConnectionData data)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			this.data = data;

		}
		public DataAuditForm(ConnectionData data,bool allowBuildManagerExport):this(data)
		{
			this.allowBuildManagerExport = allowBuildManagerExport;
        }
        #endregion
        List<string> createdScriptFiles = new List<string>();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataAuditForm));
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Added to Project File", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Candidate Tables (have a sister \"audit\" table)", System.Windows.Forms.HorizontalAlignment.Left);
            UrielGuy.SyntaxHighlighting.HighLightDescriptorCollection highLightDescriptorCollection1 = new UrielGuy.SyntaxHighlighting.HighLightDescriptorCollection();
            this.contextMenu1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.menuItem6 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuScriptMissingColumns = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuScriptTrigger = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuScriptDefault = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuAuditTable = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuSingleAuditTrigger = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem3 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuAddTable = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuDeleteTable = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuCopyTables = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCopyTablesForsql = new System.Windows.Forms.ToolStripMenuItem();
            this.statusBar1 = new System.Windows.Forms.StatusStrip();
            this.statStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.ddDatabaseList = new System.Windows.Forms.ComboBox();
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.grpDatabaseInfo = new System.Windows.Forms.GroupBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.backgrounLegendToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.allowMultipleRunsOfScriptOnSameServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.leaveTransactionTextInScriptsdontStripOutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.groupingHelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.candidateTablesTheToolHasDetectedThatTheseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.addedToProjectFileYouHaveSelectedThisTableForAuditingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.radPrefix = new System.Windows.Forms.RadioButton();
            this.radSuffix = new System.Windows.Forms.RadioButton();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.panel3 = new System.Windows.Forms.Panel();
            this.toolStripCommands = new System.Windows.Forms.ToolStrip();
            this.toolStripSplitButton1 = new System.Windows.Forms.ToolStripSplitButton();
            this.scriptCompletedAuditScriptSolutionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.scriptAuditTablesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scriptAuditTriggersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.scriptMasterTransactionTableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSplitButton2 = new System.Windows.Forms.ToolStripSplitButton();
            this.disableAuditTriggersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.enableAuditTriggersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSplitButton3 = new System.Windows.Forms.ToolStripSplitButton();
            this.saveToOpenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveToNewSBMPackageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.rtbSqlScript = new UrielGuy.SyntaxHighlighting.SyntaxHighlightingTextBox();
            this.contextMenu2 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem11 = new System.Windows.Forms.ToolStripMenuItem();
            this.splitter2 = new System.Windows.Forms.Splitter();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.settingsControl1 = new SqlSync.SettingsControl();
            this.contextMenu1.SuspendLayout();
            this.statusBar1.SuspendLayout();
            this.mainMenu1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.grpDatabaseInfo.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.toolStripCommands.SuspendLayout();
            this.contextMenu2.SuspendLayout();
            this.SuspendLayout();
            // 
            // contextMenu1
            // 
            this.contextMenu1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem6,
            this.mnuScriptMissingColumns,
            this.mnuScriptTrigger,
            this.mnuScriptDefault,
            this.mnuAuditTable,
            this.mnuSingleAuditTrigger,
            this.menuItem3,
            this.mnuAddTable,
            this.mnuDeleteTable,
            this.menuItem1,
            this.mnuCopyTables,
            this.mnuCopyTablesForsql});
            this.contextMenu1.Name = "contextMenu1";
            this.contextMenu1.Size = new System.Drawing.Size(283, 220);
            this.contextMenu1.Click += new System.EventHandler(this.contextMenu1_Popup);
            // 
            // menuItem6
            // 
            this.menuItem6.MergeIndex = 0;
            this.menuItem6.Name = "menuItem6";
            this.menuItem6.Size = new System.Drawing.Size(279, 6);
            this.menuItem6.Visible = false;
            // 
            // mnuScriptMissingColumns
            // 
            this.mnuScriptMissingColumns.Enabled = false;
            this.mnuScriptMissingColumns.MergeIndex = 1;
            this.mnuScriptMissingColumns.Name = "mnuScriptMissingColumns";
            this.mnuScriptMissingColumns.Size = new System.Drawing.Size(282, 22);
            this.mnuScriptMissingColumns.Text = "Script For Missing Update Columns";
            this.mnuScriptMissingColumns.Visible = false;
            // 
            // mnuScriptTrigger
            // 
            this.mnuScriptTrigger.MergeIndex = 2;
            this.mnuScriptTrigger.Name = "mnuScriptTrigger";
            this.mnuScriptTrigger.Size = new System.Drawing.Size(282, 22);
            this.mnuScriptTrigger.Text = "Script For Update Trigger";
            this.mnuScriptTrigger.Visible = false;
            // 
            // mnuScriptDefault
            // 
            this.mnuScriptDefault.MergeIndex = 3;
            this.mnuScriptDefault.Name = "mnuScriptDefault";
            this.mnuScriptDefault.Size = new System.Drawing.Size(282, 22);
            this.mnuScriptDefault.Text = "Script to Reset Update Column Defaults";
            this.mnuScriptDefault.Visible = false;
            // 
            // mnuAuditTable
            // 
            this.mnuAuditTable.MergeIndex = 4;
            this.mnuAuditTable.Name = "mnuAuditTable";
            this.mnuAuditTable.Size = new System.Drawing.Size(282, 22);
            this.mnuAuditTable.Text = "Script for Audit Table";
            this.mnuAuditTable.Click += new System.EventHandler(this.mnuAuditTable_Click);
            // 
            // mnuSingleAuditTrigger
            // 
            this.mnuSingleAuditTrigger.MergeIndex = 5;
            this.mnuSingleAuditTrigger.Name = "mnuSingleAuditTrigger";
            this.mnuSingleAuditTrigger.Size = new System.Drawing.Size(282, 22);
            this.mnuSingleAuditTrigger.Text = "Script for Audit Triggers";
            this.mnuSingleAuditTrigger.Click += new System.EventHandler(this.mnuSingleAuditTrigger_Click);
            // 
            // menuItem3
            // 
            this.menuItem3.MergeIndex = 6;
            this.menuItem3.Name = "menuItem3";
            this.menuItem3.Size = new System.Drawing.Size(279, 6);
            // 
            // mnuAddTable
            // 
            this.mnuAddTable.MergeIndex = 7;
            this.mnuAddTable.Name = "mnuAddTable";
            this.mnuAddTable.Size = new System.Drawing.Size(282, 22);
            this.mnuAddTable.Text = "Add Table";
            this.mnuAddTable.Click += new System.EventHandler(this.mnuAddTable_Click);
            // 
            // mnuDeleteTable
            // 
            this.mnuDeleteTable.MergeIndex = 8;
            this.mnuDeleteTable.Name = "mnuDeleteTable";
            this.mnuDeleteTable.Size = new System.Drawing.Size(282, 22);
            this.mnuDeleteTable.Text = "Delete Selected Table";
            this.mnuDeleteTable.Click += new System.EventHandler(this.mnuDeleteTable_Click);
            // 
            // menuItem1
            // 
            this.menuItem1.MergeIndex = 9;
            this.menuItem1.Name = "menuItem1";
            this.menuItem1.Size = new System.Drawing.Size(279, 6);
            // 
            // mnuCopyTables
            // 
            this.mnuCopyTables.MergeIndex = 10;
            this.mnuCopyTables.Name = "mnuCopyTables";
            this.mnuCopyTables.Size = new System.Drawing.Size(282, 22);
            this.mnuCopyTables.Text = "Copy Table List";
            this.mnuCopyTables.Click += new System.EventHandler(this.mnuCopyTables_Click);
            // 
            // mnuCopyTablesForsql
            // 
            this.mnuCopyTablesForsql.MergeIndex = 11;
            this.mnuCopyTablesForsql.Name = "mnuCopyTablesForsql";
            this.mnuCopyTablesForsql.Size = new System.Drawing.Size(282, 22);
            this.mnuCopyTablesForsql.Text = "Copy Table List for Sql";
            this.mnuCopyTablesForsql.Click += new System.EventHandler(this.mnuCopyTablesForsql_Click);
            // 
            // statusBar1
            // 
            this.statusBar1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statStatus});
            this.statusBar1.Location = new System.Drawing.Point(0, 606);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Size = new System.Drawing.Size(896, 22);
            this.statusBar1.TabIndex = 6;
            this.statusBar1.Text = "statusBar1";
            // 
            // statStatus
            // 
            this.statStatus.Name = "statStatus";
            this.statStatus.Size = new System.Drawing.Size(881, 17);
            this.statStatus.Spring = true;
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.CheckFileExists = false;
            this.openFileDialog1.DefaultExt = "xml";
            this.openFileDialog1.Filter = "Sql Sync Audit Config *.audit|*.audit|Sql Sync Audit Config *.adt|*.adt|XML Files" +
    "|*.xml|All Files|*.*";
            this.openFileDialog1.Title = "Open SQL Sync Audit Project File";
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "");
            this.imageList1.Images.SetKeyName(1, "");
            this.imageList1.Images.SetKeyName(2, "");
            this.imageList1.Images.SetKeyName(3, "");
            this.imageList1.Images.SetKeyName(4, "");
            this.imageList1.Images.SetKeyName(5, "");
            this.imageList1.Images.SetKeyName(6, "");
            // 
            // ddDatabaseList
            // 
            this.ddDatabaseList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.ddDatabaseList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddDatabaseList.Location = new System.Drawing.Point(12, 48);
            this.ddDatabaseList.Name = "ddDatabaseList";
            this.ddDatabaseList.Size = new System.Drawing.Size(248, 21);
            this.ddDatabaseList.TabIndex = 0;
            this.ddDatabaseList.SelectionChangeCommitted += new System.EventHandler(this.ddDatabaseList_SelectionChangeCommitted);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(10, 30);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(100, 16);
            this.label3.TabIndex = 15;
            this.label3.Text = "Select Database:";
            // 
            // lnkCheckAll
            // 
            this.lnkCheckAll.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkCheckAll.Location = new System.Drawing.Point(140, 72);
            this.lnkCheckAll.Name = "lnkCheckAll";
            this.lnkCheckAll.Size = new System.Drawing.Size(120, 16);
            this.lnkCheckAll.TabIndex = 1;
            this.lnkCheckAll.TabStop = true;
            this.lnkCheckAll.Text = "Check All";
            this.lnkCheckAll.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lnkCheckAll.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkCheckAll_LinkClicked);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 70);
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
            this.lstTables.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.lstTables.FullRowSelect = true;
            listViewGroup1.Header = "Added to Project File";
            listViewGroup1.Name = "lstGrpAdded";
            listViewGroup2.Header = "Candidate Tables (have a sister \"audit\" table)";
            listViewGroup2.Name = "lstGrpCandidate";
            this.lstTables.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2});
            this.lstTables.HideSelection = false;
            this.lstTables.Location = new System.Drawing.Point(14, 92);
            this.lstTables.Name = "lstTables";
            this.lstTables.Size = new System.Drawing.Size(248, 384);
            this.lstTables.Sorting = System.Windows.Forms.SortOrder.Ascending;
            this.lstTables.TabIndex = 8;
            this.lstTables.UseCompatibleStateImageBehavior = false;
            this.lstTables.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Table Name";
            this.columnHeader1.Width = 171;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Rows";
            // 
            // mainMenu1
            // 
            this.mainMenu1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuActionMain});
            this.mainMenu1.Location = new System.Drawing.Point(0, 0);
            this.mainMenu1.Name = "mainMenu1";
            this.mainMenu1.Size = new System.Drawing.Size(896, 24);
            this.mainMenu1.TabIndex = 0;
            // 
            // mnuActionMain
            // 
            this.mnuActionMain.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuLoadProjectFile,
            this.mnuChangeSqlServer,
            this.menuItem4,
            this.mnuFileMRU});
            this.mnuActionMain.MergeIndex = 0;
            this.mnuActionMain.Name = "mnuActionMain";
            this.mnuActionMain.Size = new System.Drawing.Size(54, 20);
            this.mnuActionMain.Text = "&Action";
            // 
            // mnuLoadProjectFile
            // 
            this.mnuLoadProjectFile.MergeIndex = 0;
            this.mnuLoadProjectFile.Name = "mnuLoadProjectFile";
            this.mnuLoadProjectFile.Size = new System.Drawing.Size(234, 22);
            this.mnuLoadProjectFile.Text = "&Load/ New Project File";
            this.mnuLoadProjectFile.Click += new System.EventHandler(this.mnuLoadProjectFile_Click);
            // 
            // mnuChangeSqlServer
            // 
            this.mnuChangeSqlServer.MergeIndex = 1;
            this.mnuChangeSqlServer.Name = "mnuChangeSqlServer";
            this.mnuChangeSqlServer.Size = new System.Drawing.Size(234, 22);
            this.mnuChangeSqlServer.Text = "&Change Sql Server Connection";
            this.mnuChangeSqlServer.Click += new System.EventHandler(this.mnuChangeSqlServer_Click);
            // 
            // menuItem4
            // 
            this.menuItem4.MergeIndex = 2;
            this.menuItem4.Name = "menuItem4";
            this.menuItem4.Size = new System.Drawing.Size(231, 6);
            // 
            // mnuFileMRU
            // 
            this.mnuFileMRU.MergeIndex = 3;
            this.mnuFileMRU.Name = "mnuFileMRU";
            this.mnuFileMRU.Size = new System.Drawing.Size(234, 22);
            this.mnuFileMRU.Text = "Recent Files";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.grpDatabaseInfo);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 80);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(280, 526);
            this.panel1.TabIndex = 13;
            // 
            // grpDatabaseInfo
            // 
            this.grpDatabaseInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpDatabaseInfo.Controls.Add(this.menuStrip1);
            this.grpDatabaseInfo.Controls.Add(this.radPrefix);
            this.grpDatabaseInfo.Controls.Add(this.radSuffix);
            this.grpDatabaseInfo.Controls.Add(this.lnkCheckAll);
            this.grpDatabaseInfo.Controls.Add(this.label3);
            this.grpDatabaseInfo.Controls.Add(this.lstTables);
            this.grpDatabaseInfo.Controls.Add(this.ddDatabaseList);
            this.grpDatabaseInfo.Controls.Add(this.label1);
            this.grpDatabaseInfo.Enabled = false;
            this.grpDatabaseInfo.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.grpDatabaseInfo.Location = new System.Drawing.Point(2, 2);
            this.grpDatabaseInfo.Name = "grpDatabaseInfo";
            this.grpDatabaseInfo.Size = new System.Drawing.Size(272, 518);
            this.grpDatabaseInfo.TabIndex = 0;
            this.grpDatabaseInfo.TabStop = false;
            // 
            // menuStrip1
            // 
            this.menuStrip1.BackColor = System.Drawing.Color.Gainsboro;
            this.menuStrip1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.backgrounLegendToolStripMenuItem,
            this.groupingHelpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(3, 491);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(266, 24);
            this.menuStrip1.TabIndex = 20;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // backgrounLegendToolStripMenuItem
            // 
            this.backgrounLegendToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.allowMultipleRunsOfScriptOnSameServerToolStripMenuItem,
            this.leaveTransactionTextInScriptsdontStripOutToolStripMenuItem});
            this.backgrounLegendToolStripMenuItem.ForeColor = System.Drawing.Color.Blue;
            this.backgrounLegendToolStripMenuItem.Name = "backgrounLegendToolStripMenuItem";
            this.backgrounLegendToolStripMenuItem.Size = new System.Drawing.Size(143, 20);
            this.backgrounLegendToolStripMenuItem.Text = "Background Color Help";
            // 
            // allowMultipleRunsOfScriptOnSameServerToolStripMenuItem
            // 
            this.allowMultipleRunsOfScriptOnSameServerToolStripMenuItem.BackColor = System.Drawing.Color.Red;
            this.allowMultipleRunsOfScriptOnSameServerToolStripMenuItem.Name = "allowMultipleRunsOfScriptOnSameServerToolStripMenuItem";
            this.allowMultipleRunsOfScriptOnSameServerToolStripMenuItem.Size = new System.Drawing.Size(681, 22);
            this.allowMultipleRunsOfScriptOnSameServerToolStripMenuItem.Text = "The highlighted table is missing one or more triggers required for auditing";
            // 
            // leaveTransactionTextInScriptsdontStripOutToolStripMenuItem
            // 
            this.leaveTransactionTextInScriptsdontStripOutToolStripMenuItem.BackColor = System.Drawing.Color.LightBlue;
            this.leaveTransactionTextInScriptsdontStripOutToolStripMenuItem.Name = "leaveTransactionTextInScriptsdontStripOutToolStripMenuItem";
            this.leaveTransactionTextInScriptsdontStripOutToolStripMenuItem.RightToLeftAutoMirrorImage = true;
            this.leaveTransactionTextInScriptsdontStripOutToolStripMenuItem.Size = new System.Drawing.Size(681, 22);
            this.leaveTransactionTextInScriptsdontStripOutToolStripMenuItem.Text = "The \"candidate\" table has all the objects for auditing (audit table and triggers)" +
    ". Do you want to add it to the project?";
            // 
            // groupingHelpToolStripMenuItem
            // 
            this.groupingHelpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.candidateTablesTheToolHasDetectedThatTheseToolStripMenuItem,
            this.addedToProjectFileYouHaveSelectedThisTableForAuditingToolStripMenuItem});
            this.groupingHelpToolStripMenuItem.ForeColor = System.Drawing.Color.Blue;
            this.groupingHelpToolStripMenuItem.Name = "groupingHelpToolStripMenuItem";
            this.groupingHelpToolStripMenuItem.Size = new System.Drawing.Size(97, 20);
            this.groupingHelpToolStripMenuItem.Text = "Grouping Help";
            // 
            // candidateTablesTheToolHasDetectedThatTheseToolStripMenuItem
            // 
            this.candidateTablesTheToolHasDetectedThatTheseToolStripMenuItem.Name = "candidateTablesTheToolHasDetectedThatTheseToolStripMenuItem";
            this.candidateTablesTheToolHasDetectedThatTheseToolStripMenuItem.Size = new System.Drawing.Size(761, 22);
            this.candidateTablesTheToolHasDetectedThatTheseToolStripMenuItem.Text = "\"Candidate Tables\": The tool has detected that these tables have a matching \"audi" +
    "t\" table. Do you want to add them to the project?";
            // 
            // addedToProjectFileYouHaveSelectedThisTableForAuditingToolStripMenuItem
            // 
            this.addedToProjectFileYouHaveSelectedThisTableForAuditingToolStripMenuItem.Name = "addedToProjectFileYouHaveSelectedThisTableForAuditingToolStripMenuItem";
            this.addedToProjectFileYouHaveSelectedThisTableForAuditingToolStripMenuItem.Size = new System.Drawing.Size(761, 22);
            this.addedToProjectFileYouHaveSelectedThisTableForAuditingToolStripMenuItem.Text = "\"Added to Project File\": You have selected this table for auditing";
            // 
            // radPrefix
            // 
            this.radPrefix.Location = new System.Drawing.Point(138, 8);
            this.radPrefix.Name = "radPrefix";
            this.radPrefix.Size = new System.Drawing.Size(106, 24);
            this.radPrefix.TabIndex = 19;
            this.radPrefix.Text = "Audit as Prefix";
            this.radPrefix.CheckedChanged += new System.EventHandler(this.radPrefix_CheckedChanged);
            // 
            // radSuffix
            // 
            this.radSuffix.Checked = true;
            this.radSuffix.Location = new System.Drawing.Point(10, 8);
            this.radSuffix.Name = "radSuffix";
            this.radSuffix.Size = new System.Drawing.Size(106, 24);
            this.radSuffix.TabIndex = 18;
            this.radSuffix.TabStop = true;
            this.radSuffix.Text = "Audit as Suffix";
            this.radSuffix.CheckedChanged += new System.EventHandler(this.radSuffix_CheckedChanged);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "Sql Build Manager Project *.sbm|*.sbm|All Files *.*|*.*";
            this.saveFileDialog1.OverwritePrompt = false;
            this.saveFileDialog1.Title = "Save Sql Build Manager File";
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(280, 80);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 526);
            this.splitter1.TabIndex = 14;
            this.splitter1.TabStop = false;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.toolStripCommands);
            this.panel3.Controls.Add(this.rtbSqlScript);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(283, 80);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(613, 526);
            this.panel3.TabIndex = 17;
            // 
            // toolStripCommands
            // 
            this.toolStripCommands.Enabled = false;
            this.toolStripCommands.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSplitButton1,
            this.toolStripSplitButton2,
            this.toolStripSplitButton3});
            this.toolStripCommands.Location = new System.Drawing.Point(0, 0);
            this.toolStripCommands.Margin = new System.Windows.Forms.Padding(5);
            this.toolStripCommands.Name = "toolStripCommands";
            this.toolStripCommands.Padding = new System.Windows.Forms.Padding(5);
            this.toolStripCommands.Size = new System.Drawing.Size(613, 32);
            this.toolStripCommands.TabIndex = 10;
            this.toolStripCommands.Text = "toolStripCommands";
            // 
            // toolStripSplitButton1
            // 
            this.toolStripSplitButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.scriptCompletedAuditScriptSolutionToolStripMenuItem,
            this.toolStripSeparator1,
            this.scriptAuditTablesToolStripMenuItem,
            this.scriptAuditTriggersToolStripMenuItem,
            this.toolStripSeparator2,
            this.scriptMasterTransactionTableToolStripMenuItem});
            this.toolStripSplitButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitButton1.Name = "toolStripSplitButton1";
            this.toolStripSplitButton1.RightToLeftAutoMirrorImage = true;
            this.toolStripSplitButton1.Size = new System.Drawing.Size(166, 19);
            this.toolStripSplitButton1.Text = "Auditing Solution Scripting";
            this.toolStripSplitButton1.ToolTipText = "Creates the scripts required to implement the \"sister table/trigger\" auditing sol" +
    "ution";
            this.toolStripSplitButton1.Click += new System.EventHandler(this.splitButton_Click);
            // 
            // scriptCompletedAuditScriptSolutionToolStripMenuItem
            // 
            this.scriptCompletedAuditScriptSolutionToolStripMenuItem.Name = "scriptCompletedAuditScriptSolutionToolStripMenuItem";
            this.scriptCompletedAuditScriptSolutionToolStripMenuItem.RightToLeftAutoMirrorImage = true;
            this.scriptCompletedAuditScriptSolutionToolStripMenuItem.Size = new System.Drawing.Size(416, 22);
            this.scriptCompletedAuditScriptSolutionToolStripMenuItem.Text = "Script Complete Audit  Solution";
            this.scriptCompletedAuditScriptSolutionToolStripMenuItem.ToolTipText = resources.GetString("scriptCompletedAuditScriptSolutionToolStripMenuItem.ToolTipText");
            this.scriptCompletedAuditScriptSolutionToolStripMenuItem.Click += new System.EventHandler(this.mnuCompleteAudit_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(413, 6);
            // 
            // scriptAuditTablesToolStripMenuItem
            // 
            this.scriptAuditTablesToolStripMenuItem.Name = "scriptAuditTablesToolStripMenuItem";
            this.scriptAuditTablesToolStripMenuItem.Size = new System.Drawing.Size(416, 22);
            this.scriptAuditTablesToolStripMenuItem.Text = "Audit Tables Creation/Modification Scripts -  for Checked Tables";
            this.scriptAuditTablesToolStripMenuItem.ToolTipText = "For the checked tables, will generate scripts for their \"sister\" audit tables\r\nan" +
    "d save them to the directory you choose\r\n";
            this.scriptAuditTablesToolStripMenuItem.Click += new System.EventHandler(this.mnuScriptAllAudit_Click);
            // 
            // scriptAuditTriggersToolStripMenuItem
            // 
            this.scriptAuditTriggersToolStripMenuItem.Name = "scriptAuditTriggersToolStripMenuItem";
            this.scriptAuditTriggersToolStripMenuItem.Size = new System.Drawing.Size(416, 22);
            this.scriptAuditTriggersToolStripMenuItem.Text = "Audit Triggers Creation/Modification Scripts - for Checked Tables";
            this.scriptAuditTriggersToolStripMenuItem.ToolTipText = "For the checked tables, will generate scripts for the 3 audit triggers (INSERT, U" +
    "PDATE, DELETE)\r\nand save them to the directory you choose\r\n";
            this.scriptAuditTriggersToolStripMenuItem.Click += new System.EventHandler(this.scriptAuditTriggersToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(413, 6);
            // 
            // scriptMasterTransactionTableToolStripMenuItem
            // 
            this.scriptMasterTransactionTableToolStripMenuItem.Enabled = false;
            this.scriptMasterTransactionTableToolStripMenuItem.Name = "scriptMasterTransactionTableToolStripMenuItem";
            this.scriptMasterTransactionTableToolStripMenuItem.Size = new System.Drawing.Size(416, 22);
            this.scriptMasterTransactionTableToolStripMenuItem.Text = "Master Transaction Table Creation Script";
            this.scriptMasterTransactionTableToolStripMenuItem.ToolTipText = "Generates the create script for the master audit table to the script window below" +
    ".";
            this.scriptMasterTransactionTableToolStripMenuItem.Visible = false;
            this.scriptMasterTransactionTableToolStripMenuItem.Click += new System.EventHandler(this.mnuMasterTrx_Click);
            // 
            // toolStripSplitButton2
            // 
            this.toolStripSplitButton2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.disableAuditTriggersToolStripMenuItem,
            this.enableAuditTriggersToolStripMenuItem});
            this.toolStripSplitButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitButton2.Name = "toolStripSplitButton2";
            this.toolStripSplitButton2.Size = new System.Drawing.Size(140, 19);
            this.toolStripSplitButton2.Text = "Trigger Enable/Disable";
            this.toolStripSplitButton2.ToolTipText = "There may be cases where you need to disable the auditing triggers temporarily.\r\n" +
    "These commands will create the scripts to disable and enable the auditing trigge" +
    "rs.";
            this.toolStripSplitButton2.Click += new System.EventHandler(this.splitButton_Click);
            // 
            // disableAuditTriggersToolStripMenuItem
            // 
            this.disableAuditTriggersToolStripMenuItem.Name = "disableAuditTriggersToolStripMenuItem";
            this.disableAuditTriggersToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            this.disableAuditTriggersToolStripMenuItem.Text = "Disable Audit Triggers Scripts";
            this.disableAuditTriggersToolStripMenuItem.ToolTipText = "For the checked tables, create the scripts to disable the 3 auditing triggers (IN" +
    "SERT, UPDATE, DELETE)";
            this.disableAuditTriggersToolStripMenuItem.Click += new System.EventHandler(this.mnuDisableAuditTrigs_Click);
            // 
            // enableAuditTriggersToolStripMenuItem
            // 
            this.enableAuditTriggersToolStripMenuItem.Name = "enableAuditTriggersToolStripMenuItem";
            this.enableAuditTriggersToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            this.enableAuditTriggersToolStripMenuItem.Text = "Enable Audit Triggers Scripts";
            this.enableAuditTriggersToolStripMenuItem.ToolTipText = "For the checked tables, create the scripts to enable the 3 auditing triggers (INS" +
    "ERT, UPDATE, DELETE)";
            this.enableAuditTriggersToolStripMenuItem.Click += new System.EventHandler(this.mnuEnableAuditTrigs_Click);
            // 
            // toolStripSplitButton3
            // 
            this.toolStripSplitButton3.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripSplitButton3.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveToOpenToolStripMenuItem,
            this.saveToNewSBMPackageToolStripMenuItem});
            this.toolStripSplitButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.toolStripSplitButton3.Name = "toolStripSplitButton3";
            this.toolStripSplitButton3.Size = new System.Drawing.Size(85, 19);
            this.toolStripSplitButton3.Text = "Save Scripts";
            this.toolStripSplitButton3.ToolTipText = "Save scripts";
            this.toolStripSplitButton3.Click += new System.EventHandler(this.splitButton_Click);
            // 
            // saveToOpenToolStripMenuItem
            // 
            this.saveToOpenToolStripMenuItem.Enabled = false;
            this.saveToOpenToolStripMenuItem.Name = "saveToOpenToolStripMenuItem";
            this.saveToOpenToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.saveToOpenToolStripMenuItem.Text = "Save to Open  Sql Build Package";
            this.saveToOpenToolStripMenuItem.ToolTipText = "Save scripts to the Sql Build Package that is open in the parent window";
            this.saveToOpenToolStripMenuItem.Click += new System.EventHandler(this.saveToOpenToolStripMenuItem_Click);
            // 
            // saveToNewSBMPackageToolStripMenuItem
            // 
            this.saveToNewSBMPackageToolStripMenuItem.Name = "saveToNewSBMPackageToolStripMenuItem";
            this.saveToNewSBMPackageToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            this.saveToNewSBMPackageToolStripMenuItem.Text = "Save to new SBM package";
            this.saveToNewSBMPackageToolStripMenuItem.ToolTipText = "Save scripts to a new SBM package, or add them to an alternate existing SBM packa" +
    "ge";
            this.saveToNewSBMPackageToolStripMenuItem.Click += new System.EventHandler(this.toolStripSave_Click);
            // 
            // rtbSqlScript
            // 
            this.rtbSqlScript.AcceptsTab = true;
            this.rtbSqlScript.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbSqlScript.CaseSensitive = false;
            this.rtbSqlScript.ContextMenuStrip = this.contextMenu2;
            this.rtbSqlScript.FilterAutoComplete = true;
            this.rtbSqlScript.Font = new System.Drawing.Font("Lucida Console", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.rtbSqlScript.HighlightDescriptors = highLightDescriptorCollection1;
            this.rtbSqlScript.HighlightType = SqlSync.Highlighting.SyntaxHightlightType.Sql;
            this.rtbSqlScript.Location = new System.Drawing.Point(4, 38);
            this.rtbSqlScript.MaxUndoRedoSteps = 50;
            this.rtbSqlScript.Name = "rtbSqlScript";
            this.rtbSqlScript.Size = new System.Drawing.Size(604, 479);
            this.rtbSqlScript.SuspendHighlighting = false;
            this.rtbSqlScript.TabIndex = 9;
            this.rtbSqlScript.Text = "";
            // 
            // contextMenu2
            // 
            this.contextMenu2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuCopy,
            this.menuItem11});
            this.contextMenu2.Name = "contextMenu2";
            this.contextMenu2.Size = new System.Drawing.Size(173, 48);
            // 
            // mnuCopy
            // 
            this.mnuCopy.MergeIndex = 0;
            this.mnuCopy.Name = "mnuCopy";
            this.mnuCopy.Size = new System.Drawing.Size(172, 22);
            this.mnuCopy.Text = "Copy To Clipboard";
            this.mnuCopy.Click += new System.EventHandler(this.mnuCopy_Click);
            // 
            // menuItem11
            // 
            this.menuItem11.MergeIndex = 1;
            this.menuItem11.Name = "menuItem11";
            this.menuItem11.Size = new System.Drawing.Size(172, 22);
            this.menuItem11.Text = "Save to File";
            // 
            // splitter2
            // 
            this.splitter2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.splitter2.Location = new System.Drawing.Point(283, 603);
            this.splitter2.Name = "splitter2";
            this.splitter2.Size = new System.Drawing.Size(613, 3);
            this.splitter2.TabIndex = 18;
            this.splitter2.TabStop = false;
            // 
            // settingsControl1
            // 
            this.settingsControl1.BackColor = System.Drawing.Color.White;
            this.settingsControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.settingsControl1.Location = new System.Drawing.Point(0, 24);
            this.settingsControl1.Name = "settingsControl1";
            this.settingsControl1.Project = "";
            this.settingsControl1.ProjectLabelText = "Project File:";
            this.settingsControl1.Server = "";
            this.settingsControl1.Size = new System.Drawing.Size(896, 56);
            this.settingsControl1.TabIndex = 12;
            this.settingsControl1.ServerChanged += new SqlSync.ServerChangedEventHandler(this.settingsControl1_ServerChanged);
            // 
            // DataAuditForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(896, 628);
            this.Controls.Add(this.splitter2);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.statusBar1);
            this.Controls.Add(this.settingsControl1);
            this.Controls.Add(this.mainMenu1);
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.mainMenu1;
            this.Name = "DataAuditForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sql Build Manager :: User Data Audit Scripting";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DataAuditForm_FormClosing);
            this.Load += new System.EventHandler(this.LookUpTable_Load);
            this.contextMenu1.ResumeLayout(false);
            this.statusBar1.ResumeLayout(false);
            this.statusBar1.PerformLayout();
            this.mainMenu1.ResumeLayout(false);
            this.mainMenu1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.grpDatabaseInfo.ResumeLayout(false);
            this.grpDatabaseInfo.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.toolStripCommands.ResumeLayout(false);
            this.toolStripCommands.PerformLayout();
            this.contextMenu2.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void LookUpTable_Load(object sender, System.EventArgs e)
		{
		if(this.data == null)
			{
				ConnectionForm frmConnect = new ConnectionForm("Sql Sync Auditing");
				DialogResult result =  frmConnect.ShowDialog();
				if(result == DialogResult.OK)
				{
					this.data = frmConnect.SqlConnection;
				}
				else
				{
					MessageBox.Show("Sql Sync Auditing can not continue without a valid Sql Connection","Unable to Load",MessageBoxButtons.OK,MessageBoxIcon.Hand);
					this.Close();
				}

			}

			this.mruManager = new MRUManager();
            this.mruManager.Initialize(
                this,                              // owner form
                mnuActionMain,
                mnuFileMRU,                        // Recent Files menu item
                @"Software\Michael McKechney\Sql Sync\Sql Sync Auditing"); // Registry path to keep MRU list
            this.mruManager.MaxDisplayNameLength = 150;

			this.settingsControl1.Server =  this.data.SQLServerName;
			if(this.projectFileName.Length > 0)
				this.LoadAuditTableData(this.projectFileName,true);


            if (this.SqlBuildManagerFileExport != null)
            {
                this.saveToOpenToolStripMenuItem.Enabled = true;
                this.saveToOpenToolStripMenuItem.ToolTipText = "Save scripts to the Sql Build Package that is open in the parent window";
            }
            else
            {
                this.saveToOpenToolStripMenuItem.Enabled = false;
                this.saveToOpenToolStripMenuItem.ToolTipText = "No Sql Build Package loaded. Unable to save to an open package";

            }



			

		}

		private void LoadAuditTableData(string configFile,bool validateSchema)
		{
			this.Cursor = Cursors.WaitCursor;
			try
			{
				if(File.Exists(configFile))
				{
					try
					{
						using(StreamReader sr = new StreamReader(configFile))
						{
							System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(SqlSync.TableScript.Audit.SQLSyncAuditing));
							object obj = serializer.Deserialize(sr);
							this.auditConfig = (SqlSync.TableScript.Audit.SQLSyncAuditing)obj;
						}
					}
					catch(Exception e)
					{
						string excep = e.ToString();
					}

				}
				
				if(auditConfig != null)
				{
					this.projectFileName = configFile;
					this.settingsControl1.Project = configFile;
					grpDatabaseInfo.Enabled = true;
                    toolStripCommands.Enabled = true;
					this.mruManager.Add(configFile);
					PopulateDatabaseList();
				}
				else
				{
					MessageBox.Show("Unable to Read the selected file.\r\nIt is not a valid audit configuration file","Invalid File",MessageBoxButtons.OK,MessageBoxIcon.Error);
				}
			}
			finally
			{
				this.Cursor = Cursors.Default;
			}
		}
		private void PopulateDatabaseList()
		{
			if(this.auditConfig == null)
				return;

			string currentSelection = string.Empty;
			if(this.ddDatabaseList.Items.Count > 0 && this.ddDatabaseList.SelectedItem != null &&
				this.ddDatabaseList.SelectedItem.ToString() != DropDownConstants.AddNew)
			{
				currentSelection = this.ddDatabaseList.SelectedItem.ToString();
			}
			this.ddDatabaseList.Items.Clear();
			this.lstTables.Items.Clear();

			ddDatabaseList.Items.Add(DropDownConstants.SelectOne);
			this.ddDatabaseList.SelectedIndex = 0;
			if(this.auditConfig.Items != null)
			{
				for(int i=0;i<this.auditConfig.Items.Length;i++)
				{
					ddDatabaseList.Items.Add(this.auditConfig.Items[i].Name);
					if(currentSelection.Length > 0 && currentSelection == this.auditConfig.Items[i].Name)
						ddDatabaseList.SelectedIndex = i+1;
				}
			}
			ddDatabaseList.Items.Add(DropDownConstants.AddNew);

			if(ddDatabaseList.SelectedIndex > 0)
				this.PopulateTableList(ddDatabaseList.SelectedItem.ToString());
			
		}
		private void PopulateTableList(string databaseName)
		{
			if(this.auditConfig == null)
				return;

			SQLSyncAuditingDatabase selectedDb =null;
            int auditConfigDbIndex = -1;
			for(int i=0;i<this.auditConfig.Items.Length;i++)
			{
				if(this.auditConfig.Items[i].Name.ToLower() == databaseName.ToLower())
				{
					selectedDb = this.auditConfig.Items[i];
                    auditConfigDbIndex = i;
					break;
				}
			}

			if(selectedDb == null)
				return;


			this.statStatus.Text = "Retrieving Table List and Row Count";
			this.lstTables.Items.Clear();
			this.data.DatabaseName = databaseName;
		
			SqlSync.DbInformation.TableSize[] tables = SqlSync.DbInformation.InfoHelper.GetDatabaseTableListWithRowCount(this.data);	
			AuditAutoDetectData[] trigTable = SqlSync.TableScript.Audit.AuditHelper.AutoDetectDataAuditing(this.data);
			for(int i=0;i<tables.Length;i++)
			{
				if(selectedDb.TableToAudit != null)
				{
					bool added = false;


                    for (int j = 0; j < selectedDb.TableToAudit.Length; j++)
                    {
                        string[] allTriggers = SqlSync.DbInformation.InfoHelper.GetTriggers(this.data);
                        IAuditInfo tbl = selectedDb.TableToAudit[j];
                        SqlSync.TableScript.Audit.AuditHelper.CheckForStandardAuditTriggers(ref tbl, allTriggers);

                        if (tables[i].TableName.ToLower() == selectedDb.TableToAudit[j].Name.ToLower())
                        {
                            ListViewItem item = new ListViewItem(new string[] { selectedDb.TableToAudit[j].Name, tables[i].StrRowCount });
                            item.Tag = selectedDb.TableToAudit[j];
                            item.Group = lstTables.Groups["lstGrpAdded"];

                            if (!tbl.HasAuditDeleteTrigger || !tbl.HasAuditInsertTrigger || !tbl.HasAuditUpdateTrigger)
                                item.BackColor = Color.Red;
                            else
                                item.BackColor = Color.LightBlue;


                            lstTables.Items.Add(item);
                            added = true;
                        }
                    }

                    if (added)
                        continue;

					for(int j=0;j<trigTable.Length;j++)
					{
						if(tables[i].TableName.ToLower() == trigTable[j].TableName.ToLower())
						{
							ListViewItem item = new ListViewItem(new string[]{trigTable[j].TableName,trigTable[j].StringRowCount});
							if(!trigTable[j].HasAuditDeleteTrigger || !trigTable[j].HasAuditInsertTrigger || !trigTable[j].HasAuditUpdateTrigger)
								item.BackColor = Color.Red;
							else
								item.BackColor = 	Color.LightBlue;

                            //Set the tag object if available
                            for (int x = 0; x<this.auditConfig.Items[auditConfigDbIndex].TableToAudit.Length; x++)
                                if (this.auditConfig.Items[auditConfigDbIndex].TableToAudit[x].Name.ToLower() == tables[i].TableName.ToLower())
                                    item.Tag = this.auditConfig.Items[auditConfigDbIndex].TableToAudit[x];

                            item.Group = lstTables.Groups["lstGrpCandidate"];
							lstTables.Items.Add(item);
							break;
						}
					}

	

				
				}
			}
			
			this.statStatus.Text = "Ready.";
		}
		private void AddNewDatabase()
		{
			string[] listedDatabases = new string[this.ddDatabaseList.Items.Count -2];
			for(int i=1;i<this.ddDatabaseList.Items.Count-1;i++)
			{
				listedDatabases[i-1] = this.ddDatabaseList.Items[i].ToString();
			}
            SqlSync.TableScript.NewLookUpDatabaseForm frmNewDb = new SqlSync.TableScript.NewLookUpDatabaseForm(new PopulateHelper(this.data, null).RemainingDatabases(listedDatabases));
			DialogResult result = frmNewDb.ShowDialog();
			if(result == DialogResult.OK)
			{
				string[] newDatabases = frmNewDb.DatabaseList;
				SaveNewDatabaseNames(newDatabases);
			}
		}
		private void SaveNewDatabaseNames(string[] newDatabases)
		{
			ArrayList lst = new ArrayList();
			if(this.auditConfig.Items != null)
				lst.AddRange(this.auditConfig.Items);
			for(int i=0;i<newDatabases.Length;i++)
			{
				SQLSyncAuditingDatabase db = new SQLSyncAuditingDatabase();
				db.Name = newDatabases[i];
				lst.Add(db);
			}
			this.auditConfig.Items = new SQLSyncAuditingDatabase[lst.Count];
			lst.CopyTo(this.auditConfig.Items);

			this.SaveXmlTemplate(this.projectFileName);
			this.LoadAuditTableData(this.projectFileName,false);
		}

		private void SaveXmlTemplate(string fileName)
		{
	
			try
			{
				System.Xml.XmlTextWriter tw = null;
				try
				{
					XmlSerializer xmlS = new XmlSerializer(typeof(SqlSync.TableScript.Audit.SQLSyncAuditing));
					tw = new System.Xml.XmlTextWriter(this.projectFileName,Encoding.UTF8);
                    tw.Formatting = System.Xml.Formatting.Indented;
					xmlS.Serialize(tw,this.auditConfig);
				}
				finally
				{
					if(tw != null)
						tw.Close();
				}
			}
			catch(System.UnauthorizedAccessException)
			{
				MessageBox.Show("Unable to save project file to:\r\n"+fileName,"Unable to Save",MessageBoxButtons.OK,MessageBoxIcon.Error);
			}
		}

 		private SQLSyncAuditingDatabase GetDatabaseObject(string databaseName)
		{
			for(int i=0;i<this.auditConfig.Items.Length;i++)
			{
				if(this.auditConfig.Items[i].Name.ToLower() == databaseName.ToLower())
					return this.auditConfig.Items[i];
			}
			return null;
		}
		private SQLSyncAuditingDatabaseTableToAudit GetTableObject(string databaseName,string tableName)
		{

			SQLSyncAuditingDatabase selectedDb = GetDatabaseObject(databaseName);
			if(selectedDb == null)
				return null;

			for(int i=0;i<selectedDb.TableToAudit.Length;i++)
			{
				if(selectedDb.TableToAudit[i].Name.ToLower() == tableName.ToLower())
					return selectedDb.TableToAudit[i];
			}
			return null;
		}

		private void RemoveDatabase(string databaseName)
		{
			if(this.auditConfig == null)
				return;

			SQLSyncAuditingDatabase selectedDb = GetDatabaseObject(databaseName);

			if(selectedDb == null)
				return;

			if( selectedDb.TableToAudit.Length > 0)
			{
				string entry = (selectedDb.TableToAudit.Length > 1)? "entries":"entry";
				string message = "The Database "+selectedDb.Name +" has "+selectedDb.TableToAudit.Length.ToString()+" table "+entry+".\r\nAre you sure you want to remove it?";
				DialogResult result = MessageBox.Show(message,"Confirm Removal",MessageBoxButtons.YesNo,MessageBoxIcon.Question);
				if(result == DialogResult.No) return;
			}

			for(int i=0;i<this.auditConfig.Items.Length;i++)
			{
				if(this.auditConfig.Items[i].Equals(selectedDb))
				{
					this.auditConfig.Items[i] = null;
					break;
				}
			}

			this.SaveXmlTemplate(this.projectFileName);
			this.LoadAuditTableData(this.projectFileName,false);
		}
		private void RemoveTable(string databaseName, string tableName)
		{

			SQLSyncAuditingDatabase selectedDb = GetDatabaseObject(databaseName);
			if(selectedDb == null)
				return;

			for(int i=0;i<selectedDb.TableToAudit.Length;i++)
			{
				if(selectedDb.TableToAudit[i].Name.ToLower() == tableName.ToLower())
				{
					selectedDb.TableToAudit[i] = null;
					break;
				}
			}

			this.SaveXmlTemplate(this.projectFileName);
			this.LoadAuditTableData(this.projectFileName,false);

			
		}
		private void AddNewTables(List<string> tables)
		{
			SQLSyncAuditingDatabase selectedDb = GetDatabaseObject(this.data.DatabaseName);
			if(selectedDb ==null)
				return;

			ArrayList newLst = new ArrayList();
			for(int i=0;i<tables.Count;i++)
			{
				SQLSyncAuditingDatabaseTableToAudit table = new SQLSyncAuditingDatabaseTableToAudit();
				table.Name = tables[i];
				newLst.Add(table);
			}
			ArrayList current = new ArrayList();
			if(selectedDb.TableToAudit != null)
				current.AddRange(selectedDb.TableToAudit);
			current.AddRange(newLst);
			selectedDb.TableToAudit = new SQLSyncAuditingDatabaseTableToAudit[current.Count];
			current.CopyTo(selectedDb.TableToAudit);

		
			this.SaveXmlTemplate(this.projectFileName);
			this.LoadAuditTableData(this.projectFileName,false);
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

		private void mnuAddTable_Click(object sender, System.EventArgs e)
		{
			List<string> listedTables = new List<string>();
			for(int i=0;i<this.lstTables.Items.Count;i++)
			{
                if (this.lstTables.Items[i].Group == this.lstTables.Groups["lstGrpAdded"])
                    listedTables.Add(this.lstTables.Items[i].Text);
			}

			NewLookUpForm frmNew = new NewLookUpForm( new PopulateHelper(this.data,null).RemainingTables(listedTables));
			DialogResult result =  frmNew.ShowDialog();
			if(result == DialogResult.OK)
			{
				this.AddNewTables(frmNew.TableList);
			}
		}

		private void mnuDeleteTable_Click(object sender, System.EventArgs e)
		{
			if(lstTables.SelectedItems.Count > 0)
			{
				ListViewItem item = lstTables.SelectedItems[0];
				this.RemoveTable(this.data.DatabaseName, item.Text);
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

		private void mnuLoadProjectFile_Click(object sender, System.EventArgs e)
		{
			DialogResult result = openFileDialog1.ShowDialog();
			if(result == DialogResult.OK)
			{
				if(File.Exists(openFileDialog1.FileName) == false)
				{
					this.projectFileName = openFileDialog1.FileName;
					this.auditConfig = new SQLSyncAuditing();
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

		private void contextMenu1_Popup(object sender, System.EventArgs e)
		{
			if(this.lstTables.SelectedItems.Count > 0)
			{
				if(this.lstTables.SelectedItems[0].ForeColor.ToString() == Color.Purple.ToString())
					mnuScriptMissingColumns.Enabled = true;
				else
					mnuScriptMissingColumns.Enabled = false;
			}
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

		
		#region IMRUClient Members

		public void OpenMRUFile(string fileName)
		{
			this.Cursor = Cursors.WaitCursor;
			try
			{
				if(System.IO.File.Exists(fileName))
				{
					this.projectFileName = fileName;
					this.LoadAuditTableData(this.projectFileName,true);
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


        #region Script Creation and Set-up
        private void GenerateSelectedScripts(AuditScriptType type, string suggestedProjectName)
        {
            List<TableConfig> tables = new List<TableConfig>();
            for (int i = 0; i < this.lstTables.CheckedItems.Count; i++)
            {
                tables.Add(new TableConfig(this.lstTables.CheckedItems[i].Text, this.lstTables.CheckedItems[i].Tag as SQLSyncAuditingDatabaseTableToAudit));
            }
            GenerateScriptsToWindow(tables, type, suggestedProjectName);

        }
        private void GenerateScriptsToWindow(TableConfig table, AuditScriptType type, string suggestedProjectName)
        {
            List<TableConfig> tables = new List<TableConfig>();
            tables.Add(table);
            GenerateScriptsToWindow(tables, type, suggestedProjectName);
        }
        private void GenerateScriptsToWindow(List<TableConfig> tables, AuditScriptType type, string suggestedProjectName)
        {
            this.ClearExistingTempDirectory();

            if (!this.SetUpForNewScripts(suggestedProjectName))
                return;

            List<string> scripts = GenerateAuditingScripts(tables, type);
            StringBuilder sb = new StringBuilder();
            foreach (string script in scripts)
                sb.AppendLine(script + "\r\n---------------------------------");

            this.rtbSqlScript.Text = sb.ToString();
        }
        private List<string> GenerateAuditingScripts(List<TableConfig> tables, AuditScriptType type)
        {
            string triggerNameFormat = "{0} - "+ AuditHelper.triggerNameFormat;
            string triggerFileName = "{1}.{0}.TRG";
            string tableName, schemaName, header, triggerNameForHeader;
            List<string> scripts = new List<string>();
            foreach (TableConfig cfg in tables)
            {  
                
                InfoHelper.ExtractNameAndSchema(cfg.TableName, out tableName, out schemaName);
                
                string fileName = string.Empty;
                 string script = string.Empty;
                 if (type == AuditScriptType.CreateAllTriggers)
                 {
                     script = AuditHelper.GetAuditScript(cfg, AuditScriptType.CreateInsertTrigger, this.data);
                     triggerNameForHeader = String.Format(triggerNameFormat, tableName, "INSERT");
                     header = ObjectScriptHelper.ScriptHeader(this.data.SQLServerName, this.data.DatabaseName, DateTime.Now, schemaName, triggerNameForHeader, DbScriptDescription.Trigger, System.Environment.UserName, true, true,true);
                     fileName = this.tempFolderName + "\\" + String.Format(triggerFileName, triggerNameForHeader,schemaName); 
                     this.createdScriptFiles.Add(fileName);
                     File.WriteAllText(fileName, header + script);
                     scripts.Add(script);

                     script = AuditHelper.GetAuditScript(cfg, AuditScriptType.CreateUpdateTrigger, this.data);
                     triggerNameForHeader = String.Format(triggerNameFormat, tableName, "UPDATE");
                     header = ObjectScriptHelper.ScriptHeader(this.data.SQLServerName, this.data.DatabaseName, DateTime.Now, schemaName, triggerNameForHeader, DbScriptDescription.Trigger, System.Environment.UserName, true, true, true);
                     fileName = this.tempFolderName + "\\" + String.Format(triggerFileName, triggerNameForHeader, schemaName); 
                     this.createdScriptFiles.Add(fileName);
                     File.WriteAllText(fileName, header + script);
                     scripts.Add(script);

                     script = AuditHelper.GetAuditScript(cfg, AuditScriptType.CreateDeleteTrigger, this.data);
                     triggerNameForHeader = String.Format(triggerNameFormat, tableName, "DELETE");
                     header = ObjectScriptHelper.ScriptHeader(this.data.SQLServerName, this.data.DatabaseName, DateTime.Now, schemaName, triggerNameForHeader, DbScriptDescription.Trigger, System.Environment.UserName, true, true, true);
                     fileName = this.tempFolderName + "\\" + String.Format(triggerFileName, triggerNameForHeader, schemaName); 
                     this.createdScriptFiles.Add(fileName);
                     File.WriteAllText(fileName, header + script);
                     scripts.Add(script);
                 }
                 else
                 {
                     script = AuditHelper.GetAuditScript(cfg, type, this.data);
                     fileName = string.Empty;
                     switch (type)
                     {
                         case AuditScriptType.CreateAuditTable:
                             fileName = cfg.TableName + " - Audit  Table.sql";
                             break;
                         case AuditScriptType.TriggerDisable:
                             fileName = cfg.TableName + " Disable Triggers.sql";
                             break;
                         case AuditScriptType.TriggerEnable:
                             fileName = cfg.TableName + " Enable Triggers.sql";
                             break;
                         case AuditScriptType.MasterTable:
                             fileName = "Audit Master Table.sql";
                             break;
                         default:
                             fileName = cfg.TableName + ".sql";
                             break;
                     }
                     this.createdScriptFiles.Add(fileName);
                     File.WriteAllText(this.tempFolderName + "\\" + fileName, script);
                     scripts.Add(script);
                 }
            }

            return scripts;
        }

        private bool ClearExistingTempDirectory()
        {
            try
            {
                if (Directory.Exists(this.tempFolderName))
                {
                    Directory.Delete(this.tempFolderName, true);
                }

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                this.tempFolderName = string.Empty;
                this.tempBuildFileName = string.Empty;
            }
        }
        private bool SetUpForNewScripts(string suggestedProjectName)
        {
            try
            {
                this.tempBuildFileName = suggestedProjectName;
                this.tempFolderName = Path.Combine(Path.GetTempPath() ,"SqlBuildManagerAudit_" + System.Guid.NewGuid().ToString());
                System.IO.Directory.CreateDirectory(this.tempFolderName);

                this.createdScriptFiles.Clear();
                return true;
            }
            catch
            {
                return false;
            }

        }

        #endregion
        private void mnuAuditTable_Click(object sender, System.EventArgs e)
        {
            if (this.lstTables.SelectedItems.Count > 0)
            {
                TableConfig table = new TableConfig(this.lstTables.SelectedItems[0].Text, this.lstTables.SelectedItems[0].Tag as SQLSyncAuditingDatabaseTableToAudit);
                GenerateScriptsToWindow(table, AuditScriptType.CreateAuditTable, table.TableName + " Audit Table");
            }
        }

        private void mnuMasterTrx_Click(object sender, System.EventArgs e)
		{
            TableConfig cfg = new TableConfig("", null);
            GenerateScriptsToWindow(cfg, AuditScriptType.MasterTable, "Audit Master Table");
		}

        private void mnuScriptAllAudit_Click(object sender, System.EventArgs e)
        {
            GenerateSelectedScripts(AuditScriptType.CreateAuditTable, this.data.DatabaseName + " Audit Tables");
        }
        private void scriptAuditTriggersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GenerateSelectedScripts(AuditScriptType.CreateAllTriggers, this.data.DatabaseName + " Audit Insert Trigger");
        }

        private void mnuSingleAuditTrigger_Click(object sender, System.EventArgs e)
        {
            if (this.lstTables.SelectedItems.Count > 0)
            {
                TableConfig table = new TableConfig(this.lstTables.SelectedItems[0].Text, this.lstTables.SelectedItems[0].Tag as SQLSyncAuditingDatabaseTableToAudit);
                GenerateScriptsToWindow(table, AuditScriptType.CreateAllTriggers, table.TableName + " Audit Triggers");
            }
        }

		private void mnuCompleteAudit_Click(object sender, System.EventArgs e)
		{
            this.ClearExistingTempDirectory();

            if (!this.SetUpForNewScripts(this.data.DatabaseName + " Audit.sbm"))
                return;

            List<string> scripts = new List<string>();

            List<TableConfig> tmp = new List<TableConfig>();
            tmp.Add(new TableConfig());
            //scripts.AddRange(GenerateAuditingScripts(tmp, AuditScriptType.MasterTable));
            List<TableConfig> tables = new List<TableConfig>();
            foreach(ListViewItem item in this.lstTables.CheckedItems)
            {
                tables.Add(new TableConfig(item.Text, item.Tag as SQLSyncAuditingDatabaseTableToAudit));
            }
            scripts.AddRange(GenerateAuditingScripts(tables,AuditScriptType.CreateAuditTable));
            scripts.AddRange(GenerateAuditingScripts(tables,AuditScriptType.CreateAllTriggers));
            //scripts.AddRange(GenerateAuditingScripts(tables, AuditScriptType.CreateUpdateTrigger));
            //scripts.AddRange(GenerateAuditingScripts(tables, AuditScriptType.CreateDeleteTrigger));

			StringBuilder sb = new StringBuilder();
            foreach (string script in scripts)
                sb.AppendLine(script + "\r\n---------------------------------");

            this.rtbSqlScript.Text = sb.ToString();

		}

		private void mnuDisableAuditTrigs_Click(object sender, System.EventArgs e)
		{
            GenerateSelectedScripts(AuditScriptType.TriggerDisable, this.data.DatabaseName + " Disable Audit Triggers");
		}

        private void mnuEnableAuditTrigs_Click(object sender, System.EventArgs e)
        {
            GenerateSelectedScripts(AuditScriptType.TriggerEnable, this.data.DatabaseName + " Enable Audit Triggers");
        }
		

		private void mnuCopy_Click(object sender, System.EventArgs e)
		{
			Clipboard.SetDataObject(this.rtbSqlScript.Text);
		}

		private void ToolStripMenuItem11_Click(object sender, System.EventArgs e)
		{
			if(DialogResult.OK == saveFileDialog1.ShowDialog())
			{
				using(StreamWriter sw = File.CreateText(saveFileDialog1.FileName))
				{
					sw.WriteLine(this.rtbSqlScript.Text);
					sw.Flush();
					sw.Close();
				}
			}
		}

		private void SaveSqlFilesToNewBuildFile(string buildFileName, string directory)
		{
			//string[] files = Directory.GetFiles(directory);

			if(File.Exists(buildFileName))
			{
                if (this.SqlBuildManagerFileExport != null)
                {
                    SqlBuild.SqlBuildForm frmBuild = new SqlBuildForm(buildFileName, this.data);
                    frmBuild.Show();
                    frmBuild.BulkAdd(this.createdScriptFiles.ToArray());
                }
                else
                {
                    MessageBox.Show("This file already exists. Please select a different file", "File Exists", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    return;
                }
			}
			else
			{
				SqlSyncBuildData buildData = SqlBuildFileHelper.CreateShellSqlSyncBuildDataObject();
                string projFileName = Path.Combine(directory, XmlFileNames.MainProjectFile);
                for (int i = 0; i < this.createdScriptFiles.Count; i++)
				{
                    string shortFileName = Path.GetFileName(this.createdScriptFiles[i]);
					if(shortFileName == XmlFileNames.MainProjectFile ||
						shortFileName == XmlFileNames.ExportFile)
						continue;

					statStatus.Text = "Adding "+ shortFileName + " to Build File";

					SqlBuildFileHelper.AddScriptFileToBuild(
						ref buildData,
						projFileName,
						shortFileName,
						i+1,
						"User Data Auditing",
						true,
						true,
						this.data.DatabaseName,
						true,
						buildFileName,
						false,
						true,
						System.Environment.UserName,
						EnterpriseConfigHelper.GetMinumumScriptTimeout(shortFileName, Properties.Settings.Default.DefaultMinimumScriptTimeout),
                        "");

				}

				statStatus.Text = "Saving to Build File "+Path.GetFileName(buildFileName);
				SqlBuildFileHelper.SaveSqlBuildProjectFile(ref buildData,projFileName,buildFileName);
                ////Delete the temp directory

                //files = Directory.GetFiles(directory);
                //for(int i=0;i<files.Length;i++)
                //    File.Delete(files[i]);
                //Directory.Delete(directory,true);
				statStatus.Text = "Build file "+ Path.GetFileName(buildFileName) +" saved.";

                OpenFileMessage(buildFileName);
			}
		}
		
		private void OpenFileMessage(string fileName)
		{
			if(DialogResult.Yes == MessageBox.Show("Scripting Complete. Open File?","Open File",MessageBoxButtons.YesNo,MessageBoxIcon.Question))
			{
				System.Diagnostics.Process prc = new System.Diagnostics.Process();
				prc.StartInfo.FileName = fileName;
				prc.Start();
			}
		}

        #region Prefix/Suffix settings
		private void radPrefix_CheckedChanged(object sender, System.EventArgs e)
		{
			SqlSync.TableScript.Audit.AuditHelper.AuditTableNameFormat = "Audit_{0}";
		}

		private void radSuffix_CheckedChanged(object sender, System.EventArgs e)
		{
			SqlSync.TableScript.Audit.AuditHelper.AuditTableNameFormat = "{0}_Audit";
		}
        #endregion

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
                DbInformation.DatabaseList dbList = SqlSync.DbInformation.InfoHelper.GetDatabaseList(this.data);
            }
            catch
            {
                MessageBox.Show("Error retrieving database list. Is the server running?", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.data = oldConnData;
                this.settingsControl1.Server = oldConnData.SQLServerName;
            }


            this.Cursor = Cursors.Default;
        }

        private void toolStripSave_Click(object sender, EventArgs e)
        {
            if (this.tempFolderName.Length == 0)
            {
                MessageBox.Show("Nothing to save", "Gotta have something to save", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            this.saveFileDialog1.FileName = this.tempBuildFileName;
            if (DialogResult.OK == this.saveFileDialog1.ShowDialog())
            {
                SaveSqlFilesToNewBuildFile(this.saveFileDialog1.FileName, this.tempFolderName);
            }
        }

        private void saveToOpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if(this.SqlBuildManagerFileExport != null && this.tempFolderName.Length > 0)
            {
                string[] files = Directory.GetFiles(this.tempFolderName);
                this.SqlBuildManagerFileExport(this,new SqlBuildManagerFileExportEventArgs(files));
            }
            
        }

        private void DataAuditForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            ClearExistingTempDirectory();
        }

        private void splitButton_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripSplitButton)
                ((ToolStripSplitButton)sender).ShowDropDown();
        }

     

        

      

       

	}

    public delegate void SqlBuildManagerAuditFileExportHandler(object sender, SqlBuildManagerAuditFileExportEventArgs e);
    public class SqlBuildManagerAuditFileExportEventArgs
    {
        public readonly string[] FileNames;
        public SqlBuildManagerAuditFileExportEventArgs(string[] fileNames)
        {
            this.FileNames = fileNames;
        }
    }
		
}

