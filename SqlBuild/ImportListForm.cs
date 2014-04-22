using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
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
		private System.Windows.Forms.ContextMenu contextMenu1;
		private System.Windows.Forms.MenuItem mnuView;
        private string[] preSelectedFiles = null;
		public SqlSync.SqlBuild.SqlSyncBuildData ImportData
		{
			get { return this.importData; }    
		}

		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ImportListForm));
            this.lstImport = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
            this.contextMenu1 = new System.Windows.Forms.ContextMenu();
            this.mnuView = new System.Windows.Forms.MenuItem();
            this.label1 = new System.Windows.Forms.Label();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lstImport
            // 
            this.lstImport.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstImport.CheckBoxes = true;
            this.lstImport.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.lstImport.ContextMenu = this.contextMenu1;
            this.lstImport.GridLines = true;
            this.lstImport.Location = new System.Drawing.Point(16, 23);
            this.lstImport.Name = "lstImport";
            this.lstImport.Size = new System.Drawing.Size(528, 249);
            this.lstImport.TabIndex = 1;
            this.lstImport.UseCompatibleStateImageBehavior = false;
            this.lstImport.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "File Name";
            this.columnHeader1.Width = 381;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Destination Database";
            this.columnHeader2.Width = 126;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Script Id";
            this.columnHeader3.Width = 0;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Full Path";
            this.columnHeader4.Width = 0;
            // 
            // contextMenu1
            // 
            this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mnuView});
            // 
            // mnuView
            // 
            this.mnuView.Index = 0;
            this.mnuView.Text = "View File";
            this.mnuView.Click += new System.EventHandler(this.mnuView_Click);
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.Location = new System.Drawing.Point(16, 272);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(216, 23);
            this.label1.TabIndex = 10;
            this.label1.Text = "* Colored items denote pre-existing files";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // button2
            // 
            this.button2.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button2.Location = new System.Drawing.Point(284, 312);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(112, 23);
            this.button2.TabIndex = 7;
            this.button2.Text = "Cancel";
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // button1
            // 
            this.button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button1.Location = new System.Drawing.Point(164, 312);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(112, 23);
            this.button1.TabIndex = 6;
            this.button1.Text = "Add Checked Files";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // ImportListForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(560, 342);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.lstImport);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ImportListForm";
            this.Text = "Import Confirmation";
            this.Load += new System.EventHandler(this.ImportListForm_Load);
            this.ResumeLayout(false);

		}
		#endregion

		private void ImportListForm_Load(object sender, System.EventArgs e)
		{ 
			bool foundConflict = false;
            bool selectAllValid;
            if (this.preSelectedFiles.Length > 0)
                selectAllValid = false;
            else
                selectAllValid = true;

			System.Data.DataView importView = this.importData.Script.DefaultView;
			importView.Sort = "BuildOrder ASC";
			importView.RowStateFilter = System.Data.DataViewRowState.OriginalRows;

            for (int i = 0; i < importView.Count; i++)
            {
                SqlSyncBuildData.ScriptRow row = (SqlSyncBuildData.ScriptRow)importView[i].Row;

                ListViewItem item = new ListViewItem(new string[] { row.FileName, row.Database, row.ScriptId, this.importTempPath + row.FileName });
                item.Checked = false;

                if (this.buildData.Script.Select(
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
                    for (int j = 0; j < this.preSelectedFiles.Length; j++)
                    {
                        if (this.preSelectedFiles[j].ToLower() == row.FileName.ToLower())
                        {
                            item.Checked = true;
                            break;
                        }
                    }
                }


                this.lstImport.Items.Add(item);
            }

			if(foundConflict)
			{
				MessageBox.Show("A matching entry was already found. Please review list before accepting.","Please Review List",MessageBoxButtons.OK,MessageBoxIcon.Stop);
			}
				
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			for(int i=0;i<this.lstImport.Items.Count;i++)
			{
				if(this.lstImport.Items[i].Checked == false)
				{
					System.Data.DataRow[] rows = this.importData.Script.Select("ScriptId ='"+this.lstImport.Items[i].SubItems[2].Text+"'");
					if(rows.Length > 0)
						this.importData.Script.Rows.Remove(rows[0]);
				}
			}
			this.importData.AcceptChanges();

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void mnuView_Click(object sender, System.EventArgs e)
		{
			if(this.lstImport.SelectedItems.Count == 0)
				return;

			ListViewItem item = this.lstImport.SelectedItems[0];

			string name = item.SubItems[0].Text;
			string fullpath = item.SubItems[3].Text;
			AddScriptTextForm frmView = new AddScriptTextForm(ref this.importData, name,fullpath);
			frmView.ShowDialog();
		}

	}
}
