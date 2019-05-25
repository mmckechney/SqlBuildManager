namespace SqlSync.Test
{
    partial class SprocTestConfigForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SprocTestConfigForm));
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.ctxTestCase = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuAddNewTestCase = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteTestCaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuAddNewStoredProcedure = new System.Windows.Forms.ToolStripMenuItem();
            this.deleteStoredProcedureToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewStoredProcedureScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.addNewFromExecutionScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openNewTestConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.importTestCasesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.setTargetDatabaseToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.bulkAddFromExecutionScriptsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuFileMRU = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lnkCheck = new System.Windows.Forms.LinkLabel();
            this.btnRunTests = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.sprocTestConfigCtrl1 = new SqlSync.Test.SprocTestConfigCtrl();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statGeneral = new System.Windows.Forms.ToolStripStatusLabel();
            this.statSPCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.statTestCaseCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.statSPWithoutTests = new System.Windows.Forms.ToolStripStatusLabel();
            this.pgBar = new System.Windows.Forms.ToolStripProgressBar();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.bgSprocList = new System.ComponentModel.BackgroundWorker();
            this.bgNewTestCase = new System.ComponentModel.BackgroundWorker();
            this.settingsControl1 = new SqlSync.SettingsControl();
            this.ctxTestCase.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // treeView1
            // 
            this.treeView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.treeView1.ContextMenuStrip = this.ctxTestCase;
            this.treeView1.Location = new System.Drawing.Point(6, 30);
            this.treeView1.Margin = new System.Windows.Forms.Padding(10, 3, 3, 3);
            this.treeView1.Name = "treeView1";
            this.treeView1.Size = new System.Drawing.Size(284, 498);
            this.treeView1.TabIndex = 0;
            this.treeView1.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterCheck);
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            // 
            // ctxTestCase
            // 
            this.ctxTestCase.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuAddNewTestCase,
            this.deleteTestCaseToolStripMenuItem,
            this.toolStripSeparator1,
            this.mnuAddNewStoredProcedure,
            this.deleteStoredProcedureToolStripMenuItem,
            this.viewStoredProcedureScriptToolStripMenuItem,
            this.toolStripSeparator2,
            this.addNewFromExecutionScriptToolStripMenuItem});
            this.ctxTestCase.Name = "ctxTestCase";
            this.ctxTestCase.Size = new System.Drawing.Size(236, 148);
            this.ctxTestCase.Opening += new System.ComponentModel.CancelEventHandler(this.ctxTestCase_Opening);
            // 
            // mnuAddNewTestCase
            // 
            this.mnuAddNewTestCase.Image = global::SqlSync.Properties.Resources.Tick;
            this.mnuAddNewTestCase.Name = "mnuAddNewTestCase";
            this.mnuAddNewTestCase.Padding = new System.Windows.Forms.Padding(20, 1, 0, 1);
            this.mnuAddNewTestCase.Size = new System.Drawing.Size(255, 22);
            this.mnuAddNewTestCase.Text = "Add New Test Case";
            this.mnuAddNewTestCase.Click += new System.EventHandler(this.mnuAddNewTestCase_Click);
            // 
            // deleteTestCaseToolStripMenuItem
            // 
            this.deleteTestCaseToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Delete1;
            this.deleteTestCaseToolStripMenuItem.Name = "deleteTestCaseToolStripMenuItem";
            this.deleteTestCaseToolStripMenuItem.Size = new System.Drawing.Size(235, 22);
            this.deleteTestCaseToolStripMenuItem.Text = "Delete Test Case";
            this.deleteTestCaseToolStripMenuItem.Click += new System.EventHandler(this.deleteTestCaseToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(232, 6);
            // 
            // mnuAddNewStoredProcedure
            // 
            this.mnuAddNewStoredProcedure.Image = global::SqlSync.Properties.Resources.storedproc1;
            this.mnuAddNewStoredProcedure.Name = "mnuAddNewStoredProcedure";
            this.mnuAddNewStoredProcedure.Size = new System.Drawing.Size(235, 22);
            this.mnuAddNewStoredProcedure.Text = "Add New Stored Procedure";
            this.mnuAddNewStoredProcedure.Click += new System.EventHandler(this.mnuAddNewStoredProcedure_Click);
            // 
            // deleteStoredProcedureToolStripMenuItem
            // 
            this.deleteStoredProcedureToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Delete1;
            this.deleteStoredProcedureToolStripMenuItem.Name = "deleteStoredProcedureToolStripMenuItem";
            this.deleteStoredProcedureToolStripMenuItem.Size = new System.Drawing.Size(235, 22);
            this.deleteStoredProcedureToolStripMenuItem.Text = "Delete Stored Procedure";
            this.deleteStoredProcedureToolStripMenuItem.Click += new System.EventHandler(this.deleteStoredProcedureToolStripMenuItem_Click);
            // 
            // viewStoredProcedureScriptToolStripMenuItem
            // 
            this.viewStoredProcedureScriptToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Debug_Watch;
            this.viewStoredProcedureScriptToolStripMenuItem.Name = "viewStoredProcedureScriptToolStripMenuItem";
            this.viewStoredProcedureScriptToolStripMenuItem.Size = new System.Drawing.Size(235, 22);
            this.viewStoredProcedureScriptToolStripMenuItem.Text = "View Stored Procedure Script";
            this.viewStoredProcedureScriptToolStripMenuItem.Click += new System.EventHandler(this.viewStoredProcedureScriptToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(232, 6);
            // 
            // addNewFromExecutionScriptToolStripMenuItem
            // 
            this.addNewFromExecutionScriptToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Script_New;
            this.addNewFromExecutionScriptToolStripMenuItem.Name = "addNewFromExecutionScriptToolStripMenuItem";
            this.addNewFromExecutionScriptToolStripMenuItem.Size = new System.Drawing.Size(235, 22);
            this.addNewFromExecutionScriptToolStripMenuItem.Text = "Add New From Execution Script";
            this.addNewFromExecutionScriptToolStripMenuItem.Click += new System.EventHandler(this.addNewFromExecutionScriptToolStripMenuItem_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(933, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            this.menuStrip1.MenuActivate += new System.EventHandler(this.menuStrip1_MenuActivate);
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openNewTestConfigurationToolStripMenuItem,
            this.importTestCasesToolStripMenuItem,
            this.toolStripSeparator3,
            this.setTargetDatabaseToolStripMenuItem,
            this.bulkAddFromExecutionScriptsToolStripMenuItem,
            this.toolStripSeparator5,
            this.mnuFileMRU});
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(35, 20);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // openNewTestConfigurationToolStripMenuItem
            // 
            this.openNewTestConfigurationToolStripMenuItem.Name = "openNewTestConfigurationToolStripMenuItem";
            this.openNewTestConfigurationToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.openNewTestConfigurationToolStripMenuItem.Text = "Open/New Test Configuration";
            this.openNewTestConfigurationToolStripMenuItem.Click += new System.EventHandler(this.openNewTestConfigurationToolStripMenuItem_Click);
            // 
            // importTestCasesToolStripMenuItem
            // 
            this.importTestCasesToolStripMenuItem.Name = "importTestCasesToolStripMenuItem";
            this.importTestCasesToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.importTestCasesToolStripMenuItem.Text = "Import Test Cases";
            this.importTestCasesToolStripMenuItem.Click += new System.EventHandler(this.importTestCasesToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(235, 6);
            // 
            // setTargetDatabaseToolStripMenuItem
            // 
            this.setTargetDatabaseToolStripMenuItem.Name = "setTargetDatabaseToolStripMenuItem";
            this.setTargetDatabaseToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.setTargetDatabaseToolStripMenuItem.Text = "Set Target Database";
            this.setTargetDatabaseToolStripMenuItem.Click += new System.EventHandler(this.setTargetDatabaseToolStripMenuItem_Click);
            // 
            // bulkAddFromExecutionScriptsToolStripMenuItem
            // 
            this.bulkAddFromExecutionScriptsToolStripMenuItem.Name = "bulkAddFromExecutionScriptsToolStripMenuItem";
            this.bulkAddFromExecutionScriptsToolStripMenuItem.Size = new System.Drawing.Size(238, 22);
            this.bulkAddFromExecutionScriptsToolStripMenuItem.Text = "Bulk Add From Execution Scripts";
            this.bulkAddFromExecutionScriptsToolStripMenuItem.Click += new System.EventHandler(this.bulkAddFromExecutionScriptsToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(235, 6);
            // 
            // mnuFileMRU
            // 
            this.mnuFileMRU.Name = "mnuFileMRU";
            this.mnuFileMRU.Size = new System.Drawing.Size(238, 22);
            this.mnuFileMRU.Text = "Recent Files";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.helpToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Help_2;
            this.helpToolStripMenuItem.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(28, 20);
            this.helpToolStripMenuItem.Click += new System.EventHandler(this.helpToolStripMenuItem_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.CheckFileExists = false;
            this.openFileDialog1.DefaultExt = "sptest";
            this.openFileDialog1.Filter = "Stored Procedure Test File *.sptest|*.sptest|All Files *.*|*.*";
            this.openFileDialog1.Title = "Open / New Stored Procedure Test Configuration";
            // 
            // panel1
            // 
            this.panel1.ContextMenuStrip = this.ctxTestCase;
            this.panel1.Controls.Add(this.lnkCheck);
            this.panel1.Controls.Add(this.btnRunTests);
            this.panel1.Controls.Add(this.treeView1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel1.Location = new System.Drawing.Point(0, 80);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(297, 534);
            this.panel1.TabIndex = 5;
            // 
            // lnkCheck
            // 
            this.lnkCheck.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkCheck.AutoSize = true;
            this.lnkCheck.Location = new System.Drawing.Point(235, 13);
            this.lnkCheck.Name = "lnkCheck";
            this.lnkCheck.Size = new System.Drawing.Size(52, 13);
            this.lnkCheck.TabIndex = 2;
            this.lnkCheck.TabStop = true;
            this.lnkCheck.Text = "Check All";
            this.lnkCheck.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lnkCheck.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkCheck_LinkClicked);
            // 
            // btnRunTests
            // 
            this.btnRunTests.Location = new System.Drawing.Point(6, 3);
            this.btnRunTests.Name = "btnRunTests";
            this.btnRunTests.Size = new System.Drawing.Size(112, 23);
            this.btnRunTests.TabIndex = 1;
            this.btnRunTests.Text = "Run Checked Tests";
            this.btnRunTests.UseVisualStyleBackColor = true;
            this.btnRunTests.Click += new System.EventHandler(this.btnRunTests_Click);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.sprocTestConfigCtrl1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(300, 80);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(633, 534);
            this.panel2.TabIndex = 7;
            // 
            // sprocTestConfigCtrl1
            // 
            this.sprocTestConfigCtrl1.AutoScroll = true;
            this.sprocTestConfigCtrl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.sprocTestConfigCtrl1.Enabled = false;
            this.sprocTestConfigCtrl1.Location = new System.Drawing.Point(0, 0);
            this.sprocTestConfigCtrl1.Name = "sprocTestConfigCtrl1";
            this.sprocTestConfigCtrl1.Size = new System.Drawing.Size(633, 534);
            this.sprocTestConfigCtrl1.SprocName = "";
            this.sprocTestConfigCtrl1.TabIndex = 0;
            this.sprocTestConfigCtrl1.TestCase = null;
            this.sprocTestConfigCtrl1.TestCaseChanged += new SqlSync.Test.TestCaseChangedEventHandler(this.sprocTestConfigCtrl1_TestCaseChanged);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statGeneral,
            this.statSPCount,
            this.statTestCaseCount,
            this.statSPWithoutTests,
            this.pgBar});
            this.statusStrip1.Location = new System.Drawing.Point(0, 614);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(933, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statGeneral
            // 
            this.statGeneral.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.statGeneral.Name = "statGeneral";
            this.statGeneral.Size = new System.Drawing.Size(359, 17);
            this.statGeneral.Spring = true;
            this.statGeneral.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statSPCount
            // 
            this.statSPCount.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.statSPCount.Name = "statSPCount";
            this.statSPCount.Padding = new System.Windows.Forms.Padding(0, 0, 30, 0);
            this.statSPCount.Size = new System.Drawing.Size(143, 17);
            this.statSPCount.Text = "Stored Procedures: 0";
            this.statSPCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statTestCaseCount
            // 
            this.statTestCaseCount.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.statTestCaseCount.Name = "statTestCaseCount";
            this.statTestCaseCount.Padding = new System.Windows.Forms.Padding(0, 0, 30, 0);
            this.statTestCaseCount.Size = new System.Drawing.Size(107, 17);
            this.statTestCaseCount.Text = "Test Cases: 0";
            this.statTestCaseCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statSPWithoutTests
            // 
            this.statSPWithoutTests.ForeColor = System.Drawing.Color.OrangeRed;
            this.statSPWithoutTests.Name = "statSPWithoutTests";
            this.statSPWithoutTests.Padding = new System.Windows.Forms.Padding(0, 0, 30, 0);
            this.statSPWithoutTests.Size = new System.Drawing.Size(207, 17);
            this.statSPWithoutTests.Text = "Stored Procedures without Tests: 0";
            this.statSPWithoutTests.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // pgBar
            // 
            this.pgBar.Name = "pgBar";
            this.pgBar.Size = new System.Drawing.Size(100, 16);
            // 
            // splitter1
            // 
            this.splitter1.Location = new System.Drawing.Point(297, 80);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(3, 534);
            this.splitter1.TabIndex = 8;
            this.splitter1.TabStop = false;
            // 
            // bgSprocList
            // 
            this.bgSprocList.WorkerReportsProgress = true;
            this.bgSprocList.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgSprocList_DoWork);
            this.bgSprocList.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgSprocList_RunWorkerCompleted);
            this.bgSprocList.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgSprocList_ProgressChanged);
            // 
            // bgNewTestCase
            // 
            this.bgNewTestCase.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgNewTestCase_DoWork);
            this.bgNewTestCase.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgNewTestCase_RunWorkerCompleted);
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
            this.settingsControl1.Size = new System.Drawing.Size(933, 56);
            this.settingsControl1.TabIndex = 2;
            this.settingsControl1.ServerChanged += new SqlSync.ServerChangedEventHandler(this.settingsControl1_ServerChanged);
            // 
            // SprocTestConfigForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(933, 636);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.settingsControl1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "SprocTestConfigForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sql Build Manager :: Stored Procedure Test Configuration";
            this.Load += new System.EventHandler(this.SprocTestConfigForm_Load);
            this.ctxTestCase.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TreeView treeView1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private SettingsControl settingsControl1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openNewTestConfigurationToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ContextMenuStrip ctxTestCase;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Panel panel2;
        private SprocTestConfigCtrl sprocTestConfigCtrl1;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statGeneral;
        private System.Windows.Forms.Splitter splitter1;
        private System.ComponentModel.BackgroundWorker bgSprocList;
        private System.Windows.Forms.ToolStripMenuItem mnuAddNewStoredProcedure;
        private System.Windows.Forms.ToolStripMenuItem mnuAddNewTestCase;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.LinkLabel lnkCheck;
        private System.Windows.Forms.Button btnRunTests;
        private System.Windows.Forms.ToolStripMenuItem viewStoredProcedureScriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem deleteTestCaseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem deleteStoredProcedureToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem addNewFromExecutionScriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem setTargetDatabaseToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem importTestCasesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem bulkAddFromExecutionScriptsToolStripMenuItem;
        private System.Windows.Forms.ToolStripStatusLabel statSPCount;
        private System.Windows.Forms.ToolStripStatusLabel statTestCaseCount;
        private System.Windows.Forms.ToolStripStatusLabel statSPWithoutTests;
        private System.Windows.Forms.ToolStripMenuItem mnuFileMRU;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.ComponentModel.BackgroundWorker bgNewTestCase;
        private System.Windows.Forms.ToolStripProgressBar pgBar;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
    }
}