using SqlSync.Connection;
using SqlSync.ObjectScript.Hash;
using SqlSync.SqlBuild.Status;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
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
            collector = new HashCollector(multiDbData);
            rawReportData = collector.GetObjectHashes(ref bgWorker, args.Value, args.Key, scriptThreaded);
            e.Result = rawReportData;

            bgWorker.ReportProgress(0, "Generating report output");
            base.bgWorker.DoWork -= new System.ComponentModel.DoWorkEventHandler(base.bgWorker_DoWork);

        }

        private void bgWorker_ProgressChanged_1(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is HashCollectionRunnerUpdateEventArgs)
            {
                HashCollectionRunnerUpdateEventArgs arg = (HashCollectionRunnerUpdateEventArgs)e.UserState;
                foreach (ListViewItem item in lstDbStatus.Items)
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
            if (collector != null && rawReportData != null)
            {
                btnAnalysis.Enabled = true;
            }
        }

        private void btnAnalysis_Click(object sender, EventArgs e)
        {
            ObjectComparisonAnalysisForm frmAn = new ObjectComparisonAnalysisForm(rawReportData);
            frmAn.Show();

        }

        private void chkScriptThreaded_CheckedChanged(object sender, EventArgs e)
        {
            scriptThreaded = chkScriptThreaded.Checked;
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            SqlSync.SqlBuild.UtilityHelper.OpenManual("ObjectComparisonReport");
        }


    }
}
