using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using SqlSync.AutoScript;
using SqlSync.Connection;
using SqlSync.ObjectScript;
using Microsoft.Extensions.Logging;
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
			this.configFileName = configFile;
		}
		internal AutoScripting(AutoScriptingConfig.DatabaseScriptConfigRow configRow, bool includeFileHeaders, bool deletePreExisting,bool zipScripts)
		{
			InitializeComponent();
			this.configRow = configRow;
			this.includeFileHeaders = includeFileHeaders;
			this.deletePreExisting = deletePreExisting;
			this.zipScripts = zipScripts;
		}
		private  AutoScripting()
		{
			//
			// Required for Windows Form Designer support
			//
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

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AutoScripting));
            this.lstStatus = new System.Windows.Forms.ListView();
            this.colActivity = new System.Windows.Forms.ColumnHeader();
            this.colStart = new System.Windows.Forms.ColumnHeader();
            this.colEnd = new System.Windows.Forms.ColumnHeader();
            this.label1 = new System.Windows.Forms.Label();
            this.statusBar1 = new System.Windows.Forms.StatusStrip();
            this.statStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.lstDatabase = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
            this.label2 = new System.Windows.Forms.Label();
            this.bgWorker = new System.ComponentModel.BackgroundWorker();
            this.txtMessages = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.panel1 = new System.Windows.Forms.Panel();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.panel1.SuspendLayout();
            this.SuspendLayout();
            // 
            // lstStatus
            // 
            this.lstStatus.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstStatus.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colActivity,
            this.colStart,
            this.colEnd});
            this.lstStatus.FullRowSelect = true;
            this.lstStatus.GridLines = true;
            this.lstStatus.Location = new System.Drawing.Point(18, 25);
            this.lstStatus.MultiSelect = false;
            this.lstStatus.Name = "lstStatus";
            this.lstStatus.Size = new System.Drawing.Size(990, 196);
            this.lstStatus.TabIndex = 6;
            this.lstStatus.UseCompatibleStateImageBehavior = false;
            this.lstStatus.View = System.Windows.Forms.View.Details;
            // 
            // colActivity
            // 
            this.colActivity.Text = "Activity";
            this.colActivity.Width = 623;
            // 
            // colStart
            // 
            this.colStart.Text = "Start Time";
            this.colStart.Width = 142;
            // 
            // colEnd
            // 
            this.colEnd.Text = "End Time";
            this.colEnd.Width = 129;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(15, 9);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(100, 16);
            this.label1.TabIndex = 7;
            this.label1.Text = "Current Activity:";
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(0, 444);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Items.AddRange(new System.Windows.Forms.ToolStripStatusLabel[] {
            this.statStatus});
            //this.statusBar1.ShowPanels = true;
            this.statusBar1.Size = new System.Drawing.Size(1025, 22);
            this.statusBar1.TabIndex = 8;
            this.statusBar1.Text = "statusBar1";
            // 
            // statStatus
            // 
            this.statStatus.AutoSize = true;
            this.statStatus.Spring = true;
            this.statStatus.Name = "statStatus";
            this.statStatus.Width = 1008;
            // 
            // lstDatabase
            // 
            this.lstDatabase.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstDatabase.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.lstDatabase.FullRowSelect = true;
            this.lstDatabase.GridLines = true;
            this.lstDatabase.Location = new System.Drawing.Point(16, 22);
            this.lstDatabase.MultiSelect = false;
            this.lstDatabase.Name = "lstDatabase";
            this.lstDatabase.Size = new System.Drawing.Size(992, 64);
            this.lstDatabase.TabIndex = 9;
            this.lstDatabase.UseCompatibleStateImageBehavior = false;
            this.lstDatabase.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Server";
            this.columnHeader1.Width = 122;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Database";
            this.columnHeader2.Width = 119;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Status";
            this.columnHeader3.Width = 137;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Script Folder";
            this.columnHeader4.Width = 537;
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(15, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(112, 16);
            this.label2.TabIndex = 10;
            this.label2.Text = "Database scripting:";
            // 
            // bgWorker
            // 
            this.bgWorker.WorkerReportsProgress = true;
            this.bgWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgWorker_DoWork);
            this.bgWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgWorker_ProgressChanged);
            // 
            // txtMessages
            // 
            this.txtMessages.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtMessages.Location = new System.Drawing.Point(18, 27);
            this.txtMessages.Multiline = true;
            this.txtMessages.Name = "txtMessages";
            this.txtMessages.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtMessages.Size = new System.Drawing.Size(990, 81);
            this.txtMessages.TabIndex = 11;
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(15, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(112, 16);
            this.label3.TabIndex = 12;
            this.label3.Text = "Messages:";
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 89);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.label1);
            this.splitContainer1.Panel1.Controls.Add(this.lstStatus);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.txtMessages);
            this.splitContainer1.Panel2.Controls.Add(this.label3);
            this.splitContainer1.Size = new System.Drawing.Size(1025, 355);
            this.splitContainer1.SplitterDistance = 224;
            this.splitContainer1.TabIndex = 13;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.lstDatabase);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1025, 89);
            this.panel1.TabIndex = 14;
            // 
            // AutoScripting
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(1025, 466);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.statusBar1);
            this.Controls.Add(this.panel1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AutoScripting";
            this.Text = "Sql Build Manager :: Auto Scripting";
            this.Load += new System.EventHandler(this.AutoScripting_Load);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.Panel2.PerformLayout();
            this.splitContainer1.ResumeLayout(false);
            this.panel1.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		private void AutoScripting_Load(object sender, System.EventArgs e)
		{
			if(this.configRow != null)
			{
				this.Show();
				this.config = new AutoScriptingConfig();
				this.config.AutoScripting.AddAutoScriptingRow(false,this.includeFileHeaders,this.deletePreExisting,this.zipScripts);
				this.config.DatabaseScriptConfig.ImportRow(this.configRow);
	
				StartAutoScripting();
			}

		}

        private void StartAutoScripting()
        {
            AutoScriptingConfig.DatabaseScriptConfigRow row = this.config.DatabaseScriptConfig[0];
            ListViewItem newDb = new ListViewItem(new string[] { row.ServerName, row.DatabaseName, "Scripting", row.ScriptToPath });
            lstDatabase.Items.Insert(0, newDb);
            ConnectionData data = new ConnectionData();
            data.DatabaseName = row.DatabaseName;
            data.UserId = row.UserName;
            data.Password = row.Password;
            data.SQLServerName = row.ServerName;
            data.AuthenticationType = (AuthenticationType)Enum.Parse(typeof(AuthenticationType), row.AuthenticationType);
            data.StartingDirectory = row.ScriptToPath;
            this.Text = "Scripting " + row.DatabaseName + " on " + row.ServerName+" :: ";

            ObjectScriptingConfigData cfgData =
                new ObjectScriptingConfigData(this.deletePreExisting, true, this.zipScripts, this.includeFileHeaders, false, data);

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

                if (this.ScriptingCompleted != null)
                    this.ScriptingCompleted(null, EventArgs.Empty);
            }

            if (e.ProgressPercentage == -1) //error message
            {
                string message = "[" + DateTime.Now.ToString() + "]" + ((StatusEventArgs)e.UserState).StatusMessage + "\r\n";
                this.txtMessages.Text += message;
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
                this.Text = this.Text.Substring(0, this.Text.IndexOf("::") + 2) + " " + ((StatusEventArgs)e.UserState).StatusMessage;
            }
        }
	}

	public class MessageUpdateEventArgs: EventArgs
	{
		public readonly string Message;
		public MessageUpdateEventArgs(string message)
		{
			this.Message = message;
		}
	}
}
