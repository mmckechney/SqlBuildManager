namespace SqlSync.SqlBuild.MultiDb
{
    partial class ObjectComparisonAnalysisForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ObjectComparisonAnalysisForm));
            this.label1 = new System.Windows.Forms.Label();
            this.btnReport = new System.Windows.Forms.Button();
            this.lstDbs = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(17, 11);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(286, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Select a database from the list below to use as the baseline";
            // 
            // btnReport
            // 
            this.btnReport.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnReport.Location = new System.Drawing.Point(90, 289);
            this.btnReport.Name = "btnReport";
            this.btnReport.Size = new System.Drawing.Size(142, 23);
            this.btnReport.TabIndex = 2;
            this.btnReport.Text = "Generate Report";
            this.btnReport.UseVisualStyleBackColor = true;
            this.btnReport.Click += new System.EventHandler(this.btnReport_Click);
            // 
            // lstDbs
            // 
            this.lstDbs.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstDbs.CheckBoxes = true;
            this.lstDbs.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.lstDbs.GridLines = true;
            this.lstDbs.Location = new System.Drawing.Point(20, 27);
            this.lstDbs.Name = "lstDbs";
            this.lstDbs.Size = new System.Drawing.Size(286, 256);
            this.lstDbs.TabIndex = 3;
            this.lstDbs.UseCompatibleStateImageBehavior = false;
            this.lstDbs.View = System.Windows.Forms.View.Details;
            this.lstDbs.ItemChecked += new System.Windows.Forms.ItemCheckedEventHandler(this.lstDbs_ItemChecked);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Database";
            this.columnHeader1.Width = 255;
            // 
            // ObjectComparisonAnalysisForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(323, 324);
            this.Controls.Add(this.lstDbs);
            this.Controls.Add(this.btnReport);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ObjectComparisonAnalysisForm";
            this.Text = "Comparison Analysis ";
            this.Load += new System.EventHandler(this.ObjectComparisonAnalysisForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnReport;
        private System.Windows.Forms.ListView lstDbs;
        private System.Windows.Forms.ColumnHeader columnHeader1;
    }
}