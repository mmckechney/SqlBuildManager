using SqlSync.ObjectScript;
using System;
using System.Windows.Forms;
namespace SqlSync.AutoScript
{
    /// <summary>
    /// Summary description for AutoScriptHelper.
    /// </summary>
    public class AutoScriptMDI : Form
    {
        private string configFile = string.Empty;
        private System.Windows.Forms.StatusStrip statusBar1;
        private System.Windows.Forms.ToolStripStatusLabel statOverall;
        private System.Windows.Forms.ToolStripStatusLabel statRunningCount;
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
            if (!closeOnComplete)
                Text += " (Remain open on Complete)";
            else
                Text += " (Close on Complete)";
        }
        public AutoScriptMDI(string configFile)
        {
            InitializeComponent();
            this.configFile = configFile;
            closeOnComplete = true;
            Text += " (Close on Complete)";
        }

        private void Process()
        {
            config = new AutoScriptingConfig();
            config.ReadXml(configFile);
            if (config.AutoScripting[0].AllowManualSelection)
            {
                AutoScriptingSelect frmSelect = new AutoScriptingSelect(config);
                DialogResult result = frmSelect.ShowDialog();
                if (result == DialogResult.OK)
                    config = frmSelect.config;
                else
                    return;
            }

            if (config.DatabaseScriptConfig.Rows.Count > 0)
            {
                for (int i = 0; i < config.DatabaseScriptConfig.Rows.Count; i++)
                {
                    childCount++;
                    AutoScripting frmScr = new AutoScripting(config.DatabaseScriptConfig[i], config.AutoScripting[0].IncludeFileHeaders, config.AutoScripting[0].DeletePreExistingFiles, config.AutoScripting[0].ZipScripts);
                    frmScr.ScriptingCompleted += new EventHandler(frmScr_ScriptingCompleted);
                    frmScr.MdiParent = this;
                    frmScr.Show();
                }
                statOverall.Text = "Scripting";
                statRunningCount.Text = String.Format(runStatus, 0, childCount.ToString());
            }
            else
            {
                frmScr_ScriptingCompleted(null, EventArgs.Empty);
            }
        }

        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AutoScriptMDI));
            statusBar1 = new System.Windows.Forms.StatusStrip();
            statOverall = new System.Windows.Forms.ToolStripStatusLabel();
            statRunningCount = new System.Windows.Forms.ToolStripStatusLabel();
            mainMenu1 = new System.Windows.Forms.MenuStrip();
            menuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            menuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            SuspendLayout();
            // 
            // statusBar1
            // 
            statusBar1.Location = new System.Drawing.Point(0, 555);
            statusBar1.Name = "statusBar1";
            statusBar1.Items.AddRange(new System.Windows.Forms.ToolStripStatusLabel[] {
            statOverall,
            statRunningCount});
            //this.statusBar1.ShowPanels = true;
            statusBar1.Size = new System.Drawing.Size(1000, 22);
            statusBar1.TabIndex = 1;
            statusBar1.Text = "statusBar1";
            // 
            // statOverall
            // 
            statOverall.Name = "statOverall";
            statOverall.Width = 350;
            // 
            // statRunningCount
            // 
            statRunningCount.AutoSize = true;
            statRunningCount.Spring = true;
            statRunningCount.Name = "statRunningCount";
            statRunningCount.Width = 633;
            // 
            // mainMenu1
            // 
            mainMenu1.Items.AddRange(new System.Windows.Forms.ToolStripMenuItem[] {
            menuItem1});
            // 
            // menuItem1
            // 
            //this.menuItem1.Index = 0;
            //this.menuItem1.IsMdiWindowListEntry = true;
            menuItem1.DropDownItems.AddRange(new System.Windows.Forms.ToolStripMenuItem[] {
            menuItem2});
            menuItem1.Text = "&Window";
            // 
            // menuItem2
            // 
            //this.menuItem2.Index = 0;
            menuItem2.Text = "&Cascade";
            menuItem2.Click += new System.EventHandler(menuItem2_Click);
            // 
            // AutoScriptMDI
            // 
            AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            ClientSize = new System.Drawing.Size(1000, 577);
            Controls.Add(statusBar1);
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            IsMdiContainer = true;
            MainMenuStrip = mainMenu1;
            Name = "AutoScriptMDI";
            Text = "Sql Build Manager :: Auto Scripting";
            Load += new System.EventHandler(AutoScriptMDI_Load);
            Closing += new System.ComponentModel.CancelEventHandler(AutoScriptMDI_Closing);
            ResumeLayout(false);

        }



        private void frmScr_ScriptingCompleted(object sender, EventArgs e)
        {
            completedCount++;
            statRunningCount.Text = String.Format(runStatus, completedCount.ToString(), childCount.ToString());

            if (completedCount >= childCount)
            {
                statOverall.Text = "Complete";

                if (config.PostScriptingAction != null && config.PostScriptingAction.Count > 0)
                {

                    foreach (AutoScriptingConfig.PostScriptingActionRow row in config.PostScriptingAction)
                    {
                        statOverall.Text = "Running post script action: " + row.Name;
                        try
                        {
                            SqlSync.ProcessHelper prc = new SqlSync.ProcessHelper();
                            prc.ExecuteProcess(row.Command, row.Arguments);

                        }
                        catch (Exception exe)
                        {
                            string debug = exe.ToString();
                        }
                    }
                }
                if (closeOnComplete)
                    Application.Exit();
            }
        }

        private void AutoScriptMDI_Load(object sender, System.EventArgs e)
        {
            Show();
            Process();
        }

        private void menuItem2_Click(object sender, System.EventArgs e)
        {
            LayoutMdi(MdiLayout.Cascade);
        }

        private void AutoScriptMDI_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (completedCount != childCount)
            {
                if (DialogResult.No == MessageBox.Show("There are still scripting tasks running. Are you sure you want to exit?", "Still Scripting", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                {
                    e.Cancel = true;
                }
            }
        }
    }
}
