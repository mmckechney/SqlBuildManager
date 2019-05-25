using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Text;
using System.Windows.Forms;
using System.Xml.Serialization;
using SqlSync.SprocTest;
using SqlSync.SprocTest.Configuration;
using  SqlSync.Connection;
namespace SqlSync.Test
{
    public partial class SprocTestForm : Form
    {
        string logFileName = string.Empty;
        string configurationFileName = string.Empty;
        SprocTestResults testResults = new SprocTestResults();
        int testCasesRun = 0;
        int testsPassed = 0;
        int testsFailed = 0;
        SqlSync.SprocTest.Configuration.Database cfg;
        SqlSync.Connection.ConnectionData connData;
        private SprocTestForm()
        {
            InitializeComponent();
        }
        public SprocTestForm(Database cfg, ConnectionData connData, string configFileName, string logFileName)
            : this(cfg, connData, configFileName)
        {
            this.logFileName = logFileName;
        }
        public SprocTestForm(Database cfg, ConnectionData connData, string configFileName)
            : this()
        {
            this.cfg = cfg;
            this.connData = connData;
            this.configurationFileName = configFileName;
        }
        //private TestCase testCase = null;
        List<string> procsTested = new List<string>();
        public SqlSync.SprocTest.Configuration.TestCase TestCase
        {
            get
            {
                if (listView1.SelectedItems.Count > 0)
                    if (listView1.SelectedItems[0].Tag is TestManager.TestResult)
                        return ((TestManager.TestResult)listView1.SelectedItems[0].Tag).TestCase;

                return null;
            }
        }


        private void SprocTestForm_Load(object sender, EventArgs e)
        {
            if (this.logFileName.Length > 0)
                this.WindowState = FormWindowState.Minimized;

            bgTestRunner.RunWorkerAsync();
        }

        private void bgTestRunner_DoWork(object sender, DoWorkEventArgs e)
        {
            testResults.StartTime = DateTime.Now;
            testResults.TargetServer = this.connData.SQLServerName;
            testResults.TargetDatabase = this.connData.DatabaseName;
            testResults.TestConfigurationFile = this.configurationFileName;
            if(this.cfg.StoredProcedure != null)
                testResults.StoredProceduresTested = this.cfg.StoredProcedure.Length;

            ((BackgroundWorker)sender).ReportProgress(-1);
            TestManager mgr = new TestManager(this.cfg, this.connData);
            mgr.StartTests(sender as BackgroundWorker, e);

        }

        private void bgTestRunner_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.ProgressPercentage < 0)
            {
                statGeneral.Text = "Running Tests";
                statProgressBar.Style = ProgressBarStyle.Marquee;
                this.testCasesRun = 0;
            }
            else if (e.UserState != null && e.UserState is TestManager.TestResult)
            {
                this.testResults.TestCasesRun++;
                this.testCasesRun++;
                TestManager.TestResult args = (TestManager.TestResult)e.UserState;
                testResults.Results.Add(args);
                ListViewItem item = new ListViewItem(new string[] { args.Passed.ToString(), args.StoredProcedureName, args.TestCaseName, args.StatusMessage.Trim().Replace('\r', ':').Replace('\n', ' ') });
                item.Tag = args;
                if (args.Passed == false)
                {
                    item.BackColor = Color.Tomato;
                    this.testResults.FailedTests++;
                    testsFailed++;
                    statFailed.Text = "Failed: " + testsFailed.ToString();

                }
                else
                {
                    this.testResults.PassedTests++;
                    testsPassed++;
                    statPassed.Text = "Passed: " + testsPassed.ToString();
                }

                if (!procsTested.Contains(args.StoredProcedureName))
                {
                    procsTested.Add(args.StoredProcedureName);
                    statSpCount.Text = "Stored Procedures Tested: " + procsTested.Count.ToString();
                }

                listView1.Items.Insert(0, item);
                statTestCaseRunCount.Text = "Test Cases Run: " + this.testCasesRun.ToString();
            }
        }

        private void bgTestRunner_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.testResults.EndTime = DateTime.Now;
            if (e.Cancelled)
                statGeneral.Text = "Testing Cancelled";
            else
                statGeneral.Text = " Testing Complete";

            statProgressBar.Style = ProgressBarStyle.Blocks;
            if (this.logFileName.Length > 0)
            {
                SaveTestResults(this.logFileName);
                Program.WriteLog("Stored Procedure Testing Complete.\r\nResults saved to: " + this.logFileName);
                this.Close();
            }
            btnCancel.Text = "Close";
        }
        private string SaveTestResults(string fileName)
        {
            try
            {
                using (StreamWriter sw = new StreamWriter(fileName, false))
                {
                    XmlSerializer xmlS = new XmlSerializer(typeof(SprocTestResults));
                    xmlS.Serialize(sw, this.testResults);
                    sw.Flush();
                    sw.Close();
                }
                return string.Empty;
            }
            catch (Exception exe)
            {
                Program.WriteLog("Error saving Stored Procedure Test Results.\r\n" + exe.ToString());
                return exe.ToString();
            }
        }
        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (((Button)sender).Text != "Close")
                this.bgTestRunner.CancelAsync();
            else
                this.Close();
        }
        public event TestCaseSelectedEventHandler TestCaseSelected;

        private void listView1_Click(object sender, EventArgs e)
        {
            if (listView1.SelectedItems.Count > 0 &&
               listView1.SelectedItems[0].Tag is TestManager.TestResult &&
               TestCaseSelected != null)
            {
                TestManager.TestResult args = (TestManager.TestResult)listView1.SelectedItems[0].Tag;
                TestCaseSelected(this, new TestCaseSelectedArgs(args.StoredProcedureName, args.TestCase));
            }

        }

        private void showExecutedSqlToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count > 0)
            {
                PasteScriptForm frmPaste = new PasteScriptForm();
                frmPaste.ScriptText = this.listView1.SelectedItems[0].SubItems[4].Text;
                frmPaste.ShowDialog();
            }
        }

        private void showDetailedResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count > 0)
            {
                ResultsViewForm frmR = new ResultsViewForm(this.listView1.SelectedItems[0].Tag as TestManager.TestResult);
                frmR.ShowDialog();
            }
        }

        private void saveAllTestResultsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == saveFileDialog1.ShowDialog())
            {
                string fileName = saveFileDialog1.FileName;
                saveFileDialog1.Dispose();

                string result = SaveTestResults(fileName);
                if(result.Length > 0)
                    MessageBox.Show("Error saving results.\r\n" + result, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);

            }
        }
    }
    public delegate void TestCaseSelectedEventHandler(object sender,TestCaseSelectedArgs args);
    public class TestCaseSelectedArgs : EventArgs
    {
        public readonly SqlSync.SprocTest.Configuration.TestCase TestCase;
        public readonly string StoredProcedureName;
        public TestCaseSelectedArgs(string storedProcedureName, SqlSync.SprocTest.Configuration.TestCase testCase)
        {
            this.StoredProcedureName = storedProcedureName;
            this.TestCase = testCase;
        }
    }
}