using Microsoft.Extensions.Logging;
using SqlSync.Connection;
using SqlSync.MRU;
using SqlSync.SqlBuild.AdHocQuery;
using SqlSync.SqlBuild.Status;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
namespace SqlSync.SqlBuild.MultiDb
{
    public partial class BuildValidationForm : SqlSync.SqlBuild.MultiDb.StatusReportForm
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        MRUManager mruManager = new MRUManager();
        QueryCollector collector;
        string query = string.Empty;
        string checkFieldValue = string.Empty;
        BuildValidationType buildValidation = BuildValidationType.BuildFileHash;
        //List<QueryResultData> rawReportData;
        int timeOut = 20;
        public BuildValidationForm(MultiDb.MultiDbData multiDbData, ConnectionData connData)
            : base(multiDbData, connData)
        {
            InitializeComponent();

            base.ddOutputType.Items.Clear();
            base.ddOutputType.Items.Add("CSV");
            base.ddOutputType.Items.Add("HTML");
            base.ddOutputType.Items.Add("XML");
            base.ddOutputType.SelectedIndex = 0;
            btnGenerate.Click -= new EventHandler(btnGenerate_Click);
            btnGenerate.Click -= new EventHandler(btnGenerate_Click_1);

            //Get this form's click handler to fire first...
            btnGenerate.Click += new EventHandler(btnGenerate_Click_1);
            btnGenerate.Click += new EventHandler(btnGenerate_Click);
        }

        protected override void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            KeyValuePair<ReportType, string> args = (KeyValuePair<ReportType, string>)e.Argument;
            collector = new QueryCollector(multiDbData, connData);
            //this.rawReportData = 
            try
            {
                collector.GetBuildValidationResults(ref bgWorker, args.Value, txtCheckValue.Text, args.Key, buildValidation, timeOut);
                //collector.GetQueryResults(ref this.bgWorker, args.Value,args.Key, this.query, this.timeOut);
                bgWorker.ReportProgress(0, "Generating report output");
                e.Result = true; // rawReportData;
            }
            catch (Exception exe)
            {
                e.Result = exe;
            }
            base.bgWorker.DoWork -= new System.ComponentModel.DoWorkEventHandler(base.bgWorker_DoWork);

        }

        private void bgWorker_ProgressChanged_1(object sender, ProgressChangedEventArgs e)
        {
        }


        private void bgWorker_RunWorkerCompleted_1(object sender, RunWorkerCompletedEventArgs e)
        {
        }

        private void btnGenerate_Click_1(object sender, EventArgs e)
        {
            base.buildDuration = 0;
            base.timer1.Start();
            checkFieldValue = txtCheckValue.Text;
            if (!int.TryParse(txtTimeout.Text, out timeOut))
                timeOut = 20;

            switch (ddValidationType.SelectedItem.ToString())
            {

                case "Build File Name":
                    buildValidation = BuildValidationType.BuildFileName;
                    break;
                case "Individual Script Hash":
                    buildValidation = BuildValidationType.IndividualScriptHash;
                    break;
                case "Individual Script Name":
                    buildValidation = BuildValidationType.IndividualScriptName;
                    break;
                case "Build File Hash":
                default:
                    buildValidation = BuildValidationType.BuildFileHash;
                    break;
            }

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SqlSync.SqlBuild.UtilityHelper.OpenManual("RunningAdhocQueriesagainstmultiple");
        }

        private void ddOutputType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (ddOutputType.SelectedItem.ToString() != "CSV")
            {
                lblCsvWarning.Visible = true;
            }
            else
            {
                lblCsvWarning.Visible = false;
            }

        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if ((e.KeyValue < (int)Keys.D0 || e.KeyValue > (int)Keys.D9) &&
                (e.KeyValue < (int)Keys.NumPad0 || e.KeyValue > (int)Keys.NumPad9))
            {
                if (e.KeyCode != Keys.Back && e.KeyCode != Keys.Delete)
                    e.SuppressKeyPress = true;
            }

        }
    }
}
