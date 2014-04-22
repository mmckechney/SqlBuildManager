namespace SqlSync.SqlBuild
{
    partial class RebuildForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RebuildForm));
            this.lstBuildFiles = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.rebuildFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.actionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeSqlServerConnectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.bgWorker = new System.ComponentModel.BackgroundWorker();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statGeneral = new System.Windows.Forms.ToolStripStatusLabel();
            this.statBar = new System.Windows.Forms.ToolStripProgressBar();
            this.settingsControl1 = new SqlSync.SettingsControl();
            this.label1 = new System.Windows.Forms.Label();
            this.ddDatabases = new System.Windows.Forms.ComboBox();
            this.btnGetList = new System.Windows.Forms.Button();
            this.contextMenuStrip1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstBuildFiles
            // 
            this.lstBuildFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstBuildFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader4,
            this.columnHeader2,
            this.columnHeader3});
            this.lstBuildFiles.ContextMenuStrip = this.contextMenuStrip1;
            this.lstBuildFiles.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstBuildFiles.FullRowSelect = true;
            this.lstBuildFiles.GridLines = true;
            this.lstBuildFiles.Location = new System.Drawing.Point(12, 101);
            this.lstBuildFiles.MultiSelect = false;
            this.lstBuildFiles.Name = "lstBuildFiles";
            this.lstBuildFiles.Size = new System.Drawing.Size(707, 359);
            this.lstBuildFiles.TabIndex = 1;
            this.lstBuildFiles.UseCompatibleStateImageBehavior = false;
            this.lstBuildFiles.View = System.Windows.Forms.View.Details;
            this.lstBuildFiles.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lstBuildFiles_ColumnClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Build File Name";
            this.columnHeader1.Width = 347;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Database(s)";
            this.columnHeader4.Width = 0;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Committed Date";
            this.columnHeader2.Width = 217;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Number of Scripts";
            this.columnHeader3.Width = 109;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.rebuildFileToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(140, 26);
            // 
            // rebuildFileToolStripMenuItem
            // 
            this.rebuildFileToolStripMenuItem.Name = "rebuildFileToolStripMenuItem";
            this.rebuildFileToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            this.rebuildFileToolStripMenuItem.Text = "Rebuild File";
            this.rebuildFileToolStripMenuItem.Click += new System.EventHandler(this.rebuildFileToolStripMenuItem_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.actionToolStripMenuItem,
            this.toolStripMenuItem1});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(741, 24);
            this.menuStrip1.TabIndex = 18;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // actionToolStripMenuItem
            // 
            this.actionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.changeSqlServerConnectionToolStripMenuItem});
            this.actionToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Execute;
            this.actionToolStripMenuItem.Name = "actionToolStripMenuItem";
            this.actionToolStripMenuItem.Size = new System.Drawing.Size(65, 20);
            this.actionToolStripMenuItem.Text = "Action";
            // 
            // changeSqlServerConnectionToolStripMenuItem
            // 
            this.changeSqlServerConnectionToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Server1;
            this.changeSqlServerConnectionToolStripMenuItem.Name = "changeSqlServerConnectionToolStripMenuItem";
            this.changeSqlServerConnectionToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
            this.changeSqlServerConnectionToolStripMenuItem.Text = "&Change Sql Server Connection";
            this.changeSqlServerConnectionToolStripMenuItem.Click += new System.EventHandler(this.changeSqlServerConnectionToolStripMenuItem_Click);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripMenuItem1.Image = global::SqlSync.Properties.Resources.Help_2;
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(28, 20);
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.CheckFileExists = false;
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "Sql Build Manager File *.sbm|*.sbm|All Files *.*|*.*";
            this.openFileDialog1.Title = "Select Reconstructed File Name";
            // 
            // bgWorker
            // 
            this.bgWorker.WorkerReportsProgress = true;
            this.bgWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgWorker_DoWork);
            this.bgWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgWorker_ProgressChanged);
            this.bgWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgWorker_RunWorkerCompleted);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statGeneral,
            this.statBar});
            this.statusStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.Flow;
            this.statusStrip1.Location = new System.Drawing.Point(0, 473);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(741, 23);
            this.statusStrip1.TabIndex = 19;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statGeneral
            // 
            this.statGeneral.Name = "statGeneral";
            this.statGeneral.Overflow = System.Windows.Forms.ToolStripItemOverflow.Always;
            this.statGeneral.Size = new System.Drawing.Size(42, 13);
            this.statGeneral.Text = "Ready.";
            // 
            // statBar
            // 
            this.statBar.Margin = new System.Windows.Forms.Padding(3);
            this.statBar.MarqueeAnimationSpeed = 200;
            this.statBar.Name = "statBar";
            this.statBar.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.statBar.Size = new System.Drawing.Size(200, 17);
            this.statBar.Style = System.Windows.Forms.ProgressBarStyle.Marquee;
            // 
            // settingsControl1
            // 
            this.settingsControl1.BackColor = System.Drawing.Color.White;
            this.settingsControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.settingsControl1.Location = new System.Drawing.Point(0, 24);
            this.settingsControl1.Name = "settingsControl1";
            this.settingsControl1.Project = "";
            this.settingsControl1.ProjectLabelText = "";
            this.settingsControl1.Server = "";
            this.settingsControl1.Size = new System.Drawing.Size(741, 44);
            this.settingsControl1.TabIndex = 13;
            this.settingsControl1.ServerChanged += new SqlSync.ServerChangedEventHandler(this.settingsControl1_ServerChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 78);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 13);
            this.label1.TabIndex = 20;
            this.label1.Text = "Select Database: ";
            // 
            // ddDatabases
            // 
            this.ddDatabases.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddDatabases.FormattingEnabled = true;
            this.ddDatabases.Location = new System.Drawing.Point(110, 74);
            this.ddDatabases.Name = "ddDatabases";
            this.ddDatabases.Size = new System.Drawing.Size(181, 21);
            this.ddDatabases.TabIndex = 21;
            this.ddDatabases.SelectionChangeCommitted += new System.EventHandler(this.ddDatabases_SelectionChangeCommitted);
            // 
            // btnGetList
            // 
            this.btnGetList.Location = new System.Drawing.Point(298, 72);
            this.btnGetList.Name = "btnGetList";
            this.btnGetList.Size = new System.Drawing.Size(109, 23);
            this.btnGetList.TabIndex = 22;
            this.btnGetList.Text = "Get Build File List";
            this.btnGetList.UseVisualStyleBackColor = true;
            this.btnGetList.Click += new System.EventHandler(this.btnGetList_Click);
            // 
            // RebuildForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(741, 496);
            this.Controls.Add(this.btnGetList);
            this.Controls.Add(this.ddDatabases);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.settingsControl1);
            this.Controls.Add(this.lstBuildFiles);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "RebuildForm";
            this.Text = "Rebuild Sql Build Manager File";
            this.Load += new System.EventHandler(this.RebuildForm_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView lstBuildFiles;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private SettingsControl settingsControl1;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem actionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeSqlServerConnectionToolStripMenuItem;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem rebuildFileToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.ComponentModel.BackgroundWorker bgWorker;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statGeneral;
        private System.Windows.Forms.ToolStripProgressBar statBar;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox ddDatabases;
        private System.Windows.Forms.Button btnGetList;
    }
}