using SqlSync.Connection;
using SqlSync.DbInformation;
using System;
using System.Windows.Forms;
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
                return connData;
            }
        }
        public DatabaseList DatabaseList
        {
            get
            {
                return databaseList;
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
            lblTitle.Text = title;
            Cursor = Cursors.AppStarting;
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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ConnectionForm));
            lblTitle = new System.Windows.Forms.Label();
            statusBar1 = new System.Windows.Forms.StatusStrip();
            pnlStatus = new System.Windows.Forms.ToolStripStatusLabel();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            lblVersion = new System.Windows.Forms.Label();
            sqlConnect1 = new SqlSync.SQLConnect(lastAuthenticationType);
            ((System.ComponentModel.ISupportInitialize)(pictureBox1)).BeginInit();
            SuspendLayout();
            // 
            // lblTitle
            // 
            lblTitle.Font = new System.Drawing.Font("Arial", 14F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            lblTitle.ForeColor = System.Drawing.Color.LightSlateGray;
            lblTitle.Location = new System.Drawing.Point(40, 10);
            lblTitle.Name = "lblTitle";
            lblTitle.Size = new System.Drawing.Size(260, 27);
            lblTitle.TabIndex = 1;
            lblTitle.Text = "Sql Build Manager";
            lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            // 
            // statusBar1
            // 
            statusBar1.Location = new System.Drawing.Point(0, 424);
            statusBar1.Name = "statusBar1";
            statusBar1.Items.AddRange(new System.Windows.Forms.ToolStripStatusLabel[] {
            pnlStatus});
            //this.statusBar1.ShowPanels = true;
            statusBar1.Size = new System.Drawing.Size(287, 28);
            statusBar1.TabIndex = 2;
            statusBar1.Text = "statusBar1";
            // 
            // pnlStatus
            // 
            pnlStatus.AutoSize = true;
            pnlStatus.Spring = true;
            pnlStatus.Name = "pnlStatus";
            pnlStatus.Text = "Enumerating Sql Server List...";
            pnlStatus.Width = 266;
            // 
            // pictureBox1
            // 
            pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            pictureBox1.Location = new System.Drawing.Point(298, 3);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(38, 41);
            pictureBox1.TabIndex = 3;
            pictureBox1.TabStop = false;
            // 
            // lblVersion
            // 
            lblVersion.AutoSize = true;
            lblVersion.Font = new System.Drawing.Font("Microsoft Sans Serif", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            lblVersion.ForeColor = System.Drawing.SystemColors.Highlight;
            lblVersion.Location = new System.Drawing.Point(4, 473);
            lblVersion.Name = "lblVersion";
            lblVersion.Size = new System.Drawing.Size(54, 15);
            lblVersion.TabIndex = 4;
            lblVersion.Text = "Version: ";
            // 
            // sqlConnect1
            // 
            sqlConnect1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            sqlConnect1.DisplayDatabaseDropDown = false;
            sqlConnect1.Enabled = false;
            sqlConnect1.Location = new System.Drawing.Point(11, 40);
            sqlConnect1.Name = "sqlConnect1";
            sqlConnect1.Size = new System.Drawing.Size(260, 356);
            sqlConnect1.TabIndex = 0;
            sqlConnect1.ServerConnected += new SqlSync.ServerConnectedEventHandler(sqlConnect1_ServerConnected);
            sqlConnect1.ServersEnumerated += new SqlSync.ServersEnumeratedEventHandler(sqlConnect1_ServersEnumerated);
            // 
            // ConnectionForm
            // 
            AutoScaleBaseSize = new System.Drawing.Size(6, 15);
            ClientSize = new System.Drawing.Size(287, 452);
            Controls.Add(lblVersion);
            Controls.Add(pictureBox1);
            Controls.Add(statusBar1);
            Controls.Add(lblTitle);
            Controls.Add(sqlConnect1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            KeyPreview = true;
            Name = "ConnectionForm";
            Text = "Initalize Sql Server Connection";
            Load += new System.EventHandler(ConnectionForm_Load);
            KeyDown += new System.Windows.Forms.KeyEventHandler(ConnectionForm_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(pictureBox1)).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }
        #endregion

        private void sqlConnect1_ServerConnected(object sender, SqlSync.ServerConnectedEventArgs e)
        {
            if (e.Connected)
            {
                connData.DatabaseName = sqlConnect1.Database;
                connData.Password = sqlConnect1.Password;
                connData.SQLServerName = sqlConnect1.SQLServer;
                connData.UserId = sqlConnect1.UserId;
                connData.AuthenticationType = sqlConnect1.AuthenticationType;
                lastAuthenticationType = sqlConnect1.AuthenticationType;
                databaseList = sqlConnect1.DatabaseList;
                DialogResult = DialogResult.OK;
                Close();
            }
            else
            {
                DialogResult = DialogResult.Cancel;
            }
        }

        private void sqlConnect1_ServersEnumerated(object sender, SqlSync.ServersEnumeratedEventArgs e)
        {
            if (e.Message.Length > 0)
                pnlStatus.Text = e.Message;
            else
                pnlStatus.Text = "Ready";
            sqlConnect1.Enabled = true;
            Cursor = Cursors.Default;
        }

        private void ConnectionForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
            else if (e.KeyCode == Keys.Enter)
            {
                sqlConnect1.SetConnection();
            }
        }

        private void ConnectionForm_Load(object sender, EventArgs e)
        {
            lblVersion.Text = "Version: " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            Show();
        }
    }
}
