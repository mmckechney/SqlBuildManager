namespace SqlSync.SqlBuild.Remote
{
    partial class RemoteServiceForm
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
            System.Windows.Forms.Button btnMultDbCfg;
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(RemoteServiceForm));
            this.dgvRemoteServers = new System.Windows.Forms.DataGridView();
            this.ServerColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.actionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.manageServerSetsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openRemoteExecutionServerPackagerespToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.saveExecutionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.helptoolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.activeProtocolToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.protocolComboBox = new System.Windows.Forms.ToolStripComboBox();
            this.btnCheckServiceStatus = new System.Windows.Forms.Button();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.groupBox9 = new System.Windows.Forms.GroupBox();
            this.btnTestConnections = new System.Windows.Forms.Button();
            this.btnCommandLine = new System.Windows.Forms.Button();
            this.groupBox8 = new System.Windows.Forms.GroupBox();
            this.btnPreview = new System.Windows.Forms.Button();
            this.ddDistribution = new System.Windows.Forms.ComboBox();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.label1 = new System.Windows.Forms.Label();
            this.txtTimeoutRetryCount = new System.Windows.Forms.TextBox();
            this.groupBox6 = new System.Windows.Forms.GroupBox();
            this.txtLoggingDatabase = new System.Windows.Forms.TextBox();
            this.groupBox7 = new System.Windows.Forms.GroupBox();
            this.label9 = new System.Windows.Forms.Label();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.txtUserName = new System.Windows.Forms.TextBox();
            this.chkUseWindowsAuth = new System.Windows.Forms.CheckBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.chkUseOverrideAsExeList = new System.Windows.Forms.CheckBox();
            this.lblOpenConfigForm = new System.Windows.Forms.LinkLabel();
            this.lblCreateViaQuery = new System.Windows.Forms.LinkLabel();
            this.txtOverride = new System.Windows.Forms.TextBox();
            this.label6 = new System.Windows.Forms.Label();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.btnOpenDacpac = new System.Windows.Forms.Button();
            this.txtPlatinumDacpac = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.lnkViewSbmPackage = new System.Windows.Forms.LinkLabel();
            this.btnOpenSbm = new System.Windows.Forms.Button();
            this.txtSbmFile = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.chkRunTrial = new System.Windows.Forms.CheckBox();
            this.txtRootLoggingPath = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.chkNotTransactional = new System.Windows.Forms.CheckBox();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.btnSubmitPackage = new System.Windows.Forms.Button();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.dgvServerStatus = new System.Windows.Forms.DataGridView();
            this.serverNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.serviceReadinessDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.lastStatusCheckDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.executionReturnDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ServiceVersion = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.TcpServiceEndpoint = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.remoteExecutionLogsContextMenuStrip1 = new SqlSync.Controls.RemoteExecutionLogsContextMenuStrip();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.serverConfigDataBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statGeneral = new System.Windows.Forms.ToolStripStatusLabel();
            this.statProgBar = new System.Windows.Forms.ToolStripProgressBar();
            this.fileSbm = new System.Windows.Forms.OpenFileDialog();
            this.fileOverride = new System.Windows.Forms.OpenFileDialog();
            this.bgStatusCheck = new System.ComponentModel.BackgroundWorker();
            this.bgSubmit = new System.ComponentModel.BackgroundWorker();
            this.tmrCheckStatus = new System.Windows.Forms.Timer(this.components);
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog2 = new System.Windows.Forms.SaveFileDialog();
            this.bgConnectionTest = new System.ComponentModel.BackgroundWorker();
            this.fileDacPac = new System.Windows.Forms.OpenFileDialog();
            btnMultDbCfg = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dgvRemoteServers)).BeginInit();
            this.menuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox9.SuspendLayout();
            this.groupBox8.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox6.SuspendLayout();
            this.groupBox7.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgvServerStatus)).BeginInit();
            this.remoteExecutionLogsContextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.serverConfigDataBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnMultDbCfg
            // 
            btnMultDbCfg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            btnMultDbCfg.AutoSize = true;
            btnMultDbCfg.Image = global::SqlSync.Properties.Resources.Open;
            btnMultDbCfg.Location = new System.Drawing.Point(728, 17);
            btnMultDbCfg.Name = "btnMultDbCfg";
            btnMultDbCfg.Size = new System.Drawing.Size(28, 22);
            btnMultDbCfg.TabIndex = 9;
            btnMultDbCfg.UseVisualStyleBackColor = true;
            btnMultDbCfg.Click += new System.EventHandler(this.btnMultDbCfg_Click);
            // 
            // dgvRemoteServers
            // 
            this.dgvRemoteServers.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvRemoteServers.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvRemoteServers.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.ServerColumn});
            this.dgvRemoteServers.Location = new System.Drawing.Point(6, 19);
            this.dgvRemoteServers.Name = "dgvRemoteServers";
            this.dgvRemoteServers.RowHeadersVisible = false;
            this.dgvRemoteServers.Size = new System.Drawing.Size(251, 318);
            this.dgvRemoteServers.TabIndex = 0;
            // 
            // ServerColumn
            // 
            this.ServerColumn.HeaderText = "Remote Execution Server";
            this.ServerColumn.Name = "ServerColumn";
            this.ServerColumn.Width = 200;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.actionToolStripMenuItem,
            this.helptoolStripMenuItem,
            this.activeProtocolToolStripMenuItem,
            this.protocolComboBox});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(1084, 27);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // actionToolStripMenuItem
            // 
            this.actionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.manageServerSetsToolStripMenuItem,
            this.openRemoteExecutionServerPackagerespToolStripMenuItem,
            this.toolStripSeparator2,
            this.saveExecutionToolStripMenuItem});
            this.actionToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Execute;
            this.actionToolStripMenuItem.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.actionToolStripMenuItem.Name = "actionToolStripMenuItem";
            this.actionToolStripMenuItem.Size = new System.Drawing.Size(70, 23);
            this.actionToolStripMenuItem.Text = "&Action";
            // 
            // manageServerSetsToolStripMenuItem
            // 
            this.manageServerSetsToolStripMenuItem.Image = global::SqlSync.Properties.Resources.edit;
            this.manageServerSetsToolStripMenuItem.Name = "manageServerSetsToolStripMenuItem";
            this.manageServerSetsToolStripMenuItem.Size = new System.Drawing.Size(319, 22);
            this.manageServerSetsToolStripMenuItem.Text = "Manage Server Sets";
            this.manageServerSetsToolStripMenuItem.Click += new System.EventHandler(this.manageServerSetsToolStripMenuItem_Click);
            // 
            // openRemoteExecutionServerPackagerespToolStripMenuItem
            // 
            this.openRemoteExecutionServerPackagerespToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Open;
            this.openRemoteExecutionServerPackagerespToolStripMenuItem.Name = "openRemoteExecutionServerPackagerespToolStripMenuItem";
            this.openRemoteExecutionServerPackagerespToolStripMenuItem.Size = new System.Drawing.Size(319, 22);
            this.openRemoteExecutionServerPackagerespToolStripMenuItem.Text = "Open Remote Execution Server Package (.resp)";
            this.openRemoteExecutionServerPackagerespToolStripMenuItem.Click += new System.EventHandler(this.openRemoteExecutionServerPackagerespToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(316, 6);
            // 
            // saveExecutionToolStripMenuItem
            // 
            this.saveExecutionToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Save;
            this.saveExecutionToolStripMenuItem.Name = "saveExecutionToolStripMenuItem";
            this.saveExecutionToolStripMenuItem.Size = new System.Drawing.Size(319, 22);
            this.saveExecutionToolStripMenuItem.Text = "Save Remote Execution Server Package (.resp)";
            this.saveExecutionToolStripMenuItem.Click += new System.EventHandler(this.saveExecutionToolStripMenuItem_Click);
            // 
            // helptoolStripMenuItem
            // 
            this.helptoolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.helptoolStripMenuItem.Image = global::SqlSync.Properties.Resources.Help_2;
            this.helptoolStripMenuItem.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.helptoolStripMenuItem.Name = "helptoolStripMenuItem";
            this.helptoolStripMenuItem.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.helptoolStripMenuItem.Size = new System.Drawing.Size(28, 23);
            this.helptoolStripMenuItem.Click += new System.EventHandler(this.helptoolStripMenuItem_Click);
            // 
            // activeProtocolToolStripMenuItem
            // 
            this.activeProtocolToolStripMenuItem.Name = "activeProtocolToolStripMenuItem";
            this.activeProtocolToolStripMenuItem.Size = new System.Drawing.Size(103, 23);
            this.activeProtocolToolStripMenuItem.Text = "Active Protocol:";
            // 
            // protocolComboBox
            // 
            this.protocolComboBox.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.protocolComboBox.Items.AddRange(new object[] {
            "Tcp",
            "Http",
            "Azure-Http"});
            this.protocolComboBox.Name = "protocolComboBox";
            this.protocolComboBox.Size = new System.Drawing.Size(121, 23);
            this.protocolComboBox.SelectedIndexChanged += new System.EventHandler(this.protocolComboBox_SelectedIndexChanged);
            // 
            // btnCheckServiceStatus
            // 
            this.btnCheckServiceStatus.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnCheckServiceStatus.Location = new System.Drawing.Point(6, 19);
            this.btnCheckServiceStatus.Name = "btnCheckServiceStatus";
            this.btnCheckServiceStatus.Size = new System.Drawing.Size(123, 23);
            this.btnCheckServiceStatus.TabIndex = 0;
            this.btnCheckServiceStatus.Text = "Check Service Status";
            this.btnCheckServiceStatus.UseVisualStyleBackColor = true;
            this.btnCheckServiceStatus.Click += new System.EventHandler(this.btnCheckServiceStatus_Click);
            this.btnCheckServiceStatus.MouseClick += new System.Windows.Forms.MouseEventHandler(this.btnCheckServiceStatus_MouseClick);
            this.btnCheckServiceStatus.MouseDown += new System.Windows.Forms.MouseEventHandler(this.btnCheckServiceStatus_MouseClick);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.groupBox2);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.btnCommandLine);
            this.splitContainer1.Panel2.Controls.Add(this.groupBox8);
            this.splitContainer1.Panel2.Controls.Add(this.groupBox3);
            this.splitContainer1.Panel2.Controls.Add(this.btnSubmitPackage);
            this.splitContainer1.Size = new System.Drawing.Size(1084, 415);
            this.splitContainer1.SplitterDistance = 279;
            this.splitContainer1.TabIndex = 3;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.groupBox9);
            this.groupBox2.Controls.Add(this.dgvRemoteServers);
            this.groupBox2.Location = new System.Drawing.Point(8, 7);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(263, 404);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Remote Servers";
            // 
            // groupBox9
            // 
            this.groupBox9.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.groupBox9.Controls.Add(this.btnTestConnections);
            this.groupBox9.Controls.Add(this.btnCheckServiceStatus);
            this.groupBox9.Location = new System.Drawing.Point(6, 343);
            this.groupBox9.Name = "groupBox9";
            this.groupBox9.Size = new System.Drawing.Size(251, 55);
            this.groupBox9.TabIndex = 4;
            this.groupBox9.TabStop = false;
            this.groupBox9.Text = "Server Validation";
            // 
            // btnTestConnections
            // 
            this.btnTestConnections.Anchor = System.Windows.Forms.AnchorStyles.None;
            this.btnTestConnections.Location = new System.Drawing.Point(139, 19);
            this.btnTestConnections.Name = "btnTestConnections";
            this.btnTestConnections.Size = new System.Drawing.Size(105, 23);
            this.btnTestConnections.TabIndex = 1;
            this.btnTestConnections.Text = "Test Connections";
            this.btnTestConnections.UseVisualStyleBackColor = true;
            this.btnTestConnections.Click += new System.EventHandler(this.btnTestConnections_Click);
            // 
            // btnCommandLine
            // 
            this.btnCommandLine.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnCommandLine.Location = new System.Drawing.Point(632, 387);
            this.btnCommandLine.Name = "btnCommandLine";
            this.btnCommandLine.Size = new System.Drawing.Size(136, 23);
            this.btnCommandLine.TabIndex = 1;
            this.btnCommandLine.Text = "Create Command Line";
            this.btnCommandLine.UseVisualStyleBackColor = true;
            this.btnCommandLine.Click += new System.EventHandler(this.btnCommandLine_Click);
            // 
            // groupBox8
            // 
            this.groupBox8.Controls.Add(this.btnPreview);
            this.groupBox8.Controls.Add(this.ddDistribution);
            this.groupBox8.Location = new System.Drawing.Point(3, 336);
            this.groupBox8.Name = "groupBox8";
            this.groupBox8.Size = new System.Drawing.Size(783, 45);
            this.groupBox8.TabIndex = 5;
            this.groupBox8.TabStop = false;
            this.groupBox8.Text = "Workload Distribution";
            // 
            // btnPreview
            // 
            this.btnPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnPreview.Enabled = false;
            this.btnPreview.Location = new System.Drawing.Point(647, 13);
            this.btnPreview.Name = "btnPreview";
            this.btnPreview.Size = new System.Drawing.Size(119, 23);
            this.btnPreview.TabIndex = 1;
            this.btnPreview.Text = "Preview Distribution";
            this.btnPreview.UseVisualStyleBackColor = true;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            // 
            // ddDistribution
            // 
            this.ddDistribution.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddDistribution.FormattingEnabled = true;
            this.ddDistribution.Items.AddRange(new object[] {
            "Equally distribute load across execution servers",
            "Each execution server handles only its local load (matches host names)"});
            this.ddDistribution.Location = new System.Drawing.Point(157, 15);
            this.ddDistribution.Name = "ddDistribution";
            this.ddDistribution.Size = new System.Drawing.Size(469, 21);
            this.ddDistribution.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Controls.Add(this.label1);
            this.groupBox3.Controls.Add(this.txtTimeoutRetryCount);
            this.groupBox3.Controls.Add(this.groupBox6);
            this.groupBox3.Controls.Add(this.groupBox7);
            this.groupBox3.Controls.Add(this.groupBox5);
            this.groupBox3.Controls.Add(this.groupBox4);
            this.groupBox3.Controls.Add(this.label10);
            this.groupBox3.Controls.Add(this.chkRunTrial);
            this.groupBox3.Controls.Add(this.txtRootLoggingPath);
            this.groupBox3.Controls.Add(this.label2);
            this.groupBox3.Controls.Add(this.chkNotTransactional);
            this.groupBox3.Controls.Add(this.txtDescription);
            this.groupBox3.Location = new System.Drawing.Point(3, 7);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(783, 326);
            this.groupBox3.TabIndex = 4;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Execution Settings";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(305, 73);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(177, 13);
            this.label1.TabIndex = 26;
            this.label1.Text = "Allowed Script Timeout Retry Count:";
            // 
            // txtTimeoutRetryCount
            // 
            this.txtTimeoutRetryCount.Location = new System.Drawing.Point(493, 70);
            this.txtTimeoutRetryCount.Name = "txtTimeoutRetryCount";
            this.txtTimeoutRetryCount.Size = new System.Drawing.Size(42, 20);
            this.txtTimeoutRetryCount.TabIndex = 4;
            this.txtTimeoutRetryCount.Text = "0";
            // 
            // groupBox6
            // 
            this.groupBox6.Controls.Add(this.txtLoggingDatabase);
            this.groupBox6.Location = new System.Drawing.Point(592, 268);
            this.groupBox6.Name = "groupBox6";
            this.groupBox6.Size = new System.Drawing.Size(180, 48);
            this.groupBox6.TabIndex = 24;
            this.groupBox6.TabStop = false;
            this.groupBox6.Text = "Alternate Logging Database";
            // 
            // txtLoggingDatabase
            // 
            this.txtLoggingDatabase.Location = new System.Drawing.Point(6, 19);
            this.txtLoggingDatabase.Name = "txtLoggingDatabase";
            this.txtLoggingDatabase.Size = new System.Drawing.Size(148, 20);
            this.txtLoggingDatabase.TabIndex = 0;
            // 
            // groupBox7
            // 
            this.groupBox7.Controls.Add(this.label9);
            this.groupBox7.Controls.Add(this.txtPassword);
            this.groupBox7.Controls.Add(this.label8);
            this.groupBox7.Controls.Add(this.txtUserName);
            this.groupBox7.Controls.Add(this.chkUseWindowsAuth);
            this.groupBox7.Location = new System.Drawing.Point(10, 267);
            this.groupBox7.Name = "groupBox7";
            this.groupBox7.Size = new System.Drawing.Size(576, 50);
            this.groupBox7.TabIndex = 5;
            this.groupBox7.TabStop = false;
            this.groupBox7.Text = "Database Authentication Settings";
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(358, 22);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(56, 13);
            this.label9.TabIndex = 4;
            this.label9.Text = "Password:";
            // 
            // txtPassword
            // 
            this.txtPassword.Enabled = false;
            this.txtPassword.Location = new System.Drawing.Point(428, 18);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(100, 20);
            this.txtPassword.TabIndex = 1;
            this.txtPassword.UseSystemPasswordChar = true;
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(176, 22);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(63, 13);
            this.label8.TabIndex = 2;
            this.label8.Text = "User Name:";
            // 
            // txtUserName
            // 
            this.txtUserName.Enabled = false;
            this.txtUserName.Location = new System.Drawing.Point(246, 18);
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new System.Drawing.Size(100, 20);
            this.txtUserName.TabIndex = 0;
            // 
            // chkUseWindowsAuth
            // 
            this.chkUseWindowsAuth.AutoSize = true;
            this.chkUseWindowsAuth.Checked = true;
            this.chkUseWindowsAuth.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkUseWindowsAuth.Location = new System.Drawing.Point(12, 20);
            this.chkUseWindowsAuth.Name = "chkUseWindowsAuth";
            this.chkUseWindowsAuth.Size = new System.Drawing.Size(163, 17);
            this.chkUseWindowsAuth.TabIndex = 0;
            this.chkUseWindowsAuth.Text = "Use Windows Authentication";
            this.chkUseWindowsAuth.UseVisualStyleBackColor = true;
            this.chkUseWindowsAuth.CheckedChanged += new System.EventHandler(this.chkUseWindowsAuth_CheckedChanged);
            // 
            // groupBox5
            // 
            this.groupBox5.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox5.Controls.Add(this.chkUseOverrideAsExeList);
            this.groupBox5.Controls.Add(this.lblOpenConfigForm);
            this.groupBox5.Controls.Add(this.lblCreateViaQuery);
            this.groupBox5.Controls.Add(btnMultDbCfg);
            this.groupBox5.Controls.Add(this.txtOverride);
            this.groupBox5.Controls.Add(this.label6);
            this.groupBox5.Location = new System.Drawing.Point(10, 187);
            this.groupBox5.Margin = new System.Windows.Forms.Padding(0);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(762, 74);
            this.groupBox5.TabIndex = 14;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Override Target Settings";
            // 
            // chkUseOverrideAsExeList
            // 
            this.chkUseOverrideAsExeList.AutoSize = true;
            this.chkUseOverrideAsExeList.CheckAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.chkUseOverrideAsExeList.Checked = true;
            this.chkUseOverrideAsExeList.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkUseOverrideAsExeList.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkUseOverrideAsExeList.ForeColor = System.Drawing.Color.Black;
            this.chkUseOverrideAsExeList.Location = new System.Drawing.Point(384, 47);
            this.chkUseOverrideAsExeList.Name = "chkUseOverrideAsExeList";
            this.chkUseOverrideAsExeList.Size = new System.Drawing.Size(337, 17);
            this.chkUseOverrideAsExeList.TabIndex = 1;
            this.chkUseOverrideAsExeList.Text = "Derive Remote Execution Server list from Override Target Settings";
            this.chkUseOverrideAsExeList.UseVisualStyleBackColor = true;
            this.chkUseOverrideAsExeList.CheckedChanged += new System.EventHandler(this.chkUseOverrideAsExeList_CheckedChanged);
            // 
            // lblOpenConfigForm
            // 
            this.lblOpenConfigForm.AutoSize = true;
            this.lblOpenConfigForm.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblOpenConfigForm.Location = new System.Drawing.Point(9, 48);
            this.lblOpenConfigForm.Name = "lblOpenConfigForm";
            this.lblOpenConfigForm.Size = new System.Drawing.Size(157, 13);
            this.lblOpenConfigForm.TabIndex = 10;
            this.lblOpenConfigForm.TabStop = true;
            this.lblOpenConfigForm.Text = "Open Multi-Db config form";
            this.lblOpenConfigForm.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblOpenConfigForm_LinkClicked);
            // 
            // lblCreateViaQuery
            // 
            this.lblCreateViaQuery.AutoSize = true;
            this.lblCreateViaQuery.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblCreateViaQuery.Location = new System.Drawing.Point(172, 48);
            this.lblCreateViaQuery.Name = "lblCreateViaQuery";
            this.lblCreateViaQuery.Size = new System.Drawing.Size(182, 13);
            this.lblCreateViaQuery.TabIndex = 11;
            this.lblCreateViaQuery.TabStop = true;
            this.lblCreateViaQuery.Text = "Create configuration via query";
            this.lblCreateViaQuery.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lblCreateViaQuery_LinkClicked);
            // 
            // txtOverride
            // 
            this.txtOverride.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOverride.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.txtOverride.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txtOverride.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtOverride.ForeColor = System.Drawing.Color.Black;
            this.txtOverride.Location = new System.Drawing.Point(270, 17);
            this.txtOverride.Name = "txtOverride";
            this.txtOverride.Size = new System.Drawing.Size(452, 20);
            this.txtOverride.TabIndex = 0;
            this.txtOverride.TextChanged += new System.EventHandler(this.txtOverride_TextChanged);
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 21);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(254, 13);
            this.label6.TabIndex = 7;
            this.label6.Text = "Target Override Settings (.multiDb, multiDbQ  or .cfg)";
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(this.btnOpenDacpac);
            this.groupBox4.Controls.Add(this.txtPlatinumDacpac);
            this.groupBox4.Controls.Add(this.label4);
            this.groupBox4.Controls.Add(this.lnkViewSbmPackage);
            this.groupBox4.Controls.Add(this.btnOpenSbm);
            this.groupBox4.Controls.Add(this.txtSbmFile);
            this.groupBox4.Controls.Add(this.label3);
            this.groupBox4.Location = new System.Drawing.Point(10, 87);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(762, 95);
            this.groupBox4.TabIndex = 12;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Script Source";
            // 
            // btnOpenDacpac
            // 
            this.btnOpenDacpac.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenDacpac.AutoSize = true;
            this.btnOpenDacpac.Image = global::SqlSync.Properties.Resources.Open;
            this.btnOpenDacpac.Location = new System.Drawing.Point(727, 67);
            this.btnOpenDacpac.Name = "btnOpenDacpac";
            this.btnOpenDacpac.Size = new System.Drawing.Size(28, 22);
            this.btnOpenDacpac.TabIndex = 14;
            this.btnOpenDacpac.UseVisualStyleBackColor = true;
            this.btnOpenDacpac.Click += new System.EventHandler(this.btnOpenDacpac_Click);
            // 
            // txtPlatinumDacpac
            // 
            this.txtPlatinumDacpac.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtPlatinumDacpac.Location = new System.Drawing.Point(180, 67);
            this.txtPlatinumDacpac.Name = "txtPlatinumDacpac";
            this.txtPlatinumDacpac.Size = new System.Drawing.Size(540, 20);
            this.txtPlatinumDacpac.TabIndex = 12;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(8, 71);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(151, 13);
            this.label4.TabIndex = 13;
            this.label4.Text = "Platinum dacpac (if applicable)";
            // 
            // lnkViewSbmPackage
            // 
            this.lnkViewSbmPackage.AutoSize = true;
            this.lnkViewSbmPackage.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lnkViewSbmPackage.Location = new System.Drawing.Point(635, 43);
            this.lnkViewSbmPackage.Name = "lnkViewSbmPackage";
            this.lnkViewSbmPackage.Size = new System.Drawing.Size(86, 13);
            this.lnkViewSbmPackage.TabIndex = 11;
            this.lnkViewSbmPackage.TabStop = true;
            this.lnkViewSbmPackage.Text = "View Package";
            this.lnkViewSbmPackage.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkViewSbmPackage_LinkClicked);
            // 
            // btnOpenSbm
            // 
            this.btnOpenSbm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenSbm.AutoSize = true;
            this.btnOpenSbm.Image = global::SqlSync.Properties.Resources.Open;
            this.btnOpenSbm.Location = new System.Drawing.Point(728, 17);
            this.btnOpenSbm.Name = "btnOpenSbm";
            this.btnOpenSbm.Size = new System.Drawing.Size(28, 22);
            this.btnOpenSbm.TabIndex = 9;
            this.btnOpenSbm.UseVisualStyleBackColor = true;
            this.btnOpenSbm.Click += new System.EventHandler(this.btnOpenSbm_Click);
            // 
            // txtSbmFile
            // 
            this.txtSbmFile.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtSbmFile.Location = new System.Drawing.Point(181, 17);
            this.txtSbmFile.Name = "txtSbmFile";
            this.txtSbmFile.Size = new System.Drawing.Size(540, 20);
            this.txtSbmFile.TabIndex = 0;
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 21);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(170, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Sql Build Manager Package (.sbm)";
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(393, 48);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(89, 13);
            this.label10.TabIndex = 7;
            this.label10.Text = "Build Description:";
            // 
            // chkRunTrial
            // 
            this.chkRunTrial.AutoSize = true;
            this.chkRunTrial.Location = new System.Drawing.Point(36, 21);
            this.chkRunTrial.Name = "chkRunTrial";
            this.chkRunTrial.Size = new System.Drawing.Size(158, 17);
            this.chkRunTrial.TabIndex = 0;
            this.chkRunTrial.Text = "Run as Trial (rollback) mode";
            this.chkRunTrial.UseVisualStyleBackColor = true;
            // 
            // txtRootLoggingPath
            // 
            this.txtRootLoggingPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRootLoggingPath.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.txtRootLoggingPath.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txtRootLoggingPath.Location = new System.Drawing.Point(493, 19);
            this.txtRootLoggingPath.Name = "txtRootLoggingPath";
            this.txtRootLoggingPath.Size = new System.Drawing.Size(279, 20);
            this.txtRootLoggingPath.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(228, 23);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(254, 13);
            this.label2.TabIndex = 10;
            this.label2.Text = "Root Logging path (local path on execution servers):";
            // 
            // chkNotTransactional
            // 
            this.chkNotTransactional.AutoSize = true;
            this.chkNotTransactional.Location = new System.Drawing.Point(36, 45);
            this.chkNotTransactional.Name = "chkNotTransactional";
            this.chkNotTransactional.Size = new System.Drawing.Size(173, 17);
            this.chkNotTransactional.TabIndex = 1;
            this.chkNotTransactional.Text = "Run builds without transactions";
            this.chkNotTransactional.UseVisualStyleBackColor = true;
            // 
            // txtDescription
            // 
            this.txtDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtDescription.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.txtDescription.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txtDescription.Location = new System.Drawing.Point(493, 44);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(279, 20);
            this.txtDescription.TabIndex = 3;
            // 
            // btnSubmitPackage
            // 
            this.btnSubmitPackage.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSubmitPackage.Location = new System.Drawing.Point(332, 388);
            this.btnSubmitPackage.Name = "btnSubmitPackage";
            this.btnSubmitPackage.Size = new System.Drawing.Size(136, 23);
            this.btnSubmitPackage.TabIndex = 0;
            this.btnSubmitPackage.Text = "Submit Build Request";
            this.btnSubmitPackage.UseVisualStyleBackColor = true;
            this.btnSubmitPackage.Click += new System.EventHandler(this.btnSubmitPackage_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.dgvServerStatus);
            this.groupBox1.Location = new System.Drawing.Point(7, 6);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(1065, 246);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Remove Service Status Dashboard";
            // 
            // dgvServerStatus
            // 
            this.dgvServerStatus.AllowUserToAddRows = false;
            this.dgvServerStatus.AllowUserToDeleteRows = false;
            this.dgvServerStatus.AllowUserToOrderColumns = true;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.WhiteSmoke;
            this.dgvServerStatus.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgvServerStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.dgvServerStatus.AutoGenerateColumns = false;
            this.dgvServerStatus.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dgvServerStatus.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.serverNameDataGridViewTextBoxColumn,
            this.serviceReadinessDataGridViewTextBoxColumn,
            this.lastStatusCheckDataGridViewTextBoxColumn,
            this.executionReturnDataGridViewTextBoxColumn,
            this.ServiceVersion,
            this.TcpServiceEndpoint});
            this.dgvServerStatus.ContextMenuStrip = this.remoteExecutionLogsContextMenuStrip1;
            this.dgvServerStatus.DataSource = this.serverConfigDataBindingSource;
            this.dgvServerStatus.Location = new System.Drawing.Point(11, 19);
            this.dgvServerStatus.Name = "dgvServerStatus";
            this.dgvServerStatus.ReadOnly = true;
            this.dgvServerStatus.RowHeadersVisible = false;
            this.dgvServerStatus.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.CellSelect;
            this.dgvServerStatus.Size = new System.Drawing.Size(1044, 221);
            this.dgvServerStatus.TabIndex = 0;
            this.dgvServerStatus.CellFormatting += new System.Windows.Forms.DataGridViewCellFormattingEventHandler(this.dgvServerStatus_CellFormatting);
            // 
            // serverNameDataGridViewTextBoxColumn
            // 
            this.serverNameDataGridViewTextBoxColumn.DataPropertyName = "ServerName";
            this.serverNameDataGridViewTextBoxColumn.HeaderText = "Server Name";
            this.serverNameDataGridViewTextBoxColumn.Name = "serverNameDataGridViewTextBoxColumn";
            this.serverNameDataGridViewTextBoxColumn.ReadOnly = true;
            this.serverNameDataGridViewTextBoxColumn.Width = 150;
            // 
            // serviceReadinessDataGridViewTextBoxColumn
            // 
            this.serviceReadinessDataGridViewTextBoxColumn.DataPropertyName = "ServiceReadiness";
            this.serviceReadinessDataGridViewTextBoxColumn.HeaderText = "Service Readiness";
            this.serviceReadinessDataGridViewTextBoxColumn.Name = "serviceReadinessDataGridViewTextBoxColumn";
            this.serviceReadinessDataGridViewTextBoxColumn.ReadOnly = true;
            this.serviceReadinessDataGridViewTextBoxColumn.Width = 180;
            // 
            // lastStatusCheckDataGridViewTextBoxColumn
            // 
            this.lastStatusCheckDataGridViewTextBoxColumn.DataPropertyName = "LastStatusCheck";
            dataGridViewCellStyle2.Format = "MM/dd/yyyy hh:mm:ss.fff";
            this.lastStatusCheckDataGridViewTextBoxColumn.DefaultCellStyle = dataGridViewCellStyle2;
            this.lastStatusCheckDataGridViewTextBoxColumn.HeaderText = "Last Status Check";
            this.lastStatusCheckDataGridViewTextBoxColumn.Name = "lastStatusCheckDataGridViewTextBoxColumn";
            this.lastStatusCheckDataGridViewTextBoxColumn.ReadOnly = true;
            this.lastStatusCheckDataGridViewTextBoxColumn.Width = 180;
            // 
            // executionReturnDataGridViewTextBoxColumn
            // 
            this.executionReturnDataGridViewTextBoxColumn.DataPropertyName = "ExecutionReturn";
            this.executionReturnDataGridViewTextBoxColumn.HeaderText = "Last Execution Result";
            this.executionReturnDataGridViewTextBoxColumn.Name = "executionReturnDataGridViewTextBoxColumn";
            this.executionReturnDataGridViewTextBoxColumn.ReadOnly = true;
            this.executionReturnDataGridViewTextBoxColumn.Width = 180;
            // 
            // ServiceVersion
            // 
            this.ServiceVersion.DataPropertyName = "ServiceVersion";
            this.ServiceVersion.HeaderText = "Service Version";
            this.ServiceVersion.Name = "ServiceVersion";
            this.ServiceVersion.ReadOnly = true;
            this.ServiceVersion.Width = 120;
            // 
            // TcpServiceEndpoint
            // 
            this.TcpServiceEndpoint.DataPropertyName = "ActiveServiceEndpoint";
            this.TcpServiceEndpoint.HeaderText = "Service Endpoint";
            this.TcpServiceEndpoint.Name = "TcpServiceEndpoint";
            this.TcpServiceEndpoint.ReadOnly = true;
            this.TcpServiceEndpoint.Width = 200;
            // 
            // remoteExecutionLogsContextMenuStrip1
            // 
            this.remoteExecutionLogsContextMenuStrip1.CommitsLogMenuItemText = "View Last Execution \"Commits\" log";
            this.remoteExecutionLogsContextMenuStrip1.ErrorsLogMenuItemText = "View Last Execution \"Errors\" log";
            this.remoteExecutionLogsContextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator1,
            this.toolStripMenuItem1});
            this.remoteExecutionLogsContextMenuStrip1.Name = "remoteExecutionLogsContextMenuStrip1";
            this.remoteExecutionLogsContextMenuStrip1.Size = new System.Drawing.Size(441, 201);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(437, 6);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Image = global::SqlSync.Properties.Resources.History;
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(440, 22);
            this.toolStripMenuItem1.Text = "View Build Request History for this Remote Service";
            this.toolStripMenuItem1.Click += new System.EventHandler(this.viewBuildRequestHistoryForThisRemoteServiceToolStripMenuItem_Click);
            // 
            // serverConfigDataBindingSource
            // 
            this.serverConfigDataBindingSource.AllowNew = true;
            this.serverConfigDataBindingSource.DataSource = typeof(SqlBuildManager.ServiceClient.ServerConfigData);
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
            this.splitContainer2.Location = new System.Drawing.Point(0, 27);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.splitContainer1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.groupBox1);
            this.splitContainer2.Panel2.Controls.Add(this.statusStrip1);
            this.splitContainer2.Size = new System.Drawing.Size(1084, 696);
            this.splitContainer2.SplitterDistance = 415;
            this.splitContainer2.TabIndex = 4;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statGeneral,
            this.statProgBar});
            this.statusStrip1.Location = new System.Drawing.Point(0, 255);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1084, 22);
            this.statusStrip1.TabIndex = 5;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statGeneral
            // 
            this.statGeneral.Name = "statGeneral";
            this.statGeneral.Size = new System.Drawing.Size(917, 17);
            this.statGeneral.Spring = true;
            this.statGeneral.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statProgBar
            // 
            this.statProgBar.Name = "statProgBar";
            this.statProgBar.Size = new System.Drawing.Size(150, 16);
            // 
            // fileSbm
            // 
            this.fileSbm.Filter = "Sql Build Manager *.sbm|*.sbm|Sql Build Control File *.sbx|*.sbx|All Files *.*|*." +
    "*";
            this.fileSbm.Title = "Select build package";
            // 
            // fileOverride
            // 
            this.fileOverride.Filter = "MultiDb Config Query file *.multiDbQ|*.multiDbQ|MultiDb Config file *.multidb|*.m" +
    "ultidb|Config file *.cfg|*.cfg|All Files *.*|*.*";
            this.fileOverride.Title = "Select override configuration file";
            // 
            // bgStatusCheck
            // 
            this.bgStatusCheck.WorkerReportsProgress = true;
            this.bgStatusCheck.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgStatusCheck_DoWork);
            this.bgStatusCheck.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgStatusCheck_ProgressChanged);
            this.bgStatusCheck.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgStatusCheck_RunWorkerCompleted);
            // 
            // bgSubmit
            // 
            this.bgSubmit.WorkerReportsProgress = true;
            this.bgSubmit.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgSubmit_DoWork);
            this.bgSubmit.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgSubmit_ProgressChanged);
            this.bgSubmit.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgSubmit_RunWorkerCompleted);
            // 
            // tmrCheckStatus
            // 
            this.tmrCheckStatus.Interval = 500;
            this.tmrCheckStatus.Tick += new System.EventHandler(this.tmrCheckStatus_Tick);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "resp";
            this.saveFileDialog1.Filter = "Remote Execution Server Package *.resp|*.resp|All Files *.*|*.*";
            this.saveFileDialog1.Title = "Save Remote Execution Server Package";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.DefaultExt = "resp";
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "Remote Execution Server Package *.resp|*.resp|All Files *.*|*.*";
            this.openFileDialog1.Title = "Save Remote Execution Server Package";
            // 
            // saveFileDialog2
            // 
            this.saveFileDialog2.DefaultExt = "txt";
            this.saveFileDialog2.Filter = "Text Files *.txt|*.txt|All Files *.*|*.*";
            this.saveFileDialog2.Title = "Save Remote Execution Servers to File";
            // 
            // bgConnectionTest
            // 
            this.bgConnectionTest.WorkerReportsProgress = true;
            this.bgConnectionTest.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgConnectionTest_DoWork);
            this.bgConnectionTest.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgConnectionTest_ProgressChanged);
            this.bgConnectionTest.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgConnectionTest_RunWorkerCompleted);
            // 
            // fileDacPac
            // 
            this.fileDacPac.Filter = "Data-tier application  *.dacpac|*.dacpac";
            this.fileDacPac.Title = "Select Platinum dacpac file";
            // 
            // RemoteServiceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1084, 723);
            this.Controls.Add(this.splitContainer2);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "RemoteServiceForm";
            this.Text = "Remote Execution Service";
            this.Load += new System.EventHandler(this.RemoteServiceForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgvRemoteServers)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.groupBox2.ResumeLayout(false);
            this.groupBox9.ResumeLayout(false);
            this.groupBox8.ResumeLayout(false);
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox6.ResumeLayout(false);
            this.groupBox6.PerformLayout();
            this.groupBox7.ResumeLayout(false);
            this.groupBox7.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.groupBox1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgvServerStatus)).EndInit();
            this.remoteExecutionLogsContextMenuStrip1.ResumeLayout(false);
            this.remoteExecutionLogsContextMenuStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.serverConfigDataBindingSource)).EndInit();
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            this.splitContainer2.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.DataGridView dgvRemoteServers;
        private System.Windows.Forms.DataGridViewTextBoxColumn ServerColumn;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.Button btnCheckServiceStatus;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.DataGridView dgvServerStatus;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Button btnSubmitPackage;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.CheckBox chkRunTrial;
        private System.Windows.Forms.TextBox txtRootLoggingPath;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox chkNotTransactional;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.Button btnOpenSbm;
        private System.Windows.Forms.TextBox txtSbmFile;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.TextBox txtOverride;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.GroupBox groupBox6;
        private System.Windows.Forms.TextBox txtLoggingDatabase;
        private System.Windows.Forms.GroupBox groupBox7;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.CheckBox chkUseWindowsAuth;
        private System.Windows.Forms.OpenFileDialog fileSbm;
        private System.Windows.Forms.OpenFileDialog fileOverride;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statGeneral;
        private System.Windows.Forms.ToolStripProgressBar statProgBar;
        private System.ComponentModel.BackgroundWorker bgStatusCheck;
        //private System.Windows.Forms.DataGridViewTextBoxColumn serviceStatusDataGridViewTextBoxColumn;
        private System.Windows.Forms.BindingSource serverConfigDataBindingSource;
        private System.ComponentModel.BackgroundWorker bgSubmit;
        private System.Windows.Forms.Timer tmrCheckStatus;
        private System.Windows.Forms.GroupBox groupBox8;
        private System.Windows.Forms.ComboBox ddDistribution;
        private System.Windows.Forms.ToolStripMenuItem actionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem manageServerSetsToolStripMenuItem;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.LinkLabel lblCreateViaQuery;
        private System.Windows.Forms.LinkLabel lblOpenConfigForm;
        private System.Windows.Forms.ToolStripMenuItem helptoolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem saveExecutionToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ToolStripMenuItem openRemoteExecutionServerPackagerespToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.LinkLabel lnkViewSbmPackage;
        private System.Windows.Forms.CheckBox chkUseOverrideAsExeList;
        private System.Windows.Forms.Button btnCommandLine;
        private System.Windows.Forms.SaveFileDialog saveFileDialog2;
        private System.Windows.Forms.GroupBox groupBox9;
        private System.Windows.Forms.Button btnTestConnections;
        private System.ComponentModel.BackgroundWorker bgConnectionTest;
        private Controls.RemoteExecutionLogsContextMenuStrip remoteExecutionLogsContextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.TextBox txtTimeoutRetryCount;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolStripMenuItem activeProtocolToolStripMenuItem;
        private System.Windows.Forms.ToolStripComboBox protocolComboBox;
        private System.Windows.Forms.DataGridViewTextBoxColumn serverNameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn serviceReadinessDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn lastStatusCheckDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn executionReturnDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ServiceVersion;
        private System.Windows.Forms.DataGridViewTextBoxColumn TcpServiceEndpoint;
        private System.Windows.Forms.Button btnOpenDacpac;
        private System.Windows.Forms.TextBox txtPlatinumDacpac;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.OpenFileDialog fileDacPac;
    }
}