using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SqlBuildManager.ServiceClient;
using SqlBuildManager.ServiceClient.Sbm.BuildService;
using SqlSync.SqlBuild.Remote;
namespace SqlSync.Controls
{
    public partial class RemoteExecutionLogsContextMenuStrip : System.Windows.Forms.ContextMenuStrip
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static BuildServiceManager buildManager = new BuildServiceManager();
        public RemoteExecutionLogsContextMenuStrip()
        {
            InitializeComponent();

            viewExecutionErrorsLogToolStripMenuItem.Click += new EventHandler(viewExecutionErrorsLogToolStripMenuItem_Click);
            viewExecutionCommitsLogToolStripMenuItem.Click += new EventHandler(viewExecutionCommitsLogToolStripMenuItem_Click);
            txtDetailedLogTarget.KeyDown += new KeyEventHandler(txtDetailedLogTarget_KeyDown);
            viewRemoteServiceExecutableLogFileToolStripMenuItem.Click += new EventHandler(viewRemoteServiceExecutableLogFileToolStripMenuItem_Click);
            this.retrieveAllApplicableErrorLogsToolStripMenuItem.Click +=new EventHandler(retrieveAllApplicableErrorLogsToolStripMenuItem_Click);
        }
       
        private void viewExecutionErrorsLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.SourceControl is DataGridView)
            {
                DataGridView dgvServerStatus = (DataGridView)this.SourceControl;
                if (dgvServerStatus.SelectedCells.Count == 0)
                    return;

                try
                {
  
                    string serverName = string.Empty;
                    string text = string.Empty;
                    string remoteEndpoint = string.Empty;
                    if (dgvServerStatus.Rows[dgvServerStatus.SelectedCells[0].RowIndex].DataBoundItem is ServerConfigData)
                    {
                        ServerConfigData server = (ServerConfigData)dgvServerStatus.Rows[dgvServerStatus.SelectedCells[0].RowIndex].DataBoundItem;
                        remoteEndpoint = server.ActiveServiceEndpoint;
                        text= buildManager.GetErrorsLog(server.ActiveServiceEndpoint);
                        serverName = server.ServerName;
                    }
                    else if (dgvServerStatus.Rows[dgvServerStatus.SelectedCells[0].RowIndex].DataBoundItem is BuildRecord)
                    {
                        BuildRecord buildRec = (BuildRecord)dgvServerStatus.Rows[dgvServerStatus.SelectedCells[0].RowIndex].DataBoundItem;
                        remoteEndpoint = buildRec.RemoteEndPoint;
                        text = buildManager.GetSpecificSummaryLogFile(buildRec.RemoteEndPoint, SummaryLogType.Errors,buildRec.submissionDate);
                        serverName = buildRec.RemoteServerName;
                    }

                    SqlSync.ScriptDisplayForm frmScript = new ScriptDisplayForm(text, serverName, "Errors Log from " + serverName, SqlSync.Highlighting.SyntaxHightlightType.RemoteServiceLog,remoteEndpoint);
                    frmScript.LoggingLinkClicked += new LoggingLinkClickedEventHandler(frmScript_LoggingLinkClicked);
                    frmScript.Show();
                }
                catch (System.ServiceModel.CommunicationObjectFaultedException comExe)
                {
                    MessageBox.Show("There was a communication issue retrieving the log file.\r\nTo try to clear the communication error, try closing this page and reopening. If that does not work, you may need to restart the service on the target remote execution server.\r\n\r\nError Message:\r\n" + comExe.Message, "Whoops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception exe)
                {
                    MessageBox.Show("There was an issue retrieving the log file.\r\n" + exe.Message, "Whoops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void viewExecutionCommitsLogToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.SourceControl is DataGridView)
            {
                DataGridView dgvServerStatus = (DataGridView)this.SourceControl;
                if (dgvServerStatus.SelectedCells.Count == 0)
                    return;

                try
                {
                    string serverName = string.Empty;
                    string text = string.Empty;
                    string remoteEndpoint = string.Empty;
                    if (dgvServerStatus.Rows[dgvServerStatus.SelectedCells[0].RowIndex].DataBoundItem is ServerConfigData)
                    {
                        ServerConfigData server = (ServerConfigData)dgvServerStatus.Rows[dgvServerStatus.SelectedCells[0].RowIndex].DataBoundItem;
                        remoteEndpoint = server.ActiveServiceEndpoint;
                        text = buildManager.GetCommitsLog(server.ActiveServiceEndpoint);
                        serverName = server.ServerName;
                    }
                    else if (dgvServerStatus.Rows[dgvServerStatus.SelectedCells[0].RowIndex].DataBoundItem is BuildRecord)
                    {
                        BuildRecord buildRec = (BuildRecord)dgvServerStatus.Rows[dgvServerStatus.SelectedCells[0].RowIndex].DataBoundItem;
                        remoteEndpoint = buildRec.RemoteEndPoint;
                        text = buildManager.GetSpecificSummaryLogFile(buildRec.RemoteEndPoint, SummaryLogType.Commits, buildRec.submissionDate);
                        serverName = buildRec.RemoteServerName;
                    }

                    SqlSync.ScriptDisplayForm frmScript = new ScriptDisplayForm(text, serverName, "Commits Log from " + serverName, SqlSync.Highlighting.SyntaxHightlightType.RemoteServiceLog,remoteEndpoint);
                    frmScript.LoggingLinkClicked += new LoggingLinkClickedEventHandler(frmScript_LoggingLinkClicked);
                    frmScript.Show();
                }
                catch (System.ServiceModel.CommunicationObjectFaultedException comExe)
                {
                    MessageBox.Show("There was a communication issue retrieving the log file.\r\nTo try to clear the communication error, try closing this page and reopening. If that does not work, you may need to restart the service on the target remote execution server.\r\n\r\nError Message:\r\n" + comExe.Message, "Whoops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                catch (Exception exe)
                {
                    MessageBox.Show("There was an issue retrieving the log file.\r\n" + exe.Message, "Whoops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        void frmScript_LoggingLinkClicked(string databaseInfo, string remoteEndPoint)
        {
            //database info expected int the format:   TLODDNO2\TLO_Clt1.TLO_002011#[01/17/2011 09:40:31.199]
            try
            {
                //MessageBox.Show(databaseInfo + "      " + remoteEndPoint);
                string database = databaseInfo.Split(new char[] { '#' })[0];
                string timeStamp = databaseInfo.Split(new char[] { '#' })[1].Replace("[", "").Replace("]", "");
                DateTime submittedDate;
                if (!DateTime.TryParse(timeStamp, out submittedDate))
                {
                    MessageBox.Show(String.Format("Unable to determine build date with supplied Date/Time: {0}", timeStamp),"Sorry!",MessageBoxButtons.OK,MessageBoxIcon.Error);
                    log.Error(String.Format("Unable to get submittedDate with info: databaseInfo=\"{0}\"; remoteEndPoint=\"{1}\"", databaseInfo, remoteEndPoint));
                }

                string text = buildManager.GetSpecificDetailedDatabaseLog(remoteEndPoint, database, submittedDate);
                SqlSync.ScriptDisplayForm frmScript = new ScriptDisplayForm(text, "", "Detailed Log from " + database);
                frmScript.Show();
            }
            catch (Exception exe)
            {
                log.Error(String.Format("Unable to get detailed log with info: databaseInfo=\"{0}\"; remoteEndPoint=\"{1}\"",databaseInfo,remoteEndPoint),exe);
                MessageBox.Show("Something went wrong! Please check the application log for detail","Whoops",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }


        }

        private void txtDetailedLogTarget_KeyDown(object sender, KeyEventArgs e)
        {
            if (this.SourceControl is DataGridView)
            {
                DataGridView dgvServerStatus = (DataGridView)this.SourceControl;

                if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Enter)
                {
                    if (dgvServerStatus.SelectedCells.Count == 0 || txtDetailedLogTarget.Text.Length == 0)
                        return;
                    try
                    {
                        string serverName = string.Empty;
                        string text = string.Empty;
                        if (dgvServerStatus.Rows[dgvServerStatus.SelectedCells[0].RowIndex].DataBoundItem is ServerConfigData)
                        {
                            ServerConfigData server = (ServerConfigData)dgvServerStatus.Rows[dgvServerStatus.SelectedCells[0].RowIndex].DataBoundItem;
                            text = buildManager.GetDetailedDatabaseLog(server.ActiveServiceEndpoint, txtDetailedLogTarget.Text);
                            serverName = server.ServerName;
                        }
                        else if (dgvServerStatus.Rows[dgvServerStatus.SelectedCells[0].RowIndex].DataBoundItem is BuildRecord)
                        {
                            BuildRecord buildRec = (BuildRecord)dgvServerStatus.Rows[dgvServerStatus.SelectedCells[0].RowIndex].DataBoundItem;
                            text = buildManager.GetSpecificDetailedDatabaseLog(buildRec.RemoteEndPoint, txtDetailedLogTarget.Text, buildRec.submissionDate);
                            serverName = buildRec.RemoteServerName;
                        }

                        SqlSync.ScriptDisplayForm frmScript = new ScriptDisplayForm(text, serverName, "Detailed Log from " + txtDetailedLogTarget.Text);
                        frmScript.Show();

                        txtDetailedLogTarget.Text = string.Empty;
                    }
                    catch (Exception exe)
                    {
                        MessageBox.Show("There was an issue retrieving the log file.\r\n" + exe.Message, "Whoops!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void viewRemoteServiceExecutableLogFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.SourceControl is DataGridView)
            {
                DataGridView dgvServerStatus = (DataGridView)this.SourceControl;
                if (dgvServerStatus.SelectedCells.Count == 0)
                    return;

                this.Cursor = Cursors.WaitCursor;
                //this.statGeneral.Text = "Retrieving Remote Service Executable log file...";
                //this.statProgBar.Style = ProgressBarStyle.Marquee;
                try
                {
                    string serverName = string.Empty;
                    string remoteEndpoint = string.Empty;
                    string text = string.Empty;
                    if (dgvServerStatus.Rows[dgvServerStatus.SelectedCells[0].RowIndex].DataBoundItem is ServerConfigData)
                    {
                        ServerConfigData server = (ServerConfigData)dgvServerStatus.Rows[dgvServerStatus.SelectedCells[0].RowIndex].DataBoundItem;
                        remoteEndpoint = server.ActiveServiceEndpoint;
                        serverName = server.ServerName;
                    }
                    else if (dgvServerStatus.Rows[dgvServerStatus.SelectedCells[0].RowIndex].DataBoundItem is BuildRecord)
                    {
                        BuildRecord buildRec = (BuildRecord)dgvServerStatus.Rows[dgvServerStatus.SelectedCells[0].RowIndex].DataBoundItem;
                        remoteEndpoint = buildRec.RemoteEndPoint;
                        serverName = buildRec.RemoteServerName;
                    }

                    text = buildManager.GetServiceLog(remoteEndpoint);
                    SqlSync.ScriptDisplayForm frmScript = new ScriptDisplayForm(text, serverName, "Service Executable Log from " + serverName, SqlSync.Highlighting.SyntaxHightlightType.LogFile);
                        frmScript.Show();
                    
                }
                catch (Exception exe)
                {
                    MessageBox.Show("Sorry... There was an error attempting to retrieve the log file.\r\n " + exe.Message, "Problem!", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;

                }
                finally
                {
                    this.Cursor = Cursors.Default;
                    //this.statGeneral.Text = "Ready.";
                    //this.statProgBar.Style = ProgressBarStyle.Blocks;
                }
            }
        }

        //private void viewBuildRequestHistoryForThisRemoteServiceToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    if (this.SourceControl is DataGridView)
        //    {
        //        DataGridView dgvServerStatus = (DataGridView)this.SourceControl;
        //        if (dgvServerStatus.SelectedCells.Count == 0)
        //            return;

        //        ServerConfigData server = (ServerConfigData)dgvServerStatus.Rows[dgvServerStatus.SelectedCells[0].RowIndex].DataBoundItem;
        //        string message;
        //        IList<BuildRecord> history = buildManager.GetBuildServiceHistory(server.TcpServiceEndpoint, out message);
        //        if (history.Count == 0 && message.Length > 0)
        //        {
        //            MessageBox.Show(message, "Can't get the history", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        //            return;
        //        }

        //        BuildHistoryForm frmHist = new BuildHistoryForm(history, server.ServerName, server.TcpServiceEndpoint);
        //        frmHist.ShowDialog();

        //    }
        //}

        private void retrieveAllApplicableErrorLogsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.SourceControl is DataGridView)
            {
                DataGridView dgvServerStatus = (DataGridView)this.SourceControl;
                if (dgvServerStatus.SelectedCells.Count == 0)
                    return;

                string serverName = string.Empty;
                string text = string.Empty;
                string remoteEndpoint = string.Empty;
                DateTime submissionDate = DateTime.MaxValue;
                if (dgvServerStatus.Rows[dgvServerStatus.SelectedCells[0].RowIndex].DataBoundItem is ServerConfigData)
                {
                    ServerConfigData server = (ServerConfigData)dgvServerStatus.Rows[dgvServerStatus.SelectedCells[0].RowIndex].DataBoundItem;
                    remoteEndpoint = server.ActiveServiceEndpoint;
                    serverName = server.ServerName;
                }
                else if (dgvServerStatus.Rows[dgvServerStatus.SelectedCells[0].RowIndex].DataBoundItem is BuildRecord)
                {
                    BuildRecord buildRec = (BuildRecord)dgvServerStatus.Rows[dgvServerStatus.SelectedCells[0].RowIndex].DataBoundItem;
                    remoteEndpoint = buildRec.RemoteEndPoint;
                    serverName = buildRec.RemoteServerName;
                    submissionDate = buildRec.submissionDate;
                }

                if (DialogResult.OK == saveFileDialog1.ShowDialog())
                {
                    if (buildManager.GetConsolidatedErrorLogs(remoteEndpoint, submissionDate, saveFileDialog1.FileName))
                    {
                        if (DialogResult.Yes == MessageBox.Show("Successfully retrieved error logs. Do you want to open the zip file now?", "Open", MessageBoxButtons.YesNo))
                            System.Diagnostics.Process.Start(saveFileDialog1.FileName);
                    }
                    else
                    {
                        MessageBox.Show("Sorry, there was an error retrieving the error logs. Please check the application log and try again", "Whoops. Something didn't work!", MessageBoxButtons.OK);
                    }
                }
            }
        }

        public string ErrorsLogMenuItemText
        {
            get
            {
                if (this.viewExecutionErrorsLogToolStripMenuItem != null)
                    return this.viewExecutionErrorsLogToolStripMenuItem.Text;
                else
                    return string.Empty;
            }
            set
            {
                if (this.viewExecutionErrorsLogToolStripMenuItem != null)
                    this.viewExecutionErrorsLogToolStripMenuItem.Text = value;
            }
        }

        public string CommitsLogMenuItemText
        {
            get
            {
                if (this.viewExecutionCommitsLogToolStripMenuItem != null)
                    return this.viewExecutionCommitsLogToolStripMenuItem.Text;
                else
                    return string.Empty;
            }
            set
            {
                if (this.viewExecutionCommitsLogToolStripMenuItem != null)
                    this.viewExecutionCommitsLogToolStripMenuItem.Text = value;
            }
        }
    }
}
