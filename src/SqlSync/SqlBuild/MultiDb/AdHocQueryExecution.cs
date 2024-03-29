﻿using Microsoft.Extensions.Logging;
using SqlSync.Connection;
using SqlSync.MRU;
using SqlSync.SqlBuild.AdHocQuery;
using SqlSync.SqlBuild.Status;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows.Forms;
namespace SqlSync.SqlBuild.MultiDb
{
    public partial class AdHocQueryExecution : SqlSync.SqlBuild.MultiDb.StatusReportForm, IMRUClient
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        MRUManager mruManager = new MRUManager();
        QueryCollector collector;
        string query = string.Empty;
        //List<QueryResultData> rawReportData;
        int timeOut = 20;

        public AdHocQueryExecution(MultiDb.MultiDbData multiDbData, ConnectionData connData)
            : base(multiDbData, connData)
        {
            InitializeComponent();

            base.ddOutputType.Items.Clear();
            base.ddOutputType.Items.Add("CSV");
            base.ddOutputType.Items.Add("HTML");
            base.ddOutputType.Items.Add("XML");
            base.ddOutputType.SelectedIndex = 0;
            base.btnGenerate.Click -= new EventHandler(btnGenerate_Click);
            btnGenerate.Click -= new EventHandler(btnGenerate_Click_1);

            //Get this form's click handler to fire first...
            btnGenerate.Click += new EventHandler(btnGenerate_Click_1);
            //base.btnGenerate.Click += new EventHandler(this.btnGenerate_Click);
        }

        protected override void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            KeyValuePair<ReportType, string> args = (KeyValuePair<ReportType, string>)e.Argument;
            collector = new QueryCollector(multiDbData, connData);
            collector.BackgroundWorker = bgWorker;
            
            //this.rawReportData = 
            try
            {
                collector.GetQueryResults(args.Value, args.Key, query, timeOut);
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

        protected void btnGenerate_Click_1(object sender, EventArgs e)
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
            query = rtbSqlScript.Text;
            if (!int.TryParse(txtTimeout.Text, out timeOut))
                timeOut = 20;

            base.btnGenerate_Click(sender, e);

        }

        /// <summary>
        /// Initializes the "Recent Files" menu option off the "Actions" menu
        /// </summary>
        private void InitMRU()
        {
            mruManager = new MRUManager();
            mruManager.Initialize(
                this,                              // owner form
                actionToolStripMenuItem,
                recentFilesToolStripMenuItem,                        // Recent Files menu item
                @"Software\Michael McKechney\Sql Sync\AdHoc Query"); // Registry path to keep MRU list
            mruManager.MaxDisplayNameLength = 40;
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
            rtbSqlScript.Text = File.ReadAllText(fileName);
            mruManager.Add(fileName);

        }

        private void saveScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == saveFileDialog1.ShowDialog())
            {
                File.WriteAllText(saveFileDialog1.FileName, rtbSqlScript.Text);
                mruManager.Add(saveFileDialog1.FileName);

            }
        }

        private void AdHocQueryExecution_Load(object sender, EventArgs e)
        {
            InitMRU();

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
