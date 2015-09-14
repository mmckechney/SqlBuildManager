using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Text;
using System.Data;
using System.Windows.Forms;
using SqlSync.SqlBuild;
using System.Xml.Serialization;
namespace SqlSync.BuildHistory
{
	/// <summary>
	/// Summary description for PastBuildReviewForm.
	/// </summary>
	public class PastBuildReviewForm : System.Windows.Forms.Form
    {
        private SqlSync.SqlBuild.SqlSyncBuildData sqlSyncBuildData1;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem viewScriptToolStripMenuItem;
        private SplitContainer splitContainer1;
        private DataGridView dgDetail;
        private BindingSource scriptRunBindingSource;
        private BindingSource sqlSyncBuildDataBindingSource;
        private DataGridView dgMaster;
        private BindingSource buildBindingSource;
        private BindingSource sqlSyncBuildProjectBindingSource;
        private DataGridViewTextBoxColumn buildStartDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn buildEndDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn serverNameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn nameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn buildTypeDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn finalStatusDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn userIdDataGridViewTextBoxColumn;
        private Label label2;
        private Label label1;
        private DataGridViewTextBoxColumn fileNameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn resultsDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn runOrderDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn databaseDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn runStartDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn runEndDataGridViewTextBoxColumn;
        private DataGridViewCheckBoxColumn successDataGridViewCheckBoxColumn;
        private DataGridViewTextBoxColumn fileHashDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn scriptRunIdDataGridViewTextBoxColumn;
        private ContextMenuStrip ctxMaster;
        private ToolStripMenuItem saveBuildDetailsForSelectedRowsToolStripMenuItem;
        private SaveFileDialog saveFileDialog1;
        private IContainer components;

		public PastBuildReviewForm(SqlSyncBuildData buildData)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			this.sqlSyncBuildData1 = buildData;

		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PastBuildReviewForm));
            this.sqlSyncBuildData1 = new SqlSync.SqlBuild.SqlSyncBuildData();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.viewScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.label2 = new System.Windows.Forms.Label();
            this.dgMaster = new System.Windows.Forms.DataGridView();
            this.buildStartDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.buildEndDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.serverNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.buildTypeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.finalStatusDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.userIdDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.buildBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.sqlSyncBuildDataBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.label1 = new System.Windows.Forms.Label();
            this.dgDetail = new System.Windows.Forms.DataGridView();
            this.fileNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.resultsDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.runOrderDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.databaseDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.runStartDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.runEndDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.successDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            this.fileHashDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.scriptRunIdDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.scriptRunBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.sqlSyncBuildProjectBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.ctxMaster = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.saveBuildDetailsForSelectedRowsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)(this.sqlSyncBuildData1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dgMaster)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.buildBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sqlSyncBuildDataBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgDetail)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.scriptRunBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sqlSyncBuildProjectBindingSource)).BeginInit();
            this.ctxMaster.SuspendLayout();
            this.SuspendLayout();
            // 
            // sqlSyncBuildData1
            // 
            this.sqlSyncBuildData1.DataSetName = "SqlSyncBuildData";
            this.sqlSyncBuildData1.EnforceConstraints = false;
            this.sqlSyncBuildData1.Locale = new System.Globalization.CultureInfo("en-US");
            this.sqlSyncBuildData1.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewScriptToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(146, 26);
            // 
            // viewScriptToolStripMenuItem
            // 
            this.viewScriptToolStripMenuItem.Name = "viewScriptToolStripMenuItem";
            this.viewScriptToolStripMenuItem.Size = new System.Drawing.Size(145, 22);
            this.viewScriptToolStripMenuItem.Text = "View Results";
            this.viewScriptToolStripMenuItem.Click += new System.EventHandler(this.viewScriptToolStripMenuItem_Click);
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 0);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.label2);
            this.splitContainer1.Panel1.Controls.Add(this.dgMaster);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.label1);
            this.splitContainer1.Panel2.Controls.Add(this.dgDetail);
            this.splitContainer1.Size = new System.Drawing.Size(1096, 534);
            this.splitContainer1.SplitterDistance = 173;
            this.splitContainer1.TabIndex = 2;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(3, 5);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 16);
            this.label2.TabIndex = 4;
            this.label2.Text = "Build List:";
            // 
            // dgMaster
            // 
            this.dgMaster.AllowUserToAddRows = false;
            this.dgMaster.AllowUserToDeleteRows = false;
            this.dgMaster.AllowUserToOrderColumns = true;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgMaster.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dgMaster.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dgMaster.AutoGenerateColumns = false;
            this.dgMaster.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.buildStartDataGridViewTextBoxColumn,
            this.buildEndDataGridViewTextBoxColumn,
            this.serverNameDataGridViewTextBoxColumn,
            this.nameDataGridViewTextBoxColumn,
            this.buildTypeDataGridViewTextBoxColumn,
            this.finalStatusDataGridViewTextBoxColumn,
            this.userIdDataGridViewTextBoxColumn});
            this.dgMaster.ContextMenuStrip = this.ctxMaster;
            this.dgMaster.DataSource = this.buildBindingSource;
            this.dgMaster.Location = new System.Drawing.Point(0, 34);
            this.dgMaster.Name = "dgMaster";
            this.dgMaster.ReadOnly = true;
            this.dgMaster.RowHeadersVisible = false;
            this.dgMaster.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dgMaster.Size = new System.Drawing.Size(1096, 149);
            this.dgMaster.TabIndex = 3;
            this.dgMaster.CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(this.dgMaster_CellEnter);
            this.dgMaster.Click += new System.EventHandler(this.dgMaster_Click);
            // 
            // buildStartDataGridViewTextBoxColumn
            // 
            this.buildStartDataGridViewTextBoxColumn.DataPropertyName = "BuildStart";
            this.buildStartDataGridViewTextBoxColumn.HeaderText = "Start";
            this.buildStartDataGridViewTextBoxColumn.Name = "buildStartDataGridViewTextBoxColumn";
            this.buildStartDataGridViewTextBoxColumn.ReadOnly = true;
            this.buildStartDataGridViewTextBoxColumn.Width = 150;
            // 
            // buildEndDataGridViewTextBoxColumn
            // 
            this.buildEndDataGridViewTextBoxColumn.DataPropertyName = "BuildEnd";
            this.buildEndDataGridViewTextBoxColumn.HeaderText = "End";
            this.buildEndDataGridViewTextBoxColumn.Name = "buildEndDataGridViewTextBoxColumn";
            this.buildEndDataGridViewTextBoxColumn.ReadOnly = true;
            this.buildEndDataGridViewTextBoxColumn.Width = 150;
            // 
            // serverNameDataGridViewTextBoxColumn
            // 
            this.serverNameDataGridViewTextBoxColumn.DataPropertyName = "ServerName";
            this.serverNameDataGridViewTextBoxColumn.HeaderText = "ServerName";
            this.serverNameDataGridViewTextBoxColumn.Name = "serverNameDataGridViewTextBoxColumn";
            this.serverNameDataGridViewTextBoxColumn.ReadOnly = true;
            this.serverNameDataGridViewTextBoxColumn.Width = 200;
            // 
            // nameDataGridViewTextBoxColumn
            // 
            this.nameDataGridViewTextBoxColumn.DataPropertyName = "Name";
            this.nameDataGridViewTextBoxColumn.HeaderText = "Description";
            this.nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
            this.nameDataGridViewTextBoxColumn.ReadOnly = true;
            this.nameDataGridViewTextBoxColumn.Width = 200;
            // 
            // buildTypeDataGridViewTextBoxColumn
            // 
            this.buildTypeDataGridViewTextBoxColumn.DataPropertyName = "BuildType";
            this.buildTypeDataGridViewTextBoxColumn.HeaderText = "Build Type";
            this.buildTypeDataGridViewTextBoxColumn.Name = "buildTypeDataGridViewTextBoxColumn";
            this.buildTypeDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // finalStatusDataGridViewTextBoxColumn
            // 
            this.finalStatusDataGridViewTextBoxColumn.DataPropertyName = "FinalStatus";
            this.finalStatusDataGridViewTextBoxColumn.HeaderText = "Final Status";
            this.finalStatusDataGridViewTextBoxColumn.Name = "finalStatusDataGridViewTextBoxColumn";
            this.finalStatusDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // userIdDataGridViewTextBoxColumn
            // 
            this.userIdDataGridViewTextBoxColumn.DataPropertyName = "UserId";
            this.userIdDataGridViewTextBoxColumn.HeaderText = "UserId";
            this.userIdDataGridViewTextBoxColumn.Name = "userIdDataGridViewTextBoxColumn";
            this.userIdDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // buildBindingSource
            // 
            this.buildBindingSource.DataMember = "Build";
            this.buildBindingSource.DataSource = this.sqlSyncBuildDataBindingSource;
            // 
            // sqlSyncBuildDataBindingSource
            // 
            this.sqlSyncBuildDataBindingSource.DataSource = typeof(SqlSync.SqlBuild.SqlSyncBuildData);
            this.sqlSyncBuildDataBindingSource.Position = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(3, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(86, 16);
            this.label1.TabIndex = 3;
            this.label1.Text = "Build Details:";
            // 
            // dgDetail
            // 
            this.dgDetail.AllowUserToAddRows = false;
            this.dgDetail.AllowUserToDeleteRows = false;
            this.dgDetail.AllowUserToOrderColumns = true;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dgDetail.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle2;
            this.dgDetail.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dgDetail.AutoGenerateColumns = false;
            this.dgDetail.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.fileNameDataGridViewTextBoxColumn,
            this.resultsDataGridViewTextBoxColumn,
            this.runOrderDataGridViewTextBoxColumn,
            this.databaseDataGridViewTextBoxColumn,
            this.runStartDataGridViewTextBoxColumn,
            this.runEndDataGridViewTextBoxColumn,
            this.successDataGridViewCheckBoxColumn,
            this.fileHashDataGridViewTextBoxColumn,
            this.scriptRunIdDataGridViewTextBoxColumn});
            this.dgDetail.ContextMenuStrip = this.contextMenuStrip1;
            this.dgDetail.DataSource = this.scriptRunBindingSource;
            this.dgDetail.Location = new System.Drawing.Point(0, 31);
            this.dgDetail.Name = "dgDetail";
            this.dgDetail.ReadOnly = true;
            this.dgDetail.RowHeadersVisible = false;
            this.dgDetail.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dgDetail.Size = new System.Drawing.Size(1096, 326);
            this.dgDetail.TabIndex = 2;
            // 
            // fileNameDataGridViewTextBoxColumn
            // 
            this.fileNameDataGridViewTextBoxColumn.DataPropertyName = "FileName";
            this.fileNameDataGridViewTextBoxColumn.HeaderText = "File Name";
            this.fileNameDataGridViewTextBoxColumn.Name = "fileNameDataGridViewTextBoxColumn";
            this.fileNameDataGridViewTextBoxColumn.ReadOnly = true;
            this.fileNameDataGridViewTextBoxColumn.Width = 200;
            // 
            // resultsDataGridViewTextBoxColumn
            // 
            this.resultsDataGridViewTextBoxColumn.DataPropertyName = "Results";
            this.resultsDataGridViewTextBoxColumn.HeaderText = "Results";
            this.resultsDataGridViewTextBoxColumn.Name = "resultsDataGridViewTextBoxColumn";
            this.resultsDataGridViewTextBoxColumn.ReadOnly = true;
            this.resultsDataGridViewTextBoxColumn.Width = 200;
            // 
            // runOrderDataGridViewTextBoxColumn
            // 
            this.runOrderDataGridViewTextBoxColumn.DataPropertyName = "RunOrder";
            this.runOrderDataGridViewTextBoxColumn.HeaderText = "Order";
            this.runOrderDataGridViewTextBoxColumn.Name = "runOrderDataGridViewTextBoxColumn";
            this.runOrderDataGridViewTextBoxColumn.ReadOnly = true;
            this.runOrderDataGridViewTextBoxColumn.Width = 65;
            // 
            // databaseDataGridViewTextBoxColumn
            // 
            this.databaseDataGridViewTextBoxColumn.DataPropertyName = "Database";
            this.databaseDataGridViewTextBoxColumn.HeaderText = "Database";
            this.databaseDataGridViewTextBoxColumn.Name = "databaseDataGridViewTextBoxColumn";
            this.databaseDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // runStartDataGridViewTextBoxColumn
            // 
            this.runStartDataGridViewTextBoxColumn.DataPropertyName = "RunStart";
            this.runStartDataGridViewTextBoxColumn.HeaderText = "Start";
            this.runStartDataGridViewTextBoxColumn.Name = "runStartDataGridViewTextBoxColumn";
            this.runStartDataGridViewTextBoxColumn.ReadOnly = true;
            this.runStartDataGridViewTextBoxColumn.Width = 120;
            // 
            // runEndDataGridViewTextBoxColumn
            // 
            this.runEndDataGridViewTextBoxColumn.DataPropertyName = "RunEnd";
            this.runEndDataGridViewTextBoxColumn.HeaderText = "End";
            this.runEndDataGridViewTextBoxColumn.Name = "runEndDataGridViewTextBoxColumn";
            this.runEndDataGridViewTextBoxColumn.ReadOnly = true;
            this.runEndDataGridViewTextBoxColumn.Width = 120;
            // 
            // successDataGridViewCheckBoxColumn
            // 
            this.successDataGridViewCheckBoxColumn.DataPropertyName = "Success";
            this.successDataGridViewCheckBoxColumn.HeaderText = "Success";
            this.successDataGridViewCheckBoxColumn.Name = "successDataGridViewCheckBoxColumn";
            this.successDataGridViewCheckBoxColumn.ReadOnly = true;
            this.successDataGridViewCheckBoxColumn.Width = 70;
            // 
            // fileHashDataGridViewTextBoxColumn
            // 
            this.fileHashDataGridViewTextBoxColumn.DataPropertyName = "FileHash";
            this.fileHashDataGridViewTextBoxColumn.HeaderText = "FileHash";
            this.fileHashDataGridViewTextBoxColumn.Name = "fileHashDataGridViewTextBoxColumn";
            this.fileHashDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // scriptRunIdDataGridViewTextBoxColumn
            // 
            this.scriptRunIdDataGridViewTextBoxColumn.DataPropertyName = "ScriptRunId";
            this.scriptRunIdDataGridViewTextBoxColumn.HeaderText = "Run ID";
            this.scriptRunIdDataGridViewTextBoxColumn.Name = "scriptRunIdDataGridViewTextBoxColumn";
            this.scriptRunIdDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // scriptRunBindingSource
            // 
            this.scriptRunBindingSource.DataMember = "ScriptRun";
            this.scriptRunBindingSource.DataSource = this.sqlSyncBuildDataBindingSource;
            // 
            // sqlSyncBuildProjectBindingSource
            // 
            this.sqlSyncBuildProjectBindingSource.DataMember = "SqlSyncBuildProject";
            this.sqlSyncBuildProjectBindingSource.DataSource = this.sqlSyncBuildDataBindingSource;
            // 
            // ctxMaster
            // 
            this.ctxMaster.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveBuildDetailsForSelectedRowsToolStripMenuItem});
            this.ctxMaster.Name = "ctxMaster";
            this.ctxMaster.Size = new System.Drawing.Size(260, 26);
            // 
            // saveBuildDetailsForSelectedRowsToolStripMenuItem
            // 
            this.saveBuildDetailsForSelectedRowsToolStripMenuItem.Name = "saveBuildDetailsForSelectedRowsToolStripMenuItem";
            this.saveBuildDetailsForSelectedRowsToolStripMenuItem.Size = new System.Drawing.Size(259, 22);
            this.saveBuildDetailsForSelectedRowsToolStripMenuItem.Text = "Save Build Details for Selected Rows";
            this.saveBuildDetailsForSelectedRowsToolStripMenuItem.Click += new System.EventHandler(this.saveBuildDetailsForSelectedRowsToolStripMenuItem_Click);
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "xml";
            this.saveFileDialog1.Filter = "XML Files *.xml|*.xml|All Files *.*|*.*";
            this.saveFileDialog1.Title = "Save Build History";
            // 
            // PastBuildReviewForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(1096, 534);
            this.Controls.Add(this.splitContainer1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PastBuildReviewForm";
            this.Text = "Review Past Builds";
            this.Load += new System.EventHandler(this.PastBuildReviewForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.sqlSyncBuildData1)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel1.PerformLayout();
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.dgMaster)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.buildBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sqlSyncBuildDataBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.dgDetail)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.scriptRunBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sqlSyncBuildProjectBindingSource)).EndInit();
            this.ctxMaster.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		private void PastBuildReviewForm_Load(object sender, System.EventArgs e)
		{

            //this.scriptRunBindingSource.DataSource = this.sqlSyncBuildData1.ScriptRun;
            this.buildBindingSource.DataSource = this.sqlSyncBuildData1.Build;
        }

        private void dgMaster_Click(object sender, EventArgs e)
        {
            if (this.dgMaster.SelectedCells.Count == 0)
                return;

            SqlSyncBuildData.BuildRow row = (SqlSyncBuildData.BuildRow)((System.Data.DataRowView)this.dgMaster.SelectedCells[0].OwningRow.DataBoundItem).Row;
            System.Data.DataView view = this.sqlSyncBuildData1.ScriptRun.DefaultView;
            view.RowFilter = this.sqlSyncBuildData1.ScriptRun.Build_IdColumn.ColumnName + "= '" + row.Build_Id.ToString() + "'";
            this.scriptRunBindingSource.DataSource = view;

        }

        private void viewScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.dgDetail.SelectedCells.Count == 0)
                return;


            SqlSyncBuildData.ScriptRunRow row = (SqlSyncBuildData.ScriptRunRow)((System.Data.DataRowView)this.dgDetail.SelectedCells[0].OwningRow.DataBoundItem).Row;
            ScriptDisplayForm frmDisp = new ScriptDisplayForm(row.Results, row.Database, row.FileName);
            frmDisp.ShowDialog();
            frmDisp.Dispose();

        }

        private void dgMaster_CellEnter(object sender, DataGridViewCellEventArgs e)
        {
            dgMaster_Click(null, EventArgs.Empty);
        }

        private void saveBuildDetailsForSelectedRowsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.dgMaster.SelectedCells.Count == 0)
                return;

            List<SqlSyncBuildData.BuildRow> rows = new List<SqlSyncBuildData.BuildRow>();
            for (int i = 0; i < this.dgMaster.SelectedCells.Count; i++)
            {
                SqlSyncBuildData.BuildRow row = (SqlSyncBuildData.BuildRow)((System.Data.DataRowView)this.dgMaster.SelectedCells[i].OwningRow.DataBoundItem).Row;
                if (!rows.Contains(row))
                    rows.Add(row);
            }

            if (DialogResult.OK == saveFileDialog1.ShowDialog())
            {
                try
                {
                    //Perform a deep copy of the build data...
                    SqlSyncBuildData data = new SqlSyncBuildData();
                    using (StringReader sr = new StringReader("<?xml version=\"1.0\" standalone=\"yes\"?>"+ this.sqlSyncBuildData1.GetXml()))
                    {
                        data.ReadXml(sr);
                    }

                    //Get ID's for selected rows...
                    StringBuilder sb = new StringBuilder("(");
                    foreach (SqlSyncBuildData.BuildRow row in rows)
                        sb.Append("'" + row.BuildId.ToString() + "',");
                    sb.Length = sb.Length - 1;
                    sb.Append(")");

                    //Filter out any rows that were not selected
                    System.Data.DataView view = data.Build.DefaultView;
                    view.RowFilter = data.Build.BuildIdColumn.ColumnName + " not in " + sb.ToString();
                    
                    //Delete the non-selected rows from the build data copy object.
                    while(view.Count > 0)
                    {
                        DataRow[] kids = view[0].Row.GetChildRows("Builds_Build");
                        for (int j = 0; j < kids.Length; j++)
                            kids[j].Delete();

                        view[0].Row.Delete();
                    }

                    //Save the file...
                    string fileName = saveFileDialog1.FileName;
                    data.AcceptChanges();
                    data.WriteXml(fileName);

                    if (DialogResult.Yes == MessageBox.Show("Saved history to " + fileName + "\r\nOpen this file?", "Save Complete", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                        System.Diagnostics.Process.Start(fileName);
                }
                catch (Exception exe)
                {
                    MessageBox.Show("There was an error saving the history.\r\n" + exe.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

                }
            }
        }

       

        

        
	}
}
