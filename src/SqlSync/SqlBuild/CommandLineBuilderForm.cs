using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.IO;
using SqlSync;
using SqlSync.Connection;

namespace SqlSync.SqlBuild
{
    public partial class CommandLineBuilderForm : Form
    {
        string sbmExeName = string.Empty;
        string tmpSbm = string.Empty;
        string tmpMultiDb = string.Empty;
        public CommandLineBuilderForm()
        {
            InitializeComponent();
        }
        public CommandLineBuilderForm(string sbmProjectFile) :this()
        {
            this.tmpSbm = string.IsNullOrEmpty(sbmProjectFile) ? string.Empty : sbmProjectFile;
        }
        public CommandLineBuilderForm(string sbmProjectFile, string multiDbFileName) : this(sbmProjectFile)
        {
            this.tmpMultiDb = string.IsNullOrEmpty(multiDbFileName) ? string.Empty : multiDbFileName;
        }
        private void btnLoggingPath_Click(object sender, EventArgs e)
        {

            this.folderBrowserDialog1.Description = "Root Logging Path";
            if(DialogResult.OK == this.folderBrowserDialog1.ShowDialog())
            {
                this.txtRootLoggingPath.Text = this.folderBrowserDialog1.SelectedPath;
            }
            this.folderBrowserDialog1.Dispose();
        }

        private void btnOpenSbm_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == this.fileSbm.ShowDialog())
            {
                this.txtSbmFile.Text = this.fileSbm.FileName;
            }
            this.fileSbm.Dispose();
        }

        private void btnScriptSrcDir_Click(object sender, EventArgs e)
        {
            this.folderBrowserDialog1.Description = "Script Source Directory (for loose scripts)";
            if (DialogResult.OK == this.folderBrowserDialog1.ShowDialog())
            {
                this.txtScriptSrcDir.Text = this.folderBrowserDialog1.SelectedPath;
            }
            this.folderBrowserDialog1.Dispose();
        }

        private void btnMultDbCfg_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == this.fileOverride.ShowDialog())
            {
                this.txtOverride.Text = this.fileOverride.FileName;
            }
            this.fileOverride.Dispose();
        }

        private void txtSbmFile_TextChanged(object sender, EventArgs e)
        {
            if (txtSbmFile.Text.Length > 0)
            {
                txtScriptSrcDir.Enabled = false;
                btnScriptSrcDir.Enabled = false;
            }
            else
            {
                txtScriptSrcDir.Enabled = true;
                btnScriptSrcDir.Enabled = true;
            }
        }
        private void ddAuthentication_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (ddAuthentication.SelectedItem.ToString() == Connection.AuthenticationType.AzureADPassword.GetDescription()
                || ddAuthentication.SelectedItem.ToString() == Connection.AuthenticationType.Password.GetDescription())
            {

                txtPassword.Enabled = true;
                txtUserName.Enabled = true;
            }
            else if (ddAuthentication.SelectedItem.ToString() == Connection.AuthenticationType.AzureADIntegrated.GetDescription()
                 || ddAuthentication.SelectedItem.ToString() == Connection.AuthenticationType.Windows.GetDescription())
            {
                txtPassword.Enabled = false;
                txtUserName.Enabled = false;
            }
        }
        private void btnConstructCommand_Click(object sender, EventArgs e)
        {
            string exePath = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\SqlBuildManager.Console.exe";
            StringBuilder sb = new StringBuilder();

            sb.Append("\"" + exePath + "\" ");
            this.sbmExeName = sb.ToString();
            sb.Append("/trial=\"" + chkRunTrial.Checked.ToString() + "\" ");

            if (chkRunThreaded.Checked)
            {
                sb.Append("/Action=Threaded");
            }

            if (chkRunThreaded.Checked)
            {
                if (ddLogStyle.SelectedItem.ToString().Equals("HTML"))
                    sb.Append("/LogAsText=\"false\" ");
                else
                    sb.Append("/LogAsText=\"true\" ");
            }

            if (ddAuthentication.SelectedItem.ToString() == AuthenticationType.Password.GetDescription())
            {
                sb.Append("/AuthType=Password");
            }
            else if (ddAuthentication.SelectedItem.ToString() == AuthenticationType.AzureADIntegrated.GetDescription())
            {
                sb.Append("/AuthType=AzureADIntegrated");
            }
            else if (ddAuthentication.SelectedItem.ToString() == AuthenticationType.AzureADPassword.GetDescription())
            {
                sb.Append("/AuthType=AzureADPassword");
            }
            else if (ddAuthentication.SelectedItem.ToString() == AuthenticationType.Windows.GetDescription())
            {
                sb.Append("/AuthType=Windows");
            }

            if ((ddAuthentication.SelectedItem.ToString() == "Username/Password" || ddAuthentication.SelectedItem.ToString() == "Azure AD Password Authentication")
                &&
                (txtUserName.Text.Length == 0 || txtPassword.Text.Length == 0))
            {
                MessageBox.Show("A User Name and Password are required when not using Windows Authentication", "Configuration error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if(!string.IsNullOrWhiteSpace(txtUserName.Text))
            {
                sb.Append("/username=\"" + txtUserName.Text + "\" ");
            }
            if (!string.IsNullOrWhiteSpace(txtPassword.Text))
            {
                sb.Append("/password=\"" + txtPassword.Text + "\" ");
            }

            if (txtLoggingDatabase.Text.Length > 0)
                sb.Append("/LogToDatabaseName=\"" + txtLoggingDatabase.Text + "\" ");

            if (txtSbmFile.Text.Length == 0 && txtScriptSrcDir.Text.Length == 0)
            {
                MessageBox.Show("A Sql Build Manager file or script source directory is required", "Configuration error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (txtSbmFile.Text.Length > 0)
                sb.Append("/PackageName=\"" + txtSbmFile.Text + "\" ");
            else
                sb.Append("/ScriptSrcDir=\"" + txtScriptSrcDir.Text + "\" ");

            if (chkRunThreaded.Checked && txtRootLoggingPath.Text.Length == 0)
            {
                MessageBox.Show("A Root logging path is required for a threaded build", "Logging error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            string root = (txtRootLoggingPath.Text.EndsWith(@"\")) ? txtRootLoggingPath.Text.Substring(0, txtRootLoggingPath.Text.Length - 1) : txtRootLoggingPath.Text;
            sb.Append("/RootLoggingPath=\"" + root + "\" ");



            if (txtOverride.Text.Length == 0)
            {
                MessageBox.Show("A Target Override is required for a threaded build", "Configuration error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            sb.Append("/override=\"" + txtOverride.Text + "\" ");

            if (txtDescription.Text.Length > 0)
                sb.Append("/description=\"" + txtDescription.Text + "\" ");

            if (chkNotTransactional.Checked)
                sb.Append("/transactional=false ");

            int allowedTimeoutRetries = 0;
            if (!Int32.TryParse(txtAllowedTimeoutRetries.Text,out allowedTimeoutRetries))
            {
                MessageBox.Show("The \"Allowed Script Timeout Retry Count\" value must be an integer ", "Configuration error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            sb.Append("/TimeoutRetryCount=" + allowedTimeoutRetries.ToString() + " ");

            if (txtDescription.Text.Length > 0 && !SqlSync.Properties.Settings.Default.Description.Contains(txtDescription.Text))
                SqlSync.Properties.Settings.Default.Description.Add(txtDescription.Text);

            if (txtSbmFile.Text.Length > 0 && !SqlSync.Properties.Settings.Default.CmdLineSbmFileNameList.Contains(txtSbmFile.Text))
                SqlSync.Properties.Settings.Default.CmdLineSbmFileNameList.Add(txtSbmFile.Text);


            if (txtRootLoggingPath.Text.Length > 0 && !SqlSync.Properties.Settings.Default.CmdLineRootLoggingPath.Contains(txtRootLoggingPath.Text))
                SqlSync.Properties.Settings.Default.CmdLineRootLoggingPath.Add(txtRootLoggingPath.Text);

            if (txtOverride.Text.Length > 0 && !SqlSync.Properties.Settings.Default.CmdLineMultiDbFileNameList.Contains(txtOverride.Text))
                SqlSync.Properties.Settings.Default.CmdLineMultiDbFileNameList.Add(txtOverride.Text);

            SqlSync.Properties.Settings.Default.Save();

            rtbCommandLine.Text = sb.ToString();
        }

        private void chkRunThreaded_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRunThreaded.Checked)
                grpLogging.Enabled = true;
            else
                grpLogging.Enabled = false;
        }

        private void CommandLineBuilderForm_Load(object sender, EventArgs e)
        {
            if(tmpSbm.Length > 0)
                txtSbmFile.Text = tmpSbm;

            if (tmpMultiDb.Length > 0)
                txtOverride.Text = tmpMultiDb;

            ddLogStyle.SelectedIndex = 0;

            if (SqlSync.Properties.Settings.Default.CmdLineMultiDbFileNameList == null) SqlSync.Properties.Settings.Default.CmdLineMultiDbFileNameList = new AutoCompleteStringCollection();
            if (SqlSync.Properties.Settings.Default.CmdLineRootLoggingPath == null) SqlSync.Properties.Settings.Default.CmdLineRootLoggingPath = new AutoCompleteStringCollection();
            if (SqlSync.Properties.Settings.Default.CmdLineSbmFileNameList == null) SqlSync.Properties.Settings.Default.CmdLineSbmFileNameList = new AutoCompleteStringCollection();
            if (SqlSync.Properties.Settings.Default.Description == null) SqlSync.Properties.Settings.Default.Description = new AutoCompleteStringCollection();

            txtOverride.AutoCompleteCustomSource = SqlSync.Properties.Settings.Default.CmdLineMultiDbFileNameList;
            txtRootLoggingPath.AutoCompleteCustomSource = SqlSync.Properties.Settings.Default.CmdLineRootLoggingPath;
            txtSbmFile.AutoCompleteCustomSource = SqlSync.Properties.Settings.Default.CmdLineSbmFileNameList;
            txtDescription.AutoCompleteCustomSource = SqlSync.Properties.Settings.Default.Description;

            chkRunThreaded.Checked = true;
            var vals = Enum.GetValues(typeof(Connection.AuthenticationType));
            foreach (Connection.AuthenticationType item in Enum.GetValues(typeof(Connection.AuthenticationType)))
            {
                ddAuthentication.Items.Add(item.GetDescription());
            }
            ddAuthentication.SelectedIndex = 0;
            ddAuthentication_SelectionChangeCommitted(null, null);
        }

        private void btnExecute_Click(object sender, EventArgs e)
        {
            
            System.Diagnostics.Process prc = new System.Diagnostics.Process();
            prc.StartInfo.FileName = this.sbmExeName.Replace('"',' ').Trim();
            prc.StartInfo.Arguments = rtbCommandLine.Text.Replace(this.sbmExeName, "");
           // prc.StartInfo.Arguments =  rtbCommandLine.Text;
            prc.StartInfo.RedirectStandardOutput = true;
            prc.StartInfo.RedirectStandardError = true;
            prc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
            prc.StartInfo.UseShellExecute = false;
            prc.Start();

            rtbOutput.Text = prc.StandardOutput.ReadToEnd() + "\r\n";
            rtbOutput.Text += prc.StandardError.ReadToEnd();
            pnlOutput.Enabled = true;
            if (this.txtRootLoggingPath.Text.Length == 0)
                this.btnOpenFolder.Enabled = false;
        }

        private void rtbCommandLine_TextChanged(object sender, EventArgs e)
        {
            if (rtbCommandLine.Text.Length > 0)
                btnExecute.Enabled = true;
            else
                btnExecute.Enabled = false;
        }

        private void btnOpenFolder_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process prc = new System.Diagnostics.Process();
            prc.StartInfo.FileName = "explorer";
            prc.StartInfo.Arguments = this.txtRootLoggingPath.Text;
            prc.Start();
        }

        private void chkNoTransaction_CheckedChanged(object sender, EventArgs e)
        {
            if (this.chkNotTransactional.Checked)
            {
                string message = "WARNING!\r\nBy checking this box, you are disabling the transaction handling of Sql Build Manager.\r\nIn the event of a script failure, your scripts will NOT be rolled back\r\nand your databases will be left in an inconsistent state!\r\n\r\nAre you certain you want this checked?";
                if (DialogResult.No == MessageBox.Show(message, "Are you sure you want this?", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2))
                {
                    this.chkNotTransactional.Checked = false;
                    return;
                }

                if (chkRunTrial.Checked)
                {
                    this.chkNotTransactional.Checked = false;
                    message = "You can not have a Trial run without transactions! Please uncheck the \"Run As Trial\" checkbox then re-check the transaction box";
                    MessageBox.Show(message, "Invalid Build/Transaction combination", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
            }
        }

        private void helptoolStripMenuItem_Click(object sender, EventArgs e)
        {
            SqlSync.SqlBuild.UtilityHelper.OpenManual("AutoCreationofCommandLineStatements");
        }

    }
}
