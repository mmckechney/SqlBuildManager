using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SqlSync.DbInformation;

namespace SqlSync.Analysis
{
    public partial class DatabaseSizeSummaryForm : Form
    {
        Connection.ConnectionData connData;
        public DatabaseSizeSummaryForm()
        {
            InitializeComponent();
        }

        public DatabaseSizeSummaryForm(Connection.ConnectionData connData) :this()
        {
            this.connData = connData;
            this.Text = String.Format(this.Text, connData.SQLServerName);
        }

        private void DatabaseSizeSummaryForm_Load(object sender, EventArgs e)
        {
            this.settingsControl1.Server = this.connData.SQLServerName;
            GetDatabaseSummary();
        }


 
        private void mnuChangeSqlServer_Click(object sender, System.EventArgs e)
		{
			ConnectionForm frmConnect = new ConnectionForm("Sql Build Manager");
			DialogResult result = frmConnect.ShowDialog();
			if(result == DialogResult.OK)
			{
					this.connData = frmConnect.SqlConnection;
                    GetDatabaseSummary();
			}
		}

        private void settingsControl1_ServerChanged(object sender, string serverName, string username, string password)
        {
            string oldServer = this.connData.SQLServerName;
            this.connData.SQLServerName = this.settingsControl1.Server;
            if(!string.IsNullOrWhiteSpace(username) && (!string.IsNullOrWhiteSpace(password)))
            {
                this.connData.UserId = username;
                this.connData.Password = password;
                this.connData.UseWindowAuthentication = false;
            }
            else
            {
                this.connData.UseWindowAuthentication = true;
            }
            GetDatabaseSummary();
        }

        private void getDatabaseDetailsToolStripMenuItem_Click(object sender, EventArgs e)
        {

            ServerSizeSummaryRow row = (ServerSizeSummaryRow)((DataRowView)this.dataGridView1.CurrentRow.DataBoundItem).Row;
            AnalysisForm frmAnaly = new AnalysisForm(this.connData, row.DatabaseName);
            frmAnaly.Show();
        }

        private void GetDatabaseSummary()
        {
            this.Cursor = Cursors.WaitCursor;
            this.statSizeSum.Text = "Retrieving Server Data...";
            this.toolStripProgressBar1.Style = ProgressBarStyle.Marquee;
            bgWorker.RunWorkerAsync();
        }
        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            ServerSizeSummary summ = DbInformation.InfoHelper.GetServerDatabaseInfo(this.connData);
            e.Result = summ;
        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            ServerSizeSummary summ = (ServerSizeSummary)e.Result;
            this.serverSizeSummary1 = summ;
            this.serverSizeSummary1.AcceptChanges();
            this.dataGridView1.DataSource = summ;

            double sum = 0.0;
            foreach (ServerSizeSummaryRow row in summ)
                sum = sum + row.DataSize;

            statSizeSum.Text = "Total Server Size: " + sum.ToString() + " MB";
            this.toolStripProgressBar1.Style = ProgressBarStyle.Blocks;

            this.Cursor = Cursors.Default;

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
                sb.AppendLine("Server:\t" + this.connData.SQLServerName);
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