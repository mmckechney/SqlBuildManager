namespace SqlSync.SqlBuild.MultiDb
{
    partial class ObjectComparisonReportForm : StatusReportForm
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
            this.lstDbStatus = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.btnAnalysis = new System.Windows.Forms.Button();
            this.chkScriptThreaded = new System.Windows.Forms.CheckBox();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(273, 38);
            // 
            // bgWorker
            // 
            this.bgWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgWorker_RunWorkerCompleted_1);
            this.bgWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgWorker_ProgressChanged_1);
            // 
            // lstDbStatus
            // 
            this.lstDbStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstDbStatus.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3});
            this.lstDbStatus.GridLines = true;
            this.lstDbStatus.Location = new System.Drawing.Point(15, 94);
            this.lstDbStatus.Name = "lstDbStatus";
            this.lstDbStatus.Size = new System.Drawing.Size(482, 147);
            this.lstDbStatus.TabIndex = 8;
            this.lstDbStatus.UseCompatibleStateImageBehavior = false;
            this.lstDbStatus.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Server";
            this.columnHeader1.Width = 132;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Database";
            this.columnHeader2.Width = 132;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Status";
            this.columnHeader3.Width = 196;
            // 
            // btnAnalysis
            // 
            this.btnAnalysis.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnAnalysis.Enabled = false;
            this.btnAnalysis.Location = new System.Drawing.Point(353, 244);
            this.btnAnalysis.Name = "btnAnalysis";
            this.btnAnalysis.Size = new System.Drawing.Size(144, 23);
            this.btnAnalysis.TabIndex = 9;
            this.btnAnalysis.Text = "Additional Analysis";
            this.btnAnalysis.UseVisualStyleBackColor = true;
            this.btnAnalysis.Click += new System.EventHandler(this.btnAnalysis_Click);
            // 
            // chkScriptThreaded
            // 
            this.chkScriptThreaded.AutoSize = true;
            this.chkScriptThreaded.Checked = true;
            this.chkScriptThreaded.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkScriptThreaded.Location = new System.Drawing.Point(273, 13);
            this.chkScriptThreaded.Name = "chkScriptThreaded";
            this.chkScriptThreaded.Size = new System.Drawing.Size(203, 17);
            this.chkScriptThreaded.TabIndex = 10;
            this.chkScriptThreaded.Text = "Script databases in parallel (threaded)";
            this.chkScriptThreaded.UseVisualStyleBackColor = true;
            this.chkScriptThreaded.CheckedChanged += new System.EventHandler(this.chkScriptThreaded_CheckedChanged);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::SqlSync.Properties.Resources.Help_2;
            this.pictureBox1.Location = new System.Drawing.Point(488, 1);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(21, 16);
            this.pictureBox1.TabIndex = 11;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // ObjectComparisonReportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.ClientSize = new System.Drawing.Size(509, 292);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.chkScriptThreaded);
            this.Controls.Add(this.btnAnalysis);
            this.Controls.Add(this.lstDbStatus);
            this.Name = "ObjectComparisonReportForm";
            this.Text = "Multi-Database Object Comparison Report";
            this.Controls.SetChildIndex(this.lblServerCount, 0);
            this.Controls.SetChildIndex(this.lblDatabaseCount, 0);
            this.Controls.SetChildIndex(this.btnGenerate, 0);
            this.Controls.SetChildIndex(this.lstDbStatus, 0);
            this.Controls.SetChildIndex(this.btnAnalysis, 0);
            this.Controls.SetChildIndex(this.chkScriptThreaded, 0);
            this.Controls.SetChildIndex(this.label1, 0);
            this.Controls.SetChildIndex(this.ddOutputType, 0);
            this.Controls.SetChildIndex(this.label2, 0);
            this.Controls.SetChildIndex(this.label3, 0);
            this.Controls.SetChildIndex(this.pictureBox1, 0);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListView lstDbStatus;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Button btnAnalysis;
        private System.Windows.Forms.CheckBox chkScriptThreaded;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}
