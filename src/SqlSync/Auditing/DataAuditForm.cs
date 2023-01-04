using SqlBuildManager.Enterprise;
using SqlSync.Connection;
using SqlSync.Constants;
using SqlSync.DbInformation;
using SqlSync.MRU;
using SqlSync.ObjectScript;
using SqlSync.SqlBuild;
using SqlSync.TableScript;
using SqlSync.TableScript.Audit;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
namespace SqlSync
{
    /// <summary>
    /// Summary description for LookUpTable.
    /// </summary>
    public class DataAuditForm : System.Windows.Forms.Form, IMRUClient
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
            projectFileName = fileName;

        }
        public DataAuditForm(ConnectionData data)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            this.data = data;

        }
        public DataAuditForm(ConnectionData data, bool allowBuildManagerExport) : this(data)
        {
            this.allowBuildManagerExport = allowBuildManagerExport;
        }
        #endregion
        List<string> createdScriptFiles = new List<string>();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataAuditForm));
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Added to Project File", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Candidate Tables (have a sister \"audit\" table)", System.Windows.Forms.HorizontalAlignment.Left);
            UrielGuy.SyntaxHighlighting.HighLightDescriptorCollection highLightDescriptorCollection1 = new UrielGuy.SyntaxHighlighting.HighLightDescriptorCollection();
            contextMenu1 = new System.Windows.Forms.ContextMenuStrip(components);
            menuItem6 = new System.Windows.Forms.ToolStripSeparator();
            mnuScriptMissingColumns = new System.Windows.Forms.ToolStripMenuItem();
            mnuScriptTrigger = new System.Windows.Forms.ToolStripMenuItem();
            mnuScriptDefault = new System.Windows.Forms.ToolStripMenuItem();
            mnuAuditTable = new System.Windows.Forms.ToolStripMenuItem();
            mnuSingleAuditTrigger = new System.Windows.Forms.ToolStripMenuItem();
            menuItem3 = new System.Windows.Forms.ToolStripSeparator();
            mnuAddTable = new System.Windows.Forms.ToolStripMenuItem();
            mnuDeleteTable = new System.Windows.Forms.ToolStripMenuItem();
            menuItem1 = new System.Windows.Forms.ToolStripSeparator();
            mnuCopyTables = new System.Windows.Forms.ToolStripMenuItem();
            mnuCopyTablesForsql = new System.Windows.Forms.ToolStripMenuItem();
            statusBar1 = new System.Windows.Forms.StatusStrip();
            statStatus = new System.Windows.Forms.ToolStripStatusLabel();
            openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            imageList1 = new System.Windows.Forms.ImageList(components);
            ddDatabaseList = new System.Windows.Forms.ComboBox();
            label3 = new System.Windows.Forms.Label();
            lnkCheckAll = new System.Windows.Forms.LinkLabel();
            label1 = new System.Windows.Forms.Label();
            lstTables = new System.Windows.Forms.ListView();
            columnHeader1 = new System.Windows.Forms.ColumnHeader();
            columnHeader2 = new System.Windows.Forms.ColumnHeader();
            mainMenu1 = new System.Windows.Forms.MenuStrip();
            mnuActionMain = new System.Windows.Forms.ToolStripMenuItem();
            mnuLoadProjectFile = new System.Windows.Forms.ToolStripMenuItem();
            mnuChangeSqlServer = new System.Windows.Forms.ToolStripMenuItem();
            menuItem4 = new System.Windows.Forms.ToolStripSeparator();
            mnuFileMRU = new System.Windows.Forms.ToolStripMenuItem();
            panel1 = new System.Windows.Forms.Panel();
            grpDatabaseInfo = new System.Windows.Forms.GroupBox();
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            backgrounLegendToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            allowMultipleRunsOfScriptOnSameServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            leaveTransactionTextInScriptsdontStripOutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            groupingHelpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            candidateTablesTheToolHasDetectedThatTheseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            addedToProjectFileYouHaveSelectedThisTableForAuditingToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            radPrefix = new System.Windows.Forms.RadioButton();
            radSuffix = new System.Windows.Forms.RadioButton();
            saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            splitter1 = new System.Windows.Forms.Splitter();
            panel3 = new System.Windows.Forms.Panel();
            toolStripCommands = new System.Windows.Forms.ToolStrip();
            toolStripSplitButton1 = new System.Windows.Forms.ToolStripSplitButton();
            scriptCompletedAuditScriptSolutionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            scriptAuditTablesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            scriptAuditTriggersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            scriptMasterTransactionTableToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSplitButton2 = new System.Windows.Forms.ToolStripSplitButton();
            disableAuditTriggersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            enableAuditTriggersToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSplitButton3 = new System.Windows.Forms.ToolStripSplitButton();
            saveToOpenToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            saveToNewSBMPackageToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            rtbSqlScript = new UrielGuy.SyntaxHighlighting.SyntaxHighlightingTextBox();
            contextMenu2 = new System.Windows.Forms.ContextMenuStrip(components);
            mnuCopy = new System.Windows.Forms.ToolStripMenuItem();
            menuItem11 = new System.Windows.Forms.ToolStripMenuItem();
            splitter2 = new System.Windows.Forms.Splitter();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            settingsControl1 = new SqlSync.SettingsControl();
            contextMenu1.SuspendLayout();
            statusBar1.SuspendLayout();
            mainMenu1.SuspendLayout();
            panel1.SuspendLayout();
            grpDatabaseInfo.SuspendLayout();
            menuStrip1.SuspendLayout();
            panel3.SuspendLayout();
            toolStripCommands.SuspendLayout();
            contextMenu2.SuspendLayout();
            SuspendLayout();
            // 
            // contextMenu1
            // 
            contextMenu1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            menuItem6,
            mnuScriptMissingColumns,
            mnuScriptTrigger,
            mnuScriptDefault,
            mnuAuditTable,
            mnuSingleAuditTrigger,
            menuItem3,
            mnuAddTable,
            mnuDeleteTable,
            menuItem1,
            mnuCopyTables,
            mnuCopyTablesForsql});
            contextMenu1.Name = "contextMenu1";
            contextMenu1.Size = new System.Drawing.Size(283, 220);
            contextMenu1.Click += new System.EventHandler(contextMenu1_Popup);
            // 
            // menuItem6
            // 
            menuItem6.MergeIndex = 0;
            menuItem6.Name = "menuItem6";
            menuItem6.Size = new System.Drawing.Size(279, 6);
            menuItem6.Visible = false;
            // 
            // mnuScriptMissingColumns
            // 
            mnuScriptMissingColumns.Enabled = false;
            mnuScriptMissingColumns.MergeIndex = 1;
            mnuScriptMissingColumns.Name = "mnuScriptMissingColumns";
            mnuScriptMissingColumns.Size = new System.Drawing.Size(282, 22);
            mnuScriptMissingColumns.Text = "Script For Missing Update Columns";
            mnuScriptMissingColumns.Visible = false;
            // 
            // mnuScriptTrigger
            // 
            mnuScriptTrigger.MergeIndex = 2;
            mnuScriptTrigger.Name = "mnuScriptTrigger";
            mnuScriptTrigger.Size = new System.Drawing.Size(282, 22);
            mnuScriptTrigger.Text = "Script For Update Trigger";
            mnuScriptTrigger.Visible = false;
            // 
            // mnuScriptDefault
            // 
            mnuScriptDefault.MergeIndex = 3;
            mnuScriptDefault.Name = "mnuScriptDefault";
            mnuScriptDefault.Size = new System.Drawing.Size(282, 22);
            mnuScriptDefault.Text = "Script to Reset Update Column Defaults";
            mnuScriptDefault.Visible = false;
            // 
            // mnuAuditTable
            // 
            mnuAuditTable.MergeIndex = 4;
            mnuAuditTable.Name = "mnuAuditTable";
            mnuAuditTable.Size = new System.Drawing.Size(282, 22);
            mnuAuditTable.Text = "Script for Audit Table";
            mnuAuditTable.Click += new System.EventHandler(mnuAuditTable_Click);
            // 
            // mnuSingleAuditTrigger
            // 
            mnuSingleAuditTrigger.MergeIndex = 5;
            mnuSingleAuditTrigger.Name = "mnuSingleAuditTrigger";
            mnuSingleAuditTrigger.Size = new System.Drawing.Size(282, 22);
            mnuSingleAuditTrigger.Text = "Script for Audit Triggers";
            mnuSingleAuditTrigger.Click += new System.EventHandler(mnuSingleAuditTrigger_Click);
            // 
            // menuItem3
            // 
            menuItem3.MergeIndex = 6;
            menuItem3.Name = "menuItem3";
            menuItem3.Size = new System.Drawing.Size(279, 6);
            // 
            // mnuAddTable
            // 
            mnuAddTable.MergeIndex = 7;
            mnuAddTable.Name = "mnuAddTable";
            mnuAddTable.Size = new System.Drawing.Size(282, 22);
            mnuAddTable.Text = "Add Table";
            mnuAddTable.Click += new System.EventHandler(mnuAddTable_Click);
            // 
            // mnuDeleteTable
            // 
            mnuDeleteTable.MergeIndex = 8;
            mnuDeleteTable.Name = "mnuDeleteTable";
            mnuDeleteTable.Size = new System.Drawing.Size(282, 22);
            mnuDeleteTable.Text = "Delete Selected Table";
            mnuDeleteTable.Click += new System.EventHandler(mnuDeleteTable_Click);
            // 
            // menuItem1
            // 
            menuItem1.MergeIndex = 9;
            menuItem1.Name = "menuItem1";
            menuItem1.Size = new System.Drawing.Size(279, 6);
            // 
            // mnuCopyTables
            // 
            mnuCopyTables.MergeIndex = 10;
            mnuCopyTables.Name = "mnuCopyTables";
            mnuCopyTables.Size = new System.Drawing.Size(282, 22);
            mnuCopyTables.Text = "Copy Table List";
            mnuCopyTables.Click += new System.EventHandler(mnuCopyTables_Click);
            // 
            // mnuCopyTablesForsql
            // 
            mnuCopyTablesForsql.MergeIndex = 11;
            mnuCopyTablesForsql.Name = "mnuCopyTablesForsql";
            mnuCopyTablesForsql.Size = new System.Drawing.Size(282, 22);
            mnuCopyTablesForsql.Text = "Copy Table List for Sql";
            mnuCopyTablesForsql.Click += new System.EventHandler(mnuCopyTablesForsql_Click);
            // 
            // statusBar1
            // 
            statusBar1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            statStatus});
            statusBar1.Location = new System.Drawing.Point(0, 606);
            statusBar1.Name = "statusBar1";
            statusBar1.Size = new System.Drawing.Size(896, 22);
            statusBar1.TabIndex = 6;
            statusBar1.Text = "statusBar1";
            // 
            // statStatus
            // 
            statStatus.Name = "statStatus";
            statStatus.Size = new System.Drawing.Size(881, 17);
            statStatus.Spring = true;
            // 
            // openFileDialog1
            // 
            openFileDialog1.CheckFileExists = false;
            openFileDialog1.DefaultExt = "xml";
            openFileDialog1.Filter = "Sql Sync Audit Config *.audit|*.audit|Sql Sync Audit Config *.adt|*.adt|XML Files" +
    "|*.xml|All Files|*.*";
            openFileDialog1.Title = "Open SQL Sync Audit Project File";
            // 
            // imageList1
            // 
            imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            imageList1.TransparentColor = System.Drawing.Color.Transparent;
            imageList1.Images.SetKeyName(0, "");
            imageList1.Images.SetKeyName(1, "");
            imageList1.Images.SetKeyName(2, "");
            imageList1.Images.SetKeyName(3, "");
            imageList1.Images.SetKeyName(4, "");
            imageList1.Images.SetKeyName(5, "");
            imageList1.Images.SetKeyName(6, "");
            // 
            // ddDatabaseList
            // 
            ddDatabaseList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            ddDatabaseList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            ddDatabaseList.Location = new System.Drawing.Point(12, 48);
            ddDatabaseList.Name = "ddDatabaseList";
            ddDatabaseList.Size = new System.Drawing.Size(248, 21);
            ddDatabaseList.TabIndex = 0;
            ddDatabaseList.SelectionChangeCommitted += new System.EventHandler(ddDatabaseList_SelectionChangeCommitted);
            // 
            // label3
            // 
            label3.Location = new System.Drawing.Point(10, 30);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(100, 16);
            label3.TabIndex = 15;
            label3.Text = "Select Database:";
            // 
            // lnkCheckAll
            // 
            lnkCheckAll.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            lnkCheckAll.Location = new System.Drawing.Point(140, 72);
            lnkCheckAll.Name = "lnkCheckAll";
            lnkCheckAll.Size = new System.Drawing.Size(120, 16);
            lnkCheckAll.TabIndex = 1;
            lnkCheckAll.TabStop = true;
            lnkCheckAll.Text = "Check All";
            lnkCheckAll.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            lnkCheckAll.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(lnkCheckAll_LinkClicked);
            // 
            // label1
            // 
            label1.Location = new System.Drawing.Point(12, 70);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(100, 16);
            label1.TabIndex = 9;
            label1.Text = "Tables to Script";
            // 
            // lstTables
            // 
            lstTables.Activation = System.Windows.Forms.ItemActivation.OneClick;
            lstTables.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            lstTables.CheckBoxes = true;
            lstTables.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader1,
            columnHeader2});
            lstTables.ContextMenuStrip = contextMenu1;
            lstTables.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lstTables.FullRowSelect = true;
            listViewGroup1.Header = "Added to Project File";
            listViewGroup1.Name = "lstGrpAdded";
            listViewGroup2.Header = "Candidate Tables (have a sister \"audit\" table)";
            listViewGroup2.Name = "lstGrpCandidate";
            lstTables.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2});
            lstTables.HideSelection = false;
            lstTables.Location = new System.Drawing.Point(14, 92);
            lstTables.Name = "lstTables";
            lstTables.Size = new System.Drawing.Size(248, 384);
            lstTables.Sorting = System.Windows.Forms.SortOrder.Ascending;
            lstTables.TabIndex = 8;
            lstTables.UseCompatibleStateImageBehavior = false;
            lstTables.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Table Name";
            columnHeader1.Width = 171;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Rows";
            // 
            // mainMenu1
            // 
            mainMenu1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            mnuActionMain});
            mainMenu1.Location = new System.Drawing.Point(0, 0);
            mainMenu1.Name = "mainMenu1";
            mainMenu1.Size = new System.Drawing.Size(896, 24);
            mainMenu1.TabIndex = 0;
            // 
            // mnuActionMain
            // 
            mnuActionMain.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            mnuLoadProjectFile,
            mnuChangeSqlServer,
            menuItem4,
            mnuFileMRU});
            mnuActionMain.MergeIndex = 0;
            mnuActionMain.Name = "mnuActionMain";
            mnuActionMain.Size = new System.Drawing.Size(54, 20);
            mnuActionMain.Text = "&Action";
            // 
            // mnuLoadProjectFile
            // 
            mnuLoadProjectFile.MergeIndex = 0;
            mnuLoadProjectFile.Name = "mnuLoadProjectFile";
            mnuLoadProjectFile.Size = new System.Drawing.Size(234, 22);
            mnuLoadProjectFile.Text = "&Load/ New Project File";
            mnuLoadProjectFile.Click += new System.EventHandler(mnuLoadProjectFile_Click);
            // 
            // mnuChangeSqlServer
            // 
            mnuChangeSqlServer.MergeIndex = 1;
            mnuChangeSqlServer.Name = "mnuChangeSqlServer";
            mnuChangeSqlServer.Size = new System.Drawing.Size(234, 22);
            mnuChangeSqlServer.Text = "&Change Sql Server Connection";
            mnuChangeSqlServer.Click += new System.EventHandler(mnuChangeSqlServer_Click);
            // 
            // menuItem4
            // 
            menuItem4.MergeIndex = 2;
            menuItem4.Name = "menuItem4";
            menuItem4.Size = new System.Drawing.Size(231, 6);
            // 
            // mnuFileMRU
            // 
            mnuFileMRU.MergeIndex = 3;
            mnuFileMRU.Name = "mnuFileMRU";
            mnuFileMRU.Size = new System.Drawing.Size(234, 22);
            mnuFileMRU.Text = "Recent Files";
            // 
            // panel1
            // 
            panel1.Controls.Add(grpDatabaseInfo);
            panel1.Dock = System.Windows.Forms.DockStyle.Left;
            panel1.Location = new System.Drawing.Point(0, 80);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(280, 526);
            panel1.TabIndex = 13;
            // 
            // grpDatabaseInfo
            // 
            grpDatabaseInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            grpDatabaseInfo.Controls.Add(menuStrip1);
            grpDatabaseInfo.Controls.Add(radPrefix);
            grpDatabaseInfo.Controls.Add(radSuffix);
            grpDatabaseInfo.Controls.Add(lnkCheckAll);
            grpDatabaseInfo.Controls.Add(label3);
            grpDatabaseInfo.Controls.Add(lstTables);
            grpDatabaseInfo.Controls.Add(ddDatabaseList);
            grpDatabaseInfo.Controls.Add(label1);
            grpDatabaseInfo.Enabled = false;
            grpDatabaseInfo.FlatStyle = System.Windows.Forms.FlatStyle.System;
            grpDatabaseInfo.Location = new System.Drawing.Point(2, 2);
            grpDatabaseInfo.Name = "grpDatabaseInfo";
            grpDatabaseInfo.Size = new System.Drawing.Size(272, 518);
            grpDatabaseInfo.TabIndex = 0;
            grpDatabaseInfo.TabStop = false;
            // 
            // menuStrip1
            // 
            menuStrip1.BackColor = System.Drawing.Color.Gainsboro;
            menuStrip1.Dock = System.Windows.Forms.DockStyle.Bottom;
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            backgrounLegendToolStripMenuItem,
            groupingHelpToolStripMenuItem});
            menuStrip1.Location = new System.Drawing.Point(3, 491);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(266, 24);
            menuStrip1.TabIndex = 20;
            menuStrip1.Text = "menuStrip1";
            // 
            // backgrounLegendToolStripMenuItem
            // 
            backgrounLegendToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            allowMultipleRunsOfScriptOnSameServerToolStripMenuItem,
            leaveTransactionTextInScriptsdontStripOutToolStripMenuItem});
            backgrounLegendToolStripMenuItem.ForeColor = System.Drawing.Color.Blue;
            backgrounLegendToolStripMenuItem.Name = "backgrounLegendToolStripMenuItem";
            backgrounLegendToolStripMenuItem.Size = new System.Drawing.Size(143, 20);
            backgrounLegendToolStripMenuItem.Text = "Background Color Help";
            // 
            // allowMultipleRunsOfScriptOnSameServerToolStripMenuItem
            // 
            allowMultipleRunsOfScriptOnSameServerToolStripMenuItem.BackColor = System.Drawing.Color.Red;
            allowMultipleRunsOfScriptOnSameServerToolStripMenuItem.Name = "allowMultipleRunsOfScriptOnSameServerToolStripMenuItem";
            allowMultipleRunsOfScriptOnSameServerToolStripMenuItem.Size = new System.Drawing.Size(681, 22);
            allowMultipleRunsOfScriptOnSameServerToolStripMenuItem.Text = "The highlighted table is missing one or more triggers required for auditing";
            // 
            // leaveTransactionTextInScriptsdontStripOutToolStripMenuItem
            // 
            leaveTransactionTextInScriptsdontStripOutToolStripMenuItem.BackColor = System.Drawing.Color.LightBlue;
            leaveTransactionTextInScriptsdontStripOutToolStripMenuItem.Name = "leaveTransactionTextInScriptsdontStripOutToolStripMenuItem";
            leaveTransactionTextInScriptsdontStripOutToolStripMenuItem.RightToLeftAutoMirrorImage = true;
            leaveTransactionTextInScriptsdontStripOutToolStripMenuItem.Size = new System.Drawing.Size(681, 22);
            leaveTransactionTextInScriptsdontStripOutToolStripMenuItem.Text = "The \"candidate\" table has all the objects for auditing (audit table and triggers)" +
    ". Do you want to add it to the project?";
            // 
            // groupingHelpToolStripMenuItem
            // 
            groupingHelpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            candidateTablesTheToolHasDetectedThatTheseToolStripMenuItem,
            addedToProjectFileYouHaveSelectedThisTableForAuditingToolStripMenuItem});
            groupingHelpToolStripMenuItem.ForeColor = System.Drawing.Color.Blue;
            groupingHelpToolStripMenuItem.Name = "groupingHelpToolStripMenuItem";
            groupingHelpToolStripMenuItem.Size = new System.Drawing.Size(97, 20);
            groupingHelpToolStripMenuItem.Text = "Grouping Help";
            // 
            // candidateTablesTheToolHasDetectedThatTheseToolStripMenuItem
            // 
            candidateTablesTheToolHasDetectedThatTheseToolStripMenuItem.Name = "candidateTablesTheToolHasDetectedThatTheseToolStripMenuItem";
            candidateTablesTheToolHasDetectedThatTheseToolStripMenuItem.Size = new System.Drawing.Size(761, 22);
            candidateTablesTheToolHasDetectedThatTheseToolStripMenuItem.Text = "\"Candidate Tables\": The tool has detected that these tables have a matching \"audi" +
    "t\" table. Do you want to add them to the project?";
            // 
            // addedToProjectFileYouHaveSelectedThisTableForAuditingToolStripMenuItem
            // 
            addedToProjectFileYouHaveSelectedThisTableForAuditingToolStripMenuItem.Name = "addedToProjectFileYouHaveSelectedThisTableForAuditingToolStripMenuItem";
            addedToProjectFileYouHaveSelectedThisTableForAuditingToolStripMenuItem.Size = new System.Drawing.Size(761, 22);
            addedToProjectFileYouHaveSelectedThisTableForAuditingToolStripMenuItem.Text = "\"Added to Project File\": You have selected this table for auditing";
            // 
            // radPrefix
            // 
            radPrefix.Location = new System.Drawing.Point(138, 8);
            radPrefix.Name = "radPrefix";
            radPrefix.Size = new System.Drawing.Size(106, 24);
            radPrefix.TabIndex = 19;
            radPrefix.Text = "Audit as Prefix";
            radPrefix.CheckedChanged += new System.EventHandler(radPrefix_CheckedChanged);
            // 
            // radSuffix
            // 
            radSuffix.Checked = true;
            radSuffix.Location = new System.Drawing.Point(10, 8);
            radSuffix.Name = "radSuffix";
            radSuffix.Size = new System.Drawing.Size(106, 24);
            radSuffix.TabIndex = 18;
            radSuffix.TabStop = true;
            radSuffix.Text = "Audit as Suffix";
            radSuffix.CheckedChanged += new System.EventHandler(radSuffix_CheckedChanged);
            // 
            // saveFileDialog1
            // 
            saveFileDialog1.Filter = "Sql Build Manager Project *.sbm|*.sbm|All Files *.*|*.*";
            saveFileDialog1.OverwritePrompt = false;
            saveFileDialog1.Title = "Save Sql Build Manager File";
            // 
            // splitter1
            // 
            splitter1.Location = new System.Drawing.Point(280, 80);
            splitter1.Name = "splitter1";
            splitter1.Size = new System.Drawing.Size(3, 526);
            splitter1.TabIndex = 14;
            splitter1.TabStop = false;
            // 
            // panel3
            // 
            panel3.Controls.Add(toolStripCommands);
            panel3.Controls.Add(rtbSqlScript);
            panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            panel3.Location = new System.Drawing.Point(283, 80);
            panel3.Name = "panel3";
            panel3.Size = new System.Drawing.Size(613, 526);
            panel3.TabIndex = 17;
            // 
            // toolStripCommands
            // 
            toolStripCommands.Enabled = false;
            toolStripCommands.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            toolStripSplitButton1,
            toolStripSplitButton2,
            toolStripSplitButton3});
            toolStripCommands.Location = new System.Drawing.Point(0, 0);
            toolStripCommands.Margin = new System.Windows.Forms.Padding(5);
            toolStripCommands.Name = "toolStripCommands";
            toolStripCommands.Padding = new System.Windows.Forms.Padding(5);
            toolStripCommands.Size = new System.Drawing.Size(613, 32);
            toolStripCommands.TabIndex = 10;
            toolStripCommands.Text = "toolStripCommands";
            // 
            // toolStripSplitButton1
            // 
            toolStripSplitButton1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            scriptCompletedAuditScriptSolutionToolStripMenuItem,
            toolStripSeparator1,
            scriptAuditTablesToolStripMenuItem,
            scriptAuditTriggersToolStripMenuItem,
            toolStripSeparator2,
            scriptMasterTransactionTableToolStripMenuItem});
            toolStripSplitButton1.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripSplitButton1.Name = "toolStripSplitButton1";
            toolStripSplitButton1.RightToLeftAutoMirrorImage = true;
            toolStripSplitButton1.Size = new System.Drawing.Size(166, 19);
            toolStripSplitButton1.Text = "Auditing Solution Scripting";
            toolStripSplitButton1.ToolTipText = "Creates the scripts required to implement the \"sister table/trigger\" auditing sol" +
    "ution";
            toolStripSplitButton1.Click += new System.EventHandler(splitButton_Click);
            // 
            // scriptCompletedAuditScriptSolutionToolStripMenuItem
            // 
            scriptCompletedAuditScriptSolutionToolStripMenuItem.Name = "scriptCompletedAuditScriptSolutionToolStripMenuItem";
            scriptCompletedAuditScriptSolutionToolStripMenuItem.RightToLeftAutoMirrorImage = true;
            scriptCompletedAuditScriptSolutionToolStripMenuItem.Size = new System.Drawing.Size(416, 22);
            scriptCompletedAuditScriptSolutionToolStripMenuItem.Text = "Script Complete Audit  Solution";
            scriptCompletedAuditScriptSolutionToolStripMenuItem.ToolTipText = resources.GetString("scriptCompletedAuditScriptSolutionToolStripMenuItem.ToolTipText");
            scriptCompletedAuditScriptSolutionToolStripMenuItem.Click += new System.EventHandler(mnuCompleteAudit_Click);
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(413, 6);
            // 
            // scriptAuditTablesToolStripMenuItem
            // 
            scriptAuditTablesToolStripMenuItem.Name = "scriptAuditTablesToolStripMenuItem";
            scriptAuditTablesToolStripMenuItem.Size = new System.Drawing.Size(416, 22);
            scriptAuditTablesToolStripMenuItem.Text = "Audit Tables Creation/Modification Scripts -  for Checked Tables";
            scriptAuditTablesToolStripMenuItem.ToolTipText = "For the checked tables, will generate scripts for their \"sister\" audit tables\r\nan" +
    "d save them to the directory you choose\r\n";
            scriptAuditTablesToolStripMenuItem.Click += new System.EventHandler(mnuScriptAllAudit_Click);
            // 
            // scriptAuditTriggersToolStripMenuItem
            // 
            scriptAuditTriggersToolStripMenuItem.Name = "scriptAuditTriggersToolStripMenuItem";
            scriptAuditTriggersToolStripMenuItem.Size = new System.Drawing.Size(416, 22);
            scriptAuditTriggersToolStripMenuItem.Text = "Audit Triggers Creation/Modification Scripts - for Checked Tables";
            scriptAuditTriggersToolStripMenuItem.ToolTipText = "For the checked tables, will generate scripts for the 3 audit triggers (INSERT, U" +
    "PDATE, DELETE)\r\nand save them to the directory you choose\r\n";
            scriptAuditTriggersToolStripMenuItem.Click += new System.EventHandler(scriptAuditTriggersToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            toolStripSeparator2.Name = "toolStripSeparator2";
            toolStripSeparator2.Size = new System.Drawing.Size(413, 6);
            // 
            // scriptMasterTransactionTableToolStripMenuItem
            // 
            scriptMasterTransactionTableToolStripMenuItem.Enabled = false;
            scriptMasterTransactionTableToolStripMenuItem.Name = "scriptMasterTransactionTableToolStripMenuItem";
            scriptMasterTransactionTableToolStripMenuItem.Size = new System.Drawing.Size(416, 22);
            scriptMasterTransactionTableToolStripMenuItem.Text = "Master Transaction Table Creation Script";
            scriptMasterTransactionTableToolStripMenuItem.ToolTipText = "Generates the create script for the master audit table to the script window below" +
    ".";
            scriptMasterTransactionTableToolStripMenuItem.Visible = false;
            scriptMasterTransactionTableToolStripMenuItem.Click += new System.EventHandler(mnuMasterTrx_Click);
            // 
            // toolStripSplitButton2
            // 
            toolStripSplitButton2.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            disableAuditTriggersToolStripMenuItem,
            enableAuditTriggersToolStripMenuItem});
            toolStripSplitButton2.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripSplitButton2.Name = "toolStripSplitButton2";
            toolStripSplitButton2.Size = new System.Drawing.Size(140, 19);
            toolStripSplitButton2.Text = "Trigger Enable/Disable";
            toolStripSplitButton2.ToolTipText = "There may be cases where you need to disable the auditing triggers temporarily.\r\n" +
    "These commands will create the scripts to disable and enable the auditing trigge" +
    "rs.";
            toolStripSplitButton2.Click += new System.EventHandler(splitButton_Click);
            // 
            // disableAuditTriggersToolStripMenuItem
            // 
            disableAuditTriggersToolStripMenuItem.Name = "disableAuditTriggersToolStripMenuItem";
            disableAuditTriggersToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            disableAuditTriggersToolStripMenuItem.Text = "Disable Audit Triggers Scripts";
            disableAuditTriggersToolStripMenuItem.ToolTipText = "For the checked tables, create the scripts to disable the 3 auditing triggers (IN" +
    "SERT, UPDATE, DELETE)";
            disableAuditTriggersToolStripMenuItem.Click += new System.EventHandler(mnuDisableAuditTrigs_Click);
            // 
            // enableAuditTriggersToolStripMenuItem
            // 
            enableAuditTriggersToolStripMenuItem.Name = "enableAuditTriggersToolStripMenuItem";
            enableAuditTriggersToolStripMenuItem.Size = new System.Drawing.Size(226, 22);
            enableAuditTriggersToolStripMenuItem.Text = "Enable Audit Triggers Scripts";
            enableAuditTriggersToolStripMenuItem.ToolTipText = "For the checked tables, create the scripts to enable the 3 auditing triggers (INS" +
    "ERT, UPDATE, DELETE)";
            enableAuditTriggersToolStripMenuItem.Click += new System.EventHandler(mnuEnableAuditTrigs_Click);
            // 
            // toolStripSplitButton3
            // 
            toolStripSplitButton3.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            toolStripSplitButton3.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            saveToOpenToolStripMenuItem,
            saveToNewSBMPackageToolStripMenuItem});
            toolStripSplitButton3.ImageTransparentColor = System.Drawing.Color.Magenta;
            toolStripSplitButton3.Name = "toolStripSplitButton3";
            toolStripSplitButton3.Size = new System.Drawing.Size(85, 19);
            toolStripSplitButton3.Text = "Save Scripts";
            toolStripSplitButton3.ToolTipText = "Save scripts";
            toolStripSplitButton3.Click += new System.EventHandler(splitButton_Click);
            // 
            // saveToOpenToolStripMenuItem
            // 
            saveToOpenToolStripMenuItem.Enabled = false;
            saveToOpenToolStripMenuItem.Name = "saveToOpenToolStripMenuItem";
            saveToOpenToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            saveToOpenToolStripMenuItem.Text = "Save to Open  Sql Build Package";
            saveToOpenToolStripMenuItem.ToolTipText = "Save scripts to the Sql Build Package that is open in the parent window";
            saveToOpenToolStripMenuItem.Click += new System.EventHandler(saveToOpenToolStripMenuItem_Click);
            // 
            // saveToNewSBMPackageToolStripMenuItem
            // 
            saveToNewSBMPackageToolStripMenuItem.Name = "saveToNewSBMPackageToolStripMenuItem";
            saveToNewSBMPackageToolStripMenuItem.Size = new System.Drawing.Size(243, 22);
            saveToNewSBMPackageToolStripMenuItem.Text = "Save to new SBM package";
            saveToNewSBMPackageToolStripMenuItem.ToolTipText = "Save scripts to a new SBM package, or add them to an alternate existing SBM packa" +
    "ge";
            saveToNewSBMPackageToolStripMenuItem.Click += new System.EventHandler(toolStripSave_Click);
            // 
            // rtbSqlScript
            // 
            rtbSqlScript.AcceptsTab = true;
            rtbSqlScript.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            rtbSqlScript.CaseSensitive = false;
            rtbSqlScript.ContextMenuStrip = contextMenu2;
            rtbSqlScript.FilterAutoComplete = true;
            rtbSqlScript.Font = new System.Drawing.Font("Lucida Console", 8F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            rtbSqlScript.HighlightDescriptors = highLightDescriptorCollection1;
            rtbSqlScript.HighlightType = SqlSync.Highlighting.SyntaxHightlightType.Sql;
            rtbSqlScript.Location = new System.Drawing.Point(4, 38);
            rtbSqlScript.MaxUndoRedoSteps = 50;
            rtbSqlScript.Name = "rtbSqlScript";
            rtbSqlScript.Size = new System.Drawing.Size(604, 479);
            rtbSqlScript.SuspendHighlighting = false;
            rtbSqlScript.TabIndex = 9;
            rtbSqlScript.Text = "";
            // 
            // contextMenu2
            // 
            contextMenu2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            mnuCopy,
            menuItem11});
            contextMenu2.Name = "contextMenu2";
            contextMenu2.Size = new System.Drawing.Size(173, 48);
            // 
            // mnuCopy
            // 
            mnuCopy.MergeIndex = 0;
            mnuCopy.Name = "mnuCopy";
            mnuCopy.Size = new System.Drawing.Size(172, 22);
            mnuCopy.Text = "Copy To Clipboard";
            mnuCopy.Click += new System.EventHandler(mnuCopy_Click);
            // 
            // menuItem11
            // 
            menuItem11.MergeIndex = 1;
            menuItem11.Name = "menuItem11";
            menuItem11.Size = new System.Drawing.Size(172, 22);
            menuItem11.Text = "Save to File";
            // 
            // splitter2
            // 
            splitter2.Dock = System.Windows.Forms.DockStyle.Bottom;
            splitter2.Location = new System.Drawing.Point(283, 603);
            splitter2.Name = "splitter2";
            splitter2.Size = new System.Drawing.Size(613, 3);
            splitter2.TabIndex = 18;
            splitter2.TabStop = false;
            // 
            // settingsControl1
            // 
            settingsControl1.BackColor = System.Drawing.Color.White;
            settingsControl1.Dock = System.Windows.Forms.DockStyle.Top;
            settingsControl1.Location = new System.Drawing.Point(0, 24);
            settingsControl1.Name = "settingsControl1";
            settingsControl1.Project = "";
            settingsControl1.ProjectLabelText = "Project File:";
            settingsControl1.Server = "";
            settingsControl1.Size = new System.Drawing.Size(896, 56);
            settingsControl1.TabIndex = 12;
            settingsControl1.ServerChanged += new SqlSync.ServerChangedEventHandler(settingsControl1_ServerChanged);
            // 
            // DataAuditForm
            // 
            AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            ClientSize = new System.Drawing.Size(896, 628);
            Controls.Add(splitter2);
            Controls.Add(panel3);
            Controls.Add(splitter1);
            Controls.Add(panel1);
            Controls.Add(statusBar1);
            Controls.Add(settingsControl1);
            Controls.Add(mainMenu1);
            Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            MainMenuStrip = mainMenu1;
            Name = "DataAuditForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Sql Build Manager :: User Data Audit Scripting";
            FormClosing += new System.Windows.Forms.FormClosingEventHandler(DataAuditForm_FormClosing);
            Load += new System.EventHandler(LookUpTable_Load);
            contextMenu1.ResumeLayout(false);
            statusBar1.ResumeLayout(false);
            statusBar1.PerformLayout();
            mainMenu1.ResumeLayout(false);
            mainMenu1.PerformLayout();
            panel1.ResumeLayout(false);
            grpDatabaseInfo.ResumeLayout(false);
            grpDatabaseInfo.PerformLayout();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            toolStripCommands.ResumeLayout(false);
            toolStripCommands.PerformLayout();
            contextMenu2.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }
        #endregion

        private void LookUpTable_Load(object sender, System.EventArgs e)
        {
            if (data == null)
            {
                ConnectionForm frmConnect = new ConnectionForm("Sql Sync Auditing");
                DialogResult result = frmConnect.ShowDialog();
                if (result == DialogResult.OK)
                {
                    data = frmConnect.SqlConnection;
                }
                else
                {
                    MessageBox.Show("Sql Sync Auditing can not continue without a valid Sql Connection", "Unable to Load", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    Close();
                }

            }

            mruManager = new MRUManager();
            mruManager.Initialize(
                this,                              // owner form
                mnuActionMain,
                mnuFileMRU,                        // Recent Files menu item
                @"Software\Michael McKechney\Sql Sync\Sql Sync Auditing"); // Registry path to keep MRU list
            mruManager.MaxDisplayNameLength = 150;

            settingsControl1.Server = data.SQLServerName;
            if (projectFileName.Length > 0)
                LoadAuditTableData(projectFileName, true);


            if (SqlBuildManagerFileExport != null)
            {
                saveToOpenToolStripMenuItem.Enabled = true;
                saveToOpenToolStripMenuItem.ToolTipText = "Save scripts to the Sql Build Package that is open in the parent window";
            }
            else
            {
                saveToOpenToolStripMenuItem.Enabled = false;
                saveToOpenToolStripMenuItem.ToolTipText = "No Sql Build Package loaded. Unable to save to an open package";

            }





        }

        private void LoadAuditTableData(string configFile, bool validateSchema)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                if (File.Exists(configFile))
                {
                    try
                    {
                        using (StreamReader sr = new StreamReader(configFile))
                        {
                            System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(SqlSync.TableScript.Audit.SQLSyncAuditing));
                            object obj = serializer.Deserialize(sr);
                            auditConfig = (SqlSync.TableScript.Audit.SQLSyncAuditing)obj;
                        }
                    }
                    catch (Exception e)
                    {
                        string excep = e.ToString();
                    }

                }

                if (auditConfig != null)
                {
                    projectFileName = configFile;
                    settingsControl1.Project = configFile;
                    grpDatabaseInfo.Enabled = true;
                    toolStripCommands.Enabled = true;
                    mruManager.Add(configFile);
                    PopulateDatabaseList();
                }
                else
                {
                    MessageBox.Show("Unable to Read the selected file.\r\nIt is not a valid audit configuration file", "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
        private void PopulateDatabaseList()
        {
            if (auditConfig == null)
                return;

            string currentSelection = string.Empty;
            if (ddDatabaseList.Items.Count > 0 && ddDatabaseList.SelectedItem != null &&
                ddDatabaseList.SelectedItem.ToString() != DropDownConstants.AddNew)
            {
                currentSelection = ddDatabaseList.SelectedItem.ToString();
            }
            ddDatabaseList.Items.Clear();
            lstTables.Items.Clear();

            ddDatabaseList.Items.Add(DropDownConstants.SelectOne);
            ddDatabaseList.SelectedIndex = 0;
            if (auditConfig.Items != null)
            {
                for (int i = 0; i < auditConfig.Items.Length; i++)
                {
                    ddDatabaseList.Items.Add(auditConfig.Items[i].Name);
                    if (currentSelection.Length > 0 && currentSelection == auditConfig.Items[i].Name)
                        ddDatabaseList.SelectedIndex = i + 1;
                }
            }
            ddDatabaseList.Items.Add(DropDownConstants.AddNew);

            if (ddDatabaseList.SelectedIndex > 0)
                PopulateTableList(ddDatabaseList.SelectedItem.ToString());

        }
        private void PopulateTableList(string databaseName)
        {
            if (auditConfig == null)
                return;

            SQLSyncAuditingDatabase selectedDb = null;
            int auditConfigDbIndex = -1;
            for (int i = 0; i < auditConfig.Items.Length; i++)
            {
                if (auditConfig.Items[i].Name.ToLower() == databaseName.ToLower())
                {
                    selectedDb = auditConfig.Items[i];
                    auditConfigDbIndex = i;
                    break;
                }
            }

            if (selectedDb == null)
                return;


            statStatus.Text = "Retrieving Table List and Row Count";
            lstTables.Items.Clear();
            data.DatabaseName = databaseName;

            SqlSync.DbInformation.TableSize[] tables = SqlSync.DbInformation.InfoHelper.GetDatabaseTableListWithRowCount(data);
            AuditAutoDetectData[] trigTable = SqlSync.TableScript.Audit.AuditHelper.AutoDetectDataAuditing(data);
            for (int i = 0; i < tables.Length; i++)
            {
                if (selectedDb.TableToAudit != null)
                {
                    bool added = false;


                    for (int j = 0; j < selectedDb.TableToAudit.Length; j++)
                    {
                        string[] allTriggers = SqlSync.DbInformation.InfoHelper.GetTriggers(data);
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

                    for (int j = 0; j < trigTable.Length; j++)
                    {
                        if (tables[i].TableName.ToLower() == trigTable[j].TableName.ToLower())
                        {
                            ListViewItem item = new ListViewItem(new string[] { trigTable[j].TableName, trigTable[j].StringRowCount });
                            if (!trigTable[j].HasAuditDeleteTrigger || !trigTable[j].HasAuditInsertTrigger || !trigTable[j].HasAuditUpdateTrigger)
                                item.BackColor = Color.Red;
                            else
                                item.BackColor = Color.LightBlue;

                            //Set the tag object if available
                            for (int x = 0; x < auditConfig.Items[auditConfigDbIndex].TableToAudit.Length; x++)
                                if (auditConfig.Items[auditConfigDbIndex].TableToAudit[x].Name.ToLower() == tables[i].TableName.ToLower())
                                    item.Tag = auditConfig.Items[auditConfigDbIndex].TableToAudit[x];

                            item.Group = lstTables.Groups["lstGrpCandidate"];
                            lstTables.Items.Add(item);
                            break;
                        }
                    }




                }
            }

            statStatus.Text = "Ready.";
        }
        private void AddNewDatabase()
        {
            string[] listedDatabases = new string[ddDatabaseList.Items.Count - 2];
            for (int i = 1; i < ddDatabaseList.Items.Count - 1; i++)
            {
                listedDatabases[i - 1] = ddDatabaseList.Items[i].ToString();
            }
            SqlSync.TableScript.NewLookUpDatabaseForm frmNewDb = new SqlSync.TableScript.NewLookUpDatabaseForm(new PopulateHelper(data, null).RemainingDatabases(listedDatabases));
            DialogResult result = frmNewDb.ShowDialog();
            if (result == DialogResult.OK)
            {
                string[] newDatabases = frmNewDb.DatabaseList;
                SaveNewDatabaseNames(newDatabases);
            }
        }
        private void SaveNewDatabaseNames(string[] newDatabases)
        {
            ArrayList lst = new ArrayList();
            if (auditConfig.Items != null)
                lst.AddRange(auditConfig.Items);
            for (int i = 0; i < newDatabases.Length; i++)
            {
                SQLSyncAuditingDatabase db = new SQLSyncAuditingDatabase();
                db.Name = newDatabases[i];
                lst.Add(db);
            }
            auditConfig.Items = new SQLSyncAuditingDatabase[lst.Count];
            lst.CopyTo(auditConfig.Items);

            SaveXmlTemplate(projectFileName);
            LoadAuditTableData(projectFileName, false);
        }

        private void SaveXmlTemplate(string fileName)
        {

            try
            {
                System.Xml.XmlTextWriter tw = null;
                try
                {
                    XmlSerializer xmlS = new XmlSerializer(typeof(SqlSync.TableScript.Audit.SQLSyncAuditing));
                    tw = new System.Xml.XmlTextWriter(projectFileName, Encoding.UTF8);
                    tw.Formatting = System.Xml.Formatting.Indented;
                    xmlS.Serialize(tw, auditConfig);
                }
                finally
                {
                    if (tw != null)
                        tw.Close();
                }
            }
            catch (System.UnauthorizedAccessException)
            {
                MessageBox.Show("Unable to save project file to:\r\n" + fileName, "Unable to Save", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private SQLSyncAuditingDatabase GetDatabaseObject(string databaseName)
        {
            for (int i = 0; i < auditConfig.Items.Length; i++)
            {
                if (auditConfig.Items[i].Name.ToLower() == databaseName.ToLower())
                    return auditConfig.Items[i];
            }
            return null;
        }
        private SQLSyncAuditingDatabaseTableToAudit GetTableObject(string databaseName, string tableName)
        {

            SQLSyncAuditingDatabase selectedDb = GetDatabaseObject(databaseName);
            if (selectedDb == null)
                return null;

            for (int i = 0; i < selectedDb.TableToAudit.Length; i++)
            {
                if (selectedDb.TableToAudit[i].Name.ToLower() == tableName.ToLower())
                    return selectedDb.TableToAudit[i];
            }
            return null;
        }

        private void RemoveDatabase(string databaseName)
        {
            if (auditConfig == null)
                return;

            SQLSyncAuditingDatabase selectedDb = GetDatabaseObject(databaseName);

            if (selectedDb == null)
                return;

            if (selectedDb.TableToAudit.Length > 0)
            {
                string entry = (selectedDb.TableToAudit.Length > 1) ? "entries" : "entry";
                string message = "The Database " + selectedDb.Name + " has " + selectedDb.TableToAudit.Length.ToString() + " table " + entry + ".\r\nAre you sure you want to remove it?";
                DialogResult result = MessageBox.Show(message, "Confirm Removal", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No) return;
            }

            for (int i = 0; i < auditConfig.Items.Length; i++)
            {
                if (auditConfig.Items[i].Equals(selectedDb))
                {
                    auditConfig.Items[i] = null;
                    break;
                }
            }

            SaveXmlTemplate(projectFileName);
            LoadAuditTableData(projectFileName, false);
        }
        private void RemoveTable(string databaseName, string tableName)
        {

            SQLSyncAuditingDatabase selectedDb = GetDatabaseObject(databaseName);
            if (selectedDb == null)
                return;

            for (int i = 0; i < selectedDb.TableToAudit.Length; i++)
            {
                if (selectedDb.TableToAudit[i].Name.ToLower() == tableName.ToLower())
                {
                    selectedDb.TableToAudit[i] = null;
                    break;
                }
            }

            SaveXmlTemplate(projectFileName);
            LoadAuditTableData(projectFileName, false);


        }
        private void AddNewTables(List<string> tables)
        {
            SQLSyncAuditingDatabase selectedDb = GetDatabaseObject(data.DatabaseName);
            if (selectedDb == null)
                return;

            ArrayList newLst = new ArrayList();
            for (int i = 0; i < tables.Count; i++)
            {
                SQLSyncAuditingDatabaseTableToAudit table = new SQLSyncAuditingDatabaseTableToAudit();
                table.Name = tables[i];
                newLst.Add(table);
            }
            ArrayList current = new ArrayList();
            if (selectedDb.TableToAudit != null)
                current.AddRange(selectedDb.TableToAudit);
            current.AddRange(newLst);
            selectedDb.TableToAudit = new SQLSyncAuditingDatabaseTableToAudit[current.Count];
            current.CopyTo(selectedDb.TableToAudit);


            SaveXmlTemplate(projectFileName);
            LoadAuditTableData(projectFileName, false);
        }
        private void lnkCheckAll_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            if (lnkCheckAll.Text == "Check All")
            {
                for (int i = 0; i < lstTables.Items.Count; i++)
                    lstTables.Items[i].Checked = true;

                lnkCheckAll.Text = "Uncheck All";
            }
            else
            {
                while (lstTables.CheckedItems.Count > 0)
                    lstTables.CheckedItems[0].Checked = false;

                lnkCheckAll.Text = "Check All";
            }
        }

        private void mnuAddTable_Click(object sender, System.EventArgs e)
        {
            List<string> listedTables = new List<string>();
            for (int i = 0; i < lstTables.Items.Count; i++)
            {
                if (lstTables.Items[i].Group == lstTables.Groups["lstGrpAdded"])
                    listedTables.Add(lstTables.Items[i].Text);
            }

            NewLookUpForm frmNew = new NewLookUpForm(new PopulateHelper(data, null).RemainingTables(listedTables));
            DialogResult result = frmNew.ShowDialog();
            if (result == DialogResult.OK)
            {
                AddNewTables(frmNew.TableList);
            }
        }

        private void mnuDeleteTable_Click(object sender, System.EventArgs e)
        {
            if (lstTables.SelectedItems.Count > 0)
            {
                ListViewItem item = lstTables.SelectedItems[0];
                RemoveTable(data.DatabaseName, item.Text);
            }
        }

        private void ddDatabaseList_SelectionChangeCommitted(object sender, System.EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                switch (ddDatabaseList.SelectedItem.ToString())
                {
                    case DropDownConstants.SelectOne:
                        lstTables.Items.Clear();
                        break;
                    case DropDownConstants.AddNew:
                        AddNewDatabase();
                        break;
                    default:
                        PopulateTableList(ddDatabaseList.Text);
                        break;
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }

        }

        private void mnuRemoveDatabase_Click(object sender, System.EventArgs e)
        {
            RemoveDatabase(ddDatabaseList.SelectedItem.ToString());
        }

        private void mnuLoadProjectFile_Click(object sender, System.EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (File.Exists(openFileDialog1.FileName) == false)
                {
                    projectFileName = openFileDialog1.FileName;
                    auditConfig = new SQLSyncAuditing();
                    SaveXmlTemplate(openFileDialog1.FileName);
                }

                OpenMRUFile(openFileDialog1.FileName);
            }
        }

        private void mnuChangeSqlServer_Click(object sender, System.EventArgs e)
        {
            ConnectionForm frmConnect = new ConnectionForm("Sql Table Script");
            DialogResult result = frmConnect.ShowDialog();
            if (result == DialogResult.OK)
            {
                data = frmConnect.SqlConnection;
                settingsControl1.Server = data.SQLServerName;
            }
        }

        private void contextMenu1_Popup(object sender, System.EventArgs e)
        {
            if (lstTables.SelectedItems.Count > 0)
            {
                if (lstTables.SelectedItems[0].ForeColor.ToString() == Color.Purple.ToString())
                    mnuScriptMissingColumns.Enabled = true;
                else
                    mnuScriptMissingColumns.Enabled = false;
            }
        }

        private void mnuCopyTablesForsql_Click(object sender, System.EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < lstTables.Items.Count; i++)
            {
                sb.Append("'" + lstTables.Items[i].Text + "',");
            }
            sb.Length = sb.Length - 1;
            Clipboard.SetDataObject(sb.ToString(), true);
        }

        private void mnuCopyTables_Click(object sender, System.EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < lstTables.Items.Count; i++)
            {
                sb.Append(lstTables.Items[i].Text + "\r\n");
            }
            sb.Length = sb.Length - 2;
            Clipboard.SetDataObject(sb.ToString(), true);
        }

        private void SettingsItem_Click(object sender, System.EventArgs e)
        {
            ((ToolStripMenuItem)sender).Checked = !((ToolStripMenuItem)sender).Checked;
        }


        #region IMRUClient Members

        public void OpenMRUFile(string fileName)
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                if (System.IO.File.Exists(fileName))
                {
                    projectFileName = fileName;
                    LoadAuditTableData(projectFileName, true);
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        #endregion

        public event SqlBuildManagerFileExportHandler SqlBuildManagerFileExport;

        private void disp_SqlBuildManagerFileExport(object sender, SqlBuildManagerFileExportEventArgs e)
        {
            if (SqlBuildManagerFileExport != null)
                SqlBuildManagerFileExport(sender, e);
        }


        #region Script Creation and Set-up
        private void GenerateSelectedScripts(AuditScriptType type, string suggestedProjectName)
        {
            List<TableConfig> tables = new List<TableConfig>();
            for (int i = 0; i < lstTables.CheckedItems.Count; i++)
            {
                tables.Add(new TableConfig(lstTables.CheckedItems[i].Text, lstTables.CheckedItems[i].Tag as SQLSyncAuditingDatabaseTableToAudit));
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
            ClearExistingTempDirectory();

            if (!SetUpForNewScripts(suggestedProjectName))
                return;

            List<string> scripts = GenerateAuditingScripts(tables, type);
            StringBuilder sb = new StringBuilder();
            foreach (string script in scripts)
                sb.AppendLine(script + "\r\n---------------------------------");

            rtbSqlScript.Text = sb.ToString();
        }
        private List<string> GenerateAuditingScripts(List<TableConfig> tables, AuditScriptType type)
        {
            string triggerNameFormat = "{0} - " + AuditHelper.triggerNameFormat;
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
                    script = AuditHelper.GetAuditScript(cfg, AuditScriptType.CreateInsertTrigger, data);
                    triggerNameForHeader = String.Format(triggerNameFormat, tableName, "INSERT");
                    header = ObjectScriptHelper.ScriptHeader(data.SQLServerName, data.DatabaseName, DateTime.Now, schemaName, triggerNameForHeader, DbScriptDescription.Trigger, System.Environment.UserName, true, true, true);
                    fileName = tempFolderName + "\\" + String.Format(triggerFileName, triggerNameForHeader, schemaName);
                    createdScriptFiles.Add(fileName);
                    File.WriteAllText(fileName, header + script);
                    scripts.Add(script);

                    script = AuditHelper.GetAuditScript(cfg, AuditScriptType.CreateUpdateTrigger, data);
                    triggerNameForHeader = String.Format(triggerNameFormat, tableName, "UPDATE");
                    header = ObjectScriptHelper.ScriptHeader(data.SQLServerName, data.DatabaseName, DateTime.Now, schemaName, triggerNameForHeader, DbScriptDescription.Trigger, System.Environment.UserName, true, true, true);
                    fileName = tempFolderName + "\\" + String.Format(triggerFileName, triggerNameForHeader, schemaName);
                    createdScriptFiles.Add(fileName);
                    File.WriteAllText(fileName, header + script);
                    scripts.Add(script);

                    script = AuditHelper.GetAuditScript(cfg, AuditScriptType.CreateDeleteTrigger, data);
                    triggerNameForHeader = String.Format(triggerNameFormat, tableName, "DELETE");
                    header = ObjectScriptHelper.ScriptHeader(data.SQLServerName, data.DatabaseName, DateTime.Now, schemaName, triggerNameForHeader, DbScriptDescription.Trigger, System.Environment.UserName, true, true, true);
                    fileName = tempFolderName + "\\" + String.Format(triggerFileName, triggerNameForHeader, schemaName);
                    createdScriptFiles.Add(fileName);
                    File.WriteAllText(fileName, header + script);
                    scripts.Add(script);
                }
                else
                {
                    script = AuditHelper.GetAuditScript(cfg, type, data);
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
                    createdScriptFiles.Add(fileName);
                    File.WriteAllText(tempFolderName + "\\" + fileName, script);
                    scripts.Add(script);
                }
            }

            return scripts;
        }

        private bool ClearExistingTempDirectory()
        {
            try
            {
                if (Directory.Exists(tempFolderName))
                {
                    Directory.Delete(tempFolderName, true);
                }

                return true;
            }
            catch
            {
                return false;
            }
            finally
            {
                tempFolderName = string.Empty;
                tempBuildFileName = string.Empty;
            }
        }
        private bool SetUpForNewScripts(string suggestedProjectName)
        {
            try
            {
                tempBuildFileName = suggestedProjectName;
                tempFolderName = Path.Combine(Path.GetTempPath(), "SqlBuildManagerAudit_" + System.Guid.NewGuid().ToString());
                System.IO.Directory.CreateDirectory(tempFolderName);

                createdScriptFiles.Clear();
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
            if (lstTables.SelectedItems.Count > 0)
            {
                TableConfig table = new TableConfig(lstTables.SelectedItems[0].Text, lstTables.SelectedItems[0].Tag as SQLSyncAuditingDatabaseTableToAudit);
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
            GenerateSelectedScripts(AuditScriptType.CreateAuditTable, data.DatabaseName + " Audit Tables");
        }
        private void scriptAuditTriggersToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GenerateSelectedScripts(AuditScriptType.CreateAllTriggers, data.DatabaseName + " Audit Insert Trigger");
        }

        private void mnuSingleAuditTrigger_Click(object sender, System.EventArgs e)
        {
            if (lstTables.SelectedItems.Count > 0)
            {
                TableConfig table = new TableConfig(lstTables.SelectedItems[0].Text, lstTables.SelectedItems[0].Tag as SQLSyncAuditingDatabaseTableToAudit);
                GenerateScriptsToWindow(table, AuditScriptType.CreateAllTriggers, table.TableName + " Audit Triggers");
            }
        }

        private void mnuCompleteAudit_Click(object sender, System.EventArgs e)
        {
            ClearExistingTempDirectory();

            if (!SetUpForNewScripts(data.DatabaseName + " Audit.sbm"))
                return;

            List<string> scripts = new List<string>();

            List<TableConfig> tmp = new List<TableConfig>();
            tmp.Add(new TableConfig());
            //scripts.AddRange(GenerateAuditingScripts(tmp, AuditScriptType.MasterTable));
            List<TableConfig> tables = new List<TableConfig>();
            foreach (ListViewItem item in lstTables.CheckedItems)
            {
                tables.Add(new TableConfig(item.Text, item.Tag as SQLSyncAuditingDatabaseTableToAudit));
            }
            scripts.AddRange(GenerateAuditingScripts(tables, AuditScriptType.CreateAuditTable));
            scripts.AddRange(GenerateAuditingScripts(tables, AuditScriptType.CreateAllTriggers));
            //scripts.AddRange(GenerateAuditingScripts(tables, AuditScriptType.CreateUpdateTrigger));
            //scripts.AddRange(GenerateAuditingScripts(tables, AuditScriptType.CreateDeleteTrigger));

            StringBuilder sb = new StringBuilder();
            foreach (string script in scripts)
                sb.AppendLine(script + "\r\n---------------------------------");

            rtbSqlScript.Text = sb.ToString();

        }

        private void mnuDisableAuditTrigs_Click(object sender, System.EventArgs e)
        {
            GenerateSelectedScripts(AuditScriptType.TriggerDisable, data.DatabaseName + " Disable Audit Triggers");
        }

        private void mnuEnableAuditTrigs_Click(object sender, System.EventArgs e)
        {
            GenerateSelectedScripts(AuditScriptType.TriggerEnable, data.DatabaseName + " Enable Audit Triggers");
        }


        private void mnuCopy_Click(object sender, System.EventArgs e)
        {
            Clipboard.SetDataObject(rtbSqlScript.Text);
        }

        private void ToolStripMenuItem11_Click(object sender, System.EventArgs e)
        {
            if (DialogResult.OK == saveFileDialog1.ShowDialog())
            {
                using (StreamWriter sw = File.CreateText(saveFileDialog1.FileName))
                {
                    sw.WriteLine(rtbSqlScript.Text);
                    sw.Flush();
                    sw.Close();
                }
            }
        }

        private void SaveSqlFilesToNewBuildFile(string buildFileName, string directory)
        {
            //string[] files = Directory.GetFiles(directory);

            if (File.Exists(buildFileName))
            {
                if (SqlBuildManagerFileExport != null)
                {
                    SqlBuild.SqlBuildForm frmBuild = new SqlBuildForm(buildFileName, data);
                    frmBuild.Show();
                    frmBuild.BulkAdd(createdScriptFiles.ToArray());
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
                for (int i = 0; i < createdScriptFiles.Count; i++)
                {
                    string shortFileName = Path.GetFileName(createdScriptFiles[i]);
                    if (shortFileName == XmlFileNames.MainProjectFile ||
                        shortFileName == XmlFileNames.ExportFile)
                        continue;

                    statStatus.Text = "Adding " + shortFileName + " to Build File";

                    SqlBuildFileHelper.AddScriptFileToBuild(
                        ref buildData,
                        projFileName,
                        shortFileName,
                        i + 1,
                        "User Data Auditing",
                        true,
                        true,
                        data.DatabaseName,
                        true,
                        buildFileName,
                        false,
                        true,
                        System.Environment.UserName,
                        EnterpriseConfigHelper.GetMinumumScriptTimeout(shortFileName, Properties.Settings.Default.DefaultMinimumScriptTimeout),
                        "");

                }

                statStatus.Text = "Saving to Build File " + Path.GetFileName(buildFileName);
                SqlBuildFileHelper.SaveSqlBuildProjectFile(ref buildData, projFileName, buildFileName);
                ////Delete the temp directory

                //files = Directory.GetFiles(directory);
                //for(int i=0;i<files.Length;i++)
                //    File.Delete(files[i]);
                //Directory.Delete(directory,true);
                statStatus.Text = "Build file " + Path.GetFileName(buildFileName) + " saved.";

                OpenFileMessage(buildFileName);
            }
        }

        private void OpenFileMessage(string fileName)
        {
            if (DialogResult.Yes == MessageBox.Show("Scripting Complete. Open File?", "Open File", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
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
            data.Fill(oldConnData);
            Cursor = Cursors.WaitCursor;

            data.SQLServerName = serverName;
            if (!string.IsNullOrWhiteSpace(username) && (!string.IsNullOrWhiteSpace(password)))
            {
                data.UserId = username;
                data.Password = password;
            }
            data.AuthenticationType = authType;
            data.ScriptTimeout = 5;
            try
            {
                DbInformation.DatabaseList dbList = SqlSync.DbInformation.InfoHelper.GetDatabaseList(data);
            }
            catch
            {
                MessageBox.Show("Error retrieving database list. Is the server running?", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                data = oldConnData;
                settingsControl1.Server = oldConnData.SQLServerName;
            }


            Cursor = Cursors.Default;
        }

        private void toolStripSave_Click(object sender, EventArgs e)
        {
            if (tempFolderName.Length == 0)
            {
                MessageBox.Show("Nothing to save", "Gotta have something to save", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                return;
            }

            saveFileDialog1.FileName = tempBuildFileName;
            if (DialogResult.OK == saveFileDialog1.ShowDialog())
            {
                SaveSqlFilesToNewBuildFile(saveFileDialog1.FileName, tempFolderName);
            }
        }

        private void saveToOpenToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SqlBuildManagerFileExport != null && tempFolderName.Length > 0)
            {
                string[] files = Directory.GetFiles(tempFolderName);
                SqlBuildManagerFileExport(this, new SqlBuildManagerFileExportEventArgs(files));
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
            FileNames = fileNames;
        }
    }

}

