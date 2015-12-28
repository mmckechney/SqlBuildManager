using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SqlSync.SprocTest.Configuration;
using SqlSync.SprocTest;
using System.IO;
using SqlSync.DbInformation;
using SqlSync.Connection;

namespace SqlSync.Test
{
    public partial class SprocTestConfigForm : Form, SqlSync.MRU.IMRUClient 
    {
        private MRU.MRUManager mruManager = null;
        private Font normalFont = new Font("Microsoft Sans Serif", 8);
        private Font highlightFont = new Font("Microsoft Sans Serif", 8,FontStyle.Italic);
        private int totalTestCases = 0;
        private int totalSprocs = 0;
        private int totalSprocNoTest = 0;
        string strTestCases = "Test Cases: {0}";
        string strSprocs = "Stored Procedures: {0}";
         string strSprocsNoTest = "Stored Procedures without Tests: {0}";
        TreeNode lastSelectedNode = null;
        SqlSync.SprocTest.Configuration.Database testConfig = null;
        Connection.ConnectionData connData = null;
        string configFileName = string.Empty;
        List<DbInformation.ObjectData> unusedStoredProcList = null;
        private SprocTestConfigForm()
        {
            InitializeComponent();
        }
        public SprocTestConfigForm(Connection.ConnectionData connData): this()
        {
            this.connData = connData;
        }
        public SprocTestConfigForm(string configFileName) : this()
        {
            this.configFileName = configFileName;
        }
        private void openNewTestConfigurationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK == openFileDialog1.ShowDialog())
            {
                this.configFileName = openFileDialog1.FileName;
                OpenMRUFile(this.configFileName);
            }
        }

        private void SprocTestConfigForm_Load(object sender, EventArgs e)
        {
            if (this.connData == null)
            {
                ConnectionForm frmConnect = new ConnectionForm("Stored Proc Testing");
                DialogResult result = frmConnect.ShowDialog();
                if (result == DialogResult.OK)
                {
                    this.connData = frmConnect.SqlConnection;
                }
                else
                {
                    MessageBox.Show("Stored Procedure Testing can not continue without a valid Sql Connection", "Unable to Load", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                    this.Close();
                    Application.Exit();
                }
            }

            this.settingsControl1.Server = this.connData.SQLServerName;
            if (this.configFileName.Length > 0)
            {
                try
                {
                    this.settingsControl1.Project = this.configFileName;
                    testConfig = SqlSync.SprocTest.TestManager.ReadConfiguration(this.configFileName);
                    this.connData.DatabaseName = testConfig.Name;
                    BindTreeView();
                    pgBar.Style = ProgressBarStyle.Marquee;
                    bgSprocList.RunWorkerAsync();
                }
                catch (Exception exe)
                {
                    MessageBox.Show("Error loading config file.\r\n" + exe.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

            }

            this.mruManager = new SqlSync.MRU.MRUManager();
            this.mruManager.Initialize(
                this,                              // owner form
                fileToolStripMenuItem,
                mnuFileMRU,                        // Recent Files menu item
                @"Software\Michael McKechney\Sql Sync\Stored Proc Testing"); // Registry path to keep MRU list
            this.mruManager.MaxDisplayNameLength = 40;

        }

        private void BindTreeView()
        {
            if (sprocTestConfigCtrl1.SprocName.Length > 0)
                BindTreeView(sprocTestConfigCtrl1.SprocName);
            else
                BindTreeView(Guid.NewGuid().ToString());
        }
        private void BindTreeView(string selectedSPName)
        {
            if (this.testConfig == null)
                return;

            this.treeView1.Nodes.Clear();
            this.treeView1.SuspendLayout();
            this.treeView1.Enabled = false;
            if (this.testConfig.StoredProcedure == null)
                return;

            TreeNode selected = null;
            for (int i = 0; i < this.testConfig.StoredProcedure.Length; i++)
            {
                StoredProcedure sp = this.testConfig.StoredProcedure[i];
                if (sp.ID.Length == 0) sp.ID = Guid.NewGuid().ToString();

               
                TreeNode spNode = new TreeNode(sp.Name);
                if (sp.Name == selectedSPName)
                    selected = spNode;

                spNode.Tag = sp;
                spNode.Expand();
                if (sp.TestCase != null && sp.TestCase.Length > 0)
                {
                    for (int j = 0; j < sp.TestCase.Length; j++)
                    {
                        TreeNode caseNode = new TreeNode(sp.TestCase[j].Name);
                        caseNode.Tag = sp.TestCase[j];
                        if (sp.TestCase[j] == sprocTestConfigCtrl1.TestCase)
                        {
                            caseNode.ForeColor = Color.Blue;
                            spNode.ForeColor = Color.Blue;
                        }

                        caseNode.Checked = sp.TestCase[j].SelectedForRun;
                        spNode.Nodes.Add(caseNode);
                    }
                }
                this.treeView1.Nodes.Add(spNode);
          }

          this.treeView1.CheckBoxes = true;
          this.treeView1.TreeViewNodeSorter = new NodeSorter();
          //this.treeView1.ExpandAll();
          this.treeView1.Sort();

          if (selected != null)
          {
              selected.EnsureVisible();
              this.treeView1.SelectedNode = selected;
          }

          this.treeView1.Enabled = true;
          this.treeView1.ResumeLayout();
          this.UpdateStatusCount();
        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            this.treeView1.ResetText();
            if (this.lastSelectedNode != null)
            {
                this.lastSelectedNode.ForeColor = Color.Black;
                this.lastSelectedNode.NodeFont = normalFont;
                if (this.lastSelectedNode.Parent != null)
                {
                    this.lastSelectedNode.Parent.ForeColor = Color.Black;
                    this.lastSelectedNode.Parent.NodeFont = normalFont;

                }
            }
            this.lastSelectedNode = e.Node;

            if (e.Node.Tag is TestCase)
            {
                sprocTestConfigCtrl1.Enabled = true;
                this.lastSelectedNode.ForeColor = Color.Blue;
                this.lastSelectedNode.NodeFont = highlightFont;
                TestCase tCase = (TestCase)treeView1.SelectedNode.Tag;

                if (e.Node.Parent.Tag is StoredProcedure)
                {
                    e.Node.Parent.ForeColor = Color.Blue;
                    StoredProcedure sp = (StoredProcedure)e.Node.Parent.Tag;
                    if (sp.DerivedParameters == null)
                    {
                        sp.DerivedParameters = DbInformation.InfoHelper.GetStoredProcParameters(sp.Name, this.connData);
                        if (sp.DerivedParameters == null)
                            MessageBox.Show("Caution: Unable to derive a parameter list for the stored procedure.\r\nYou will not be able to add test cases.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    }

                    sprocTestConfigCtrl1.SetTestCaseData(tCase, sp.DerivedParameters, sp.Name, this.testConfig.Name);
                    sprocTestConfigCtrl1.Enabled = true;
                }

            }
            else
            {
                e.Node.ForeColor = Color.Blue;
                e.Node.NodeFont = highlightFont;
                this.lastSelectedNode = e.Node;
                sprocTestConfigCtrl1.Enabled = false;
            }
        }
        

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Tag is StoredProcedure)
            {
                if (e.Node.Checked)
                {
                    for (int i = 0; i < e.Node.Nodes.Count; i++)
                        e.Node.Nodes[i].Checked = true;
                }
                else
                {
                    for (int i = 0; i < e.Node.Nodes.Count; i++)
                        e.Node.Nodes[i].Checked = false;

                }
            }else if(e.Node.Tag is TestCase)
            {
                if (e.Node.Checked)
                    ((TestCase)e.Node.Tag).SelectedForRun = true;
                else
                    ((TestCase)e.Node.Tag).SelectedForRun = false;
            }
        }

        private void sprocTestConfigCtrl1_TestCaseChanged(object sender, TestConfigChangedEventArgs e)
        {
            TreeNode node = null;
            if (treeView1.SelectedNode == null && this.lastSelectedNode != null)
                node = this.lastSelectedNode;
            else
                node = this.treeView1.SelectedNode;

            if (node == null) return;

            if (e.IsNew)
            {
                StoredProcedure sp = null;
                if (node.Tag is StoredProcedure)
                    sp = (StoredProcedure)node.Tag;
                else if (node.Parent.Tag is StoredProcedure)
                    sp = (StoredProcedure)node.Parent.Tag;

                if (sp != null)
                {
                    if (!this.testConfig.AddNewTestCase(sp.Name, e.TestCaseConfig))
                    {
                        MessageBox.Show("Failed to add the new test case.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    else
                    {
                        TreeNode newNode = new TreeNode();
                        newNode.Tag = e.TestCaseConfig;
                        newNode.Text = e.TestCaseConfig.Name;
                        newNode.ForeColor = Color.Blue;
                        newNode.NodeFont = highlightFont;
                        
                        if (node.Tag is StoredProcedure)
                        {
                            node.Nodes.Add(newNode);
                            node.Expand();
                        }
                        else
                        {
                            node.Parent.Nodes.Add(newNode);
                            node.Expand();
                        }

                        totalTestCases++;
                        statTestCaseCount.Text = string.Format(strTestCases, totalTestCases.ToString());
                        newNode.EnsureVisible();
                        this.lastSelectedNode = newNode;
                    }
                }
            }
            else if (node != null && node.Tag is TestCase)
            {
                node.Tag = e.TestCaseConfig;
                node.ForeColor = Color.Blue;
                node.NodeFont = highlightFont;
                if (node.Parent.Tag is StoredProcedure)
                {
                    node.Parent.ForeColor = Color.Blue;
                    node.Parent.NodeFont = highlightFont;
                    StoredProcedure sp = (StoredProcedure)node.Parent.Tag;
                    this.testConfig.ModifyExistingTestCase(sp.Name, e.TestCaseConfig);
                }
                node.Text = e.TestCaseConfig.Name;
                node.EnsureVisible();

            }

            this.testConfig.SaveConfiguration(this.configFileName);


        }

        private void bgSprocList_DoWork(object sender, DoWorkEventArgs e)
        {
            if (sender is BackgroundWorker)
                ((BackgroundWorker)sender).ReportProgress(-1);
            this.connData.DatabaseName = this.testConfig.Name;

            List<DbInformation.ObjectData> fullSpList = DbInformation.InfoHelper.GetStoredProcedureList(this.connData);
            List<DbInformation.ObjectData> unusedSprocs = new List<DbInformation.ObjectData>();
            bool found;
            if (testConfig.StoredProcedure != null)
                for (int i = 0; i < fullSpList.Count; i++)
                {
                    found = false;
                    for (int j = 0; j < testConfig.StoredProcedure.Length; j++)
                    {
                        if (testConfig.StoredProcedure[j].Name.ToLower() == fullSpList[i].ObjectName.ToLower())
                        {
                            found = true;
                            break;
                        }
                    }

                    if (!found)
                        unusedSprocs.Add(fullSpList[i]);
                }
            else
            {
                unusedSprocs = fullSpList;
            }

            e.Result = unusedSprocs;

        }

        private void bgSprocList_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Result != null)
                this.unusedStoredProcList = (List<DbInformation.ObjectData>)e.Result;

            this.statGeneral.Text = "Ready.";
            this.Cursor = Cursors.Default;
            pgBar.Style = ProgressBarStyle.Blocks;
        }

        private void bgSprocList_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {
            this.Cursor = Cursors.AppStarting;
            this.statGeneral.Text = "Loading list of Stored Procedures...";
        }


        private void mnuAddNewStoredProcedure_Click(object sender, EventArgs e)
        {
            SqlBuild.Objects.AddObjectForm frmAdd = new SqlSync.SqlBuild.Objects.AddObjectForm(this.unusedStoredProcList, SqlSync.Constants.DbScriptDescription.StoredProcedure, SqlSync.Constants.DbObjectType.StoredProcedure, this.connData);
            if (DialogResult.OK == frmAdd.ShowDialog())
            {
                if (frmAdd.SelectedObjects.Count> 0)
                {
                    string[] spNames = new string[frmAdd.SelectedObjects.Count];
                    for (int i = 0; i < frmAdd.SelectedObjects.Count; i++)
                        spNames[i] = frmAdd.SelectedObjects[i].SchemaOwner+"."+frmAdd.SelectedObjects[i].ObjectName;

                    AddNewStoredProcedures(spNames);
                }

                frmAdd.Dispose();

            }

        }
        private void AddNewStoredProcedures(string[] storedProcNames)
        {
            if (!this.testConfig.AddNewStoredProcedures(storedProcNames))
            {
                MessageBox.Show("Error adding new stored procedures", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            if (!this.testConfig.SaveConfiguration(this.configFileName))
            {
                MessageBox.Show("Error saving the configuration file", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            BindTreeView(storedProcNames[0]);
            bgSprocList.RunWorkerAsync();

        }


        private void ctxTestCase_Opening(object sender, CancelEventArgs e)
        {
            if (treeView1.SelectedNode != null)
            {
                mnuAddNewTestCase.Enabled = true;
                deleteTestCaseToolStripMenuItem.Enabled = true;
                deleteStoredProcedureToolStripMenuItem.Enabled = true;
                viewStoredProcedureScriptToolStripMenuItem.Enabled = true;
                addNewFromExecutionScriptToolStripMenuItem.Enabled = true;


                StoredProcedure sp = null;
                TestCase tc = null;
                if (treeView1.SelectedNode.Tag is StoredProcedure)
                    sp = (StoredProcedure)treeView1.SelectedNode.Tag;
                else if (treeView1.SelectedNode.Parent.Tag is StoredProcedure)
                    sp = (StoredProcedure)treeView1.SelectedNode.Parent.Tag;

                if (treeView1.SelectedNode.Tag is TestCase)
                    tc = (TestCase)treeView1.SelectedNode.Tag;

                if (sp != null)
                {
                    mnuAddNewTestCase.Text = "Add New Test Case to \"" + sp.Name + "\""; ;
                    viewStoredProcedureScriptToolStripMenuItem.Text = "View Script for \"" + sp.Name + "\"";
                    deleteStoredProcedureToolStripMenuItem.Text = "Delete Stored Procedure \"" + sp.Name + "\" from configuration";
                }

                if (tc != null)
                {
                    deleteTestCaseToolStripMenuItem.Enabled = true;
                    deleteTestCaseToolStripMenuItem.Text = "Delete Test Case \"" + ((TestCase)treeView1.SelectedNode.Tag).Name + "\"";
                }
                else
                {
                    deleteTestCaseToolStripMenuItem.Enabled = false;
                    deleteTestCaseToolStripMenuItem.Text = "Delete Test Case ";

                }
            }
            else
            {
                mnuAddNewTestCase.Enabled = false;
                deleteTestCaseToolStripMenuItem.Enabled = false;
                deleteStoredProcedureToolStripMenuItem.Enabled = false;
                viewStoredProcedureScriptToolStripMenuItem.Enabled = false;
                addNewFromExecutionScriptToolStripMenuItem.Enabled = false;
            }
        }

        private void lnkCheck_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (lnkCheck.Text == "Check All")
            {
                foreach (TreeNode node in treeView1.Nodes)
                    node.Checked = true;
                lnkCheck.Text = "Uncheck All";
            }
            else
            {
                foreach (TreeNode node in treeView1.Nodes)
                    node.Checked = false;
                lnkCheck.Text = "Check All";
            }
        }

        private void btnRunTests_Click(object sender, EventArgs e)
        {
            SprocTestForm frmTest = new SprocTestForm(this.testConfig, this.connData, this.configFileName);
            frmTest.TestCaseSelected += new TestCaseSelectedEventHandler(frmTest_TestCaseSelected);
            frmTest.Show();
        }

        void frmTest_TestCaseSelected(object sender, TestCaseSelectedArgs args)
        {
            TreeNode node = FindTreeNode(args.StoredProcedureName, args.TestCase);
            if (node != null)
            {
                node.EnsureVisible();
                treeView1.SelectedNode = node;
            }
        }

        private void viewStoredProcedureScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string script = "";
            string desc = "";
            string message;
            if (this.treeView1.SelectedNode == null)
                return;

            StoredProcedure sp = null;
            if(this.treeView1.SelectedNode.Tag is StoredProcedure)
                sp = (StoredProcedure)this.treeView1.SelectedNode.Tag;
            else if(this.treeView1.SelectedNode.Parent.Tag is StoredProcedure)
                sp = (StoredProcedure)this.treeView1.SelectedNode.Parent.Tag;

            if (sp == null)
                return;

            SqlSync.ObjectScript.ObjectScriptHelper helper = new SqlSync.ObjectScript.ObjectScriptHelper(this.connData);
            string name = sp.Name;
            string schemaOwner;
            InfoHelper.ExtractNameAndSchema(name, out name, out schemaOwner);

            helper.ScriptDatabaseObject(SqlSync.Constants.DbObjectType.StoredProcedure, name, schemaOwner, ref script, ref desc, out message);

            ScriptDisplayForm frmDisplay;
            if (script.Length > 0)
                frmDisplay = new ScriptDisplayForm(script, this.connData.SQLServerName, sp.Name);
            else
                frmDisplay = new ScriptDisplayForm(message, this.connData.SQLServerName, sp.Name);

            frmDisplay.Show();
        }

        private void deleteTestCaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
                return;

            if (treeView1.SelectedNode.Tag is TestCase)
            {
                TestCase tC = (TestCase)treeView1.SelectedNode.Tag;
                TreeNode node = treeView1.SelectedNode;
                if (treeView1.SelectedNode.Parent.Tag is StoredProcedure)
                {
                    StoredProcedure sp = (StoredProcedure)treeView1.SelectedNode.Parent.Tag;
                    if (DialogResult.Yes == MessageBox.Show("Are you sure you want to delete the test case \"" + tC.Name + "\" from the Stored Procedure \"" + sp.Name + "\"?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                    {
                        this.testConfig.RemovedTestCase(sp.Name, tC);
                        this.sprocTestConfigCtrl1.Enabled = false;
                        this.testConfig.SaveConfiguration(this.configFileName);
                        this.treeView1.Nodes.Remove(node);
                        totalTestCases--;
                        this.statTestCaseCount.Text = String.Format(strTestCases, totalTestCases.ToString());
                        //BindTreeView();
                    }
                }
            }

        }
        private void deleteStoredProcedureToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (treeView1.SelectedNode == null)
                return;

            StoredProcedure sp = null;
            TreeNode node = null;
            if (treeView1.SelectedNode.Tag is StoredProcedure)
            {
                sp = (StoredProcedure)treeView1.SelectedNode.Tag;
                node = treeView1.SelectedNode;
            }
            else if (treeView1.SelectedNode.Parent.Tag is StoredProcedure)
            {
                sp = (StoredProcedure)treeView1.SelectedNode.Parent.Tag;
                node = treeView1.SelectedNode.Parent;
            }


            if (sp != null)
                if (DialogResult.Yes == MessageBox.Show("Are you sure you want to delete the stored procedure \"" + sp.Name + "\"\r\n and all of its test cases?", "Confirm Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                {
                    this.testConfig.RemoveStoredProcedure(sp.Name,sp.ID);
                    this.testConfig.SaveConfiguration(this.configFileName);
                    if (node.Nodes.Count > 0)
                        totalTestCases = totalTestCases - node.Nodes.Count;
                    else
                        totalSprocNoTest--;

                    totalSprocs--;
                    this.treeView1.Nodes.Remove(node);
                    statTestCaseCount.Text = string.Format(strTestCases, totalTestCases.ToString());
                    statSPCount.Text = string.Format(strSprocs, totalSprocs.ToString());
                    statSPWithoutTests.Text = string.Format(strSprocsNoTest,totalSprocNoTest.ToString());
                    //this.BindTreeView();

                }
        }


        private bool AddNewTestCaseFromScript(string script)
        {
            try
            {
                Dictionary<string, string> parameterList;
                string spName;
                TestManager.GetConfigurationFromScript(script, this.connData, out spName, out parameterList);
                this.testConfig.AddNewTestCase(spName, parameterList);
                return true;
            }
            catch
            {
                return false;
            }
        }
        private TreeNode FindTreeNode(string storedProcName, TestCase testCase)
        {


            foreach (TreeNode node in treeView1.Nodes)
            {
                if (node.Tag is StoredProcedure && ((StoredProcedure)node.Tag).Name == storedProcName)
                {
                    if (testCase == null)
                        return node;

                    foreach (TreeNode child in node.Nodes)
                    {
                        if (child.Tag is TestCase && ((TestCase)child.Tag).TestCaseId == testCase.TestCaseId)
                            return child;
                    }
                }
            }
            return null;
        }

        private void settingsControl1_ServerChanged(object sender, string serverName, string username, string password, AuthenticationType authType)
        {
            this.connData.SQLServerName = serverName;
            if (!string.IsNullOrWhiteSpace(username) && (!string.IsNullOrWhiteSpace(password)))
            {
                this.connData.UserId = username;
                this.connData.Password = password;
            }
            this.connData.AuthenticationType = authType;

            if(!bgSprocList.IsBusy)
                bgSprocList.RunWorkerAsync();
        }

        private void setTargetDatabaseToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.testConfig == null)
            {
                MessageBox.Show("Please open or create a new configuration first", "Configuration Needed", MessageBoxButtons.OK, MessageBoxIcon.Hand);
                return;
            }
            else
            {
                SetDatabaseForm frmDb = new SetDatabaseForm(DbInformation.InfoHelper.GetDatabaseList(this.connData), this.testConfig.Name);
                frmDb.ShowDialog();
                this.testConfig.Name = frmDb.CurrentDatabase;
                this.testConfig.SaveConfiguration(this.configFileName);
                bgSprocList.RunWorkerAsync();
            }

        }

        private void importTestCasesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (DialogResult.OK != openFileDialog1.ShowDialog())
                return;

            string toImport = openFileDialog1.FileName;
            openFileDialog1.Dispose();
            Database importCfg = null;
            try
            {
                importCfg = TestManager.ReadConfiguration(toImport);
            }
            catch (Exception exe)
            {
                MessageBox.Show(exe.Message, "Unable to load", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            if (importCfg == null)
                return;

            if (this.testConfig.Import(importCfg))
            {
               this.testConfig.SaveConfiguration(this.configFileName);
               BindTreeView();
            }


        }

        private void menuStrip1_MenuActivate(object sender, EventArgs e)
        {
            if (this.testConfig == null)
            {
                importTestCasesToolStripMenuItem.Enabled = false;
                setTargetDatabaseToolStripMenuItem.Enabled = false;
            }
            else
            {
                importTestCasesToolStripMenuItem.Enabled = true;
                setTargetDatabaseToolStripMenuItem.Enabled = true;
            }
        }
        private void addNewFromExecutionScriptToolStripMenuItem_Click(object sender, EventArgs e)
        {
            PasteScriptForm frmScr = new PasteScriptForm();
            if (DialogResult.OK == frmScr.ShowDialog())
            {
                string script = frmScr.ScriptText;
                Dictionary<string, string> parameterList;
                string spName;
                TestManager.GetConfigurationFromScript(script, this.connData, out spName, out parameterList);
                TestCase tc = this.testConfig.AddNewTestCase(spName, parameterList);
                this.testConfig.SaveConfiguration(this.configFileName);
                if (tc != null)
                {
                    this.testConfig.SaveConfiguration(this.configFileName);
                    this.BindTreeView();


                    TreeNode newNode = FindTreeNode(spName, tc);
                    if (newNode != null)
                    {
                        newNode.EnsureVisible();
                        treeView1.SelectedNode = newNode;
                    }
                }
            }

        }
        private void bulkAddFromExecutionScriptsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> errors = new List<string>();
            PasteScriptForm frmP = new PasteScriptForm(true);
            if (DialogResult.OK == frmP.ShowDialog())
            {
                string[] scripts = frmP.BulkScripts;
                frmP.Dispose();

                for (int i = 0; i < scripts.Length; i++)
                {
                    if(!AddNewTestCaseFromScript(scripts[i]))
                    {
                        string err = (scripts[i].Length < 100) ? scripts[1] : scripts[i].Substring(0, 100) + "...";
                        errors.Add(err);
                    }
                }
                this.testConfig.SaveConfiguration(this.configFileName);
                this.BindTreeView();
                if (errors.Count > 0)
                {
                    StringBuilder sb = new StringBuilder("Unable to generate test cases for:\r\n");
                    for (int i = 0; i < errors.Count; i++)
                        sb.Append(errors[i] + "\r\n");

                    MessageBox.Show(sb.ToString(), "Unable to process some items...", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
        }

        public void OpenMRUFile(string fileName)
        {
            this.Cursor = Cursors.WaitCursor;
            if (File.Exists(fileName))
            {
                try
                {
                    testConfig = SqlSync.SprocTest.TestManager.ReadConfiguration(fileName);
                    this.connData.DatabaseName = testConfig.Name;
                }
                catch (Exception exe)
                {
                    MessageBox.Show(exe.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
            }
            else
            {
                testConfig = new Database();
                SetDatabaseForm frmDb = new SetDatabaseForm(DbInformation.InfoHelper.GetDatabaseList(this.connData), "");
                frmDb.ShowDialog();
                this.testConfig.Name = frmDb.CurrentDatabase;
                this.testConfig.SaveConfiguration(this.configFileName);
            }
            this.mruManager.Add(fileName);

            this.settingsControl1.Project = this.configFileName;
            BindTreeView();

            while (bgSprocList.IsBusy)
                System.Threading.Thread.Sleep(100);

            this.Cursor = Cursors.Default;
            pgBar.Style = ProgressBarStyle.Marquee;
            bgSprocList.RunWorkerAsync();
        }
        private void UpdateStatusCount()
        {
            totalSprocs = treeView1.Nodes.Count;
            statSPCount.Text = String.Format(strSprocs, totalSprocs.ToString());
            
            totalTestCases = 0;
            totalSprocNoTest = 0;

            foreach(TreeNode node in treeView1.Nodes)
            {
                if(node.Nodes.Count == 0)
                    totalSprocNoTest++;
                else
                    totalTestCases = totalTestCases+node.Nodes.Count;
            }

            statSPWithoutTests.Text = String.Format(strSprocsNoTest, totalSprocNoTest.ToString());
            statTestCaseCount.Text = String.Format(strTestCases, totalTestCases.ToString());

        }
        #region .: New Test Cases :.
        private void mnuAddNewTestCase_Click(object sender, EventArgs e)
        {
            TreeNode node = null;
            if (treeView1.SelectedNode == null && this.lastSelectedNode != null)
                node = this.lastSelectedNode;
            else
                node = this.treeView1.SelectedNode;

            if (node == null) return;

            StoredProcedure sp = null;
            if (node.Tag is StoredProcedure)
                sp = (StoredProcedure)node.Tag;
            else if (node.Tag is TestCase)
                if (node.Parent.Tag is StoredProcedure)
                    sp = (StoredProcedure)node.Parent.Tag;

            if (sp != null)
            {
                if (sp.DerivedParameters == null)
                {
                    this.Cursor = Cursors.AppStarting;
                    this.statGeneral.Text = "Accessing " + sp.Name + " and deriving parameters.";
                    pgBar.Style = ProgressBarStyle.Marquee;
                    bgNewTestCase.RunWorkerAsync(sp);
                }
                else
                {
                    bgNewTestCase_RunWorkerCompleted(null, new RunWorkerCompletedEventArgs(sp, null, false));
                }
            }
        }


        private void bgNewTestCase_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bg = (BackgroundWorker)sender;
            StoredProcedure sp = (StoredProcedure)e.Argument;
            sp.DerivedParameters = DbInformation.InfoHelper.GetStoredProcParameters(sp.Name, this.connData);
            e.Result = sp;

        }

        private void bgNewTestCase_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            this.Cursor = Cursors.Default;
            this.statGeneral.Text = "Ready.";

            StoredProcedure sp = (StoredProcedure)e.Result;

            if (sp == null || sp.DerivedParameters == null)
            {
                MessageBox.Show("Caution: Unable to derive a parameter list for the stored procedure.\r\nYou will not be able to add test cases.", "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }


            TestCase t = new TestCase();
            t.TestCaseId = Guid.NewGuid().ToString();
            sprocTestConfigCtrl1.SetTestCaseData(t, sp.DerivedParameters, sp.Name, this.testConfig.Name, true);
            sprocTestConfigCtrl1.Enabled = true;
            pgBar.Style = ProgressBarStyle.Blocks;

        }

        #endregion

        private void helpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SqlSync.SqlBuild.UtilityHelper.OpenManual("StoredProcedureTesting");
        }






    }

    // Create a node sorter that implements the IComparer interface.
    public class NodeSorter : System.Collections.IComparer
    {
        // Compare the length of the strings, or the strings
        // themselves, if they are the same length.
        public int Compare(object x, object y)
        {

            TreeNode tx = x as TreeNode;
            TreeNode ty = y as TreeNode;
            return string.Compare(tx.Text, ty.Text);
           
        }
    }
}