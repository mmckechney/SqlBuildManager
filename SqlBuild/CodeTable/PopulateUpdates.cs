using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace SqlSync.SqlBuild.CodeTable
{
	/// <summary>
	/// Summary description for PopulateUpdates.
	/// </summary>
	public class PopulateUpdates : System.Windows.Forms.Form
	{
		protected System.Windows.Forms.ColumnHeader columnHeader1;
		protected System.Windows.Forms.ColumnHeader columnHeader2;
		protected System.Windows.Forms.ColumnHeader columnHeader3;
		protected System.Windows.Forms.ColumnHeader columnHeader4;
		protected System.Windows.Forms.Button btnCancel;
		protected System.Windows.Forms.Button btnUpdate;
		private CodeTable.ScriptUpdates[] updates;
		private CodeTable.ScriptUpdates[] selectedUpdates;
		protected System.Windows.Forms.ListView lstUpdates;
        protected LinkLabel lnkCheck;

		public SqlSync.SqlBuild.CodeTable.ScriptUpdates[] SelectedUpdates
		{
			get {  return selectedUpdates; }
			set { selectedUpdates = value; }
        }
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public PopulateUpdates()
		{
			InitializeComponent();
		}
		public PopulateUpdates(CodeTable.ScriptUpdates[] updates)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			this.updates = updates;
			
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PopulateUpdates));
            this.lstUpdates = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.lnkCheck = new System.Windows.Forms.LinkLabel();
            this.SuspendLayout();
            // 
            // lstUpdates
            // 
            this.lstUpdates.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstUpdates.CheckBoxes = true;
            this.lstUpdates.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader2});
            this.lstUpdates.GridLines = true;
            this.lstUpdates.Location = new System.Drawing.Point(24, 24);
            this.lstUpdates.Name = "lstUpdates";
            this.lstUpdates.Size = new System.Drawing.Size(648, 248);
            this.lstUpdates.TabIndex = 1;
            this.lstUpdates.UseCompatibleStateImageBehavior = false;
            this.lstUpdates.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Script  Name";
            this.columnHeader1.Width = 241;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Source Table";
            this.columnHeader3.Width = 178;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Source Database";
            this.columnHeader4.Width = 123;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Source Server";
            this.columnHeader2.Width = 100;
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnCancel.Location = new System.Drawing.Point(360, 296);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(112, 23);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnUpdate
            // 
            this.btnUpdate.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnUpdate.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnUpdate.Location = new System.Drawing.Point(224, 296);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(128, 23);
            this.btnUpdate.TabIndex = 3;
            this.btnUpdate.Text = "Update Checked Files";
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // lnkCheck
            // 
            this.lnkCheck.Location = new System.Drawing.Point(548, 8);
            this.lnkCheck.Name = "lnkCheck";
            this.lnkCheck.Size = new System.Drawing.Size(120, 12);
            this.lnkCheck.TabIndex = 5;
            this.lnkCheck.TabStop = true;
            this.lnkCheck.Text = "Uncheck All";
            this.lnkCheck.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lnkCheck.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkCheck_LinkClicked);
            // 
            // PopulateUpdates
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(696, 326);
            this.Controls.Add(this.lnkCheck);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.lstUpdates);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "PopulateUpdates";
            this.Text = "Code Table Populate Scripts Updates";
            this.Load += new System.EventHandler(this.PopulateUpdates_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.PopulateUpdates_KeyDown);
            this.ResumeLayout(false);

		}
		#endregion

		private void PopulateUpdates_Load(object sender, System.EventArgs e)
		{
			if(this.updates == null || this.updates.Length == 0)
				return;

			for(int i=0;i<this.updates.Length;i++)
			{
				ListViewItem item = new ListViewItem(new string[]{this.updates[i].ShortFileName,this.updates[i].SourceTable,this.updates[i].SourceDatabase,this.updates[i].SourceServer});
				item.Checked = true;
				lstUpdates.Items.Add(item);
			}
		}

		private void btnUpdate_Click(object sender, System.EventArgs e)
		{
			if(this.updates == null)
				return;

			this.DialogResult = DialogResult.OK;
			ArrayList checkedUpdates = new ArrayList();
			for(int i=0;i<this.lstUpdates.CheckedItems.Count;i++)
			{
				for(int j=0;j<this.updates.Length;j++)
				{
					if(this.updates[j].ShortFileName == this.lstUpdates.CheckedItems[i].SubItems[0].Text)
					{
						checkedUpdates.Add(this.updates[j]);
					}
				}
			}
			this.selectedUpdates = new SqlSync.SqlBuild.CodeTable.ScriptUpdates[checkedUpdates.Count];
			checkedUpdates.CopyTo(this.selectedUpdates);

			this.Close();
		}

		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void lnkCheck_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			if(lnkCheck.Text == "Uncheck All")
			{
				for(int i=0;i<this.lstUpdates.Items.Count;i++)
					this.lstUpdates.Items[i].Checked = false;
				lnkCheck.Text = "Check All";
			}
			else
			{
				for(int i=0;i<this.lstUpdates.Items.Count;i++)
					this.lstUpdates.Items[i].Checked = true;
				lnkCheck.Text = "Uncheck All";

			}
		}

		private void PopulateUpdates_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if(e.KeyCode == Keys.Escape)
			{
				this.DialogResult = DialogResult.Cancel;
				this.Close();
			}
		}
	}
}
