using SqlSync.Connection;
using SqlSync.DbInformation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows.Forms;
namespace SqlSync.SqlBuild
{
    public partial class RebuildForm : Form
    {
        private string selectedDatabase = string.Empty;
        private SqlSync.ColumnSorter listSorter = new ColumnSorter();
        private ConnectionData connData;
        private DatabaseList dbList;
        public RebuildForm(Connection.ConnectionData connData, DatabaseList databaseList)
        {
            dbList = databaseList;
            this.connData = connData;
            InitializeComponent();
            ddDatabases.Items.AddRange(databaseList.ToArray());
        }

        private void RebuildForm_Load(object sender, EventArgs e)
        {
            settingsControl1.Server = connData.SQLServerName;
            Show();

            bgWorker.RunWorkerAsync();

        }

        private void changeSqlServerConnectionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ConnectionForm frmConnect = new ConnectionForm("Build File Reconstructor");
            DialogResult result = frmConnect.ShowDialog();
            if (result == DialogResult.OK)
            {
                connData = frmConnect.SqlConnection;
                settingsControl1.Server = connData.SQLServerName;

                dbList = frmConnect.DatabaseList;
                ddDatabases.Items.Clear();
                ddDatabases.Items.AddRange(dbList.ToArray());

                //bgWorker.RunWorkerAsync();

            }
        }

        private void settingsControl1_ServerChanged(object sender, string serverName, string username, string password, AuthenticationType authType)
        {
            Connection.ConnectionData oldConnData = new Connection.ConnectionData();
            connData.Fill(oldConnData);
            Cursor = Cursors.WaitCursor;

            connData.SQLServerName = serverName;
            if (!string.IsNullOrWhiteSpace(username) && (!string.IsNullOrWhiteSpace(password)))
            {
                connData.UserId = username;
                connData.Password = password;
            }
            connData.AuthenticationType = authType;
            connData.ScriptTimeout = 5;

            try
            {
                dbList = SqlSync.DbInformation.InfoHelper.GetDatabaseList(connData);

                ddDatabases.Items.Clear();
                ddDatabases.Items.AddRange(dbList.ToArray());

                // bgWorker.RunWorkerAsync();
            }
            catch
            {
                MessageBox.Show("Error retrieving database list. Is the server running?", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                connData = oldConnData;
                settingsControl1.Server = oldConnData.SQLServerName;
            }
            Cursor = Cursors.Default;
        }

        private void rebuildFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (lstBuildFiles.SelectedItems.Count == 0)
                return;

            CommittedBuildData dat = (CommittedBuildData)lstBuildFiles.SelectedItems[0].Tag;

            openFileDialog1.FileName = Path.GetFileNameWithoutExtension(dat.BuildFileName) + ".sbm";
            if (openFileDialog1.ShowDialog() == DialogResult.OK)
            {
                Cursor = Cursors.WaitCursor;
                Rebuilder blrd = new Rebuilder(connData, dat, openFileDialog1.FileName);
                if (blrd.RebuildBuildManagerFile(SqlSync.Properties.Settings.Default.DefaultMinimumScriptTimeout))
                {
                    if (DialogResult.Yes == MessageBox.Show("Reconstruction Complete. Open New Build File?", "Finished", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                    {
                        try
                        {
                            System.Diagnostics.Process prc = new System.Diagnostics.Process();
                            var me = System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName;
                            prc.StartInfo.FileName = me;
                            prc.StartInfo.Arguments = openFileDialog1.FileName;
                            prc.Start();
                        }
                        catch (Exception exe)
                        {
                            MessageBox.Show($"Oops..something went wrong. Please try to open the file manually.{Environment.NewLine}{exe.Message}");
                        }
                    }
                }
                Cursor = Cursors.Default;
            }
        }

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bgW = (BackgroundWorker)sender;
            bgW.ReportProgress(0, "Clearing List");
            bgW.ReportProgress(10, "Retrieving Build file list from server");
            List<CommittedBuildData> bldData = SqlBuild.Rebuilder.GetCommitedBuildList(connData, selectedDatabase);
            e.Result = bldData;

        }

        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage == 0)
            {
                statBar.Style = ProgressBarStyle.Marquee;
                statBar.MarqueeAnimationSpeed = 200;
                Cursor = Cursors.AppStarting;
                lstBuildFiles.Items.Clear();
            }
            else
                statGeneral.Text = e.UserState.ToString();

        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            List<CommittedBuildData> bldData = (List<CommittedBuildData>)e.Result;
            for (int i = 0; i < bldData.Count; i++)
            {
                ListViewItem item = new ListViewItem(new string[] { bldData[i].BuildFileName, bldData[i].Database, bldData[i].CommitDate.ToString(), bldData[i].ScriptCount.ToString() });
                item.Tag = bldData[i];
                lstBuildFiles.Items.Add(item);
            }

            listSorter.CurrentColumn = 2;
            listSorter.Sort = SortOrder.Descending;
            lstBuildFiles.ListViewItemSorter = listSorter;
            lstBuildFiles.Sort();

            Cursor = Cursors.Default;
            statGeneral.Text = "Ready.";
            statBar.Style = ProgressBarStyle.Blocks;
            statBar.Value = 0;

        }

        private void lstBuildFiles_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            ListViewItem item = null;

            if (lstBuildFiles.SelectedItems.Count > 0)
                item = lstBuildFiles.SelectedItems[0];

            listSorter.CurrentColumn = e.Column;
            lstBuildFiles.ListViewItemSorter = listSorter;
            lstBuildFiles.Sort();
            if (item != null)
                item.EnsureVisible();
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SqlSync.SqlBuild.UtilityHelper.OpenManual("RebuildingPreviouslyCommittedBuild");
        }

        private void btnGetList_Click(object sender, EventArgs e)
        {
            selectedDatabase = (ddDatabases.SelectedItem == null) ? "" : ddDatabases.SelectedItem.ToString();
            if (selectedDatabase.Length == 0)
            {
                MessageBox.Show("Please select a database to retrieve list from", "Please make selection", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            bgWorker.RunWorkerAsync();
        }

        private void ddDatabases_SelectionChangeCommitted(object sender, EventArgs e)
        {
            selectedDatabase = ddDatabases.SelectedItem.ToString();
        }


    }
}