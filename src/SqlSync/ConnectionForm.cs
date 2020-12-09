using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Threading;
using SqlSync.Connection;
using SqlSync.DbInformation;
namespace SqlSync
{
	/// <summary>
	/// Summary description for ConnectionForm.
	/// </summary>
	public class ConnectionForm : System.Windows.Forms.Form
	{
		private SqlSync.SQLConnect sqlConnect1;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		private ConnectionData connData = new ConnectionData();
		private System.Windows.Forms.Label lblTitle;
		private System.Windows.Forms.StatusStrip statusBar1;
		private System.Windows.Forms.ToolStripStatusLabel pnlStatus;
        private PictureBox pictureBox1;
        private Label lblVersion;
        private DatabaseList databaseList = new DatabaseList();

        private AuthenticationType? lastAuthenticationType
        {
            get
            {
                if (Properties.Settings.Default.DBAuthenticationType == -1)
                {
                    return null;
                }
                else
                {
                    return (AuthenticationType)Properties.Settings.Default.DBAuthenticationType;
                }
            }

            set
            {
                Properties.Settings.Default.DBAuthenticationType = (int)value;
                Properties.Settings.Default.Save();
            }
        }
        public ConnectionData SqlConnection
		{
			get
			{
				return this.connData;
			}
		}
        public DatabaseList DatabaseList
		{
			get
			{
				return this.databaseList;
			}
		}
		public ConnectionForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

		}
		public ConnectionForm(string title)
		{
			InitializeComponent();
			this.lblTitle.Text = title;
			this.Cursor = Cursors.AppStarting;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConnectionForm));
            this.lblTitle = new System.Windows.Forms.Label();
            this.statusBar1 = new System.Windows.Forms.StatusStrip();
            this.pnlStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblVersion = new System.Windows.Forms.Label();
            this.sqlConnect1 = new SqlSync.SQLConnect(lastAuthenticationType);
            ((System.ComponentModel.ISupportInitialize)(this.pnlStatus)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // lblTitle
            // 
            this.lblTitle.Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblTitle.ForeColor = System.Drawing.Color.LightSlateGray;
            this.lblTitle.Location = new System.Drawing.Point(40, 10);
            this.lblTitle.Name = "lblTitle";
            this.lblTitle.Size = new System.Drawing.Size(260, 27);
            this.lblTitle.TabIndex = 1;
            this.lblTitle.Text = "Sql Build Manager";
            this.lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(0, 424);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Items.AddRange(new System.Windows.Forms.ToolStripStatusLabel[] {
            this.pnlStatus});
            //this.statusBar1.ShowPanels = true;
            this.statusBar1.Size = new System.Drawing.Size(287, 28);
            this.statusBar1.TabIndex = 2;
            this.statusBar1.Text = "statusBar1";
            // 
            // pnlStatus
            // 
            this.pnlStatus.AutoSize = true;
            this.pnlStatus.Spring = true;
            this.pnlStatus.Name = "pnlStatus";
            this.pnlStatus.Text = "Enumerating Sql Server List...";
            this.pnlStatus.Width = 266;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(298, 3);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(38, 41);
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblVersion.ForeColor = System.Drawing.SystemColors.Highlight;
            this.lblVersion.Location = new System.Drawing.Point(4, 473);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(54, 15);
            this.lblVersion.TabIndex = 4;
            this.lblVersion.Text = "Version: ";
            // 
            // sqlConnect1
            // 
            this.sqlConnect1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.sqlConnect1.DisplayDatabaseDropDown = false;
            this.sqlConnect1.Enabled = false;
            this.sqlConnect1.Location = new System.Drawing.Point(11, 40);
            this.sqlConnect1.Name = "sqlConnect1";
            this.sqlConnect1.Size = new System.Drawing.Size(260, 356);
            this.sqlConnect1.TabIndex = 0;
            this.sqlConnect1.ServerConnected += new SqlSync.ServerConnectedEventHandler(this.sqlConnect1_ServerConnected);
            this.sqlConnect1.ServersEnumerated += new SqlSync.ServersEnumeratedEventHandler(this.sqlConnect1_ServersEnumerated);
            // 
            // ConnectionForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 15);
            this.ClientSize = new System.Drawing.Size(287, 452);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.statusBar1);
            this.Controls.Add(this.lblTitle);
            this.Controls.Add(this.sqlConnect1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "ConnectionForm";
            this.Text = "Initalize Sql Server Connection";
            this.Load += new System.EventHandler(this.ConnectionForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ConnectionForm_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.pnlStatus)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private void sqlConnect1_ServerConnected(object sender, SqlSync.ServerConnectedEventArgs e)
		{
			if(e.Connected)
			{
				this.connData.DatabaseName = sqlConnect1.Database;
				this.connData.Password = sqlConnect1.Password;  
				this.connData.SQLServerName = sqlConnect1.SQLServer;
				this.connData.UserId = sqlConnect1.UserId;
				this.connData.AuthenticationType = sqlConnect1.AuthenticationType;
                this.lastAuthenticationType = sqlConnect1.AuthenticationType;
                this.databaseList = sqlConnect1.DatabaseList;
				this.DialogResult = DialogResult.OK;
				this.Close();
			}
			else
			{
				this.DialogResult = DialogResult.Cancel;
			}
		}

		private void sqlConnect1_ServersEnumerated(object sender, SqlSync.ServersEnumeratedEventArgs e)
		{
            if (e.Message.Length > 0)
                this.pnlStatus.Text = e.Message;
            else
			    this.pnlStatus.Text = "Ready";
			this.sqlConnect1.Enabled = true;
			this.Cursor = Cursors.Default;
		}

		private void ConnectionForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
            if (e.KeyCode == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
            else if(e.KeyCode == Keys.Enter)
            {
                this.sqlConnect1.SetConnection();
            }
		}

        private void ConnectionForm_Load(object sender, EventArgs e)
        {
            this.lblVersion.Text = "Version: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            this.Show();
        }
	}
}
