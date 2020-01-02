namespace SqlSync.SqlBuild.MultiDb
{
    partial class ConfigurationViaQueryForm
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
            UrielGuy.SyntaxHighlighting.HighLightDescriptorCollection highLightDescriptorCollection1 = new UrielGuy.SyntaxHighlighting.HighLightDescriptorCollection();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConfigurationViaQueryForm));
            this.panel1 = new System.Windows.Forms.Panel();
            this.label4 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.actionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.openSavedQueryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveQueryConfigurationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.changeSqlServerConnectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.recentFilesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.exitToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.lblServer = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.ddDatabase = new System.Windows.Forms.ComboBox();
            this.label5 = new System.Windows.Forms.Label();
            this.button1 = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.btnPreview = new System.Windows.Forms.Button();
            this.rtbSqlScript = new UrielGuy.SyntaxHighlighting.SyntaxHighlightingTextBox();
            this.cutCopyPasteContextMenuStrip1 = new SqlSync.CutCopyPasteContextMenuStrip();
            this.panel1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 24);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(675, 95);
            this.panel1.TabIndex = 0;
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(3, 73);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(623, 13);
            this.label4.TabIndex = 2;
            this.label4.Text = "NOTE: To add additional values that will be included in an AdHoc query, add the c" +
                "olumns after the <<override DB Name>> column\r\n";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(117, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(389, 30);
            this.label2.TabIndex = 1;
            this.label2.Text = "SELECT <<server name>>, <<default DB Name>>, <<override DB Name>> \r\nFROM <<table>" +
                "> WHERE <<criteria>>";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(3, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(296, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "To properly create your configuration construct your query as:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 9);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(78, 13);
            this.label3.TabIndex = 2;
            this.label3.Text = "Source Server:";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.actionToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(675, 24);
            this.menuStrip1.TabIndex = 10;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // actionToolStripMenuItem
            // 
            this.actionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.openSavedQueryToolStripMenuItem,
            this.saveQueryConfigurationToolStripMenuItem,
            this.toolStripSeparator2,
            this.changeSqlServerConnectionToolStripMenuItem,
            this.toolStripSeparator1,
            this.recentFilesToolStripMenuItem,
            this.toolStripSeparator3,
            this.exitToolStripMenuItem});
            this.actionToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Execute;
            this.actionToolStripMenuItem.Name = "actionToolStripMenuItem";
            this.actionToolStripMenuItem.Size = new System.Drawing.Size(65, 20);
            this.actionToolStripMenuItem.Text = "Action";
            // 
            // openSavedQueryToolStripMenuItem
            // 
            this.openSavedQueryToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Open;
            this.openSavedQueryToolStripMenuItem.Name = "openSavedQueryToolStripMenuItem";
            this.openSavedQueryToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
            this.openSavedQueryToolStripMenuItem.Text = "Open Saved Query Configuration";
            this.openSavedQueryToolStripMenuItem.Click += new System.EventHandler(this.openSavedQueryToolStripMenuItem_Click);
            // 
            // saveQueryConfigurationToolStripMenuItem
            // 
            this.saveQueryConfigurationToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Save;
            this.saveQueryConfigurationToolStripMenuItem.Name = "saveQueryConfigurationToolStripMenuItem";
            this.saveQueryConfigurationToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
            this.saveQueryConfigurationToolStripMenuItem.Text = "Save Query Configuration";
            this.saveQueryConfigurationToolStripMenuItem.Click += new System.EventHandler(this.saveQueryConfigurationToolStripMenuItem_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(242, 6);
            // 
            // changeSqlServerConnectionToolStripMenuItem
            // 
            this.changeSqlServerConnectionToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Server1;
            this.changeSqlServerConnectionToolStripMenuItem.Name = "changeSqlServerConnectionToolStripMenuItem";
            this.changeSqlServerConnectionToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
            this.changeSqlServerConnectionToolStripMenuItem.Text = "Change Sql Server Connection";
            this.changeSqlServerConnectionToolStripMenuItem.Click += new System.EventHandler(this.mnuChangeSqlServer_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(242, 6);
            // 
            // recentFilesToolStripMenuItem
            // 
            this.recentFilesToolStripMenuItem.Name = "recentFilesToolStripMenuItem";
            this.recentFilesToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
            this.recentFilesToolStripMenuItem.Text = "Recent Files";
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(242, 6);
            // 
            // exitToolStripMenuItem
            // 
            this.exitToolStripMenuItem.Name = "exitToolStripMenuItem";
            this.exitToolStripMenuItem.Size = new System.Drawing.Size(245, 22);
            this.exitToolStripMenuItem.Text = "Exit";
            this.exitToolStripMenuItem.Click += new System.EventHandler(this.exitToolStripMenuItem_Click);
            // 
            // lblServer
            // 
            this.lblServer.AutoSize = true;
            this.lblServer.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblServer.Location = new System.Drawing.Point(96, 9);
            this.lblServer.Name = "lblServer";
            this.lblServer.Size = new System.Drawing.Size(54, 13);
            this.lblServer.TabIndex = 3;
            this.lblServer.Text = "(not set)";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.WhiteSmoke;
            this.panel2.Controls.Add(this.ddDatabase);
            this.panel2.Controls.Add(this.label5);
            this.panel2.Controls.Add(this.lblServer);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel2.Location = new System.Drawing.Point(0, 119);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(675, 30);
            this.panel2.TabIndex = 11;
            // 
            // ddDatabase
            // 
            this.ddDatabase.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddDatabase.FormattingEnabled = true;
            this.ddDatabase.Location = new System.Drawing.Point(349, 5);
            this.ddDatabase.Name = "ddDatabase";
            this.ddDatabase.Size = new System.Drawing.Size(163, 21);
            this.ddDatabase.TabIndex = 5;
            this.ddDatabase.SelectionChangeCommitted += new System.EventHandler(this.ddDatabase_SelectionChangeCommitted);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(255, 9);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(93, 13);
            this.label5.TabIndex = 4;
            this.label5.Text = "Source Database:";
            // 
            // button1
            // 
            this.button1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.button1.Location = new System.Drawing.Point(515, 381);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(148, 23);
            this.button1.TabIndex = 12;
            this.button1.Text = "Create Configuration";
            this.button1.UseVisualStyleBackColor = true;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "MultiDbQ";
            this.saveFileDialog1.Filter = "MultiDb Query Config|*.MultiDbQ|All Files *.*|*.*";
            this.saveFileDialog1.Title = "Save Query Settings";
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.DefaultExt = "MultiDbQ";
            this.openFileDialog1.Filter = "MultiDb Query Config|*.MultiDbQ|All Files *.*|*.*";
            // 
            // btnPreview
            // 
            //this.btnPreview.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            //this.btnPreview.Location = new System.Drawing.Point(361, 381);
            //this.btnPreview.Name = "btnPreview";
            //this.btnPreview.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            //this.btnPreview.Size = new System.Drawing.Size(148, 23);
            //this.btnPreview.TabIndex = 13;
            //this.btnPreview.Text = "Preview Configuration";
            //this.btnPreview.UseVisualStyleBackColor = true;
            //this.btnPreview.Visible = false;
            //this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);
            // 
            // rtbSqlScript
            // 
            this.rtbSqlScript.AcceptsTab = true;
            this.rtbSqlScript.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbSqlScript.CaseSensitive = false;
            this.rtbSqlScript.ContextMenuStrip = this.cutCopyPasteContextMenuStrip1;
            this.rtbSqlScript.FilterAutoComplete = true;
            this.rtbSqlScript.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbSqlScript.HighlightDescriptors = highLightDescriptorCollection1;
            this.rtbSqlScript.HighlightType = SqlSync.Highlighting.SyntaxHightlightType.Sql;
            this.rtbSqlScript.Location = new System.Drawing.Point(6, 155);
            this.rtbSqlScript.MaxUndoRedoSteps = 50;
            this.rtbSqlScript.Name = "rtbSqlScript";
            this.rtbSqlScript.Size = new System.Drawing.Size(663, 223);
            this.rtbSqlScript.SuspendHighlighting = false;
            this.rtbSqlScript.TabIndex = 9;
            this.rtbSqlScript.Text = "";
            this.rtbSqlScript.WordWrap = false;
            this.rtbSqlScript.TextChanged += new System.EventHandler(this.rtbSqlScript_TextChanged);
            // 
            // cutCopyPasteContextMenuStrip1
            // 
            this.cutCopyPasteContextMenuStrip1.Name = "mnuCopyPaste";
            this.cutCopyPasteContextMenuStrip1.Size = new System.Drawing.Size(113, 70);
            // 
            // ConfigurationViaQueryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(675, 406);
            this.Controls.Add(this.btnPreview);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.rtbSqlScript);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "ConfigurationViaQueryForm";
            this.Text = "Create Configuration Via Query ";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ConfigurationViaQueryForm_FormClosing);
            this.Load += new System.EventHandler(this.ConfigurationViaQueryForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ConfigurationViaQueryForm_KeyDown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private UrielGuy.SyntaxHighlighting.SyntaxHighlightingTextBox rtbSqlScript;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.Label lblServer;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ComboBox ddDatabase;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private System.Windows.Forms.ToolStripMenuItem actionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem changeSqlServerConnectionToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem openSavedQueryToolStripMenuItem;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem recentFilesToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem saveQueryConfigurationToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator3;
        private System.Windows.Forms.ToolStripMenuItem exitToolStripMenuItem;
        private System.Windows.Forms.Label label4;
        private CutCopyPasteContextMenuStrip cutCopyPasteContextMenuStrip1;
    }
}