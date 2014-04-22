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
using System.Text.RegularExpressions;
namespace SqlSync.SqlBuild.MultiDb
{
    public partial class AdHocQueryExecution : SqlSync.SqlBuild.MultiDb.StatusReportForm, IMRUClient
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        MRUManager mruManager = new MRUManager();
        QueryCollector collector;
        string query = string.Empty;
        //List<QueryResultData> rawReportData;
        int timeOut = 20;
        public AdHocQueryExecution(MultiDb.MultiDbData multiDbData) : base(multiDbData)
        {
            InitializeComponent();

            base.ddOutputType.Items.Clear();
            base.ddOutputType.Items.Add("CSV");
            base.ddOutputType.Items.Add("HTML");
            base.ddOutputType.Items.Add("XML");
            base.ddOutputType.SelectedIndex = 0;
            base.btnGenerate.Click -= new EventHandler(this.btnGenerate_Click);
            this.btnGenerate.Click -= new EventHandler(this.btnGenerate_Click_1);

            //Get this form's click handler to fire first...
            this.btnGenerate.Click += new EventHandler(this.btnGenerate_Click_1);
            //base.btnGenerate.Click += new EventHandler(this.btnGenerate_Click);
        }

        protected override void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            KeyValuePair<ReportType, string> args = (KeyValuePair<ReportType, string>)e.Argument;
            this.collector = new QueryCollector(this.multiDbData);
            //this.rawReportData = 
            try
            {
                collector.GetQueryResults(ref this.bgWorker, args.Value, args.Key, this.query, this.timeOut);
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

        protected new void btnGenerate_Click_1(object sender, EventArgs e)
        {
            Regex noNo = new Regex(@"(UPDATE\s)|(INSERT\s)|(DELETE\s)", RegexOptions.IgnoreCase);
            //if (noNo.Match(rtbSqlScript.Text).Success)
            //{
            //    MessageBox.Show("In INSERT, UPDATE or DELETE keyword was found. You can not use the AdHoc query function to modify data.\r\n\r\nInstead, please run your data modification script as a SQL Build Package", "No Data Manipulation Allowed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            //    this.btnGenerate.Click -= new EventHandler(this.btnGenerate_Click);
            //    return;
            //}

            base.buildDuration = 0;
            base.timer1.Start();
            this.query = this.rtbSqlScript.Text;
            if (!int.TryParse(this.txtTimeout.Text, out this.timeOut))
                this.timeOut = 20;

            base.btnGenerate_Click(sender, e);

        }

        /// <summary>
        /// Initializes the "Recent Files" menu option off the "Actions" menu
        /// </summary>
        private void InitMRU()
        {
            this.mruManager = new MRUManager();
            this.mruManager.Initialize(
                this,                              // owner form
                actionToolStripMenuItem,
                recentFilesToolStripMenuItem,                        // Recent Files menu item
                @"Software\Michael McKechney\Sql Sync\AdHoc Query"); // Registry path to keep MRU list
            this.mruManager.MaxDisplayNameLength = 40;
        }

        #region IMRUClient Members

        public void OpenMRUFile(string fileName)
        {
            if (File.Exists(fileName))
            {
                LoadSqlScript(fileName);
            }
        }

        #endregion

        private void openScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == openFileDialog1.ShowDialog())
            {
                LoadSqlScript(openFileDialog1.FileName);
            }
        }
        private void LoadSqlScript(string fileName)
        {
            this.rtbSqlScript.Text = File.ReadAllText(fileName);
            this.mruManager.Add(fileName);

        }

        private void saveScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == saveFileDialog1.ShowDialog())
            {
                File.WriteAllText(saveFileDialog1.FileName, rtbSqlScript.Text);
                this.mruManager.Add(saveFileDialog1.FileName);

            }
        }

        private void AdHocQueryExecution_Load(object sender, EventArgs e)
        {
            this.InitMRU();

        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SqlSync.Utility.OpenManual("RunningAdhocQueriesagainstmultiple");
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
