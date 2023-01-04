using Microsoft.Extensions.Logging;
using SqlSync.Connection;
using SqlSync.ObjectScript;
using System;
using System.ComponentModel;
using System.Windows.Forms;
namespace SqlSync
{
    /// <summary>
    /// Summary description for AutoScripting.
    /// </summary>
    public class AutoScripting : System.Windows.Forms.Form
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        private System.Windows.Forms.ListView lstStatus;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.StatusStrip statusBar1;
        private System.Windows.Forms.ToolStripStatusLabel statStatus;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ListView lstDatabase;
        private string configFileName;
        AutoScriptingConfig config = null;
        AutoScriptingConfig.DatabaseScriptConfigRow configRow = null;
        private bool includeFileHeaders = true;
        private bool deletePreExisting = true;
        private System.Windows.Forms.ColumnHeader colActivity;
        private System.Windows.Forms.ColumnHeader colStart;
        private System.Windows.Forms.ColumnHeader colEnd;
        private BackgroundWorker bgWorker;
        private TextBox txtMessages;
        private Label label3;
        private SplitContainer splitContainer1;
        private Panel panel1;
        private bool zipScripts = true;
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public AutoScripting(string configFile)
        {
            InitializeComponent();
            configFileName = configFile;
        }
        internal AutoScripting(AutoScriptingConfig.DatabaseScriptConfigRow configRow, bool includeFileHeaders, bool deletePreExisting, bool zipScripts)
        {
            InitializeComponent();
            this.configRow = configRow;
            this.includeFileHeaders = includeFileHeaders;
            this.deletePreExisting = deletePreExisting;
            this.zipScripts = zipScripts;
        }
        private AutoScripting()
        {
            //
            // Required for Windows Form Designer support
            //
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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AutoScripting));
            lstStatus = new System.Windows.Forms.ListView();
            colActivity = new System.Windows.Forms.ColumnHeader();
            colStart = new System.Windows.Forms.ColumnHeader();
            colEnd = new System.Windows.Forms.ColumnHeader();
            label1 = new System.Windows.Forms.Label();
            statusBar1 = new System.Windows.Forms.StatusStrip();
            statStatus = new System.Windows.Forms.ToolStripStatusLabel();
            lstDatabase = new System.Windows.Forms.ListView();
            columnHeader1 = new System.Windows.Forms.ColumnHeader();
            columnHeader2 = new System.Windows.Forms.ColumnHeader();
            columnHeader3 = new System.Windows.Forms.ColumnHeader();
            columnHeader4 = new System.Windows.Forms.ColumnHeader();
            label2 = new System.Windows.Forms.Label();
            bgWorker = new System.ComponentModel.BackgroundWorker();
            txtMessages = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            splitContainer1 = new System.Windows.Forms.SplitContainer();
            panel1 = new System.Windows.Forms.Panel();
            splitContainer1.Panel1.SuspendLayout();
            splitContainer1.Panel2.SuspendLayout();
            splitContainer1.SuspendLayout();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // lstStatus
            // 
            lstStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            lstStatus.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            colActivity,
            colStart,
            colEnd});
            lstStatus.FullRowSelect = true;
            lstStatus.GridLines = true;
            lstStatus.Location = new System.Drawing.Point(18, 25);
            lstStatus.MultiSelect = false;
            lstStatus.Name = "lstStatus";
            lstStatus.Size = new System.Drawing.Size(990, 196);
            lstStatus.TabIndex = 6;
            lstStatus.UseCompatibleStateImageBehavior = false;
            lstStatus.View = System.Windows.Forms.View.Details;
            // 
            // colActivity
            // 
            colActivity.Text = "Activity";
            colActivity.Width = 623;
            // 
            // colStart
            // 
            colStart.Text = "Start Time";
            colStart.Width = 142;
            // 
            // colEnd
            // 
            colEnd.Text = "End Time";
            colEnd.Width = 129;
            // 
            // label1
            // 
            label1.Location = new System.Drawing.Point(15, 9);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(100, 16);
            label1.TabIndex = 7;
            label1.Text = "Current Activity:";
            // 
            // statusBar1
            // 
            statusBar1.Location = new System.Drawing.Point(0, 444);
            statusBar1.Name = "statusBar1";
            statusBar1.Items.AddRange(new System.Windows.Forms.ToolStripStatusLabel[] {
            statStatus});
            //this.statusBar1.ShowPanels = true;
            statusBar1.Size = new System.Drawing.Size(1025, 22);
            statusBar1.TabIndex = 8;
            statusBar1.Text = "statusBar1";
            // 
            // statStatus
            // 
            statStatus.AutoSize = true;
            statStatus.Spring = true;
            statStatus.Name = "statStatus";
            statStatus.Width = 1008;
            // 
            // lstDatabase
            // 
            lstDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            lstDatabase.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader1,
            columnHeader2,
            columnHeader3,
            columnHeader4});
            lstDatabase.FullRowSelect = true;
            lstDatabase.GridLines = true;
            lstDatabase.Location = new System.Drawing.Point(16, 22);
            lstDatabase.MultiSelect = false;
            lstDatabase.Name = "lstDatabase";
            lstDatabase.Size = new System.Drawing.Size(992, 64);
            lstDatabase.TabIndex = 9;
            lstDatabase.UseCompatibleStateImageBehavior = false;
            lstDatabase.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Server";
            columnHeader1.Width = 122;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Database";
            columnHeader2.Width = 119;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "Status";
            columnHeader3.Width = 137;
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "Script Folder";
            columnHeader4.Width = 537;
            // 
            // label2
            // 
            label2.Location = new System.Drawing.Point(15, 9);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(112, 16);
            label2.TabIndex = 10;
            label2.Text = "Database scripting:";
            // 
            // bgWorker
            // 
            bgWorker.WorkerReportsProgress = true;
            bgWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(bgWorker_DoWork);
            bgWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(bgWorker_ProgressChanged);
            // 
            // txtMessages
            // 
            txtMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            txtMessages.Location = new System.Drawing.Point(18, 27);
            txtMessages.Multiline = true;
            txtMessages.Name = "txtMessages";
            txtMessages.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            txtMessages.Size = new System.Drawing.Size(990, 81);
            txtMessages.TabIndex = 11;
            // 
            // label3
            // 
            label3.Location = new System.Drawing.Point(15, 10);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(112, 16);
            label3.TabIndex = 12;
            label3.Text = "Messages:";
            // 
            // splitContainer1
            // 
            splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            splitContainer1.Location = new System.Drawing.Point(0, 89);
            splitContainer1.Name = "splitContainer1";
            splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            splitContainer1.Panel1.Controls.Add(label1);
            splitContainer1.Panel1.Controls.Add(lstStatus);
            // 
            // splitContainer1.Panel2
            // 
            splitContainer1.Panel2.Controls.Add(txtMessages);
            splitContainer1.Panel2.Controls.Add(label3);
            splitContainer1.Size = new System.Drawing.Size(1025, 355);
            splitContainer1.SplitterDistance = 224;
            splitContainer1.TabIndex = 13;
            // 
            // panel1
            // 
            panel1.Controls.Add(lstDatabase);
            panel1.Controls.Add(label2);
            panel1.Dock = System.Windows.Forms.DockStyle.Top;
            panel1.Location = new System.Drawing.Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(1025, 89);
            panel1.TabIndex = 14;
            // 
            // AutoScripting
            // 
            AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            ClientSize = new System.Drawing.Size(1025, 466);
            Controls.Add(splitContainer1);
            Controls.Add(statusBar1);
            Controls.Add(panel1);
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            Name = "AutoScripting";
            Text = "Sql Build Manager :: Auto Scripting";
            Load += new System.EventHandler(AutoScripting_Load);
            splitContainer1.Panel1.ResumeLayout(false);
            splitContainer1.Panel2.ResumeLayout(false);
            splitContainer1.Panel2.PerformLayout();
            splitContainer1.ResumeLayout(false);
            panel1.ResumeLayout(false);
            ResumeLayout(false);

        }
        #endregion

        private void AutoScripting_Load(object sender, System.EventArgs e)
        {
            if (configRow != null)
            {
                Show();
                config = new AutoScriptingConfig();
                config.AutoScripting.AddAutoScriptingRow(false, includeFileHeaders, deletePreExisting, zipScripts);
                config.DatabaseScriptConfig.ImportRow(configRow);

                StartAutoScripting();
            }

        }

        private void StartAutoScripting()
        {
            AutoScriptingConfig.DatabaseScriptConfigRow row = config.DatabaseScriptConfig[0];
            ListViewItem newDb = new ListViewItem(new string[] { row.ServerName, row.DatabaseName, "Scripting", row.ScriptToPath });
            lstDatabase.Items.Insert(0, newDb);
            ConnectionData data = new ConnectionData();
            data.DatabaseName = row.DatabaseName;
            data.UserId = row.UserName;
            data.Password = row.Password;
            data.SQLServerName = row.ServerName;
            data.AuthenticationType = (AuthenticationType)Enum.Parse(typeof(AuthenticationType), row.AuthenticationType);
            data.StartingDirectory = row.ScriptToPath;
            Text = "Scripting " + row.DatabaseName + " on " + row.ServerName + " :: ";

            ObjectScriptingConfigData cfgData =
                new ObjectScriptingConfigData(deletePreExisting, true, zipScripts, includeFileHeaders, false, data);

            bgWorker.RunWorkerAsync(cfgData);
        }

        public event EventHandler ScriptingCompleted;
        //public event MessageUpdateEventHander MessageUpdateEvent;
        public delegate void MessageUpdateEventHander(object sender, MessageUpdateEventArgs e);

        private void AutoScripting_MessageUpdateEvent(object sender, MessageUpdateEventArgs e)
        {
            lstDatabase.Items[0].SubItems[2].Text = e.Message;
        }

        //private void helper_DatabaseScriptEvent(object sender, DatabaseScriptEventArgs e)
        //{
        //    if(e.IsNew)
        //    {
        //        lstStatus.Items.Insert(0,new ListViewItem(new string[]{e.SourceFile,e.Status,e.FullPath}));
        //    }
        //    else
        //    {
        //        for(int i=0;i<15;i++)
        //        {
        //            if(lstStatus.Items[i].SubItems[0].Text == e.SourceFile)
        //            {
        //                lstStatus.Items[i].SubItems[1].Text = e.Status;
        //                if(e.Status == "Object not in Db")
        //                {
        //                    lstStatus.Items[i].BackColor = Color.Orange;
        //                }
        //                break;
        //            }		
        //        }
        //    }
        //}



        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if (e.Argument is ObjectScriptingConfigData)
            {
                ObjectScriptHelper helper = new ObjectScriptHelper(((ObjectScriptingConfigData)e.Argument).ConnData);
                helper.ProcessFullScripting(e.Argument as ObjectScriptingConfigData, sender as BackgroundWorker, e);
            }
        }

        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 100)
            {
                if (lstStatus.Items.Count > 0)
                    lstStatus.Items[0].SubItems[2].Text = DateTime.Now.ToString();

                lstDatabase.Items[0].SubItems[2].Text = "Scripting Complete";

                if (ScriptingCompleted != null)
                    ScriptingCompleted(null, EventArgs.Empty);
            }

            if (e.ProgressPercentage == -1) //error message
            {
                string message = "[" + DateTime.Now.ToString() + "]" + ((StatusEventArgs)e.UserState).StatusMessage + "\r\n";
                txtMessages.Text += message;
                try
                {
                    System.Diagnostics.EventLog.WriteEntry("SqlSync-AutoScript", message);
                }
                catch { }
            }
            else if (e.UserState != null && e.UserState is StatusEventArgs)
            {
                if (lstStatus.Items.Count > 0)
                    lstStatus.Items[0].SubItems[2].Text = DateTime.Now.ToString();

                lstStatus.Items.Insert(0, new ListViewItem(new string[] { ((StatusEventArgs)e.UserState).StatusMessage, DateTime.Now.ToString(), "" }));
                Text = Text.Substring(0, Text.IndexOf("::") + 2) + " " + ((StatusEventArgs)e.UserState).StatusMessage;
            }
        }
    }

    public class MessageUpdateEventArgs : EventArgs
    {
        public readonly string Message;
        public MessageUpdateEventArgs(string message)
        {
            Message = message;
        }
    }
}
