using SqlSync.Connection;
using SqlSync.Constants;
using SqlSync.DbInformation;
using SqlSync.MRU;
using SqlSync.SqlBuild;
using SqlSync.TableScript;
using SqlSync.TableScript.Audit;
using SqlSync.Validator;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
namespace SqlSync
{
    /// <summary>
    /// Summary description for LookUpTable.
    /// </summary>
    public class CodeTableScriptingForm : System.Windows.Forms.Form, IMRUClient
    {
        private MRUManager mruManager;
        private System.ComponentModel.IContainer components;
        ConnectionData data = null;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.StatusStrip statusBar1;
        private System.Windows.Forms.ToolStripStatusLabel statStatus;
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
        private System.Windows.Forms.ToolStrip toolBar1;
        private System.Windows.Forms.ToolStripButton tbbSave;
        private System.Windows.Forms.ToolStripButton tbbExportToBM;
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
            projectFileName = fileName;

        }
        public CodeTableScriptingForm(ConnectionData data)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            this.data = data;

        }
        public CodeTableScriptingForm(ConnectionData data, bool allowBuildManagerExport) : this(data)
        {
            this.allowBuildManagerExport = allowBuildManagerExport;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CodeTableScriptingForm));
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Included (with Triggers)", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Included (Missing Triggers)", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup3 = new System.Windows.Forms.ListViewGroup("Excluded (Have Triggers and Columns)", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup4 = new System.Windows.Forms.ListViewGroup("Candidate (Have Some Audit Columns)", System.Windows.Forms.HorizontalAlignment.Left);
            contextMenu1 = new System.Windows.Forms.ContextMenuStrip(components);
            mnuWhereClause = new System.Windows.Forms.ToolStripMenuItem();
            menuItem6 = new System.Windows.Forms.ToolStripSeparator();
            mnuScriptMissingColumns = new System.Windows.Forms.ToolStripMenuItem();
            mnuScriptTrigger = new System.Windows.Forms.ToolStripMenuItem();
            mnuScriptDefault = new System.Windows.Forms.ToolStripMenuItem();
            menuItem3 = new System.Windows.Forms.ToolStripSeparator();
            mnuAddTable = new System.Windows.Forms.ToolStripMenuItem();
            mnuDeleteTable = new System.Windows.Forms.ToolStripMenuItem();
            menuItem1 = new System.Windows.Forms.ToolStripSeparator();
            mnuCopyTables = new System.Windows.Forms.ToolStripMenuItem();
            mnuCopyTablesForsql = new System.Windows.Forms.ToolStripMenuItem();
            folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            statusBar1 = new System.Windows.Forms.StatusStrip();
            statStatus = new System.Windows.Forms.ToolStripStatusLabel();
            openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            grpScripting = new System.Windows.Forms.GroupBox();
            btnGenerateScripts = new System.Windows.Forms.Button();
            chkSelectByDate = new System.Windows.Forms.CheckBox();
            dateTimePicker1 = new System.Windows.Forms.DateTimePicker();
            tcTables = new System.Windows.Forms.TabControl();
            toolBar1 = new System.Windows.Forms.ToolStrip();
            tbbSave = new System.Windows.Forms.ToolStripButton();
            tbbExportToBM = new System.Windows.Forms.ToolStripButton();
            imageList1 = new System.Windows.Forms.ImageList(components);
            ddDatabaseList = new System.Windows.Forms.ComboBox();
            contextDatabase = new System.Windows.Forms.ContextMenuStrip(components);
            mnuRemoveDatabase = new System.Windows.Forms.ToolStripMenuItem();
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
            mnuSettings = new System.Windows.Forms.ToolStripMenuItem();
            mnuGoSeparators = new System.Windows.Forms.ToolStripMenuItem();
            mnuIncludeUpdateStatements = new System.Windows.Forms.ToolStripMenuItem();
            mnuUpdateDateAndId = new System.Windows.Forms.ToolStripMenuItem();
            menuItem7 = new System.Windows.Forms.ToolStripMenuItem();
            mnuScriptTriggers = new System.Windows.Forms.ToolStripMenuItem();
            mnuSaveUpdateTrigToBuildFile = new System.Windows.Forms.ToolStripMenuItem();
            mnuScriptTriggersOneFile = new System.Windows.Forms.ToolStripMenuItem();
            mnuScriptTriggersPerTable = new System.Windows.Forms.ToolStripMenuItem();
            mnuScriptAllMissingColumns = new System.Windows.Forms.ToolStripMenuItem();
            mnuScriptAllColumnDefaults = new System.Windows.Forms.ToolStripMenuItem();
            openBuildManager = new System.Windows.Forms.OpenFileDialog();
            panel1 = new System.Windows.Forms.Panel();
            grpDatabaseInfo = new System.Windows.Forms.GroupBox();
            groupBox1 = new System.Windows.Forms.GroupBox();
            lblNotInDb = new System.Windows.Forms.Label();
            lblPK = new System.Windows.Forms.Label();
            lblMissingCols = new System.Windows.Forms.Label();
            splitter1 = new System.Windows.Forms.Splitter();
            panel2 = new System.Windows.Forms.Panel();
            saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            saveUpdateTrigBuildFile = new System.Windows.Forms.SaveFileDialog();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            toolStripContainer1 = new System.Windows.Forms.ToolStripContainer();
            settingsControl1 = new SqlSync.SettingsControl();
            ctxCandidate = new System.Windows.Forms.ContextMenuStrip(components);
            mnuAddCandidateTable = new System.Windows.Forms.ToolStripMenuItem();
            mnuAddNewTable = new System.Windows.Forms.ToolStripMenuItem();
            bgTriggerScript = new System.ComponentModel.BackgroundWorker();
            contextMenu1.SuspendLayout();
            statusBar1.SuspendLayout();
            grpScripting.SuspendLayout();
            toolBar1.SuspendLayout();
            contextDatabase.SuspendLayout();
            mainMenu1.SuspendLayout();
            panel1.SuspendLayout();
            grpDatabaseInfo.SuspendLayout();
            groupBox1.SuspendLayout();
            panel2.SuspendLayout();
            toolStripContainer1.ContentPanel.SuspendLayout();
            toolStripContainer1.TopToolStripPanel.SuspendLayout();
            ctxCandidate.SuspendLayout();
            SuspendLayout();
            // 
            // contextMenu1
            // 
            contextMenu1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            mnuWhereClause,
            menuItem6,
            mnuScriptMissingColumns,
            mnuScriptTrigger,
            mnuScriptDefault,
            menuItem3,
            mnuAddTable,
            mnuDeleteTable,
            menuItem1,
            mnuCopyTables,
            mnuCopyTablesForsql});
            contextMenu1.Name = "contextMenu1";
            contextMenu1.Size = new System.Drawing.Size(322, 198);
            contextMenu1.Opening += new System.ComponentModel.CancelEventHandler(contextMenu1_Opening);
            // 
            // mnuWhereClause
            // 
            mnuWhereClause.MergeIndex = 0;
            mnuWhereClause.Name = "mnuWhereClause";
            mnuWhereClause.Size = new System.Drawing.Size(321, 22);
            mnuWhereClause.Text = "Add/View \"Where\" Clause";
            mnuWhereClause.Click += new System.EventHandler(mnuWhereClause_Click);
            // 
            // menuItem6
            // 
            menuItem6.MergeIndex = 1;
            menuItem6.Name = "menuItem6";
            menuItem6.Size = new System.Drawing.Size(318, 6);
            // 
            // mnuScriptMissingColumns
            // 
            mnuScriptMissingColumns.Enabled = false;
            mnuScriptMissingColumns.MergeIndex = 2;
            mnuScriptMissingColumns.Name = "mnuScriptMissingColumns";
            mnuScriptMissingColumns.Size = new System.Drawing.Size(321, 22);
            mnuScriptMissingColumns.Text = "Script For Missing Create/Update Columns";
            mnuScriptMissingColumns.Click += new System.EventHandler(mnuScriptMissingColumns_Click);
            // 
            // mnuScriptTrigger
            // 
            mnuScriptTrigger.Enabled = false;
            mnuScriptTrigger.MergeIndex = 3;
            mnuScriptTrigger.Name = "mnuScriptTrigger";
            mnuScriptTrigger.Size = new System.Drawing.Size(321, 22);
            mnuScriptTrigger.Text = "Script For Update Trigger";
            mnuScriptTrigger.Click += new System.EventHandler(mnuScriptTrigger_Click);
            // 
            // mnuScriptDefault
            // 
            mnuScriptDefault.MergeIndex = 4;
            mnuScriptDefault.Name = "mnuScriptDefault";
            mnuScriptDefault.Size = new System.Drawing.Size(321, 22);
            mnuScriptDefault.Text = "Script to Reset Create/Update Column Defaults";
            mnuScriptDefault.Click += new System.EventHandler(mnuScriptDefault_Click);
            // 
            // menuItem3
            // 
            menuItem3.MergeIndex = 5;
            menuItem3.Name = "menuItem3";
            menuItem3.Size = new System.Drawing.Size(318, 6);
            // 
            // mnuAddTable
            // 
            mnuAddTable.MergeIndex = 6;
            mnuAddTable.Name = "mnuAddTable";
            mnuAddTable.Size = new System.Drawing.Size(321, 22);
            mnuAddTable.Text = "Add New Table to List";
            mnuAddTable.Click += new System.EventHandler(mnuAddNewTable_Click);
            // 
            // mnuDeleteTable
            // 
            mnuDeleteTable.MergeIndex = 7;
            mnuDeleteTable.Name = "mnuDeleteTable";
            mnuDeleteTable.Size = new System.Drawing.Size(321, 22);
            mnuDeleteTable.Text = "Remove Selected Table";
            mnuDeleteTable.Click += new System.EventHandler(mnuDeleteTable_Click);
            // 
            // menuItem1
            // 
            menuItem1.MergeIndex = 8;
            menuItem1.Name = "menuItem1";
            menuItem1.Size = new System.Drawing.Size(318, 6);
            // 
            // mnuCopyTables
            // 
            mnuCopyTables.MergeIndex = 9;
            mnuCopyTables.Name = "mnuCopyTables";
            mnuCopyTables.Size = new System.Drawing.Size(321, 22);
            mnuCopyTables.Text = "Copy Table List";
            mnuCopyTables.Click += new System.EventHandler(mnuCopyTables_Click);
            // 
            // mnuCopyTablesForsql
            // 
            mnuCopyTablesForsql.MergeIndex = 10;
            mnuCopyTablesForsql.Name = "mnuCopyTablesForsql";
            mnuCopyTablesForsql.Size = new System.Drawing.Size(321, 22);
            mnuCopyTablesForsql.Text = "Copy Table List for Sql";
            mnuCopyTablesForsql.Click += new System.EventHandler(mnuCopyTablesForsql_Click);
            // 
            // folderBrowserDialog1
            // 
            folderBrowserDialog1.Description = "Select a folder to save your scripts";
            // 
            // statusBar1
            // 
            statusBar1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            statStatus});
            statusBar1.Location = new System.Drawing.Point(0, 648);
            statusBar1.Name = "statusBar1";
            statusBar1.Size = new System.Drawing.Size(944, 22);
            statusBar1.TabIndex = 6;
            statusBar1.Text = "statusBar1";
            // 
            // statStatus
            // 
            statStatus.Name = "statStatus";
            statStatus.Size = new System.Drawing.Size(929, 17);
            statStatus.Spring = true;
            // 
            // openFileDialog1
            // 
            openFileDialog1.CheckFileExists = false;
            openFileDialog1.DefaultExt = "xml";
            openFileDialog1.Filter = "Sql Table Script Files *.sts|*.sts|XML Files|*.xml|All Files|*.*";
            openFileDialog1.Title = "Open SQL Sync Project File";
            // 
            // grpScripting
            // 
            grpScripting.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            grpScripting.Controls.Add(btnGenerateScripts);
            grpScripting.Controls.Add(chkSelectByDate);
            grpScripting.Controls.Add(dateTimePicker1);
            grpScripting.Controls.Add(tcTables);
            grpScripting.Controls.Add(toolBar1);
            grpScripting.Enabled = false;
            grpScripting.FlatStyle = System.Windows.Forms.FlatStyle.System;
            grpScripting.Location = new System.Drawing.Point(2, 2);
            grpScripting.Name = "grpScripting";
            grpScripting.Size = new System.Drawing.Size(631, 584);
            grpScripting.TabIndex = 11;
            grpScripting.TabStop = false;
            grpScripting.Enter += new System.EventHandler(grpScripting_Enter);
            // 
            // btnGenerateScripts
            // 
            btnGenerateScripts.FlatStyle = System.Windows.Forms.FlatStyle.System;
            btnGenerateScripts.Location = new System.Drawing.Point(24, 16);
            btnGenerateScripts.Name = "btnGenerateScripts";
            btnGenerateScripts.Size = new System.Drawing.Size(128, 23);
            btnGenerateScripts.TabIndex = 21;
            btnGenerateScripts.Text = "Generate Scripts";
            btnGenerateScripts.Click += new System.EventHandler(btnGenerateScripts_Click);
            // 
            // chkSelectByDate
            // 
            chkSelectByDate.Cursor = System.Windows.Forms.Cursors.IBeam;
            chkSelectByDate.Font = new System.Drawing.Font("Verdana", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            chkSelectByDate.Location = new System.Drawing.Point(176, 17);
            chkSelectByDate.Name = "chkSelectByDate";
            chkSelectByDate.Size = new System.Drawing.Size(200, 20);
            chkSelectByDate.TabIndex = 20;
            chkSelectByDate.Text = "Select Rows Updated On or After:";
            chkSelectByDate.CheckedChanged += new System.EventHandler(chkSelectByDate_CheckedChanged);
            // 
            // dateTimePicker1
            // 
            dateTimePicker1.Cursor = System.Windows.Forms.Cursors.IBeam;
            dateTimePicker1.Enabled = false;
            dateTimePicker1.Font = new System.Drawing.Font("Verdana", 7.5F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dateTimePicker1.Format = System.Windows.Forms.DateTimePickerFormat.Short;
            dateTimePicker1.Location = new System.Drawing.Point(376, 17);
            dateTimePicker1.MaxDate = new System.DateTime(3000, 12, 31, 0, 0, 0, 0);
            dateTimePicker1.MinDate = new System.DateTime(2005, 1, 1, 0, 0, 0, 0);
            dateTimePicker1.Name = "dateTimePicker1";
            dateTimePicker1.Size = new System.Drawing.Size(112, 20);
            dateTimePicker1.TabIndex = 19;
            // 
            // tcTables
            // 
            tcTables.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            tcTables.Location = new System.Drawing.Point(8, 56);
            tcTables.Multiline = true;
            tcTables.Name = "tcTables";
            tcTables.SelectedIndex = 0;
            tcTables.Size = new System.Drawing.Size(617, 520);
            tcTables.TabIndex = 11;
            tcTables.SelectedIndexChanged += new System.EventHandler(tcTables_SelectedIndexChanged);
            // 
            // toolBar1
            // 
            toolBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            toolBar1.Dock = System.Windows.Forms.DockStyle.None;
            toolBar1.ImageList = imageList1;
            toolBar1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            tbbSave,
            tbbExportToBM});
            toolBar1.Location = new System.Drawing.Point(559, 31);
            toolBar1.Name = "toolBar1";
            toolBar1.Size = new System.Drawing.Size(58, 25);
            toolBar1.TabIndex = 22;
            toolBar1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(toolBar1_ButtonClick);
            // 
            // tbbSave
            // 
            tbbSave.ImageIndex = 6;
            tbbSave.Name = "tbbSave";
            tbbSave.Size = new System.Drawing.Size(23, 22);
            tbbSave.Tag = "SaveAll";
            tbbSave.ToolTipText = "Save to File";
            // 
            // tbbExportToBM
            // 
            tbbExportToBM.ImageIndex = 5;
            tbbExportToBM.Name = "tbbExportToBM";
            tbbExportToBM.Size = new System.Drawing.Size(23, 22);
            tbbExportToBM.Tag = "export";
            tbbExportToBM.ToolTipText = "Export All Scripts to New Sql Build Manager File";
            // 
            // imageList1
            // 
            imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            imageList1.TransparentColor = System.Drawing.Color.Transparent;
            imageList1.Images.SetKeyName(0, "");
            imageList1.Images.SetKeyName(1, "Cut-2.png");
            imageList1.Images.SetKeyName(2, "Save.png");
            imageList1.Images.SetKeyName(3, "");
            imageList1.Images.SetKeyName(4, "");
            imageList1.Images.SetKeyName(5, "");
            imageList1.Images.SetKeyName(6, "Save All.png");
            // 
            // ddDatabaseList
            // 
            ddDatabaseList.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            ddDatabaseList.ContextMenuStrip = contextDatabase;
            ddDatabaseList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            ddDatabaseList.Location = new System.Drawing.Point(12, 30);
            ddDatabaseList.Name = "ddDatabaseList";
            ddDatabaseList.Size = new System.Drawing.Size(271, 21);
            ddDatabaseList.TabIndex = 0;
            ddDatabaseList.SelectionChangeCommitted += new System.EventHandler(ddDatabaseList_SelectionChangeCommitted);
            // 
            // contextDatabase
            // 
            contextDatabase.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            mnuRemoveDatabase});
            contextDatabase.Name = "contextDatabase";
            contextDatabase.Size = new System.Drawing.Size(235, 26);
            // 
            // mnuRemoveDatabase
            // 
            mnuRemoveDatabase.MergeIndex = 0;
            mnuRemoveDatabase.Name = "mnuRemoveDatabase";
            mnuRemoveDatabase.Size = new System.Drawing.Size(234, 22);
            mnuRemoveDatabase.Text = "Remove Database (and Tables)";
            mnuRemoveDatabase.Click += new System.EventHandler(mnuRemoveDatabase_Click);
            // 
            // label3
            // 
            label3.Location = new System.Drawing.Point(12, 14);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(116, 16);
            label3.TabIndex = 15;
            label3.Text = "Select Database:";
            // 
            // lnkCheckAll
            // 
            lnkCheckAll.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            lnkCheckAll.Location = new System.Drawing.Point(116, 58);
            lnkCheckAll.Name = "lnkCheckAll";
            lnkCheckAll.Size = new System.Drawing.Size(171, 16);
            lnkCheckAll.TabIndex = 1;
            lnkCheckAll.TabStop = true;
            lnkCheckAll.Text = "Check All";
            lnkCheckAll.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(lnkCheckAll_LinkClicked);
            // 
            // label1
            // 
            label1.Location = new System.Drawing.Point(12, 58);
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
            listViewGroup1.Header = "Included (with Triggers)";
            listViewGroup1.Name = "Included";
            listViewGroup2.Header = "Included (Missing Triggers)";
            listViewGroup2.Name = "IncludedNoTrigger";
            listViewGroup3.Header = "Excluded (Have Triggers and Columns)";
            listViewGroup3.Name = "Excluded";
            listViewGroup4.Header = "Candidate (Have Some Audit Columns)";
            listViewGroup4.Name = "Candidate";
            lstTables.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2,
            listViewGroup3,
            listViewGroup4});
            lstTables.HideSelection = false;
            lstTables.Location = new System.Drawing.Point(14, 80);
            lstTables.Name = "lstTables";
            lstTables.ShowItemToolTips = true;
            lstTables.Size = new System.Drawing.Size(271, 466);
            lstTables.Sorting = System.Windows.Forms.SortOrder.Ascending;
            lstTables.TabIndex = 8;
            lstTables.UseCompatibleStateImageBehavior = false;
            lstTables.View = System.Windows.Forms.View.Details;
            lstTables.MouseUp += new System.Windows.Forms.MouseEventHandler(lstTables_MouseUp);
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Table Name";
            columnHeader1.Width = 189;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Rows";
            // 
            // mainMenu1
            // 
            mainMenu1.Dock = System.Windows.Forms.DockStyle.None;
            mainMenu1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            mnuActionMain,
            mnuSettings,
            menuItem7});
            mainMenu1.Location = new System.Drawing.Point(0, 0);
            mainMenu1.Name = "mainMenu1";
            mainMenu1.Size = new System.Drawing.Size(944, 24);
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
            mnuLoadProjectFile.Text = "&Load/New  Configuration File";
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
            // mnuSettings
            // 
            mnuSettings.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            mnuGoSeparators,
            mnuIncludeUpdateStatements,
            mnuUpdateDateAndId});
            mnuSettings.MergeIndex = 1;
            mnuSettings.Name = "mnuSettings";
            mnuSettings.Size = new System.Drawing.Size(61, 20);
            mnuSettings.Text = "Settings";
            // 
            // mnuGoSeparators
            // 
            mnuGoSeparators.Checked = true;
            mnuGoSeparators.CheckState = System.Windows.Forms.CheckState.Checked;
            mnuGoSeparators.MergeIndex = 0;
            mnuGoSeparators.Name = "mnuGoSeparators";
            mnuGoSeparators.Size = new System.Drawing.Size(309, 22);
            mnuGoSeparators.Text = "Add Batch \"GO\" Separators";
            mnuGoSeparators.Click += new System.EventHandler(SettingsItem_Click);
            // 
            // mnuIncludeUpdateStatements
            // 
            mnuIncludeUpdateStatements.Checked = true;
            mnuIncludeUpdateStatements.CheckState = System.Windows.Forms.CheckState.Checked;
            mnuIncludeUpdateStatements.MergeIndex = 1;
            mnuIncludeUpdateStatements.Name = "mnuIncludeUpdateStatements";
            mnuIncludeUpdateStatements.Size = new System.Drawing.Size(309, 22);
            mnuIncludeUpdateStatements.Text = "Include Update Statements When Applicable";
            mnuIncludeUpdateStatements.Click += new System.EventHandler(SettingsItem_Click);
            // 
            // mnuUpdateDateAndId
            // 
            mnuUpdateDateAndId.MergeIndex = 2;
            mnuUpdateDateAndId.Name = "mnuUpdateDateAndId";
            mnuUpdateDateAndId.Size = new System.Drawing.Size(309, 22);
            mnuUpdateDateAndId.Text = "Replace UpdateDate and UpdateId Values";
            mnuUpdateDateAndId.Click += new System.EventHandler(SettingsItem_Click);
            // 
            // menuItem7
            // 
            menuItem7.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            mnuScriptTriggers,
            mnuScriptAllMissingColumns,
            mnuScriptAllColumnDefaults});
            menuItem7.MergeIndex = 2;
            menuItem7.Name = "menuItem7";
            menuItem7.Size = new System.Drawing.Size(101, 20);
            menuItem7.Text = "Update Triggers";
            // 
            // mnuScriptTriggers
            // 
            mnuScriptTriggers.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            mnuSaveUpdateTrigToBuildFile,
            mnuScriptTriggersOneFile,
            mnuScriptTriggersPerTable});
            mnuScriptTriggers.MergeIndex = 0;
            mnuScriptTriggers.Name = "mnuScriptTriggers";
            mnuScriptTriggers.Size = new System.Drawing.Size(322, 22);
            mnuScriptTriggers.Text = "Script Checked For Update Column Triggers";
            // 
            // mnuSaveUpdateTrigToBuildFile
            // 
            mnuSaveUpdateTrigToBuildFile.MergeIndex = 0;
            mnuSaveUpdateTrigToBuildFile.Name = "mnuSaveUpdateTrigToBuildFile";
            mnuSaveUpdateTrigToBuildFile.Size = new System.Drawing.Size(190, 22);
            mnuSaveUpdateTrigToBuildFile.Text = "Save to New Build File";
            mnuSaveUpdateTrigToBuildFile.Click += new System.EventHandler(mnuSaveUpdateTrigToBuildFile_Click);
            // 
            // mnuScriptTriggersOneFile
            // 
            mnuScriptTriggersOneFile.MergeIndex = 1;
            mnuScriptTriggersOneFile.Name = "mnuScriptTriggersOneFile";
            mnuScriptTriggersOneFile.Size = new System.Drawing.Size(190, 22);
            mnuScriptTriggersOneFile.Text = "Single Sql File";
            mnuScriptTriggersOneFile.Click += new System.EventHandler(mnuScriptTriggersOneFile_Click);
            // 
            // mnuScriptTriggersPerTable
            // 
            mnuScriptTriggersPerTable.MergeIndex = 2;
            mnuScriptTriggersPerTable.Name = "mnuScriptTriggersPerTable";
            mnuScriptTriggersPerTable.Size = new System.Drawing.Size(190, 22);
            mnuScriptTriggersPerTable.Text = "One File Per Table";
            mnuScriptTriggersPerTable.Click += new System.EventHandler(mnuScriptTriggers_Click);
            // 
            // mnuScriptAllMissingColumns
            // 
            mnuScriptAllMissingColumns.MergeIndex = 1;
            mnuScriptAllMissingColumns.Name = "mnuScriptAllMissingColumns";
            mnuScriptAllMissingColumns.Size = new System.Drawing.Size(322, 22);
            mnuScriptAllMissingColumns.Text = "Script All For Missing Create/Update Columns";
            mnuScriptAllMissingColumns.Click += new System.EventHandler(mnuScriptAllMissingColumns_Click);
            // 
            // mnuScriptAllColumnDefaults
            // 
            mnuScriptAllColumnDefaults.MergeIndex = 2;
            mnuScriptAllColumnDefaults.Name = "mnuScriptAllColumnDefaults";
            mnuScriptAllColumnDefaults.Size = new System.Drawing.Size(322, 22);
            mnuScriptAllColumnDefaults.Text = "Script To Reset Create/Update Column Defaults";
            mnuScriptAllColumnDefaults.Click += new System.EventHandler(mnuScriptAllColumnDefaults_Click);
            // 
            // openBuildManager
            // 
            openBuildManager.CheckFileExists = false;
            openBuildManager.DefaultExt = "xml";
            openBuildManager.Filter = "Sql Build Manager Project (*.sbm)|*.sbm|Sql Build Export File (*.sbe)|*.sbe|Zip F" +
    "iles (*.zip)|*.zip|All Files|*.*";
            openBuildManager.Title = "Open SQL Sync Build Project File";
            // 
            // panel1
            // 
            panel1.Controls.Add(grpDatabaseInfo);
            panel1.Dock = System.Windows.Forms.DockStyle.Left;
            panel1.Location = new System.Drawing.Point(0, 56);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(303, 592);
            panel1.TabIndex = 13;
            // 
            // grpDatabaseInfo
            // 
            grpDatabaseInfo.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            grpDatabaseInfo.Controls.Add(groupBox1);
            grpDatabaseInfo.Controls.Add(lnkCheckAll);
            grpDatabaseInfo.Controls.Add(label3);
            grpDatabaseInfo.Controls.Add(lstTables);
            grpDatabaseInfo.Controls.Add(ddDatabaseList);
            grpDatabaseInfo.Controls.Add(label1);
            grpDatabaseInfo.Enabled = false;
            grpDatabaseInfo.FlatStyle = System.Windows.Forms.FlatStyle.System;
            grpDatabaseInfo.Location = new System.Drawing.Point(2, 2);
            grpDatabaseInfo.Name = "grpDatabaseInfo";
            grpDatabaseInfo.Size = new System.Drawing.Size(295, 584);
            grpDatabaseInfo.TabIndex = 0;
            grpDatabaseInfo.TabStop = false;
            // 
            // groupBox1
            // 
            groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            groupBox1.Controls.Add(lblNotInDb);
            groupBox1.Controls.Add(lblPK);
            groupBox1.Controls.Add(lblMissingCols);
            groupBox1.Location = new System.Drawing.Point(8, 546);
            groupBox1.Name = "groupBox1";
            groupBox1.Size = new System.Drawing.Size(260, 32);
            groupBox1.TabIndex = 16;
            groupBox1.TabStop = false;
            // 
            // lblNotInDb
            // 
            lblNotInDb.BackColor = System.Drawing.Color.White;
            lblNotInDb.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblNotInDb.ForeColor = System.Drawing.Color.LightGray;
            lblNotInDb.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            lblNotInDb.Location = new System.Drawing.Point(154, 12);
            lblNotInDb.Name = "lblNotInDb";
            lblNotInDb.Size = new System.Drawing.Size(93, 16);
            lblNotInDb.TabIndex = 6;
            lblNotInDb.Text = "Missing from DB";
            lblNotInDb.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            toolTip1.SetToolTip(lblNotInDb, "Project table is missing from Database on the current server.");
            // 
            // lblPK
            // 
            lblPK.BackColor = System.Drawing.SystemColors.Control;
            lblPK.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblPK.ForeColor = System.Drawing.Color.Red;
            lblPK.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            lblPK.Location = new System.Drawing.Point(85, 12);
            lblPK.Name = "lblPK";
            lblPK.Size = new System.Drawing.Size(69, 16);
            lblPK.TabIndex = 2;
            lblPK.Text = "Missing PK";
            lblPK.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            toolTip1.SetToolTip(lblPK, "Included Table is missing either a PK or \"WHERE\" clause to allow for specific dat" +
        "a matches");
            // 
            // lblMissingCols
            // 
            lblMissingCols.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblMissingCols.ForeColor = System.Drawing.Color.Orange;
            lblMissingCols.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            lblMissingCols.Location = new System.Drawing.Point(14, 12);
            lblMissingCols.Name = "lblMissingCols";
            lblMissingCols.Size = new System.Drawing.Size(71, 16);
            lblMissingCols.TabIndex = 1;
            lblMissingCols.Text = "Missing Cols";
            lblMissingCols.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            toolTip1.SetToolTip(lblMissingCols, "Included table is missing at least 1 audit column.\r\n(Refer to item tool tip for s" +
        "pecifics)");
            // 
            // splitter1
            // 
            splitter1.Location = new System.Drawing.Point(303, 56);
            splitter1.Name = "splitter1";
            splitter1.Size = new System.Drawing.Size(3, 592);
            splitter1.TabIndex = 14;
            splitter1.TabStop = false;
            // 
            // panel2
            // 
            panel2.Controls.Add(grpScripting);
            panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            panel2.Location = new System.Drawing.Point(306, 56);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(638, 592);
            panel2.TabIndex = 15;
            // 
            // saveFileDialog1
            // 
            saveFileDialog1.Filter = "Sql Files *.sql|*.sql|All Files *.*|*.*";
            saveFileDialog1.Title = "Select File For Full Audit";
            // 
            // saveUpdateTrigBuildFile
            // 
            saveUpdateTrigBuildFile.DefaultExt = "sbm";
            saveUpdateTrigBuildFile.Filter = "Sql Manager Build File *.sbm|*.sbm|All Files *.*|*.*";
            saveUpdateTrigBuildFile.Title = "Create New Sql Manager Build File";
            // 
            // toolStripContainer1
            // 
            // 
            // 
            // 
            toolStripContainer1.BottomToolStripPanel.Location = new System.Drawing.Point(0, 0);
            toolStripContainer1.BottomToolStripPanel.Name = "";
            toolStripContainer1.BottomToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            toolStripContainer1.BottomToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            // 
            // 
            // 
            toolStripContainer1.ContentPanel.AutoScroll = true;
            toolStripContainer1.ContentPanel.Controls.Add(panel2);
            toolStripContainer1.ContentPanel.Controls.Add(splitter1);
            toolStripContainer1.ContentPanel.Controls.Add(panel1);
            toolStripContainer1.ContentPanel.Controls.Add(statusBar1);
            toolStripContainer1.ContentPanel.Controls.Add(settingsControl1);
            toolStripContainer1.ContentPanel.Size = new System.Drawing.Size(944, 670);
            toolStripContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            // 
            // 
            // 
            toolStripContainer1.LeftToolStripPanel.Location = new System.Drawing.Point(0, 0);
            toolStripContainer1.LeftToolStripPanel.Name = "";
            toolStripContainer1.LeftToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            toolStripContainer1.LeftToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            toolStripContainer1.Location = new System.Drawing.Point(0, 0);
            toolStripContainer1.Name = "toolStripContainer1";
            // 
            // 
            // 
            toolStripContainer1.RightToolStripPanel.Location = new System.Drawing.Point(0, 0);
            toolStripContainer1.RightToolStripPanel.Name = "";
            toolStripContainer1.RightToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            toolStripContainer1.RightToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            toolStripContainer1.Size = new System.Drawing.Size(944, 670);
            toolStripContainer1.TabIndex = 16;
            toolStripContainer1.Text = "toolStripContainer1";
            // 
            // 
            // 
            toolStripContainer1.TopToolStripPanel.Controls.Add(mainMenu1);
            toolStripContainer1.TopToolStripPanel.Location = new System.Drawing.Point(0, 0);
            toolStripContainer1.TopToolStripPanel.Name = "";
            toolStripContainer1.TopToolStripPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            toolStripContainer1.TopToolStripPanel.RowMargin = new System.Windows.Forms.Padding(3, 0, 0, 0);
            toolStripContainer1.TopToolStripPanel.Size = new System.Drawing.Size(944, 24);
            // 
            // settingsControl1
            // 
            settingsControl1.BackColor = System.Drawing.Color.White;
            settingsControl1.Dock = System.Windows.Forms.DockStyle.Top;
            settingsControl1.Location = new System.Drawing.Point(0, 0);
            settingsControl1.Name = "settingsControl1";
            settingsControl1.Project = "";
            settingsControl1.ProjectLabelText = "Configuration File:";
            settingsControl1.Server = "";
            settingsControl1.Size = new System.Drawing.Size(944, 56);
            settingsControl1.TabIndex = 12;
            settingsControl1.ServerChanged += new SqlSync.ServerChangedEventHandler(settingsControl1_ServerChanged);
            // 
            // ctxCandidate
            // 
            ctxCandidate.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            mnuAddCandidateTable,
            mnuAddNewTable});
            ctxCandidate.Name = "ctxCandidate";
            ctxCandidate.Size = new System.Drawing.Size(297, 48);
            // 
            // mnuAddCandidateTable
            // 
            mnuAddCandidateTable.MergeIndex = 11;
            mnuAddCandidateTable.Name = "mnuAddCandidateTable";
            mnuAddCandidateTable.Size = new System.Drawing.Size(296, 22);
            mnuAddCandidateTable.Text = "Add Candiate/Excluded Table(s) to Project";
            mnuAddCandidateTable.Click += new System.EventHandler(mnuAddCandidateTable_Click);
            // 
            // mnuAddNewTable
            // 
            mnuAddNewTable.MergeIndex = 6;
            mnuAddNewTable.Name = "mnuAddNewTable";
            mnuAddNewTable.Size = new System.Drawing.Size(296, 22);
            mnuAddNewTable.Text = "Add New Table to List";
            mnuAddNewTable.Click += new System.EventHandler(mnuAddNewTable_Click);
            // 
            // bgTriggerScript
            // 
            bgTriggerScript.WorkerReportsProgress = true;
            bgTriggerScript.DoWork += new System.ComponentModel.DoWorkEventHandler(bgTriggerScript_DoWork);
            bgTriggerScript.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(bgTriggerScript_ProgressChanged);
            bgTriggerScript.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bgTriggerScript_RunWorkerCompleted);
            // 
            // CodeTableScriptingForm
            // 
            AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            ClientSize = new System.Drawing.Size(944, 670);
            Controls.Add(toolStripContainer1);
            Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            MainMenuStrip = mainMenu1;
            Name = "CodeTableScriptingForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Sql Build Manager ::  Code Table Scripting and Auditing";
            Load += new System.EventHandler(LookUpTable_Load);
            contextMenu1.ResumeLayout(false);
            statusBar1.ResumeLayout(false);
            statusBar1.PerformLayout();
            grpScripting.ResumeLayout(false);
            grpScripting.PerformLayout();
            toolBar1.ResumeLayout(false);
            toolBar1.PerformLayout();
            contextDatabase.ResumeLayout(false);
            mainMenu1.ResumeLayout(false);
            mainMenu1.PerformLayout();
            panel1.ResumeLayout(false);
            grpDatabaseInfo.ResumeLayout(false);
            groupBox1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            toolStripContainer1.ContentPanel.ResumeLayout(false);
            toolStripContainer1.ContentPanel.PerformLayout();
            toolStripContainer1.TopToolStripPanel.ResumeLayout(false);
            toolStripContainer1.TopToolStripPanel.PerformLayout();
            ctxCandidate.ResumeLayout(false);
            ResumeLayout(false);

        }
        #endregion

        private void LookUpTable_Load(object sender, System.EventArgs e)
        {
            if (data == null)
            {
                ConnectionForm frmConnect = new ConnectionForm("Sql Table Script");
                DialogResult result = frmConnect.ShowDialog();
                if (result == DialogResult.OK)
                {
                    data = frmConnect.SqlConnection;
                }
                else
                {
                    MessageBox.Show("Sql Table Script can not continue without a valid Sql Connection", "Unable to Load", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    Close();
                }

            }

            mruManager = new MRUManager();
            mruManager.Initialize(
                this,                              // owner form
                mnuActionMain,
                mnuFileMRU,                        // Recent Files menu item
                @"Software\Michael McKechney\Sql Sync\Sql Table Scripting"); // Registry path to keep MRU list
            mruManager.MaxDisplayNameLength = 40;

            dateTimePicker1.Value = DateTime.Now.AddDays(-30);
            settingsControl1.Server = data.SQLServerName;
            if (projectFileName.Length > 0)
                LoadLookUpTableData(projectFileName, true);

            if (allowBuildManagerExport)
                tbbExportToBM.ToolTipText = "Export All Scripts to current Sql Build Manager File";


        }

        private void LoadLookUpTableData(string fileName, bool validateSchema)
        {
            bool successfulLoad = true;
            Cursor = Cursors.WaitCursor;
            try
            {
                string configFile = fileName;
                tableList = new SqlSync.TableScript.SQLSyncData();
                if (File.Exists(configFile))
                {
                    //Read the table list
                    try
                    {
                        bool isValid = true;
                        if (validateSchema)
                        {
                            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
                            SchemaValidator validator = new SchemaValidator();
                            isValid = validator.ValidateAgainstSchema(fileName,
                                Path.Combine(path, SchemaEnums.LookUpTableProjectXsd),
                                SchemaEnums.LookUpTableProjectNamespace);
                        }

                        if (isValid)
                        {
                            tableList.ReadXml(configFile);
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
                    tableList.Database.AddDatabaseRow(data.DatabaseName);
                    tableList.LookUpTable.AddLookUpTableRow("", "", false, "", (SqlSync.TableScript.SQLSyncData.DatabaseRow)tableList.Database.Rows[0]);
                    tableList.WriteXml(configFile);
                }
                if (successfulLoad)
                {
                    projectFileName = configFile;
                    settingsControl1.Project = configFile;
                    grpScripting.Enabled = true;
                    grpDatabaseInfo.Enabled = true;
                    mruManager.Add(configFile);
                    PopulateDatabaseList(tableList.Database);
                }
                else
                {
                    MessageBox.Show("Unable to Read the selected file.\r\nIt is not a valid table scripting file", "Invalid File", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }
        private void PopulateDatabaseList(SqlSync.TableScript.SQLSyncData.DatabaseDataTable table)
        {
            string currentSelection = string.Empty;
            if (ddDatabaseList.Items.Count > 0 && ddDatabaseList.SelectedItem != null &&
                ddDatabaseList.SelectedItem.ToString() != DropDownConstants.AddNew)
            {
                currentSelection = ddDatabaseList.SelectedItem.ToString();
            }
            ddDatabaseList.Items.Clear();
            lstTables.Items.Clear();
            DataView view = table.DefaultView;
            view.Sort = table.NameColumn.ColumnName + " ASC";
            ddDatabaseList.Items.Add(DropDownConstants.SelectOne);
            ddDatabaseList.SelectedIndex = 0;
            for (int i = 0; i < view.Count; i++)
            {
                ddDatabaseList.Items.Add(view[i].Row[table.NameColumn.ColumnName].ToString());
                if (currentSelection.Length > 0 &&
                    currentSelection == view[i].Row[table.NameColumn.ColumnName].ToString())
                {
                    ddDatabaseList.SelectedIndex = i + 1;
                }
            }
            //If there's only 1 database, select it.
            if (view.Count == 1)
                ddDatabaseList.SelectedIndex = 1;

            ddDatabaseList.Items.Add(DropDownConstants.AddNew);

            if (ddDatabaseList.SelectedIndex > 0)
            {
                PopulateTableList(ddDatabaseList.SelectedItem.ToString());
            }

        }
        private void CombineTableData(ref SortedDictionary<string, CodeTableAudit> auditTables, SqlSync.DbInformation.TableSize[] tables, UpdateAutoDetectData[] updateDetect)
        {
            CodeTableAudit aud;
            for (int i = 0; i < tables.Length; i++)
                if (auditTables.TryGetValue(tables[i].TableName, out aud))
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
                    auditTables.Add(updateDetect[i].TableName, aud);
                }
            }
        }
        private void PopulateTableList(string databaseName)
        {
            statStatus.Text = "Retrieving Table List and Row Count";
            lstTables.Items.Clear();
            data.DatabaseName = databaseName;
            PopulateHelper helper = new PopulateHelper(data, null);

            //Pull the row for the database config, if available
            DataRow[] dbRows = tableList.Database.Select(tableList.Database.NameColumn.ColumnName + " = '" + data.DatabaseName + "'");

            //Get table size data for all the tables in the DB. 
            SqlSync.DbInformation.TableSize[] tableSizeList = SqlSync.DbInformation.InfoHelper.GetDatabaseTableListWithRowCount(data);

            //Auto detect audit tables by virtue of the auditing triggers
            UpdateAutoDetectData[] updateDetect = SqlSync.TableScript.PopulateHelper.AutoDetectUpdateTriggers(data, tableSizeList);

            //Get tables with potential audit columns
            System.Collections.Generic.SortedDictionary<string, CodeTableAudit> auditTables = SqlSync.DbInformation.InfoHelper.GetTablesWithAuditColumns(data);

            //Combine all of the data into one object for easier handling.
            CombineTableData(ref auditTables, tableSizeList, updateDetect);

            //Create a new table object to store the rows. 
            SqlSync.TableScript.SQLSyncData.LookUpTableDataTable lookTable = null;
            //int rowCount = -1;
            //If the current database has been configed, get tables defined in the config file.
            if (dbRows.Length > 0)
            {
                SqlSync.TableScript.SQLSyncData.LookUpTableRow[] lookUpRows = ((SQLSyncData.DatabaseRow)dbRows[0]).GetLookUpTableRows();
                //Add the rows to a table so we can sort by Name
                lookTable = new SqlSync.TableScript.SQLSyncData.LookUpTableDataTable();
                CodeTableAudit aud;
                for (int i = 0; i < lookUpRows.Length; i++)
                {
                    lookTable.ImportRow(lookUpRows[i]);
                    if (auditTables.TryGetValue(lookUpRows[i].Name, out aud))
                        aud.LookUpTableRow = lookUpRows[i];
                    else
                    {
                        aud = new CodeTableAudit();
                        aud.TableName = lookUpRows[i].Name;
                        aud.LookUpTableRow = lookUpRows[i];
                        auditTables.Add(lookUpRows[i].Name, aud);
                    }
                }
            }
            foreach (KeyValuePair<string, CodeTableAudit> auditTable in auditTables)
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
                item = new ListViewItem(new string[] { auditTable.Value.TableName, auditTable.Value.RowCount.ToString() });
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
                else if (auditTable.Value.HasUpdateTrigger)
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
            statStatus.Text = "Ready.";
        }
        private void AddNewDatabase()
        {
            string[] listedDatabases = new string[ddDatabaseList.Items.Count - 2];
            for (int i = 1; i < ddDatabaseList.Items.Count - 1; i++)
            {
                listedDatabases[i - 1] = ddDatabaseList.Items[i].ToString();
            }

            NewLookUpDatabaseForm frmNewDb = new NewLookUpDatabaseForm(new PopulateHelper(data, null).RemainingDatabases(listedDatabases));
            DialogResult result = frmNewDb.ShowDialog();
            if (result == DialogResult.OK)
            {
                string[] newDatabases = frmNewDb.DatabaseList;
                SaveNewDatabaseNames(newDatabases);
            }
        }
        private void SaveNewDatabaseNames(string[] newDatabases)
        {
            for (int i = 0; i < newDatabases.Length; i++)
            {
                tableList.Database.AddDatabaseRow(newDatabases[i]);
            }
            SaveXmlTemplate(projectFileName);
            LoadLookUpTableData(projectFileName, false);
        }

        private void SaveXmlTemplate(string fileName)
        {

            try
            {
                tableList.WriteXml(fileName);
            }
            catch (System.UnauthorizedAccessException)
            {
                MessageBox.Show("Unable to save project file to:\r\n" + fileName, "Unable to Save", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RemoveDatabase(string databaseName)
        {
            DataRow[] deleteRows = tableList.Database.Select(tableList.Database.NameColumn.ColumnName + "='" + databaseName + "'");
            if (deleteRows.Length > 0)
            {
                SQLSyncData.DatabaseRow dbRow = (SQLSyncData.DatabaseRow)deleteRows[0];
                int childRowsCount = dbRow.GetLookUpTableRows().Length;
                if (childRowsCount > 0)
                {
                    string entry = (childRowsCount > 1) ? "entries" : "entry";
                    string message = "The Database " + dbRow.Name + " has " + childRowsCount.ToString() + " table " + entry + ".\r\nAre you sure you want to remove it?";
                    DialogResult result = MessageBox.Show(message, "Confirm Removal", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.No) return;
                }

                tableList.Database.RemoveDatabaseRow(dbRow);
                tableList.Database.AcceptChanges();
                SaveXmlTemplate(projectFileName);
                LoadLookUpTableData(projectFileName, false);
            }
        }
        private void RemoveTable(string tableName)
        {
            DataRow[] deleteRows = tableList.LookUpTable.Select(tableList.LookUpTable.NameColumn.ColumnName + "='" + tableName + "'");
            if (deleteRows.Length > 0)
            {
                SQLSyncData.LookUpTableRow tblRow = (SQLSyncData.LookUpTableRow)deleteRows[0];
                if (tblRow.WhereClause.Length > 0)
                {
                    string message = "The table " + tblRow.Name + " has a WHERE clause.\r\nAre you sure you want to remove it?";
                    DialogResult result = MessageBox.Show(message, "Confirm Removal", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                    if (result == DialogResult.No) return;
                }

                tableList.LookUpTable.RemoveLookUpTableRow(tblRow);
                tableList.LookUpTable.AcceptChanges();
                SaveXmlTemplate(projectFileName);
                LoadLookUpTableData(projectFileName, false);

            }
        }
        private void AddNewTables(List<string> tables)
        {
            DataRow[] rows = tableList.Database.Select(tableList.Database.NameColumn.ColumnName + " ='" + data.DatabaseName + "'");
            if (rows.Length == 0)
            {

                tableList.Database.AddDatabaseRow(data.DatabaseName);
                rows = tableList.Database.Select(tableList.Database.NameColumn.ColumnName + " ='" + data.DatabaseName + "'");
            }
            for (int i = 0; i < tables.Count; i++)
            {
                string[] pks = SqlSync.DbInformation.InfoHelper.GetPrimaryKeyColumns(tables[i], data);
                string pklist = String.Join(",", pks);
                tableList.LookUpTable.AddLookUpTableRow(tables[i], "", false, pklist, (SQLSyncData.DatabaseRow)rows[0]);
            }
            SaveXmlTemplate(projectFileName);
            LoadLookUpTableData(projectFileName, false);
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

        private void lnkGenerateScripts_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {

        }

        private void lnkSaveScripts_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            DialogResult result = folderBrowserDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                Cursor = Cursors.WaitCursor;
                try
                {
                    statStatus.Text = "Saving Scripts";
                    Cursor = Cursors.WaitCursor;

                    foreach (TabPage page in tcTables.TabPages)
                    {
                        foreach (Control ctrl in page.Controls)
                        {
                            if (ctrl.GetType() == typeof(PopulateScriptDisplay))
                            {

                                statStatus.Text = "Saving:" + ((PopulateScriptDisplay)ctrl).ScriptName;
                                ((PopulateScriptDisplay)ctrl).SaveScript(folderBrowserDialog1.SelectedPath);
                                break;
                            }
                        }
                    }
                }
                finally
                {
                    Cursor = Cursors.WaitCursor;
                }

            }
            statStatus.Text = "Save complete. Ready.";

            Cursor = Cursors.Default;
        }


        private void mnuDeleteTable_Click(object sender, System.EventArgs e)
        {
            if (lstTables.SelectedItems.Count > 0)
            {
                ListViewItem item = lstTables.SelectedItems[0];
                RemoveTable(item.Text);
            }
        }

        private void mnuWhereClause_Click(object sender, System.EventArgs e)
        {
            if (lstTables.SelectedItems.Count > 0)
            {
                ListViewItem item = lstTables.SelectedItems[0];
                SQLSyncData.LookUpTableRow lookUpRow;
                DataRow[] rows = tableList.Database.Select(tableList.Database.NameColumn.ColumnName + " ='" + data.DatabaseName + "'");
                if (rows.Length > 0)
                {
                    DataRow[] lookUpRows = ((SQLSyncData.DatabaseRow)rows[0]).GetLookUpTableRows();
                    for (int i = 0; i < lookUpRows.Length; i++)
                    {
                        SQLSyncData.DatabaseRow dbRow = (SQLSyncData.DatabaseRow)rows[0];
                        if (((SQLSyncData.LookUpTableRow)lookUpRows[i]).Name.ToUpper() == item.Text.ToUpper())
                        {
                            lookUpRow = (SQLSyncData.LookUpTableRow)lookUpRows[i];
                            string[] updateKeycols = lookUpRow.CheckKeyColumns.Split(',');
                            WhereClauseForm frmWhere = new WhereClauseForm(data, item.Text, lookUpRow.WhereClause, lookUpRow.UseAsFullSelect, updateKeycols);
                            DialogResult result = frmWhere.ShowDialog();
                            if (result == DialogResult.OK)
                            {
                                lookUpRow.WhereClause = frmWhere.WhereClause;
                                lookUpRow.UseAsFullSelect = frmWhere.UseAsFullSelect;
                                lookUpRow.CheckKeyColumns = String.Join(",", frmWhere.CheckKeyColumns);
                                lookUpRow.AcceptChanges();
                                SaveXmlTemplate(projectFileName);
                                break;
                            }
                        }
                    }
                }
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

        private void tcTables_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            Size = new System.Drawing.Size(Width + 1, Height);
            Size = new System.Drawing.Size(Width - 1, Height);

        }



        private void mnuLoadProjectFile_Click(object sender, System.EventArgs e)
        {
            DialogResult result = openFileDialog1.ShowDialog();
            if (result == DialogResult.OK)
            {
                if (File.Exists(openFileDialog1.FileName) == false)
                {
                    tableList = new SQLSyncData();
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

        private void SaveSqlFilesToNewBuildFile(string buildFileName, string directory)
        {
            string[] files = Directory.GetFiles(directory);

            if (File.Exists(buildFileName))
            {
                SqlBuild.SqlBuildForm frmBuild = new SqlBuildForm(buildFileName, data);
                frmBuild.Show();
                frmBuild.BulkAdd(files);
            }
            else
            {

                SqlBuildFileHelper.SaveSqlFilesToNewBuildFile(buildFileName, files.ToList(), data.DatabaseName, SqlSync.Properties.Settings.Default.DefaultMinimumScriptTimeout);

                files = Directory.GetFiles(directory);
                for (int i = 0; i < files.Length; i++)
                    File.Delete(files[i]);
                Directory.Delete(directory, true);

            }
        }
        private void SaveToNewBuildFile()
        {
            DialogResult result = openBuildManager.ShowDialog();
            if (result == DialogResult.OK)
            {
                string buildFileName = openBuildManager.FileName;
                string directory = Path.Combine(Path.GetDirectoryName(openBuildManager.FileName), "~sts");
                Directory.CreateDirectory(directory);

                foreach (TabPage page in tcTables.TabPages)
                {
                    foreach (Control ctrl in page.Controls)
                    {
                        if (ctrl.GetType() == typeof(PopulateScriptDisplay))
                        {

                            statStatus.Text = "Saving:" + ((PopulateScriptDisplay)ctrl).ScriptName;
                            ((PopulateScriptDisplay)ctrl).SaveScript(directory);
                            break;
                        }
                    }
                }
                SaveSqlFilesToNewBuildFile(buildFileName, directory);


            }
        }


        private void ExportToAttachedBuildFile()
        {
            if (SqlBuildManagerFileExport != null)
            {
                ArrayList lst = new ArrayList();
                string name;
                string path = Path.GetTempPath();
                foreach (TabPage page in tcTables.TabPages)
                {
                    foreach (Control ctrl in page.Controls)
                    {
                        if (ctrl.GetType() == typeof(PopulateScriptDisplay))
                        {

                            statStatus.Text = "Saving:" + ((PopulateScriptDisplay)ctrl).ScriptName;
                            name = ((PopulateScriptDisplay)ctrl).SaveScript(path);
                            lst.Add(name);
                            break;
                        }
                    }
                }
                string[] fileList = new string[lst.Count];
                lst.CopyTo(fileList);
                SqlBuildManagerFileExport(this, new SqlBuildManagerFileExportEventArgs(fileList));
            }


        }

        private void mnuScriptDefault_Click(object sender, System.EventArgs e)
        {

            CodeTableAudit codeTable = (CodeTableAudit)lstTables.SelectedItems[0].Tag;
            string script = GetDefaultReset(codeTable);
            ScriptDisplayForm frmDisplay = new ScriptDisplayForm(script, data.SQLServerName, "Default Name Resets for " + codeTable.TableName);
            frmDisplay.ShowDialog();

        }
        private string GetDefaultReset(CodeTableAudit codeTable)
        {
            return PopulateHelper.ScriptColumnDefaultsReset(codeTable);
        }
        private void mnuScriptAllColumnDefaults_Click(object sender, System.EventArgs e)
        {
            if (DialogResult.OK == folderBrowserDialog1.ShowDialog())
            {
                string script;
                string folder = folderBrowserDialog1.SelectedPath;
                for (int i = 0; i < lstTables.Items.Count; i++)
                {
                    CodeTableAudit codeTable = (CodeTableAudit)lstTables.Items[i].Tag;
                    script = GetDefaultReset(codeTable);
                    if (script.Length == 0)
                        continue;

                    statStatus.Text = "Creating default reset scripts for " + codeTable.TableName;
                    using (StreamWriter sw = File.CreateText(Path.Combine(folder, codeTable.TableName + " - Default Reset.sql")))
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
            CodeTableAudit codeTable = (CodeTableAudit)lstTables.SelectedItems[0].Tag;
            string script = GetMissingScript(codeTable);
            ScriptDisplayForm frmDisplay = new ScriptDisplayForm(script, data.SQLServerName, "Missing Audit Columns for " + codeTable.TableName);
            frmDisplay.ShowDialog();

        }
        private string GetMissingScript(CodeTableAudit codeTable)
        {
            return PopulateHelper.ScriptForMissingColumns(codeTable);
        }
        private void mnuScriptAllMissingColumns_Click(object sender, System.EventArgs e)
        {
            if (DialogResult.OK == folderBrowserDialog1.ShowDialog())
            {
                string script;
                string folder = folderBrowserDialog1.SelectedPath;
                for (int i = 0; i < lstTables.Items.Count; i++)
                {
                    CodeTableAudit codeTable = (CodeTableAudit)lstTables.Items[i].Tag;
                    script = GetMissingScript(codeTable);
                    if (script.Length == 0)
                        continue;

                    statStatus.Text = "Creating alter table scripts for " + codeTable.TableName;
                    using (StreamWriter sw = File.CreateText(Path.Combine(folder, codeTable.TableName + " - Update Columns.sql")))
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
            if (chkSelectByDate.Checked)
                dateTimePicker1.Enabled = true;
            else
                dateTimePicker1.Enabled = false;
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

        private void btnGenerateScripts_Click(object sender, System.EventArgs e)
        {
            Cursor = Cursors.WaitCursor;
            bool alertNonIncuded = false;
            statStatus.Text = "Retrieving Data; Generating Scripts";
            try
            {
                //Clear out any old controls
                tcTables.Controls.Clear();

                //Get the list of selected tables
                StringBuilder sb = new StringBuilder();
                for (int i = 0; i < lstTables.Items.Count; i++)
                {
                    if (lstTables.Items[i].Checked && lstTables.Items[i].ForeColor != lblNotInDb.ForeColor)
                    {
                        if (lstTables.Items[i].Group.Name == "Candidate")
                            alertNonIncuded = true;

                        sb.Append("'" + lstTables.Items[i].Text + "'");
                        if (i < lstTables.Items.Count - 1) sb.Append(",");
                    }
                }

                if (alertNonIncuded)
                    MessageBox.Show("Note: The Checked candidate tables will not be scripted.\r\nPlease add them to the configuration file before generating scripts", "Candidate Tables Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                if (sb.Length == 0)
                {
                    MessageBox.Show("Note: Please check at least one table from the list on the left.", "No Tables Selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                //Get their "ExistsExcludeColumns" if any
                DataRow[] rows = tableList.LookUpTable.Select(tableList.LookUpTable.NameColumn.ColumnName + " IN (" + sb.ToString() + ")");
                DataRow parentRow = tableList.Database.Select(tableList.Database.NameColumn.ColumnName + " ='" + data.DatabaseName + "'")[0];
                ArrayList ruleList = new ArrayList();
                for (int i = 0; i < rows.Length; i++)
                {
                    SqlSync.TableScript.SQLSyncData.LookUpTableRow current = (SqlSync.TableScript.SQLSyncData.LookUpTableRow)rows[i];
                    if (current.DatabaseRow.Equals(parentRow))
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
                PopulateHelper helper = new PopulateHelper(data, rules);
                helper.SyncData = tableList;
                helper.ReplaceDateAndId = mnuUpdateDateAndId.Checked;
                helper.IncludeUpdates = mnuIncludeUpdateStatements.Checked;
                helper.AddBatchGoSeparators = mnuGoSeparators.Checked;

                if (chkSelectByDate.Checked)
                    helper.SelectByUpdateDate = dateTimePicker1.Value;

                TableScriptData[] scriptedTables = helper.GeneratePopulateScripts();

                //Add a tab page per generated script
                statStatus.Text = "Generating script displays.";
                for (int i = 0; i < scriptedTables.Length; i++)
                {
                    TabPage page = new TabPage();
                    page.BorderStyle = BorderStyle.Fixed3D;
                    page.Text = scriptedTables[i].TableName;
                    PopulateScriptDisplay disp = new PopulateScriptDisplay(scriptedTables[i], allowBuildManagerExport);
                    disp.SqlBuildManagerFileExport += new SqlBuildManagerFileExportHandler(disp_SqlBuildManagerFileExport);
                    disp.ScriptText = scriptedTables[i].InsertScript;
                    disp.ScriptName = scriptedTables[i].TableName;
                    disp.ScriptDataTable = scriptedTables[i].ValuesTable;
                    disp.Size = page.Size;
                    disp.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
                    page.Controls.Add(disp);
                    tcTables.Controls.Add(page);

                }
                Size = new System.Drawing.Size(Width + 1, Height);
            }
            finally
            {
                Cursor = Cursors.Default;
                statStatus.Text = "Ready.";

            }
        }

        private void grpScripting_Enter(object sender, System.EventArgs e)
        {

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
                    LoadLookUpTableData(projectFileName, false);
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

        private void toolBar1_ButtonClick(object sender, System.Windows.Forms.ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Tag.ToString().ToLower())
            {
                case "saveall":
                    DialogResult result = folderBrowserDialog1.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        Cursor = Cursors.WaitCursor;
                        try
                        {
                            statStatus.Text = "Saving Scripts";
                            Cursor = Cursors.WaitCursor;

                            foreach (TabPage page in tcTables.TabPages)
                            {
                                foreach (Control ctrl in page.Controls)
                                {
                                    if (ctrl.GetType() == typeof(PopulateScriptDisplay))
                                    {

                                        statStatus.Text = "Saving:" + ((PopulateScriptDisplay)ctrl).ScriptName;
                                        ((PopulateScriptDisplay)ctrl).SaveScript(folderBrowserDialog1.SelectedPath);
                                        break;
                                    }
                                }
                            }
                        }
                        finally
                        {
                            Cursor = Cursors.WaitCursor;
                        }

                    }
                    statStatus.Text = "Save complete. Ready.";

                    Cursor = Cursors.Default;
                    break;
                case "export":
                    if (allowBuildManagerExport)
                        ExportToAttachedBuildFile();
                    else
                        SaveToNewBuildFile();
                    break;
            }
        }

        private void mnuAuditTable_Click(object sender, System.EventArgs e)
        {
            if (lstTables.SelectedItems.Count > 0)
            {
                TableConfig table = new TableConfig(lstTables.SelectedItems[0].Text, lstTables.SelectedItems[0].Tag as SQLSyncAuditingDatabaseTableToAudit);
                string script = AuditHelper.GetAuditScript(table, AuditScriptType.CreateAuditTable, data);
                if (DialogResult.Yes == MessageBox.Show(script + "\r\nCopy to Clipboard?", "Copy?", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                    Clipboard.SetDataObject(script, true);
            }
        }

        private void mnuMasterTrx_Click(object sender, System.EventArgs e)
        {

        }

        private void mnuScriptAllAudit_Click(object sender, System.EventArgs e)
        {
            if (DialogResult.OK == folderBrowserDialog1.ShowDialog())
            {
                string script;
                TableConfig table;
                string folder = folderBrowserDialog1.SelectedPath;
                for (int i = 0; i < lstTables.Items.Count; i++)
                {
                    table = new TableConfig(lstTables.Items[i].Text, lstTables.Items[i].Tag as SQLSyncAuditingDatabaseTableToAudit);
                    script = SqlSync.TableScript.Audit.AuditHelper.GetAuditScript(table, AuditScriptType.CreateAuditTable, data);
                    if (script.Length == 0)
                        continue;

                    statStatus.Text = "Creating audit table scripts for " + table;
                    using (StreamWriter sw = File.CreateText(Path.Combine(folder, table + " - Audit Table.sql")))
                    {
                        sw.WriteLine(script);
                        sw.Flush();
                        sw.Close();
                    }
                }
                statStatus.Text = "Complete";
            }
        }



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
            if (lstTables.SelectedItems.Count == 0)
                return;

            List<String> tables = new List<String>();
            for (int i = 0; i < lstTables.SelectedItems.Count; i++)
                tables.Add(lstTables.SelectedItems[i].Text);

            AddNewTables(tables);
        }

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
                DatabaseList dbList = SqlSync.DbInformation.InfoHelper.GetDatabaseList(data);
                //this.LookUpTable_Load(null, EventArgs.Empty);
            }
            catch
            {
                MessageBox.Show("Error retrieving database list. Is the server running?", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                data = oldConnData;
                settingsControl1.Server = oldConnData.SQLServerName;
            }


            Cursor = Cursors.Default;
        }


        private void mnuAddNewTable_Click(object sender, EventArgs e)
        {
            List<string> listedTables = new List<string>();
            for (int i = 0; i < lstTables.Items.Count; i++)
                listedTables.Add(lstTables.Items[i].Text);

            NewLookUpForm frmNew = new NewLookUpForm(new PopulateHelper(data, null).RemainingTables(listedTables));
            DialogResult result = frmNew.ShowDialog();

            if (result == DialogResult.OK)
                AddNewTables(frmNew.TableList);
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
            if (lstTables.SelectedItems.Count > 0)
            {
                CodeTableAudit codeTable = (CodeTableAudit)lstTables.SelectedItems[0].Tag;
                if (codeTable.UpdateDateColumn.Length == 0 || codeTable.UpdateIdColumn.Length == 0 ||
                    codeTable.CreateDateColumn.Length == 0 || codeTable.CreateIdColumn.Length == 0)
                    mnuScriptMissingColumns.Enabled = true;
                else
                    mnuScriptMissingColumns.Enabled = false;

                if (codeTable.HasUpdateTrigger == false)
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
                for (int i = 0; i < lstTables.CheckedItems.Count; i++)
                    codeTables.Add((CodeTableAudit)lstTables.CheckedItems[i].Tag);

                TriggerScriptData args = new TriggerScriptData(TriggerScriptType.SingleFile, file, codeTables);

                Cursor = Cursors.AppStarting;
                bgTriggerScript.RunWorkerAsync(args);
            }
        }
        private void mnuScriptTrigger_Click(object sender, System.EventArgs e)
        {
            if (lstTables.SelectedItems.Count > 0)
            {
                CodeTableAudit codeTable = (CodeTableAudit)lstTables.SelectedItems[0].Tag;
                string script = PopulateHelper.ScriptForUpdateTrigger(codeTable, data);
                ScriptDisplayForm frmDisplay = new ScriptDisplayForm(script, data.SQLServerName, "Audit Trigger for " + codeTable.TableName);
                frmDisplay.ShowDialog();
            }
        }
        private void mnuScriptTriggers_Click(object sender, System.EventArgs e)
        {
            if (DialogResult.OK == folderBrowserDialog1.ShowDialog())
            {
                string folder = folderBrowserDialog1.SelectedPath;

                List<CodeTableAudit> codeTables = new List<CodeTableAudit>();
                for (int i = 0; i < lstTables.CheckedItems.Count; i++)
                    codeTables.Add((CodeTableAudit)lstTables.CheckedItems[i].Tag);

                TriggerScriptData args = new TriggerScriptData(TriggerScriptType.FilePerScript, folder, codeTables);

                Cursor = Cursors.AppStarting;
                bgTriggerScript.RunWorkerAsync(args);


            }
        }
        private void mnuSaveUpdateTrigToBuildFile_Click(object sender, System.EventArgs e)
        {

            if (DialogResult.OK == saveUpdateTrigBuildFile.ShowDialog())
            {
                List<CodeTableAudit> codeTables = new List<CodeTableAudit>();
                for (int i = 0; i < lstTables.CheckedItems.Count; i++)
                    codeTables.Add((CodeTableAudit)lstTables.CheckedItems[i].Tag);

                TriggerScriptData args = new TriggerScriptData(TriggerScriptType.NewSqlBuildFile, saveUpdateTrigBuildFile.FileName, codeTables);

                Cursor = Cursors.AppStarting;
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
            string folder = Path.Combine(Path.GetTempPath(), System.Guid.NewGuid().ToString());
            if (args.ScriptType == TriggerScriptType.NewSqlBuildFile)
                Directory.CreateDirectory(folder);
            string fileName;
            string script;
            for (int i = 0; i < codeTables.Count; i++)
            {

                bg.ReportProgress(10, "Creating trigger scripts for " + codeTables[i].TableName);
                script = PopulateHelper.ScriptForUpdateTrigger(codeTables[i], data);
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
                            fileName = Path.Combine(folder, codeTables[i].TableName + " - Update Trigger.sql");
                        else
                            fileName = Path.Combine(destination, codeTables[i].TableName + " - Update Trigger.sql");

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
                        bg.ReportProgress(10, "Build file saved to " + destination);

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
            Cursor = Cursors.Default;
            string destination = e.Result.ToString();
            OpenResultMessage(destination, true);
        }
        #endregion

        class TriggerScriptData
        {
            public TriggerScriptType ScriptType;
            public string Destination;
            public List<CodeTableAudit> CodeTables;
            public TriggerScriptData(TriggerScriptType scriptType, string destination, List<CodeTableAudit> codeTables)
            {
                CodeTables = codeTables;
                Destination = destination;
                ScriptType = scriptType;
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
            FileNames = fileNames;
        }
    }

}

