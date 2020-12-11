namespace SqlSync.SqlBuild.Default_Scripts
{
    partial class DefaultScriptMaintenanceForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DefaultScriptMaintenanceForm));
            this.lstScripts = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.deleteDefaultScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnAddNew = new System.Windows.Forms.Button();
            this.btnSave = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.scriptConfigCtrl1 = new SqlSync.SqlBuild.ScriptConfigCtrl();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.lnkScriptPath = new System.Windows.Forms.LinkLabel();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statGeneral = new System.Windows.Forms.ToolStripStatusLabel();
            this.progBar = new System.Windows.Forms.ToolStripProgressBar();
            this.bgLoadScript = new System.ComponentModel.BackgroundWorker();
            this.contextMenuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstScripts
            // 
            this.lstScripts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstScripts.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstScripts.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.lstScripts.ContextMenuStrip = this.contextMenuStrip1;
            this.lstScripts.FullRowSelect = true;
            this.lstScripts.GridLines = true;
            this.lstScripts.HideSelection = false;
            this.lstScripts.Location = new System.Drawing.Point(8, 59);
            this.lstScripts.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.lstScripts.MultiSelect = false;
            this.lstScripts.Name = "lstScripts";
            this.lstScripts.Size = new System.Drawing.Size(1008, 119);
            this.lstScripts.TabIndex = 0;
            this.lstScripts.UseCompatibleStateImageBehavior = false;
            this.lstScripts.View = System.Windows.Forms.View.Details;
            this.lstScripts.SelectedIndexChanged += new System.EventHandler(this.listView1_SelectedIndexChanged);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Script Name";
            this.columnHeader1.Width = 339;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Description";
            this.columnHeader2.Width = 482;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.deleteDefaultScriptToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(180, 26);
            // 
            // deleteDefaultScriptToolStripMenuItem
            // 
            this.deleteDefaultScriptToolStripMenuItem.Name = "deleteDefaultScriptToolStripMenuItem";
            this.deleteDefaultScriptToolStripMenuItem.Size = new System.Drawing.Size(179, 22);
            this.deleteDefaultScriptToolStripMenuItem.Text = "Delete default script";
            this.deleteDefaultScriptToolStripMenuItem.Click += new System.EventHandler(this.deleteDefaultScriptToolStripMenuItem_Click);
            // 
            // btnAddNew
            // 
            this.btnAddNew.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAddNew.Location = new System.Drawing.Point(8, 318);
            this.btnAddNew.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnAddNew.Name = "btnAddNew";
            this.btnAddNew.Size = new System.Drawing.Size(82, 27);
            this.btnAddNew.TabIndex = 3;
            this.btnAddNew.Text = "Add New";
            this.btnAddNew.UseVisualStyleBackColor = true;
            this.btnAddNew.Click += new System.EventHandler(this.btnAddNew_Click);
            // 
            // btnSave
            // 
            this.btnSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnSave.Location = new System.Drawing.Point(455, 318);
            this.btnSave.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnSave.Name = "btnSave";
            this.btnSave.Size = new System.Drawing.Size(118, 27);
            this.btnSave.TabIndex = 4;
            this.btnSave.Text = "Close";
            this.btnSave.UseVisualStyleBackColor = true;
            this.btnSave.Click += new System.EventHandler(this.btnSave_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.DefaultExt = "sql";
            this.openFileDialog1.FileName = "openFileDialog1";
            this.openFileDialog1.Filter = "SQL Files *.sql|*.sql|All Files  *.*|*.*";
            // 
            // scriptConfigCtrl1
            // 
            this.scriptConfigCtrl1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.scriptConfigCtrl1.BackColor = System.Drawing.SystemColors.Control;
            this.scriptConfigCtrl1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.scriptConfigCtrl1.BuildSequenceChanged = false;
            this.scriptConfigCtrl1.DatabaseList = null;
            this.scriptConfigCtrl1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.scriptConfigCtrl1.HasChanged = false;
            this.scriptConfigCtrl1.Location = new System.Drawing.Point(8, 181);
            this.scriptConfigCtrl1.Margin = new System.Windows.Forms.Padding(0);
            this.scriptConfigCtrl1.Name = "scriptConfigCtrl1";
            this.scriptConfigCtrl1.ShowFull = false;
            this.scriptConfigCtrl1.Size = new System.Drawing.Size(1008, 128);
            this.scriptConfigCtrl1.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(77, 5);
            this.label1.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(742, 16);
            this.label1.TabIndex = 5;
            this.label1.Text = "\"Default\" scripts are generally maintenance scripts and  will be added to any new" +
    "ly created build files";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label2.Location = new System.Drawing.Point(8, 37);
            this.label2.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(235, 13);
            this.label2.TabIndex = 6;
            this.label2.Text = "The scripts are copied to and may be located at:";
            // 
            // lnkScriptPath
            // 
            this.lnkScriptPath.AutoSize = true;
            this.lnkScriptPath.Location = new System.Drawing.Point(282, 37);
            this.lnkScriptPath.Margin = new System.Windows.Forms.Padding(4, 0, 4, 0);
            this.lnkScriptPath.Name = "lnkScriptPath";
            this.lnkScriptPath.Size = new System.Drawing.Size(31, 15);
            this.lnkScriptPath.TabIndex = 7;
            this.lnkScriptPath.TabStop = true;
            this.lnkScriptPath.Text = "Path";
            this.lnkScriptPath.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkScriptPath_LinkClicked);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statGeneral,
            this.progBar});
            this.statusStrip1.Location = new System.Drawing.Point(0, 359);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            this.statusStrip1.Size = new System.Drawing.Size(1026, 24);
            this.statusStrip1.TabIndex = 8;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statGeneral
            // 
            this.statGeneral.Name = "statGeneral";
            this.statGeneral.Size = new System.Drawing.Size(890, 19);
            this.statGeneral.Spring = true;
            this.statGeneral.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // progBar
            // 
            this.progBar.Name = "progBar";
            this.progBar.Size = new System.Drawing.Size(117, 18);
            // 
            // bgLoadScript
            // 
            this.bgLoadScript.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgLoadScript_DoWork);
            this.bgLoadScript.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgLoadScript_RunWorkerCompleted);
            // 
            // DefaultScriptMaintenanceForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(1026, 383);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.lnkScriptPath);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnSave);
            this.Controls.Add(this.btnAddNew);
            this.Controls.Add(this.scriptConfigCtrl1);
            this.Controls.Add(this.lstScripts);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "DefaultScriptMaintenanceForm";
            this.Text = "\"Default Script\" Maintenance";
            this.Load += new System.EventHandler(this.DefaultScriptMaintenanceForm_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView lstScripts;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private ScriptConfigCtrl scriptConfigCtrl1;
        private System.Windows.Forms.Button btnAddNew;
        private System.Windows.Forms.Button btnSave;
        private System.Windows.Forms.OpenFileDialog openFileDialog1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.LinkLabel lnkScriptPath;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statGeneral;
        private System.Windows.Forms.ToolStripProgressBar progBar;
        private System.ComponentModel.BackgroundWorker bgLoadScript;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem deleteDefaultScriptToolStripMenuItem;


    }
}