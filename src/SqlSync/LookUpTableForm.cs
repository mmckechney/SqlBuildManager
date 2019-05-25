using System;
using System.Data;
using System.Drawing;
using System.Collections;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using System.Reflection;

namespace SQLSync
{
	/// <summary>
	/// Summary description for LookUpTable.
	/// </summary>
	public class LookUpTableForm : System.Windows.Forms.Form
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		ConnectionData data = null;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
		private System.Windows.Forms.StatusBar statusBar1;
		private System.Windows.Forms.StatusBarPanel statStatus;
		private System.Windows.Forms.ContextMenu contextMenu1;
		private System.Windows.Forms.MenuItem mnuAddTable;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.MenuItem mnuDeleteTable;
		private System.Windows.Forms.MenuItem mnuWhereClause;
		private System.Windows.Forms.MenuItem menuItem3;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.LinkLabel lnkLoadProject;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.GroupBox grpScripting;
		private System.Windows.Forms.CheckBox chkReplaceDateAndId;
		private System.Windows.Forms.LinkLabel lnkSaveScripts;
		private System.Windows.Forms.LinkLabel lnkGenerateScripts;
		private System.Windows.Forms.LinkLabel lnkCheckAll;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListView lstTables;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		SQLSync.SQLSyncData tableList = null;
		private System.Windows.Forms.Label lblCurrentProject;
		private System.Windows.Forms.TabControl tcTables;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ComboBox ddDatabaseList;
		private System.Windows.Forms.ContextMenu contextDatabase;
		private System.Windows.Forms.MenuItem mnuRemoveDatabase;
		private string projectFileName = string.Empty;
		private string newDatabaseName = string.Empty;
		public LookUpTableForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

		}
		public LookUpTableForm(ConnectionData data)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			this.data = data;

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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(LookUpTableForm));
			this.contextMenu1 = new System.Windows.Forms.ContextMenu();
			this.mnuWhereClause = new System.Windows.Forms.MenuItem();
			this.menuItem3 = new System.Windows.Forms.MenuItem();
			this.mnuAddTable = new System.Windows.Forms.MenuItem();
			this.menuItem1 = new System.Windows.Forms.MenuItem();
			this.mnuDeleteTable = new System.Windows.Forms.MenuItem();
			this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
			this.statusBar1 = new System.Windows.Forms.StatusBar();
			this.statStatus = new System.Windows.Forms.StatusBarPanel();
			this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
			this.lnkLoadProject = new System.Windows.Forms.LinkLabel();
			this.label2 = new System.Windows.Forms.Label();
			this.lblCurrentProject = new System.Windows.Forms.Label();
			this.grpScripting = new System.Windows.Forms.GroupBox();
			this.ddDatabaseList = new System.Windows.Forms.ComboBox();
			this.contextDatabase = new System.Windows.Forms.ContextMenu();
			this.mnuRemoveDatabase = new System.Windows.Forms.MenuItem();
			this.label3 = new System.Windows.Forms.Label();
			this.chkReplaceDateAndId = new System.Windows.Forms.CheckBox();
			this.lnkSaveScripts = new System.Windows.Forms.LinkLabel();
			this.lnkGenerateScripts = new System.Windows.Forms.LinkLabel();
			this.tcTables = new System.Windows.Forms.TabControl();
			this.lnkCheckAll = new System.Windows.Forms.LinkLabel();
			this.label1 = new System.Windows.Forms.Label();
			this.lstTables = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			((System.ComponentModel.ISupportInitialize)(this.statStatus)).BeginInit();
			this.grpScripting.SuspendLayout();
			this.SuspendLayout();
			// 
			// contextMenu1
			// 
			this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																						 this.mnuWhereClause,
																						 this.menuItem3,
																						 this.mnuAddTable,
																						 this.menuItem1,
																						 this.mnuDeleteTable});
			// 
			// mnuWhereClause
			// 
			this.mnuWhereClause.Index = 0;
			this.mnuWhereClause.Text = "Add/View \"Where\" Clause";
			this.mnuWhereClause.Click += new System.EventHandler(this.mnuWhereClause_Click);
			// 
			// menuItem3
			// 
			this.menuItem3.Index = 1;
			this.menuItem3.Text = "-";
			// 
			// mnuAddTable
			// 
			this.mnuAddTable.Index = 2;
			this.mnuAddTable.Text = "Add Table";
			this.mnuAddTable.Click += new System.EventHandler(this.mnuAddTable_Click);
			// 
			// menuItem1
			// 
			this.menuItem1.Index = 3;
			this.menuItem1.Text = "-";
			// 
			// mnuDeleteTable
			// 
			this.mnuDeleteTable.Index = 4;
			this.mnuDeleteTable.Text = "Delete Selected Table";
			this.mnuDeleteTable.Click += new System.EventHandler(this.mnuDeleteTable_Click);
			// 
			// folderBrowserDialog1
			// 
			this.folderBrowserDialog1.Description = "Select a folder to save your scripts";
			// 
			// statusBar1
			// 
			this.statusBar1.Location = new System.Drawing.Point(0, 600);
			this.statusBar1.Name = "statusBar1";
			this.statusBar1.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
																						  this.statStatus});
			this.statusBar1.ShowPanels = true;
			this.statusBar1.Size = new System.Drawing.Size(952, 22);
			this.statusBar1.TabIndex = 6;
			this.statusBar1.Text = "statusBar1";
			// 
			// statStatus
			// 
			this.statStatus.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
			this.statStatus.Width = 936;
			// 
			// openFileDialog1
			// 
			this.openFileDialog1.CheckFileExists = false;
			this.openFileDialog1.DefaultExt = "xml";
			this.openFileDialog1.Filter = "XML Files|*.xml|All Files|*.*";
			this.openFileDialog1.Title = "Open SQL Sync Project File";
			// 
			// lnkLoadProject
			// 
			this.lnkLoadProject.Location = new System.Drawing.Point(16, 16);
			this.lnkLoadProject.Name = "lnkLoadProject";
			this.lnkLoadProject.Size = new System.Drawing.Size(104, 16);
			this.lnkLoadProject.TabIndex = 8;
			this.lnkLoadProject.TabStop = true;
			this.lnkLoadProject.Text = "Load Project file";
			this.lnkLoadProject.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkLoadProject_LinkClicked);
			// 
			// label2
			// 
			this.label2.ForeColor = System.Drawing.Color.OrangeRed;
			this.label2.Location = new System.Drawing.Point(112, 16);
			this.label2.Name = "label2";
			this.label2.Size = new System.Drawing.Size(88, 16);
			this.label2.TabIndex = 9;
			this.label2.Text = "Current Project:";
			// 
			// lblCurrentProject
			// 
			this.lblCurrentProject.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lblCurrentProject.ForeColor = System.Drawing.Color.OrangeRed;
			this.lblCurrentProject.Location = new System.Drawing.Point(200, 16);
			this.lblCurrentProject.Name = "lblCurrentProject";
			this.lblCurrentProject.Size = new System.Drawing.Size(736, 16);
			this.lblCurrentProject.TabIndex = 10;
			this.lblCurrentProject.Text = "None";
			// 
			// grpScripting
			// 
			this.grpScripting.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.grpScripting.Controls.Add(this.ddDatabaseList);
			this.grpScripting.Controls.Add(this.label3);
			this.grpScripting.Controls.Add(this.chkReplaceDateAndId);
			this.grpScripting.Controls.Add(this.lnkSaveScripts);
			this.grpScripting.Controls.Add(this.lnkGenerateScripts);
			this.grpScripting.Controls.Add(this.tcTables);
			this.grpScripting.Controls.Add(this.lnkCheckAll);
			this.grpScripting.Controls.Add(this.label1);
			this.grpScripting.Controls.Add(this.lstTables);
			this.grpScripting.Enabled = false;
			this.grpScripting.Location = new System.Drawing.Point(12, 32);
			this.grpScripting.Name = "grpScripting";
			this.grpScripting.Size = new System.Drawing.Size(928, 552);
			this.grpScripting.TabIndex = 11;
			this.grpScripting.TabStop = false;
			// 
			// ddDatabaseList
			// 
			this.ddDatabaseList.ContextMenu = this.contextDatabase;
			this.ddDatabaseList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ddDatabaseList.Location = new System.Drawing.Point(8, 32);
			this.ddDatabaseList.Name = "ddDatabaseList";
			this.ddDatabaseList.Size = new System.Drawing.Size(176, 21);
			this.ddDatabaseList.TabIndex = 16;
			this.ddDatabaseList.SelectionChangeCommitted += new System.EventHandler(this.ddDatabaseList_SelectionChangeCommitted);
			// 
			// contextDatabase
			// 
			this.contextDatabase.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
																							this.mnuRemoveDatabase});
			// 
			// mnuRemoveDatabase
			// 
			this.mnuRemoveDatabase.Index = 0;
			this.mnuRemoveDatabase.Text = "Remove Database (and Tables)";
			this.mnuRemoveDatabase.Click += new System.EventHandler(this.mnuRemoveDatabase_Click);
			// 
			// label3
			// 
			this.label3.Location = new System.Drawing.Point(8, 16);
			this.label3.Name = "label3";
			this.label3.Size = new System.Drawing.Size(100, 16);
			this.label3.TabIndex = 15;
			this.label3.Text = "Select Database:";
			// 
			// chkReplaceDateAndId
			// 
			this.chkReplaceDateAndId.Checked = true;
			this.chkReplaceDateAndId.CheckState = System.Windows.Forms.CheckState.Checked;
			this.chkReplaceDateAndId.Location = new System.Drawing.Point(216, 16);
			this.chkReplaceDateAndId.Name = "chkReplaceDateAndId";
			this.chkReplaceDateAndId.Size = new System.Drawing.Size(168, 16);
			this.chkReplaceDateAndId.TabIndex = 14;
			this.chkReplaceDateAndId.Text = "Replace Update Date and Id";
			// 
			// lnkSaveScripts
			// 
			this.lnkSaveScripts.Location = new System.Drawing.Point(848, 16);
			this.lnkSaveScripts.Name = "lnkSaveScripts";
			this.lnkSaveScripts.Size = new System.Drawing.Size(72, 16);
			this.lnkSaveScripts.TabIndex = 13;
			this.lnkSaveScripts.TabStop = true;
			this.lnkSaveScripts.Text = "Save Scripts";
			this.lnkSaveScripts.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkSaveScripts_LinkClicked);
			// 
			// lnkGenerateScripts
			// 
			this.lnkGenerateScripts.Location = new System.Drawing.Point(392, 16);
			this.lnkGenerateScripts.Name = "lnkGenerateScripts";
			this.lnkGenerateScripts.Size = new System.Drawing.Size(224, 16);
			this.lnkGenerateScripts.TabIndex = 12;
			this.lnkGenerateScripts.TabStop = true;
			this.lnkGenerateScripts.Text = "Generate Insert Scripts for Checked Tables";
			this.lnkGenerateScripts.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkGenerateScripts_LinkClicked);
			// 
			// tcTables
			// 
			this.tcTables.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.tcTables.Location = new System.Drawing.Point(216, 40);
			this.tcTables.Multiline = true;
			this.tcTables.Name = "tcTables";
			this.tcTables.SelectedIndex = 0;
			this.tcTables.Size = new System.Drawing.Size(704, 504);
			this.tcTables.TabIndex = 11;
			this.tcTables.SelectedIndexChanged += new System.EventHandler(this.tcTables_SelectedIndexChanged);
			// 
			// lnkCheckAll
			// 
			this.lnkCheckAll.Location = new System.Drawing.Point(128, 64);
			this.lnkCheckAll.Name = "lnkCheckAll";
			this.lnkCheckAll.Size = new System.Drawing.Size(56, 16);
			this.lnkCheckAll.TabIndex = 10;
			this.lnkCheckAll.TabStop = true;
			this.lnkCheckAll.Text = "Check All";
			this.lnkCheckAll.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkCheckAll_LinkClicked);
			// 
			// label1
			// 
			this.label1.Location = new System.Drawing.Point(8, 64);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 16);
			this.label1.TabIndex = 9;
			this.label1.Text = "Tables to Script";
			// 
			// lstTables
			// 
			this.lstTables.Activation = System.Windows.Forms.ItemActivation.OneClick;
			this.lstTables.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left)));
			this.lstTables.CheckBoxes = true;
			this.lstTables.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						this.columnHeader1});
			this.lstTables.ContextMenu = this.contextMenu1;
			this.lstTables.FullRowSelect = true;
			this.lstTables.Location = new System.Drawing.Point(8, 80);
			this.lstTables.Name = "lstTables";
			this.lstTables.Size = new System.Drawing.Size(176, 464);
			this.lstTables.TabIndex = 8;
			this.lstTables.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Table Name";
			this.columnHeader1.Width = 171;
			// 
			// LookUpTableForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(952, 622);
			this.Controls.Add(this.grpScripting);
			this.Controls.Add(this.lblCurrentProject);
			this.Controls.Add(this.label2);
			this.Controls.Add(this.lnkLoadProject);
			this.Controls.Add(this.statusBar1);
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "LookUpTableForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Text = "Look Up Table Insert Scripting ::   Server: {0}";
			this.Load += new System.EventHandler(this.LookUpTable_Load);
			((System.ComponentModel.ISupportInitialize)(this.statStatus)).EndInit();
			this.grpScripting.ResumeLayout(false);
			this.ResumeLayout(false);

		}
		#endregion

		private void LookUpTable_Load(object sender, System.EventArgs e)
		{
			this.Text = String.Format(this.Text,new object[]{this.data.SQLServerName});
		}

		private void LoadLookUpTableData(string fileName)
		{
			bool successfulLoad = true;
			this.Cursor = Cursors.WaitCursor;
			try
			{
				string configFile = fileName; //Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location) +@"\LookUpTables.xml";
				this.tableList = new SQLSyncData();
				if(File.Exists(configFile))
				{
					//Read the table list
					try
					{
						this.tableList.ReadXml(configFile);
					}
					catch
					{
						MessageBox.Show("Unable to Read the selected file.\r\nIt is not a valid table scripting file","Invalid File",MessageBoxButtons.OK,MessageBoxIcon.Error);
						successfulLoad = false;
					}

				}
				else
				{
					this.tableList.Database.AddDatabaseRow(this.data.DatabaseName);
					this.tableList.LookUpTable.AddLookUpTableRow("","",false,(SQLSyncData.DatabaseRow)tableList.Database.Rows[0]);
					this.tableList.WriteXml(configFile);
				}
				if(successfulLoad)
				{
					this.projectFileName = configFile;
					lblCurrentProject.Text = configFile;
					grpScripting.Enabled = true;

					PopulateDatabaseList(this.tableList.Database);
				}
			}
			finally
			{
				this.Cursor = Cursors.Default;
			}
		}
		private void PopulateDatabaseList(SQLSyncData.DatabaseDataTable table)
		{
			string currentSelection = string.Empty;
			if(this.ddDatabaseList.Items.Count > 0 && this.ddDatabaseList.SelectedItem != null &&
				this.ddDatabaseList.SelectedItem.ToString() != DropDownConstants.AddNew)
			{
				currentSelection = this.ddDatabaseList.SelectedItem.ToString();
			}
			this.ddDatabaseList.Items.Clear();
			this.lstTables.Items.Clear();
			DataView view = table.DefaultView;
			view.Sort = table.NameColumn.ColumnName + " ASC";
			ddDatabaseList.Items.Add(DropDownConstants.SelectOne);
			this.ddDatabaseList.SelectedIndex = 0;
			for(int i=0;i<view.Count;i++)
			{
				ddDatabaseList.Items.Add(view[i].Row[table.NameColumn.ColumnName].ToString());
				if(currentSelection.Length > 0 && 
					currentSelection == view[i].Row[table.NameColumn.ColumnName].ToString())
				{
					ddDatabaseList.SelectedIndex = i+1;
				}
			}
			ddDatabaseList.Items.Add(DropDownConstants.AddNew);

			if(ddDatabaseList.SelectedIndex > 0)
			{
				this.PopulateTableList(ddDatabaseList.SelectedItem.ToString());
			}
			
		}
		private void PopulateTableList(string databaseName)
		{
			this.lstTables.Items.Clear();
			this.data.DatabaseName = databaseName;
			PopulateHelper helper = new PopulateHelper(this.data,null);

			DataRow[] dbRows =  this.tableList.Database.Select(tableList.Database.NameColumn.ColumnName + " = '"+this.data.DatabaseName+"'");
				
			if(dbRows.Length > 0)
			{
				SQLSyncData.LookUpTableRow[] lookUpRows =  ((SQLSyncData.DatabaseRow)dbRows[0]).GetLookUpTableRows();
				//Add the rows to a table so we can sort by Name

				SQLSyncData.LookUpTableDataTable lookTable = new SQLSync.SQLSyncData.LookUpTableDataTable();
				for(int i=0;i<lookUpRows.Length;i++)
				{
					lookTable.ImportRow(lookUpRows[i]);
				}

				DataView view = lookTable.DefaultView;
				view.Sort = tableList.LookUpTable.NameColumn.ColumnName +" ASC";
				for(int i=0;i<view.Count;i++)
				{
					SQLSyncData.LookUpTableRow row = (SQLSyncData.LookUpTableRow)view[i].Row;
					ListViewItem item = new ListViewItem(row.Name);
					if(helper.DbContainsTable(row.Name) == false)
					{
						item.ForeColor = Color.LightGray;
					}
					if(row.WhereClause.Length > 0)
					{
						item.BackColor = Color.LightBlue;
					}
					lstTables.Items.Add(item);

				}
			}
		}
		private void AddNewDatabase()
		{
			string[] listedDatabases = new string[this.ddDatabaseList.Items.Count -2];
			for(int i=1;i<this.ddDatabaseList.Items.Count-1;i++)
			{
				listedDatabases[i-1] = this.ddDatabaseList.Items[i].ToString();
			}
			NewLookUpDatabaseForm frmNewDb = new NewLookUpDatabaseForm(new PopulateHelper(this.data,null).RemainingDatabases(listedDatabases));
			DialogResult result = frmNewDb.ShowDialog();
			if(result == DialogResult.OK)
			{
				string[] newDatabases = frmNewDb.DatabaseList;
				SaveNewDatabaseNames(newDatabases);
			}
		}
		private void SaveNewDatabaseNames(string[] newDatabases)
		{
			for(int i=0;i<newDatabases.Length;i++)
			{
				this.tableList.Database.AddDatabaseRow(newDatabases[i]);
			}
			this.SaveXmlTemplate(this.projectFileName);
			this.LoadLookUpTableData(this.projectFileName);
		}

		private void SaveXmlTemplate(string fileName)
		{
	
			try
			{
				this.tableList.WriteXml(fileName);
			}
			catch(System.UnauthorizedAccessException)
			{
				MessageBox.Show("Unable to save project file to:\r\n"+fileName,"Unable to Save",MessageBoxButtons.OK,MessageBoxIcon.Error);
			}
		}

		private void RemoveDatabase(string databaseName)
		{
			DataRow[] deleteRows = this.tableList.Database.Select(this.tableList.Database.NameColumn.ColumnName +"='"+databaseName+"'");
			if(deleteRows.Length > 0)
			{
				SQLSyncData.DatabaseRow dbRow = (SQLSyncData.DatabaseRow)deleteRows[0];
				int childRowsCount = dbRow.GetLookUpTableRows().Length;
				if( childRowsCount > 0)
				{
					string entry = (childRowsCount > 1)? "entries":"entry";
					string message = "The Database "+dbRow.Name +" has "+childRowsCount.ToString()+" table "+entry+".\r\nAre you sure you want to remove it?";
					DialogResult result = MessageBox.Show(message,"Confirm Removal",MessageBoxButtons.YesNo,MessageBoxIcon.Question);
					if(result == DialogResult.No) return;
				}

				this.tableList.Database.RemoveDatabaseRow(dbRow);
				this.tableList.Database.AcceptChanges();
				this.SaveXmlTemplate(this.projectFileName);
				this.LoadLookUpTableData(this.projectFileName);
			}
		}
		private void RemoveTable(string tableName)
		{
			DataRow[] deleteRows =  this.tableList.LookUpTable.Select(this.tableList.LookUpTable.NameColumn.ColumnName + "='"+tableName+"'");
			if(deleteRows.Length > 0)
			{
				SQLSyncData.LookUpTableRow tblRow = (SQLSyncData.LookUpTableRow)deleteRows[0];
				if(tblRow.WhereClause.Length > 0)
				{
					string message = "The table "+tblRow.Name+" has a WHERE clause.\r\nAre you sure you want to remove it?";
					DialogResult result = MessageBox.Show(message,"Confirm Removal",MessageBoxButtons.YesNo,MessageBoxIcon.Question);
					if(result == DialogResult.No) return;
				}

				this.tableList.LookUpTable.RemoveLookUpTableRow(tblRow);
				this.tableList.LookUpTable.AcceptChanges();
				this.SaveXmlTemplate(this.projectFileName);
				this.LoadLookUpTableData(this.projectFileName);

			}
		}
		private void AddNewTables(string[] tables)
		{
			DataRow[] rows =  tableList.Database.Select(tableList.Database.NameColumn.ColumnName +" ='"+this.data.DatabaseName +"'");
			if(rows.Length ==  0)
			{
				tableList.Database.AddDatabaseRow(this.data.DatabaseName);
				rows =  tableList.Database.Select(tableList.Database.NameColumn.ColumnName +" ='"+this.data.DatabaseName +"'");
			}

			for(int  i=0;i<tables.Length;i++)
			{
				this.tableList.LookUpTable.AddLookUpTableRow(tables[i],"",false,(SQLSyncData.DatabaseRow)rows[0]);
			}
			this.SaveXmlTemplate(this.projectFileName);
			this.LoadLookUpTableData(this.projectFileName);
		}
		private void lnkCheckAll_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			for(int i=0;i<lstTables.Items.Count;i++)
			{
				lstTables.Items[i].Checked = true;
			}
		}

		private void lnkGenerateScripts_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			this.Cursor = Cursors.WaitCursor;
			try
			{
				//Clear out any old controls
				tcTables.Controls.Clear();

				//Get the list of selected tables
				ArrayList list = new ArrayList();
				for(int i=0;i<lstTables.Items.Count;i++)
				{
					if(lstTables.Items[i].Checked && lstTables.Items[i].ForeColor != Color.LightGray)
					{
						list.Add(lstTables.Items[i].Text);
					}
				}
				string[] tableList = new string[list.Count];
				list.CopyTo(tableList);

				//Run the scripting
				PopulateHelper helper = new PopulateHelper(this.data,tableList);
				helper.SyncData = this.tableList;
				helper.ReplaceDateAndId = chkReplaceDateAndId.Checked;
			
				TableScriptData[] scriptedTables =  helper.GeneratePopulateScripts();
			
				//Add a tab page per generated script
				for(int i=0;i<scriptedTables.Length;i++)
				{
					TabPage page = new TabPage();
					page.Text = scriptedTables[i].TableName;
					PopulateScriptDisplay disp = new PopulateScriptDisplay(scriptedTables[i]);
					disp.ScriptText = scriptedTables[i].InsertScript;
					disp.ScriptName = scriptedTables[i].TableName;
					disp.ScriptDataTable = scriptedTables[i].ValuesTable;
					disp.Size = page.Size;
					disp.Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
					page.Controls.Add(disp);
					tcTables.Controls.Add(page);

				}
				this.Size = new System.Drawing.Size(this.Width+1,this.Height);
			}
			finally
			{
				this.Cursor = Cursors.Default;
			}
		}

		private void lnkSaveScripts_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			DialogResult result = folderBrowserDialog1.ShowDialog();
			if(result == DialogResult.OK)
			{
				this.Cursor = Cursors.WaitCursor;
				try
				{
					statStatus.Text = "Saving Scripts";
					this.Cursor = Cursors.WaitCursor;

					foreach(TabPage page in tcTables.TabPages)
					{
						foreach(Control ctrl in page.Controls)
						{
							if(ctrl.GetType() == typeof(PopulateScriptDisplay))
							{

								statStatus.Text = "Saving:"+ ((PopulateScriptDisplay)ctrl).ScriptName;
								((PopulateScriptDisplay)ctrl).SaveScript(folderBrowserDialog1.SelectedPath);
								break;
							}
						}
					}
				}
				finally
				{
					this.Cursor = Cursors.WaitCursor;
				}
			
			}
			statStatus.Text = "Save complete. Ready.";

			this.Cursor = Cursors.Default;
		}

		private void mnuAddTable_Click(object sender, System.EventArgs e)
		{
			string[] listedTables = new string[this.lstTables.Items.Count];
			for(int i=0;i<this.lstTables.Items.Count;i++)
			{
				listedTables[i] = this.lstTables.Items[i].Text;
			}

			NewLookUpForm frmNew = new NewLookUpForm( new PopulateHelper(this.data,null).RemainingTables(listedTables));
			DialogResult result =  frmNew.ShowDialog();
			if(result == DialogResult.OK)
			{
				this.AddNewTables(frmNew.TableList);
			}
		}

		private void mnuDeleteTable_Click(object sender, System.EventArgs e)
		{
			if(lstTables.SelectedItems.Count > 0)
			{
				ListViewItem item = lstTables.SelectedItems[0];
				this.RemoveTable(item.Text);
			}
		}

		private void mnuWhereClause_Click(object sender, System.EventArgs e)
		{
			if(lstTables.SelectedItems.Count > 0)
			{
				ListViewItem item = lstTables.SelectedItems[0];
				SQLSyncData.LookUpTableRow lookUpRow;
				DataRow[] rows =  tableList.Database.Select(tableList.Database.NameColumn.ColumnName +" ='"+this.data.DatabaseName +"'");
				if(rows.Length > 0)
				{
					DataRow[] lookUpRows = ((SQLSyncData.DatabaseRow)rows[0]).GetLookUpTableRows();
					for(int i=0;i<lookUpRows.Length;i++)
					{
						SQLSyncData.DatabaseRow dbRow = (SQLSyncData.DatabaseRow)rows[0];
						if(((SQLSyncData.LookUpTableRow)lookUpRows[i]).Name.ToUpper() == item.Text.ToUpper())
						{
							lookUpRow = (SQLSyncData.LookUpTableRow)lookUpRows[i];

							WhereClauseForm frmWhere = new WhereClauseForm(this.data, item.Text,lookUpRow.WhereClause,lookUpRow.UseAsFullSelect);
							DialogResult result = frmWhere.ShowDialog();
							if(result == DialogResult.OK)
							{
								lookUpRow.WhereClause = frmWhere.WhereClause;
								lookUpRow.UseAsFullSelect = frmWhere.UseAsFullSelect;
								lookUpRow.AcceptChanges();
								this.SaveXmlTemplate(this.projectFileName);
								break;
							}
						}
					}
				}
			}
		}
		private void lnkLoadProject_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			DialogResult result = openFileDialog1.ShowDialog();
			if(result == DialogResult.OK)
			{
				this.projectFileName = openFileDialog1.FileName;
				this.LoadLookUpTableData(this.projectFileName);
			}
		}

		private void ddDatabaseList_SelectionChangeCommitted(object sender, System.EventArgs e)
		{
			this.Cursor = Cursors.WaitCursor;
			try
			{
				switch(ddDatabaseList.SelectedItem.ToString())
				{
					case DropDownConstants.SelectOne:
						this.lstTables.Items.Clear();
						break;
					case DropDownConstants.AddNew:
						this.AddNewDatabase();
						break;
					default:
						this.PopulateTableList(ddDatabaseList.Text);
						break;
				}
			}
			finally
			{
				this.Cursor = Cursors.Default;
			}
			
		}

		private void mnuRemoveDatabase_Click(object sender, System.EventArgs e)
		{
			this.RemoveDatabase(this.ddDatabaseList.SelectedItem.ToString());
		}

		private void tcTables_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			this.Size = new System.Drawing.Size(this.Width+1,this.Height);
			this.Size = new System.Drawing.Size(this.Width-1,this.Height);

		}

		
	}
}
