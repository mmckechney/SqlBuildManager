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
namespace SqlSync.SqlBuild.Remote
{
    public partial class BuildHistoryForm : Form
    {
        BindingList<BuildRecord> buildHistory = new BindingList<BuildRecord>();
        private string remoteServerEndpoint = string.Empty;
        BuildServiceManager buildManager = null;
        string remoteServerName = string.Empty;
        public BuildHistoryForm()
        {
            InitializeComponent();
            buildManager = new BuildServiceManager();

        }

        public BuildHistoryForm(IList<BuildRecord> buildHistory, string remoteServerName, string remoteServerEndpoint) : this()
        {
            this.buildHistory = new BindingList<BuildRecord>((IList<BuildRecord>)buildHistory);
            foreach (BuildRecord r in this.buildHistory)
            {
                r.RemoteEndPoint = remoteServerEndpoint;
                r.RemoteServerName = remoteServerName;
            }

            this.Text = "Build History for: " + remoteServerName;
            this.remoteServerEndpoint = remoteServerEndpoint;
            this.remoteServerName = remoteServerName;
        }

        private void BuildHistoryForm_Load(object sender, EventArgs e)
        {
            BindingList<BuildRecord> sorted = new BindingList<BuildRecord>((from h in buildHistory
                                                                            orderby h.submissionDate descending
                                                                            select h).ToList());
            this.dataGridView1.DataSource = sorted;

            this.remoteExecutionLogsContextMenuStrip1.ErrorsLogMenuItemText = "View Execution \"Errors\" log from this log path";
            this.remoteExecutionLogsContextMenuStrip1.CommitsLogMenuItemText = "View Execution \"Commits\" log from this log path";
        }

        private void BuildHistoryForm_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                this.Close();
        }

        //private void viewExecutionErrorsLogToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    if (this.dataGridView1.SelectedCells.Count == 0)
        //        return;

        //    BuildRecord server = (BuildRecord)this.dataGridView1.Rows[this.dataGridView1.SelectedCells[0].RowIndex].DataBoundItem;
        //    string text = buildManager.GetSpecificSummaryLogFile(this.remoteServerEndpoint,SummaryLogType.Errors,server.submissionDate);
        //    SqlSync.ScriptDisplayForm frmScript = new ScriptDisplayForm(text, this.remoteServerName, "Errors Log from " + this.remoteServerName, SqlSync.Highlighting.SyntaxHightlightType.None);
        //    frmScript.Show();

        //}

        //private void viewExecutionCommitsLogToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //      if (this.dataGridView1.SelectedCells.Count == 0)
        //        return;

        //    BuildRecord server = (BuildRecord)this.dataGridView1.Rows[this.dataGridView1.SelectedCells[0].RowIndex].DataBoundItem;
        //    string text = buildManager.GetSpecificSummaryLogFile(this.remoteServerEndpoint,SummaryLogType.Commits,server.submissionDate);
        //    SqlSync.ScriptDisplayForm frmScript = new ScriptDisplayForm(text, this.remoteServerName, "Commits Log from " + this.remoteServerName, SqlSync.Highlighting.SyntaxHightlightType.None);
        //    frmScript.Show();
        //}

        //private void txtDetailedLogTarget_KeyDown(object sender, KeyEventArgs e)
        //{
        //    if (e.KeyCode == Keys.Return || e.KeyCode == Keys.Enter)
        //    {
        //        if (this.dataGridView1.SelectedCells.Count == 0 || txtDetailedLogTarget.Text.Length == 0)
        //            return;

        //        BuildRecord server = (BuildRecord)this.dataGridView1.Rows[this.dataGridView1.SelectedCells[0].RowIndex].DataBoundItem;
        //        string text = buildManager.GetSpecificDetailedDatabaseLog(this.remoteServerEndpoint, txtDetailedLogTarget.Text, server.submissionDate);
        //        SqlSync.ScriptDisplayForm frmScript = new ScriptDisplayForm(text, this.remoteServerName, "Detailed Log from " + txtDetailedLogTarget.Text);
        //        frmScript.Show();

        //        txtDetailedLogTarget.Text = string.Empty;

        //    }
        //}

        //private void retrieveAllApplicableErrorLogsToolStripMenuItem_Click(object sender, EventArgs e)
        //{
        //    if (this.dataGridView1.SelectedCells.Count == 0)
        //        return;

        //    BuildRecord server = (BuildRecord)this.dataGridView1.Rows[this.dataGridView1.SelectedCells[0].RowIndex].DataBoundItem;

        //    if (DialogResult.OK == saveFileDialog1.ShowDialog())
        //    {

        //        if (buildManager.GetConsolidatedErrorLogs(this.remoteServerEndpoint, server.submissionDate, saveFileDialog1.FileName))
        //        {
        //            if (DialogResult.Yes == MessageBox.Show("Successfully retrieved error logs. Do you want to open the zip file now?", "Open", MessageBoxButtons.YesNo))
        //                System.Diagnostics.Process.Start(saveFileDialog1.FileName);
        //        }
        //        else
        //        {
        //            MessageBox.Show("Sorry, there was an error retrieving the error logs. Please check the application log and try again", "Whoops. Something didn't work!", MessageBoxButtons.OK);
        //        }
        //    }
        //}

       
    }
}
