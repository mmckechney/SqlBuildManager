using System;
using System.Windows.Forms;
using SqlSync.ObjectScript;
namespace SqlSync.AutoScript
{
	/// <summary>
	/// Summary description for AutoScriptHelper.
	/// </summary>
	public class AutoScriptMDI :Form 
	{
		private string configFile = string.Empty;
		private System.Windows.Forms.StatusBar statusBar1;
		private System.Windows.Forms.StatusBarPanel statOverall;
		private System.Windows.Forms.StatusBarPanel statRunningCount;
		private AutoScriptingConfig config;
		private int childCount = 0;
		private int completedCount = 0;
		private System.Windows.Forms.MenuStrip mainMenu1;
		private System.Windows.Forms.ToolStripMenuItem menuItem1;
		private System.Windows.Forms.ToolStripMenuItem menuItem2;
		private string runStatus = "Completed {0} of {1} Database Scripting Tasks";
        private System.ComponentModel.IContainer components;
		private bool closeOnComplete = false;
		public AutoScriptMDI(string configFile, bool closeOnComplete)
		{
			InitializeComponent();
			this.configFile = configFile;
			this.closeOnComplete = closeOnComplete;
			if(!closeOnComplete)
				this.Text += " (Remain open on Complete)";
			else
				this.Text += " (Close on Complete)";
		}
		public AutoScriptMDI(string configFile)
		{
			InitializeComponent();
			this.configFile = configFile;
			this.closeOnComplete = true;
			this.Text += " (Close on Complete)";
		}

		private void Process()
		{
			this.config = new AutoScriptingConfig();
			this.config.ReadXml(this.configFile);
			if(this.config.AutoScripting[0].AllowManualSelection)
			{
				AutoScriptingSelect frmSelect = new AutoScriptingSelect(this.config);
				DialogResult result = frmSelect.ShowDialog();
				if(result == DialogResult.OK)
					this.config = frmSelect.config;
				else
					return;
			}

			if(this.config.DatabaseScriptConfig.Rows.Count > 0)
			{
				for(int i=0;i<this.config.DatabaseScriptConfig.Rows.Count;i++)
				{
					childCount++;
					AutoScripting frmScr = new AutoScripting(this.config.DatabaseScriptConfig[i],this.config.AutoScripting[0].IncludeFileHeaders,this.config.AutoScripting[0].DeletePreExistingFiles,this.config.AutoScripting[0].ZipScripts);
					frmScr.ScriptingCompleted +=new EventHandler(frmScr_ScriptingCompleted);
					frmScr.MdiParent = this;
					frmScr.Show();
				}
				this.statOverall.Text = "Scripting";
				this.statRunningCount.Text = String.Format(this.runStatus,0,this.childCount.ToString());
			}
			else
			{
				frmScr_ScriptingCompleted(null, EventArgs.Empty);
			}
		}

		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AutoScriptMDI));
            this.statusBar1 = new System.Windows.Forms.StatusBar();
            this.statOverall = new System.Windows.Forms.StatusBarPanel();
            this.statRunningCount = new System.Windows.Forms.StatusBarPanel();
			this.mainMenu1 = new System.Windows.Forms.MenuStrip();
            this.menuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.statOverall)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.statRunningCount)).BeginInit();
            this.SuspendLayout();
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(0, 555);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Panels.AddRange(new System.Windows.Forms.StatusBarPanel[] {
            this.statOverall,
            this.statRunningCount});
            this.statusBar1.ShowPanels = true;
            this.statusBar1.Size = new System.Drawing.Size(1000, 22);
            this.statusBar1.TabIndex = 1;
            this.statusBar1.Text = "statusBar1";
            // 
            // statOverall
            // 
            this.statOverall.Name = "statOverall";
            this.statOverall.Width = 350;
            // 
            // statRunningCount
            // 
            this.statRunningCount.AutoSize = System.Windows.Forms.StatusBarPanelAutoSize.Spring;
            this.statRunningCount.Name = "statRunningCount";
            this.statRunningCount.Width = 633;
            // 
            // mainMenu1
            // 
            this.mainMenu1.Items.AddRange(new System.Windows.Forms.ToolStripMenuItem[] {
            this.menuItem1});
            // 
            // menuItem1
            // 
            //this.menuItem1.Index = 0;
            //this.menuItem1.IsMdiWindowListEntry = true;
            this.menuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripMenuItem[] {
            this.menuItem2});
            this.menuItem1.Text = "&Window";
            // 
            // menuItem2
            // 
            //this.menuItem2.Index = 0;
            this.menuItem2.Text = "&Cascade";
            this.menuItem2.Click += new System.EventHandler(this.menuItem2_Click);
            // 
            // AutoScriptMDI
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(1000, 577);
            this.Controls.Add(this.statusBar1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.IsMdiContainer = true;
            this.MainMenuStrip = this.mainMenu1;
            this.Name = "AutoScriptMDI";
            this.Text = "Sql Build Manager :: Auto Scripting";
            this.Load += new System.EventHandler(this.AutoScriptMDI_Load);
            this.Closing += new System.ComponentModel.CancelEventHandler(this.AutoScriptMDI_Closing);
            ((System.ComponentModel.ISupportInitialize)(this.statOverall)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.statRunningCount)).EndInit();
            this.ResumeLayout(false);

		}



		private void frmScr_ScriptingCompleted(object sender, EventArgs e)
		{
			this.completedCount++;
			statRunningCount.Text = String.Format(this.runStatus,this.completedCount.ToString(),this.childCount.ToString());

			if(this.completedCount >= this.childCount)
			{
				this.statOverall.Text = "Complete";

				if(this.config.PostScriptingAction != null && this.config.PostScriptingAction.Count > 0)
				{

					foreach(AutoScriptingConfig.PostScriptingActionRow row in this.config.PostScriptingAction)
					{
						this.statOverall.Text = "Running post script action: "+ row.Name;
						try
						{
							SqlSync.ProcessHelper prc = new SqlSync.ProcessHelper();
							prc.ExecuteProcess(row.Command, row.Arguments);
						
						}
						catch(Exception exe)
						{
							string debug = exe.ToString();
						}
					}
				}
				if(this.closeOnComplete)
					Application.Exit();
			}
		}

		private void AutoScriptMDI_Load(object sender, System.EventArgs e)
		{	this.Show();
			this.Process();
		}

		private void menuItem2_Click(object sender, System.EventArgs e)
		{
			this.LayoutMdi(MdiLayout.Cascade);
		}

		private void AutoScriptMDI_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if(this.completedCount != this.childCount)
			{
				if(DialogResult.No == MessageBox.Show("There are still scripting tasks running. Are you sure you want to exit?","Still Scripting",MessageBoxButtons.YesNo,MessageBoxIcon.Question))
				{
					e.Cancel = true;
				}
			}
		}
	}
}
