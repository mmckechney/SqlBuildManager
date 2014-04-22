namespace SqlSync.SqlBuild
{
    partial class ScriptConfigCtrl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScriptConfigCtrl));
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.chkRollBackBuild = new System.Windows.Forms.CheckBox();
            this.chkRollBackScript = new System.Windows.Forms.CheckBox();
            this.chkStripTransactions = new System.Windows.Forms.CheckBox();
            this.chkAllowMultipleRuns = new System.Windows.Forms.CheckBox();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtBuildOrder = new System.Windows.Forms.TextBox();
            this.txtScriptTimeout = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.ddInfer = new System.Windows.Forms.ComboBox();
            this.rtbDescription = new System.Windows.Forms.RichTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.cbTag = new System.Windows.Forms.ComboBox();
            this.picToggle = new System.Windows.Forms.PictureBox();
            this.label6 = new System.Windows.Forms.Label();
            this.ddDatabaseList = new SqlSync.DatabaseDropDown();
            ((System.ComponentModel.ISupportInitialize)(this.picToggle)).BeginInit();
            this.SuspendLayout();
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "downarrow_white.gif");
            this.imageList1.Images.SetKeyName(1, "uparrow_white.gif");
            // 
            // chkRollBackBuild
            // 
            this.chkRollBackBuild.Checked = true;
            this.chkRollBackBuild.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRollBackBuild.Location = new System.Drawing.Point(10, 67);
            this.chkRollBackBuild.Name = "chkRollBackBuild";
            this.chkRollBackBuild.Size = new System.Drawing.Size(314, 23);
            this.chkRollBackBuild.TabIndex = 3;
            this.chkRollBackBuild.TabStop = false;
            this.chkRollBackBuild.Text = "Roll back entire build on failure";
            this.chkRollBackBuild.CheckedChanged += new System.EventHandler(this.chkRollBackBuild_CheckedChanged);
            // 
            // chkRollBackScript
            // 
            this.chkRollBackScript.Checked = true;
            this.chkRollBackScript.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRollBackScript.Enabled = false;
            this.chkRollBackScript.Location = new System.Drawing.Point(35, 90);
            this.chkRollBackScript.Name = "chkRollBackScript";
            this.chkRollBackScript.Size = new System.Drawing.Size(310, 17);
            this.chkRollBackScript.TabIndex = 9;
            this.chkRollBackScript.TabStop = false;
            this.chkRollBackScript.Text = "Roll back full script file contents on partial failure";
            // 
            // chkStripTransactions
            // 
            this.chkStripTransactions.Checked = true;
            this.chkStripTransactions.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkStripTransactions.Location = new System.Drawing.Point(350, 67);
            this.chkStripTransactions.Name = "chkStripTransactions";
            this.chkStripTransactions.Size = new System.Drawing.Size(292, 23);
            this.chkStripTransactions.TabIndex = 4;
            this.chkStripTransactions.TabStop = false;
            this.chkStripTransactions.Text = "Strip Transaction References";
            this.chkStripTransactions.CheckedChanged += new System.EventHandler(this.chkStripTransactions_CheckedChanged);
            // 
            // chkAllowMultipleRuns
            // 
            this.chkAllowMultipleRuns.Checked = true;
            this.chkAllowMultipleRuns.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAllowMultipleRuns.Location = new System.Drawing.Point(350, 90);
            this.chkAllowMultipleRuns.Name = "chkAllowMultipleRuns";
            this.chkAllowMultipleRuns.Size = new System.Drawing.Size(315, 17);
            this.chkAllowMultipleRuns.TabIndex = 5;
            this.chkAllowMultipleRuns.TabStop = false;
            this.chkAllowMultipleRuns.Text = "Allow Multiple Committed Runs on same Server";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(1, 4);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(75, 16);
            this.label4.TabIndex = 13;
            this.label4.Text = "Target DB:";
            this.toolTip1.SetToolTip(this.label4, "Target Database to run this script on");
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(277, 4);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(117, 16);
            this.label3.TabIndex = 15;
            this.label3.Text = "Build Sequence #:";
            // 
            // txtBuildOrder
            // 
            this.txtBuildOrder.Location = new System.Drawing.Point(394, 2);
            this.txtBuildOrder.Name = "txtBuildOrder";
            this.txtBuildOrder.Size = new System.Drawing.Size(91, 21);
            this.txtBuildOrder.TabIndex = 1;
            this.txtBuildOrder.Leave += new System.EventHandler(this.txtBuildOrder_Leave);
            // 
            // txtScriptTimeout
            // 
            this.txtScriptTimeout.Location = new System.Drawing.Point(649, 2);
            this.txtScriptTimeout.Name = "txtScriptTimeout";
            this.txtScriptTimeout.Size = new System.Drawing.Size(54, 21);
            this.txtScriptTimeout.TabIndex = 2;
            this.txtScriptTimeout.Text = "20";
            this.txtScriptTimeout.Leave += new System.EventHandler(this.txtScriptTimeout_Leave);
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(488, 4);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(161, 19);
            this.label5.TabIndex = 19;
            this.label5.Text = "Script Time Out (seconds):";
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 10000;
            this.toolTip1.InitialDelay = 500;
            this.toolTip1.IsBalloon = true;
            this.toolTip1.ReshowDelay = 100;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(709, 5);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(35, 16);
            this.label1.TabIndex = 25;
            this.label1.Text = "Tag:";
            this.toolTip1.SetToolTip(this.label1, "ID tag to assign to script.\r\nFor example: Code Branch association.");
            // 
            // ddInfer
            // 
            this.ddInfer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddInfer.FormattingEnabled = true;
            this.ddInfer.Items.AddRange(new object[] {
            "Script/Name",
            "Name/Script",
            "Script Only",
            "Name Only",
            "None"});
            this.ddInfer.Location = new System.Drawing.Point(712, 85);
            this.ddInfer.Name = "ddInfer";
            this.ddInfer.Size = new System.Drawing.Size(121, 21);
            this.ddInfer.TabIndex = 28;
            this.toolTip1.SetToolTip(this.ddInfer, "With a setting other than \"None\", the tool will try to extract a tag from the scr" +
        "ipt text or the filename upon saving.");
            // 
            // rtbDescription
            // 
            this.rtbDescription.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbDescription.Location = new System.Drawing.Point(95, 29);
            this.rtbDescription.Name = "rtbDescription";
            this.rtbDescription.Size = new System.Drawing.Size(768, 36);
            this.rtbDescription.TabIndex = 6;
            this.rtbDescription.Text = "";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(1, 28);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 18);
            this.label2.TabIndex = 24;
            this.label2.Text = "Description:";
            // 
            // cbTag
            // 
            this.cbTag.FormattingEnabled = true;
            this.cbTag.Location = new System.Drawing.Point(740, 2);
            this.cbTag.Name = "cbTag";
            this.cbTag.Size = new System.Drawing.Size(90, 21);
            this.cbTag.TabIndex = 26;
            // 
            // picToggle
            // 
            this.picToggle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picToggle.Image = global::SqlSync.Properties.Resources.downarrow_white;
            this.picToggle.Location = new System.Drawing.Point(836, 3);
            this.picToggle.Name = "picToggle";
            this.picToggle.Size = new System.Drawing.Size(29, 18);
            this.picToggle.TabIndex = 22;
            this.picToggle.TabStop = false;
            this.picToggle.Click += new System.EventHandler(this.picToggle_Click);
            // 
            // label6
            // 
            this.label6.Location = new System.Drawing.Point(709, 67);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(143, 16);
            this.label6.TabIndex = 29;
            this.label6.Text = "Infer tag value from:";
            // 
            // ddDatabaseList
            // 
            this.ddDatabaseList.DatabaseList = null;
            this.ddDatabaseList.FormattingEnabled = true;
            this.ddDatabaseList.Location = new System.Drawing.Point(95, 3);
            this.ddDatabaseList.Name = "ddDatabaseList";
            this.ddDatabaseList.SelectedDatabase = "";
            this.ddDatabaseList.Size = new System.Drawing.Size(176, 21);
            this.ddDatabaseList.TabIndex = 27;
            // 
            // ScriptConfigCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.BackColor = System.Drawing.SystemColors.Control;
            this.Controls.Add(this.label6);
            this.Controls.Add(this.ddInfer);
            this.Controls.Add(this.rtbDescription);
            this.Controls.Add(this.ddDatabaseList);
            this.Controls.Add(this.cbTag);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.picToggle);
            this.Controls.Add(this.txtScriptTimeout);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtBuildOrder);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.chkAllowMultipleRuns);
            this.Controls.Add(this.chkRollBackBuild);
            this.Controls.Add(this.chkRollBackScript);
            this.Controls.Add(this.chkStripTransactions);
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "ScriptConfigCtrl";
            this.Size = new System.Drawing.Size(866, 111);
            ((System.ComponentModel.ISupportInitialize)(this.picToggle)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.CheckBox chkRollBackBuild;
        private System.Windows.Forms.CheckBox chkRollBackScript;
        private System.Windows.Forms.CheckBox chkStripTransactions;
        private System.Windows.Forms.CheckBox chkAllowMultipleRuns;
        //private System.Windows.Forms.ComboBox ddDatabaseList;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.TextBox txtBuildOrder;
        private System.Windows.Forms.TextBox txtScriptTimeout;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.RichTextBox rtbDescription;
        private System.Windows.Forms.Label label2;
        public System.Windows.Forms.PictureBox picToggle;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox cbTag;
        private DatabaseDropDown ddDatabaseList;
        private System.Windows.Forms.ComboBox ddInfer;
        private System.Windows.Forms.Label label6;
    }
}
