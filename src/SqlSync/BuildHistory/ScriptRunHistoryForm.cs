using System;
using System.Drawing;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Data;
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
        private ToolStripMenuItem compareToCurrentScriptToolStripMenuItem;
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
        private ToolStripMenuItem compareSelectedScriptsToolStripMenuItem;
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
            this(connData,databaseName,currentFileText,scriptHash)
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScriptRunHistoryForm));
            this.label1 = new System.Windows.Forms.Label();
            this.txtScriptName = new System.Windows.Forms.TextBox();
            this.txtScriptId = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.label3 = new System.Windows.Forms.Label();
            this.txtScriptHash = new System.Windows.Forms.TextBox();
            this.dataGridTextBoxColumn10 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridTextBoxColumn9 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridTextBoxColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridView1 = new System.Windows.Forms.DataGridView();
            this.buildFileNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.scriptFileNameDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.tagDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.scriptFileHashDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.commitDateDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.userIdDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.scriptTextDataGridViewTextBoxColumn = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.viewScriptAsRunOnServerToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.compareToCurrentScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.scriptRunLogBindingSource = new System.Windows.Forms.BindingSource(this.components);
            this.scriptRunLog1 = new SqlSync.SqlBuild.ScriptRunLog();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.compareSelectedScriptsToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).BeginInit();
            this.contextMenuStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.scriptRunLogBindingSource)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.scriptRunLog1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(16, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "Script Name:";
            // 
            // txtScriptName
            // 
            this.txtScriptName.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtScriptName.Location = new System.Drawing.Point(120, 9);
            this.txtScriptName.Name = "txtScriptName";
            this.txtScriptName.Size = new System.Drawing.Size(568, 14);
            this.txtScriptName.TabIndex = 2;
            this.txtScriptName.TabStop = false;
            // 
            // txtScriptId
            // 
            this.txtScriptId.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtScriptId.Location = new System.Drawing.Point(120, 27);
            this.txtScriptId.Name = "txtScriptId";
            this.txtScriptId.Size = new System.Drawing.Size(568, 14);
            this.txtScriptId.TabIndex = 4;
            this.txtScriptId.TabStop = false;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(16, 27);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(96, 16);
            this.label2.TabIndex = 3;
            this.label2.Text = "Script ID:";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.txtScriptHash);
            this.panel1.Controls.Add(this.txtScriptName);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.txtScriptId);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(976, 66);
            this.panel1.TabIndex = 5;
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(16, 46);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(96, 16);
            this.label3.TabIndex = 5;
            this.label3.Text = "Script Hash:";
            // 
            // txtScriptHash
            // 
            this.txtScriptHash.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtScriptHash.Location = new System.Drawing.Point(120, 46);
            this.txtScriptHash.Name = "txtScriptHash";
            this.txtScriptHash.Size = new System.Drawing.Size(568, 14);
            this.txtScriptHash.TabIndex = 6;
            this.txtScriptHash.TabStop = false;
            // 
            // dataGridTextBoxColumn10
            // 
           // this.dataGridTextBoxColumn10.Format = "";
            //this.dataGridTextBoxColumn10.FormatInfo = null;
            this.dataGridTextBoxColumn10.HeaderText = "Script Text";
            this.dataGridTextBoxColumn10.DataPropertyName = "ScriptText";
            this.dataGridTextBoxColumn10.Width = 75;
            // 
            // dataGridTextBoxColumn9
            // 
            //this.dataGridTextBoxColumn9.Format = "";
            //this.dataGridTextBoxColumn9.FormatInfo = null;
            this.dataGridTextBoxColumn9.HeaderText = "Blocking?";
            this.dataGridTextBoxColumn9.DataPropertyName = "AllowScriptBlock";
            this.dataGridTextBoxColumn9.Width = 70;
            // 
            // dataGridTextBoxColumn3
            // 
            //this.dataGridTextBoxColumn3.Format = "";
            //this.dataGridTextBoxColumn3.FormatInfo = null;
            this.dataGridTextBoxColumn3.HeaderText = "Source Build File";
            this.dataGridTextBoxColumn3.DataPropertyName = "BuildFileName";
            this.dataGridTextBoxColumn3.Width = 245;
            // 
            // dataGridTextBoxColumn2
            // 
            //this.dataGridTextBoxColumn2.Format = "";
            //this.dataGridTextBoxColumn2.FormatInfo = null;
            this.dataGridTextBoxColumn2.HeaderText = "Script Hash";
            this.dataGridTextBoxColumn2.DataPropertyName = "ScriptFileHash";
            this.dataGridTextBoxColumn2.Width = 305;
            // 
            // dataGridTextBoxColumn7
            // 
            //this.dataGridTextBoxColumn7.Format = "";
            //this.dataGridTextBoxColumn7.FormatInfo = null;
            this.dataGridTextBoxColumn7.HeaderText = "Run By";
            this.dataGridTextBoxColumn7.DataPropertyName = "UserId";
            this.dataGridTextBoxColumn7.Width = 80;
            // 
            // dataGridTextBoxColumn1
            // 
            //this.dataGridTextBoxColumn1.Format = "";
            //this.dataGridTextBoxColumn1.FormatInfo = null;
            this.dataGridTextBoxColumn1.HeaderText = "Commit Date";
            this.dataGridTextBoxColumn1.DataPropertyName = "CommitDate";
            //this.dataGridTextBoxColumn1.NullText = "";
            this.dataGridTextBoxColumn1.Width = 125;
            // 
            // dataGridView1
            // 
            this.dataGridView1.AllowUserToAddRows = false;
            this.dataGridView1.AllowUserToDeleteRows = false;
            this.dataGridView1.AllowUserToOrderColumns = true;
            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(224)))), ((int)(((byte)(224)))), ((int)(((byte)(224)))));
            this.dataGridView1.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;
            this.dataGridView1.AutoGenerateColumns = false;
            this.dataGridView1.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.buildFileNameDataGridViewTextBoxColumn,
            this.scriptFileNameDataGridViewTextBoxColumn,
            this.tagDataGridViewTextBoxColumn,
            this.scriptFileHashDataGridViewTextBoxColumn,
            this.commitDateDataGridViewTextBoxColumn,
            this.userIdDataGridViewTextBoxColumn,
            this.scriptTextDataGridViewTextBoxColumn});
            this.dataGridView1.ContextMenuStrip = this.contextMenuStrip1;
            this.dataGridView1.DataSource = this.scriptRunLogBindingSource;
            this.dataGridView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataGridView1.Location = new System.Drawing.Point(0, 66);
            this.dataGridView1.Name = "dataGridView1";
            this.dataGridView1.ReadOnly = true;
            this.dataGridView1.RowHeadersVisible = false;
            this.dataGridView1.RowTemplate.DefaultCellStyle.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.dataGridView1.Size = new System.Drawing.Size(976, 308);
            this.dataGridView1.TabIndex = 0;
            this.dataGridView1.RowsAdded += new System.Windows.Forms.DataGridViewRowsAddedEventHandler(this.dataGridView1_RowsAdded);
            // 
            // buildFileNameDataGridViewTextBoxColumn
            // 
            this.buildFileNameDataGridViewTextBoxColumn.DataPropertyName = "BuildFileName";
            this.buildFileNameDataGridViewTextBoxColumn.HeaderText = "Build File Name";
            this.buildFileNameDataGridViewTextBoxColumn.Name = "buildFileNameDataGridViewTextBoxColumn";
            this.buildFileNameDataGridViewTextBoxColumn.ReadOnly = true;
            this.buildFileNameDataGridViewTextBoxColumn.Width = 200;
            // 
            // scriptFileNameDataGridViewTextBoxColumn
            // 
            this.scriptFileNameDataGridViewTextBoxColumn.DataPropertyName = "ScriptFileName";
            this.scriptFileNameDataGridViewTextBoxColumn.HeaderText = "Script File Name";
            this.scriptFileNameDataGridViewTextBoxColumn.Name = "scriptFileNameDataGridViewTextBoxColumn";
            this.scriptFileNameDataGridViewTextBoxColumn.ReadOnly = true;
            this.scriptFileNameDataGridViewTextBoxColumn.Width = 200;
            // 
            // tagDataGridViewTextBoxColumn
            // 
            this.tagDataGridViewTextBoxColumn.DataPropertyName = "Tag";
            this.tagDataGridViewTextBoxColumn.HeaderText = "Tag";
            this.tagDataGridViewTextBoxColumn.Name = "tagDataGridViewTextBoxColumn";
            this.tagDataGridViewTextBoxColumn.ReadOnly = true;
            this.tagDataGridViewTextBoxColumn.Width = 75;
            // 
            // scriptFileHashDataGridViewTextBoxColumn
            // 
            this.scriptFileHashDataGridViewTextBoxColumn.DataPropertyName = "ScriptFileHash";
            this.scriptFileHashDataGridViewTextBoxColumn.HeaderText = "Script Hash";
            this.scriptFileHashDataGridViewTextBoxColumn.Name = "scriptFileHashDataGridViewTextBoxColumn";
            this.scriptFileHashDataGridViewTextBoxColumn.ReadOnly = true;
            this.scriptFileHashDataGridViewTextBoxColumn.Width = 200;
            // 
            // commitDateDataGridViewTextBoxColumn
            // 
            this.commitDateDataGridViewTextBoxColumn.DataPropertyName = "CommitDate";
            this.commitDateDataGridViewTextBoxColumn.HeaderText = "Commit Date";
            this.commitDateDataGridViewTextBoxColumn.Name = "commitDateDataGridViewTextBoxColumn";
            this.commitDateDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // userIdDataGridViewTextBoxColumn
            // 
            this.userIdDataGridViewTextBoxColumn.DataPropertyName = "UserId";
            this.userIdDataGridViewTextBoxColumn.HeaderText = "User Id";
            this.userIdDataGridViewTextBoxColumn.Name = "userIdDataGridViewTextBoxColumn";
            this.userIdDataGridViewTextBoxColumn.ReadOnly = true;
            this.userIdDataGridViewTextBoxColumn.Width = 80;
            // 
            // scriptTextDataGridViewTextBoxColumn
            // 
            this.scriptTextDataGridViewTextBoxColumn.DataPropertyName = "ScriptText";
            this.scriptTextDataGridViewTextBoxColumn.HeaderText = "Script Text";
            this.scriptTextDataGridViewTextBoxColumn.Name = "scriptTextDataGridViewTextBoxColumn";
            this.scriptTextDataGridViewTextBoxColumn.ReadOnly = true;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.viewScriptAsRunOnServerToolStripMenuItem,
            this.compareToCurrentScriptToolStripMenuItem,
            this.toolStripSeparator1,
            this.compareSelectedScriptsToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(224, 98);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // viewScriptAsRunOnServerToolStripMenuItem
            // 
            this.viewScriptAsRunOnServerToolStripMenuItem.Name = "viewScriptAsRunOnServerToolStripMenuItem";
            this.viewScriptAsRunOnServerToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.viewScriptAsRunOnServerToolStripMenuItem.Text = "View Script as Run on Server";
            this.viewScriptAsRunOnServerToolStripMenuItem.Click += new System.EventHandler(this.viewScriptAsRunOnServerToolStripMenuItem_Click);
            // 
            // compareToCurrentScriptToolStripMenuItem
            // 
            this.compareToCurrentScriptToolStripMenuItem.Name = "compareToCurrentScriptToolStripMenuItem";
            this.compareToCurrentScriptToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.compareToCurrentScriptToolStripMenuItem.Text = "Compare to Current Script";
            this.compareToCurrentScriptToolStripMenuItem.Click += new System.EventHandler(this.compareToCurrentScriptToolStripMenuItem_Click);
            // 
            // scriptRunLogBindingSource
            // 
            this.scriptRunLogBindingSource.DataSource = typeof(SqlSync.SqlBuild.ScriptRunLog);
            // 
            // scriptRunLog1
            // 
            this.scriptRunLog1.TableName = "ScriptRunLog";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(220, 6);
            // 
            // compareSelectedScriptsToolStripMenuItem
            // 
            this.compareSelectedScriptsToolStripMenuItem.Enabled = false;
            this.compareSelectedScriptsToolStripMenuItem.Name = "compareSelectedScriptsToolStripMenuItem";
            this.compareSelectedScriptsToolStripMenuItem.Size = new System.Drawing.Size(223, 22);
            this.compareSelectedScriptsToolStripMenuItem.Text = "Compare selected scripts";
            this.compareSelectedScriptsToolStripMenuItem.Click += new System.EventHandler(this.compareSelectedScriptsToolStripMenuItem_Click);
            // 
            // ScriptRunHistoryForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(976, 374);
            this.Controls.Add(this.dataGridView1);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "ScriptRunHistoryForm";
            this.Text = "Script Run History on {0}";
            this.Load += new System.EventHandler(this.PastBuildReviewForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ScriptRunHistoryForm_KeyDown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.dataGridView1)).EndInit();
            this.contextMenuStrip1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.scriptRunLogBindingSource)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.scriptRunLog1)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion

        private void PastBuildReviewForm_Load(object sender, System.EventArgs e)
        {
            this.Text = String.Format(this.Text, this.connData.SQLServerName);

            if (this.scriptId != Guid.Empty)
            {
                this.txtScriptId.Text = this.scriptId.ToString();

                try
                {
                    SqlSync.SqlBuild.ScriptRunLog log = SqlSync.SqlBuild.SqlBuildHelper.GetScriptRunLog(this.scriptId, this.connData);
                    this.dataGridView1.DataSource = log;
                    if (log.Rows.Count > 0)
                        this.txtScriptName.Text = log[0].ScriptFileName;
                    else
                        this.txtScriptName.Text = "Unable to read from log";
                }
                catch (Exception exe)
                {
                    string message = "Unable to retrieve script run log.\r\n" + exe.Message;
                    MessageBox.Show(message, "Error getting log", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.txtScriptName.Text = "Unable to read from log";
                }
            }
            else if (this.scriptFileName != string.Empty)
            {
                this.txtScriptId.Text = "N/A";

                try
                {
                    SqlSync.SqlBuild.ScriptRunLog log = SqlSync.SqlBuild.SqlBuildHelper.GetObjectRunHistoryLog(this.scriptFileName, this.connData);
                    this.dataGridView1.DataSource = log;
                    if (log.Rows.Count > 0)
                        this.txtScriptName.Text = log[0].ScriptFileName;
                    else
                        this.txtScriptName.Text = "Unable to read from log";
                }
                catch (Exception exe)
                {
                    string message = "Unable to retrieve object run log.\r\n" + exe.Message;
                    MessageBox.Show(message, "Error getting log", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.txtScriptName.Text = "Unable to read from log";
                }
            }
            txtScriptHash.Text = this.scriptHash;

        }

		private void ScriptRunHistoryForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if(e.KeyCode == Keys.Escape)
			{
				e.Handled = true;
				this.Close();
			}
		}

        private void viewScriptAsRunOnServerToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<DataGridViewRow> selectedRows = GetSelectedRows(this.dataGridView1);
            if (selectedRows.Count != 1)
                return;

            SqlSync.SqlBuild.ScriptRunLogRow row = (SqlSync.SqlBuild.ScriptRunLogRow)((DataRowView)selectedRows[0].DataBoundItem).Row;
            ScriptDisplayForm frmDisp = new ScriptDisplayForm(row.ScriptText, this.connData.SQLServerName, row.ScriptFileName);
            frmDisp.ShowDialog();
            frmDisp.Dispose();

        }

        private void compareToCurrentScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.dataGridView1.SelectedCells.Count == 0)
                return;

            SqlSync.SqlBuild.ScriptRunLogRow row = (SqlSync.SqlBuild.ScriptRunLogRow)((DataRowView)this.dataGridView1.SelectedCells[0].OwningRow.DataBoundItem).Row;

            Analysis.SimpleDiffForm frmDiff = new SqlSync.Analysis.SimpleDiffForm(this.currentFileText, row.ScriptText, this.connData.DatabaseName, this.connData.SQLServerName);
            frmDiff.ShowDialog();
            frmDiff.Dispose();
        }

        private void dataGridView1_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            SqlSync.SqlBuild.ScriptRunLogRow row = (SqlSync.SqlBuild.ScriptRunLogRow)((DataRowView)this.dataGridView1[0,e.RowIndex].OwningRow.DataBoundItem).Row;
            if (row.ScriptFileHash != this.scriptHash)
            {
                DataGridViewCellStyle style = new DataGridViewCellStyle();
                style.ForeColor = Color.Red;
                string colName = ((SqlSync.SqlBuild.ScriptRunLog)row.Table).ScriptFileHashColumn.ColumnName;
                foreach (DataGridViewColumn col in this.dataGridView1.Columns)
                {
                    if(col.Name.ToLower().IndexOf(colName.ToLower()) > -1)
                    {
                        this.dataGridView1[col.Name, e.RowIndex].Style = style;
                        break;
                    }
                }
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (this.dataGridView1.SelectedCells.Count == 0)
                return;

            List<DataGridViewRow> selectedRows = GetSelectedRows(this.dataGridView1);
            if (selectedRows.Count == 2)
                compareSelectedScriptsToolStripMenuItem.Enabled = true;
            else
                compareSelectedScriptsToolStripMenuItem.Enabled = false;


            if (selectedRows.Count == 1)
            {
                compareToCurrentScriptToolStripMenuItem.Enabled = true;
                viewScriptAsRunOnServerToolStripMenuItem.Enabled = true;
            }
            else
            {
                compareToCurrentScriptToolStripMenuItem.Enabled = false;
                viewScriptAsRunOnServerToolStripMenuItem.Enabled = false;
            }

        }

        private void compareSelectedScriptsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<DataGridViewRow> selectedRows = GetSelectedRows(this.dataGridView1);
            if (selectedRows.Count != 2)
                return;

            SqlSync.SqlBuild.ScriptRunLogRow row1 = (SqlSync.SqlBuild.ScriptRunLogRow)((DataRowView)selectedRows[0].DataBoundItem).Row;
            SqlSync.SqlBuild.ScriptRunLogRow row2 = (SqlSync.SqlBuild.ScriptRunLogRow)((DataRowView)selectedRows[1].DataBoundItem).Row;

            Analysis.SimpleDiffForm frmDiff = new SqlSync.Analysis.SimpleDiffForm(row1.ScriptText, row2.ScriptText, this.connData.DatabaseName, this.connData.SQLServerName, row1.CommitDate.ToString(), row2.CommitDate.ToString());
            frmDiff.ShowDialog();
            frmDiff.Dispose();
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
