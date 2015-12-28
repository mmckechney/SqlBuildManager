using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using SqlSync.SqlBuild;
using SqlSync.Connection;

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
				return this.lblProjectLbl.Text;
			}
			set
			{
				this.lblProjectLbl.Text = value;
			}
		}
	
		public string Server
		{
			get
			{
				return this.lblServer.Text;
			}
			set
			{
				this.lblServer.Text = value;
			}
		}
		public string Project
		{
			get
			{
				return this.lblProject.Text;
			}
			set
			{
				this.lblProject.Text =value;
			}
        }

		public SettingsControl()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();

            try
            {
                InitServers(false);
            }
            catch { }

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
            this.label1 = new System.Windows.Forms.Label();
            this.lblProjectLbl = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblServer = new System.Windows.Forms.Label();
            this.lblProject = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblRecentServers = new System.Windows.Forms.Label();
            this.ddRecentServers = new System.Windows.Forms.ComboBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(0, 0);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Settings";
            this.label1.DoubleClick += new System.EventHandler(this.panel1_DoubleClick);
            this.label1.Click += new System.EventHandler(this.panel1_Click);
            // 
            // lblProjectLbl
            // 
            this.lblProjectLbl.AutoSize = true;
            this.lblProjectLbl.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProjectLbl.Location = new System.Drawing.Point(16, 32);
            this.lblProjectLbl.Name = "lblProjectLbl";
            this.lblProjectLbl.Size = new System.Drawing.Size(86, 13);
            this.lblProjectLbl.TabIndex = 1;
            this.lblProjectLbl.Text = "Project File:";
            this.lblProjectLbl.DoubleClick += new System.EventHandler(this.panel1_DoubleClick);
            this.lblProjectLbl.Click += new System.EventHandler(this.panel1_Click);
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label3.Location = new System.Drawing.Point(16, 16);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(56, 16);
            this.label3.TabIndex = 2;
            this.label3.Text = "Server:";
            this.label3.DoubleClick += new System.EventHandler(this.panel1_DoubleClick);
            this.label3.Click += new System.EventHandler(this.panel1_Click);
            // 
            // lblServer
            // 
            this.lblServer.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblServer.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblServer.Location = new System.Drawing.Point(72, 16);
            this.lblServer.Name = "lblServer";
            this.lblServer.Size = new System.Drawing.Size(381, 16);
            this.lblServer.TabIndex = 3;
            this.lblServer.DoubleClick += new System.EventHandler(this.panel1_DoubleClick);
            this.lblServer.Click += new System.EventHandler(this.panel1_Click);
            this.lblServer.TextChanged += new System.EventHandler(this.lblServer_TextChanged);
            // 
            // lblProject
            // 
            this.lblProject.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lblProject.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblProject.Location = new System.Drawing.Point(104, 32);
            this.lblProject.Name = "lblProject";
            this.lblProject.Size = new System.Drawing.Size(474, 16);
            this.lblProject.TabIndex = 4;
            this.lblProject.DoubleClick += new System.EventHandler(this.panel1_DoubleClick);
            this.lblProject.Click += new System.EventHandler(this.panel1_Click);
            // 
            // panel1
            // 
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.lblRecentServers);
            this.panel1.Controls.Add(this.ddRecentServers);
            this.panel1.Controls.Add(this.lblProjectLbl);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.lblServer);
            this.panel1.Controls.Add(this.label3);
            this.panel1.Controls.Add(this.lblProject);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(584, 56);
            this.panel1.TabIndex = 5;
            this.panel1.DoubleClick += new System.EventHandler(this.panel1_DoubleClick);
            this.panel1.Click += new System.EventHandler(this.panel1_Click);
            // 
            // lblRecentServers
            // 
            this.lblRecentServers.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lblRecentServers.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblRecentServers.Location = new System.Drawing.Point(314, 6);
            this.lblRecentServers.Name = "lblRecentServers";
            this.lblRecentServers.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.lblRecentServers.Size = new System.Drawing.Size(87, 16);
            this.lblRecentServers.TabIndex = 6;
            this.lblRecentServers.Text = "Recent Servers:";
            this.lblRecentServers.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.toolTip1.SetToolTip(this.lblRecentServers, "Quick Change Server Connection");
            // 
            // ddRecentServers
            // 
            this.ddRecentServers.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.ddRecentServers.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddRecentServers.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ddRecentServers.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.ddRecentServers.FormattingEnabled = true;
            this.ddRecentServers.Location = new System.Drawing.Point(407, 2);
            this.ddRecentServers.Name = "ddRecentServers";
            this.ddRecentServers.Size = new System.Drawing.Size(172, 20);
            this.ddRecentServers.TabIndex = 5;
            this.toolTip1.SetToolTip(this.ddRecentServers, "Quick Change Server Connection");
            this.ddRecentServers.SelectionChangeCommitted += new System.EventHandler(this.ddRecentServers_SelectionChangeCommitted);
            // 
            // SettingsControl
            // 
            this.BackColor = System.Drawing.Color.White;
            this.Controls.Add(this.panel1);
            this.Name = "SettingsControl";
            this.Size = new System.Drawing.Size(584, 56);
            this.Load += new System.EventHandler(this.SettingsControl_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.ResumeLayout(false);

		}
		#endregion

		private void SettingsControl_Load(object sender, System.EventArgs e)
		{
			int startPosition = this.lblProjectLbl.Location.X+this.lblProjectLbl.Width;
			if(startPosition > this.lblServer.Location.X)
				this.lblServer.Location = new System.Drawing.Point(startPosition,this.lblServer.Location.Y);

			this.lblProject.Location = new System.Drawing.Point(startPosition,this.lblProject.Location.Y);

            if (this.ServerChanged == null)
            {
                lblRecentServers.Visible = false;
                ddRecentServers.Visible = false;
            }

            this.lblServer_TextChanged(null, EventArgs.Empty);
		}

		private void panel1_Click(object sender, System.EventArgs e)
		{
			if(this.Click != null)
				this.Click(sender,e);
		}

		private void panel1_DoubleClick(object sender, System.EventArgs e)
		{
			if(this.DoubleClick != null)
				this.DoubleClick(sender,e);
		}

        public void InitServers(bool forceRefresh)
        {
            this.fireServerChangeEvent = false;
            if (this.ddRecentServers.Items.Count != 0 && !forceRefresh)
                return;
            
            ddRecentServers.Items.Clear();

           
            List<string> recentDbs = UtilityHelper.GetRecentServers(out serverConfigTbl);
            this.ddRecentServers.Items.AddRange(recentDbs.ToArray());
            this.fireServerChangeEvent = true;
        }
		public new event EventHandler Click;
		public new event EventHandler DoubleClick;
        public event ServerChangedEventHandler ServerChanged;

        private void ddRecentServers_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (this.ServerChanged != null)
            {
                if (this.ddRecentServers.SelectedItem != null && this.fireServerChangeEvent)
                {
                    this.lblServer.Text = this.ddRecentServers.SelectedItem.ToString();
                    
                    string username, password;
                    AuthenticationType authType = UtilityHelper.GetServerCredentials(this.serverConfigTbl, this.ddRecentServers.SelectedItem.ToString(), out username, out password);

                    this.ServerChanged(this,this.ddRecentServers.SelectedItem.ToString(),username,password, authType);
                }
            }
            else
            {
                MessageBox.Show("ERROR! No subscribers to the Recent Servers drop down changed event!");
            }
        }

        private void lblServer_TextChanged(object sender, EventArgs e)
        {
            this.fireServerChangeEvent = false;
            bool found = false;
            for (int i = 0; i < this.ddRecentServers.Items.Count; i++)
            {
                if (this.ddRecentServers.Items[i].ToString().ToUpper() == lblServer.Text.ToUpper())
                {
                    this.ddRecentServers.SelectedIndex = i;
                    found = true;
                }
            }
            if (!found)
                InitServers(true);

            this.fireServerChangeEvent = true;

        }

    }
    public delegate void ServerChangedEventHandler(object sender, string serverName, string username, string password, AuthenticationType authType);
}
