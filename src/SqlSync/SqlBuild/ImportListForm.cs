using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SqlSync.SqlBuild
{
    /// <summary>
    /// Summary description for ImportListForm.
    /// </summary>
    public class ImportListForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ListView lstImport;
        private SqlBuild.SqlSyncBuildData buildData = null;
        private SqlBuild.SqlSyncBuildData importData = null;
        private string importTempPath;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.ContextMenuStrip contextMenu1;
        private System.Windows.Forms.ToolStripMenuItem mnuView;
        private string[] preSelectedFiles = null;
        private IContainer components;

        public SqlSync.SqlBuild.SqlSyncBuildData ImportData
        {
            get { return importData; }
        }

        public ImportListForm(SqlBuild.SqlSyncBuildData importData, string importTempPath, SqlBuild.SqlSyncBuildData buildData, string[] preSelectedFiles)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.importData = importData;
            this.buildData = buildData;
            this.importTempPath = importTempPath;
            this.preSelectedFiles = preSelectedFiles;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportListForm));
            lstImport = new System.Windows.Forms.ListView();
            columnHeader1 = new System.Windows.Forms.ColumnHeader();
            columnHeader2 = new System.Windows.Forms.ColumnHeader();
            columnHeader3 = new System.Windows.Forms.ColumnHeader();
            columnHeader4 = new System.Windows.Forms.ColumnHeader();
            contextMenu1 = new System.Windows.Forms.ContextMenuStrip(components);
            mnuView = new System.Windows.Forms.ToolStripMenuItem();
            label1 = new System.Windows.Forms.Label();
            button2 = new System.Windows.Forms.Button();
            button1 = new System.Windows.Forms.Button();
            contextMenu1.SuspendLayout();
            SuspendLayout();
            // 
            // lstImport
            // 
            lstImport.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            lstImport.CheckBoxes = true;
            lstImport.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader1,
            columnHeader2,
            columnHeader3,
            columnHeader4});
            lstImport.ContextMenuStrip = contextMenu1;
            lstImport.GridLines = true;
            lstImport.HideSelection = false;
            lstImport.Location = new System.Drawing.Point(19, 28);
            lstImport.Name = "lstImport";
            lstImport.Size = new System.Drawing.Size(522, 228);
            lstImport.TabIndex = 1;
            lstImport.UseCompatibleStateImageBehavior = false;
            lstImport.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "File Name";
            columnHeader1.Width = 381;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Destination Database";
            columnHeader2.Width = 126;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "Script Id";
            columnHeader3.Width = 0;
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "Full Path";
            columnHeader4.Width = 0;
            // 
            // contextMenu1
            // 
            contextMenu1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            mnuView});
            contextMenu1.Name = "contextMenu1";
            contextMenu1.Size = new System.Drawing.Size(121, 26);
            // 
            // mnuView
            // 
            mnuView.Name = "mnuView";
            mnuView.Size = new System.Drawing.Size(120, 22);
            mnuView.Text = "View File";
            mnuView.Click += new System.EventHandler(mnuView_Click);
            // 
            // label1
            // 
            label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            label1.Location = new System.Drawing.Point(19, 256);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(259, 28);
            label1.TabIndex = 10;
            label1.Text = "* Colored items denote pre-existing files";
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // button2
            // 
            button2.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            button2.Location = new System.Drawing.Point(285, 305);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(134, 28);
            button2.TabIndex = 7;
            button2.Text = "Cancel";
            button2.Click += new System.EventHandler(button2_Click);
            // 
            // button1
            // 
            button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            button1.Location = new System.Drawing.Point(141, 305);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(134, 28);
            button1.TabIndex = 6;
            button1.Text = "Add Checked Files";
            button1.Click += new System.EventHandler(button1_Click);
            // 
            // ImportListForm
            // 
            AutoScaleBaseSize = new System.Drawing.Size(6, 16);
            ClientSize = new System.Drawing.Size(560, 342);
            Controls.Add(label1);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(lstImport);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            Name = "ImportListForm";
            Text = "Import Confirmation";
            Load += new System.EventHandler(ImportListForm_Load);
            contextMenu1.ResumeLayout(false);
            ResumeLayout(false);

        }
        #endregion

        private void ImportListForm_Load(object sender, System.EventArgs e)
        {
            bool foundConflict = false;
            bool selectAllValid;
            if (preSelectedFiles.Length > 0)
                selectAllValid = false;
            else
                selectAllValid = true;

            System.Data.DataView importView = importData.Script.DefaultView;
            importView.Sort = "BuildOrder ASC";
            importView.RowStateFilter = System.Data.DataViewRowState.OriginalRows;

            for (int i = 0; i < importView.Count; i++)
            {
                SqlSyncBuildData.ScriptRow row = (SqlSyncBuildData.ScriptRow)importView[i].Row;

                ListViewItem item = new ListViewItem(new string[] { row.FileName, row.Database, row.ScriptId, System.IO.Path.Combine(importTempPath, row.FileName) });
                item.Checked = false;

                if (buildData.Script.Select(
                    "ScriptId = '" + row.ScriptId + "' OR " +
                    "FileName = '" + row.FileName + "'").Length > 0)
                {
                    item.Checked = false;
                    item.BackColor = Color.Red;
                    foundConflict = true;
                }
                else if (selectAllValid)
                    item.Checked = true;
                else
                {
                    for (int j = 0; j < preSelectedFiles.Length; j++)
                    {
                        if (preSelectedFiles[j].ToLower() == row.FileName.ToLower())
                        {
                            item.Checked = true;
                            break;
                        }
                    }
                }


                lstImport.Items.Add(item);
            }

            if (foundConflict)
            {
                MessageBox.Show("A matching entry was already found. Please review list before accepting.", "Please Review List", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }

        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            for (int i = 0; i < lstImport.Items.Count; i++)
            {
                if (lstImport.Items[i].Checked == false)
                {
                    System.Data.DataRow[] rows = importData.Script.Select("ScriptId ='" + lstImport.Items[i].SubItems[2].Text + "'");
                    if (rows.Length > 0)
                        importData.Script.Rows.Remove(rows[0]);
                }
            }
            importData.AcceptChanges();

            DialogResult = DialogResult.OK;
            Close();
        }

        private void button2_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void mnuView_Click(object sender, System.EventArgs e)
        {
            if (lstImport.SelectedItems.Count == 0)
                return;

            ListViewItem item = lstImport.SelectedItems[0];

            string name = item.SubItems[0].Text;
            string fullpath = item.SubItems[3].Text;
            AddScriptTextForm frmView = new AddScriptTextForm(ref importData, name, fullpath);
            frmView.ShowDialog();
        }

    }
}
