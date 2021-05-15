namespace SqlSync.SqlBuild.MultiDb
{
    partial class MultiDbRunForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MultiDbRunForm));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statGeneral = new System.Windows.Forms.ToolStripStatusLabel();
            this.statChanged = new System.Windows.Forms.ToolStripStatusLabel();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.mnuActionMain = new System.Windows.Forms.ToolStripMenuItem();
            this.loadConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.createConfigurationViaQueryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.addAnotherServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.tryBuildUsingCurrentConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.runBuildUsingCurrentConfigurationCommitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator5 = new System.Windows.Forms.ToolStripSeparator();
            this.runBuildUsingCurrentConfigurationWithoutTransactionsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.constructCommandLineToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator4 = new System.Windows.Forms.ToolStripSeparator();
            this.mnuFileMRU = new System.Windows.Forms.ToolStripMenuItem();
            this.toolsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generateScriptStatusReportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.generateObjectComparisonReportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator6 = new System.Windows.Forms.ToolStripSeparator();
            this.adHocQueryExecutionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.bgLoadCfg = new System.ComponentModel.BackgroundWorker();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.lstServers = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.label1 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label2 = new System.Windows.Forms.Label();
            this.buildValidationReportToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator7 = new System.Windows.Forms.ToolStripSeparator();
            this.statusStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statGeneral,
            this.statChanged});
            this.statusStrip1.Location = new System.Drawing.Point(0, 560);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(589, 22);
            this.statusStrip1.TabIndex = 1;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statGeneral
            // 
            this.statGeneral.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.statGeneral.Name = "statGeneral";
            this.statGeneral.Size = new System.Drawing.Size(513, 17);
            this.statGeneral.Spring = true;
            this.statGeneral.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statChanged
            // 
            this.statChanged.ForeColor = System.Drawing.Color.Green;
            this.statChanged.Name = "statChanged";
            this.statChanged.Size = new System.Drawing.Size(61, 17);
            this.statChanged.Text = "Unchanged";
            this.statChanged.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuActionMain,
            this.toolsToolStripMenuItem,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(589, 24);
            this.menuStrip1.TabIndex = 2;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // mnuActionMain
            // 
            this.mnuActionMain.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.loadConfigurationToolStripMenuItem,
            this.saveConfigurationToolStripMenuItem,
            this.createConfigurationViaQueryToolStripMenuItem,
            this.toolStripSeparator2,
            this.addAnotherServerToolStripMenuItem,
            this.toolStripSeparator1,
            this.tryBuildUsingCurrentConfigurationToolStripMenuItem,
            this.runBuildUsingCurrentConfigurationCommitToolStripMenuItem,
            this.toolStripSeparator5,
            this.runBuildUsingCurrentConfigurationWithoutTransactionsToolStripMenuItem,
            this.toolStripSeparator3,
            //this.constructCommandLineToolStripMenuItem,
            this.toolStripSeparator4,
            this.mnuFileMRU});
            this.mnuActionMain.Image = global::SqlSync.Properties.Resources.Execute;
            this.mnuActionMain.Name = "mnuActionMain";
            this.mnuActionMain.Size = new System.Drawing.Size(65, 20);
            this.mnuActionMain.Text = "Action";
            // 
            // loadConfigurationToolStripMenuItem
            // 
            this.loadConfigurationToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Open;
            this.loadConfigurationToolStripMenuItem.Name = "loadConfigurationToolStripMenuItem";
            this.loadConfigurationToolStripMenuItem.Size = new System.Drawing.Size(375, 22);
            this.loadConfigurationToolStripMenuItem.Text = "Load Configuration";
            this.loadConfigurationToolStripMenuItem.Click += new System.EventHandler(this.loadConfigurationToolStripMenuItem_Click);
            // 
            // saveConfigurationToolStripMenuItem
            // 
            this.saveConfigurationToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Save;
            this.saveConfigurationToolStripMenuItem.Name = "saveConfigurationToolStripMenuItem";
            this.saveConfigurationToolStripMenuItem.Size = new System.Drawing.Size(375, 22);
            this.saveConfigurationToolStripMenuItem.Text = "Save Configuration";
            this.saveConfigurationToolStripMenuItem.Click += new System.EventHandler(this.saveConfigurationToolStripMenuItem_Click);
            // 
            // createConfigurationViaQueryToolStripMenuItem
            // 
            this.createConfigurationViaQueryToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Database_Search;
            this.createConfigurationViaQueryToolStripMenuItem.Name = "createConfigurationViaQueryToolStripMenuItem";
            this.createConfigurationViaQueryToolStripMenuItem.Size = new System.Drawing.Size(375, 22);
            this.createConfigurationViaQueryToolStripMenuItem.Text = "Load Configuration via Query";
            this.createConfigurationViaQueryToolStripMenuItem.Click += new System.EventHandler(this.createConfigurationViaQueryToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(372, 6);
            // 
            // addAnotherServerToolStripMenuItem
            // 
            this.addAnotherServerToolStripMenuItem.Image = global::SqlSync.Properties.Resources.DB_Add;
            this.addAnotherServerToolStripMenuItem.Name = "addAnotherServerToolStripMenuItem";
            this.addAnotherServerToolStripMenuItem.Size = new System.Drawing.Size(375, 22);
            this.addAnotherServerToolStripMenuItem.Text = "Add Another Server Configuration";
            this.addAnotherServerToolStripMenuItem.Click += new System.EventHandler(this.addAnotherServerToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(372, 6);
            // 
            // tryBuildUsingCurrentConfigurationToolStripMenuItem
            // 
            this.tryBuildUsingCurrentConfigurationToolStripMenuItem.Image = global::SqlSync.Properties.Resources.db_next;
            this.tryBuildUsingCurrentConfigurationToolStripMenuItem.Name = "tryBuildUsingCurrentConfigurationToolStripMenuItem";
            this.tryBuildUsingCurrentConfigurationToolStripMenuItem.Size = new System.Drawing.Size(375, 22);
            this.tryBuildUsingCurrentConfigurationToolStripMenuItem.Text = "Try Build using Current Configuration (Rollback)";
            this.tryBuildUsingCurrentConfigurationToolStripMenuItem.Click += new System.EventHandler(this.runBuildUsingCurrentConfigurationToolStripMenuItem_Click);
            // 
            // runBuildUsingCurrentConfigurationCommitToolStripMenuItem
            // 
            this.runBuildUsingCurrentConfigurationCommitToolStripMenuItem.Image = global::SqlSync.Properties.Resources.db_edit_green;
            this.runBuildUsingCurrentConfigurationCommitToolStripMenuItem.Name = "runBuildUsingCurrentConfigurationCommitToolStripMenuItem";
            this.runBuildUsingCurrentConfigurationCommitToolStripMenuItem.Size = new System.Drawing.Size(375, 22);
            this.runBuildUsingCurrentConfigurationCommitToolStripMenuItem.Text = "Run Build using Current Configuration (Commit)";
            this.runBuildUsingCurrentConfigurationCommitToolStripMenuItem.Click += new System.EventHandler(this.runBuildUsingCurrentConfigurationCommitToolStripMenuItem_Click);
            // 
            // toolStripSeparator5
            // 
            this.toolStripSeparator5.Name = "toolStripSeparator5";
            this.toolStripSeparator5.Size = new System.Drawing.Size(372, 6);
            // 
            // runBuildUsingCurrentConfigurationWithoutTransactionsToolStripMenuItem
            // 
            this.runBuildUsingCurrentConfigurationWithoutTransactionsToolStripMenuItem.Name = "runBuildUsingCurrentConfigurationWithoutTransactionsToolStripMenuItem";
            this.runBuildUsingCurrentConfigurationWithoutTransactionsToolStripMenuItem.Size = new System.Drawing.Size(375, 22);
            this.runBuildUsingCurrentConfigurationWithoutTransactionsToolStripMenuItem.Text = "Run Build using Current Configuration - without Transactions";
            this.runBuildUsingCurrentConfigurationWithoutTransactionsToolStripMenuItem.ToolTipText = "CAUTION!!\r\nBuild will be run without a transaction!\r\n**Your scripts will not be r" +
                "olled back in the event of a failure! **";
            this.runBuildUsingCurrentConfigurationWithoutTransactionsToolStripMenuItem.Click += new System.EventHandler(this.runBuildUsingCurrentConfigurationWithoutTransactionsToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(372, 6);
            // 
            // constructCommandLineToolStripMenuItem
            // 
            this.constructCommandLineToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Database_commandline;
            this.constructCommandLineToolStripMenuItem.Name = "constructCommandLineToolStripMenuItem";
            this.constructCommandLineToolStripMenuItem.Size = new System.Drawing.Size(375, 22);
            this.constructCommandLineToolStripMenuItem.Text = "Construct Command Line String";
            this.constructCommandLineToolStripMenuItem.Click += new System.EventHandler(this.constructCommandLineToolStripMenuItem_Click);
            // 
            // toolStripSeparator4
            // 
            this.toolStripSeparator4.Name = "toolStripSeparator4";
            this.toolStripSeparator4.Size = new System.Drawing.Size(372, 6);
            // 
            // mnuFileMRU
            // 
            this.mnuFileMRU.Name = "mnuFileMRU";
            this.mnuFileMRU.Size = new System.Drawing.Size(375, 22);
            this.mnuFileMRU.Text = "Recent Files";
            // 
            // toolsToolStripMenuItem
            // 
            this.toolsToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.buildValidationReportToolStripMenuItem,
            this.toolStripSeparator7,
            this.generateScriptStatusReportToolStripMenuItem,
            this.generateObjectComparisonReportToolStripMenuItem,
            this.toolStripSeparator6,
            this.adHocQueryExecutionToolStripMenuItem});
            this.toolsToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Report_2;
            this.toolsToolStripMenuItem.Name = "toolsToolStripMenuItem";
            this.toolsToolStripMenuItem.Size = new System.Drawing.Size(73, 20);
            this.toolsToolStripMenuItem.Text = "Reports";
            // 
            // generateScriptStatusReportToolStripMenuItem
            // 
            this.generateScriptStatusReportToolStripMenuItem.Name = "generateScriptStatusReportToolStripMenuItem";
            this.generateScriptStatusReportToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.generateScriptStatusReportToolStripMenuItem.Text = "Script Status Report";
            this.generateScriptStatusReportToolStripMenuItem.Click += new System.EventHandler(this.generateScriptStatusReportToolStripMenuItem_Click);
            // 
            // generateObjectComparisonReportToolStripMenuItem
            // 
            this.generateObjectComparisonReportToolStripMenuItem.Name = "generateObjectComparisonReportToolStripMenuItem";
            this.generateObjectComparisonReportToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.generateObjectComparisonReportToolStripMenuItem.Text = "Object Comparison Report";
            this.generateObjectComparisonReportToolStripMenuItem.Click += new System.EventHandler(this.generateObjectComparisonReportToolStripMenuItem_Click);
            // 
            // toolStripSeparator6
            // 
            this.toolStripSeparator6.Name = "toolStripSeparator6";
            this.toolStripSeparator6.Size = new System.Drawing.Size(209, 6);
            // 
            // adHocQueryExecutionToolStripMenuItem
            // 
            this.adHocQueryExecutionToolStripMenuItem.Name = "adHocQueryExecutionToolStripMenuItem";
            this.adHocQueryExecutionToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.adHocQueryExecutionToolStripMenuItem.Text = "AdHoc Query Execution";
            this.adHocQueryExecutionToolStripMenuItem.Click += new System.EventHandler(this.adHocQueryExecutionToolStripMenuItem_Click);
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.helpToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Help_2;
            this.helpToolStripMenuItem.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(28, 20);
            this.helpToolStripMenuItem.ToolTipText = "Open Help file";
            this.helpToolStripMenuItem.Click += new System.EventHandler(this.helpToolStripMenuItem_Click);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.Filter = "MultiDb Config Files *.multiDb|*.multiDb|All Files *.*|*.*";
            this.saveFileDialog1.Title = "Save Multi Database Run Configuration";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "MultiDb Config Files *.multiDb|*.multiDb|Config File *.cfg|*.cfg|All Files *.*|*." +
                "*";
            this.openFileDialog1.Title = "Save Multi Database Run Configuration";
            // 
            // bgLoadCfg
            // 
            this.bgLoadCfg.WorkerReportsProgress = true;
            this.bgLoadCfg.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgLoadCfg_DoWork);
            this.bgLoadCfg.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgLoadCfg_ProgressChanged);
            this.bgLoadCfg.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgLoadCfg_RunWorkerCompleted);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 47);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.lstServers);
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Size = new System.Drawing.Size(589, 513);
            this.splitContainer1.SplitterDistance = 173;
            this.splitContainer1.TabIndex = 3;
            // 
            // lstServers
            // 
            this.lstServers.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.lstServers.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.lstServers.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lstServers.FullRowSelect = true;
            this.lstServers.GridLines = true;
            this.lstServers.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lstServers.Location = new System.Drawing.Point(0, 23);
            this.lstServers.Name = "lstServers";
            this.lstServers.Size = new System.Drawing.Size(173, 490);
            this.lstServers.TabIndex = 1;
            this.lstServers.UseCompatibleStateImageBehavior = false;
            this.lstServers.View = System.Windows.Forms.View.Details;
            this.lstServers.SelectedIndexChanged += new System.EventHandler(this.lstServers_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "";
            this.columnHeader1.Width = 167;
            // 
            // label1
            // 
            this.label1.BackColor = System.Drawing.Color.White;
            this.label1.Dock = System.Windows.Forms.DockStyle.Top;
            this.label1.Font = new System.Drawing.Font("Calibri", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(173, 23);
            this.label1.TabIndex = 0;
            this.label1.Text = "Servers";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.LightSteelBlue;
            this.panel1.Controls.Add(this.label2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 24);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(589, 23);
            this.panel1.TabIndex = 4;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(93, 4);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(369, 15);
            this.label2.TabIndex = 0;
            this.label2.Text = "Select a server in the list to display and configure the run settings.";
            // 
            // buildValidationReportToolStripMenuItem
            // 
            this.buildValidationReportToolStripMenuItem.Name = "buildValidationReportToolStripMenuItem";
            this.buildValidationReportToolStripMenuItem.Size = new System.Drawing.Size(212, 22);
            this.buildValidationReportToolStripMenuItem.Text = "Build Validation Report";
            this.buildValidationReportToolStripMenuItem.Click += new System.EventHandler(this.buildValidationReportToolStripMenuItem_Click);
            // 
            // toolStripSeparator7
            // 
            this.toolStripSeparator7.Name = "toolStripSeparator7";
            this.toolStripSeparator7.Size = new System.Drawing.Size(209, 6);
            // 
            // MultiDbRunForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.ClientSize = new System.Drawing.Size(589, 582);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "MultiDbRunForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Multiple Database Configuration";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MultiDbRunForm_FormClosing);
            this.Load += new System.EventHandler(this.MultiDbRunForm_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuActionMain;
        private System.Windows.Forms.ToolStripMenuItem addAnotherServerToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveConfigurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem tryBuildUsingCurrentConfigurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem runBuildUsingCurrentConfigurationCommitToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ToolStripMenuItem loadConfigurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem mnuFileMRU;
        private System.Windows.Forms.ToolStripStatusLabel statGeneral;
        private System.Windows.Forms.ToolStripStatusLabel statChanged;
        private System.ComponentModel.BackgroundWorker bgLoadCfg;
        private System.Windows.Forms.ToolStripMenuItem constructCommandLineToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator4;
        private System.Windows.Forms.ToolStripMenuItem runBuildUsingCurrentConfigurationWithoutTransactionsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator5;
        private System.Windows.Forms.ToolStripMenuItem toolsToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem generateScriptStatusReportToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem generateObjectComparisonReportToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator6;
        private System.Windows.Forms.ToolStripMenuItem adHocQueryExecutionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem createConfigurationViaQueryToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.ListView lstServers;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Label label1;
        //private MultiDbPage multiDbPage1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ToolStripMenuItem buildValidationReportToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator7;
    }
}