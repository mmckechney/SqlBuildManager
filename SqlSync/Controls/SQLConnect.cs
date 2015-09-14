using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.IO;
using System.Threading;
using System.Data.SqlClient;
using SqlSync.DbInformation;
using SqlSync.Connection;
using Microsoft.SqlServer.Management.Smo;
using System.Collections.Generic;
using SqlBuildManager.Enterprise;
using System.Linq;
namespace SqlSync
{
	/// <summary>
	/// User Control to Encapsulate the selection of a SQL Server, 
	/// Connecting to it and selecting a database.
	/// </summary>
	public class SQLConnect : System.Windows.Forms.UserControl
	{

		private const string ConfigFileName = "SqlSync.cfg";
		private System.Windows.Forms.ComboBox ddDatabase;
		private System.Windows.Forms.TextBox txtPassword;
		private System.Windows.Forms.TextBox txtUser;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.ComboBox ddServers;
		private System.Windows.Forms.CheckBox chkWindowsAuthentication;
		private bool displayDatabaseDropDown = true;
		private System.Windows.Forms.Label lblDatabases;
        private DatabaseList databaseList = new DatabaseList();
		private System.Windows.Forms.Button btnConnect;
        private BackgroundWorker bgWorker;
        private TreeView treeView1;
        private ContextMenuStrip contextMenuStrip1;
        private ToolStripMenuItem newServerRegistrationToolStripMenuItem;
        private ToolStripMenuItem newServerGroupToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator1;
        private ToolStripMenuItem deleteToolStripMenuItem;
        private ToolStripSeparator toolStripSeparator3;
        private ToolStripMenuItem importFromMasterListMenuStripItem;
        private ImageList imageList1;
        private ToolStripMenuItem testToolStripMenuItem;
        //private ToolStripMenuItem toolStripMenuItem1;
        private IContainer components;

		[Category("Appearance")]
		public bool DisplayDatabaseDropDown
		{
			get
			{
				if(ddDatabase != null)
				{
					return ddDatabase.Visible;
				}
				else
				{
					return displayDatabaseDropDown;
				}
			}
			set
			{
				if(ddDatabase != null)
				{
					ddDatabase.Visible = value;
					lblDatabases.Visible = value;
					this.displayDatabaseDropDown = value;
				}
				else
				{
					this.displayDatabaseDropDown = value;
				}
			}
		}

		public SQLConnect()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Registered Servers");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SQLConnect));
            this.lblDatabases = new System.Windows.Forms.Label();
            this.ddDatabase = new System.Windows.Forms.ComboBox();
            this.txtPassword = new System.Windows.Forms.TextBox();
            this.txtUser = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.ddServers = new System.Windows.Forms.ComboBox();
            this.chkWindowsAuthentication = new System.Windows.Forms.CheckBox();
            this.btnConnect = new System.Windows.Forms.Button();
            this.bgWorker = new System.ComponentModel.BackgroundWorker();
            this.treeView1 = new System.Windows.Forms.TreeView();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.newServerRegistrationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.newServerGroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            this.importFromMasterListMenuStripItem = new System.Windows.Forms.ToolStripMenuItem();
            this.testToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblDatabases
            // 
            this.lblDatabases.Location = new System.Drawing.Point(8, 144);
            this.lblDatabases.Name = "lblDatabases";
            this.lblDatabases.Size = new System.Drawing.Size(88, 16);
            this.lblDatabases.TabIndex = 25;
            this.lblDatabases.Text = "Databases";
            // 
            // ddDatabase
            // 
            this.ddDatabase.Enabled = false;
            this.ddDatabase.Location = new System.Drawing.Point(12, 160);
            this.ddDatabase.Name = "ddDatabase";
            this.ddDatabase.Size = new System.Drawing.Size(240, 21);
            this.ddDatabase.TabIndex = 24;
            // 
            // txtPassword
            // 
            this.txtPassword.Enabled = false;
            this.txtPassword.Location = new System.Drawing.Point(12, 104);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.Size = new System.Drawing.Size(240, 20);
            this.txtPassword.TabIndex = 23;
            // 
            // txtUser
            // 
            this.txtUser.Enabled = false;
            this.txtUser.Location = new System.Drawing.Point(12, 64);
            this.txtUser.Name = "txtUser";
            this.txtUser.Size = new System.Drawing.Size(240, 20);
            this.txtUser.TabIndex = 21;
            this.txtUser.Text = "sa";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(12, 88);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(88, 16);
            this.label3.TabIndex = 22;
            this.label3.Text = "Password";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(12, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(120, 16);
            this.label2.TabIndex = 20;
            this.label2.Text = "User Name";
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(12, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(240, 16);
            this.label1.TabIndex = 19;
            this.label1.Text = "SQL Servers";
            // 
            // ddServers
            // 
            this.ddServers.Location = new System.Drawing.Point(12, 24);
            this.ddServers.Name = "ddServers";
            this.ddServers.Size = new System.Drawing.Size(240, 21);
            this.ddServers.TabIndex = 18;
            // 
            // chkWindowsAuthentication
            // 
            this.chkWindowsAuthentication.Checked = true;
            this.chkWindowsAuthentication.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkWindowsAuthentication.Location = new System.Drawing.Point(12, 128);
            this.chkWindowsAuthentication.Name = "chkWindowsAuthentication";
            this.chkWindowsAuthentication.Size = new System.Drawing.Size(168, 16);
            this.chkWindowsAuthentication.TabIndex = 27;
            this.chkWindowsAuthentication.Text = "Use Windows Authentication";
            this.chkWindowsAuthentication.CheckedChanged += new System.EventHandler(this.chkWindowsAuthentication_CheckedChanged);
            // 
            // btnConnect
            // 
            this.btnConnect.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnConnect.Location = new System.Drawing.Point(188, 128);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(64, 23);
            this.btnConnect.TabIndex = 28;
            this.btnConnect.Text = "Connect";
            this.btnConnect.Click += new System.EventHandler(this.btnConnect_Click);
            // 
            // bgWorker
            // 
            this.bgWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgWorker_DoWork);
            this.bgWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgWorker_RunWorkerCompleted);
            // 
            // treeView1
            // 
            this.treeView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.treeView1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.treeView1.ContextMenuStrip = this.contextMenuStrip1;
            this.treeView1.ImageIndex = 2;
            this.treeView1.ImageList = this.imageList1;
            this.treeView1.Indent = 19;
            this.treeView1.Location = new System.Drawing.Point(12, 187);
            this.treeView1.Name = "treeView1";
            treeNode1.ImageIndex = 3;
            treeNode1.Name = "Node0";
            treeNode1.Text = "Registered Servers";
            this.treeView1.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.treeView1.SelectedImageIndex = 3;
            this.treeView1.ShowRootLines = false;
            this.treeView1.Size = new System.Drawing.Size(240, 327);
            this.treeView1.TabIndex = 29;
            this.treeView1.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseClick);
            this.treeView1.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(this.treeView1_NodeMouseDoubleClick);
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.newServerRegistrationToolStripMenuItem,
            this.newServerGroupToolStripMenuItem,
            this.toolStripSeparator1,
            this.deleteToolStripMenuItem,
            this.toolStripSeparator3,
            this.importFromMasterListMenuStripItem});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(312, 126);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // newServerRegistrationToolStripMenuItem
            // 
            this.newServerRegistrationToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Server1;
            this.newServerRegistrationToolStripMenuItem.Name = "newServerRegistrationToolStripMenuItem";
            this.newServerRegistrationToolStripMenuItem.Size = new System.Drawing.Size(311, 22);
            this.newServerRegistrationToolStripMenuItem.Text = "Add SQL Server listed above to this group";
            this.newServerRegistrationToolStripMenuItem.Click += new System.EventHandler(this.newServerRegistrationToolStripMenuItem_Click);
            // 
            // newServerGroupToolStripMenuItem
            // 
            this.newServerGroupToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Folder_Closed;
            this.newServerGroupToolStripMenuItem.Name = "newServerGroupToolStripMenuItem";
            this.newServerGroupToolStripMenuItem.Size = new System.Drawing.Size(311, 22);
            this.newServerGroupToolStripMenuItem.Text = "New Server Group";
            this.newServerGroupToolStripMenuItem.Click += new System.EventHandler(this.newServerGroupToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(308, 6);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Delete1;
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(311, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(308, 6);
            // 
            // importFromMasterListMenuStripItem
            // 
            this.importFromMasterListMenuStripItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.testToolStripMenuItem});
            this.importFromMasterListMenuStripItem.Image = global::SqlSync.Properties.Resources.Import;
            this.importFromMasterListMenuStripItem.Name = "importFromMasterListMenuStripItem";
            this.importFromMasterListMenuStripItem.Size = new System.Drawing.Size(311, 22);
            this.importFromMasterListMenuStripItem.Text = "Import from pre-defined master registration list";
            this.importFromMasterListMenuStripItem.Visible = false;
            // 
            // testToolStripMenuItem
            // 
            this.testToolStripMenuItem.Name = "testToolStripMenuItem";
            this.testToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.testToolStripMenuItem.Text = "test";
            this.testToolStripMenuItem.Click += new System.EventHandler(this.importRegisteredServerList_Click);
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "database.png");
            this.imageList1.Images.SetKeyName(1, "Folder-Closed.png");
            this.imageList1.Images.SetKeyName(2, "Server1.png");
            this.imageList1.Images.SetKeyName(3, "data_server.ico");
            // 
            // SQLConnect
            // 
            this.Controls.Add(this.treeView1);
            this.Controls.Add(this.btnConnect);
            this.Controls.Add(this.chkWindowsAuthentication);
            this.Controls.Add(this.lblDatabases);
            this.Controls.Add(this.ddDatabase);
            this.Controls.Add(this.txtPassword);
            this.Controls.Add(this.txtUser);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.ddServers);
            this.Name = "SQLConnect";
            this.Size = new System.Drawing.Size(264, 526);
            this.Load += new System.EventHandler(this.SQLConnect_Load);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		#region ## Public Properties ##
		public string SQLServer
		{
			get
			{
				if(this.ddServers.Items.Count > 0)
				{
					return this.ddServers.Text.ToString();
				}
				else
				{
					return string.Empty;
				}
			}
		}
		public string Database
		{
			get
			{
				if(this.ddDatabase.Items.Count > 0 && this.ddDatabase.SelectedItem != null)
				{
					return this.ddDatabase.SelectedItem.ToString();
				}
				else
				{
					return string.Empty;
				}
			}		

		}
		public string Password
		{
			get
			{
				return this.txtPassword.Text;
			}
		}
		public string UserId
		{
			get
			{
				return this.txtUser.Text;
			}
		}
		public bool UseWindowsAuthentication
		{
			get
			{
				return this.chkWindowsAuthentication.Checked;
			}
		}
		public DatabaseList DatabaseList
		{
			get
			{
				return this.databaseList;
			}							 
		
		}
		#endregion



		internal void SetConnection()
		{
            try
            {
                
                this.ddDatabase.Items.Clear();
                ConnectionData connData = new ConnectionData();
                connData.SQLServerName = this.ddServers.Text;
                connData.UserId = this.txtUser.Text;
                connData.Password = this.txtPassword.Text;
                connData.UseWindowAuthentication = chkWindowsAuthentication.Checked;
                connData.ScriptTimeout = 10;

                bool hasError;
                this.databaseList = InfoHelper.GetDatabaseList(connData,out hasError);
                if (hasError)
                {
                    MessageBox.Show("Unable to connect to specified SQL Server.\r\nPlease select another server.", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    if (this.ServersEnumerated != null)
                        this.ServersEnumerated(this, new ServersEnumeratedEventArgs(new string[0], "Connection error. Please re-select."));

                    this.Cursor = Cursors.Default;
                    return;
                }

                for (int i = 0; i < databaseList.Count; i++)
                    this.ddDatabase.Items.Add(databaseList[i].DatabaseName);
               if (ddDatabase.Visible)
                {
                    this.ddDatabase.Sorted = true;
                    if (this.ddDatabase.Items.Count > 0)
                    {
                        this.ddDatabase.SelectedIndex = 0;
                        this.ddDatabase.Enabled = true;
                    }
                    else
                    {
                        this.ddDatabase.Enabled = false;
                        this.ddDatabase.Text = "<No databases found>";
                    }
                }

                if (this.ServerConnected != null)
                {
                    this.UpdateRecentServerList(this.ddServers.Text);
                    this.ServerConnected(this, new ServerConnectedEventArgs(true, chkWindowsAuthentication.Checked));
                }


                this.Cursor = Cursors.Default;
            }
            catch (Exception err)
            {
                this.Cursor = Cursors.Default;
                MessageBox.Show(err.Message, "Error");
            }
		}
		/// <summary>
		/// Initilized the network scan for SQL Servers
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void SQLConnect_Load(object sender, System.EventArgs e)
		{
			this.ddDatabase.Visible = this.displayDatabaseDropDown;
			this.lblDatabases.Visible = this.displayDatabaseDropDown;
            PopulateRegisteredServerTree();
            InitializeSqlEnumeration();
		}
		private string[] GetRecentDatabases()
		{
			string homePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) +@"\";
			if(File.Exists(homePath+ConfigFileName))
			{
				try
				{
					SqlSyncConfig config = new SqlSyncConfig();
					config.ReadXml(homePath+"SqlSync.cfg");
					string[] recentDbs = new string[config.RecentDatabase.Count];
					DataView view = config.RecentDatabase.DefaultView;
					view.Sort = config.RecentDatabase.LastAccessedColumn.ColumnName +" DESC";
					for(int i=0;i<view.Count;i++)
					{
						recentDbs[i] = ((SqlSyncConfig.RecentDatabaseRow) view[i].Row).Name;
					}
					return recentDbs;
				}
				catch
				{
					
				}

			}
			return new string[0];
		}
		private void UpdateRecentServerList(string databaseName)
		{
            try
            {
                string homePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\";
                SqlSyncConfig config = new SqlSyncConfig();
                if (File.Exists(homePath + ConfigFileName))
                    config.ReadXml(homePath + ConfigFileName);

                DataRow[] row = config.RecentDatabase.Select(config.RecentDatabase.NameColumn.ColumnName + " ='" + databaseName + "'");
                if (row.Length == 0)
                {
                    config.RecentDatabase.AddRecentDatabaseRow(databaseName, DateTime.Now);
                }
                else
                {
                    ((SqlSyncConfig.RecentDatabaseRow)row[0]).LastAccessed = DateTime.Now;
                }
                config.WriteXml(homePath + ConfigFileName);
            }
            catch (Exception exe)
            {
                //System.Diagnostics.EventLog.WriteEntry("SqlSync", "Error updating Recent Server List\r\n" + exe.ToString(), System.Diagnostics.EventLogEntryType.Error, 432);
            }


		}
		public void InitializeSqlEnumeration()
		{
			try
			{
				string[] recentDbs = GetRecentDatabases();
				if(recentDbs.Length >0)
				{
					this.ddServers.Items.AddRange(recentDbs);
					this.ddServers.SelectedIndex = 0;
					this.Enabled = true;
				}
                bgWorker.RunWorkerAsync();
			}
			catch
			{
				//MessageBox.Show(err.Message,"Error");
			}
		}

        private void chkWindowsAuthentication_CheckedChanged(object sender, System.EventArgs e)
		{
			if(chkWindowsAuthentication.Checked)
			{
				txtPassword.Enabled = false;
				txtUser.Enabled = false;
			}
			else
			{
				txtPassword.Enabled = true;
				txtUser.Enabled = true;
			}
		}

		private void btnConnect_Click(object sender, System.EventArgs e)
		{
            if (this.ServersEnumerated != null)
                this.ServersEnumerated(this, new ServersEnumeratedEventArgs(new string[0], "Connecting to Specified Server..."));

			SetConnection();
		}

		[Category("Action")]
		public event ServerConnectedEventHandler ServerConnected;
		[Category("Action")]
		public event ServersEnumeratedEventHandler ServersEnumerated;

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                DataTable tbl = SmoApplication.EnumAvailableSqlServers(false);

                string[] servers = new string[tbl.Rows.Count];
                for (int i = 0; i < tbl.Rows.Count; i++)
                {
                    servers[i] = tbl.Rows[i][0].ToString();
                }
                e.Result = servers;
            }
            catch (Exception exe)
            {
                MessageBox.Show("Unable to get server list.\r\n" + exe.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                string[] servers = (string[])e.Result;

                for (int i = 0; i < servers.Length; i++)
                    if (this.ddDatabase.FindString(servers[i]) == -1)
                        this.ddServers.Items.Add(servers[i]);
            }

            if (this.ddServers.Items.Count > 0)
                this.ddServers.SelectedIndex = 0;
            else
                this.ddServers.Text = "<No available SQL Servers>";


            if (this.ServersEnumerated != null)
                this.ServersEnumerated(this, new ServersEnumeratedEventArgs(new string[0]));

        }

        #region Registration Tree Methods

        private void PopulateRegisteredServerTree()
        {
            try
            {
                RegisteredServers regServers = RegisteredServerHelper.RegisteredServerData;
                if (regServers == null || regServers.ServerGroup == null || regServers.ServerGroup.Length == 0)
                    return;

                treeView1.Nodes[0].Nodes.Clear();

                foreach (ServerGroup grp in regServers.ServerGroup)
                {
                    if (grp == null)
                        continue;

                    TreeNode grpNode = new TreeNode(grp.Name, 1, 1);
                    grpNode.Tag = grp;
                    treeView1.Nodes[0].Nodes.Add(grpNode);
                    if (grp.RegServer != null && grp.RegServer.Length > 0)
                    {
                        foreach (RegServer srv in grp.RegServer)
                        {
                            TreeNode srvNode = new TreeNode(srv.Name, 2, 2);
                            srvNode.Tag = srv;
                            grpNode.Nodes.Add(srvNode);
                        }
                    }
                    //grpNode.Expand();
                }
                treeView1.Nodes[0].Expand();
            }
            catch
            {
            }
            

        }
        private void UpdateRegisteredServersList()
        {
            RegisteredServers tmpRegServers = new RegisteredServers();
            List<ServerGroup> grps = new List<ServerGroup>();
            foreach (TreeNode grpNode in this.treeView1.Nodes[0].Nodes)
            {
                if (grpNode.Tag is ServerGroup)
                {
                    ServerGroup tmpGrp = (ServerGroup)grpNode.Tag;
                    List<RegServer> srvs = new List<RegServer>();
                    foreach (TreeNode srvNode in grpNode.Nodes)
                    {
                        if (srvNode.Tag is RegServer)
                        {
                            srvs.Add((RegServer)srvNode.Tag);
                        }
                    }
                    tmpGrp.RegServer = srvs.ToArray();
                    grps.Add(tmpGrp);
                }
            }
            tmpRegServers.ServerGroup = grps.ToArray();

            RegisteredServerHelper.SaveRegisteredServers(tmpRegServers);

        }
        private void newServerGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            GetTextValueForm frmText = new GetTextValueForm("Add Server Group");
            if (DialogResult.OK == frmText.ShowDialog())
            {
                string grpName = frmText.TextValue;
                TreeNode newGroupNode = new TreeNode(grpName, 1, 1);
                ServerGroup grp = new ServerGroup() { Name = grpName };
                newGroupNode.ContextMenuStrip = contextMenuStrip1;
                newGroupNode.Tag = grp;
                treeView1.SelectedNode.Nodes.Add(newGroupNode);
                treeView1.SelectedNode.Expand();
                treeView1.Invalidate();

            }
            frmText.Dispose();

            UpdateRegisteredServersList();
        }

        private void treeView1_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            treeView1.SelectedNode = e.Node;
            if (e.Node.Tag is RegServer)
            {
                string serverName = e.Node.Text;
                ddServers.Items.Insert(0, serverName);
                ddServers.SelectedIndex = 0;

               
            }
        }


        private void newServerRegistrationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.Nodes.Find(this.SQLServer, true).Length == 0)
            {
                TreeNode newServerNode = new TreeNode(this.SQLServer, 2, 2);
                newServerNode.ContextMenuStrip = contextMenuStrip1;
                RegServer srv = new RegServer() { Name = this.SQLServer };
                newServerNode.Tag = srv;
                treeView1.SelectedNode.Nodes.Add(newServerNode);
                treeView1.SelectedNode.Expand();
                treeView1.Invalidate();
            }
            else
            {
                MessageBox.Show("This SQL Server is already registered", "Already have it", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            UpdateRegisteredServersList();
        }
        
        #endregion

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (EnterpriseConfigHelper.EnterpriseConfig != null && EnterpriseConfigHelper.EnterpriseConfig.RegisteredServerMasters != null &&
                    EnterpriseConfigHelper.EnterpriseConfig.RegisteredServerMasters.Length > 0)
            {
                importFromMasterListMenuStripItem.DropDownItems.Clear();

                importFromMasterListMenuStripItem.Visible = true;
                foreach (RegisteredServerListFile regServList in EnterpriseConfigHelper.EnterpriseConfig.RegisteredServerMasters)
                {
                    ToolStripMenuItem tmpItem = new ToolStripMenuItem(regServList.Description);
                    tmpItem.Tag = regServList;
                    tmpItem.Click += new EventHandler(importRegisteredServerList_Click);
                    importFromMasterListMenuStripItem.DropDownItems.Add(tmpItem);
                }
            }

            if (treeView1.SelectedNode.Tag is RegServer)
            {
                newServerGroupToolStripMenuItem.Enabled = false;
                newServerRegistrationToolStripMenuItem.Enabled = false;
                deleteToolStripMenuItem.Enabled = true;
            }
            else if(treeView1.SelectedNode.Tag is ServerGroup)
            {
                newServerGroupToolStripMenuItem.Enabled = false;
                newServerRegistrationToolStripMenuItem.Enabled = true;
                deleteToolStripMenuItem.Enabled = true;
            }
            else
            {
                newServerGroupToolStripMenuItem.Enabled = true;
                newServerRegistrationToolStripMenuItem.Enabled = false;
                deleteToolStripMenuItem.Enabled = false;
            }
        }

        private void treeView1_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag is RegServer)
            {
                treeView1_NodeMouseClick(sender, e);
                btnConnect_Click(null, System.EventArgs.Empty);
            }
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode.Tag is ServerGroup)
            {
                if (DialogResult.No == MessageBox.Show("Are you sure you want to remove this server group and all its registered servers?", "Just double checking", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                    return;
            }
            treeView1.SelectedNode.Remove();
            UpdateRegisteredServersList();
        }

        private void importRegisteredServerList_Click(object sender, EventArgs e)
        {
            if (sender is ToolStripMenuItem)
            {
                if (((ToolStripMenuItem)sender).Tag is RegisteredServerListFile)
                {
                    RegisteredServerListFile tmp = (RegisteredServerListFile)((ToolStripMenuItem)sender).Tag;
                    tmp.Path = (tmp.Path.EndsWith("\\") ? tmp.Path : tmp.Path + "\\");

                    if (!RegisteredServerHelper.ReloadRegisteredServerData(tmp.Path + tmp.FileName))
                        MessageBox.Show("Unable to load the registered server list \"" + tmp.Description + "\" from " + tmp.Path + "\\" + tmp.FileName, "Can't load the file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    else
                        PopulateRegisteredServerTree();
                }
            }
        }
    }

	#region ## ServerConnected Event Declaration ##
	public delegate void ServerConnectedEventHandler(object sender, ServerConnectedEventArgs e);
	public class ServerConnectedEventArgs : EventArgs
	{
		public readonly bool Connected;
		public readonly bool UseWindowsAuthentication;

		public ServerConnectedEventArgs(bool connected, bool useWindowsAuthentication)
		{
			this.Connected = connected;
			this.UseWindowsAuthentication = useWindowsAuthentication;
		}
	}
	#endregion

	#region ## ServersEnumerated Event Declaration ##
	public delegate void ServersEnumeratedEventHandler(object sender, ServersEnumeratedEventArgs e);
	public class ServersEnumeratedEventArgs : EventArgs
	{
		public readonly string[] SqlServers;
        public readonly string Message = string.Empty;

		public ServersEnumeratedEventArgs(string[] sqlServers)
		{
			this.SqlServers = sqlServers;
		}
        public ServersEnumeratedEventArgs(string[] sqlServers, string message)
            : this(sqlServers)
        {
            this.Message = message;
        }
	}
	#endregion
}
