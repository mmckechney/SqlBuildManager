using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SqlSync.ObjectScript.Hash;
using SqlSync.SqlBuild.Status;
using SqlSync.Connection;
namespace SqlSync.SqlBuild.MultiDb
{
    public partial class ObjectComparisonReportForm : SqlSync.SqlBuild.MultiDb.StatusReportForm
    {
        bool scriptThreaded = true;
        HashCollector collector = null;
        ObjectScriptHashReportData rawReportData = null;
        public ObjectComparisonReportForm(MultiDb.MultiDbData multiDbData, ConnectionData connData)
            : base(multiDbData, connData)
        {
            InitializeComponent();

            base.ddOutputType.Items.Clear();
            base.ddOutputType.Items.Add("XML");
            base.ddOutputType.Items.Add("Summary");
            base.ddOutputType.SelectedIndex = 1;
        }


        protected override void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            KeyValuePair<ReportType, string> args = (KeyValuePair<ReportType, string>)e.Argument;
            this.collector = new HashCollector(this.multiDbData);
            this.rawReportData = collector.GetObjectHashes(ref this.bgWorker, args.Value, args.Key,this.scriptThreaded);
            e.Result = rawReportData;

            bgWorker.ReportProgress(0, "Generating report output");
            base.bgWorker.DoWork -= new System.ComponentModel.DoWorkEventHandler(base.bgWorker_DoWork);

        }

        private void bgWorker_ProgressChanged_1(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is HashCollectionRunnerUpdateEventArgs)
            {
                HashCollectionRunnerUpdateEventArgs arg = (HashCollectionRunnerUpdateEventArgs)e.UserState;
                foreach (ListViewItem item in this.lstDbStatus.Items)
                {
                    if (item.SubItems[0].Text == arg.Server && item.SubItems[1].Text == arg.Database)
                    {
                        item.SubItems[2].Text = arg.Message;
                        return;
                    }
                }
                ListViewItem newItem = new ListViewItem(new string[] { arg.Server, arg.Database, arg.Message });
                lstDbStatus.Items.Insert(0, newItem);
            }
        }


        private void bgWorker_RunWorkerCompleted_1(object sender, RunWorkerCompletedEventArgs e)
        {
            if (this.collector != null && this.rawReportData != null)
            {
                btnAnalysis.Enabled = true;
            }
        }

        private void btnAnalysis_Click(object sender, EventArgs e)
        {
            ObjectComparisonAnalysisForm frmAn = new ObjectComparisonAnalysisForm(this.rawReportData);
            frmAn.Show();

        }

        private void chkScriptThreaded_CheckedChanged(object sender, EventArgs e)
        {
            this.scriptThreaded = chkScriptThreaded.Checked;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            SqlSync.SqlBuild.UtilityHelper.OpenManual("ObjectComparisonReport");
        }

      
    }
}
