using SqlSync.Connection;
using SqlSync.SqlBuild;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;

namespace SqlSync
{
    /// <summary>
    /// Summary description for SettingsControl.
    /// </summary>
    public class SettingsControl : System.Windows.Forms.UserControl
    {

        private bool fireServerChangeEvent = true;

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label lblServer;
        private System.Windows.Forms.Label lblProject;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label lblProjectLbl;
        private ComboBox ddRecentServers;
        private Label lblRecentServers;
        private ToolTip toolTip1;
        private IContainer components;
        private ServerConnectConfig.ServerConfigurationDataTable serverConfigTbl = null;

        public string ProjectLabelText
        {
            get
            {
                return lblProjectLbl.Text;
            }
            set
            {
                lblProjectLbl.Text = value;
            }
        }

        public string Server
        {
            get
            {
                return lblServer.Text;
            }
            set
            {
                lblServer.Text = value;
            }
        }
        public string Project
        {
            get
            {
                return lblProject.Text;
            }
            set
            {
                lblProject.Text = value;
            }
        }

        public SettingsControl()
        {
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
            label1 = new System.Windows.Forms.Label();
            lblProjectLbl = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            lblServer = new System.Windows.Forms.Label();
            lblProject = new System.Windows.Forms.Label();
            panel1 = new System.Windows.Forms.Panel();
            lblRecentServers = new System.Windows.Forms.Label();
            ddRecentServers = new System.Windows.Forms.ComboBox();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            label1.Location = new System.Drawing.Point(0, 0);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(100, 16);
            label1.TabIndex = 0;
            label1.Text = "Settings";
            label1.Click += new System.EventHandler(panel1_Click);
            label1.DoubleClick += new System.EventHandler(panel1_DoubleClick);
            // 
            // lblProjectLbl
            // 
            lblProjectLbl.AutoSize = true;
            lblProjectLbl.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            lblProjectLbl.Location = new System.Drawing.Point(16, 32);
            lblProjectLbl.Name = "lblProjectLbl";
            lblProjectLbl.Size = new System.Drawing.Size(86, 13);
            lblProjectLbl.TabIndex = 1;
            lblProjectLbl.Text = "Project File:";
            lblProjectLbl.Click += new System.EventHandler(panel1_Click);
            lblProjectLbl.DoubleClick += new System.EventHandler(panel1_DoubleClick);
            // 
            // label3
            // 
            label3.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            label3.Location = new System.Drawing.Point(16, 16);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(56, 16);
            label3.TabIndex = 2;
            label3.Text = "Server:";
            label3.Click += new System.EventHandler(panel1_Click);
            label3.DoubleClick += new System.EventHandler(panel1_DoubleClick);
            // 
            // lblServer
            // 
            lblServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            lblServer.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            lblServer.Location = new System.Drawing.Point(72, 16);
            lblServer.Name = "lblServer";
            lblServer.Size = new System.Drawing.Size(381, 16);
            lblServer.TabIndex = 3;
            lblServer.TextChanged += new System.EventHandler(lblServer_TextChanged);
            lblServer.Click += new System.EventHandler(panel1_Click);
            lblServer.DoubleClick += new System.EventHandler(panel1_DoubleClick);
            // 
            // lblProject
            // 
            lblProject.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            lblProject.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            lblProject.Location = new System.Drawing.Point(104, 32);
            lblProject.Name = "lblProject";
            lblProject.Size = new System.Drawing.Size(474, 16);
            lblProject.TabIndex = 4;
            lblProject.Click += new System.EventHandler(panel1_Click);
            lblProject.DoubleClick += new System.EventHandler(panel1_DoubleClick);
            // 
            // panel1
            // 
            panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panel1.Controls.Add(lblRecentServers);
            panel1.Controls.Add(ddRecentServers);
            panel1.Controls.Add(lblProjectLbl);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(lblServer);
            panel1.Controls.Add(label3);
            panel1.Controls.Add(lblProject);
            panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            panel1.Location = new System.Drawing.Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(584, 56);
            panel1.TabIndex = 5;
            panel1.Click += new System.EventHandler(panel1_Click);
            panel1.DoubleClick += new System.EventHandler(panel1_DoubleClick);
            // 
            // lblRecentServers
            // 
            lblRecentServers.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            lblRecentServers.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            lblRecentServers.Location = new System.Drawing.Point(259, 6);
            lblRecentServers.Name = "lblRecentServers";
            lblRecentServers.RightToLeft = System.Windows.Forms.RightToLeft.No;
            lblRecentServers.Size = new System.Drawing.Size(87, 16);
            lblRecentServers.TabIndex = 6;
            lblRecentServers.Text = "Recent Servers:";
            lblRecentServers.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            toolTip1.SetToolTip(lblRecentServers, "Quick Change Server Connection");
            // 
            // ddRecentServers
            // 
            ddRecentServers.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            ddRecentServers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            ddRecentServers.FlatStyle = System.Windows.Forms.FlatStyle.System;
            ddRecentServers.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            ddRecentServers.FormattingEnabled = true;
            ddRecentServers.Location = new System.Drawing.Point(352, 2);
            ddRecentServers.Name = "ddRecentServers";
            ddRecentServers.Size = new System.Drawing.Size(227, 20);
            ddRecentServers.TabIndex = 5;
            toolTip1.SetToolTip(ddRecentServers, "Quick Change Server Connection");
            ddRecentServers.SelectionChangeCommitted += new System.EventHandler(ddRecentServers_SelectionChangeCommitted);
            // 
            // SettingsControl
            // 
            BackColor = System.Drawing.Color.White;
            Controls.Add(panel1);
            Name = "SettingsControl";
            Size = new System.Drawing.Size(584, 56);
            Load += new System.EventHandler(SettingsControl_Load);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);

        }
        #endregion

        private void SettingsControl_Load(object sender, System.EventArgs e)
        {
            try
            {
                InitServers(false);
            }
            catch { }

            try
            {
                int startPosition = lblProjectLbl.Location.X + lblProjectLbl.Width;
                if (startPosition > lblServer.Location.X)
                    lblServer.Location = new System.Drawing.Point(startPosition, lblServer.Location.Y);

                lblProject.Location = new System.Drawing.Point(startPosition, lblProject.Location.Y);

                if (ServerChanged == null)
                {
                    lblRecentServers.Visible = false;
                    ddRecentServers.Visible = false;
                }

                lblServer_TextChanged(null, EventArgs.Empty);
            }
            catch { }
        }

        private void panel1_Click(object sender, System.EventArgs e)
        {
            if (Click != null)
                Click(sender, e);
        }

        private void panel1_DoubleClick(object sender, System.EventArgs e)
        {
            if (DoubleClick != null)
                DoubleClick(sender, e);
        }

        public void InitServers(bool forceRefresh)
        {
            fireServerChangeEvent = false;
            if (ddRecentServers.Items.Count != 0 && !forceRefresh)
                return;

            ddRecentServers.Items.Clear();


            List<string> recentDbs = UtilityHelper.GetRecentServers(out serverConfigTbl);
            ddRecentServers.Items.AddRange(recentDbs.ToArray());
            fireServerChangeEvent = true;
        }
        public new event EventHandler Click;
        public new event EventHandler DoubleClick;
        public event ServerChangedEventHandler ServerChanged;

        private void ddRecentServers_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (ServerChanged != null)
            {
                if (ddRecentServers.SelectedItem != null && fireServerChangeEvent)
                {
                    lblServer.Text = ddRecentServers.SelectedItem.ToString();

                    string username, password;
                    AuthenticationType authType = UtilityHelper.GetServerCredentials(serverConfigTbl, ddRecentServers.SelectedItem.ToString(), out username, out password);

                    ServerChanged(this, ddRecentServers.SelectedItem.ToString(), username, password, authType);
                }
            }
            else
            {
                MessageBox.Show("ERROR! No subscribers to the Recent Servers drop down changed event!");
            }
        }

        private void lblServer_TextChanged(object sender, EventArgs e)
        {
            fireServerChangeEvent = false;
            bool found = false;
            for (int i = 0; i < ddRecentServers.Items.Count; i++)
            {
                if (ddRecentServers.Items[i].ToString().ToUpper() == lblServer.Text.ToUpper())
                {
                    ddRecentServers.SelectedIndex = i;
                    found = true;
                }
            }
            if (!found)
                InitServers(true);

            fireServerChangeEvent = true;

        }

    }
    public delegate void ServerChangedEventHandler(object sender, string serverName, string username, string password, AuthenticationType authType);
}
