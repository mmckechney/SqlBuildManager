namespace SqlSync.SqlBuild.MultiDb
{
    partial class BuildValidationForm 
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BuildValidationForm));
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.viewLogFileMenuItem1 = new SqlSync.Controls.ViewLogFileMenuItem();
            this.setLoggingLevelMenuItem1 = new SqlSync.Controls.SetLoggingLevelMenuItem();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.lblCsvWarning = new System.Windows.Forms.Label();
            this.txtTimeout = new System.Windows.Forms.TextBox();
            this.label4 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.txtCheckValue = new System.Windows.Forms.TextBox();
            this.ddValidationType = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnGenerate
            // 
            this.btnGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGenerate.Location = new System.Drawing.Point(580, 168);
            this.btnGenerate.TabIndex = 3;
            this.btnGenerate.Text = "Run Script";
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click_1);
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 41);
            // 
            // ddOutputType
            // 
            this.ddOutputType.Location = new System.Drawing.Point(123, 37);
            this.ddOutputType.TabIndex = 0;
            this.ddOutputType.SelectedIndexChanged += new System.EventHandler(this.ddOutputType_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(12, 68);
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(12, 91);
            // 
            // lblServerCount
            // 
            this.lblServerCount.Location = new System.Drawing.Point(169, 68);
            // 
            // lblDatabaseCount
            // 
            this.lblDatabaseCount.Location = new System.Drawing.Point(169, 91);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(701, 24);
            this.menuStrip1.TabIndex = 9;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripMenuItem1.Image = global::SqlSync.Properties.Resources.Help_2;
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(28, 20);
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
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
            // openFileDialog1
            // 
            this.openFileDialog1.DefaultExt = "sql";
            this.openFileDialog1.Filter = "SQL files *.sql|*.sql|All Files *.*|*.*";
            this.openFileDialog1.Title = "Open SQL query file";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "sql";
            this.saveFileDialog1.Filter = "SQL files *.sql|*.sql|All Files *.*|*.*";
            this.saveFileDialog1.Title = "Save SQL Query file";
            // 
            // lblCsvWarning
            // 
            this.lblCsvWarning.AutoSize = true;
            this.lblCsvWarning.ForeColor = System.Drawing.Color.Red;
            this.lblCsvWarning.Location = new System.Drawing.Point(251, 34);
            this.lblCsvWarning.Name = "lblCsvWarning";
            this.lblCsvWarning.Size = new System.Drawing.Size(322, 26);
            this.lblCsvWarning.TabIndex = 13;
            this.lblCsvWarning.Text = "Warning: CSV is recommended if you are retrieving a lot of data. \r\nHTML and XML f" +
                "ormats may error out due to disk or memory issues";
            this.lblCsvWarning.Visible = false;
            // 
            // txtTimeout
            // 
            this.txtTimeout.Location = new System.Drawing.Point(650, 88);
            this.txtTimeout.Name = "txtTimeout";
            this.txtTimeout.Size = new System.Drawing.Size(39, 20);
            this.txtTimeout.TabIndex = 1;
            this.txtTimeout.Text = "20";
            this.txtTimeout.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.txtTimeout.KeyDown += new System.Windows.Forms.KeyEventHandler(this.textBox1_KeyDown);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(418, 92);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(206, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "Query timeout in seconds  (per database): ";
            this.toolTip1.SetToolTip(this.label4, "Sets the query timeout per database connection");
            // 
            // txtCheckValue
            // 
            this.txtCheckValue.Location = new System.Drawing.Point(123, 157);
            this.txtCheckValue.Name = "txtCheckValue";
            this.txtCheckValue.Size = new System.Drawing.Size(311, 20);
            this.txtCheckValue.TabIndex = 16;
            // 
            // ddValidationType
            // 
            this.ddValidationType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddValidationType.FormattingEnabled = true;
            this.ddValidationType.Items.AddRange(new object[] {
            "Build File Hash",
            "Build File Name",
            "Individual Script Hash",
            "Individual Script Name"});
            this.ddValidationType.Location = new System.Drawing.Point(123, 130);
            this.ddValidationType.Name = "ddValidationType";
            this.ddValidationType.Size = new System.Drawing.Size(121, 21);
            this.ddValidationType.TabIndex = 17;
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(12, 133);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(83, 13);
            this.label5.TabIndex = 18;
            this.label5.Text = "Validation Type:";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Location = new System.Drawing.Point(12, 160);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(71, 13);
            this.label6.TabIndex = 19;
            this.label6.Text = "Check Value:";
            // 
            // BuildValidationForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(701, 221);
            this.Controls.Add(this.label6);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.ddValidationType);
            this.Controls.Add(this.txtCheckValue);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtTimeout);
            this.Controls.Add(this.lblCsvWarning);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "BuildValidationForm";
            this.Text = "Build Validation Check";
            this.Controls.SetChildIndex(this.lblServerCount, 0);
            this.Controls.SetChildIndex(this.lblDatabaseCount, 0);
            this.Controls.SetChildIndex(this.menuStrip1, 0);
            this.Controls.SetChildIndex(this.btnGenerate, 0);
            this.Controls.SetChildIndex(this.label1, 0);
            this.Controls.SetChildIndex(this.ddOutputType, 0);
            this.Controls.SetChildIndex(this.label2, 0);
            this.Controls.SetChildIndex(this.label3, 0);
            this.Controls.SetChildIndex(this.lblCsvWarning, 0);
            this.Controls.SetChildIndex(this.txtTimeout, 0);
            this.Controls.SetChildIndex(this.label4, 0);
            this.Controls.SetChildIndex(this.txtCheckValue, 0);
            this.Controls.SetChildIndex(this.ddValidationType, 0);
            this.Controls.SetChildIndex(this.label5, 0);
            this.Controls.SetChildIndex(this.label6, 0);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ToolStripMenuItem toolStripMenuItem1;
        private System.Windows.Forms.Label lblCsvWarning;
        private System.Windows.Forms.ToolStripMenuItem helpToolStripMenuItem;
        private Controls.ViewLogFileMenuItem viewLogFileMenuItem1;
        private Controls.SetLoggingLevelMenuItem setLoggingLevelMenuItem1;
        private System.Windows.Forms.TextBox txtTimeout;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TextBox txtCheckValue;
        private System.Windows.Forms.ComboBox ddValidationType;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label6;
    }
}