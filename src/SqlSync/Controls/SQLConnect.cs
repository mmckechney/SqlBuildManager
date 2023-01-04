using Microsoft.SqlServer.Management.Smo;
using SqlBuildManager.Enterprise;
using SqlSync.Connection;
using SqlSync.DbInformation;
using SqlSync.SqlBuild;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Windows.Forms;


namespace SqlSync
{
    /// <summary>
    /// User Control to Encapsulate the selection of a SQL Server, 
    /// Connecting to it and selecting a database.
    /// </summary>
    public class SQLConnect : System.Windows.Forms.UserControl
    {

        private System.Windows.Forms.ComboBox ddDatabase;
        private System.Windows.Forms.TextBox txtPassword;
        private System.Windows.Forms.TextBox txtUser;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ComboBox ddServers;
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
        private Label label4;
        private ComboBox ddAuthentication;

        private SqlSync.Connection.AuthenticationType? initialAuthenticationType;

        private ServerConnectConfig.ServerConfigurationDataTable serverConfigTbl = null;
        [Category("Appearance")]
        public bool DisplayDatabaseDropDown
        {
            get
            {
                if (ddDatabase != null)
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
                if (ddDatabase != null)
                {
                    ddDatabase.Visible = value;
                    lblDatabases.Visible = value;
                    displayDatabaseDropDown = value;
                }
                else
                {
                    displayDatabaseDropDown = value;
                }
            }
        }

        public SQLConnect()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
        }

        public SQLConnect(Connection.AuthenticationType? authenticationType)
        {
            initialAuthenticationType = authenticationType;
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
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

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("Registered Servers");
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SQLConnect));
            lblDatabases = new System.Windows.Forms.Label();
            ddDatabase = new System.Windows.Forms.ComboBox();
            txtPassword = new System.Windows.Forms.TextBox();
            txtUser = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label1 = new System.Windows.Forms.Label();
            ddServers = new System.Windows.Forms.ComboBox();
            btnConnect = new System.Windows.Forms.Button();
            bgWorker = new System.ComponentModel.BackgroundWorker();
            treeView1 = new System.Windows.Forms.TreeView();
            contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(components);
            newServerRegistrationToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            newServerGroupToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            deleteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            toolStripSeparator3 = new System.Windows.Forms.ToolStripSeparator();
            importFromMasterListMenuStripItem = new System.Windows.Forms.ToolStripMenuItem();
            testToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            imageList1 = new System.Windows.Forms.ImageList(components);
            label4 = new System.Windows.Forms.Label();
            ddAuthentication = new System.Windows.Forms.ComboBox();
            contextMenuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // lblDatabases
            // 
            lblDatabases.Location = new System.Drawing.Point(9, 179);
            lblDatabases.Name = "lblDatabases";
            lblDatabases.Size = new System.Drawing.Size(88, 16);
            lblDatabases.TabIndex = 25;
            lblDatabases.Text = "Databases";
            lblDatabases.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // ddDatabase
            // 
            ddDatabase.Enabled = false;
            ddDatabase.Location = new System.Drawing.Point(12, 198);
            ddDatabase.Name = "ddDatabase";
            ddDatabase.Size = new System.Drawing.Size(240, 21);
            ddDatabase.TabIndex = 5;
            // 
            // txtPassword
            // 
            txtPassword.Enabled = false;
            txtPassword.Location = new System.Drawing.Point(12, 139);
            txtPassword.Name = "txtPassword";
            txtPassword.PasswordChar = '*';
            txtPassword.Size = new System.Drawing.Size(240, 20);
            txtPassword.TabIndex = 3;
            txtPassword.MouseEnter += new System.EventHandler(txtPassword_MouseEnter);
            txtPassword.MouseLeave += new System.EventHandler(txtPassword_MouseLeave);
            // 
            // txtUser
            // 
            txtUser.Enabled = false;
            txtUser.Location = new System.Drawing.Point(12, 101);
            txtUser.Name = "txtUser";
            txtUser.Size = new System.Drawing.Size(240, 20);
            txtUser.TabIndex = 2;
            // 
            // label3
            // 
            label3.Location = new System.Drawing.Point(8, 121);
            label3.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(180, 18);
            label3.TabIndex = 22;
            label3.Text = "Password";
            label3.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // label2
            // 
            label2.Location = new System.Drawing.Point(8, 83);
            label2.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(180, 18);
            label2.TabIndex = 20;
            label2.Text = "User Name";
            label2.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // label1
            // 
            label1.Location = new System.Drawing.Point(9, 3);
            label1.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(180, 18);
            label1.TabIndex = 19;
            label1.Text = "SQL Servers";
            label1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // ddServers
            // 
            ddServers.Location = new System.Drawing.Point(12, 21);
            ddServers.Name = "ddServers";
            ddServers.Size = new System.Drawing.Size(240, 21);
            ddServers.TabIndex = 0;
            ddServers.SelectionChangeCommitted += new System.EventHandler(ddServers_SelectionChangeCommitted);
            // 
            // btnConnect
            // 
            btnConnect.FlatStyle = System.Windows.Forms.FlatStyle.System;
            btnConnect.Location = new System.Drawing.Point(188, 169);
            btnConnect.Name = "btnConnect";
            btnConnect.Size = new System.Drawing.Size(64, 23);
            btnConnect.TabIndex = 4;
            btnConnect.Text = "Connect";
            btnConnect.Click += new System.EventHandler(btnConnect_Click);
            // 
            // bgWorker
            // 
            bgWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(bgWorker_DoWork);
            bgWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bgWorker_RunWorkerCompleted);
            // 
            // treeView1
            // 
            treeView1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            treeView1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            treeView1.ContextMenuStrip = contextMenuStrip1;
            treeView1.ImageIndex = 2;
            treeView1.ImageList = imageList1;
            treeView1.Indent = 19;
            treeView1.Location = new System.Drawing.Point(12, 227);
            treeView1.Name = "treeView1";
            treeNode1.ImageIndex = 3;
            treeNode1.Name = "Node0";
            treeNode1.Text = "Registered Servers";
            treeView1.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            treeView1.SelectedImageIndex = 3;
            treeView1.ShowRootLines = false;
            treeView1.Size = new System.Drawing.Size(240, 287);
            treeView1.TabIndex = 6;
            treeView1.NodeMouseClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(treeView1_NodeMouseClick);
            treeView1.NodeMouseDoubleClick += new System.Windows.Forms.TreeNodeMouseClickEventHandler(treeView1_NodeMouseDoubleClick);
            // 
            // contextMenuStrip1
            // 
            contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            newServerRegistrationToolStripMenuItem,
            newServerGroupToolStripMenuItem,
            toolStripSeparator1,
            deleteToolStripMenuItem,
            toolStripSeparator3,
            importFromMasterListMenuStripItem});
            contextMenuStrip1.Name = "contextMenuStrip1";
            contextMenuStrip1.Size = new System.Drawing.Size(325, 104);
            contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(contextMenuStrip1_Opening);
            // 
            // newServerRegistrationToolStripMenuItem
            // 
            newServerRegistrationToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Server1;
            newServerRegistrationToolStripMenuItem.Name = "newServerRegistrationToolStripMenuItem";
            newServerRegistrationToolStripMenuItem.Size = new System.Drawing.Size(324, 22);
            newServerRegistrationToolStripMenuItem.Text = "Add SQL Server listed above to this group";
            newServerRegistrationToolStripMenuItem.Click += new System.EventHandler(newServerRegistrationToolStripMenuItem_Click);
            // 
            // newServerGroupToolStripMenuItem
            // 
            newServerGroupToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Folder_Closed;
            newServerGroupToolStripMenuItem.Name = "newServerGroupToolStripMenuItem";
            newServerGroupToolStripMenuItem.Size = new System.Drawing.Size(324, 22);
            newServerGroupToolStripMenuItem.Text = "New Server Group";
            newServerGroupToolStripMenuItem.Click += new System.EventHandler(newServerGroupToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(321, 6);
            // 
            // deleteToolStripMenuItem
            // 
            deleteToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Delete1;
            deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            deleteToolStripMenuItem.Size = new System.Drawing.Size(324, 22);
            deleteToolStripMenuItem.Text = "Delete";
            deleteToolStripMenuItem.Click += new System.EventHandler(deleteToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            toolStripSeparator3.Name = "toolStripSeparator3";
            toolStripSeparator3.Size = new System.Drawing.Size(321, 6);
            // 
            // importFromMasterListMenuStripItem
            // 
            importFromMasterListMenuStripItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            testToolStripMenuItem});
            importFromMasterListMenuStripItem.Image = global::SqlSync.Properties.Resources.Import;
            importFromMasterListMenuStripItem.Name = "importFromMasterListMenuStripItem";
            importFromMasterListMenuStripItem.Size = new System.Drawing.Size(324, 22);
            importFromMasterListMenuStripItem.Text = "Import from pre-defined master registration list";
            importFromMasterListMenuStripItem.Visible = false;
            // 
            // testToolStripMenuItem
            // 
            testToolStripMenuItem.Name = "testToolStripMenuItem";
            testToolStripMenuItem.Size = new System.Drawing.Size(93, 22);
            testToolStripMenuItem.Text = "test";
            testToolStripMenuItem.Click += new System.EventHandler(importRegisteredServerList_Click);
            // 
            // imageList1
            // 
            imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            imageList1.TransparentColor = System.Drawing.Color.Transparent;
            imageList1.Images.SetKeyName(0, "database.png");
            imageList1.Images.SetKeyName(1, "Folder-Closed.png");
            imageList1.Images.SetKeyName(2, "Server1.png");
            imageList1.Images.SetKeyName(3, "data_server.ico");
            // 
            // label4
            // 
            label4.Location = new System.Drawing.Point(9, 44);
            label4.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(180, 18);
            label4.TabIndex = 33;
            label4.Text = "Authentication type";
            label4.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // ddAuthentication
            // 
            ddAuthentication.BackColor = System.Drawing.Color.Snow;
            ddAuthentication.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            ddAuthentication.Location = new System.Drawing.Point(12, 62);
            ddAuthentication.Name = "ddAuthentication";
            ddAuthentication.Size = new System.Drawing.Size(240, 21);
            ddAuthentication.TabIndex = 1;
            ddAuthentication.SelectionChangeCommitted += new System.EventHandler(ddAuthentication_SelectionChangeCommitted);
            // 
            // SQLConnect
            // 
            Controls.Add(label4);
            Controls.Add(ddAuthentication);
            Controls.Add(treeView1);
            Controls.Add(btnConnect);
            Controls.Add(lblDatabases);
            Controls.Add(ddDatabase);
            Controls.Add(txtPassword);
            Controls.Add(txtUser);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            Controls.Add(ddServers);
            Name = "SQLConnect";
            Size = new System.Drawing.Size(264, 526);
            Load += new System.EventHandler(SQLConnect_Load);
            contextMenuStrip1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }
        #endregion

        #region ## Public Properties ##
        public string SQLServer
        {
            get
            {
                if (ddServers.Items.Count > 0)
                {
                    return ddServers.Text.ToString();
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
                if (ddDatabase.Items.Count > 0 && ddDatabase.SelectedItem != null)
                {
                    return ddDatabase.SelectedItem.ToString();
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
                return txtPassword.Text;
            }
        }
        public string UserId
        {
            get
            {
                return txtUser.Text;
            }
        }
        public SqlSync.Connection.AuthenticationType AuthenticationType
        {
            get
            {
                return (SqlSync.Connection.AuthenticationType)SqlSync.Connection.Extensions.GetValueFromDescription<Connection.AuthenticationType>(ddAuthentication.SelectedItem.ToString());
            }

        }

        public DatabaseList DatabaseList
        {
            get
            {
                return databaseList;
            }

        }
        #endregion



        internal void SetConnection()
        {
            try
            {

                ddDatabase.Items.Clear();
                ConnectionData connData = new ConnectionData();
                connData.SQLServerName = ddServers.Text;
                connData.UserId = txtUser.Text;
                connData.Password = txtPassword.Text;
                connData.AuthenticationType = AuthenticationType;
                connData.ScriptTimeout = 10;

                bool hasError;
                databaseList = InfoHelper.GetDatabaseList(connData, out hasError);
                if (hasError)
                {
                    MessageBox.Show("Unable to connect to specified SQL Server.\r\nPlease select another server.", "Connection Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    if (ServersEnumerated != null)
                        ServersEnumerated(this, new ServersEnumeratedEventArgs(new string[0], "Connection error. Please re-select."));

                    Cursor = Cursors.Default;
                    return;
                }

                for (int i = 0; i < databaseList.Count; i++)
                    ddDatabase.Items.Add(databaseList[i].DatabaseName);
                if (ddDatabase.Visible)
                {
                    ddDatabase.Sorted = true;
                    if (ddDatabase.Items.Count > 0)
                    {
                        ddDatabase.SelectedIndex = 0;
                        ddDatabase.Enabled = true;
                    }
                    else
                    {
                        ddDatabase.Enabled = false;
                        ddDatabase.Text = "<No databases found>";
                    }
                }

                if (ServerConnected != null)
                {
                    UtilityHelper.UpdateRecentServerList(ddServers.Text, txtUser.Text, txtPassword.Text, AuthenticationType);
                    ServerConnected(this, new ServerConnectedEventArgs(true, AuthenticationType));
                }


                Cursor = Cursors.Default;
            }
            catch (Exception err)
            {
                Cursor = Cursors.Default;
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
            ddDatabase.Visible = displayDatabaseDropDown;
            lblDatabases.Visible = displayDatabaseDropDown;

            PopulateRegisteredServerTree();
            InitializeSqlEnumeration();

            var vals = Enum.GetValues(typeof(Connection.AuthenticationType));
            foreach (Connection.AuthenticationType item in Enum.GetValues(typeof(Connection.AuthenticationType)))
            {
                ddAuthentication.Items.Add(item.GetDescription());
            }
            ddAuthentication.SelectedIndex = 0;
            if (initialAuthenticationType.HasValue)
            {
                ddAuthentication.SelectedIndex = (int)initialAuthenticationType.Value;
            }
            ddAuthentication_SelectionChangeCommitted(null, null);
        }

        public void InitializeSqlEnumeration()
        {
            try
            {
                string[] recentDbs = UtilityHelper.GetRecentServers(out serverConfigTbl).ToArray();
                if (recentDbs.Length > 0)
                {
                    ddServers.Items.AddRange(recentDbs);
                    ddServers.SelectedIndex = 0;
                    Enabled = true;
                }
                bgWorker.RunWorkerAsync();
            }
            catch
            {
                //MessageBox.Show(err.Message,"Error");
            }
        }
        private void ddAuthentication_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (ddAuthentication.SelectedItem.ToString() == Connection.AuthenticationType.AzureADPassword.GetDescription()
                || ddAuthentication.SelectedItem.ToString() == Connection.AuthenticationType.Password.GetDescription())
            {

                txtPassword.Enabled = true;
                txtUser.Enabled = true;
            }
            else if (ddAuthentication.SelectedItem.ToString() == Connection.AuthenticationType.AzureADIntegrated.GetDescription()
                 || ddAuthentication.SelectedItem.ToString() == Connection.AuthenticationType.Windows.GetDescription())
            {
                txtPassword.Enabled = false;
                txtUser.Enabled = false;
            }
        }


        private void btnConnect_Click(object sender, System.EventArgs e)
        {
            if (ServersEnumerated != null)
                ServersEnumerated(this, new ServersEnumeratedEventArgs(new string[0], "Connecting to Specified Server..."));

            SetConnection();
        }

        [Category("Action")]
        public event ServerConnectedEventHandler ServerConnected;
        [Category("Action")]
        public event ServersEnumeratedEventHandler ServersEnumerated;

        /// <summary>
        /// Determine if running on an azure instance or not
        /// </summary>
        private static bool IsRunningInAzure
        {
            get
            {
                return false;
            }
            //get
            //{
            //    Guid guidId;
            //    if (RoleEnvironment.IsAvailable && Guid.TryParse(RoleEnvironment.DeploymentId, out guidId))
            //        return true;
            //    return false;
            //}
        }

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (IsRunningInAzure) //SmoApplication.EnumAvailableSqlServers does not work on Azure
            {
                e.Result = new string[0]; // Just go with the empty list
            }
            else
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
                catch
                {
                    // MessageBox.Show("Unable to get server list."); //\r\n" + exe.ToString(), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    e.Result = null;
                }
            }
        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
            {
                string[] servers = (string[])e.Result;

                for (int i = 0; i < servers.Length; i++)
                    if (ddDatabase.FindString(servers[i]) == -1)
                        ddServers.Items.Add(servers[i]);
            }

            if (ddServers.Items.Count > 0)
                ddServers.SelectedIndex = 0;
            else
                ddServers.Text = "<No available SQL Servers>";


            if (ServersEnumerated != null)
                ServersEnumerated(this, new ServersEnumeratedEventArgs(new string[0]));

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
            foreach (TreeNode grpNode in treeView1.Nodes[0].Nodes)
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
            if (treeView1.Nodes.Find(SQLServer, true).Length == 0)
            {
                TreeNode newServerNode = new TreeNode(SQLServer, 2, 2);
                newServerNode.ContextMenuStrip = contextMenuStrip1;
                RegServer srv = new RegServer() { Name = SQLServer };
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
            else if (treeView1.SelectedNode.Tag is ServerGroup)
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

                    if (!RegisteredServerHelper.ReloadRegisteredServerData(Path.Combine(tmp.Path, tmp.FileName)))
                    {
                        MessageBox.Show("Unable to load the registered server list \"" + tmp.Description + "\" from " + tmp.Path + "\\" + tmp.FileName, "Can't load the file", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        PopulateRegisteredServerTree();
                    }
                }
            }
        }

        private void ddServers_SelectionChangeCommitted(object sender, EventArgs e)
        {
            string username, password;
            Connection.AuthenticationType authType = UtilityHelper.GetServerCredentials(serverConfigTbl, ddServers.SelectedItem.ToString(), out username, out password);

            if (!string.IsNullOrWhiteSpace(username) || !string.IsNullOrWhiteSpace(password))
            {
                txtPassword.Text = password;
                txtUser.Text = username;
            }

            switch (authType)
            {
                case Connection.AuthenticationType.Password:
                    ddAuthentication.SelectedIndex = ddAuthentication.FindStringExact(Connection.AuthenticationType.Password.GetDescription());
                    break;
                case Connection.AuthenticationType.Windows:
                    ddAuthentication.SelectedIndex = ddAuthentication.FindStringExact(Connection.AuthenticationType.Windows.GetDescription());
                    break;
                case Connection.AuthenticationType.AzureADIntegrated:
                    ddAuthentication.SelectedIndex = ddAuthentication.FindStringExact(Connection.AuthenticationType.AzureADIntegrated.GetDescription());
                    break;
                case Connection.AuthenticationType.AzureADPassword:
                    ddAuthentication.SelectedIndex = ddAuthentication.FindStringExact(Connection.AuthenticationType.AzureADPassword.GetDescription());
                    break;
            }
            ddAuthentication_SelectionChangeCommitted(null, null);
        }

        private void txtPassword_MouseEnter(object sender, EventArgs e)
        {
            txtPassword.PasswordChar = '\0';
            txtPassword.Invalidate();
        }

        private void txtPassword_MouseLeave(object sender, EventArgs e)
        {
            txtPassword.PasswordChar = '*';
            txtPassword.Invalidate();
        }


    }

    #region ## ServerConnected Event Declaration ##
    public delegate void ServerConnectedEventHandler(object sender, ServerConnectedEventArgs e);
    public class ServerConnectedEventArgs : EventArgs
    {
        public readonly bool Connected;
        public readonly SqlSync.Connection.AuthenticationType AuthenticationType;

        public ServerConnectedEventArgs(bool connected, SqlSync.Connection.AuthenticationType authenticationType)
        {
            Connected = connected;
            AuthenticationType = authenticationType;
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
            SqlServers = sqlServers;
        }
        public ServersEnumeratedEventArgs(string[] sqlServers, string message)
            : this(sqlServers)
        {
            Message = message;
        }
    }
    #endregion
}
