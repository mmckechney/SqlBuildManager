using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SqlBuildManager.Enterprise.Policy;
using SqlBuildManager.Interfaces.ScriptHandling.Policy;
using System.IO;
namespace SqlSync.SqlBuild.Policy
{
    public partial class PolicyForm : Form
    {
        ColumnSorter listSorter = new ColumnSorter();
        SqlSyncBuildData buildData = null;
        string projectFilePath = string.Empty;
        public PolicyForm(ref SqlSyncBuildData buildData, string projectFilePath)
        {
            InitializeComponent();
            this.buildData = buildData;
            this.projectFilePath = projectFilePath;
        }

        private void PolicyForm_Load(object sender, EventArgs e)
        {


            if (this.buildData == null || this.buildData.Script == null)
            {
                MessageBox.Show("You need to have an open build project prior to running policy checks", "Project needed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.Close();
            }

            List<IScriptPolicy> lstPol = PolicyHelper.GetPolicies();
            for (int i = 0; i < lstPol.Count; i++)
            {
                ListViewItem item = new ListViewItem(new string[] { lstPol[i].ShortDescription, lstPol[i].LongDescription });
                item.Checked = true;
                item.Tag = lstPol[i];
                lstPolicies.Items.Add(item);
            }

            listSorter.CurrentColumn = 2;
            listSorter.Sort = SortOrder.Ascending;
            lstResults.ListViewItemSorter = listSorter;
            lstResults.Sort();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            this.lstResults.Items.Clear();

            List<IScriptPolicy> selectedPolices = new List<IScriptPolicy>();
            for(int i=0;i<lstPolicies.CheckedItems.Count;i++)
                selectedPolices.Add((IScriptPolicy)lstPolicies.CheckedItems[i].Tag);

            statGeneral.Text = "Checking scripts for policy compliance...";
            statProgress.Style = ProgressBarStyle.Marquee;
            bgWorker.RunWorkerAsync(selectedPolices);
        }

        private void lstResults_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            listSorter.CurrentColumn = e.Column;
            lstResults.ListViewItemSorter = listSorter;
            lstResults.Sort();
        }

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {
            if( !(sender is BackgroundWorker))
                return;

            BackgroundWorker bg = (BackgroundWorker)sender;
            if(!(e.Argument is List<IScriptPolicy>))
            {
                bg.ReportProgress(0,"Error running. Policies not selected");
                return;
            }
            List<IScriptPolicy> selectedPolices = (List<IScriptPolicy>)e.Argument;

            string message, fileName, script, targetDatabase;
            bool passed;
            foreach (SqlSyncBuildData.ScriptRow row in this.buildData.Script)
            {
                fileName = Path.Combine(this.projectFilePath , row.FileName);
                if (File.Exists(fileName))
                {
                    script = File.ReadAllText(fileName);
                    targetDatabase = row.Database;
                    for (int i = 0; i < selectedPolices.Count; i++)
                    {
                        passed = false;
                        message = string.Empty;

                        if (selectedPolices[i] is CommentHeaderPolicy)
                            ((CommentHeaderPolicy)selectedPolices[i]).DayThreshold = 40;

                        Violation tmp = PolicyHelper.ValidateScriptAgainstPolicy(script, targetDatabase, selectedPolices[i]);
                        
                        if (tmp == null)
                            passed = true;
                        else
                            message = tmp.Message;

                        PolicyMessage msg = new PolicyMessage(row.FileName, selectedPolices[i].ShortDescription, passed, message,row);
                        bg.ReportProgress(0, msg);
                    }
                }
            }
        }

        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is PolicyMessage)
            {
                PolicyMessage msg = (PolicyMessage)e.UserState;

                string changeDate = (msg.ScriptRow.DateModified == DateTime.MinValue) ? msg.ScriptRow.DateAdded.ToString() : msg.ScriptRow.DateModified.ToString();

                ListViewItem item = new ListViewItem(new string[] { msg.ScriptName,changeDate, msg.PolicyType, msg.Passed.ToString(), msg.Message });
                item.Tag = msg.ScriptRow;
                lstResults.Items.Insert(0,item);
            }
        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            statGeneral.Text = "Check complete.";
            statProgress.Style = ProgressBarStyle.Blocks;
        }

        public event ScriptSelectedHandler ScriptSelected;
        public delegate void ScriptSelectedHandler(object sender, ScriptSelectedEventArgs e);

        private class PolicyMessage
        {
            public readonly string ScriptName; 
            public readonly string PolicyType;
            public readonly bool Passed;
            public readonly string Message;
            public readonly SqlSyncBuildData.ScriptRow ScriptRow;
            public PolicyMessage(string scriptName, string policyType, bool passed, string message, SqlSyncBuildData.ScriptRow scriptRow)
            {
                this.ScriptName = scriptName;
                this.PolicyType = policyType;
                this.Passed = passed;
                this.Message = message;
                this.ScriptRow = scriptRow;
            }
        }

        private void lstResults_DoubleClick(object sender, EventArgs e)
        {
            if (lstResults.SelectedItems.Count == 0)
                return;

            if(this.ScriptSelected != null)
                this.ScriptSelected(this,new ScriptSelectedEventArgs((SqlSyncBuildData.ScriptRow)lstResults.SelectedItems[0].Tag));
        }

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SqlSync.SqlBuild.UtilityHelper.OpenManual("ManualPolicyCheckingofBuildPackage");
        }
    }
    public class ScriptSelectedEventArgs : EventArgs
    {
        public SqlSyncBuildData.ScriptRow SelectedRow = null;
        public ScriptSelectedEventArgs(SqlSyncBuildData.ScriptRow selectedRow)
        {
            this.SelectedRow = selectedRow;
        }
    }
}
