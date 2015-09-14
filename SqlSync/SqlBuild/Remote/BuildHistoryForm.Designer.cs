namespace SqlSync.SqlBuild.Remote
{
    partial class BuildHistoryForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BuildHistoryForm));
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.submissionDateDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.buildPackageNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.requestedByDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.ReturnValueString = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.rootLogPathDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.bindingSource1 = new System.Windows.Forms.BindingSource(this.components);
            this.remoteExecutionLogsContextMenuStrip1 = new SqlSync.Controls.RemoteExecutionLogsContextMenuStrip();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).BeginInit();
            this.SuspendLayout();
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGridView1.AutoGenerateColumns = false;
            this.dataGridView1.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.submissionDateDataGridViewTextBoxColumn,
            this.buildPackageNameDataGridViewTextBoxColumn,
            this.requestedByDataGridViewTextBoxColumn,
            this.ReturnValueString,
            this.rootLogPathDataGridViewTextBoxColumn});
            this.dataGridView1.ContextMenuStrip = this.remoteExecutionLogsContextMenuStrip1;
            this.dataGridView1.DataSource = this.bindingSource1;
            this.dataGridView1.Location = new System.Drawing.Point(8, 7);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.Size = new System.Drawing.Size(952, 381);
            this.dataGridView1.TabIndex = 0;
            // 
            // submissionDateDataGridViewTextBoxColumn
            // 
            this.submissionDateDataGridViewTextBoxColumn.DataPropertyName = "submissionDate";
            this.submissionDateDataGridViewTextBoxColumn.HeaderText = "Submission Date";
            this.submissionDateDataGridViewTextBoxColumn.Name = "submissionDateDataGridViewTextBoxColumn";
            this.submissionDateDataGridViewTextBoxColumn.Width = 150;
            // 
            // buildPackageNameDataGridViewTextBoxColumn
            // 
            this.buildPackageNameDataGridViewTextBoxColumn.DataPropertyName = "buildPackageName";
            this.buildPackageNameDataGridViewTextBoxColumn.HeaderText = "Build Package Name";
            this.buildPackageNameDataGridViewTextBoxColumn.Name = "buildPackageNameDataGridViewTextBoxColumn";
            this.buildPackageNameDataGridViewTextBoxColumn.Width = 200;
            // 
            // requestedByDataGridViewTextBoxColumn
            // 
            this.requestedByDataGridViewTextBoxColumn.DataPropertyName = "requestedBy";
            this.requestedByDataGridViewTextBoxColumn.HeaderText = "Requested By";
            this.requestedByDataGridViewTextBoxColumn.Name = "requestedByDataGridViewTextBoxColumn";
            this.requestedByDataGridViewTextBoxColumn.Width = 130;
            // 
            // ReturnValueString
            // 
            this.ReturnValueString.DataPropertyName = "ReturnValueString";
            this.ReturnValueString.HeaderText = "Execution Result";
            this.ReturnValueString.Name = "ReturnValueString";
            this.ReturnValueString.ReadOnly = true;
            this.ReturnValueString.Width = 150;
            // 
            // rootLogPathDataGridViewTextBoxColumn
            // 
            this.rootLogPathDataGridViewTextBoxColumn.DataPropertyName = "rootLogPath";
            this.rootLogPathDataGridViewTextBoxColumn.HeaderText = "Local Root Log Path";
            this.rootLogPathDataGridViewTextBoxColumn.Name = "rootLogPathDataGridViewTextBoxColumn";
            this.rootLogPathDataGridViewTextBoxColumn.Width = 300;
            // 
            // bindingSource1
            // 
            this.bindingSource1.DataSource = typeof(SqlBuildManager.ServiceClient.Sbm.BuildService.BuildRecord);
            // 
            // remoteExecutionLogsContextMenuStrip1
            // 
            this.remoteExecutionLogsContextMenuStrip1.Name = "remoteExecutionLogsContextMenuStrip1";
            this.remoteExecutionLogsContextMenuStrip1.Size = new System.Drawing.Size(430, 193);
            // 
            // BuildHistoryForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(968, 394);
            this.Controls.Add(this.dataGridView1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "BuildHistoryForm";
            this.Text = "BuildHistoryForm";
            this.Load += new System.EventHandler(this.BuildHistoryForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.BuildHistoryForm_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.bindingSource1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView dataGridView1;
        private System.Windows.Forms.BindingSource bindingSource1;
        private System.Windows.Forms.DataGridViewTextBoxColumn submissionDateDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn buildPackageNameDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn requestedByDataGridViewTextBoxColumn;
        private System.Windows.Forms.DataGridViewTextBoxColumn ReturnValueString;
        private System.Windows.Forms.DataGridViewTextBoxColumn rootLogPathDataGridViewTextBoxColumn;
        private Controls.RemoteExecutionLogsContextMenuStrip remoteExecutionLogsContextMenuStrip1;
    }
}