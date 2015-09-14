using System;
using System.Drawing;
using System.Collections;
using System.IO;
using System.ComponentModel;
using System.Windows.Forms;
using SqlSync.TableScript;
using SqlSync.DbInformation;
namespace SqlSync.DataDump
{
	/// <summary>
	/// Summary description for DataDumpForm.
	/// </summary>
	public class DataDumpForm : System.Windows.Forms.Form
	{
		private SqlSync.ColumnSorter tableSorter = new ColumnSorter();
		private SqlSync.ColumnSorter resultSorter = new ColumnSorter();
		private System.Windows.Forms.LinkLabel lnkCheckAll;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ListView lstTables;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.MenuItem menuItem2;
		private System.Windows.Forms.ComboBox ddDatabaseList;
		private Connection.ConnectionData connData;
		private System.Windows.Forms.LinkLabel lnkExtractData;
		private System.Windows.Forms.StatusBar statusBar1;
		private System.Windows.Forms.StatusBarPanel statStatus;
        private System.Windows.Forms.ColumnHeader columnHeader2;
		private System.Windows.Forms.MenuItem mnuChangeFolder;
		private System.Windows.Forms.MenuItem menuItem1;
		private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		private System.Windows.Forms.ColumnHeader columnHeader4;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ListView lstResults;
		private System.Windows.Forms.ColumnHeader columnHeader5;
		private System.Windows.Forms.ContextMenu contextMenu1;
        private System.Windows.Forms.MenuItem mnuOpen;
        private SettingsControl settingsControl1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripMenuItem changeSqlServerConnectionToolStripMenuItem;
        private ToolStripMenuItem changeDestinationFolderToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem2;
        //private IContainer components;

		public DataDumpForm(Connection.ConnectionData connData)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			this.connData = connData;
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
            //if( disposing )
            //{
            //    if(components != null)
            //    {
            //        components.Dispose();
            //    }
            //}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataDumpForm));
            this.lnkCheckAll = new System.Windows.Forms.LinkLabel();
            this.label1 = new System.Windows.Forms.Label();
            this.lstTables = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.menuItem2 = new System.Windows.Forms.MenuItem();
            this.mnuChangeFolder = new System.Windows.Forms.MenuItem();
            this.menuItem1 = new System.Windows.Forms.MenuItem();
            this.ddDatabaseList = new System.Windows.Forms.ComboBox();
            this.lnkExtractData = new System.Windows.Forms.LinkLabel();
            this.statusBar1 = new System.Windows.Forms.StatusBar();
            this.statStatus = new System.Windows.Forms.StatusBarPanel();
            this.folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            this.label2 = new System.Windows.Forms.Label();
            this.lstResults = new System.Windows.Forms.ListView();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader5 = new System.Windows.Forms.ColumnHeader();
            this.contextMenu1 = new System.Windows.Forms.ContextMenu();
            this.mnuOpen = new System.Windows.Forms.MenuItem();
            this.label3 = new System.Windows.Forms.Label();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.changeDestinationFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeSqlServerConnectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsControl1 = new SqlSync.SettingsControl();
            this.toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.statStatus)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lnkCheckAll
            // 
            this.lnkCheckAll.Location = new System.Drawing.Point(240, 129);
            this.lnkCheckAll.Name = "lnkCheckAll";
            this.lnkCheckAll.Size = new System.Drawing.Size(104, 16);
            this.lnkCheckAll.TabIndex = 10;
            this.lnkCheckAll.TabStop = true;
            this.lnkCheckAll.Text = "Check All";
            this.lnkCheckAll.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.lnkCheckAll.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkCheckAll_LinkClicked);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(16, 129);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(112, 16);
            this.label1.TabIndex = 12;
            this.label1.Text = "Tables to Script:";
            // 
            // lstTables
            // 
            this.lstTables.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.lstTables.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.lstTables.CheckBoxes = true;
            this.lstTables.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.lstTables.FullRowSelect = true;
            this.lstTables.GridLines = true;
            this.lstTables.Location = new System.Drawing.Point(16, 147);
            this.lstTables.Name = "lstTables";
            this.lstTables.Size = new System.Drawing.Size(328, 226);
            this.lstTables.TabIndex = 11;
            this.lstTables.UseCompatibleStateImageBehavior = false;
            this.lstTables.View = System.Windows.Forms.View.Details;
            this.lstTables.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lstTables_ColumnClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Table Name";
            this.columnHeader1.Width = 229;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Row Count";
            this.columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader2.Width = 78;
            // 
            // menuItem2
            // 
            this.menuItem2.Index = -1;
            this.menuItem2.Text = "";
            // 
            // mnuChangeFolder
            // 
            this.mnuChangeFolder.Index = -1;
            this.mnuChangeFolder.Text = "";
            // 
            // menuItem1
            // 
            this.menuItem1.Index = -1;
            this.menuItem1.Text = "";
            // 
            // ddDatabaseList
            // 
            this.ddDatabaseList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddDatabaseList.Location = new System.Drawing.Point(16, 101);
            this.ddDatabaseList.Name = "ddDatabaseList";
            this.ddDatabaseList.Size = new System.Drawing.Size(176, 21);
            this.ddDatabaseList.TabIndex = 13;
            this.ddDatabaseList.SelectedIndexChanged += new System.EventHandler(this.ddDatabaseList_SelectedIndexChanged);
            // 
            // lnkExtractData
            // 
            this.lnkExtractData.Enabled = false;
            this.lnkExtractData.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lnkExtractData.Location = new System.Drawing.Point(384, 113);
            this.lnkExtractData.Name = "lnkExtractData";
            this.lnkExtractData.Size = new System.Drawing.Size(100, 16);
            this.lnkExtractData.TabIndex = 14;
            this.lnkExtractData.TabStop = true;
            this.lnkExtractData.Text = "Extract Data";
            this.lnkExtractData.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkExtractData_LinkClicked);
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(0, 389);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
            this.statStatus});
            this.statusBar1.ShowPanels = true;
            this.statusBar1.Size = new System.Drawing.Size(744, 22);
            this.statusBar1.TabIndex = 15;
            this.statusBar1.Text = "statusBar1";
            // 
            // statStatus
            // 
            this.statStatus.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
            this.statStatus.Name = "statStatus";
            this.statStatus.Width = 727;
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(16, 85);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(112, 16);
            this.label2.TabIndex = 17;
            this.label2.Text = "Select Database:";
            // 
            // lstResults
            // 
            this.lstResults.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.lstResults.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.lstResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3,
            this.columnHeader4,
            this.columnHeader5});
            this.lstResults.ContextMenu = this.contextMenu1;
            this.lstResults.FullRowSelect = true;
            this.lstResults.GridLines = true;
            this.lstResults.Location = new System.Drawing.Point(384, 147);
            this.lstResults.Name = "lstResults";
            this.lstResults.Size = new System.Drawing.Size(328, 226);
            this.lstResults.TabIndex = 18;
            this.lstResults.UseCompatibleStateImageBehavior = false;
            this.lstResults.View = System.Windows.Forms.View.Details;
            this.lstResults.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lstResults_ColumnClick);
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "File Name";
            this.columnHeader3.Width = 229;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Size (Kb)";
            this.columnHeader4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            this.columnHeader4.Width = 78;
            // 
            // columnHeader5
            // 
            this.columnHeader5.Text = "Full Path";
            this.columnHeader5.Width = 0;
            // 
            // contextMenu1
            // 
            this.contextMenu1.MenuItems.AddRange(new System.Windows.Forms.MenuItem[] {
            this.mnuOpen});
            // 
            // mnuOpen
            // 
            this.mnuOpen.Index = 0;
            this.mnuOpen.Text = "Open File";
            this.mnuOpen.Click += new System.EventHandler(this.mnuOpen_Click);
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(384, 129);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(112, 16);
            this.label3.TabIndex = 19;
            this.label3.Text = "Extract Results:";
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripMenuItem1,
            this.toolStripMenuItem2});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(744, 24);
            this.menuStrip1.TabIndex = 20;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.changeDestinationFolderToolStripMenuItem,
            this.changeSqlServerConnectionToolStripMenuItem});
            this.toolStripMenuItem1.Image = global::SqlSync.Properties.Resources.Execute;
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(65, 20);
            this.toolStripMenuItem1.Text = "Action";
            // 
            // changeDestinationFolderToolStripMenuItem
            // 
            this.changeDestinationFolderToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Open;
            this.changeDestinationFolderToolStripMenuItem.Name = "changeDestinationFolderToolStripMenuItem";
            this.changeDestinationFolderToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
            this.changeDestinationFolderToolStripMenuItem.Text = "Change &Destination Folder";
            this.changeDestinationFolderToolStripMenuItem.Click += new System.EventHandler(this.mnuChangeFolder_Click);
            // 
            // changeSqlServerConnectionToolStripMenuItem
            // 
            this.changeSqlServerConnectionToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Server1;
            this.changeSqlServerConnectionToolStripMenuItem.Name = "changeSqlServerConnectionToolStripMenuItem";
            this.changeSqlServerConnectionToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
            this.changeSqlServerConnectionToolStripMenuItem.Text = "Change Sql Server Connection";
            this.changeSqlServerConnectionToolStripMenuItem.Click += new System.EventHandler(this.mnuChangeSqlServer_Click);
            // 
            // settingsControl1
            // 
            this.settingsControl1.BackColor = System.Drawing.Color.White;
            this.settingsControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.settingsControl1.Location = new System.Drawing.Point(0, 24);
            this.settingsControl1.Name = "settingsControl1";
            this.settingsControl1.Project = "(select destination)";
            this.settingsControl1.ProjectLabelText = "Destination Folder:";
            this.settingsControl1.Server = "";
            this.settingsControl1.Size = new System.Drawing.Size(744, 56);
            this.settingsControl1.TabIndex = 16;
            this.settingsControl1.Load += new System.EventHandler(this.settingsControl1_Load);
            this.settingsControl1.ServerChanged += new SqlSync.ServerChangedEventHandler(this.settingsControl1_ServerChanged);
            this.settingsControl1.DoubleClick += new System.EventHandler(this.settingsControl1_Click);
            this.settingsControl1.Click += new System.EventHandler(this.settingsControl1_Click);
            // 
            // toolStripMenuItem2
            // 
            this.toolStripMenuItem2.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripMenuItem2.Image = global::SqlSync.Properties.Resources.Help_2;
            this.toolStripMenuItem2.Name = "toolStripMenuItem2";
            this.toolStripMenuItem2.Size = new System.Drawing.Size(28, 20);
            this.toolStripMenuItem2.Click += new System.EventHandler(this.toolStripMenuItem2_Click);
            // 
            // DataDumpForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(744, 411);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lstResults);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.settingsControl1);
            this.Controls.Add(this.statusBar1);
            this.Controls.Add(this.lnkExtractData);
            this.Controls.Add(this.ddDatabaseList);
            this.Controls.Add(this.lnkCheckAll);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.lstTables);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "DataDumpForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sql Build Manager :: Data Extraction";
            this.Load += new System.EventHandler(this.DataDumpForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.statStatus)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void mnuLoadProjectFile_Click(object sender, System.EventArgs e)
		{
		    		}

		private void DataDumpForm_Load(object sender, System.EventArgs e)
		{
			if(this.connData != null)
			{
				GetDatabaseList();
				this.settingsControl1.Server = this.connData.SQLServerName;
			}
		}

		private void GetDatabaseList()
		{
			this.Cursor = Cursors.WaitCursor;
			try
			{
				this.statStatus.Text = "Retrieving Database list from "+this.connData.SQLServerName;
				this.ddDatabaseList.Items.Clear();
				DatabaseList dbs = SqlSync.DbInformation.InfoHelper.GetDatabaseList(this.connData);
				for(int i=0;i<dbs.Count;i++)
                    this.ddDatabaseList.Items.Add(dbs[i].DatabaseName);

				this.statStatus.Text = "Ready";
			}
			finally
			{
				this.Cursor = Cursors.Default;
			}
		}

		private void ddDatabaseList_SelectedIndexChanged(object sender, System.EventArgs e)
		{
			this.Cursor = Cursors.AppStarting;
            this.lstTables.Items.Clear();
			try
			{
				if(this.ddDatabaseList.SelectedItem == null)
					return;

				this.statStatus.Text = "Enumerating tables and row count";
				string database = this.ddDatabaseList.SelectedItem.ToString();
				this.connData.DatabaseName = database;
				TableSize[] tables = SqlSync.DbInformation.InfoHelper.GetDatabaseTableListWithRowCount(this.connData);
				this.lstTables.Items.Clear();
				for(int i=0;i<tables.Length;i++)
				{
					this.lstTables.Items.Add(new ListViewItem(new string[]{tables[i].TableName,tables[i].RowCount.ToString()}));
				}
				this.statStatus.Text = "Ready";
			}
			finally
			{
				this.Cursor = Cursors.Default;
			}
		}

		private void lnkCheckAll_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			if(lnkCheckAll.Text == "Check All")
			{
				lnkCheckAll.Text = "Uncheck All";
				for(int i=0;i<this.lstTables.Items.Count;i++)
					this.lstTables.Items[i].Checked = true;
			}
			else
			{
				lnkCheckAll.Text = "Check All";
				while(this.lstTables.CheckedItems.Count > 0)
					this.lstTables.CheckedItems[0].Checked = false;
			}
		}

		private void lnkExtractData_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			this.lstResults.Items.Clear();
			string[] selectedTables = new string[this.lstTables.CheckedItems.Count];
			for(int i=0;i<this.lstTables.CheckedItems.Count;i++)
				selectedTables[i] = this.lstTables.CheckedItems[i].Text;

			DataDumpHelper helper = new DataDumpHelper(this.connData,selectedTables,this.settingsControl1.Project);
			helper.ProcessingTableData +=new SqlSync.TableScript.DataDumpHelper.ProcessingTableDataEventHandler(helper_ProcessingTableData);
			helper.FileWritten +=new SqlSync.TableScript.DataDumpHelper.FileWrittenEventHandler(helper_FileWritten);
			helper.ExtractAndWriteData();
		}

	

		private void mnuChangeFolder_Click(object sender, System.EventArgs e)
		{
			if(DialogResult.OK == folderBrowserDialog1.ShowDialog())
			{
				this.settingsControl1.Project = folderBrowserDialog1.SelectedPath;
				this.lnkExtractData.Enabled = true;

			}
		}

		private void settingsControl1_Click(object sender, System.EventArgs e)
		{
			this.mnuChangeFolder_Click(sender,e);
		}

		private void mnuChangeSqlServer_Click(object sender, System.EventArgs e)
		{
			ConnectionForm frmConnect = new ConnectionForm("Sql Data Extraction");
			DialogResult result = frmConnect.ShowDialog();
			if(result == DialogResult.OK)
			{
				this.connData = frmConnect.SqlConnection;
				this.settingsControl1.Server = this.connData.SQLServerName;
				GetDatabaseList();
			}
		}

		private void helper_FileWritten(object sender, FileWrittenEventArgs e)
		{
			ListViewItem item = new ListViewItem(new string[]{
																 Path.GetFileName(e.FileName),
																 (e.Size/1000).ToString(),
																 e.FileName});
			//this.lstResults.Items.Insert(0,item);
			this.lstResults.Items.Add(item);
		}

		private void helper_ProcessingTableData(object sender, ProcessingTableDataEventArgs e)
		{
			if(e.ProcessingComplete == false)
				this.statStatus.Text = e.TableName +" :: "+e.Message;
			else
				this.statStatus.Text = "Extraction Complete";
		}
		private void mnuOpen_Click(object sender, System.EventArgs e)
		{
			if(lstResults.SelectedItems.Count > 0)
			{
				this.Cursor = Cursors.WaitCursor;
				string file = lstResults.SelectedItems[0].SubItems[2].Text;
				System.Diagnostics.Process prc = new System.Diagnostics.Process();
				prc.StartInfo.FileName = "notepad.exe";
				prc.StartInfo.Arguments = file;
				prc.Start();
				this.Cursor = Cursors.Default;
			}
		}

		private void settingsControl1_Load(object sender, System.EventArgs e)
		{
		
		}

		private void lstTables_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
		{
			tableSorter.CurrentColumn = e.Column;
			lstTables.ListViewItemSorter = tableSorter;
			lstTables.Sort();
		}

		private void lstResults_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
		{
			resultSorter.CurrentColumn = e.Column;
			lstResults.ListViewItemSorter = resultSorter;
			lstResults.Sort();
		}

        private void settingsControl1_ServerChanged(object sender, string serverName, string username, string password)
        {
            Connection.ConnectionData oldConnData = new Connection.ConnectionData();
            this.connData.Fill(oldConnData);
            this.Cursor = Cursors.WaitCursor;

            this.connData.SQLServerName = serverName;
            if (!string.IsNullOrWhiteSpace(username) && (!string.IsNullOrWhiteSpace(password)))
            {
                this.connData.UserId = username;
                this.connData.Password = password;
                this.connData.UseWindowAuthentication = false;
            }
            else
            {
                this.connData.UseWindowAuthentication = true;
            }
            this.connData.ScriptTimeout = 5;
            try
            {
                GetDatabaseList();
                this.lstTables.Items.Clear();
                //this.ddDatabaseList = SqlSync.DbInformation.InfoHelper.GetDatabaseList(this.connData);
            }
            catch
            {
                MessageBox.Show("Error retrieving database list. Is the server running?", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.connData = oldConnData;
                this.settingsControl1.Server = oldConnData.SQLServerName;
            }


            this.Cursor = Cursors.Default;
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            SqlSync.Utility.OpenManual("DataExtractionandInsertion");
        }
	}
}
