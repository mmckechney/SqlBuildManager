using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SqlBuildManager.ServiceClient;
using SqlBuildManager.ServiceClient.Sbm.BuildService;
using SqlSync.SqlBuild.MultiDb;
using System.IO;
using System.Runtime.Serialization;
using SqlSync.Connection;
using SqlSync.DbInformation;
using System.Xml.Serialization;

namespace SqlSync.SqlBuild.Remote
{
    public partial class RemoteServiceForm : Form
    {
        bool packageSubmitted = false;
        BuildServiceManager buildManager = new BuildServiceManager(Protocol.Tcp);
        BindingList<ServerConfigData> serverData = new BindingList<ServerConfigData>();
        private DistributionType loadDistributionType = DistributionType.EqualSplit;

        private ConnectionData connData;
        private DatabaseList dbList;
        public RemoteServiceForm(ConnectionData connData, DatabaseList dbList)
        {
            InitializeComponent();
            this.connData = connData;
            this.dbList = dbList;
        }
        public RemoteServiceForm(ConnectionData connData, DatabaseList dbList, string sbmFilePath)
            : this(connData, dbList)
        {
            this.txtSbmFile.Text = sbmFilePath;
        }

        private void btnCheckServiceStatus_Click(object sender, EventArgs e)
        {
            if (!bgStatusCheck.IsBusy)
            {
                List<string> servers = GetExecutionServerListFromGrid();
                bgStatusCheck.RunWorkerAsync(servers);
            }

        }
        private List<string> GetExecutionServerListFromGrid()
        {
            List<string> servers = new List<string>();
            foreach (DataGridViewRow row in this.dgvRemoteServers.Rows)
            {
                if (row.Cells[0].Value != null && row.Cells[0].Value.ToString().Trim().Length > 0)
                    servers.Add(row.Cells[0].Value.ToString().Trim());
            }
            return servers;
        }
        private bool ValidateRequest()
        {
            if (txtRootLoggingPath.Text.Length == 0 || txtSbmFile.Text.Length == 0 || txtOverride.Text.Length == 0 || txtDescription.Text.Length == 0)
            {
                MessageBox.Show("Valid entries are required for:\r\n\r\n\tRoot Logging Path\r\n\tBuild Description\r\n\tSql Build Manager Package\r\n\tTarget Override Settings\r\n\r\nPlease make sure you have entries for each and try again.", "Missing Values", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }

            if (!File.Exists(txtSbmFile.Text))
            {
                MessageBox.Show("The \"Sql Build Manager Package\" value must point to a valid, accessable build package.\r\nPlease confirm your value and try again.", "Invalid sbm path", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }

            if (!File.Exists(txtSbmFile.Text))
            {
                MessageBox.Show("The \"Target Override Settings\" value must point to a valid, accessable configuration file.\r\nPlease confirm your value and try again.", "Invalid target override file path", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return false;
            }

            if (this.serverData == null || this.serverData.Count == 0)
            {
                MessageBox.Show("Please add execution servers and check their service status prior to submitting your build.", "Not so fast!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return false;
            }
            else
            {
                IEnumerable<bool> allReady = (from s in this.serverData where s.ServiceReadiness != ServiceReadiness.ReadyToAccept select false);
                if (allReady.Count() > 0)
                {
                    MessageBox.Show("One or more Remote Execution Servers last reported a status other than \"ReadyToAccept\".\r\nPlease remove this server from the list or correct the issue with the service.", "Not ready!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return false;
                }
            }
            return true;
        }
        private bool AcceptLoadDistribution(List<string> lstUntaskedExecutionServers, List<string> lstUnassignedDatabaseServers)
        {
            StringBuilder sb = new StringBuilder();
            if (lstUnassignedDatabaseServers.Count > 0)
            {
                sb.AppendLine("Based on your workload distribution, the following database servers will not have their databases updated:");
                sb.AppendLine("\t" + String.Join("\r\n\t", lstUnassignedDatabaseServers.ToArray()));
            }

            if (lstUntaskedExecutionServers.Count > 0)
            {
                if (sb.Length > 0) sb.AppendLine("\r\n");
                sb.AppendLine("Based on your workload distribution, the following execution servers will not be tasked with work:");
                sb.AppendLine("\t" + String.Join("\r\n\t", lstUntaskedExecutionServers.ToArray()));
            }
            if (sb.Length > 0)
            {
                sb.AppendLine("\r\nDo you want to continue?");
                if (DialogResult.No == MessageBox.Show(sb.ToString(), "Uneven or Incomplete workload distribution!", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2))
                    return false;
            }
            return true;
        }
        private DistributionType GetDistributionType()
        {
            if (ddDistribution.SelectedItem.ToString().StartsWith("Each execution"))
                return DistributionType.OwnMachineName;
            else
                return DistributionType.EqualSplit;

        }
        private string[] GetMultiDbConfigLines()
        {
            if (txtOverride.Text.ToLower().EndsWith(".multidb"))
            {
                MultiDbData multiDb = MultiDbHelper.DeserializeMultiDbConfiguration(txtOverride.Text);
                string cfg = MultiDbHelper.ConvertMultiDbDataToTextConfig(multiDb);
                return cfg.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            }
            else if (txtOverride.Text.ToLower().EndsWith(".cfg"))
            {
                return File.ReadAllText(txtOverride.Text).Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries); //would use ReadAllLines but want to make sure we remove empties.
            }
            else if (txtOverride.Text.ToLower().EndsWith(".multidbq"))
            {
                string message;
                MultiDbData dbData = MultiDbHelper.CreateMultiDbConfigFromQuery(txtOverride.Text, out message);
                if (dbData == null)
                {
                    MessageBox.Show(message, "Error creating configuration", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return new string[0];
                }
                string cfg = MultiDbHelper.ConvertMultiDbDataToTextConfig(dbData);
                return cfg.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            }
            return new string[0];
        }
        private void btnSubmitPackage_Click(object sender, EventArgs e)
        {

            if (!ValidateRequest())
                return;

            List<string> lstUnassignedDatabaseServers, lstUntaskedExecutionServers;
            BuildSettings settings = CompileBuildSettings(out lstUntaskedExecutionServers, out lstUnassignedDatabaseServers);

            if (settings == null)
                return;

            if (!AcceptLoadDistribution(lstUntaskedExecutionServers, lstUnassignedDatabaseServers))
                return;


            this.packageSubmitted = true;
            bgSubmit.RunWorkerAsync(settings);
            //System.Threading.Thread.Sleep(1500); // Sleep for 1.5 seconds for the packages to get sent...
            tmrCheckStatus.Start();
            btnSubmitPackage.Enabled = false;

            if (txtOverride.Text.Length > 0 && !SqlSync.Properties.Settings.Default.RemoteTargetOverrideSettings.Contains(txtOverride.Text))
                SqlSync.Properties.Settings.Default.RemoteTargetOverrideSettings.Add(txtOverride.Text);

            if (txtDescription.Text.Length > 0 && !SqlSync.Properties.Settings.Default.RemoteBuildDescription.Contains(txtDescription.Text))
                SqlSync.Properties.Settings.Default.RemoteBuildDescription.Add(txtDescription.Text);

            if (txtRootLoggingPath.Text.Length > 0 && !SqlSync.Properties.Settings.Default.RemoteLoggingPath.Contains(txtRootLoggingPath.Text))
                SqlSync.Properties.Settings.Default.RemoteLoggingPath.Add(txtRootLoggingPath.Text);

            if (txtUserName.Text.Length > 0 && !SqlSync.Properties.Settings.Default.RemoteUsername.Contains(txtUserName.Text))
                SqlSync.Properties.Settings.Default.RemoteUsername.Add(txtUserName.Text);

            SqlSync.Properties.Settings.Default.Save();


        }

        private void btnOpenSbm_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == this.fileSbm.ShowDialog())
            {
                this.txtSbmFile.Text = this.fileSbm.FileName;
            }
            this.fileSbm.Dispose();
        }

        private void btnMultDbCfg_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == this.fileOverride.ShowDialog())
            {
                this.txtOverride.Text = this.fileOverride.FileName;
            }
            this.fileOverride.Dispose();
        }

        private void bgStatusCheck_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bg = (BackgroundWorker)sender;
            List<string> servers = (List<string>)e.Argument;

            bg.ReportProgress(0, "Connecting to remote services. Checking Status...");

            buildManager.SetServerNames(servers);
            e.Result = buildManager.GetServiceStatus();

        }
        private void bgStatusCheck_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0)
            {
                if (this.statProgBar.Style != ProgressBarStyle.Marquee)
                    this.statProgBar.Style = ProgressBarStyle.Marquee;
                this.Cursor = Cursors.AppStarting;
            }

            if (e.UserState is string)
                this.statGeneral.Text = e.UserState.ToString();
        }
        private void bgStatusCheck_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result is IList<ServerConfigData>)
            {
                this.serverData = (BindingList<ServerConfigData>)e.Result;
                this.dgvServerStatus.DataSource = this.serverData;
                IEnumerable<ServerConfigData> complete = from s in this.serverData
                                                         where
                                                             s.ServiceReadiness == ServiceReadiness.Error || s.ServiceReadiness == ServiceReadiness.ReadyToAccept ||
                                                             s.ServiceReadiness == ServiceReadiness.ProcessingCompletedSuccessfully || s.ServiceReadiness == ServiceReadiness.Unknown
                                                         select s;

                if (complete.Count<ServerConfigData>() == this.serverData.Count)
                {
                    this.statProgBar.Style = ProgressBarStyle.Blocks;
                    this.Cursor = Cursors.Default;
                    this.statGeneral.Text = "Ready.";
                    this.packageSubmitted = false;
                    btnSubmitPackage.Enabled = true;
                }
                this.dgvServerStatus.Invalidate();
            }

        }
        
        private void bgSubmit_DoWork(object sender, DoWorkEventArgs e)
        {
            try
            {
                BackgroundWorker bg = (BackgroundWorker)sender;
                BuildSettings settings = (BuildSettings)e.Argument;
                bg.ReportProgress(0, "Submitting build package for execution...");
                this.packageSubmitted = true;
                buildManager.SubmitBuildRequest(settings, this.loadDistributionType);
            }
            catch
            {
                this.packageSubmitted = false;
            }
        }
        private void bgSubmit_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0)
            {
                if (this.statProgBar.Style != ProgressBarStyle.Marquee)
                    this.statProgBar.Style = ProgressBarStyle.Marquee;
                this.Cursor = Cursors.AppStarting;
            }

            if (e.UserState is string)
                this.statGeneral.Text = e.UserState.ToString();
        }
        private void bgSubmit_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            //this.statProgBar.Style = ProgressBarStyle.Blocks;
            //this.Cursor = Cursors.Default;
            this.statGeneral.Text = "Execution Submitted.";

            // tmrCheckStatus.Stop();

            // if (!bgWorker.IsBusy)
            //     btnCheckServiceStatus_Click(null, EventArgs.Empty);

            //btnSubmitPackage.Enabled = true;
        }

        private void bgConnectionTest_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bg = (BackgroundWorker)sender;
            bg.ReportProgress(0, "Testing database connectivity");
            BuildSettings settings = (BuildSettings)e.Argument;
            IList<ServerConfigData> tmpCfgData = buildManager.TestDatabaseConnectivity(settings, this.loadDistributionType);
            e.Result = tmpCfgData;
        }
        private void bgConnectionTest_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0)
            {
                if (this.statProgBar.Style != ProgressBarStyle.Marquee)
                    this.statProgBar.Style = ProgressBarStyle.Marquee;
                this.Cursor = Cursors.AppStarting;
            }

            if (e.UserState is string)
                this.statGeneral.Text = e.UserState.ToString();
        }
        private void bgConnectionTest_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.statProgBar.Style = ProgressBarStyle.Blocks;
            this.Cursor = Cursors.Default;

            if (e.Result is IList<ServerConfigData>)
            {
                this.serverData =  new BindingList<ServerConfigData>((IList<ServerConfigData>)e.Result);
                 this.dgvServerStatus.DataSource = this.serverData;

                var err = from s in this.serverData 
                          from c in s.ConnectionTestResults
                          where s.ConnectionTestResults.Count == 0 || c.Successful == false
                          select s;

                if (err.Any())
                {
                    var list = from er in err
                               from c in er.ConnectionTestResults
                               select er.ServerName +" :: "+ c.ServerName +"."+ c.DatabaseName;
                               
                    SimpleListForm frmSimple = new SimpleListForm(list.ToList<string>(),"Connection errors found with the following databases:", "Execution Server :: DB Server.Database");
                    frmSimple.ShowDialog();
                }
                else
                {
                    MessageBox.Show("All connections tested successfully!", "Success!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
   
            }
        }

        //private void viewExecutionCommitsLogToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    if (this.dgvServerStatus.SelectedCells.Count == 0)
        //        return;
        //    try
        //    {
        //        ServerConfigData server = (ServerConfigData)this.dgvServerStatus.Rows[this.dgvServerStatus.SelectedCells[0].RowIndex].DataBoundItem;
        //        string text = buildManager.GetCommitsLog(server.TcpServiceEndpoint);
        //        SqlSync.ScriptDisplayForm frmScript = new ScriptDisplayForm(text, server.ServerName, "Commits Log from " + server.ServerName, SqlSync.Highlighting.SyntaxHightlightType.LogFile);
        //        frmScript.Show();
        //    }
        //    catch (System.ServiceModel.CommunicationObjectFaultedException comExe)
        //    {
        //        MessageBox.Show("There was a communication issue retrieving the log file.\r\nTo try to clear the communication error, try closing this page and reopening. If that does not work, you may need to restart the service on the target remote execution server.\r\n\r\nError Message:\r\n" + comExe.Message, "Whoops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //    catch (Exception exe)
        //    {
        //        MessageBox.Show("There was an issue retrieving the log file.\r\n" + exe.Message, "Whoops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}

        //private void viewExecutionErrorsLogToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    if (this.dgvServerStatus.SelectedCells.Count == 0)
        //        return;
        //    try
        //    {
        //        ServerConfigData server = (ServerConfigData)this.dgvServerStatus.Rows[this.dgvServerStatus.SelectedCells[0].RowIndex].DataBoundItem;
        //        string text = buildManager.GetErrorsLog(server.TcpServiceEndpoint);
        //        SqlSync.ScriptDisplayForm frmScript = new ScriptDisplayForm(text, server.ServerName, "Errors Log from " + server.ServerName, SqlSync.Highlighting.SyntaxHightlightType.LogFile);
        //        frmScript.Show();          
        //    }
        //    catch (System.ServiceModel.CommunicationObjectFaultedException comExe)
        //    {
        //        MessageBox.Show("There was a communication issue retrieving the log file.\r\nTo try to clear the communication error, try closing this page and reopening. If that does not work, you may need to restart the service on the target remote execution server.\r\n\r\nError Message:\r\n" + comExe.Message, "Whoops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //    catch (Exception exe)
        //    {
        //        MessageBox.Show("There was an issue retrieving the log file.\r\n" + exe.Message, "Whoops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //    }
        //}

        // private void viewRemoteServiceExecutableLogFileToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    if (this.dgvServerStatus.SelectedCells.Count == 0)
        //        return;

        //    this.Cursor = Cursors.WaitCursor;
        //    this.statGeneral.Text = "Retrieving Remote Service Executable log file...";
        //    this.statProgBar.Style = ProgressBarStyle.Marquee;
        //    try
        //    {
        //        ServerConfigData server = (ServerConfigData)this.dgvServerStatus.Rows[this.dgvServerStatus.SelectedCells[0].RowIndex].DataBoundItem;

        //        string text = buildManager.GetServiceLog(server.TcpServiceEndpoint);
        //        SqlSync.ScriptDisplayForm frmScript = new ScriptDisplayForm(text, server.ServerName, "Service Executable Log from " + server.ServerName, SqlSync.Highlighting.SyntaxHightlightType.LogFile);
        //        frmScript.Show();
        //    }
        //    catch (Exception exe)
        //    {
        //        MessageBox.Show("Sorry... There was an error attempting to retrieve the log file.\r\n " + exe.Message, "Problem!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        return;

        //    }
        //    finally
        //    {
        //        this.Cursor = Cursors.Default;
        //        this.statGeneral.Text = "Ready.";
        //        this.statProgBar.Style = ProgressBarStyle.Blocks;
        //    }
        //}
        //private void txtDetailedLogTarget_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Enter)
        //    {
        //        if (this.dgvServerStatus.SelectedCells.Count == 0 || txtDetailedLogTarget.Text.Length == 0)
        //            return;
        //        try
        //        {
        //            ServerConfigData server = (ServerConfigData)this.dgvServerStatus.Rows[this.dgvServerStatus.SelectedCells[0].RowIndex].DataBoundItem;
        //            string text = buildManager.GetDetailedDatabaseLog(server.TcpServiceEndpoint, txtDetailedLogTarget.Text);
        //            SqlSync.ScriptDisplayForm frmScript = new ScriptDisplayForm(text, server.ServerName, "Detailed Log from " + txtDetailedLogTarget.Text);
        //            frmScript.Show();

        //            txtDetailedLogTarget.Text = string.Empty;
        //        }
        //        catch (Exception exe)
        //        {
        //            MessageBox.Show("There was an issue retrieving the log file.\r\n" + exe.Message, "Whoops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
        //        }
        //    }
        //}

        private void RemoteServiceForm_Load(object sender, EventArgs e)
        {
            ddDistribution.SelectedIndex = 0;

            if (SqlSync.Properties.Settings.Default.RemoteBuildDescription == null)
                SqlSync.Properties.Settings.Default.RemoteBuildDescription = new AutoCompleteStringCollection();

            txtDescription.AutoCompleteCustomSource = SqlSync.Properties.Settings.Default.RemoteBuildDescription;

            if (SqlSync.Properties.Settings.Default.RemoteLoggingPath == null)
                SqlSync.Properties.Settings.Default.RemoteLoggingPath = new AutoCompleteStringCollection();

            txtRootLoggingPath.AutoCompleteCustomSource = SqlSync.Properties.Settings.Default.RemoteLoggingPath;

            if (SqlSync.Properties.Settings.Default.RemoteTargetOverrideSettings == null)
                SqlSync.Properties.Settings.Default.RemoteTargetOverrideSettings = new AutoCompleteStringCollection();

            txtOverride.AutoCompleteCustomSource = SqlSync.Properties.Settings.Default.RemoteTargetOverrideSettings;

            if (SqlSync.Properties.Settings.Default.RemoteUsername == null)
                SqlSync.Properties.Settings.Default.RemoteUsername = new AutoCompleteStringCollection();

            txtUserName.AutoCompleteCustomSource = SqlSync.Properties.Settings.Default.RemoteUsername;

            ddDistribution.SelectedIndex = 1;
            protocolComboBox.SelectedIndex = 0;



        }

        private void manageServerSetsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ExecutionServerForm frmServers = new ExecutionServerForm();
            if (DialogResult.OK == frmServers.ShowDialog())
            {
                dgvRemoteServers.Rows.Clear();
                BindingList<string> servers = frmServers.SelectedExecutionServers;
                foreach (string s in servers)
                    dgvRemoteServers.Rows.Add(s);

                dgvRemoteServers.Invalidate();
            }
            frmServers.Dispose();
        }

        private void txtOverride_TextChanged(object sender, EventArgs e)
        {
            if (txtOverride.Text.Length > 0)
            {
                btnPreview.Enabled = true;
                if (chkUseOverrideAsExeList.Checked)
                {
                    //Clear it out..
                    chkUseOverrideAsExeList.Checked = false;
                    dgvRemoteServers.Rows.Clear();
                    dgvServerStatus.Rows.Clear();

                    //recheck to fire events again
                    chkUseOverrideAsExeList.Checked = true;
                }
            }
            else
                btnPreview.Enabled = false;
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            //Make sure there is an override setting!
            if (txtOverride.Text.Length == 0)
            {
                MessageBox.Show("No override settings configuration has been specified. Please enter an appropriate file name.  ", "Missing 'Override Target Settings'", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }



            //What execution servers to use... defined or inferred.
            if (this.serverData == null || this.serverData.Count == 0)
            {
                MessageBox.Show("Please add execution servers and check their service status prior to previewing load distribution", "Not so fast!", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            DistributionType distType = GetDistributionType();

            //Get the configuration lines...
            string[] multiDbConfigLines;
            try
            {
                multiDbConfigLines = RemoteHelper.GetMultiDbConfigLinesArray(txtOverride.Text);
            }
            catch (MultiDbConfigurationException exe)
            {
                MessageBox.Show(exe.Message, "Error");
                return;
            }

            ExecutionDistributionForm frmDist = new ExecutionDistributionForm(distType, multiDbConfigLines, this.serverData.ToList());
            frmDist.ShowDialog();

        }

        private void lblOpenConfigForm_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            MultiDbRunForm frmMult = new MultiDbRunForm(); //todo: open the file that is set....
            frmMult.ShowDialog();

            if (frmMult.LoadedFileName != null && frmMult.LoadedFileName.Length > 0)
                txtOverride.Text = frmMult.LoadedFileName;

            frmMult.Dispose();
        }

        private void lblCreateViaQuery_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            ConfigurationViaQueryForm frmQ = new ConfigurationViaQueryForm(this.connData, this.dbList, txtOverride.Text, true);
            frmQ.ShowDialog();
            if (frmQ.SavedMultiDbQFile != null && frmQ.SavedMultiDbQFile.Length > 0)
                txtOverride.Text = frmQ.SavedMultiDbQFile;

            frmQ.Dispose();

        }

        private void helptoolStripMenuItem_Click(object sender, EventArgs e)
        {
            SqlSync.Utility.OpenManual("RemoteServicsExecutionandDeployment");
        }

        private BuildSettings CompileBuildSettings(out List<string> untaskedExecutionServers, out List<string> unassignedDatabaseServers)
        {

            try
            {
                BuildSettings settings = new BuildSettings();

                try
                {
                    settings.MultiDbTextConfig = GetMultiDbConfigLines();
                }
                catch (MultiDbConfigurationException exe)
                {
                    MessageBox.Show(exe.Message, "Error");
                    untaskedExecutionServers = new List<string>();
                    unassignedDatabaseServers = new List<string>();
                    return null;
                }

                this.loadDistributionType = GetDistributionType();

                buildManager.ValidateLoadDistribution(this.loadDistributionType, this.serverData.ToList(), settings.MultiDbTextConfig, out untaskedExecutionServers, out unassignedDatabaseServers);

                settings.BuildRunGuid = Guid.NewGuid().ToString();
                settings.IsTransactional = !chkNotTransactional.Checked;
                settings.IsTrialBuild = chkRunTrial.Checked;
                settings.LocalRootLoggingPath = txtRootLoggingPath.Text;
                settings.SqlBuildManagerProjectContents = System.IO.File.ReadAllBytes(txtSbmFile.Text);
                settings.SqlBuildManagerProjectFileName = Path.GetFileName(txtSbmFile.Text);
                settings.Description = txtDescription.Text;
                settings.AlternateLoggingDatabase = txtLoggingDatabase.Text;
                int retryCnt;
                int.TryParse(txtTimeoutRetryCount.Text, out retryCnt);
                settings.TimeoutRetryCount = retryCnt;

                return settings;
            }
            catch (Exception)
            {
                untaskedExecutionServers = new List<string>();
                unassignedDatabaseServers = new List<string>();
                return null;
            }
        }

        private bool DecompileBuildSettings(BuildSettings settings)
        {

            try
            {
                string baseFileName = Path.GetFileNameWithoutExtension(settings.SqlBuildManagerProjectFileName);

                //Save temp .cfg file
                string cfgFileName = Path.GetTempPath() + baseFileName + ".cfg";
                File.WriteAllLines(cfgFileName, settings.MultiDbTextConfig, Encoding.UTF8);
                txtOverride.Text = cfgFileName;

                //Save temp .sbm file
                string sbmFileName = Path.GetTempPath() + baseFileName + ".sbm";
                File.WriteAllBytes(sbmFileName, settings.SqlBuildManagerProjectContents);
                txtSbmFile.Text = sbmFileName;

                dgvRemoteServers.Rows.Clear();
                dgvServerStatus.DataSource = null;
                dgvServerStatus.Rows.Clear();

                if (settings.RemoteExecutionServers.Count == 1 && settings.RemoteExecutionServers[0].ServerName.ToLower() == "derive")
                {
                    if (chkUseOverrideAsExeList.Checked)
                        chkUseOverrideAsExeList_CheckedChanged(null, EventArgs.Empty);
                    else
                        chkUseOverrideAsExeList.Checked = true;
                }
                else
                {
                    foreach (ServerConfigData exeServer in settings.RemoteExecutionServers)
                    {
                        dgvRemoteServers.Rows.Add(exeServer.ServerName);
                    }
                }
                chkNotTransactional.Checked = !settings.IsTransactional;
                chkRunTrial.Checked = settings.IsTrialBuild;
                txtRootLoggingPath.Text = settings.LocalRootLoggingPath;
                txtDescription.Text = settings.Description;
                txtLoggingDatabase.Text = settings.AlternateLoggingDatabase;

                if (settings.DistributionType == DistributionType.EqualSplit)
                    ddDistribution.SelectedIndex = 0;
                else
                    ddDistribution.SelectedIndex = 1;

                return true;
            }
            catch (Exception exe)
            {
                MessageBox.Show("Unable to set values from remote execution server package\r\n\r\n" + exe.Message);
                return false;
            }
        }
        private void SaveBuildSettingPackage()
        {




        }

        private void saveExecutionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!ValidateRequest())
                return;

            List<string> lstUnassignedDatabaseServers, lstUntaskedExecutionServers;
            BuildSettings settings = CompileBuildSettings(out lstUntaskedExecutionServers, out lstUnassignedDatabaseServers);

            if (settings == null)
                return;

            settings.DistributionType = this.loadDistributionType;
            if (!chkUseOverrideAsExeList.Checked)
            {
                settings.RemoteExecutionServers = serverData.ToList();
            }
            else
            {
                ServerConfigData cfg = new ServerConfigData() { ServerName = "derive" };
                settings.RemoteExecutionServers = new List<ServerConfigData>();
                settings.RemoteExecutionServers.Add(cfg);
            }


            if (!AcceptLoadDistribution(lstUntaskedExecutionServers, lstUnassignedDatabaseServers))
                return;

            if (DialogResult.OK == saveFileDialog1.ShowDialog())
            {
                string fileName = saveFileDialog1.FileName;

                try
                {
                    System.Xml.XmlTextWriter tw = null;
                    try
                    {
                        XmlSerializer xmlS = new XmlSerializer(typeof(BuildSettings));
                        tw = new System.Xml.XmlTextWriter(fileName, Encoding.UTF8);
                        tw.Formatting = System.Xml.Formatting.Indented;
                        xmlS.Serialize(tw, settings);
                    }
                    finally
                    {
                        if (tw != null)
                            tw.Close();
                    }
                }
                catch (Exception exe)
                {
                    MessageBox.Show("Unable to save package file to:\r\n" + fileName + "\r\n\r\n" + exe.ToString(), "Unable to Save", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }
        }

        private void tmrCheckStatus_Tick(object sender, EventArgs e)
        {
            if (this.packageSubmitted)
                btnCheckServiceStatus_Click(sender, e);
            else
            {
                this.tmrCheckStatus.Enabled = false;
            }
        }

        private void openRemoteExecutionServerPackagerespToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == openFileDialog1.ShowDialog())
            {
                string fileName = openFileDialog1.FileName;
                BuildSettings settings = null;

                try
                {
                    System.Xml.XmlTextReader tr = null;
                    try
                    {
                        XmlSerializer xmlS = new XmlSerializer(typeof(BuildSettings));
                        tr = new System.Xml.XmlTextReader(fileName);
                        settings = (BuildSettings)xmlS.Deserialize(tr);
                        if (settings != null)
                            DecompileBuildSettings(settings);


                    }
                    finally
                    {
                        if (tr != null)
                            tr.Close();
                    }
                }
                catch (Exception exe)
                {
                    MessageBox.Show("Unable to open package file to:\r\n" + fileName + "\r\n\r\n" + exe.ToString(), "Unable to Open", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void lnkViewSbmPackage_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (txtSbmFile.Text.Trim().Length > 0)
            {
                if (!File.Exists(txtSbmFile.Text))
                {
                    MessageBox.Show("The selected file does not exist!", "Missing File", MessageBoxButtons.OK);
                    return;
                }
                SqlBuildForm sbmForm = new SqlBuildForm(txtSbmFile.Text, this.connData);
                sbmForm.ShowDialog();
            }

        }

        private void chkUseOverrideAsExeList_CheckedChanged(object sender, EventArgs e)
        {
            if (chkUseOverrideAsExeList.Checked)
            {
                if (txtOverride.Text.Length == 0)
                {
                    MessageBox.Show("Please enter an override target settings file to use", "Missing Override Target Settings");
                    return;
                }

                string[] multiDbConfigLines;
                try
                {
                    multiDbConfigLines = RemoteHelper.GetMultiDbConfigLinesArray(txtOverride.Text);
                    string[] exeServerList = RemoteHelper.GetUniqueServerNamesFromMultiDb(multiDbConfigLines);

                    dgvRemoteServers.Rows.Clear();

                    foreach (string s in exeServerList)
                        dgvRemoteServers.Rows.Add(s);

                    dgvRemoteServers.Invalidate();
                    
                    btnCheckServiceStatus_Click(sender, e);
                }
                catch (MultiDbConfigurationException exe)
                {
                    MessageBox.Show(exe.Message, "Error");
                    return;
                }

                if (GetDistributionType() == DistributionType.EqualSplit)
                {
                    ddDistribution.SelectedIndex = 1;
                    MessageBox.Show("Based on this selection, the 'Distribution Type' has been automatically changed to \"... server handles only its local load ...\"", "Distribution Type Changed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }

            }

        }

        private void btnCommandLine_Click(object sender, EventArgs e)
        {
            try
            {

                string distTypeString = (GetDistributionType() == DistributionType.EqualSplit ? "equal" : "local");

                string remoteExeString;
                if (chkUseOverrideAsExeList.Checked)
                    remoteExeString = "derive";
                else
                {
                    if (DialogResult.OK == saveFileDialog2.ShowDialog())
                    {
                        List<string> servers = (from s in this.serverData select s.ServerName).ToList();
                        File.WriteAllLines(saveFileDialog2.FileName, servers.ToArray());
                        remoteExeString = saveFileDialog2.FileName;
                    }
                    else
                    {
                        return;
                    }
                }
                int retryCnt;
                Int32.TryParse(txtTimeoutRetryCount.Text,out retryCnt);
                string commandLine = SqlSync.SqlBuild.Remote.RemoteHelper.BuildRemoteExecutionCommandline(txtSbmFile.Text,
                    txtOverride.Text,
                    remoteExeString,
                    txtRootLoggingPath.Text,
                    distTypeString,
                    chkRunTrial.Checked,
                    !chkNotTransactional.Checked,
                    txtDescription.Text,
                    retryCnt);

                ScriptDisplayForm frmDisp = new ScriptDisplayForm(commandLine, "", "");
                frmDisp.ShowDialog();
            }
            catch (ArgumentException exe)
            {
                MessageBox.Show(exe.Message, "Configuration Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
                

            
        }

        private void btnTestConnections_Click(object sender, EventArgs e)
        {
            if (txtOverride.Text.Length == 0)
            {
                MessageBox.Show("Please add a Target Override Setting value", "Missing Values", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (dgvRemoteServers.Rows.Count == 0 && !chkUseOverrideAsExeList.Checked)
            {
                MessageBox.Show("Please add one or more Remote Execution Server to the list or check \"Derive Remote Execution Server list from Override Target Settings\" ckeckbox.", "Missing Values", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }
            
            this.loadDistributionType = GetDistributionType();
            List<string> servers = GetExecutionServerListFromGrid();
            buildManager.SetServerNames(servers);
            BuildSettings settings = new BuildSettings();
            settings.RemoteExecutionServers = buildManager.GetServerConfigData.ToList();
            try
            {
                settings.MultiDbTextConfig = GetMultiDbConfigLines();
            }
            catch
            {
            }
            if (settings == null || settings.MultiDbTextConfig.Count() == 0)
            {
                MessageBox.Show("Sorry... Unable to compile connection settings.\r\n Maybe you can check your settings and try again.", "Oh No... :-(", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bgConnectionTest.RunWorkerAsync(settings);


        }

        private void viewBuildRequestHistoryForThisRemoteServiceToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ServerConfigData server = (ServerConfigData)this.dgvServerStatus.Rows[this.dgvServerStatus.SelectedCells[0].RowIndex].DataBoundItem;
            string message;
            IList<BuildRecord> history = this.buildManager.GetBuildServiceHistory(server.ActiveServiceEndpoint, out message);
            if (history.Count == 0 && message.Length > 0)
            {
                MessageBox.Show(message, "Can't get the history", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            BuildHistoryForm frmHist = new BuildHistoryForm(history, server.ServerName,server.ActiveServiceEndpoint);
            frmHist.ShowDialog();

        }

        private void dgvServerStatus_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if(e.ColumnIndex == 1 && e.Value.Equals( SqlBuildManager.ServiceClient.Sbm.BuildService.ServiceReadiness.Unknown))
            {
                e.CellStyle.ForeColor = Color.Red;
            }
        }

        private void protocolComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (protocolComboBox.SelectedItem.ToString() == "Tcp")
            {
                buildManager.SetProtocol(Protocol.Tcp);
                ddDistribution.Enabled = true;
            }
            else if (protocolComboBox.SelectedItem.ToString() == "Http")
            {
                buildManager.SetProtocol(Protocol.Http);
                ddDistribution.Enabled = true;
            }
            else if (protocolComboBox.SelectedItem.ToString() == "Azure-Http")
            {
                buildManager.SetProtocol(Protocol.AzureHttp);

                ddDistribution.SelectedIndex = 0;
                ddDistribution.Enabled = false;
                

                List<ServerConfigData> serverData = buildManager.GetListOfAzureInstancePublicUrls();
                this.serverData = new BindingList<ServerConfigData>(serverData);
                this.dgvServerStatus.DataSource = this.serverData;

                dgvRemoteServers.Rows.Clear();

                foreach (var s in this.serverData)
                    dgvRemoteServers.Rows.Add(s.ServerName);

                dgvRemoteServers.Invalidate();
            }
        }



 
    }
}
