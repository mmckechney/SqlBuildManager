namespace SqlSync.SqlBuild
{
    partial class BackoutPackageForm
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
            System.Windows.Forms.ListViewGroup listViewGroup1 = new System.Windows.Forms.ListViewGroup("Not Found on Target Server", System.Windows.Forms.HorizontalAlignment.Left);
            System.Windows.Forms.ListViewGroup listViewGroup2 = new System.Windows.Forms.ListViewGroup("Manual Scripts", System.Windows.Forms.HorizontalAlignment.Left);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BackoutPackageForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.actionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuChangeSqlServer = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lstObjectsToUpdate = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.viewScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lstObjectNotUpdateable = new System.Windows.Forms.ListView();
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statGeneral = new System.Windows.Forms.ToolStripStatusLabel();
            this.statBar = new System.Windows.Forms.ToolStripProgressBar();
            this.btnCreate = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.lblDatabaseSetting = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lblServerSetting = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.pnlSqlConnect = new System.Windows.Forms.Panel();
            this.btnCancelChangeSource = new System.Windows.Forms.Button();
            this.btnChangeSource = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.label12 = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.chkDropRoutines = new System.Windows.Forms.CheckBox();
            this.chkRemoveNewScripts = new System.Windows.Forms.CheckBox();
            this.label10 = new System.Windows.Forms.Label();
            this.chkManualAsRunOnce = new System.Windows.Forms.CheckBox();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.txtBackoutPackage = new System.Windows.Forms.TextBox();
            this.label7 = new System.Windows.Forms.Label();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.bgMakeBackout = new System.ComponentModel.BackgroundWorker();
            this.bgCheckTargetObjects = new System.ComponentModel.BackgroundWorker();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.viewLogFileMenuItem1 = new SqlSync.Controls.ViewLogFileMenuItem();
            this.setLoggingLevelMenuItem1 = new SqlSync.Controls.SetLoggingLevelMenuItem();
            this.sqlConnect1 = new SqlSync.SQLConnect();
            this.lnkCopyList = new System.Windows.Forms.LinkLabel();
            this.menuStrip1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.contextMenuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.pnlSqlConnect.SuspendLayout();
            this.panel3.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.tableLayoutPanel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.actionToolStripMenuItem,
            this.helpToolStripMenuItem,
            this.toolStripMenuItem1});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(742, 24);
            this.menuStrip1.TabIndex = 0;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // actionToolStripMenuItem
            // 
            this.actionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuChangeSqlServer});
            this.actionToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Execute;
            this.actionToolStripMenuItem.Name = "actionToolStripMenuItem";
            this.actionToolStripMenuItem.Size = new System.Drawing.Size(65, 20);
            this.actionToolStripMenuItem.Text = "Action";
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
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewLogFileMenuItem1,
            this.setLoggingLevelMenuItem1});
            this.helpToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Help;
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(56, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripMenuItem1.Image = global::SqlSync.Properties.Resources.Help_2;
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(28, 20);
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.label5);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 24);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(742, 76);
            this.panel1.TabIndex = 1;
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(105, 40);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(505, 26);
            this.label6.TabIndex = 5;
            this.label6.Text = "Scripts that were manually written and added to the package CAN NOT be re-scripte" +
                "d. \r\nThese will need to be updated by hand as needed.";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label5.ForeColor = System.Drawing.Color.Red;
            this.label5.Location = new System.Drawing.Point(12, 40);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(87, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Please note:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(12, 6);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(674, 26);
            this.label3.TabIndex = 3;
            this.label3.Text = resources.GetString("label3.Text");
            // 
            // lstObjectsToUpdate
            // 
            this.lstObjectsToUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstObjectsToUpdate.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.lstObjectsToUpdate.ContextMenuStrip = this.contextMenuStrip1;
            this.lstObjectsToUpdate.GridLines = true;
            this.lstObjectsToUpdate.Location = new System.Drawing.Point(3, 29);
            this.lstObjectsToUpdate.Name = "lstObjectsToUpdate";
            this.lstObjectsToUpdate.Size = new System.Drawing.Size(344, 286);
            this.lstObjectsToUpdate.TabIndex = 2;
            this.lstObjectsToUpdate.UseCompatibleStateImageBehavior = false;
            this.lstObjectsToUpdate.View = System.Windows.Forms.View.Details;
            this.lstObjectsToUpdate.SelectedIndexChanged += new System.EventHandler(this.lstObjectsToUpdate_SelectedIndexChanged);
            this.lstObjectsToUpdate.DoubleClick += new System.EventHandler(this.ViewScript);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Script Name";
            this.columnHeader1.Width = 291;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewScriptToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(138, 26);
            // 
            // viewScriptToolStripMenuItem
            // 
            this.viewScriptToolStripMenuItem.Name = "viewScriptToolStripMenuItem";
            this.viewScriptToolStripMenuItem.Size = new System.Drawing.Size(137, 22);
            this.viewScriptToolStripMenuItem.Text = "View Script";
            this.viewScriptToolStripMenuItem.Click += new System.EventHandler(this.ViewScript);
            // 
            // lstObjectNotUpdateable
            // 
            this.lstObjectNotUpdateable.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstObjectNotUpdateable.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader2});
            this.lstObjectNotUpdateable.ContextMenuStrip = this.contextMenuStrip1;
            this.lstObjectNotUpdateable.GridLines = true;
            listViewGroup1.Header = "Not Found on Target Server";
            listViewGroup1.Name = "grpNotPresent";
            listViewGroup2.Header = "Manual Scripts";
            listViewGroup2.Name = "grpNotUpdatable";
            this.lstObjectNotUpdateable.Groups.AddRange(new System.Windows.Forms.ListViewGroup[] {
            listViewGroup1,
            listViewGroup2});
            this.lstObjectNotUpdateable.Location = new System.Drawing.Point(353, 29);
            this.lstObjectNotUpdateable.Name = "lstObjectNotUpdateable";
            this.lstObjectNotUpdateable.Size = new System.Drawing.Size(344, 286);
            this.lstObjectNotUpdateable.TabIndex = 3;
            this.toolTip1.SetToolTip(this.lstObjectNotUpdateable, resources.GetString("lstObjectNotUpdateable.ToolTip"));
            this.lstObjectNotUpdateable.UseCompatibleStateImageBehavior = false;
            this.lstObjectNotUpdateable.View = System.Windows.Forms.View.Details;
            this.lstObjectNotUpdateable.DoubleClick += new System.EventHandler(this.ViewScript);
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Script Name";
            this.columnHeader2.Width = 275;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statGeneral,
            this.statBar});
            this.statusStrip1.Location = new System.Drawing.Point(0, 689);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(742, 22);
            this.statusStrip1.TabIndex = 4;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statGeneral
            // 
            this.statGeneral.AutoSize = false;
            this.statGeneral.Name = "statGeneral";
            this.statGeneral.Size = new System.Drawing.Size(625, 17);
            this.statGeneral.Spring = true;
            this.statGeneral.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statBar
            // 
            this.statBar.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.statBar.Name = "statBar";
            this.statBar.Size = new System.Drawing.Size(100, 16);
            // 
            // btnCreate
            // 
            this.btnCreate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCreate.Location = new System.Drawing.Point(213, 500);
            this.btnCreate.Name = "btnCreate";
            this.btnCreate.Size = new System.Drawing.Size(147, 23);
            this.btnCreate.TabIndex = 5;
            this.btnCreate.Text = "Create Back Out Package";
            this.btnCreate.UseVisualStyleBackColor = true;
            this.btnCreate.Click += new System.EventHandler(this.btnCreate_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCancel.Location = new System.Drawing.Point(382, 500);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(147, 23);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.Silver;
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.lblDatabaseSetting);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.lblServerSetting);
            this.panel2.Controls.Add(this.label1);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 100);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(742, 58);
            this.panel2.TabIndex = 2;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(23, 37);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(56, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Database:";
            // 
            // lblDatabaseSetting
            // 
            this.lblDatabaseSetting.AutoSize = true;
            this.lblDatabaseSetting.Location = new System.Drawing.Point(94, 37);
            this.lblDatabaseSetting.Name = "lblDatabaseSetting";
            this.lblDatabaseSetting.Size = new System.Drawing.Size(35, 13);
            this.lblDatabaseSetting.TabIndex = 3;
            this.lblDatabaseSetting.Text = "label2";
            this.lblDatabaseSetting.TextChanged += new System.EventHandler(this.lblDatabaseSetting_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(23, 20);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(41, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Server:";
            // 
            // lblServerSetting
            // 
            this.lblServerSetting.AutoSize = true;
            this.lblServerSetting.Location = new System.Drawing.Point(94, 20);
            this.lblServerSetting.Name = "lblServerSetting";
            this.lblServerSetting.Size = new System.Drawing.Size(35, 13);
            this.lblServerSetting.TabIndex = 1;
            this.lblServerSetting.Text = "label2";
            this.lblServerSetting.Click += new System.EventHandler(this.lblDatabaseSetting_TextChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(210, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Script \"back out\" objects from:";
            // 
            // pnlSqlConnect
            // 
            this.pnlSqlConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlSqlConnect.BackColor = System.Drawing.Color.LightSteelBlue;
            this.pnlSqlConnect.Controls.Add(this.btnCancelChangeSource);
            this.pnlSqlConnect.Controls.Add(this.btnChangeSource);
            this.pnlSqlConnect.Controls.Add(this.sqlConnect1);
            this.pnlSqlConnect.Location = new System.Drawing.Point(206, 74);
            this.pnlSqlConnect.Name = "pnlSqlConnect";
            this.pnlSqlConnect.Size = new System.Drawing.Size(331, 563);
            this.pnlSqlConnect.TabIndex = 9;
            this.pnlSqlConnect.Visible = false;
            // 
            // btnCancelChangeSource
            // 
            this.btnCancelChangeSource.Location = new System.Drawing.Point(206, 441);
            this.btnCancelChangeSource.Name = "btnCancelChangeSource";
            this.btnCancelChangeSource.Size = new System.Drawing.Size(75, 23);
            this.btnCancelChangeSource.TabIndex = 8;
            this.btnCancelChangeSource.Text = "Cancel";
            this.btnCancelChangeSource.UseVisualStyleBackColor = true;
            // 
            // btnChangeSource
            // 
            this.btnChangeSource.Location = new System.Drawing.Point(84, 441);
            this.btnChangeSource.Name = "btnChangeSource";
            this.btnChangeSource.Size = new System.Drawing.Size(116, 23);
            this.btnChangeSource.TabIndex = 7;
            this.btnChangeSource.Text = "Change Source";
            this.btnChangeSource.UseVisualStyleBackColor = true;
            this.btnChangeSource.Click += new System.EventHandler(this.btnChangeSource_Click);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.tableLayoutPanel1);
            this.panel3.Controls.Add(this.groupBox1);
            this.panel3.Controls.Add(this.button1);
            this.panel3.Controls.Add(this.txtBackoutPackage);
            this.panel3.Controls.Add(this.label7);
            this.panel3.Controls.Add(this.btnCancel);
            this.panel3.Controls.Add(this.btnCreate);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel3.Location = new System.Drawing.Point(0, 158);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(742, 531);
            this.panel3.TabIndex = 10;
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.label12);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.chkDropRoutines);
            this.groupBox1.Controls.Add(this.chkRemoveNewScripts);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.chkManualAsRunOnce);
            this.groupBox1.Location = new System.Drawing.Point(21, 394);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(697, 90);
            this.groupBox1.TabIndex = 15;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Back Out Creation Settings";
            // 
            // label12
            // 
            this.label12.AutoSize = true;
            this.label12.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label12.Location = new System.Drawing.Point(350, 19);
            this.label12.Name = "label12";
            this.label12.Size = new System.Drawing.Size(121, 13);
            this.label12.TabIndex = 17;
            this.label12.Text = "\"Manually Created\":";
            this.label12.Click += new System.EventHandler(this.label12_Click);
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label11.Location = new System.Drawing.Point(33, 19);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(141, 13);
            this.label11.TabIndex = 16;
            this.label11.Text = "\"Not Found on Target\":";
            // 
            // chkDropRoutines
            // 
            this.chkDropRoutines.AutoSize = true;
            this.chkDropRoutines.Checked = true;
            this.chkDropRoutines.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkDropRoutines.Location = new System.Drawing.Point(36, 36);
            this.chkDropRoutines.Name = "chkDropRoutines";
            this.chkDropRoutines.Size = new System.Drawing.Size(275, 17);
            this.chkDropRoutines.TabIndex = 15;
            this.chkDropRoutines.Text = "Script object \"DROP\" for routines, triggers and views";
            this.toolTip1.SetToolTip(this.chkDropRoutines, resources.GetString("chkDropRoutines.ToolTip"));
            this.chkDropRoutines.UseVisualStyleBackColor = true;
            // 
            // chkRemoveNewScripts
            // 
            this.chkRemoveNewScripts.AutoSize = true;
            this.chkRemoveNewScripts.Checked = true;
            this.chkRemoveNewScripts.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRemoveNewScripts.Location = new System.Drawing.Point(36, 56);
            this.chkRemoveNewScripts.Name = "chkRemoveNewScripts";
            this.chkRemoveNewScripts.Size = new System.Drawing.Size(167, 17);
            this.chkRemoveNewScripts.TabIndex = 12;
            this.chkRemoveNewScripts.Text = "Remove scripts from package";
            this.toolTip1.SetToolTip(this.chkRemoveNewScripts, "Since \"Not Found on Target\" non-routine objects are new, they should be manually " +
                    "altered to ensure a proper rollback\r\nUnchecking this box will leave the scripts " +
                    "but mark them as \"Run Once\"");
            this.chkRemoveNewScripts.UseVisualStyleBackColor = true;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(370, 60);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(293, 13);
            this.label10.TabIndex = 14;
            this.label10.Text = "(manual scripts with a build order >= 1000 will not be marked)";
            // 
            // chkManualAsRunOnce
            // 
            this.chkManualAsRunOnce.AutoSize = true;
            this.chkManualAsRunOnce.Checked = true;
            this.chkManualAsRunOnce.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkManualAsRunOnce.Location = new System.Drawing.Point(353, 41);
            this.chkManualAsRunOnce.Name = "chkManualAsRunOnce";
            this.chkManualAsRunOnce.Size = new System.Drawing.Size(162, 17);
            this.chkManualAsRunOnce.TabIndex = 13;
            this.chkManualAsRunOnce.Text = "Mark scripts as \"Run Once\" ";
            this.toolTip1.SetToolTip(this.chkManualAsRunOnce, "This will ensure that these scripts are not executed again during a rollback.\r\nIf" +
                    " these need to be modified so they do execute a rollback change, uncheck this bo" +
                    "x.");
            this.chkManualAsRunOnce.UseVisualStyleBackColor = true;
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(353, 0);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(302, 26);
            this.label9.TabIndex = 11;
            this.label9.Text = "Scipts that will NOT be updated: \r\n(Use options below to control how they are tre" +
                "ated)";
            this.label9.Click += new System.EventHandler(this.label9_Click);
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label8.Location = new System.Drawing.Point(3, 0);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(162, 13);
            this.label8.TabIndex = 10;
            this.label8.Text = "Scipts that will be updated:";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Image = global::SqlSync.Properties.Resources.Open;
            this.button1.Location = new System.Drawing.Point(662, 9);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(28, 23);
            this.button1.TabIndex = 9;
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // txtBackoutPackage
            // 
            this.txtBackoutPackage.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtBackoutPackage.Location = new System.Drawing.Point(247, 11);
            this.txtBackoutPackage.Name = "txtBackoutPackage";
            this.txtBackoutPackage.Size = new System.Drawing.Size(409, 20);
            this.txtBackoutPackage.TabIndex = 8;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(52, 14);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(199, 13);
            this.label7.TabIndex = 7;
            this.label7.Text = "Back out Package File Name: ";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "sbm";
            this.saveFileDialog1.Filter = "SBM Package *.sbm|*.sbm|All Files *.*|*.*";
            this.saveFileDialog1.Title = "Save Backout Package";
            // 
            // bgMakeBackout
            // 
            this.bgMakeBackout.WorkerReportsProgress = true;
            this.bgMakeBackout.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgMakeBackout_DoWork);
            this.bgMakeBackout.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgMakeBackout_ProgressChanged);
            this.bgMakeBackout.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgMakeBackout_RunWorkerCompleted);
            // 
            // bgCheckTargetObjects
            // 
            this.bgCheckTargetObjects.WorkerReportsProgress = true;
            this.bgCheckTargetObjects.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgCheckTargetObjects_DoWork);
            this.bgCheckTargetObjects.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgCheckTargetObjects_ProgressChanged);
            this.bgCheckTargetObjects.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgCheckTargetObjects_RunWorkerCompleted);
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 10000;
            this.toolTip1.InitialDelay = 500;
            this.toolTip1.IsBalloon = true;
            this.toolTip1.ReshowDelay = 100;
            this.toolTip1.ToolTipIcon = System.Windows.Forms.ToolTipIcon.Info;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.Controls.Add(this.lstObjectNotUpdateable, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.lstObjectsToUpdate, 0, 1);
            this.tableLayoutPanel1.Controls.Add(this.label8, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.label9, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.lnkCopyList, 1, 2);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(21, 50);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 3;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(700, 338);
            this.tableLayoutPanel1.TabIndex = 16;
            // 
            // viewLogFileMenuItem1
            // 
            this.viewLogFileMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("viewLogFileMenuItem1.Image")));
            this.viewLogFileMenuItem1.Name = "viewLogFileMenuItem1";
            this.viewLogFileMenuItem1.Size = new System.Drawing.Size(201, 22);
            this.viewLogFileMenuItem1.Text = "View Application Log File";
            // 
            // setLoggingLevelMenuItem1
            // 
            this.setLoggingLevelMenuItem1.Name = "setLoggingLevelMenuItem1";
            this.setLoggingLevelMenuItem1.Size = new System.Drawing.Size(201, 22);
            this.setLoggingLevelMenuItem1.Text = "Set Logging Level";
            // 
            // sqlConnect1
            // 
            this.sqlConnect1.DisplayDatabaseDropDown = true;
            this.sqlConnect1.Location = new System.Drawing.Point(48, 16);
            this.sqlConnect1.Name = "sqlConnect1";
            this.sqlConnect1.Size = new System.Drawing.Size(264, 419);
            this.sqlConnect1.TabIndex = 6;
            // 
            // lnkCopyList
            // 
            this.lnkCopyList.Dock = System.Windows.Forms.DockStyle.Fill;
            this.lnkCopyList.Location = new System.Drawing.Point(353, 318);
            this.lnkCopyList.Name = "lnkCopyList";
            this.lnkCopyList.Size = new System.Drawing.Size(344, 20);
            this.lnkCopyList.TabIndex = 12;
            this.lnkCopyList.TabStop = true;
            this.lnkCopyList.Text = "Copy List to Clipboard";
            this.lnkCopyList.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lnkCopyList.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkCopyList_LinkClicked);
            // 
            // BackoutPackageForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(742, 711);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.pnlSqlConnect);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "BackoutPackageForm";
            this.Text = "Create Back Out Package";
            this.Load += new System.EventHandler(this.BackoutPackageForm_Load);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.contextMenuStrip1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.pnlSqlConnect.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ListView lstObjectsToUpdate;
        private System.Windows.Forms.ListView lstObjectNotUpdateable;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.Button btnCreate;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ToolStripMenuItem actionToolStripMenuItem;
        private System.Windows.Forms.Label lblServerSetting;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripMenuItem mnuChangeSqlServer;
        private System.Windows.Forms.Panel pnlSqlConnect;
        private System.Windows.Forms.Button btnCancelChangeSource;
        private System.Windows.Forms.Button btnChangeSource;
        private SQLConnect sqlConnect1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label lblDatabaseSetting;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TextBox txtBackoutPackage;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem viewScriptToolStripMenuItem;
        private System.ComponentModel.BackgroundWorker bgMakeBackout;
        private System.Windows.Forms.ToolStripStatusLabel statGeneral;
        private System.Windows.Forms.ToolStripProgressBar statBar;
        private System.ComponentModel.BackgroundWorker bgCheckTargetObjects;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private Controls.ViewLogFileMenuItem viewLogFileMenuItem1;
        private Controls.SetLoggingLevelMenuItem setLoggingLevelMenuItem1;
        private System.Windows.Forms.CheckBox chkManualAsRunOnce;
        private System.Windows.Forms.CheckBox chkRemoveNewScripts;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.Label label12;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.CheckBox chkDropRoutines;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.LinkLabel lnkCopyList;
    }
}