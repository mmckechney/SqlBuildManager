namespace SqlSync.Test
{
    partial class SprocTestForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SprocTestForm));
            this.listView1 = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.showDetailedResultsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.saveAllTestResultsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statGeneral = new System.Windows.Forms.ToolStripStatusLabel();
            this.statSpCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.statTestCaseRunCount = new System.Windows.Forms.ToolStripStatusLabel();
            this.statPassed = new System.Windows.Forms.ToolStripStatusLabel();
            this.statFailed = new System.Windows.Forms.ToolStripStatusLabel();
            this.statProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.bgTestRunner = new System.ComponentModel.BackgroundWorker();
            this.btnCancel = new System.Windows.Forms.Button();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.contextMenuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // listView1
            // 
            this.listView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.listView1.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.listView1.ContextMenuStrip = this.contextMenuStrip1;
            this.listView1.FullRowSelect = true;
            this.listView1.GridLines = true;
            this.listView1.HideSelection = false;
            this.listView1.Location = new System.Drawing.Point(14, 35);
            this.listView1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.listView1.Name = "listView1";
            this.listView1.Size = new System.Drawing.Size(955, 272);
            this.listView1.TabIndex = 2;
            this.listView1.UseCompatibleStateImageBehavior = false;
            this.listView1.View = System.Windows.Forms.View.Details;
            this.listView1.Click += new System.EventHandler(this.listView1_Click);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Passed?";
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "SP Name";
            this.columnHeader2.Width = 200;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Test Case Name";
            this.columnHeader3.Width = 200;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Result Message";
            this.columnHeader4.Width = 331;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.showDetailedResultsToolStripMenuItem,
            this.toolStripSeparator1,
            this.saveAllTestResultsToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(190, 54);
            // 
            // showDetailedResultsToolStripMenuItem
            // 
            this.showDetailedResultsToolStripMenuItem.Name = "showDetailedResultsToolStripMenuItem";
            this.showDetailedResultsToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.showDetailedResultsToolStripMenuItem.Text = "Show Detailed Results";
            this.showDetailedResultsToolStripMenuItem.Click += new System.EventHandler(this.showDetailedResultsToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(186, 6);
            // 
            // saveAllTestResultsToolStripMenuItem
            // 
            this.saveAllTestResultsToolStripMenuItem.Name = "saveAllTestResultsToolStripMenuItem";
            this.saveAllTestResultsToolStripMenuItem.Size = new System.Drawing.Size(189, 22);
            this.saveAllTestResultsToolStripMenuItem.Text = "Save All Test Results";
            this.saveAllTestResultsToolStripMenuItem.Click += new System.EventHandler(this.saveAllTestResultsToolStripMenuItem_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statGeneral,
            this.statSpCount,
            this.statTestCaseRunCount,
            this.statPassed,
            this.statFailed,
            this.statProgressBar});
            this.statusStrip1.Location = new System.Drawing.Point(0, 319);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Padding = new System.Windows.Forms.Padding(1, 0, 16, 0);
            this.statusStrip1.Size = new System.Drawing.Size(983, 24);
            this.statusStrip1.TabIndex = 3;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statGeneral
            // 
            this.statGeneral.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.statGeneral.Name = "statGeneral";
            this.statGeneral.Size = new System.Drawing.Size(298, 19);
            this.statGeneral.Spring = true;
            this.statGeneral.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statSpCount
            // 
            this.statSpCount.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.statSpCount.Name = "statSpCount";
            this.statSpCount.Padding = new System.Windows.Forms.Padding(0, 0, 20, 0);
            this.statSpCount.Size = new System.Drawing.Size(175, 19);
            this.statSpCount.Text = "Stored Procedures Tested: 0";
            this.statSpCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statTestCaseRunCount
            // 
            this.statTestCaseRunCount.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.statTestCaseRunCount.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.statTestCaseRunCount.Name = "statTestCaseRunCount";
            this.statTestCaseRunCount.Padding = new System.Windows.Forms.Padding(0, 0, 40, 0);
            this.statTestCaseRunCount.Size = new System.Drawing.Size(140, 19);
            this.statTestCaseRunCount.Text = "Test Cases Run: 0";
            this.statTestCaseRunCount.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statPassed
            // 
            this.statPassed.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.statPassed.Name = "statPassed";
            this.statPassed.Padding = new System.Windows.Forms.Padding(0, 0, 30, 0);
            this.statPassed.Size = new System.Drawing.Size(92, 19);
            this.statPassed.Text = "Passed : 0";
            this.statPassed.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statFailed
            // 
            this.statFailed.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.statFailed.Name = "statFailed";
            this.statFailed.Padding = new System.Windows.Forms.Padding(0, 0, 30, 0);
            this.statFailed.Size = new System.Drawing.Size(84, 19);
            this.statFailed.Text = "Failed: 0";
            this.statFailed.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statProgressBar
            // 
            this.statProgressBar.Name = "statProgressBar";
            this.statProgressBar.Size = new System.Drawing.Size(175, 18);
            // 
            // bgTestRunner
            // 
            this.bgTestRunner.WorkerReportsProgress = true;
            this.bgTestRunner.WorkerSupportsCancellation = true;
            this.bgTestRunner.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgTestRunner_DoWork);
            this.bgTestRunner.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgTestRunner_ProgressChanged);
            this.bgTestRunner.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgTestRunner_RunWorkerCompleted);
            // 
            // btnCancel
            // 
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(14, 3);
            this.btnCancel.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(102, 27);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel Tests";
            this.btnCancel.UseVisualStyleBackColor = true;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "xml";
            this.saveFileDialog1.Filter = "XML Files *.xml|*.xml|All Files *.*|*.*";
            this.saveFileDialog1.Title = "Save Test Results";
            // 
            // SprocTestForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(983, 343);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.listView1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            this.Name = "SprocTestForm";
            this.Text = "Stored Procedure Test Results";
            this.Load += new System.EventHandler(this.SprocTestForm_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView listView1;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.StatusStrip statusStrip1;
        private System.ComponentModel.BackgroundWorker bgTestRunner;
        private System.Windows.Forms.ToolStripStatusLabel statGeneral;
        private System.Windows.Forms.ToolStripProgressBar statProgressBar;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.ToolStripStatusLabel statTestCaseRunCount;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripStatusLabel statPassed;
        private System.Windows.Forms.ToolStripStatusLabel statFailed;
        private System.Windows.Forms.ToolStripStatusLabel statSpCount;
        private System.Windows.Forms.ToolStripMenuItem showDetailedResultsToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem saveAllTestResultsToolStripMenuItem;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
    }
}