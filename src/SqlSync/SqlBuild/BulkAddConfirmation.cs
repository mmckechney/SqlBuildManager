using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Data;
using System.Collections.Generic;
namespace SqlSync.SqlBuild
{
	/// <summary>
	/// Summary description for BulkAddConfirmation.
	/// </summary>
	public class BulkAddConfirmation : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ListView lstBulkAdd;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.Button button1;
		private string  projectFilePath;
		private List<String> incommingFileList;
		private string[] _SelectedFiles = new string[0];
		private bool createNewEntries = false;
        private SqlSyncBuildData buildData = null;
		public bool CreateNewEntries
		{
			get {  return createNewEntries; }
			set { createNewEntries = value; }    
		}
	
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.RadioButton radUseCurrent;
		private System.Windows.Forms.RadioButton radCreateNew;

		public string[] SelectedFiles
		{
			get {  return _SelectedFiles; }
			set { _SelectedFiles = value; }    
		}
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public BulkAddConfirmation(List<string> fileList, string projectFilePath, SqlSyncBuildData buildData)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			this.projectFilePath = projectFilePath;
			this.incommingFileList = fileList;
            this.buildData = buildData;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BulkAddConfirmation));
            this.lstBulkAdd = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.button1 = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.radUseCurrent = new System.Windows.Forms.RadioButton();
            this.radCreateNew = new System.Windows.Forms.RadioButton();
            this.label1 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // lstBulkAdd
            // 
            this.lstBulkAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstBulkAdd.CheckBoxes = true;
            this.lstBulkAdd.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.lstBulkAdd.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lstBulkAdd.GridLines = true;
            this.lstBulkAdd.Location = new System.Drawing.Point(16, 16);
            this.lstBulkAdd.Name = "lstBulkAdd";
            this.lstBulkAdd.Size = new System.Drawing.Size(688, 248);
            this.lstBulkAdd.TabIndex = 0;
            this.lstBulkAdd.UseCompatibleStateImageBehavior = false;
            this.lstBulkAdd.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "File Name";
            this.columnHeader1.Width = 191;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Current Path";
            this.columnHeader2.Width = 488;
            // 
            // button1
            // 
            this.button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button1.Location = new System.Drawing.Point(236, 328);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(128, 23);
            this.button1.TabIndex = 1;
            this.button1.Text = "Add Checked Files";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // button2
            // 
            this.button2.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button2.Location = new System.Drawing.Point(372, 328);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(112, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "Cancel";
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // radUseCurrent
            // 
            this.radUseCurrent.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.radUseCurrent.Checked = true;
            this.radUseCurrent.Location = new System.Drawing.Point(108, 296);
            this.radUseCurrent.Name = "radUseCurrent";
            this.radUseCurrent.Size = new System.Drawing.Size(248, 16);
            this.radUseCurrent.TabIndex = 3;
            this.radUseCurrent.TabStop = true;
            this.radUseCurrent.Text = "Use current script entry for existing files";
            // 
            // radCreateNew
            // 
            this.radCreateNew.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.radCreateNew.Location = new System.Drawing.Point(356, 296);
            this.radCreateNew.Name = "radCreateNew";
            this.radCreateNew.Size = new System.Drawing.Size(256, 16);
            this.radCreateNew.TabIndex = 4;
            this.radCreateNew.Text = "Create new script entry for existing files";
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.label1.Location = new System.Drawing.Point(16, 264);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(256, 23);
            this.label1.TabIndex = 5;
            this.label1.Text = "* Colored items denote pre-existing files";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // BulkAddConfirmation
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(720, 366);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.radCreateNew);
            this.Controls.Add(this.radUseCurrent);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.lstBulkAdd);
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "BulkAddConfirmation";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Bulk Add Confirmation";
            this.TopMost = true;
            this.Closing += new System.ComponentModel.CancelEventHandler(this.BulkAddConfirmation_Closing);
            this.Load += new System.EventHandler(this.BulkAddConfirmation_Load);
            this.ResumeLayout(false);

		}
		#endregion

		private void BulkAddConfirmation_Load(object sender, System.EventArgs e)
		{
			for(int i=0;i<this.incommingFileList.Count;i++)
			{
				System.Drawing.Color bgColor = Color.White;

                var val = from s  in this.buildData.Script.AsEnumerable().Cast<SqlSyncBuildData.ScriptRow>()
                          where s.FileName == Path.GetFileName(this.incommingFileList[i])
                          select s.FileName;

                bool inProject = (val != null && val.Count() > 0);
                          
				string newLocalFile = Path.Combine(this.projectFilePath,Path.GetFileName(incommingFileList[i]));
                if (File.Exists(newLocalFile) && inProject)
					bgColor = Color.PeachPuff;

                ListViewItem item;
                if (this.incommingFileList[i].StartsWith("Error."))
                {
                    item = new ListViewItem(new string[] {"Error!", incommingFileList[i] });
                    item.Checked = false;
                    item.BackColor = Color.Red;
                }
                else
                {
                    item = new ListViewItem(new string[] { Path.GetFileName(newLocalFile), incommingFileList[i] });
                    item.Checked = true;
                    item.BackColor = bgColor;
                }
				this.lstBulkAdd.Items.Add(item);

			}

		}

		private void BulkAddConfirmation_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			ArrayList files = new ArrayList();
			for(int i=0;i<lstBulkAdd.Items.Count;i++)
			{
				if(lstBulkAdd.Items[i].Checked)
					files.Add(lstBulkAdd.Items[i].SubItems[1].Text);
			}
			this._SelectedFiles = new string[files.Count];
			files.CopyTo(this._SelectedFiles);

			if(radCreateNew.Checked)
				this.createNewEntries = true;
			else
				this.createNewEntries = false;
		}

		private void button2_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			this.DialogResult  =DialogResult.OK;
			this.Close();
		}

		
	
	}
}
