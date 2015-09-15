using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SqlSync.SqlBuild.AdHocQuery;
using SqlSync.SqlBuild.Status;
using SqlSync.MRU;
using System.IO;
using SqlSync.Controls;
using SqlSync.Connection;
namespace SqlSync.SqlBuild.MultiDb
{
    public partial class BuildValidationForm : SqlSync.SqlBuild.MultiDb.StatusReportForm
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
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
            this.btnGenerate.Click -= new EventHandler(this.btnGenerate_Click);
            this.btnGenerate.Click -= new EventHandler(this.btnGenerate_Click_1);

            //Get this form's click handler to fire first...
            this.btnGenerate.Click += new EventHandler(this.btnGenerate_Click_1);
            this.btnGenerate.Click += new EventHandler(this.btnGenerate_Click);
        }

        protected override void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            KeyValuePair<ReportType, string> args = (KeyValuePair<ReportType, string>)e.Argument;
            this.collector = new QueryCollector(this.multiDbData, this.connData);
            //this.rawReportData = 
            try
            {
                collector.GetBuildValidationResults(ref bgWorker, args.Value, this.txtCheckValue.Text, args.Key, this.buildValidation, this.timeOut);
                //collector.GetQueryResults(ref this.bgWorker, args.Value,args.Key, this.query, this.timeOut);
                bgWorker.ReportProgress(0, "Generating report output");
                e.Result = true; // rawReportData;
            }
            catch(Exception exe)
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
            this.checkFieldValue = this.txtCheckValue.Text;
            if (!int.TryParse(this.txtTimeout.Text, out this.timeOut))
                this.timeOut = 20;

            switch (ddValidationType.SelectedItem.ToString())
            {
                    
                case "Build File Name":
                    this.buildValidation = BuildValidationType.BuildFileName;
                    break;
                case "Individual Script Hash":
                     this.buildValidation = BuildValidationType.IndividualScriptHash;
                    break;
                case "Individual Script Name":
                    this.buildValidation = BuildValidationType.IndividualScriptName;
                    break;
                case "Build File Hash":
                  default:
                    this.buildValidation = BuildValidationType.BuildFileHash;
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
            if( (e.KeyValue < (int) Keys.D0 || e.KeyValue > (int) Keys.D9) &&
                (e.KeyValue < (int) Keys.NumPad0 || e.KeyValue > (int) Keys.NumPad9))
            {
                if(e.KeyCode != Keys.Back && e.KeyCode != Keys.Delete)
                    e.SuppressKeyPress = true;
            }
           
        }
    }
}
