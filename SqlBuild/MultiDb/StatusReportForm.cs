using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using SqlSync.ObjectScript.Hash;
using SqlSync.SqlBuild.AdHocQuery;
using SqlSync.SqlBuild.Status;
using SqlSync.Connection;
namespace SqlSync.SqlBuild.MultiDb
{
    public partial class StatusReportForm : Form
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private SqlSyncBuildData buildData;
        protected MultiDb.MultiDbData multiDbData;
        private string projectFilePath;
        private string buildZipFileName;
        protected int dbTotal = 0;
        protected string fileName;
        protected ConnectionData connData = null;
        private StatusReportForm()
        {
            InitializeComponent();
        }
        protected StatusReportForm(MultiDb.MultiDbData multiDbData, ConnectionData connData) : this()
        {
            this.multiDbData = multiDbData;
            this.connData = connData;
        }
        public StatusReportForm(SqlSyncBuildData buildData, MultiDb.MultiDbData multiDbData, string projectFilePath, string buildZipFileName, ConnectionData connData)
            : this(multiDbData, connData)
        {
            
            this.buildData = buildData;
            this.buildZipFileName = buildZipFileName;
            this.projectFilePath = projectFilePath;
            this.ddOutputType.SelectedIndex = 0;
        }

        private void StatusReport_Load(object sender, EventArgs e)
        {
            if (this.multiDbData != null)
            {
                this.lblServerCount.Text = this.multiDbData.Count.ToString();
                foreach (ServerData sData in this.multiDbData)
                    dbTotal += sData.OverrideSequence.Count;

                this.lblDatabaseCount.Text = dbTotal.ToString();
            }

        }

        protected void btnGenerate_Click(object sender, EventArgs e)
        {
            ReportType reportType;
            switch (ddOutputType.SelectedItem.ToString().ToLower())
            {
                case "html":
                    saveOutputFileDialog.DefaultExt = ".html";
                    saveOutputFileDialog.Filter = "HTML File (*.html)|*.html|All Files (*.*)|*.*";
                    reportType = ReportType.HTML;
                    break;
                case "summary":
                    saveOutputFileDialog.DefaultExt = ".html";
                    saveOutputFileDialog.Filter = "HTML File (*.html)|*.html|All Files (*.*)|*.*";
                    reportType = ReportType.Summary;
                    break;
                case "xml":
                    saveOutputFileDialog.DefaultExt = ".xml";
                    saveOutputFileDialog.Filter = "XML File (*.xml)|*.xml|All Files (*.*)|*.*";
                    reportType = ReportType.XML;
                    break;
                default:
                    saveOutputFileDialog.DefaultExt = ".csv";
                    saveOutputFileDialog.Filter = "CSV File (*.csv)|*.csv|All Files (*.*)|*.*";
                    reportType = ReportType.CSV;
                    break;
            }

            if (DialogResult.OK == saveOutputFileDialog.ShowDialog())
            {
                
                this.fileName = saveOutputFileDialog.FileName;
                statProgressBar.Style = ProgressBarStyle.Marquee;
                statGeneral.Text = "Working...";
                KeyValuePair<ReportType, string> report = new KeyValuePair<ReportType, string>(reportType, fileName);
                this.bgWorker.RunWorkerAsync(report);
                this.buildDuration = 0;
                this.timer1.Start();
            }
       }

        protected virtual void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            KeyValuePair<ReportType, string> args  = (KeyValuePair<ReportType, string>)e.Argument;
            SqlBuild.Status.StatusReporting report = new SqlSync.SqlBuild.Status.StatusReporting(this.buildData, multiDbData, this.projectFilePath,this.buildZipFileName);
            string xml = report.GetScriptStatus(ref this.bgWorker, args.Value, args.Key);
            e.Result = xml;

            bgWorker.ReportProgress(0, "Generating report output");
        }

        protected void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if(e.UserState is string)
            {                
                statDbProcessed.Text = "Databases Processed: " + (this.dbTotal - e.ProgressPercentage).ToString() + " of " + dbTotal.ToString();
            }
        }

        protected void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.timer1.Stop();
            statProgressBar.Style = ProgressBarStyle.Blocks;

            if (e.Result is string || e.Result is ObjectScriptHashReportData || e.Result is bool)
            {
                statGeneral.Text = "Report Complete.";
                if (!File.Exists(this.fileName))
                {
                    MessageBox.Show("Oh no!! The report query completed but no report file was generated.\r\nPlease check the Application log file for further details.\r\n\r\n(My guess is that there is an issue with your query)", "No file generated", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                }
                else if (DialogResult.Yes == MessageBox.Show("Report file is ready. Do you want to open it now?", "Report Ready", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                {
                    System.Diagnostics.Process prc = new System.Diagnostics.Process();
                    prc.StartInfo.FileName = this.fileName;
                    prc.Start();
                }
            }
            else if (e.Result is Exception)
            {
                log.Error("Error executing query", (Exception)e.Result);
                MessageBox.Show("An error was encountered during execution!\r\n(How much data were you expecting anyway!?!)\r\n\r\n" + ((Exception)e.Result).Message, "Sorry.. I died", MessageBoxButtons.OK, MessageBoxIcon.Error);
                statGeneral.Text = "Error!!";

            }
            
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            SqlSync.Utility.OpenManual("ScriptStatusReporting");
        }

        protected int buildDuration = 0;
        protected void timer1_Tick(object sender, EventArgs e)
        {
            buildDuration++;
            TimeSpan t = new TimeSpan(0, 0, buildDuration);
            statExecutionTime.Text = "Execution Time - " + t.ToString();
        
        }
    }
}
