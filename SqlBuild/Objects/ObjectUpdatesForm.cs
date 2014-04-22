using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace SqlSync.SqlBuild.Objects
{
	public class ObjectUpdatesForm : SqlSync.SqlBuild.CodeTable.PopulateUpdates
	{
		private System.ComponentModel.IContainer components = null;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private ObjectUpdates[] updates;
        private SQLConnect sqlConnect1;
        private Button btnChangeScripting;
        private Panel pnlSqlConnect;
        private Button btnCancelChangeSource;
        private Button btnChangeSource;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem helpToolStripMenuItem;
		private ObjectUpdates[] selectedUpdates = new ObjectUpdates[0];

		public new SqlSync.SqlBuild.Objects.ObjectUpdates[] SelectedUpdates
		{
			get {  return selectedUpdates; }
			set { selectedUpdates = value; }    
		}

		public ObjectUpdatesForm()
		{
			// This call is required by the Windows Form Designer.
			InitializeComponent();

			
		}
		public ObjectUpdatesForm(ObjectUpdates[] updates)
		{
			// This call is required by the Windows Form Designer.
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
				if (components != null) 
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ObjectUpdatesForm));
            this.columnHeader5 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.sqlConnect1 = new SqlSync.SQLConnect();
            this.btnChangeScripting = new System.Windows.Forms.Button();
            this.pnlSqlConnect = new System.Windows.Forms.Panel();
            this.btnCancelChangeSource = new System.Windows.Forms.Button();
            this.btnChangeSource = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlSqlConnect.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Source Object Name";
            this.columnHeader3.Width = 200;
            // 
            // btnCancel
            // 
            this.btnCancel.Location = new System.Drawing.Point(428, 496);
            // 
            // btnUpdate
            // 
            this.btnUpdate.Location = new System.Drawing.Point(292, 496);
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // lstUpdates
            // 
            this.lstUpdates.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader5});
            this.lstUpdates.FullRowSelect = true;
            this.lstUpdates.Location = new System.Drawing.Point(24, 38);
            this.lstUpdates.Size = new System.Drawing.Size(784, 448);
            // 
            // lnkCheck
            // 
            this.lnkCheck.Location = new System.Drawing.Point(688, 23);
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Object Type";
            this.columnHeader5.Width = 108;
            // 
            // sqlConnect1
            // 
            this.sqlConnect1.DisplayDatabaseDropDown = true;
            this.sqlConnect1.Location = new System.Drawing.Point(48, 16);
            this.sqlConnect1.Name = "sqlConnect1";
            this.sqlConnect1.Size = new System.Drawing.Size(264, 419);
            this.sqlConnect1.TabIndex = 6;
            // 
            // btnChangeScripting
            // 
            this.btnChangeScripting.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnChangeScripting.Location = new System.Drawing.Point(670, 490);
            this.btnChangeScripting.Name = "btnChangeScripting";
            this.btnChangeScripting.Size = new System.Drawing.Size(138, 22);
            this.btnChangeScripting.TabIndex = 7;
            this.btnChangeScripting.Text = "Change Scripting Source";
            this.btnChangeScripting.UseVisualStyleBackColor = true;
            this.btnChangeScripting.Click += new System.EventHandler(this.btnChangeScripting_Click);
            // 
            // pnlSqlConnect
            // 
            this.pnlSqlConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlSqlConnect.BackColor = System.Drawing.Color.LightSteelBlue;
            this.pnlSqlConnect.Controls.Add(this.btnCancelChangeSource);
            this.pnlSqlConnect.Controls.Add(this.btnChangeSource);
            this.pnlSqlConnect.Controls.Add(this.sqlConnect1);
            this.pnlSqlConnect.Location = new System.Drawing.Point(254, 12);
            this.pnlSqlConnect.Name = "pnlSqlConnect";
            this.pnlSqlConnect.Size = new System.Drawing.Size(361, 474);
            this.pnlSqlConnect.TabIndex = 8;
            // 
            // btnCancelChangeSource
            // 
            this.btnCancelChangeSource.Location = new System.Drawing.Point(206, 441);
            this.btnCancelChangeSource.Name = "btnCancelChangeSource";
            this.btnCancelChangeSource.Size = new System.Drawing.Size(75, 23);
            this.btnCancelChangeSource.TabIndex = 8;
            this.btnCancelChangeSource.Text = "Cancel";
            this.btnCancelChangeSource.UseVisualStyleBackColor = true;
            this.btnCancelChangeSource.Click += new System.EventHandler(this.btnCancelChangeSource_Click);
            // 
            // btnChangeSource
            // 
            this.btnChangeSource.Location = new System.Drawing.Point(84, 441);
            this.btnChangeSource.Name = "btnChangeSource";
            this.btnChangeSource.Size = new System.Drawing.Size(116, 23);
            this.btnChangeSource.TabIndex = 7;
            this.btnChangeSource.Text = "Change Source";
            this.btnChangeSource.UseVisualStyleBackColor = true;
            this.btnChangeSource.Click += new System.EventHandler(this.btnChangeSource_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.helpToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(832, 24);
            this.menuStrip1.TabIndex = 9;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // helpToolStripMenuItem
            // 
            this.helpToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.helpToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Help_2;
            this.helpToolStripMenuItem.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            this.helpToolStripMenuItem.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.helpToolStripMenuItem.Size = new System.Drawing.Size(28, 20);
            this.helpToolStripMenuItem.Click += new System.EventHandler(this.helpToolStripMenuItem_Click);
            // 
            // ObjectUpdatesForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(832, 526);
            this.Controls.Add(this.pnlSqlConnect);
            this.Controls.Add(this.btnChangeScripting);
            this.Controls.Add(this.menuStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "ObjectUpdatesForm";
            this.Text = "Object Updates";
            this.Load += new System.EventHandler(this.ObjectUpdatesForm_Load);
            this.Controls.SetChildIndex(this.menuStrip1, 0);
            this.Controls.SetChildIndex(this.lnkCheck, 0);
            this.Controls.SetChildIndex(this.lstUpdates, 0);
            this.Controls.SetChildIndex(this.btnChangeScripting, 0);
            this.Controls.SetChildIndex(this.btnUpdate, 0);
            this.Controls.SetChildIndex(this.btnCancel, 0);
            this.Controls.SetChildIndex(this.pnlSqlConnect, 0);
            this.pnlSqlConnect.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void ObjectUpdatesForm_Load(object sender, System.EventArgs e)
		{
            this.lstUpdates.BringToFront();
            this.pnlSqlConnect.Visible = false;
			if(this.updates != null)
			{
				for(int i=0;i<this.updates.Length;i++)
				{
                    if (this.updates[i] != null)
                    {
                        ListViewItem item = new ListViewItem(new string[] { this.updates[i].ShortFileName, this.updates[i].SourceObject, this.updates[i].SourceDatabase, this.updates[i].SourceServer, this.updates[i].ObjectType });
                        item.Tag = this.updates[i];
                        item.Checked = true;
                        lstUpdates.Items.Add(item);
                    }
				}
			}
		}

		private void btnUpdate_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			ArrayList checkedUpdates = new ArrayList();
			for(int i=0;i<this.lstUpdates.CheckedItems.Count;i++)
			{

                //for(int j=0;j<this.updates.Length;j++)
                //{
                checkedUpdates.Add((ObjectUpdates)this.lstUpdates.CheckedItems[i].Tag);
                //    if(this.updates[j].ShortFileName == this.lstUpdates.CheckedItems[i].SubItems[0].Text)
                //        checkedUpdates.Add(this.updates[j]);
                //}
			}
			this.selectedUpdates = new SqlSync.SqlBuild.Objects.ObjectUpdates[checkedUpdates.Count];
			checkedUpdates.CopyTo(this.selectedUpdates);

			this.Close();
		}

        private void btnChangeScripting_Click(object sender, EventArgs e)
        {
            lstUpdates.Enabled = false;
            btnCancel.Enabled = false;
            btnUpdate.Enabled = false;
            btnChangeScripting.Enabled = false;
            pnlSqlConnect.BringToFront();
            this.pnlSqlConnect.Visible = true;

        }

        private void btnCancelChangeSource_Click(object sender, EventArgs e)
        {
            lstUpdates.Enabled = true;
            btnCancel.Enabled = true;
            btnUpdate.Enabled = true;
            btnChangeScripting.Enabled = true;
            pnlSqlConnect.SendToBack();
            this.pnlSqlConnect.Visible = false;
        }

        private void btnChangeSource_Click(object sender, EventArgs e)
        {
            if (this.sqlConnect1.Database.Length == 0)
            {
                MessageBox.Show("Please select a database first!", "Missing Database", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }


            string database = sqlConnect1.Database;
            string server = sqlConnect1.SQLServer;

            btnCancelChangeSource_Click(sender, e);

            for (int i = 0; i < lstUpdates.CheckedItems.Count; i++)
            {
                ListViewItem item = lstUpdates.CheckedItems[i];
                item.SubItems[2].Text = database;
                item.SubItems[3].Text = server;
                ObjectUpdates update = (ObjectUpdates)item.Tag;
                update.SourceDatabase = database;
                update.SourceServer = server;
                item.Tag = update;
            }

        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SqlSync.Utility.OpenManual("UpdatingScriptedObjects");
        }
    }
}

