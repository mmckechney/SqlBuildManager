using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using SqlSync.DbInformation;
using SqlSync.Connection;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SqlSync.BuildHistory;
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
				return this.selectedObjects;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddObjectForm));
            this.lstAdds = new System.Windows.Forms.ListView();
            this.columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader3 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.columnHeader4 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.previewObjectScriptToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnUpdate = new System.Windows.Forms.Button();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statGeneral = new System.Windows.Forms.ToolStripStatusLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.txtFind = new System.Windows.Forms.TextBox();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.viewObjectsSqlBuildHistoryToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.contextMenuStrip1.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstAdds
            // 
            this.lstAdds.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstAdds.CheckBoxes = true;
            this.lstAdds.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader3,
            this.columnHeader4});
            this.lstAdds.ContextMenuStrip = this.contextMenuStrip1;
            this.lstAdds.FullRowSelect = true;
            this.lstAdds.GridLines = true;
            this.lstAdds.Location = new System.Drawing.Point(16, 16);
            this.lstAdds.Name = "lstAdds";
            this.lstAdds.Size = new System.Drawing.Size(624, 448);
            this.lstAdds.TabIndex = 2;
            this.lstAdds.UseCompatibleStateImageBehavior = false;
            this.lstAdds.View = System.Windows.Forms.View.Details;
            this.lstAdds.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lstAdds_ColumnClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Object Name";
            this.columnHeader1.Width = 273;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Altered Date";
            this.columnHeader3.Width = 178;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Created Date";
            this.columnHeader4.Width = 155;
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.previewObjectScriptToolStripMenuItem,
            this.toolStripSeparator1,
            this.viewObjectsSqlBuildHistoryToolStripMenuItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(359, 76);
            // 
            // previewObjectScriptToolStripMenuItem
            // 
            this.previewObjectScriptToolStripMenuItem.Name = "previewObjectScriptToolStripMenuItem";
            this.previewObjectScriptToolStripMenuItem.Size = new System.Drawing.Size(358, 22);
            this.previewObjectScriptToolStripMenuItem.Text = "Preview Object Script";
            this.previewObjectScriptToolStripMenuItem.Click += new System.EventHandler(this.previewObjectScriptToolStripMenuItem_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnCancel.Location = new System.Drawing.Point(340, 498);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(112, 23);
            this.btnCancel.TabIndex = 6;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnUpdate
            // 
            this.btnUpdate.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnUpdate.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnUpdate.Location = new System.Drawing.Point(204, 498);
            this.btnUpdate.Name = "btnUpdate";
            this.btnUpdate.Size = new System.Drawing.Size(128, 23);
            this.btnUpdate.TabIndex = 5;
            this.btnUpdate.Text = "Add Checked Files";
            this.btnUpdate.Click += new System.EventHandler(this.btnUpdate_Click);
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statGeneral});
            this.statusStrip1.Location = new System.Drawing.Point(0, 525);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(656, 22);
            this.statusStrip1.TabIndex = 7;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statGeneral
            // 
            this.statGeneral.Name = "statGeneral";
            this.statGeneral.Size = new System.Drawing.Size(641, 17);
            this.statGeneral.Spring = true;
            this.statGeneral.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label1
            // 
            this.label1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 471);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(32, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Filter:";
            // 
            // txtFind
            // 
            this.txtFind.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtFind.Location = new System.Drawing.Point(50, 467);
            this.txtFind.Name = "txtFind";
            this.txtFind.Size = new System.Drawing.Size(590, 20);
            this.txtFind.TabIndex = 9;
            this.txtFind.TextChanged += new System.EventHandler(this.txtFind_TextChanged);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(355, 6);
            // 
            // viewObjectsSqlBuildHistoryToolStripMenuItem
            // 
            this.viewObjectsSqlBuildHistoryToolStripMenuItem.Name = "viewObjectsSqlBuildHistoryToolStripMenuItem";
            this.viewObjectsSqlBuildHistoryToolStripMenuItem.Size = new System.Drawing.Size(358, 22);
            this.viewObjectsSqlBuildHistoryToolStripMenuItem.Text = "View Object\'s change history as run by Sql Build Manager";
            this.viewObjectsSqlBuildHistoryToolStripMenuItem.ToolTipText = "If a database object has been updated in the past via the Sql Build Manager tool," +
                " this will show you the history and allow you to compare versions.";
            this.viewObjectsSqlBuildHistoryToolStripMenuItem.Click += new System.EventHandler(this.viewObjectsSqlBuildHistoryToolStripMenuItem_Click);
            // 
            // AddObjectForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(656, 547);
            this.Controls.Add(this.txtFind);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.statusStrip1);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnUpdate);
            this.Controls.Add(this.lstAdds);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "AddObjectForm";
            this.Text = "Add New {0} Scripts From {1}.{2}";
            this.Load += new System.EventHandler(this.AddObjectForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AddObjectForm_KeyDown);
            this.contextMenuStrip1.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void AddObjectForm_Load(object sender, System.EventArgs e)
		{
            this.Text = String.Format(this.Text, this.description, this.connData.SQLServerName, this.connData.DatabaseName);
            listSorter.IncludeNonSchemaSortOption = true;
         
            
			for(int i=0;i<this.objData.Count;i++)
			{
                AddItemToList(this.objData[i]);
			}
            statGeneral.Text = "Total Objects Listed: " + this.objData.Count.ToString();

           
		}

		private void btnUpdate_Click(object sender, System.EventArgs e)
		{
            for (int i = 0; i < this.lstAdds.CheckedItems.Count; i++)
                this.selectedObjects.Add((ObjectData)this.lstAdds.CheckedItems[i].Tag);

			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void btnCancel_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void AddObjectForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			if(e.KeyCode == Keys.Escape)
			{
				this.DialogResult = DialogResult.Cancel;
				this.Close();
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
                if(script.Length > 0)
                    frmDisplay = new ScriptDisplayForm(script, this.connData.SQLServerName, item.SubItems[0].Text);
                else
                    frmDisplay = new ScriptDisplayForm(message, this.connData.SQLServerName, item.SubItems[0].Text);

                frmDisplay.ShowDialog();
            }
        }
        private string GetScriptPreviewText(ObjectData objData, out string message)
        {
            SqlSync.ObjectScript.ObjectScriptHelper helper = new SqlSync.ObjectScript.ObjectScriptHelper(this.connData, SqlSync.Properties.Settings.Default.ScriptAsAlter, SqlSync.Properties.Settings.Default.ScriptPermissions, SqlSync.Properties.Settings.Default.ScriptPkWithTables);
            string script = string.Empty;
            string desc = string.Empty;
            
            switch (this.objectType)
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
                if (this.lstAdds.Groups.Count == 0)
                {
                    ListViewGroup grpFiltered = new ListViewGroup("Filtered");
                    grpFiltered.Header = "Filtered";
                    grpFiltered.HeaderAlignment = HorizontalAlignment.Left;
                    lstAdds.Groups.Add(grpFiltered);

                    ListViewGroup grpObjectScripts = new ListViewGroup(this.description + " Objects");
                    grpObjectScripts.Header = this.description + " Objects";
                    grpObjectScripts.HeaderAlignment = HorizontalAlignment.Left;
                    lstAdds.Groups.Add(grpObjectScripts);
                }

                for (int i = 0; i < this.lstAdds.Items.Count; i++)
                {
                    if (this.lstAdds.Items[i].SubItems[0].Text.IndexOf(txtFind.Text, 0, StringComparison.CurrentCultureIgnoreCase) > -1)
                        this.lstAdds.Items[i].Group = this.lstAdds.Groups[0];
                    else
                        this.lstAdds.Items[i].Group = this.lstAdds.Groups[1];  
                }
            }
            else
            {
                lstAdds.Groups.Clear();
            }
           
        }

        private void AddItemToList(ObjectData objectData)
        {
            ListViewItem item = new ListViewItem(new string[] { objectData.SchemaOwner + "." + objectData.ObjectName, objectData.StrAlteredDate, objectData.StrCreateDate });
            item.Tag = objectData;
            //item.Group = this.lstAdds.Groups[1]; 
            this.lstAdds.Items.Add(item);
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
                    string fileName = String.Format("{0}.{1}{2}", objData.SchemaOwner, objData.ObjectName, this.objectType);
                    ScriptRunHistoryForm frmHist = new ScriptRunHistoryForm(connData, this.connData.DatabaseName, fileName, script, "");
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
