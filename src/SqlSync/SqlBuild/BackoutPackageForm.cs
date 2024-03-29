﻿using Microsoft.Extensions.Logging;
using SqlSync.Connection;
using SqlSync.ObjectScript;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
namespace SqlSync.SqlBuild
{
    public partial class BackoutPackageForm : Form
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private ConnectionData connData = null;
        private List<SqlBuild.Objects.ObjectUpdates> initialCanUpdateList;
        private List<SqlBuild.Objects.ObjectUpdates> currentTargetCanUpdateList = new List<Objects.ObjectUpdates>();
        private List<SqlBuild.Objects.ObjectUpdates> notPresentOnTarget;
        private List<string> manualScriptsCanNotUpdate;
        private string sourceSbmFullFileName;
        private string extractedPath;
        private string extractedProjectFile;
        private SqlSyncBuildData sourceBuildData;
        private bool removeNewObjectsFromPackage = true;
        private bool markManualObjectsAsRunOnce = true;
        private bool dropRoutines = true;

        private BackoutPackageForm()
        {
            InitializeComponent();
        }
        public BackoutPackageForm(ConnectionData connData, SqlSyncBuildData sourceBuildData, string sourceSbmFullFileName, string extractedPath, string extractedProjectFile)
            : this()
        {
            this.connData = connData;
            this.sourceSbmFullFileName = sourceSbmFullFileName;
            this.extractedPath = extractedPath;
            this.extractedProjectFile = extractedProjectFile;
            this.sourceBuildData = sourceBuildData;
        }
        private void mnuChangeSqlServer_Click(object sender, System.EventArgs e)
        {
            pnlSqlConnect.Visible = true;
            pnlSqlConnect.BringToFront();
        }

        private void btnChangeSource_Click(object sender, EventArgs e)
        {
            if (sqlConnect1.Database.Length == 0)
            {
                MessageBox.Show("Please select a database first!", "Missing Database", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }


            string database = sqlConnect1.Database;
            string server = sqlConnect1.SQLServer;

            lblServerSetting.Text = server;
            lblDatabaseSetting.Text = database;


            SetSourceServerAndDatabase(server, database);

            pnlSqlConnect.SendToBack();
            pnlSqlConnect.Visible = false;


        }

        private void SetSourceServerAndDatabase(string serverName, string databaseName)
        {
            BackoutPackage.SetBackoutSourceDatabaseAndServer(ref initialCanUpdateList, serverName, databaseName);

            if (serverName.Length > 0 && databaseName.Length > 0)
            {
                while (bgCheckTargetObjects.IsBusy)
                {
                    Application.DoEvents();
                }

                bgCheckTargetObjects.RunWorkerAsync(new string[] { serverName, databaseName });
            }

        }
        private void BackoutPackageForm_Load(object sender, EventArgs e)
        {
            try
            {

                SqlBuildFileHelper.GetFileDataForObjectUpdates(ref sourceBuildData, extractedProjectFile, out initialCanUpdateList, out manualScriptsCanNotUpdate);

                string pleaseSelect = "[Please select a {0} via Action --> Change SQL Server Connection menu]";
                lblDatabaseSetting.Text = connData.DatabaseName;
                lblServerSetting.Text = connData.SQLServerName;

                if (lblDatabaseSetting.Text.Length == 0)
                    lblDatabaseSetting.Text = String.Format(pleaseSelect, "database");

                if (lblServerSetting.Text.Length == 0)
                    lblServerSetting.Text = String.Format(pleaseSelect, "server");

                txtBackoutPackage.Text = BackoutPackage.GetDefaultPackageName(sourceSbmFullFileName);

                SetSourceServerAndDatabase(connData.SQLServerName, connData.DatabaseName);

                if (connData.SQLServerName.Length > 0 && connData.DatabaseName.Length > 0 && !bgCheckTargetObjects.IsBusy)
                    bgCheckTargetObjects.RunWorkerAsync(new string[] { connData.SQLServerName, connData.DatabaseName });
                else
                    BindListViews(initialCanUpdateList, notPresentOnTarget, manualScriptsCanNotUpdate);


            }
            catch (Exception exe)
            {
                log.LogError(exe, "Error loading the Backout Package form");
                MessageBox.Show("Error loading form. Please see log file for details", "Whoops!", MessageBoxButtons.OK, MessageBoxIcon.Error);

                bgCheckTargetObjects = null;
                bgMakeBackout = null;
                Close();
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == saveFileDialog1.ShowDialog())
            {
                txtBackoutPackage.Text = saveFileDialog1.FileName;
            }
        }

        private void lblDatabaseSetting_TextChanged(object sender, EventArgs e)
        {
            if (sender is Label)
            {
                Label lbl = (Label)sender;
                if (lbl.Text.StartsWith("["))
                    lbl.ForeColor = Color.Red;
                else
                {
                    lbl.ForeColor = Color.Black;
                }
            }

        }

        private void BindListViews(List<SqlBuild.Objects.ObjectUpdates> canUpdate, List<SqlBuild.Objects.ObjectUpdates> notOnTarget, List<string> notUpdateable)
        {
            lstObjectNotUpdateable.Items.Clear();
            lstObjectsToUpdate.Items.Clear();

            if (manualScriptsCanNotUpdate != null)
            {
                foreach (Objects.ObjectUpdates script in canUpdate)
                {
                    ListViewItem item = new ListViewItem(new string[] { script.ShortFileName });
                    item.Tag = script;
                    lstObjectsToUpdate.Items.Add(item);
                }
            }

            if (notOnTarget != null)
            {
                foreach (Objects.ObjectUpdates script in notOnTarget)
                {
                    ListViewItem item = new ListViewItem(new string[] { script.ShortFileName });
                    item.Tag = script;
                    item.Group = lstObjectNotUpdateable.Groups["grpNotPresent"];
                    lstObjectNotUpdateable.Items.Add(item);
                }
            }

            if (notUpdateable != null)
            {
                foreach (string tmp in notUpdateable)
                {
                    ListViewItem item = new ListViewItem(new string[] { tmp });
                    item.Tag = tmp;
                    item.Group = lstObjectNotUpdateable.Groups["grpNotUpdatable"];
                    lstObjectNotUpdateable.Items.Add(item);
                }
            }
        }

        private void ViewScript(object sender, EventArgs e)
        {
            ListView callingListView = null;
            if (sender is ToolStripDropDownItem)
            {
                ToolStripDropDownItem x = (ToolStripDropDownItem)sender;
                ContextMenuStrip strip = (ContextMenuStrip)x.Owner;
                if (strip.SourceControl is ListView)
                    callingListView = (ListView)strip.SourceControl;


            }
            else if (sender is ListView)
                callingListView = (ListView)sender;

            if (callingListView == null || callingListView.SelectedItems.Count == 0)
                return;

            string fileName = callingListView.SelectedItems[0].Text;

            if (File.Exists(Path.Combine(extractedPath, fileName)))
            {
                string scriptContents = File.ReadAllText(Path.Combine(extractedPath, fileName));
                ScriptDisplayForm frmDisp = new ScriptDisplayForm(scriptContents, "", fileName, Highlighting.SyntaxHightlightType.Sql);
                frmDisp.ShowDialog();
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            bgCheckTargetObjects = null;
            bgMakeBackout = null;
            Close();
        }

        private void btnCreate_Click(object sender, EventArgs e)
        {
            if (txtBackoutPackage.TextLength == 0 ||
                lblDatabaseSetting.ForeColor == Color.Red ||
                lblServerSetting.ForeColor == Color.Red)
            {
                MessageBox.Show("Please make sure you have all required values set:\r\n1. Source server\r\n2. Source database\r\n3.Backout Package File Name", "Missing Info", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            Cursor = Cursors.WaitCursor;
            statBar.Style = ProgressBarStyle.Marquee;
            btnCreate.Enabled = false;
            removeNewObjectsFromPackage = chkRemoveNewScripts.Checked;
            markManualObjectsAsRunOnce = chkManualAsRunOnce.Checked;
            dropRoutines = chkDropRoutines.Checked;
            bgMakeBackout.RunWorkerAsync(new List<string>(new string[] { txtBackoutPackage.Text, lblServerSetting.Text, lblDatabaseSetting.Text }));

        }

        #region Creating Backout Package BG worker event handlers
        private void bgMakeBackout_DoWork(object sender, DoWorkEventArgs e)
        {
            List<string> vals = (List<string>)e.Argument;
            e.Result = BackoutPackage.CreateBackoutPackage(connData, currentTargetCanUpdateList, notPresentOnTarget, manualScriptsCanNotUpdate,
                sourceSbmFullFileName, vals[0], vals[1], vals[2], removeNewObjectsFromPackage, markManualObjectsAsRunOnce, dropRoutines, ref bgMakeBackout);
        }

        private void bgMakeBackout_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (e.UserState is string)
                statGeneral.Text = e.UserState.ToString();
        }

        private void bgMakeBackout_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            Cursor = Cursors.Default;
            statBar.Style = ProgressBarStyle.Blocks;
            statGeneral.Text = "Ready.";
            btnCreate.Enabled = true;
            if (((bool)e.Result) == true)
            {
                if (DialogResult.Yes == MessageBox.Show("Backout package was successfully created.\r\nDo you want to open it now?", "Success!", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                {
                    System.Diagnostics.Process prc = new System.Diagnostics.Process();
                    prc.StartInfo.FileName = txtBackoutPackage.Text;
                    prc.Start();
                }
            }
            else
            {
                if (File.Exists(txtBackoutPackage.Text))
                    File.Delete(txtBackoutPackage.Text);

                MessageBox.Show("There was an error creating the backout package.\r\nPlease check the log file via the Help --> View Application Log File menu", "Whoops. Something when wrong!", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }
        #endregion

        #region Checking Target DB Objects BG worker methods and event handlers

        private void bgCheckTargetObjects_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bg = (BackgroundWorker)sender;

            if (bg.WorkerReportsProgress)
                bg.ReportProgress(-1, "Checking new database target for presence of scriptable objects...");

            string[] args = (string[])e.Argument;

            notPresentOnTarget = BackoutPackage.GetObjectsNotPresentTargetDatabase(initialCanUpdateList, connData, args[0], args[1]);
            if (notPresentOnTarget.Count > 0)
            {
                var cur = from i in initialCanUpdateList
                          where !(from n in notPresentOnTarget select n.SourceObject).Contains(i.SourceObject)
                          select i;

                if (cur.Any())
                    currentTargetCanUpdateList = cur.ToList();
                else
                    currentTargetCanUpdateList.Clear();
            }
            else
            {
                currentTargetCanUpdateList.Clear();
                currentTargetCanUpdateList.AddRange(initialCanUpdateList);
            }


        }

        private void bgCheckTargetObjects_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            if (statBar.Style != ProgressBarStyle.Marquee)
                statBar.Style = ProgressBarStyle.Marquee;

            if (Cursor != Cursors.AppStarting)
                Cursor = Cursors.AppStarting;

            if (e.UserState is string)
                statGeneral.Text = e.UserState.ToString();
        }

        private void bgCheckTargetObjects_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            BindListViews(currentTargetCanUpdateList, notPresentOnTarget, manualScriptsCanNotUpdate);

            statBar.Style = ProgressBarStyle.Blocks;
            statGeneral.Text = "Ready.";
            Cursor = Cursors.Default;
        }
        #endregion

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SqlSync.SqlBuild.UtilityHelper.OpenManual("CreatingBackoutPackage");
        }

        private void lstObjectsToUpdate_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void label12_Click(object sender, EventArgs e)
        {

        }

        private void label9_Click(object sender, EventArgs e)
        {

        }

        private void lnkCopyList_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            StringBuilder sbNotFound = new StringBuilder();
            StringBuilder sbManual = new StringBuilder();

            foreach (ListViewItem item in lstObjectNotUpdateable.Items)
            {
                if (item.Group.Name == "grpNotUpdatable")
                {
                    sbManual.AppendLine(item.Text);
                }
                else //"grpNotPresent"
                {
                    sbNotFound.AppendLine(item.Text);
                }
            }

            StringBuilder sb = new StringBuilder();
            sb.AppendLine("* Not Found on Target Server *");
            sb.Append(sbNotFound.ToString());
            sb.AppendLine();
            sb.AppendLine("* Manual Scripts *");
            sb.Append(sbManual.ToString());
            Clipboard.SetText(sb.ToString());

        }




    }
}
