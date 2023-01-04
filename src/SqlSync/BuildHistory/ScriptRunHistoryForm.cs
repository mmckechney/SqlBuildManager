using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
namespace SqlSync.BuildHistory
{
    /// <summary>
    /// Summary description for PastBuildReviewForm.
    /// </summary>
    public class ScriptRunHistoryForm : System.Windows.Forms.Form
    {
        string currentFileText = string.Empty;
        string scriptHash = string.Empty;
        Connection.ConnectionData connData;
        Guid scriptId = Guid.Empty;
        string scriptFileName = string.Empty;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox txtScriptName;
        private System.Windows.Forms.TextBox txtScriptId;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Panel panel1;
        private DataGridViewTextBoxColumn dataGridTextBoxColumn10;
        private DataGridViewTextBoxColumn dataGridTextBoxColumn9;
        private DataGridViewTextBoxColumn dataGridTextBoxColumn3;
        private DataGridViewTextBoxColumn dataGridTextBoxColumn2;
        private DataGridViewTextBoxColumn dataGridTextBoxColumn7;
        private DataGridViewTextBoxColumn dataGridTextBoxColumn1;
        private SqlSync.SqlBuild.ScriptRunLog scriptRunLog1;
        private DataGridView dataGridView1;
        private BindingSource scriptRunLogBindingSource;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem viewScriptAsRunOnServerToolStripMenuItem;
        private Label label3;
        private TextBox txtScriptHash;
        private DataGridViewTextBoxColumn buildFileNameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn scriptFileNameDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn tagDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn scriptFileHashDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn commitDateDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn userIdDataGridViewTextBoxColumn;
        private DataGridViewTextBoxColumn scriptTextDataGridViewTextBoxColumn;
        private ToolStripSeparator toolStripSeparator1;
        private IContainer components;

        private ScriptRunHistoryForm(Connection.ConnectionData connData, string databaseName, string currentFileText, string scriptHash)
        {
            InitializeComponent();
            this.connData = connData;
            this.connData.DatabaseName = databaseName;
            this.currentFileText = currentFileText;
            this.scriptHash = scriptHash;
        }

        public ScriptRunHistoryForm(Connection.ConnectionData connData, string databaseName, Guid scriptId, string currentFileText, string scriptHash) :
            this(connData, databaseName, currentFileText, scriptHash)
        {
            this.scriptId = scriptId;
        }

        public ScriptRunHistoryForm(Connection.ConnectionData connData, string databaseName, string scriptFileName, string currentFileText, string scriptHash) :
            this(connData, databaseName, currentFileText, scriptHash)
        {
            this.scriptFileName = scriptFileName;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScriptRunHistoryForm));
            label1 = new System.Windows.Forms.Label();
            txtScriptName = new System.Windows.Forms.TextBox();
            txtScriptId = new System.Windows.Forms.TextBox();
            label2 = new System.Windows.Forms.Label();
            panel1 = new System.Windows.Forms.Panel();
            label3 = new System.Windows.Forms.Label();
            txtScriptHash = new System.Windows.Forms.TextBox();
            dataGridTextBoxColumn10 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            dataGridTextBoxColumn9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            dataGridTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            dataGridTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            dataGridTextBoxColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            dataGridTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            dataGridView1 = new System.Windows.Forms.DataGridView();
            buildFileNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            scriptFileNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            tagDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            scriptFileHashDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            commitDateDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            userIdDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            scriptTextDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            viewScriptAsRunOnServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            scriptRunLogBindingSource = new System.Windows.Forms.BindingSource(components);
            scriptRunLog1 = new SqlSync.SqlBuild.ScriptRunLog();
            panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(dataGridView1)).BeginInit();
            contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(scriptRunLogBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(scriptRunLog1)).BeginInit();
            SuspendLayout();
            // 
            // label1
            // 
            label1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            label1.Location = new System.Drawing.Point(16, 8);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(96, 16);
            label1.TabIndex = 1;
            label1.Text = "Script Name:";
            // 
            // txtScriptName
            // 
            txtScriptName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            txtScriptName.Location = new System.Drawing.Point(120, 9);
            txtScriptName.Name = "txtScriptName";
            txtScriptName.Size = new System.Drawing.Size(568, 14);
            txtScriptName.TabIndex = 2;
            txtScriptName.TabStop = false;
            // 
            // txtScriptId
            // 
            txtScriptId.BorderStyle = System.Windows.Forms.BorderStyle.None;
            txtScriptId.Location = new System.Drawing.Point(120, 27);
            txtScriptId.Name = "txtScriptId";
            txtScriptId.Size = new System.Drawing.Size(568, 14);
            txtScriptId.TabIndex = 4;
            txtScriptId.TabStop = false;
            // 
            // label2
            // 
            label2.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            label2.Location = new System.Drawing.Point(16, 27);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(96, 16);
            label2.TabIndex = 3;
            label2.Text = "Script ID:";
            // 
            // panel1
            // 
            panel1.BackColor = System.Drawing.Color.White;
            panel1.Controls.Add(label3);
            panel1.Controls.Add(txtScriptHash);
            panel1.Controls.Add(txtScriptName);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(txtScriptId);
            panel1.Dock = System.Windows.Forms.DockStyle.Top;
            panel1.Location = new System.Drawing.Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(976, 66);
            panel1.TabIndex = 5;
            // 
            // label3
            // 
            label3.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            label3.Location = new System.Drawing.Point(16, 46);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(96, 16);
            label3.TabIndex = 5;
            label3.Text = "Script Hash:";
            // 
            // txtScriptHash
            // 
            txtScriptHash.BorderStyle = System.Windows.Forms.BorderStyle.None;
            txtScriptHash.Location = new System.Drawing.Point(120, 46);
            txtScriptHash.Name = "txtScriptHash";
            txtScriptHash.Size = new System.Drawing.Size(568, 14);
            txtScriptHash.TabIndex = 6;
            txtScriptHash.TabStop = false;
            // 
            // dataGridTextBoxColumn10
            // 
            dataGridTextBoxColumn10.DataPropertyName = "ScriptText";
            dataGridTextBoxColumn10.HeaderText = "Script Text";
            dataGridTextBoxColumn10.Name = "dataGridTextBoxColumn10";
            dataGridTextBoxColumn10.Width = 75;
            // 
            // dataGridTextBoxColumn9
            // 
            dataGridTextBoxColumn9.DataPropertyName = "AllowScriptBlock";
            dataGridTextBoxColumn9.HeaderText = "Blocking?";
            dataGridTextBoxColumn9.Name = "dataGridTextBoxColumn9";
            dataGridTextBoxColumn9.Width = 70;
            // 
            // dataGridTextBoxColumn3
            // 
            dataGridTextBoxColumn3.DataPropertyName = "BuildFileName";
            dataGridTextBoxColumn3.HeaderText = "Source Build File";
            dataGridTextBoxColumn3.Name = "dataGridTextBoxColumn3";
            dataGridTextBoxColumn3.Width = 245;
            // 
            // dataGridTextBoxColumn2
            // 
            dataGridTextBoxColumn2.DataPropertyName = "ScriptFileHash";
            dataGridTextBoxColumn2.HeaderText = "Script Hash";
            dataGridTextBoxColumn2.Name = "dataGridTextBoxColumn2";
            dataGridTextBoxColumn2.Width = 305;
            // 
            // dataGridTextBoxColumn7
            // 
            dataGridTextBoxColumn7.DataPropertyName = "UserId";
            dataGridTextBoxColumn7.HeaderText = "Run By";
            dataGridTextBoxColumn7.Name = "dataGridTextBoxColumn7";
            dataGridTextBoxColumn7.Width = 80;
            // 
            // dataGridTextBoxColumn1
            // 
            dataGridTextBoxColumn1.DataPropertyName = "CommitDate";
            dataGridTextBoxColumn1.HeaderText = "Commit Date";
            dataGridTextBoxColumn1.Name = "dataGridTextBoxColumn1";
            dataGridTextBoxColumn1.Width = 125;
            // 
            // dataGridView1
            // 
            dataGridView1.AllowUserToAddRows = false;
            dataGridView1.AllowUserToDeleteRows = false;
            dataGridView1.AllowUserToOrderColumns = true;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            dataGridView1.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            dataGridView1.AutoGenerateColumns = false;
            dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            buildFileNameDataGridViewTextBoxColumn,
            scriptFileNameDataGridViewTextBoxColumn,
            tagDataGridViewTextBoxColumn,
            scriptFileHashDataGridViewTextBoxColumn,
            commitDateDataGridViewTextBoxColumn,
            userIdDataGridViewTextBoxColumn,
            scriptTextDataGridViewTextBoxColumn});
            dataGridView1.ContextMenuStrip = contextMenuStrip1;
            dataGridView1.DataSource = scriptRunLogBindingSource;
            dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            dataGridView1.Location = new System.Drawing.Point(0, 66);
            dataGridView1.Name = "dataGridView1";
            dataGridView1.ReadOnly = true;
            dataGridView1.RowHeadersVisible = false;
            dataGridView1.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            dataGridView1.Size = new System.Drawing.Size(976, 308);
            dataGridView1.TabIndex = 0;
            dataGridView1.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(dataGridView1_RowsAdded);
            // 
            // buildFileNameDataGridViewTextBoxColumn
            // 
            buildFileNameDataGridViewTextBoxColumn.DataPropertyName = "BuildFileName";
            buildFileNameDataGridViewTextBoxColumn.HeaderText = "Build File Name";
            buildFileNameDataGridViewTextBoxColumn.Name = "buildFileNameDataGridViewTextBoxColumn";
            buildFileNameDataGridViewTextBoxColumn.ReadOnly = true;
            buildFileNameDataGridViewTextBoxColumn.Width = 200;
            // 
            // scriptFileNameDataGridViewTextBoxColumn
            // 
            scriptFileNameDataGridViewTextBoxColumn.DataPropertyName = "ScriptFileName";
            scriptFileNameDataGridViewTextBoxColumn.HeaderText = "Script File Name";
            scriptFileNameDataGridViewTextBoxColumn.Name = "scriptFileNameDataGridViewTextBoxColumn";
            scriptFileNameDataGridViewTextBoxColumn.ReadOnly = true;
            scriptFileNameDataGridViewTextBoxColumn.Width = 200;
            // 
            // tagDataGridViewTextBoxColumn
            // 
            tagDataGridViewTextBoxColumn.DataPropertyName = "Tag";
            tagDataGridViewTextBoxColumn.HeaderText = "Tag";
            tagDataGridViewTextBoxColumn.Name = "tagDataGridViewTextBoxColumn";
            tagDataGridViewTextBoxColumn.ReadOnly = true;
            tagDataGridViewTextBoxColumn.Width = 75;
            // 
            // scriptFileHashDataGridViewTextBoxColumn
            // 
            scriptFileHashDataGridViewTextBoxColumn.DataPropertyName = "ScriptFileHash";
            scriptFileHashDataGridViewTextBoxColumn.HeaderText = "Script Hash";
            scriptFileHashDataGridViewTextBoxColumn.Name = "scriptFileHashDataGridViewTextBoxColumn";
            scriptFileHashDataGridViewTextBoxColumn.ReadOnly = true;
            scriptFileHashDataGridViewTextBoxColumn.Width = 200;
            // 
            // commitDateDataGridViewTextBoxColumn
            // 
            commitDateDataGridViewTextBoxColumn.DataPropertyName = "CommitDate";
            commitDateDataGridViewTextBoxColumn.HeaderText = "Commit Date";
            commitDateDataGridViewTextBoxColumn.Name = "commitDateDataGridViewTextBoxColumn";
            commitDateDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // userIdDataGridViewTextBoxColumn
            // 
            userIdDataGridViewTextBoxColumn.DataPropertyName = "UserId";
            userIdDataGridViewTextBoxColumn.HeaderText = "User Id";
            userIdDataGridViewTextBoxColumn.Name = "userIdDataGridViewTextBoxColumn";
            userIdDataGridViewTextBoxColumn.ReadOnly = true;
            userIdDataGridViewTextBoxColumn.Width = 80;
            // 
            // scriptTextDataGridViewTextBoxColumn
            // 
            scriptTextDataGridViewTextBoxColumn.DataPropertyName = "ScriptText";
            scriptTextDataGridViewTextBoxColumn.HeaderText = "Script Text";
            scriptTextDataGridViewTextBoxColumn.Name = "scriptTextDataGridViewTextBoxColumn";
            scriptTextDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            viewScriptAsRunOnServerToolStripMenuItem,
            toolStripSeparator1});
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(223, 76);
            contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(contextMenuStrip1_Opening);
            // 
            // viewScriptAsRunOnServerToolStripMenuItem
            // 
            viewScriptAsRunOnServerToolStripMenuItem.Name = "viewScriptAsRunOnServerToolStripMenuItem";
            viewScriptAsRunOnServerToolStripMenuItem.Size = new System.Drawing.Size(222, 22);
            viewScriptAsRunOnServerToolStripMenuItem.Text = "View Script as Run on Server";
            viewScriptAsRunOnServerToolStripMenuItem.Click += new System.EventHandler(viewScriptAsRunOnServerToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(219, 6);
            // 
            // scriptRunLogBindingSource
            // 
            scriptRunLogBindingSource.DataSource = typeof(SqlSync.SqlBuild.ScriptRunLog);
            // 
            // scriptRunLog1
            // 
            scriptRunLog1.Namespace = "";
            scriptRunLog1.PrimaryKey = new System.Data.DataColumn[0];
            scriptRunLog1.TableName = "ScriptRunLog";
            // 
            // ScriptRunHistoryForm
            // 
            AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            ClientSize = new System.Drawing.Size(976, 374);
            Controls.Add(dataGridView1);
            Controls.Add(panel1);
            Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            KeyPreview = true;
            Name = "ScriptRunHistoryForm";
            Text = "Script Run History on {0}";
            Load += new System.EventHandler(PastBuildReviewForm_Load);
            KeyDown += new System.Windows.Forms.KeyEventHandler(ScriptRunHistoryForm_KeyDown);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(dataGridView1)).EndInit();
            contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(scriptRunLogBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(scriptRunLog1)).EndInit();
            ResumeLayout(false);

        }
        #endregion

        private void PastBuildReviewForm_Load(object sender, System.EventArgs e)
        {
            Text = String.Format(Text, connData.SQLServerName);

            if (scriptId != Guid.Empty)
            {
                txtScriptId.Text = scriptId.ToString();

                try
                {
                    SqlSync.SqlBuild.ScriptRunLog log = SqlSync.SqlBuild.SqlBuildHelper.GetScriptRunLog(scriptId, connData);
                    dataGridView1.DataSource = log;
                    if (log.Rows.Count > 0)
                        txtScriptName.Text = log[0].ScriptFileName;
                    else
                        txtScriptName.Text = "Unable to read from log";
                }
                catch (Exception exe)
                {
                    string message = "Unable to retrieve script run log.\r\n" + exe.Message;
                    MessageBox.Show(message, "Error getting log", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtScriptName.Text = "Unable to read from log";
                }
            }
            else if (scriptFileName != string.Empty)
            {
                txtScriptId.Text = "N/A";

                try
                {
                    SqlSync.SqlBuild.ScriptRunLog log = SqlSync.SqlBuild.SqlBuildHelper.GetObjectRunHistoryLog(scriptFileName, connData);
                    dataGridView1.DataSource = log;
                    if (log.Rows.Count > 0)
                        txtScriptName.Text = log[0].ScriptFileName;
                    else
                        txtScriptName.Text = "Unable to read from log";
                }
                catch (Exception exe)
                {
                    string message = "Unable to retrieve object run log.\r\n" + exe.Message;
                    MessageBox.Show(message, "Error getting log", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtScriptName.Text = "Unable to read from log";
                }
            }
            txtScriptHash.Text = scriptHash;

        }

        private void ScriptRunHistoryForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                e.Handled = true;
                Close();
            }
        }

        private void viewScriptAsRunOnServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<DataGridViewRow> selectedRows = GetSelectedRows(dataGridView1);
            if (selectedRows.Count != 1)
                return;

            SqlSync.SqlBuild.ScriptRunLogRow row = (SqlSync.SqlBuild.ScriptRunLogRow)((DataRowView)selectedRows[0].DataBoundItem).Row;
            ScriptDisplayForm frmDisp = new ScriptDisplayForm(row.ScriptText, connData.SQLServerName, row.ScriptFileName);
            frmDisp.ShowDialog();
            frmDisp.Dispose();

        }
        private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            SqlSync.SqlBuild.ScriptRunLogRow row = (SqlSync.SqlBuild.ScriptRunLogRow)((DataRowView)dataGridView1[0, e.RowIndex].OwningRow.DataBoundItem).Row;
            if (row.ScriptFileHash != scriptHash)
            {
                DataGridViewCellStyle style = new DataGridViewCellStyle();
                style.ForeColor = Color.Red;
                string colName = ((SqlSync.SqlBuild.ScriptRunLog)row.Table).ScriptFileHashColumn.ColumnName;
                foreach (DataGridViewColumn col in dataGridView1.Columns)
                {
                    if (col.Name.ToLower().IndexOf(colName.ToLower()) > -1)
                    {
                        dataGridView1[col.Name, e.RowIndex].Style = style;
                        break;
                    }
                }
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (dataGridView1.SelectedCells.Count == 0)
                return;

            List<DataGridViewRow> selectedRows = GetSelectedRows(dataGridView1);

            if (selectedRows.Count == 1)
            {
                viewScriptAsRunOnServerToolStripMenuItem.Enabled = true;
            }
            else
            {
                viewScriptAsRunOnServerToolStripMenuItem.Enabled = false;
            }

        }

        private List<DataGridViewRow> GetSelectedRows(DataGridView grid)
        {
            List<DataGridViewRow> selectedRows = new List<DataGridViewRow>();
            foreach (DataGridViewCell c in grid.SelectedCells)
            {
                if (!selectedRows.Contains(c.OwningRow))
                    selectedRows.Add(c.OwningRow);
            }

            return selectedRows;
        }


    }
}
