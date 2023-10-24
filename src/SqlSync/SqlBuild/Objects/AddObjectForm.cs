using SqlSync.BuildHistory;
using SqlSync.Connection;
using SqlSync.DbInformation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
namespace SqlSync.SqlBuild.Objects
{
    /// <summary>
    /// Summary description for AddObjectForm.
    /// </summary>
    public class AddObjectForm : System.Windows.Forms.Form
    {

        private SqlSync.ColumnSorter listSorter = new ColumnSorter();
        protected System.Windows.Forms.ColumnHeader columnHeader1;
        protected System.Windows.Forms.ColumnHeader columnHeader3;
        protected System.Windows.Forms.ColumnHeader columnHeader4;
        protected System.Windows.Forms.Button btnCancel;
        protected System.Windows.Forms.Button btnUpdate;
        private List<ObjectData> objData;
        private string objectType;
        private string description;
        private ConnectionData connData;
        private List<ObjectData> selectedObjects = new List<ObjectData>();
        public List<ObjectData> SelectedObjects
        {
            get
            {
                return selectedObjects;
            }
        }
        /// <summary>
        /// Required designer variable.
        /// </summary>
        protected System.Windows.Forms.ListView lstAdds;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem previewObjectScriptToolStripMenuItem;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel statGeneral;
        private Label label1;
        private TextBox txtFind;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem viewObjectsSqlBuildHistoryToolStripMenuItem;
        private ToolTip toolTip1;
        private IContainer components;

        public AddObjectForm(List<ObjectData> objData, string description, string objectType, ConnectionData connData)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.objData = objData;
            this.connData = connData;
            this.description = description;
            this.objectType = objectType;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddObjectForm));
            lstAdds = new System.Windows.Forms.ListView();
            columnHeader1 = new System.Windows.Forms.ColumnHeader();
            columnHeader3 = new System.Windows.Forms.ColumnHeader();
            columnHeader4 = new System.Windows.Forms.ColumnHeader();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            previewObjectScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            viewObjectsSqlBuildHistoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            btnCancel = new System.Windows.Forms.Button();
            btnUpdate = new System.Windows.Forms.Button();
            statusStrip1 = new System.Windows.Forms.StatusStrip();
            statGeneral = new System.Windows.Forms.ToolStripStatusLabel();
            label1 = new System.Windows.Forms.Label();
            txtFind = new System.Windows.Forms.TextBox();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            contextMenuStrip1.SuspendLayout();
            statusStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // lstAdds
            // 
            lstAdds.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            lstAdds.CheckBoxes = true;
            lstAdds.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader1,
            columnHeader3,
            columnHeader4});
            lstAdds.ContextMenuStrip = contextMenuStrip1;
            lstAdds.FullRowSelect = true;
            lstAdds.GridLines = true;
            lstAdds.HideSelection = false;
            lstAdds.Location = new System.Drawing.Point(19, 20);
            lstAdds.Name = "lstAdds";
            lstAdds.Size = new System.Drawing.Size(618, 425);
            lstAdds.TabIndex = 2;
            lstAdds.UseCompatibleStateImageBehavior = false;
            lstAdds.View = System.Windows.Forms.View.Details;
            lstAdds.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(lstAdds_ColumnClick);
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Object Name";
            columnHeader1.Width = 273;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "Altered Date";
            columnHeader3.Width = 178;
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "Created Date";
            columnHeader4.Width = 155;
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            previewObjectScriptToolStripMenuItem,
            toolStripSeparator1,
            viewObjectsSqlBuildHistoryToolStripMenuItem});
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(377, 54);
            // 
            // previewObjectScriptToolStripMenuItem
            // 
            previewObjectScriptToolStripMenuItem.Name = "previewObjectScriptToolStripMenuItem";
            previewObjectScriptToolStripMenuItem.Size = new System.Drawing.Size(376, 22);
            previewObjectScriptToolStripMenuItem.Text = "Preview Object Script";
            previewObjectScriptToolStripMenuItem.Click += new System.EventHandler(previewObjectScriptToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(373, 6);
            // 
            // viewObjectsSqlBuildHistoryToolStripMenuItem
            // 
            viewObjectsSqlBuildHistoryToolStripMenuItem.Name = "viewObjectsSqlBuildHistoryToolStripMenuItem";
            viewObjectsSqlBuildHistoryToolStripMenuItem.Size = new System.Drawing.Size(376, 22);
            viewObjectsSqlBuildHistoryToolStripMenuItem.Text = "View Object\'s change history as run by Sql Build Manager";
            viewObjectsSqlBuildHistoryToolStripMenuItem.ToolTipText = "If a database object has been updated in the past via the Sql Build Manager tool," +
    " this will show you the history and allow you to compare versions.";
            viewObjectsSqlBuildHistoryToolStripMenuItem.Click += new System.EventHandler(viewObjectsSqlBuildHistoryToolStripMenuItem_Click);
            // 
            // btnCancel
            // 
            btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            btnCancel.Location = new System.Drawing.Point(343, 487);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(134, 28);
            btnCancel.TabIndex = 6;
            btnCancel.Text = "Cancel";
            btnCancel.Click += new System.EventHandler(btnCancel_Click);
            // 
            // btnUpdate
            // 
            btnUpdate.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btnUpdate.FlatStyle = System.Windows.Forms.FlatStyle.System;
            btnUpdate.Location = new System.Drawing.Point(180, 487);
            btnUpdate.Name = "btnUpdate";
            btnUpdate.Size = new System.Drawing.Size(153, 28);
            btnUpdate.TabIndex = 5;
            btnUpdate.Text = "Add Checked Files";
            btnUpdate.Click += new System.EventHandler(btnUpdate_Click);
            // 
            // statusStrip1
            // 
            statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            statGeneral});
            statusStrip1.Location = new System.Drawing.Point(0, 525);
            statusStrip1.Name = "statusStrip1";
            statusStrip1.Size = new System.Drawing.Size(656, 22);
            statusStrip1.TabIndex = 7;
            statusStrip1.Text = "statusStrip1";
            // 
            // statGeneral
            // 
            statGeneral.Name = "statGeneral";
            statGeneral.Size = new System.Drawing.Size(641, 17);
            statGeneral.Spring = true;
            statGeneral.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            label1.AutoSize = true;
            label1.Location = new System.Drawing.Point(19, 454);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(36, 15);
            label1.TabIndex = 8;
            label1.Text = "Filter:";
            // 
            // txtFind
            // 
            txtFind.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            txtFind.Location = new System.Drawing.Point(60, 449);
            txtFind.Name = "txtFind";
            txtFind.Size = new System.Drawing.Size(577, 23);
            txtFind.TabIndex = 9;
            txtFind.TextChanged += new System.EventHandler(txtFind_TextChanged);
            // 
            // AddObjectForm
            // 
            AutoScaleBaseSize = new System.Drawing.Size(6, 16);
            ClientSize = new System.Drawing.Size(656, 547);
            Controls.Add(txtFind);
            Controls.Add(label1);
            Controls.Add(statusStrip1);
            Controls.Add(btnCancel);
            Controls.Add(btnUpdate);
            Controls.Add(lstAdds);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            KeyPreview = true;
            Name = "AddObjectForm";
            Text = "Add New {0} Scripts From {1}.{2}";
            Load += new System.EventHandler(AddObjectForm_Load);
            KeyDown += new System.Windows.Forms.KeyEventHandler(AddObjectForm_KeyDown);
            contextMenuStrip1.ResumeLayout(false);
            statusStrip1.ResumeLayout(false);
            statusStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }
        #endregion

        private void AddObjectForm_Load(object sender, System.EventArgs e)
        {
            Text = String.Format(Text, description, connData.SQLServerName, connData.DatabaseName);
            listSorter.IncludeNonSchemaSortOption = true;


            for (int i = 0; i < objData.Count; i++)
            {
                AddItemToList(objData[i]);
            }
            statGeneral.Text = "Total Objects Listed: " + objData.Count.ToString();


        }

        private void btnUpdate_Click(object sender, System.EventArgs e)
        {
            for (int i = 0; i < lstAdds.CheckedItems.Count; i++)
                selectedObjects.Add((ObjectData)lstAdds.CheckedItems[i].Tag);

            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void AddObjectForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private void previewObjectScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lstAdds.SelectedItems.Count == 0)
                return;

            ListViewItem item = lstAdds.SelectedItems[0];
            ObjectData objData = (ObjectData)item.Tag;
            string message;
            string script = GetScriptPreviewText(objData, out message);

            if (script.Length > 0 || message.Length > 0)
            {
                ScriptDisplayForm frmDisplay;
                if (script.Length > 0)
                    frmDisplay = new ScriptDisplayForm(script, connData.SQLServerName, item.SubItems[0].Text);
                else
                    frmDisplay = new ScriptDisplayForm(message, connData.SQLServerName, item.SubItems[0].Text);

                frmDisplay.ShowDialog();
            }
        }
        private string GetScriptPreviewText(ObjectData objData, out string message)
        {
            SqlSync.ObjectScript.ObjectScriptHelper helper = new SqlSync.ObjectScript.ObjectScriptHelper(connData, SqlSync.Properties.Settings.Default.ScriptAsAlter, SqlSync.Properties.Settings.Default.ScriptPermissions, SqlSync.Properties.Settings.Default.ScriptPkWithTables);
            string script = string.Empty;
            string desc = string.Empty;

            switch (objectType)
            {
                case SqlSync.Constants.DbObjectType.View:
                    helper.ScriptDatabaseObject(SqlSync.Constants.DbObjectType.View, objData.ObjectName, objData.SchemaOwner, ref script, ref desc, out message);
                    break;
                case SqlSync.Constants.DbObjectType.StoredProcedure:
                    helper.ScriptDatabaseObject(SqlSync.Constants.DbObjectType.StoredProcedure, objData.ObjectName, objData.SchemaOwner, ref script, ref desc, out message);
                    break;
                case SqlSync.Constants.DbObjectType.UserDefinedFunction:
                    helper.ScriptDatabaseObject(SqlSync.Constants.DbObjectType.UserDefinedFunction, objData.ObjectName, objData.SchemaOwner, ref script, ref desc, out message);
                    break;
                case SqlSync.Constants.DbObjectType.Table:
                    helper.ScriptDatabaseObject(SqlSync.Constants.DbObjectType.Table, objData.ObjectName, objData.SchemaOwner, ref script, ref desc, out message);
                    break;
                case SqlSync.Constants.DbObjectType.Trigger:
                    helper.ScriptDatabaseObject(SqlSync.Constants.DbObjectType.Trigger, objData.ObjectName, objData.SchemaOwner, ref script, ref desc, out message);
                    break;
                default:
                    message = string.Empty;
                    break;
            }

            return script;
        }
        private void lstAdds_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListViewItem item = null;

            if (lstAdds.SelectedItems.Count > 0)
                item = lstAdds.SelectedItems[0];

            listSorter.CurrentColumn = e.Column;
            lstAdds.ListViewItemSorter = listSorter;
            lstAdds.Sort();
            if (item != null)
                item.EnsureVisible();
        }

        private void txtFind_TextChanged(object sender, EventArgs e)
        {
            if (txtFind.Text.Length > 0)
            {
                if (lstAdds.Groups.Count == 0)
                {
                    ListViewGroup grpFiltered = new ListViewGroup("Filtered");
                    grpFiltered.Header = "Filtered";
                    grpFiltered.HeaderAlignment = HorizontalAlignment.Left;
                    lstAdds.Groups.Add(grpFiltered);

                    ListViewGroup grpObjectScripts = new ListViewGroup(description + " Objects");
                    grpObjectScripts.Header = description + " Objects";
                    grpObjectScripts.HeaderAlignment = HorizontalAlignment.Left;
                    lstAdds.Groups.Add(grpObjectScripts);
                }

                for (int i = 0; i < lstAdds.Items.Count; i++)
                {
                    if (lstAdds.Items[i].SubItems[0].Text.IndexOf(txtFind.Text, 0, StringComparison.CurrentCultureIgnoreCase) > -1)
                        lstAdds.Items[i].Group = lstAdds.Groups[0];
                    else
                        lstAdds.Items[i].Group = lstAdds.Groups[1];
                }
            }
            else
            {
                lstAdds.Groups.Clear();
            }

        }

        private void AddItemToList(ObjectData objectData)
        {
            ListViewItem item = new ListViewItem(new string[] { objectData.SchemaOwner + "." + objectData.ObjectName, objectData.AlteredDate.ToString(), objectData.CreateDate.ToString() });
            item.Tag = objectData;
            //item.Group = this.lstAdds.Groups[1]; 
            lstAdds.Items.Add(item);
        }

        private void viewObjectsSqlBuildHistoryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lstAdds.SelectedItems.Count == 0)
                return;

            ListViewItem item = lstAdds.SelectedItems[0];
            ObjectData objData = (ObjectData)item.Tag;
            string message;
            string script = GetScriptPreviewText(objData, out message);

            if (script.Length > 0 || message.Length > 0)
            {
                if (script.Length > 0)
                {
                    string fileName = String.Format("{0}.{1}{2}", objData.SchemaOwner, objData.ObjectName, objectType);
                    ScriptRunHistoryForm frmHist = new ScriptRunHistoryForm(connData, connData.DatabaseName, fileName, script, "");
                    frmHist.ShowDialog();
                    frmHist.Dispose();
                }
                else
                {
                    MessageBox.Show(message, "Unable to script object", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }
    }
}
