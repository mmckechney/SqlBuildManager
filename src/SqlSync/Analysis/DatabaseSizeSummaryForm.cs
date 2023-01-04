using SqlSync.Connection;
using SqlSync.DbInformation;
using System;
using System.ComponentModel;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace SqlSync.Analysis
{
    public partial class DatabaseSizeSummaryForm : Form
    {
        Connection.ConnectionData connData;
        public DatabaseSizeSummaryForm()
        {
            InitializeComponent();
        }

        public DatabaseSizeSummaryForm(Connection.ConnectionData connData) : this()
        {
            this.connData = connData;
            Text = String.Format(Text, connData.SQLServerName);
        }

        private void DatabaseSizeSummaryForm_Load(object sender, EventArgs e)
        {
            settingsControl1.Server = connData.SQLServerName;
            GetDatabaseSummary();
        }



        private void mnuChangeSqlServer_Click(object sender, System.EventArgs e)
        {
            ConnectionForm frmConnect = new ConnectionForm("Sql Build Manager");
            DialogResult result = frmConnect.ShowDialog();
            if (result == DialogResult.OK)
            {
                connData = frmConnect.SqlConnection;
                GetDatabaseSummary();
            }
        }

        private void settingsControl1_ServerChanged(object sender, string serverName, string username, string password, AuthenticationType authType)
        {
            string oldServer = connData.SQLServerName;
            connData.SQLServerName = settingsControl1.Server;
            if (!string.IsNullOrWhiteSpace(username) && (!string.IsNullOrWhiteSpace(password)))
            {
                connData.UserId = username;
                connData.Password = password;
            }

            connData.AuthenticationType = authType;
            GetDatabaseSummary();
        }

        private void getDatabaseDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {

            ServerSizeSummaryRow row = (ServerSizeSummaryRow)((DataRowView)dataGridView1.CurrentRow.DataBoundItem).Row;
            AnalysisForm frmAnaly = new AnalysisForm(connData, row.DatabaseName);
            frmAnaly.Show();
        }

        private void GetDatabaseSummary()
        {
            Cursor = Cursors.WaitCursor;
            statSizeSum.Text = "Retrieving Server Data...";
            toolStripProgressBar1.Style = ProgressBarStyle.Marquee;
            bgWorker.RunWorkerAsync();
        }
        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ServerSizeSummary summ = DbInformation.InfoHelper.GetServerDatabaseInfo(connData);
            e.Result = summ;
        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ServerSizeSummary summ = (ServerSizeSummary)e.Result;
            serverSizeSummary1 = summ;
            serverSizeSummary1.AcceptChanges();
            dataGridView1.DataSource = summ;

            double sum = 0.0;
            foreach (ServerSizeSummaryRow row in summ)
                sum = sum + row.DataSize;

            statSizeSum.Text = "Total Server Size: " + sum.ToString() + " MB";
            toolStripProgressBar1.Style = ProgressBarStyle.Blocks;

            Cursor = Cursors.Default;

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SqlSync.SqlBuild.UtilityHelper.OpenManual("DatabaseAnalysis");
        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (dataGridView1.DataSource == null)
                return;

            if (dataGridView1.DataSource is ServerSizeSummary)
            {
                ServerSizeSummary size = (ServerSizeSummary)dataGridView1.DataSource;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Database Size Summary");
                sb.AppendLine("Server:\t" + connData.SQLServerName);
                foreach (DataGridViewColumn col in dataGridView1.Columns)
                {
                    sb.Append(col.HeaderText + "\t");
                }
                sb.AppendLine();


                foreach (ServerSizeSummaryRow row in size.Rows)
                {
                    sb.AppendLine(row.DatabaseName + "\t" + row.DateCreated + "\t" + row.DataSize + "\t" + row.Location);
                }
                Clipboard.SetText(sb.ToString());

            }
        }



    }
}