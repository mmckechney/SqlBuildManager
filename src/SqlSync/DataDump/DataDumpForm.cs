using SqlSync.Connection;
using SqlSync.DbInformation;
using SqlSync.TableScript;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;

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
        private System.Windows.Forms.ToolStripMenuItem menuItem2;
        private System.Windows.Forms.ComboBox ddDatabaseList;
        private Connection.ConnectionData connData;
        private System.Windows.Forms.LinkLabel lnkExtractData;
        private System.Windows.Forms.StatusStrip statusBar1;
        private System.Windows.Forms.ToolStripStatusLabel statStatus;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ToolStripMenuItem mnuChangeFolder;
        private System.Windows.Forms.ToolStripMenuItem menuItem1;
        private System.Windows.Forms.FolderBrowserDialog folderBrowserDialog1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListView lstResults;
        private System.Windows.Forms.ColumnHeader columnHeader5;
        private System.Windows.Forms.ContextMenuStrip contextMenu1;
        private System.Windows.Forms.ToolStripMenuItem mnuOpen;
        private SettingsControl settingsControl1;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem toolStripMenuItem1;
        private ToolStripMenuItem changeSqlServerConnectionToolStripMenuItem;
        private ToolStripMenuItem changeDestinationFolderToolStripMenuItem;
        private IContainer components;
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
        protected override void Dispose(bool disposing)
        {
            //if( disposing )
            //{
            //    if(components != null)
            //    {
            //        components.Dispose();
            //    }
            //}
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(DataDumpForm));
            lnkCheckAll = new System.Windows.Forms.LinkLabel();
            label1 = new System.Windows.Forms.Label();
            lstTables = new System.Windows.Forms.ListView();
            columnHeader1 = new System.Windows.Forms.ColumnHeader();
            columnHeader2 = new System.Windows.Forms.ColumnHeader();
            menuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            mnuChangeFolder = new System.Windows.Forms.ToolStripMenuItem();
            menuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            ddDatabaseList = new System.Windows.Forms.ComboBox();
            lnkExtractData = new System.Windows.Forms.LinkLabel();
            statusBar1 = new System.Windows.Forms.StatusStrip();
            statStatus = new System.Windows.Forms.ToolStripStatusLabel();
            folderBrowserDialog1 = new System.Windows.Forms.FolderBrowserDialog();
            label2 = new System.Windows.Forms.Label();
            lstResults = new System.Windows.Forms.ListView();
            columnHeader3 = new System.Windows.Forms.ColumnHeader();
            columnHeader4 = new System.Windows.Forms.ColumnHeader();
            columnHeader5 = new System.Windows.Forms.ColumnHeader();
            contextMenu1 = new System.Windows.Forms.ContextMenuStrip(components);
            mnuOpen = new System.Windows.Forms.ToolStripMenuItem();
            label3 = new System.Windows.Forms.Label();
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            changeDestinationFolderToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            changeSqlServerConnectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripMenuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            settingsControl1 = new SqlSync.SettingsControl();
            statusBar1.SuspendLayout();
            contextMenu1.SuspendLayout();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // lnkCheckAll
            // 
            lnkCheckAll.Location = new System.Drawing.Point(240, 129);
            lnkCheckAll.Name = "lnkCheckAll";
            lnkCheckAll.Size = new System.Drawing.Size(104, 16);
            lnkCheckAll.TabIndex = 10;
            lnkCheckAll.TabStop = true;
            lnkCheckAll.Text = "Check All";
            lnkCheckAll.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            lnkCheckAll.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(lnkCheckAll_LinkClicked);
            // 
            // label1
            // 
            label1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label1.Location = new System.Drawing.Point(16, 129);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(112, 16);
            label1.TabIndex = 12;
            label1.Text = "Tables to Script:";
            // 
            // lstTables
            // 
            lstTables.Activation = System.Windows.Forms.ItemActivation.OneClick;
            lstTables.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            lstTables.CheckBoxes = true;
            lstTables.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader1,
            columnHeader2});
            lstTables.FullRowSelect = true;
            lstTables.GridLines = true;
            lstTables.HideSelection = false;
            lstTables.Location = new System.Drawing.Point(16, 147);
            lstTables.Name = "lstTables";
            lstTables.Size = new System.Drawing.Size(328, 226);
            lstTables.TabIndex = 11;
            lstTables.UseCompatibleStateImageBehavior = false;
            lstTables.View = System.Windows.Forms.View.Details;
            lstTables.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(lstTables_ColumnClick);
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Table Name";
            columnHeader1.Width = 229;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Row Count";
            columnHeader2.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            columnHeader2.Width = 78;
            // 
            // menuItem2
            // 
            menuItem2.Name = "menuItem2";
            menuItem2.Size = new System.Drawing.Size(32, 19);
            // 
            // mnuChangeFolder
            // 
            mnuChangeFolder.Name = "mnuChangeFolder";
            mnuChangeFolder.Size = new System.Drawing.Size(32, 19);
            // 
            // menuItem1
            // 
            menuItem1.Name = "menuItem1";
            menuItem1.Size = new System.Drawing.Size(32, 19);
            // 
            // ddDatabaseList
            // 
            ddDatabaseList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            ddDatabaseList.Location = new System.Drawing.Point(16, 101);
            ddDatabaseList.Name = "ddDatabaseList";
            ddDatabaseList.Size = new System.Drawing.Size(176, 21);
            ddDatabaseList.TabIndex = 13;
            ddDatabaseList.SelectedIndexChanged += new System.EventHandler(ddDatabaseList_SelectedIndexChanged);
            // 
            // lnkExtractData
            // 
            lnkExtractData.Enabled = false;
            lnkExtractData.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lnkExtractData.Location = new System.Drawing.Point(384, 113);
            lnkExtractData.Name = "lnkExtractData";
            lnkExtractData.Size = new System.Drawing.Size(100, 16);
            lnkExtractData.TabIndex = 14;
            lnkExtractData.TabStop = true;
            lnkExtractData.Text = "Extract Data";
            lnkExtractData.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(lnkExtractData_LinkClicked);
            // 
            // statusBar1
            // 
            statusBar1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            statStatus});
            statusBar1.Location = new System.Drawing.Point(0, 389);
            statusBar1.Name = "statusBar1";
            statusBar1.Size = new System.Drawing.Size(744, 22);
            statusBar1.TabIndex = 15;
            statusBar1.Text = "statusBar1";
            // 
            // statStatus
            // 
            statStatus.Name = "statStatus";
            statStatus.Size = new System.Drawing.Size(729, 17);
            statStatus.Spring = true;
            // 
            // label2
            // 
            label2.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label2.Location = new System.Drawing.Point(16, 85);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(112, 16);
            label2.TabIndex = 17;
            label2.Text = "Select Database:";
            // 
            // lstResults
            // 
            lstResults.Activation = System.Windows.Forms.ItemActivation.OneClick;
            lstResults.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            lstResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader3,
            columnHeader4,
            columnHeader5});
            lstResults.ContextMenuStrip = contextMenu1;
            lstResults.FullRowSelect = true;
            lstResults.GridLines = true;
            lstResults.HideSelection = false;
            lstResults.Location = new System.Drawing.Point(384, 147);
            lstResults.Name = "lstResults";
            lstResults.Size = new System.Drawing.Size(328, 226);
            lstResults.TabIndex = 18;
            lstResults.UseCompatibleStateImageBehavior = false;
            lstResults.View = System.Windows.Forms.View.Details;
            lstResults.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(lstResults_ColumnClick);
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "File Name";
            columnHeader3.Width = 229;
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "Size (Kb)";
            columnHeader4.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
            columnHeader4.Width = 78;
            // 
            // columnHeader5
            // 
            columnHeader5.Text = "Full Path";
            columnHeader5.Width = 0;
            // 
            // contextMenu1
            // 
            contextMenu1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            mnuOpen});
            contextMenu1.Name = "contextMenu1";
            contextMenu1.Size = new System.Drawing.Size(125, 26);
            // 
            // mnuOpen
            // 
            mnuOpen.Name = "mnuOpen";
            mnuOpen.Size = new System.Drawing.Size(124, 22);
            mnuOpen.Text = "Open File";
            mnuOpen.Click += new System.EventHandler(mnuOpen_Click);
            // 
            // label3
            // 
            label3.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label3.Location = new System.Drawing.Point(384, 129);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(112, 16);
            label3.TabIndex = 19;
            label3.Text = "Extract Results:";
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            toolStripMenuItem1,
            toolStripMenuItem2});
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(744, 24);
            menuStrip1.TabIndex = 20;
            menuStrip1.Text = "menuStrip1";
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            changeDestinationFolderToolStripMenuItem,
            changeSqlServerConnectionToolStripMenuItem});
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new System.Drawing.Size(54, 20);
            toolStripMenuItem1.Text = "Action";
            // 
            // changeDestinationFolderToolStripMenuItem
            // 
            changeDestinationFolderToolStripMenuItem.Name = "changeDestinationFolderToolStripMenuItem";
            changeDestinationFolderToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
            changeDestinationFolderToolStripMenuItem.Text = "Change &Destination Folder";
            changeDestinationFolderToolStripMenuItem.Click += new System.EventHandler(mnuChangeFolder_Click);
            // 
            // changeSqlServerConnectionToolStripMenuItem
            // 
            changeSqlServerConnectionToolStripMenuItem.Name = "changeSqlServerConnectionToolStripMenuItem";
            changeSqlServerConnectionToolStripMenuItem.Size = new System.Drawing.Size(234, 22);
            changeSqlServerConnectionToolStripMenuItem.Text = "Change Sql Server Connection";
            changeSqlServerConnectionToolStripMenuItem.Click += new System.EventHandler(mnuChangeSqlServer_Click);
            // 
            // toolStripMenuItem2
            // 
            toolStripMenuItem2.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            toolStripMenuItem2.Name = "toolStripMenuItem2";
            toolStripMenuItem2.Size = new System.Drawing.Size(12, 20);
            toolStripMenuItem2.Click += new System.EventHandler(toolStripMenuItem2_Click);
            // 
            // settingsControl1
            // 
            settingsControl1.BackColor = System.Drawing.Color.White;
            settingsControl1.Dock = System.Windows.Forms.DockStyle.Top;
            settingsControl1.Location = new System.Drawing.Point(0, 24);
            settingsControl1.Name = "settingsControl1";
            settingsControl1.Project = "(select destination)";
            settingsControl1.ProjectLabelText = "Destination Folder:";
            settingsControl1.Server = "";
            settingsControl1.Size = new System.Drawing.Size(744, 56);
            settingsControl1.TabIndex = 16;
            settingsControl1.Click += new System.EventHandler(settingsControl1_Click);
            settingsControl1.DoubleClick += new System.EventHandler(settingsControl1_Click);
            settingsControl1.ServerChanged += new SqlSync.ServerChangedEventHandler(settingsControl1_ServerChanged);
            settingsControl1.Load += new System.EventHandler(settingsControl1_Load);
            // 
            // DataDumpForm
            // 
            AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            ClientSize = new System.Drawing.Size(744, 411);
            Controls.Add(label3);
            Controls.Add(lstResults);
            Controls.Add(label2);
            Controls.Add(settingsControl1);
            Controls.Add(statusBar1);
            Controls.Add(lnkExtractData);
            Controls.Add(ddDatabaseList);
            Controls.Add(lnkCheckAll);
            Controls.Add(label1);
            Controls.Add(lstTables);
            Controls.Add(menuStrip1);
            Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            MainMenuStrip = menuStrip1;
            Name = "DataDumpForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Sql Build Manager :: Data Extraction";
            Load += new System.EventHandler(DataDumpForm_Load);
            statusBar1.ResumeLayout(false);
            statusBar1.PerformLayout();
            contextMenu1.ResumeLayout(false);
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }
        #endregion

        private void mnuLoadProjectFile_Click(object sender, System.EventArgs e)
        {
        }

        private void DataDumpForm_Load(object sender, System.EventArgs e)
        {
            if (connData != null)
            {
                GetDatabaseList();
                settingsControl1.Server = connData.SQLServerName;
            }
        }

        private void GetDatabaseList()
        {
            Cursor = Cursors.WaitCursor;
            try
            {
                statStatus.Text = "Retrieving Database list from " + connData.SQLServerName;
                ddDatabaseList.Items.Clear();
                DatabaseList dbs = SqlSync.DbInformation.InfoHelper.GetDatabaseList(connData);
                for (int i = 0; i < dbs.Count; i++)
                    ddDatabaseList.Items.Add(dbs[i].DatabaseName);

                statStatus.Text = "Ready";
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void ddDatabaseList_SelectedIndexChanged(object sender, System.EventArgs e)
        {
            Cursor = Cursors.AppStarting;
            lstTables.Items.Clear();
            try
            {
                if (ddDatabaseList.SelectedItem == null)
                    return;

                statStatus.Text = "Enumerating tables and row count";
                string database = ddDatabaseList.SelectedItem.ToString();
                connData.DatabaseName = database;
                TableSize[] tables = SqlSync.DbInformation.InfoHelper.GetDatabaseTableListWithRowCount(connData);
                lstTables.Items.Clear();
                for (int i = 0; i < tables.Length; i++)
                {
                    lstTables.Items.Add(new ListViewItem(new string[] { tables[i].TableName, tables[i].RowCount.ToString() }));
                }
                statStatus.Text = "Ready";
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void lnkCheckAll_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            if (lnkCheckAll.Text == "Check All")
            {
                lnkCheckAll.Text = "Uncheck All";
                for (int i = 0; i < lstTables.Items.Count; i++)
                    lstTables.Items[i].Checked = true;
            }
            else
            {
                lnkCheckAll.Text = "Check All";
                while (lstTables.CheckedItems.Count > 0)
                    lstTables.CheckedItems[0].Checked = false;
            }
        }

        private void lnkExtractData_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            lstResults.Items.Clear();
            string[] selectedTables = new string[lstTables.CheckedItems.Count];
            for (int i = 0; i < lstTables.CheckedItems.Count; i++)
                selectedTables[i] = lstTables.CheckedItems[i].Text;

            DataDumpHelper helper = new DataDumpHelper(connData, selectedTables, settingsControl1.Project);
            helper.ProcessingTableData += new SqlSync.TableScript.DataDumpHelper.ProcessingTableDataEventHandler(helper_ProcessingTableData);
            helper.FileWritten += new SqlSync.TableScript.DataDumpHelper.FileWrittenEventHandler(helper_FileWritten);
            helper.ExtractAndWriteData();
        }



        private void mnuChangeFolder_Click(object sender, System.EventArgs e)
        {
            if (DialogResult.OK == folderBrowserDialog1.ShowDialog())
            {
                settingsControl1.Project = folderBrowserDialog1.SelectedPath;
                lnkExtractData.Enabled = true;

            }
        }

        private void settingsControl1_Click(object sender, System.EventArgs e)
        {
            mnuChangeFolder_Click(sender, e);
        }

        private void mnuChangeSqlServer_Click(object sender, System.EventArgs e)
        {
            ConnectionForm frmConnect = new ConnectionForm("Sql Data Extraction");
            DialogResult result = frmConnect.ShowDialog();
            if (result == DialogResult.OK)
            {
                connData = frmConnect.SqlConnection;
                settingsControl1.Server = connData.SQLServerName;
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
            lstResults.Items.Add(item);
        }

        private void helper_ProcessingTableData(object sender, ProcessingTableDataEventArgs e)
        {
            if (e.ProcessingComplete == false)
                statStatus.Text = e.TableName + " :: " + e.Message;
            else
                statStatus.Text = "Extraction Complete";
        }
        private void mnuOpen_Click(object sender, System.EventArgs e)
        {
            if (lstResults.SelectedItems.Count > 0)
            {
                Cursor = Cursors.WaitCursor;
                string file = lstResults.SelectedItems[0].SubItems[2].Text;
                System.Diagnostics.Process prc = new System.Diagnostics.Process();
                prc.StartInfo.FileName = "notepad.exe";
                prc.StartInfo.Arguments = file;
                prc.Start();
                Cursor = Cursors.Default;
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

        private void settingsControl1_ServerChanged(object sender, string serverName, string username, string password, AuthenticationType authType)
        {
            Connection.ConnectionData oldConnData = new Connection.ConnectionData();
            connData.Fill(oldConnData);
            Cursor = Cursors.WaitCursor;

            connData.SQLServerName = serverName;
            if (!string.IsNullOrWhiteSpace(username) && (!string.IsNullOrWhiteSpace(password)))
            {
                connData.UserId = username;
                connData.Password = password;
            }
            connData.AuthenticationType = authType;
            connData.ScriptTimeout = 5;
            try
            {
                GetDatabaseList();
                lstTables.Items.Clear();
                //this.ddDatabaseList = SqlSync.DbInformation.InfoHelper.GetDatabaseList(this.connData);
            }
            catch
            {
                MessageBox.Show("Error retrieving database list. Is the server running?", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                connData = oldConnData;
                settingsControl1.Server = oldConnData.SQLServerName;
            }


            Cursor = Cursors.Default;
        }

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            SqlSync.SqlBuild.UtilityHelper.OpenManual("DataExtractionandInsertion");
        }
    }
}
