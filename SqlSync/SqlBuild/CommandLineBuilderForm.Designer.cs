namespace SqlSync.SqlBuild
{
    partial class CommandLineBuilderForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(CommandLineBuilderForm));
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.txtAllowedTimeoutRetries = new System.Windows.Forms.TextBox();
            this.label11 = new System.Windows.Forms.Label();
            this.chkNotTransactional = new System.Windows.Forms.CheckBox();
            this.txtDescription = new System.Windows.Forms.TextBox();
            this.label10 = new System.Windows.Forms.Label();
            this.chkRunTrial = new System.Windows.Forms.CheckBox();
            this.chkRunThreaded = new System.Windows.Forms.CheckBox();
            this.ddLogStyle = new System.Windows.Forms.ComboBox();
            this.label1 = new System.Windows.Forms.Label();
            this.grpLogging = new System.Windows.Forms.GroupBox();
            this.btnLoggingPath = new System.Windows.Forms.Button();
            this.txtRootLoggingPath = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.label8 = new System.Windows.Forms.Label();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.btnScriptSrcDir = new System.Windows.Forms.Button();
            this.txtScriptSrcDir = new System.Windows.Forms.TextBox();
            this.btnOpenSbm = new System.Windows.Forms.Button();
            this.txtSbmFile = new System.Windows.Forms.TextBox();
            this.groupBox4 = new System.Windows.Forms.GroupBox();
            this.txtOverride = new System.Windows.Forms.TextBox();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.fileSbm = new System.Windows.Forms.OpenFileDialog();
            this.fileOverride = new System.Windows.Forms.OpenFileDialog();
            this.btnConstructCommand = new System.Windows.Forms.Button();
            this.rtbCommandLine = new System.Windows.Forms.RichTextBox();
            this.btnExecute = new System.Windows.Forms.Button();
            this.rtbOutput = new System.Windows.Forms.RichTextBox();
            this.btnOpenFolder = new System.Windows.Forms.Button();
            this.pnlOutput = new System.Windows.Forms.Panel();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ddAuthentication = new System.Windows.Forms.ComboBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.txtUserName = new System.Windows.Forms.TextBox();
            this.groupBox5 = new System.Windows.Forms.GroupBox();
            this.txtLoggingDatabase = new System.Windows.Forms.TextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.helptoolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            btnMultDbCfg = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            this.grpLogging.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox4.SuspendLayout();
            this.pnlOutput.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.groupBox5.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnMultDbCfg
            // 
            btnMultDbCfg.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            btnMultDbCfg.AutoSize = true;
            btnMultDbCfg.Image = global::SqlSync.Properties.Resources.Open;
            btnMultDbCfg.Location = new System.Drawing.Point(759, 17);
            btnMultDbCfg.Name = "btnMultDbCfg";
            btnMultDbCfg.Size = new System.Drawing.Size(28, 22);
            btnMultDbCfg.TabIndex = 9;
            btnMultDbCfg.UseVisualStyleBackColor = true;
            btnMultDbCfg.Click += new System.EventHandler(this.btnMultDbCfg_Click);
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.txtAllowedTimeoutRetries);
            this.groupBox1.Controls.Add(this.label11);
            this.groupBox1.Controls.Add(this.chkNotTransactional);
            this.groupBox1.Controls.Add(this.txtDescription);
            this.groupBox1.Controls.Add(this.label10);
            this.groupBox1.Controls.Add(this.chkRunTrial);
            this.groupBox1.Controls.Add(this.chkRunThreaded);
            this.groupBox1.Location = new System.Drawing.Point(10, 28);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(271, 133);
            this.groupBox1.TabIndex = 0;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Run Settings";
            // 
            // txtAllowedTimeoutRetries
            // 
            this.txtAllowedTimeoutRetries.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.txtAllowedTimeoutRetries.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txtAllowedTimeoutRetries.Location = new System.Drawing.Point(229, 81);
            this.txtAllowedTimeoutRetries.Name = "txtAllowedTimeoutRetries";
            this.txtAllowedTimeoutRetries.Size = new System.Drawing.Size(36, 20);
            this.txtAllowedTimeoutRetries.TabIndex = 25;
            this.txtAllowedTimeoutRetries.Text = "0";
            this.txtAllowedTimeoutRetries.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            // 
            // label11
            // 
            this.label11.AutoSize = true;
            this.label11.Location = new System.Drawing.Point(6, 85);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(177, 13);
            this.label11.TabIndex = 24;
            this.label11.Text = "Allowed Script Timeout Retry Count:";
            // 
            // chkNotTransactional
            // 
            this.chkNotTransactional.AutoSize = true;
            this.chkNotTransactional.Location = new System.Drawing.Point(6, 64);
            this.chkNotTransactional.Name = "chkNotTransactional";
            this.chkNotTransactional.Size = new System.Drawing.Size(173, 17);
            this.chkNotTransactional.TabIndex = 4;
            this.chkNotTransactional.Text = "Run builds without transactions";
            this.toolTip1.SetToolTip(this.chkNotTransactional, resources.GetString("chkNotTransactional.ToolTip"));
            this.chkNotTransactional.UseVisualStyleBackColor = true;
            this.chkNotTransactional.CheckedChanged += new System.EventHandler(this.chkNoTransaction_CheckedChanged);
            // 
            // txtDescription
            // 
            this.txtDescription.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.txtDescription.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txtDescription.Location = new System.Drawing.Point(72, 107);
            this.txtDescription.Name = "txtDescription";
            this.txtDescription.Size = new System.Drawing.Size(193, 20);
            this.txtDescription.TabIndex = 3;
            // 
            // label10
            // 
            this.label10.AutoSize = true;
            this.label10.Location = new System.Drawing.Point(6, 111);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(60, 13);
            this.label10.TabIndex = 2;
            this.label10.Text = "Description";
            // 
            // chkRunTrial
            // 
            this.chkRunTrial.AutoSize = true;
            this.chkRunTrial.Location = new System.Drawing.Point(6, 42);
            this.chkRunTrial.Name = "chkRunTrial";
            this.chkRunTrial.Size = new System.Drawing.Size(158, 17);
            this.chkRunTrial.TabIndex = 1;
            this.chkRunTrial.Text = "Run as Trial (rollback) mode";
            this.toolTip1.SetToolTip(this.chkRunTrial, "/trial");
            this.chkRunTrial.UseVisualStyleBackColor = true;
            // 
            // chkRunThreaded
            // 
            this.chkRunThreaded.AutoSize = true;
            this.chkRunThreaded.Location = new System.Drawing.Point(6, 19);
            this.chkRunThreaded.Name = "chkRunThreaded";
            this.chkRunThreaded.Size = new System.Drawing.Size(176, 17);
            this.chkRunThreaded.TabIndex = 0;
            this.chkRunThreaded.Text = "Run multi-database as threaded";
            this.toolTip1.SetToolTip(this.chkRunThreaded, "/threaded");
            this.chkRunThreaded.UseVisualStyleBackColor = true;
            this.chkRunThreaded.CheckedChanged += new System.EventHandler(this.chkRunThreaded_CheckedChanged);
            // 
            // ddLogStyle
            // 
            this.ddLogStyle.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddLogStyle.FormattingEnabled = true;
            this.ddLogStyle.Items.AddRange(new object[] {
            "HTML",
            "Plain Text"});
            this.ddLogStyle.Location = new System.Drawing.Point(108, 19);
            this.ddLogStyle.Name = "ddLogStyle";
            this.ddLogStyle.Size = new System.Drawing.Size(107, 21);
            this.ddLogStyle.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(79, 13);
            this.label1.TabIndex = 3;
            this.label1.Text = "Log File Format";
            this.toolTip1.SetToolTip(this.label1, "/LogAsText");
            // 
            // grpLogging
            // 
            this.grpLogging.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.grpLogging.Controls.Add(this.btnLoggingPath);
            this.grpLogging.Controls.Add(this.txtRootLoggingPath);
            this.grpLogging.Controls.Add(this.label2);
            this.grpLogging.Controls.Add(this.label1);
            this.grpLogging.Controls.Add(this.ddLogStyle);
            this.grpLogging.Enabled = false;
            this.grpLogging.Location = new System.Drawing.Point(287, 28);
            this.grpLogging.Name = "grpLogging";
            this.grpLogging.Size = new System.Drawing.Size(516, 133);
            this.grpLogging.TabIndex = 1;
            this.grpLogging.TabStop = false;
            this.grpLogging.Text = "Threaded Run Logging";
            // 
            // btnLoggingPath
            // 
            this.btnLoggingPath.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnLoggingPath.AutoSize = true;
            this.btnLoggingPath.Image = global::SqlSync.Properties.Resources.Open;
            this.btnLoggingPath.Location = new System.Drawing.Point(482, 46);
            this.btnLoggingPath.Name = "btnLoggingPath";
            this.btnLoggingPath.Size = new System.Drawing.Size(28, 23);
            this.btnLoggingPath.TabIndex = 6;
            this.btnLoggingPath.UseVisualStyleBackColor = true;
            this.btnLoggingPath.Click += new System.EventHandler(this.btnLoggingPath_Click);
            // 
            // txtRootLoggingPath
            // 
            this.txtRootLoggingPath.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRootLoggingPath.Location = new System.Drawing.Point(104, 47);
            this.txtRootLoggingPath.Name = "txtRootLoggingPath";
            this.txtRootLoggingPath.Size = new System.Drawing.Size(372, 20);
            this.txtRootLoggingPath.TabIndex = 5;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 51);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(95, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "Root Logging path";
            this.toolTip1.SetToolTip(this.label2, "/RootLoggingPath");
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(9, 21);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(170, 13);
            this.label3.TabIndex = 7;
            this.label3.Text = "Sql Build Manager Package (.sbm)";
            this.toolTip1.SetToolTip(this.label3, "/build");
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(9, 48);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(116, 13);
            this.label4.TabIndex = 10;
            this.label4.Text = "Script Source Directory";
            this.toolTip1.SetToolTip(this.label4, "/ScriptSrcDir");
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(9, 21);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(202, 13);
            this.label6.TabIndex = 7;
            this.label6.Text = "Target Override Settings (.multiDb or .cfg)";
            this.toolTip1.SetToolTip(this.label6, "/override");
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(7, 374);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(104, 13);
            this.label5.TabIndex = 10;
            this.label5.Text = "Command line string:";
            this.toolTip1.SetToolTip(this.label5, "/RootLoggingPath");
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Location = new System.Drawing.Point(9, 10);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(90, 13);
            this.label7.TabIndex = 18;
            this.label7.Text = "Execution output:";
            this.toolTip1.SetToolTip(this.label7, "/RootLoggingPath");
            // 
            // label9
            // 
            this.label9.AutoSize = true;
            this.label9.Location = new System.Drawing.Point(453, 22);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(56, 13);
            this.label9.TabIndex = 4;
            this.label9.Text = "Password:";
            this.toolTip1.SetToolTip(this.label9, "/password");
            // 
            // label8
            // 
            this.label8.AutoSize = true;
            this.label8.Location = new System.Drawing.Point(274, 22);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(63, 13);
            this.label8.TabIndex = 2;
            this.label8.Text = "User Name:";
            this.toolTip1.SetToolTip(this.label8, "/username");
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.btnScriptSrcDir);
            this.groupBox3.Controls.Add(this.txtScriptSrcDir);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Controls.Add(this.btnOpenSbm);
            this.groupBox3.Controls.Add(this.txtSbmFile);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Location = new System.Drawing.Point(10, 167);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(793, 80);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Script Source";
            // 
            // btnScriptSrcDir
            // 
            this.btnScriptSrcDir.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnScriptSrcDir.AutoSize = true;
            this.btnScriptSrcDir.Image = global::SqlSync.Properties.Resources.Open;
            this.btnScriptSrcDir.Location = new System.Drawing.Point(759, 44);
            this.btnScriptSrcDir.Name = "btnScriptSrcDir";
            this.btnScriptSrcDir.Size = new System.Drawing.Size(28, 22);
            this.btnScriptSrcDir.TabIndex = 12;
            this.btnScriptSrcDir.UseVisualStyleBackColor = true;
            this.btnScriptSrcDir.Click += new System.EventHandler(this.btnScriptSrcDir_Click);
            // 
            // txtScriptSrcDir
            // 
            this.txtScriptSrcDir.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtScriptSrcDir.Location = new System.Drawing.Point(182, 44);
            this.txtScriptSrcDir.Name = "txtScriptSrcDir";
            this.txtScriptSrcDir.Size = new System.Drawing.Size(571, 20);
            this.txtScriptSrcDir.TabIndex = 11;
            // 
            // btnOpenSbm
            // 
            this.btnOpenSbm.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenSbm.AutoSize = true;
            this.btnOpenSbm.Image = global::SqlSync.Properties.Resources.Open;
            this.btnOpenSbm.Location = new System.Drawing.Point(759, 17);
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
            this.txtSbmFile.Size = new System.Drawing.Size(571, 20);
            this.txtSbmFile.TabIndex = 8;
            this.txtSbmFile.TextChanged += new System.EventHandler(this.txtSbmFile_TextChanged);
            // 
            // groupBox4
            // 
            this.groupBox4.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox4.Controls.Add(btnMultDbCfg);
            this.groupBox4.Controls.Add(this.txtOverride);
            this.groupBox4.Controls.Add(this.label6);
            this.groupBox4.Location = new System.Drawing.Point(10, 250);
            this.groupBox4.Name = "groupBox4";
            this.groupBox4.Size = new System.Drawing.Size(793, 50);
            this.groupBox4.TabIndex = 13;
            this.groupBox4.TabStop = false;
            this.groupBox4.Text = "Override Target Settings";
            // 
            // txtOverride
            // 
            this.txtOverride.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtOverride.AutoCompleteMode = System.Windows.Forms.AutoCompleteMode.SuggestAppend;
            this.txtOverride.AutoCompleteSource = System.Windows.Forms.AutoCompleteSource.CustomSource;
            this.txtOverride.Location = new System.Drawing.Point(217, 17);
            this.txtOverride.Name = "txtOverride";
            this.txtOverride.Size = new System.Drawing.Size(536, 20);
            this.txtOverride.TabIndex = 8;
            // 
            // folderBrowserDialog1
            // 
            this.folderBrowserDialog1.Description = "Root Logging Path";
            // 
            // fileSbm
            // 
            this.fileSbm.Filter = "Sql Build Manager *.sbm|*.sbm|Sql Build Control File *.sbx|*.sbx|All Files *.*|*." +
    "*";
            // 
            // fileOverride
            // 
            this.fileOverride.Filter = "MultiDb file *.multidb|*.multidb|Config file *.cfg|*.cfg|All Files *.*|*.*";
            // 
            // btnConstructCommand
            // 
            this.btnConstructCommand.Location = new System.Drawing.Point(519, 361);
            this.btnConstructCommand.Name = "btnConstructCommand";
            this.btnConstructCommand.Size = new System.Drawing.Size(145, 23);
            this.btnConstructCommand.TabIndex = 14;
            this.btnConstructCommand.Text = "Construct Command Line";
            this.btnConstructCommand.UseVisualStyleBackColor = true;
            this.btnConstructCommand.Click += new System.EventHandler(this.btnConstructCommand_Click);
            // 
            // rtbCommandLine
            // 
            this.rtbCommandLine.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbCommandLine.Location = new System.Drawing.Point(10, 390);
            this.rtbCommandLine.Name = "rtbCommandLine";
            this.rtbCommandLine.Size = new System.Drawing.Size(793, 65);
            this.rtbCommandLine.TabIndex = 15;
            this.rtbCommandLine.Text = "";
            this.rtbCommandLine.TextChanged += new System.EventHandler(this.rtbCommandLine_TextChanged);
            // 
            // btnExecute
            // 
            this.btnExecute.Enabled = false;
            this.btnExecute.Location = new System.Drawing.Point(670, 361);
            this.btnExecute.Name = "btnExecute";
            this.btnExecute.Size = new System.Drawing.Size(75, 23);
            this.btnExecute.TabIndex = 16;
            this.btnExecute.Text = "Execute";
            this.btnExecute.UseVisualStyleBackColor = true;
            this.btnExecute.Click += new System.EventHandler(this.btnExecute_Click);
            // 
            // rtbOutput
            // 
            this.rtbOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbOutput.Location = new System.Drawing.Point(14, 26);
            this.rtbOutput.Name = "rtbOutput";
            this.rtbOutput.Size = new System.Drawing.Size(791, 138);
            this.rtbOutput.TabIndex = 19;
            this.rtbOutput.Text = "";
            // 
            // btnOpenFolder
            // 
            this.btnOpenFolder.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnOpenFolder.Location = new System.Drawing.Point(687, 168);
            this.btnOpenFolder.Name = "btnOpenFolder";
            this.btnOpenFolder.Size = new System.Drawing.Size(118, 23);
            this.btnOpenFolder.TabIndex = 20;
            this.btnOpenFolder.Text = "Open Logging Folder";
            this.btnOpenFolder.UseVisualStyleBackColor = true;
            this.btnOpenFolder.Click += new System.EventHandler(this.btnOpenFolder_Click);
            // 
            // pnlOutput
            // 
            this.pnlOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlOutput.Controls.Add(this.rtbOutput);
            this.pnlOutput.Controls.Add(this.btnOpenFolder);
            this.pnlOutput.Controls.Add(this.label7);
            this.pnlOutput.Enabled = false;
            this.pnlOutput.Location = new System.Drawing.Point(-2, 463);
            this.pnlOutput.Name = "pnlOutput";
            this.pnlOutput.Size = new System.Drawing.Size(816, 200);
            this.pnlOutput.TabIndex = 21;
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.ddAuthentication);
            this.groupBox2.Controls.Add(this.label9);
            this.groupBox2.Controls.Add(this.txtPassword);
            this.groupBox2.Controls.Add(this.label8);
            this.groupBox2.Controls.Add(this.txtUserName);
            this.groupBox2.Location = new System.Drawing.Point(10, 304);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(625, 51);
            this.groupBox2.TabIndex = 14;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Database Authentication Settings";
            // 
            // ddAuthentication
            // 
            this.ddAuthentication.BackColor = System.Drawing.Color.Snow;
            this.ddAuthentication.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddAuthentication.Location = new System.Drawing.Point(9, 18);
            this.ddAuthentication.Name = "ddAuthentication";
            this.ddAuthentication.Size = new System.Drawing.Size(256, 21);
            this.ddAuthentication.TabIndex = 5;
            this.ddAuthentication.SelectionChangeCommitted += new System.EventHandler(this.ddAuthentication_SelectionChangeCommitted);
            // 
            // txtPassword
            // 
            this.txtPassword.Enabled = false;
            this.txtPassword.Location = new System.Drawing.Point(517, 18);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(100, 20);
            this.txtPassword.TabIndex = 3;
            // 
            // txtUserName
            // 
            this.txtUserName.Enabled = false;
            this.txtUserName.Location = new System.Drawing.Point(345, 18);
            this.txtUserName.Name = "txtUserName";
            this.txtUserName.Size = new System.Drawing.Size(100, 20);
            this.txtUserName.TabIndex = 1;
            // 
            // groupBox5
            // 
            this.groupBox5.Controls.Add(this.txtLoggingDatabase);
            this.groupBox5.Location = new System.Drawing.Point(641, 304);
            this.groupBox5.Name = "groupBox5";
            this.groupBox5.Size = new System.Drawing.Size(160, 51);
            this.groupBox5.TabIndex = 22;
            this.groupBox5.TabStop = false;
            this.groupBox5.Text = "Alternate Logging Database";
            // 
            // txtLoggingDatabase
            // 
            this.txtLoggingDatabase.Location = new System.Drawing.Point(6, 19);
            this.txtLoggingDatabase.Name = "txtLoggingDatabase";
            this.txtLoggingDatabase.Size = new System.Drawing.Size(148, 20);
            this.txtLoggingDatabase.TabIndex = 4;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helptoolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(812, 24);
            this.menuStrip1.TabIndex = 24;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // helptoolStripMenuItem
            // 
            this.helptoolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.helptoolStripMenuItem.Image = global::SqlSync.Properties.Resources.Help_2;
            this.helptoolStripMenuItem.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.helptoolStripMenuItem.Name = "helptoolStripMenuItem";
            this.helptoolStripMenuItem.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.helptoolStripMenuItem.Size = new System.Drawing.Size(28, 20);
            this.helptoolStripMenuItem.Click += new System.EventHandler(this.helptoolStripMenuItem_Click);
            // 
            // CommandLineBuilderForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ClientSize = new System.Drawing.Size(812, 663);
            this.Controls.Add(this.groupBox5);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.pnlOutput);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.btnExecute);
            this.Controls.Add(this.rtbCommandLine);
            this.Controls.Add(this.btnConstructCommand);
            this.Controls.Add(this.groupBox4);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.grpLogging);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "CommandLineBuilderForm";
            this.Text = "Command Line Builder";
            this.Load += new System.EventHandler(this.CommandLineBuilderForm_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.grpLogging.ResumeLayout(false);
            this.grpLogging.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox4.ResumeLayout(false);
            this.groupBox4.PerformLayout();
            this.pnlOutput.ResumeLayout(false);
            this.pnlOutput.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.groupBox5.ResumeLayout(false);
            this.groupBox5.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox chkRunTrial;
        private System.Windows.Forms.CheckBox chkRunThreaded;
        private System.Windows.Forms.ComboBox ddLogStyle;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox grpLogging;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnLoggingPath;
        private System.Windows.Forms.TextBox txtRootLoggingPath;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.Button btnOpenSbm;
        private System.Windows.Forms.TextBox txtSbmFile;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnScriptSrcDir;
        private System.Windows.Forms.TextBox txtScriptSrcDir;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.GroupBox groupBox4;
        private System.Windows.Forms.TextBox txtOverride;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.OpenFileDialog fileSbm;
        private System.Windows.Forms.OpenFileDialog fileOverride;
        private System.Windows.Forms.Button btnConstructCommand;
        private System.Windows.Forms.RichTextBox rtbCommandLine;
        private System.Windows.Forms.Button btnExecute;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.RichTextBox rtbOutput;
        private System.Windows.Forms.Button btnOpenFolder;
        private System.Windows.Forms.Panel pnlOutput;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Label label9;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.Label label8;
        private System.Windows.Forms.TextBox txtUserName;
        private System.Windows.Forms.GroupBox groupBox5;
        private System.Windows.Forms.TextBox txtLoggingDatabase;
        private System.Windows.Forms.TextBox txtDescription;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.CheckBox chkNotTransactional;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem helptoolStripMenuItem;
        private System.Windows.Forms.TextBox txtAllowedTimeoutRetries;
        private System.Windows.Forms.Label label11;
        private System.Windows.Forms.ComboBox ddAuthentication;
    }
}