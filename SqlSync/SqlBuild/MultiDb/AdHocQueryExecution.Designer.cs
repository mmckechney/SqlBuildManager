namespace SqlSync.SqlBuild.MultiDb
{
    partial class AdHocQueryExecution 
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
            UrielGuy.SyntaxHighlighting.HighLightDescriptorCollection highLightDescriptorCollection1 = new UrielGuy.SyntaxHighlighting.HighLightDescriptorCollection();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AdHocQueryExecution));
            this.rtbSqlScript = new UrielGuy.SyntaxHighlighting.SyntaxHighlightingTextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.actionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.recentFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
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
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // btnGenerate
            // 
            this.btnGenerate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnGenerate.Location = new System.Drawing.Point(661, 286);
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
            // rtbSqlScript
            // 
            this.rtbSqlScript.AcceptsTab = true;
            this.rtbSqlScript.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbSqlScript.CaseSensitive = false;
            this.rtbSqlScript.FilterAutoComplete = true;
            this.rtbSqlScript.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbSqlScript.HighlightDescriptors = highLightDescriptorCollection1;
            this.rtbSqlScript.HighlightType = SqlSync.Highlighting.SyntaxHightlightType.Sql;
            this.rtbSqlScript.Location = new System.Drawing.Point(12, 115);
            this.rtbSqlScript.MaxUndoRedoSteps = 50;
            this.rtbSqlScript.Name = "rtbSqlScript";
            this.rtbSqlScript.Size = new System.Drawing.Size(758, 165);
            this.rtbSqlScript.SuspendHighlighting = false;
            this.rtbSqlScript.TabIndex = 2;
            this.rtbSqlScript.Text = "";
            this.rtbSqlScript.WordWrap = false;
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.actionToolStripMenuItem,
            this.toolStripMenuItem1,
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(782, 24);
            this.menuStrip1.TabIndex = 9;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // actionToolStripMenuItem
            // 
            this.actionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openScriptToolStripMenuItem,
            this.saveScriptToolStripMenuItem,
            this.toolStripSeparator1,
            this.recentFilesToolStripMenuItem});
            this.actionToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Execute;
            this.actionToolStripMenuItem.Name = "actionToolStripMenuItem";
            this.actionToolStripMenuItem.Size = new System.Drawing.Size(70, 20);
            this.actionToolStripMenuItem.Text = "Action";
            // 
            // openScriptToolStripMenuItem
            // 
            this.openScriptToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Open;
            this.openScriptToolStripMenuItem.Name = "openScriptToolStripMenuItem";
            this.openScriptToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.openScriptToolStripMenuItem.Text = "Open Script";
            this.openScriptToolStripMenuItem.Click += new System.EventHandler(this.openScriptToolStripMenuItem_Click);
            // 
            // saveScriptToolStripMenuItem
            // 
            this.saveScriptToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Save;
            this.saveScriptToolStripMenuItem.Name = "saveScriptToolStripMenuItem";
            this.saveScriptToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.saveScriptToolStripMenuItem.Text = "Save Script";
            this.saveScriptToolStripMenuItem.Click += new System.EventHandler(this.saveScriptToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(133, 6);
            // 
            // recentFilesToolStripMenuItem
            // 
            this.recentFilesToolStripMenuItem.Name = "recentFilesToolStripMenuItem";
            this.recentFilesToolStripMenuItem.Size = new System.Drawing.Size(136, 22);
            this.recentFilesToolStripMenuItem.Text = "Recent Files";
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
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(60, 20);
            this.helpToolStripMenuItem.Text = "Help";
            // 
            // viewLogFileMenuItem1
            // 
            this.viewLogFileMenuItem1.Image = ((System.Drawing.Image)(resources.GetObject("viewLogFileMenuItem1.Image")));
            this.viewLogFileMenuItem1.Name = "viewLogFileMenuItem1";
            this.viewLogFileMenuItem1.Size = new System.Drawing.Size(207, 22);
            this.viewLogFileMenuItem1.Text = "View Application Log File";
            // 
            // setLoggingLevelMenuItem1
            // 
            this.setLoggingLevelMenuItem1.Name = "setLoggingLevelMenuItem1";
            this.setLoggingLevelMenuItem1.Size = new System.Drawing.Size(207, 22);
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
            this.txtTimeout.Location = new System.Drawing.Point(730, 89);
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
            this.label4.Location = new System.Drawing.Point(507, 92);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(206, 13);
            this.label4.TabIndex = 15;
            this.label4.Text = "Query timeout in seconds  (per database): ";
            this.toolTip1.SetToolTip(this.label4, "Sets the query timeout per database connection");
            // 
            // AdHocQueryExecution
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(782, 339);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.txtTimeout);
            this.Controls.Add(this.lblCsvWarning);
            this.Controls.Add(this.rtbSqlScript);
            this.Controls.Add(this.menuStrip1);
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "AdHocQueryExecution";
            this.Text = "AdHoc Query Execution";
            this.Load += new System.EventHandler(this.AdHocQueryExecution_Load);
            this.Controls.SetChildIndex(this.lblServerCount, 0);
            this.Controls.SetChildIndex(this.lblDatabaseCount, 0);
            this.Controls.SetChildIndex(this.menuStrip1, 0);
            this.Controls.SetChildIndex(this.btnGenerate, 0);
            this.Controls.SetChildIndex(this.label1, 0);
            this.Controls.SetChildIndex(this.ddOutputType, 0);
            this.Controls.SetChildIndex(this.label2, 0);
            this.Controls.SetChildIndex(this.label3, 0);
            this.Controls.SetChildIndex(this.rtbSqlScript, 0);
            this.Controls.SetChildIndex(this.lblCsvWarning, 0);
            this.Controls.SetChildIndex(this.txtTimeout, 0);
            this.Controls.SetChildIndex(this.label4, 0);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private UrielGuy.SyntaxHighlighting.SyntaxHighlightingTextBox rtbSqlScript;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem actionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openScriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveScriptToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem recentFilesToolStripMenuItem;
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
    }
}