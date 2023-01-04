using System;
using System.Collections;
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
            get { return selectedUpdates; }
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

        #region Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ObjectUpdatesForm));
            columnHeader5 = new System.Windows.Forms.ColumnHeader();
            sqlConnect1 = new SqlSync.SQLConnect();
            btnChangeScripting = new System.Windows.Forms.Button();
            pnlSqlConnect = new System.Windows.Forms.Panel();
            btnCancelChangeSource = new System.Windows.Forms.Button();
            btnChangeSource = new System.Windows.Forms.Button();
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            helpToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            pnlSqlConnect.SuspendLayout();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "Source Object Name";
            columnHeader3.Width = 200;
            // 
            // btnCancel
            // 
            btnCancel.Location = new System.Drawing.Point(431, 489);
            btnCancel.Size = new System.Drawing.Size(134, 29);
            // 
            // btnUpdate
            // 
            btnUpdate.Location = new System.Drawing.Point(267, 489);
            btnUpdate.Size = new System.Drawing.Size(154, 29);
            btnUpdate.Click += new System.EventHandler(btnUpdate_Click);
            // 
            // lstUpdates
            // 
            lstUpdates.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader5});
            lstUpdates.FullRowSelect = true;
            lstUpdates.Location = new System.Drawing.Point(29, 47);
            lstUpdates.Size = new System.Drawing.Size(775, 430);
            // 
            // lnkCheck
            // 
            lnkCheck.Location = new System.Drawing.Point(826, 28);
            lnkCheck.Size = new System.Drawing.Size(144, 15);
            // 
            // columnHeader5
            // 
            columnHeader5.Text = "Object Type";
            columnHeader5.Width = 108;
            // 
            // sqlConnect1
            // 
            sqlConnect1.DisplayDatabaseDropDown = true;
            sqlConnect1.Location = new System.Drawing.Point(58, 20);
            sqlConnect1.Name = "sqlConnect1";
            sqlConnect1.Size = new System.Drawing.Size(316, 515);
            sqlConnect1.TabIndex = 6;
            // 
            // btnChangeScripting
            // 
            btnChangeScripting.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            btnChangeScripting.Location = new System.Drawing.Point(638, 482);
            btnChangeScripting.Name = "btnChangeScripting";
            btnChangeScripting.Size = new System.Drawing.Size(166, 27);
            btnChangeScripting.TabIndex = 7;
            btnChangeScripting.Text = "Change Scripting Source";
            btnChangeScripting.UseVisualStyleBackColor = true;
            btnChangeScripting.Click += new System.EventHandler(btnChangeScripting_Click);
            // 
            // pnlSqlConnect
            // 
            pnlSqlConnect.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            pnlSqlConnect.BackColor = System.Drawing.Color.LightSteelBlue;
            pnlSqlConnect.Controls.Add(btnCancelChangeSource);
            pnlSqlConnect.Controls.Add(btnChangeSource);
            pnlSqlConnect.Controls.Add(sqlConnect1);
            pnlSqlConnect.Location = new System.Drawing.Point(305, 15);
            pnlSqlConnect.Name = "pnlSqlConnect";
            pnlSqlConnect.Size = new System.Drawing.Size(267, 462);
            pnlSqlConnect.TabIndex = 8;
            // 
            // btnCancelChangeSource
            // 
            btnCancelChangeSource.Location = new System.Drawing.Point(247, 543);
            btnCancelChangeSource.Name = "btnCancelChangeSource";
            btnCancelChangeSource.Size = new System.Drawing.Size(90, 28);
            btnCancelChangeSource.TabIndex = 8;
            btnCancelChangeSource.Text = "Cancel";
            btnCancelChangeSource.UseVisualStyleBackColor = true;
            btnCancelChangeSource.Click += new System.EventHandler(btnCancelChangeSource_Click);
            // 
            // btnChangeSource
            // 
            btnChangeSource.Location = new System.Drawing.Point(101, 543);
            btnChangeSource.Name = "btnChangeSource";
            btnChangeSource.Size = new System.Drawing.Size(139, 28);
            btnChangeSource.TabIndex = 7;
            btnChangeSource.Text = "Change Source";
            btnChangeSource.UseVisualStyleBackColor = true;
            btnChangeSource.Click += new System.EventHandler(btnChangeSource_Click);
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            helpToolStripMenuItem});
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(832, 24);
            menuStrip1.TabIndex = 9;
            menuStrip1.Text = "menuStrip1";
            // 
            // helpToolStripMenuItem
            // 
            helpToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            helpToolStripMenuItem.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            helpToolStripMenuItem.Name = "helpToolStripMenuItem";
            helpToolStripMenuItem.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            helpToolStripMenuItem.Size = new System.Drawing.Size(12, 20);
            helpToolStripMenuItem.Click += new System.EventHandler(helpToolStripMenuItem_Click);
            // 
            // ObjectUpdatesForm
            // 
            AutoScaleBaseSize = new System.Drawing.Size(6, 16);
            ClientSize = new System.Drawing.Size(832, 526);
            Controls.Add(pnlSqlConnect);
            Controls.Add(btnChangeScripting);
            Controls.Add(menuStrip1);
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            MainMenuStrip = menuStrip1;
            Name = "ObjectUpdatesForm";
            Text = "Object Updates";
            Load += new System.EventHandler(ObjectUpdatesForm_Load);
            Controls.SetChildIndex(menuStrip1, 0);
            Controls.SetChildIndex(lnkCheck, 0);
            Controls.SetChildIndex(lstUpdates, 0);
            Controls.SetChildIndex(btnChangeScripting, 0);
            Controls.SetChildIndex(btnUpdate, 0);
            Controls.SetChildIndex(btnCancel, 0);
            Controls.SetChildIndex(pnlSqlConnect, 0);
            pnlSqlConnect.ResumeLayout(false);
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }
        #endregion

        private void ObjectUpdatesForm_Load(object sender, System.EventArgs e)
        {
            lstUpdates.BringToFront();
            pnlSqlConnect.Visible = false;
            if (updates != null)
            {
                for (int i = 0; i < updates.Length; i++)
                {
                    if (updates[i] != null)
                    {
                        ListViewItem item = new ListViewItem(new string[] { updates[i].ShortFileName, updates[i].SourceObject, updates[i].SourceDatabase, updates[i].SourceServer, updates[i].ObjectType });
                        item.Tag = updates[i];
                        item.Checked = true;
                        lstUpdates.Items.Add(item);
                    }
                }
            }
        }

        private void btnUpdate_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
            ArrayList checkedUpdates = new ArrayList();
            for (int i = 0; i < lstUpdates.CheckedItems.Count; i++)
            {

                //for(int j=0;j<this.updates.Length;j++)
                //{
                checkedUpdates.Add((ObjectUpdates)lstUpdates.CheckedItems[i].Tag);
                //    if(this.updates[j].ShortFileName == this.lstUpdates.CheckedItems[i].SubItems[0].Text)
                //        checkedUpdates.Add(this.updates[j]);
                //}
            }
            selectedUpdates = new SqlSync.SqlBuild.Objects.ObjectUpdates[checkedUpdates.Count];
            checkedUpdates.CopyTo(selectedUpdates);

            Close();
        }

        private void btnChangeScripting_Click(object sender, EventArgs e)
        {
            lstUpdates.Enabled = false;
            btnCancel.Enabled = false;
            btnUpdate.Enabled = false;
            btnChangeScripting.Enabled = false;
            pnlSqlConnect.BringToFront();
            pnlSqlConnect.Visible = true;

        }

        private void btnCancelChangeSource_Click(object sender, EventArgs e)
        {
            lstUpdates.Enabled = true;
            btnCancel.Enabled = true;
            btnUpdate.Enabled = true;
            btnChangeScripting.Enabled = true;
            pnlSqlConnect.SendToBack();
            pnlSqlConnect.Visible = false;
        }

        private void btnChangeSource_Click(object sender, EventArgs e)
        {
            if (sqlConnect1.Database.Length == 0)
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
            SqlSync.SqlBuild.UtilityHelper.OpenManual("UpdatingScriptedObjects");
        }
    }
}

