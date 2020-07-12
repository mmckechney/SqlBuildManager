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
using SqlSync.SqlBuild;
//using Microsoft.WindowsAzure.ServiceRuntime;

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
            this.label4 = new System.Windows.Forms.Label();
            this.ddAuthentication = new System.Windows.Forms.ComboBox();
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lblDatabases
            // 
            this.lblDatabases.Location = new System.Drawing.Point(9, 179);
            this.lblDatabases.Name = "lblDatabases";
            this.lblDatabases.Size = new System.Drawing.Size(88, 16);
            this.lblDatabases.TabIndex = 25;
            this.lblDatabases.Text = "Databases";
            this.lblDatabases.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // ddDatabase
            // 
            this.ddDatabase.Enabled = false;
            this.ddDatabase.Location = new System.Drawing.Point(12, 198);
            this.ddDatabase.Name = "ddDatabase";
            this.ddDatabase.Size = new System.Drawing.Size(240, 21);
            this.ddDatabase.TabIndex = 5;
            // 
            // txtPassword
            // 
            this.txtPassword.Enabled = false;
            this.txtPassword.Location = new System.Drawing.Point(12, 139);
            this.txtPassword.Name = "txtPassword";
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Size = new System.Drawing.Size(240, 20);
            this.txtPassword.TabIndex = 3;
            this.txtPassword.MouseEnter += new System.EventHandler(this.txtPassword_MouseEnter);
            this.txtPassword.MouseLeave += new System.EventHandler(this.txtPassword_MouseLeave);
            // 
            // txtUser
            // 
            this.txtUser.Enabled = false;
            this.txtUser.Location = new System.Drawing.Point(12, 101);
            this.txtUser.Name = "txtUser";
            this.txtUser.Size = new System.Drawing.Size(240, 20);
            this.txtUser.TabIndex = 2;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(8, 121);
            this.label3.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(180, 18);
            this.label3.TabIndex = 22;
            this.label3.Text = "Password";
            this.label3.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(8, 83);
            this.label2.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(180, 18);
            this.label2.TabIndex = 20;
            this.label2.Text = "User Name";
            this.label2.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(9, 3);
            this.label1.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(180, 18);
            this.label1.TabIndex = 19;
            this.label1.Text = "SQL Servers";
            this.label1.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // ddServers
            // 
            this.ddServers.Location = new System.Drawing.Point(12, 21);
            this.ddServers.Name = "ddServers";
            this.ddServers.Size = new System.Drawing.Size(240, 21);
            this.ddServers.TabIndex = 0;
            this.ddServers.SelectionChangeCommitted += new System.EventHandler(this.ddServers_SelectionChangeCommitted);
            // 
            // btnConnect
            // 
            this.btnConnect.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnConnect.Location = new System.Drawing.Point(188, 169);
            this.btnConnect.Name = "btnConnect";
            this.btnConnect.Size = new System.Drawing.Size(64, 23);
            this.btnConnect.TabIndex = 4;
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
            this.treeView1.Location = new System.Drawing.Point(12, 227);
            this.treeView1.Name = "treeView1";
            treeNode1.ImageIndex = 3;
            treeNode1.Name = "Node0";
            treeNode1.Text = "Registered Servers";
            this.treeView1.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1});
            this.treeView1.SelectedImageIndex = 3;
            this.treeView1.ShowRootLines = false;
            this.treeView1.Size = new System.Drawing.Size(240, 287);
            this.treeView1.TabIndex = 6;
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
            this.contextMenuStrip1.Size = new System.Drawing.Size(325, 104);
            this.contextMenuStrip1.Opening += new System.ComponentModel.CancelEventHandler(this.contextMenuStrip1_Opening);
            // 
            // newServerRegistrationToolStripMenuItem
            // 
            this.newServerRegistrationToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Server1;
            this.newServerRegistrationToolStripMenuItem.Name = "newServerRegistrationToolStripMenuItem";
            this.newServerRegistrationToolStripMenuItem.Size = new System.Drawing.Size(324, 22);
            this.newServerRegistrationToolStripMenuItem.Text = "Add SQL Server listed above to this group";
            this.newServerRegistrationToolStripMenuItem.Click += new System.EventHandler(this.newServerRegistrationToolStripMenuItem_Click);
            // 
            // newServerGroupToolStripMenuItem
            // 
            this.newServerGroupToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Folder_Closed;
            this.newServerGroupToolStripMenuItem.Name = "newServerGroupToolStripMenuItem";
            this.newServerGroupToolStripMenuItem.Size = new System.Drawing.Size(324, 22);
            this.newServerGroupToolStripMenuItem.Text = "New Server Group";
            this.newServerGroupToolStripMenuItem.Click += new System.EventHandler(this.newServerGroupToolStripMenuItem_Click);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(321, 6);
            // 
            // deleteToolStripMenuItem
            // 
            this.deleteToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Delete1;
            this.deleteToolStripMenuItem.Name = "deleteToolStripMenuItem";
            this.deleteToolStripMenuItem.Size = new System.Drawing.Size(324, 22);
            this.deleteToolStripMenuItem.Text = "Delete";
            this.deleteToolStripMenuItem.Click += new System.EventHandler(this.deleteToolStripMenuItem_Click);
            // 
            // toolStripSeparator3
            // 
            this.toolStripSeparator3.Name = "toolStripSeparator3";
            this.toolStripSeparator3.Size = new System.Drawing.Size(321, 6);
            // 
            // importFromMasterListMenuStripItem
            // 
            this.importFromMasterListMenuStripItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.testToolStripMenuItem});
            this.importFromMasterListMenuStripItem.Image = global::SqlSync.Properties.Resources.Import;
            this.importFromMasterListMenuStripItem.Name = "importFromMasterListMenuStripItem";
            this.importFromMasterListMenuStripItem.Size = new System.Drawing.Size(324, 22);
            this.importFromMasterListMenuStripItem.Text = "Import from pre-defined master registration list";
            this.importFromMasterListMenuStripItem.Visible = false;
            // 
            // testToolStripMenuItem
            // 
            this.testToolStripMenuItem.Name = "testToolStripMenuItem";
            this.testToolStripMenuItem.Size = new System.Drawing.Size(93, 22);
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
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(9, 44);
            this.label4.Margin = new System.Windows.Forms.Padding(3, 0, 3, 3);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(180, 18);
            this.label4.TabIndex = 33;
            this.label4.Text = "Authentication type";
            this.label4.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
            // 
            // ddAuthentication
            // 
            this.ddAuthentication.BackColor = System.Drawing.Color.Snow;
            this.ddAuthentication.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddAuthentication.Location = new System.Drawing.Point(12, 62);
            this.ddAuthentication.Name = "ddAuthentication";
            this.ddAuthentication.Size = new System.Drawing.Size(240, 21);
            this.ddAuthentication.TabIndex = 1;
            this.ddAuthentication.SelectionChangeCommitted += new System.EventHandler(this.ddAuthentication_SelectionChangeCommitted);
            // 
            // SQLConnect
            // 
            this.Controls.Add(this.label4);
            this.Controls.Add(this.ddAuthentication);
            this.Controls.Add(this.treeView1);
            this.Controls.Add(this.btnConnect);
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
                if (this.ddServers.Items.Count > 0)
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
                if (this.ddDatabase.Items.Count > 0 && this.ddDatabase.SelectedItem != null)
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
                connData.AuthenticationType = this.AuthenticationType;
                connData.ScriptTimeout = 10;

                bool hasError;
                this.databaseList = InfoHelper.GetDatabaseList(connData, out hasError);
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
                    UtilityHelper.UpdateRecentServerList(this.ddServers.Text, this.txtUser.Text, this.txtPassword.Text, this.AuthenticationType);
                    this.ServerConnected(this, new ServerConnectedEventArgs(true, this.AuthenticationType));
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

            var vals = Enum.GetValues(typeof(Connection.AuthenticationType));
            foreach(Connection.AuthenticationType item in Enum.GetValues(typeof(Connection.AuthenticationType)))
            {
                ddAuthentication.Items.Add(item.GetDescription());
            }
            this.ddAuthentication.SelectedIndex = 0;
            if (this.initialAuthenticationType.HasValue)
            {
                this.ddAuthentication.SelectedIndex = (int)this.initialAuthenticationType.Value;
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
            if (this.ServersEnumerated != null)
                this.ServersEnumerated(this, new ServersEnumeratedEventArgs(new string[0], "Connecting to Specified Server..."));

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
            Connection.AuthenticationType authType = UtilityHelper.GetServerCredentials(this.serverConfigTbl, this.ddServers.SelectedItem.ToString(), out username, out password);

            if (!string.IsNullOrWhiteSpace(username) || !string.IsNullOrWhiteSpace(password))
            {
                this.txtPassword.Text = password;
                this.txtUser.Text = username;
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
            this.txtPassword.PasswordChar = '\0';
            this.txtPassword.Invalidate();
        }

        private void txtPassword_MouseLeave(object sender, EventArgs e)
        {
            this.txtPassword.PasswordChar = '*';
            this.txtPassword.Invalidate();
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
			this.Connected = connected;
			this.AuthenticationType = authenticationType;
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
