using System.Collections;
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
            get { return selectedUpdates; }
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PopulateUpdates));
            lstUpdates = new System.Windows.Forms.ListView();
            columnHeader1 = new System.Windows.Forms.ColumnHeader();
            columnHeader3 = new System.Windows.Forms.ColumnHeader();
            columnHeader4 = new System.Windows.Forms.ColumnHeader();
            columnHeader2 = new System.Windows.Forms.ColumnHeader();
            btnCancel = new System.Windows.Forms.Button();
            btnUpdate = new System.Windows.Forms.Button();
            lnkCheck = new System.Windows.Forms.LinkLabel();
            SuspendLayout();
            // 
            // lstUpdates
            // 
            lstUpdates.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            lstUpdates.CheckBoxes = true;
            lstUpdates.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader1,
            columnHeader3,
            columnHeader4,
            columnHeader2});
            lstUpdates.GridLines = true;
            lstUpdates.Location = new System.Drawing.Point(24, 24);
            lstUpdates.Name = "lstUpdates";
            lstUpdates.Size = new System.Drawing.Size(648, 248);
            lstUpdates.TabIndex = 1;
            lstUpdates.UseCompatibleStateImageBehavior = false;
            lstUpdates.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Script  Name";
            columnHeader1.Width = 241;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "Source Table";
            columnHeader3.Width = 178;
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "Source Database";
            columnHeader4.Width = 123;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Source Server";
            columnHeader2.Width = 100;
            // 
            // btnCancel
            // 
            btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            btnCancel.Location = new System.Drawing.Point(360, 296);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(112, 23);
            btnCancel.TabIndex = 4;
            btnCancel.Text = "Cancel";
            btnCancel.Click += new System.EventHandler(btnCancel_Click);
            // 
            // btnUpdate
            // 
            btnUpdate.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btnUpdate.FlatStyle = System.Windows.Forms.FlatStyle.System;
            btnUpdate.Location = new System.Drawing.Point(224, 296);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new System.Drawing.Size(128, 23);
            btnUpdate.TabIndex = 3;
            btnUpdate.Text = "Update Checked Files";
            btnUpdate.Click += new System.EventHandler(btnUpdate_Click);
            // 
            // lnkCheck
            // 
            lnkCheck.Location = new System.Drawing.Point(548, 8);
            lnkCheck.Name = "lnkCheck";
            lnkCheck.Size = new System.Drawing.Size(120, 12);
            lnkCheck.TabIndex = 5;
            lnkCheck.TabStop = true;
            lnkCheck.Text = "Uncheck All";
            lnkCheck.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            lnkCheck.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(lnkCheck_LinkClicked);
            // 
            // PopulateUpdates
            // 
            AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            ClientSize = new System.Drawing.Size(696, 326);
            Controls.Add(lnkCheck);
            Controls.Add(btnCancel);
            Controls.Add(btnUpdate);
            Controls.Add(lstUpdates);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            KeyPreview = true;
            Name = "PopulateUpdates";
            Text = "Code Table Populate Scripts Updates";
            Load += new System.EventHandler(PopulateUpdates_Load);
            KeyDown += new System.Windows.Forms.KeyEventHandler(PopulateUpdates_KeyDown);
            ResumeLayout(false);

        }
        #endregion

        private void PopulateUpdates_Load(object sender, System.EventArgs e)
        {
            if (updates == null || updates.Length == 0)
                return;

            for (int i = 0; i < updates.Length; i++)
            {
                ListViewItem item = new ListViewItem(new string[] { updates[i].ShortFileName, updates[i].SourceTable, updates[i].SourceDatabase, updates[i].SourceServer });
                item.Checked = true;
                lstUpdates.Items.Add(item);
            }
        }

        private void btnUpdate_Click(object sender, System.EventArgs e)
        {
            if (updates == null)
                return;

            DialogResult = DialogResult.OK;
            ArrayList checkedUpdates = new ArrayList();
            for (int i = 0; i < lstUpdates.CheckedItems.Count; i++)
            {
                for (int j = 0; j < updates.Length; j++)
                {
                    if (updates[j].ShortFileName == lstUpdates.CheckedItems[i].SubItems[0].Text)
                    {
                        checkedUpdates.Add(updates[j]);
                    }
                }
            }
            selectedUpdates = new SqlSync.SqlBuild.CodeTable.ScriptUpdates[checkedUpdates.Count];
            checkedUpdates.CopyTo(selectedUpdates);

            Close();
        }

        private void btnCancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void lnkCheck_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            if (lnkCheck.Text == "Uncheck All")
            {
                for (int i = 0; i < lstUpdates.Items.Count; i++)
                    lstUpdates.Items[i].Checked = false;
                lnkCheck.Text = "Check All";
            }
            else
            {
                for (int i = 0; i < lstUpdates.Items.Count; i++)
                    lstUpdates.Items[i].Checked = true;
                lnkCheck.Text = "Uncheck All";

            }
        }

        private void PopulateUpdates_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }
    }
}
