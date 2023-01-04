using SqlSync.SqlBuild;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Text;
using System.Windows.Forms;
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
            sqlSyncBuildData1 = buildData;

        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
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
            components = new System.ComponentModel.Container();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PastBuildReviewForm));
            sqlSyncBuildData1 = new SqlSync.SqlBuild.SqlSyncBuildData();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            viewScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            label2 = new System.Windows.Forms.Label();
            dgMaster = new System.Windows.Forms.DataGridView();
            buildStartDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            buildEndDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            serverNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            nameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            buildTypeDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            finalStatusDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            userIdDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ctxMaster = new System.Windows.Forms.ContextMenuStrip(components);
            saveBuildDetailsForSelectedRowsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            buildBindingSource = new System.Windows.Forms.BindingSource(components);
            sqlSyncBuildDataBindingSource = new System.Windows.Forms.BindingSource(components);
            label1 = new System.Windows.Forms.Label();
            dgDetail = new System.Windows.Forms.DataGridView();
            fileNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            resultsDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            runOrderDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            databaseDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            runStartDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            runEndDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            successDataGridViewCheckBoxColumn = new System.Windows.Forms.DataGridViewCheckBoxColumn();
            fileHashDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            scriptRunIdDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            scriptRunBindingSource = new System.Windows.Forms.BindingSource(components);
            sqlSyncBuildProjectBindingSource = new System.Windows.Forms.BindingSource(components);
            saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            ((System.ComponentModel.ISupportInitialize)(sqlSyncBuildData1)).BeginInit();
            contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).BeginInit();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(dgMaster)).BeginInit();
            ctxMaster.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(buildBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(sqlSyncBuildDataBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(dgDetail)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(scriptRunBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(sqlSyncBuildProjectBindingSource)).BeginInit();
            SuspendLayout();
            // 
            // sqlSyncBuildData1
            // 
            sqlSyncBuildData1.DataSetName = "SqlSyncBuildData";
            sqlSyncBuildData1.EnforceConstraints = false;
            sqlSyncBuildData1.Locale = new System.Globalization.CultureInfo("en-US");
            sqlSyncBuildData1.Namespace = "http://schemas.mckechney.com/SqlSyncBuildProject.xsd";
            sqlSyncBuildData1.SchemaSerializationMode = System.Data.SchemaSerializationMode.IncludeSchema;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            viewScriptToolStripMenuItem});
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(140, 26);
            // 
            // viewScriptToolStripMenuItem
            // 
            viewScriptToolStripMenuItem.Name = "viewScriptToolStripMenuItem";
            viewScriptToolStripMenuItem.Size = new System.Drawing.Size(139, 22);
            viewScriptToolStripMenuItem.Text = "View Results";
            viewScriptToolStripMenuItem.Click += new System.EventHandler(viewScriptToolStripMenuItem_Click);
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 0);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(label2);
            splitContainer1.Panel1.Controls.Add(dgMaster);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(label1);
            splitContainer1.Panel2.Controls.Add(dgDetail);
            splitContainer1.Size = new System.Drawing.Size(1096, 534);
            splitContainer1.SplitterDistance = 173;
            splitContainer1.TabIndex = 2;
            // 
            // label2
            // 
            label2.AutoSize = true;
            label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label2.Location = new System.Drawing.Point(4, 6);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(63, 16);
            label2.TabIndex = 4;
            label2.Text = "Build List:";
            // 
            // dgMaster
            // 
            dgMaster.AllowUserToAddRows = false;
            dgMaster.AllowUserToDeleteRows = false;
            dgMaster.AllowUserToOrderColumns = true;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            dgMaster.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dgMaster.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            dgMaster.AutoGenerateColumns = false;
            dgMaster.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            buildStartDataGridViewTextBoxColumn,
            buildEndDataGridViewTextBoxColumn,
            serverNameDataGridViewTextBoxColumn,
            nameDataGridViewTextBoxColumn,
            buildTypeDataGridViewTextBoxColumn,
            finalStatusDataGridViewTextBoxColumn,
            userIdDataGridViewTextBoxColumn});
            dgMaster.ContextMenuStrip = ctxMaster;
            dgMaster.DataSource = buildBindingSource;
            dgMaster.Location = new System.Drawing.Point(0, 42);
            dgMaster.Name = "dgMaster";
            dgMaster.ReadOnly = true;
            dgMaster.RowHeadersVisible = false;
            dgMaster.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dgMaster.Size = new System.Drawing.Size(1096, 143);
            dgMaster.TabIndex = 3;
            dgMaster.CellEnter += new System.Windows.Forms.DataGridViewCellEventHandler(dgMaster_CellEnter);
            dgMaster.Click += new System.EventHandler(dgMaster_Click);
            // 
            // buildStartDataGridViewTextBoxColumn
            // 
            buildStartDataGridViewTextBoxColumn.DataPropertyName = "BuildStart";
            buildStartDataGridViewTextBoxColumn.HeaderText = "Start";
            buildStartDataGridViewTextBoxColumn.Name = "buildStartDataGridViewTextBoxColumn";
            buildStartDataGridViewTextBoxColumn.ReadOnly = true;
            buildStartDataGridViewTextBoxColumn.Width = 150;
            // 
            // buildEndDataGridViewTextBoxColumn
            // 
            buildEndDataGridViewTextBoxColumn.DataPropertyName = "BuildEnd";
            buildEndDataGridViewTextBoxColumn.HeaderText = "End";
            buildEndDataGridViewTextBoxColumn.Name = "buildEndDataGridViewTextBoxColumn";
            buildEndDataGridViewTextBoxColumn.ReadOnly = true;
            buildEndDataGridViewTextBoxColumn.Width = 150;
            // 
            // serverNameDataGridViewTextBoxColumn
            // 
            serverNameDataGridViewTextBoxColumn.DataPropertyName = "ServerName";
            serverNameDataGridViewTextBoxColumn.HeaderText = "ServerName";
            serverNameDataGridViewTextBoxColumn.Name = "serverNameDataGridViewTextBoxColumn";
            serverNameDataGridViewTextBoxColumn.ReadOnly = true;
            serverNameDataGridViewTextBoxColumn.Width = 200;
            // 
            // nameDataGridViewTextBoxColumn
            // 
            nameDataGridViewTextBoxColumn.DataPropertyName = "Name";
            nameDataGridViewTextBoxColumn.HeaderText = "Description";
            nameDataGridViewTextBoxColumn.Name = "nameDataGridViewTextBoxColumn";
            nameDataGridViewTextBoxColumn.ReadOnly = true;
            nameDataGridViewTextBoxColumn.Width = 200;
            // 
            // buildTypeDataGridViewTextBoxColumn
            // 
            buildTypeDataGridViewTextBoxColumn.DataPropertyName = "BuildType";
            buildTypeDataGridViewTextBoxColumn.HeaderText = "Build Type";
            buildTypeDataGridViewTextBoxColumn.Name = "buildTypeDataGridViewTextBoxColumn";
            buildTypeDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // finalStatusDataGridViewTextBoxColumn
            // 
            finalStatusDataGridViewTextBoxColumn.DataPropertyName = "FinalStatus";
            finalStatusDataGridViewTextBoxColumn.HeaderText = "Final Status";
            finalStatusDataGridViewTextBoxColumn.Name = "finalStatusDataGridViewTextBoxColumn";
            finalStatusDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // userIdDataGridViewTextBoxColumn
            // 
            userIdDataGridViewTextBoxColumn.DataPropertyName = "UserId";
            userIdDataGridViewTextBoxColumn.HeaderText = "UserId";
            userIdDataGridViewTextBoxColumn.Name = "userIdDataGridViewTextBoxColumn";
            userIdDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // ctxMaster
            // 
            ctxMaster.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            saveBuildDetailsForSelectedRowsToolStripMenuItem});
            ctxMaster.Name = "ctxMaster";
            ctxMaster.Size = new System.Drawing.Size(263, 26);
            // 
            // saveBuildDetailsForSelectedRowsToolStripMenuItem
            // 
            saveBuildDetailsForSelectedRowsToolStripMenuItem.Name = "saveBuildDetailsForSelectedRowsToolStripMenuItem";
            saveBuildDetailsForSelectedRowsToolStripMenuItem.Size = new System.Drawing.Size(262, 22);
            saveBuildDetailsForSelectedRowsToolStripMenuItem.Text = "Save Build Details for Selected Rows";
            saveBuildDetailsForSelectedRowsToolStripMenuItem.Click += new System.EventHandler(saveBuildDetailsForSelectedRowsToolStripMenuItem_Click);
            // 
            // buildBindingSource
            // 
            buildBindingSource.DataMember = "Build";
            buildBindingSource.DataSource = sqlSyncBuildDataBindingSource;
            // 
            // sqlSyncBuildDataBindingSource
            // 
            sqlSyncBuildDataBindingSource.DataSource = typeof(SqlSync.SqlBuild.SqlSyncBuildData);
            sqlSyncBuildDataBindingSource.Position = 0;
            // 
            // label1
            // 
            label1.AutoSize = true;
            label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label1.Location = new System.Drawing.Point(4, 11);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(85, 16);
            label1.TabIndex = 3;
            label1.Text = "Build Details:";
            // 
            // dgDetail
            // 
            dgDetail.AllowUserToAddRows = false;
            dgDetail.AllowUserToDeleteRows = false;
            dgDetail.AllowUserToOrderColumns = true;
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            dgDetail.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle2;
            dgDetail.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            dgDetail.AutoGenerateColumns = false;
            dgDetail.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            fileNameDataGridViewTextBoxColumn,
            resultsDataGridViewTextBoxColumn,
            runOrderDataGridViewTextBoxColumn,
            databaseDataGridViewTextBoxColumn,
            runStartDataGridViewTextBoxColumn,
            runEndDataGridViewTextBoxColumn,
            successDataGridViewCheckBoxColumn,
            fileHashDataGridViewTextBoxColumn,
            scriptRunIdDataGridViewTextBoxColumn});
            dgDetail.ContextMenuStrip = contextMenuStrip1;
            dgDetail.DataSource = scriptRunBindingSource;
            dgDetail.Location = new System.Drawing.Point(0, 38);
            dgDetail.Name = "dgDetail";
            dgDetail.ReadOnly = true;
            dgDetail.RowHeadersVisible = false;
            dgDetail.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dgDetail.Size = new System.Drawing.Size(1096, 319);
            dgDetail.TabIndex = 2;
            // 
            // fileNameDataGridViewTextBoxColumn
            // 
            fileNameDataGridViewTextBoxColumn.DataPropertyName = "FileName";
            fileNameDataGridViewTextBoxColumn.HeaderText = "File Name";
            fileNameDataGridViewTextBoxColumn.Name = "fileNameDataGridViewTextBoxColumn";
            fileNameDataGridViewTextBoxColumn.ReadOnly = true;
            fileNameDataGridViewTextBoxColumn.Width = 200;
            // 
            // resultsDataGridViewTextBoxColumn
            // 
            resultsDataGridViewTextBoxColumn.DataPropertyName = "Results";
            resultsDataGridViewTextBoxColumn.HeaderText = "Results";
            resultsDataGridViewTextBoxColumn.Name = "resultsDataGridViewTextBoxColumn";
            resultsDataGridViewTextBoxColumn.ReadOnly = true;
            resultsDataGridViewTextBoxColumn.Width = 200;
            // 
            // runOrderDataGridViewTextBoxColumn
            // 
            runOrderDataGridViewTextBoxColumn.DataPropertyName = "RunOrder";
            runOrderDataGridViewTextBoxColumn.HeaderText = "Order";
            runOrderDataGridViewTextBoxColumn.Name = "runOrderDataGridViewTextBoxColumn";
            runOrderDataGridViewTextBoxColumn.ReadOnly = true;
            runOrderDataGridViewTextBoxColumn.Width = 65;
            // 
            // databaseDataGridViewTextBoxColumn
            // 
            databaseDataGridViewTextBoxColumn.DataPropertyName = "Database";
            databaseDataGridViewTextBoxColumn.HeaderText = "Database";
            databaseDataGridViewTextBoxColumn.Name = "databaseDataGridViewTextBoxColumn";
            databaseDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // runStartDataGridViewTextBoxColumn
            // 
            runStartDataGridViewTextBoxColumn.DataPropertyName = "RunStart";
            runStartDataGridViewTextBoxColumn.HeaderText = "Start";
            runStartDataGridViewTextBoxColumn.Name = "runStartDataGridViewTextBoxColumn";
            runStartDataGridViewTextBoxColumn.ReadOnly = true;
            runStartDataGridViewTextBoxColumn.Width = 120;
            // 
            // runEndDataGridViewTextBoxColumn
            // 
            runEndDataGridViewTextBoxColumn.DataPropertyName = "RunEnd";
            runEndDataGridViewTextBoxColumn.HeaderText = "End";
            runEndDataGridViewTextBoxColumn.Name = "runEndDataGridViewTextBoxColumn";
            runEndDataGridViewTextBoxColumn.ReadOnly = true;
            runEndDataGridViewTextBoxColumn.Width = 120;
            // 
            // successDataGridViewCheckBoxColumn
            // 
            successDataGridViewCheckBoxColumn.DataPropertyName = "Success";
            successDataGridViewCheckBoxColumn.HeaderText = "Success";
            successDataGridViewCheckBoxColumn.Name = "successDataGridViewCheckBoxColumn";
            successDataGridViewCheckBoxColumn.ReadOnly = true;
            successDataGridViewCheckBoxColumn.Width = 70;
            // 
            // fileHashDataGridViewTextBoxColumn
            // 
            fileHashDataGridViewTextBoxColumn.DataPropertyName = "FileHash";
            fileHashDataGridViewTextBoxColumn.HeaderText = "FileHash";
            fileHashDataGridViewTextBoxColumn.Name = "fileHashDataGridViewTextBoxColumn";
            fileHashDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // scriptRunIdDataGridViewTextBoxColumn
            // 
            scriptRunIdDataGridViewTextBoxColumn.DataPropertyName = "ScriptRunId";
            scriptRunIdDataGridViewTextBoxColumn.HeaderText = "Run ID";
            scriptRunIdDataGridViewTextBoxColumn.Name = "scriptRunIdDataGridViewTextBoxColumn";
            scriptRunIdDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // scriptRunBindingSource
            // 
            scriptRunBindingSource.DataMember = "ScriptRun";
            scriptRunBindingSource.DataSource = sqlSyncBuildDataBindingSource;
            // 
            // sqlSyncBuildProjectBindingSource
            // 
            sqlSyncBuildProjectBindingSource.DataMember = "SqlSyncBuildProject";
            sqlSyncBuildProjectBindingSource.DataSource = sqlSyncBuildDataBindingSource;
            // 
            // saveFileDialog1
            // 
            saveFileDialog1.DefaultExt = "xml";
            saveFileDialog1.Filter = "XML Files *.xml|*.xml|All Files *.*|*.*";
            saveFileDialog1.Title = "Save Build History";
            // 
            // PastBuildReviewForm
            // 
            AutoScaleBaseSize = new System.Drawing.Size(6, 16);
            ClientSize = new System.Drawing.Size(1096, 534);
            Controls.Add(splitContainer1);
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            Name = "PastBuildReviewForm";
            Text = "Review Past Builds";
            Load += new System.EventHandler(PastBuildReviewForm_Load);
            ((System.ComponentModel.ISupportInitialize)(sqlSyncBuildData1)).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel1.PerformLayout();
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(splitContainer1)).EndInit();
            splitContainer1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(dgMaster)).EndInit();
            ctxMaster.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(buildBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(sqlSyncBuildDataBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(dgDetail)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(scriptRunBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(sqlSyncBuildProjectBindingSource)).EndInit();
            ResumeLayout(false);

        }
        #endregion

        private void PastBuildReviewForm_Load(object sender, System.EventArgs e)
        {

            //this.scriptRunBindingSource.DataSource = this.sqlSyncBuildData1.ScriptRun;
            buildBindingSource.DataSource = sqlSyncBuildData1.Build;
        }

        private void dgMaster_Click(object sender, EventArgs e)
        {
            if (dgMaster.SelectedCells.Count == 0)
                return;

            SqlSyncBuildData.BuildRow row = (SqlSyncBuildData.BuildRow)((System.Data.DataRowView)dgMaster.SelectedCells[0].OwningRow.DataBoundItem).Row;
            System.Data.DataView view = sqlSyncBuildData1.ScriptRun.DefaultView;
            view.RowFilter = sqlSyncBuildData1.ScriptRun.Build_IdColumn.ColumnName + "= '" + row.Build_Id.ToString() + "'";
            scriptRunBindingSource.DataSource = view;

        }

        private void viewScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (dgDetail.SelectedCells.Count == 0)
                return;


            SqlSyncBuildData.ScriptRunRow row = (SqlSyncBuildData.ScriptRunRow)((System.Data.DataRowView)dgDetail.SelectedCells[0].OwningRow.DataBoundItem).Row;
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
            if (dgMaster.SelectedCells.Count == 0)
                return;

            List<SqlSyncBuildData.BuildRow> rows = new List<SqlSyncBuildData.BuildRow>();
            for (int i = 0; i < dgMaster.SelectedCells.Count; i++)
            {
                SqlSyncBuildData.BuildRow row = (SqlSyncBuildData.BuildRow)((System.Data.DataRowView)dgMaster.SelectedCells[i].OwningRow.DataBoundItem).Row;
                if (!rows.Contains(row))
                    rows.Add(row);
            }

            if (DialogResult.OK == saveFileDialog1.ShowDialog())
            {
                try
                {
                    //Perform a deep copy of the build data...
                    SqlSyncBuildData data = new SqlSyncBuildData();
                    using (StringReader sr = new StringReader("<?xml version=\"1.0\" standalone=\"yes\"?>" + sqlSyncBuildData1.GetXml()))
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
                    while (view.Count > 0)
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
