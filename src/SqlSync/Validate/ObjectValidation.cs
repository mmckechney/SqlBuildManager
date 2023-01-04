using SqlSync.Connection;
using SqlSync.DbInformation;
using SqlSync.ObjectScript;
using System;
using System.ComponentModel;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace SqlSync.Validate
{
    /// <summary>
    /// Summary description for AnalysisForm.
    /// </summary>
    public class ObjectValidation : System.Windows.Forms.Form
    {
        private SqlSync.ColumnSorter resultSorter = new ColumnSorter();
        bool hasValidationErrors = false;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox ddDatabaseList;
        private SqlSync.SettingsControl settingsControl1;
        private Connection.ConnectionData connData = null;
        private System.Windows.Forms.StatusStrip statusBar1;
        private System.Windows.Forms.ToolStripStatusLabel statStatus;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        private System.Windows.Forms.Button btnValidate;
        private System.Windows.Forms.ListView lstResults;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Panel pnlWarning;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ContextMenuStrip contextMenu1;
        private System.Windows.Forms.ToolStripMenuItem mnuCopy;
        private System.Windows.Forms.ToolStripMenuItem mnuCopyInvalid;
        private System.Windows.Forms.ToolStripMenuItem menuItem2;
        private System.Windows.Forms.ToolStripMenuItem mnuViewObjectScript;
        private System.Windows.Forms.ColumnHeader columnHeader4;
        private BackgroundWorker bgworker;
        private Button btnCancel;
        private MenuStrip menuStrip1;
        private ToolStripMenuItem actionToolStripMenuItem;
        private ToolStripMenuItem changeSqlServerConnectionToolStripMenuItem;
        private ToolStripMenuItem toolStripMenuItem1;
        private IContainer components = null;

        public ObjectValidation(Connection.ConnectionData connData)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            this.connData = connData;
        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (components != null)
                {
                    components.Dispose();
                }
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ObjectValidation));
            label2 = new System.Windows.Forms.Label();
            ddDatabaseList = new System.Windows.Forms.ComboBox();
            statusBar1 = new System.Windows.Forms.StatusStrip();
            statStatus = new System.Windows.Forms.ToolStripStatusLabel();
            btnValidate = new System.Windows.Forms.Button();
            lstResults = new System.Windows.Forms.ListView();
            columnHeader1 = new System.Windows.Forms.ColumnHeader();
            columnHeader2 = new System.Windows.Forms.ColumnHeader();
            columnHeader3 = new System.Windows.Forms.ColumnHeader();
            columnHeader4 = new System.Windows.Forms.ColumnHeader();
            contextMenu1 = new System.Windows.Forms.ContextMenuStrip();
            mnuCopy = new System.Windows.Forms.ToolStripMenuItem();
            mnuCopyInvalid = new System.Windows.Forms.ToolStripMenuItem();
            menuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            mnuViewObjectScript = new System.Windows.Forms.ToolStripMenuItem();
            pnlWarning = new System.Windows.Forms.Panel();
            label1 = new System.Windows.Forms.Label();
            pictureBox1 = new System.Windows.Forms.PictureBox();
            bgworker = new System.ComponentModel.BackgroundWorker();
            btnCancel = new System.Windows.Forms.Button();
            menuStrip1 = new System.Windows.Forms.MenuStrip();
            actionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            changeSqlServerConnectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            settingsControl1 = new SqlSync.SettingsControl();
            toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            pnlWarning.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(pictureBox1)).BeginInit();
            menuStrip1.SuspendLayout();
            SuspendLayout();
            // 
            // label2
            // 
            label2.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            label2.Location = new System.Drawing.Point(12, 93);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(112, 16);
            label2.TabIndex = 19;
            label2.Text = "Select Database:";
            // 
            // ddDatabaseList
            // 
            ddDatabaseList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            ddDatabaseList.Location = new System.Drawing.Point(13, 112);
            ddDatabaseList.Name = "ddDatabaseList";
            ddDatabaseList.Size = new System.Drawing.Size(176, 21);
            ddDatabaseList.TabIndex = 18;
            // 
            // statusBar1
            // 
            statusBar1.Location = new System.Drawing.Point(0, 427);
            statusBar1.Name = "statusBar1";
            statusBar1.Items.AddRange(new System.Windows.Forms.ToolStripStatusLabel[] {
            statStatus});
            //this.statusBar1.ShowPanels = true;
            statusBar1.Size = new System.Drawing.Size(910, 22);
            statusBar1.TabIndex = 21;
            statusBar1.Text = "statusBar1";
            // 
            // statStatus
            // 
            statStatus.AutoSize = true;
            statStatus.Spring = true;
            statStatus.Name = "statStatus";
            statStatus.Text = "Ready";
            statStatus.Width = 893;
            // 
            // btnValidate
            // 
            btnValidate.FlatStyle = System.Windows.Forms.FlatStyle.System;
            btnValidate.Location = new System.Drawing.Point(201, 111);
            btnValidate.Name = "btnValidate";
            btnValidate.Size = new System.Drawing.Size(112, 23);
            btnValidate.TabIndex = 23;
            btnValidate.Text = "Validate Objects";
            btnValidate.Click += new System.EventHandler(btnValidate_Click);
            // 
            // lstResults
            // 
            lstResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            lstResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader1,
            columnHeader2,
            columnHeader3,
            columnHeader4});
            lstResults.ContextMenuStrip = contextMenu1;
            lstResults.FullRowSelect = true;
            lstResults.GridLines = true;
            lstResults.Location = new System.Drawing.Point(8, 141);
            lstResults.Name = "lstResults";
            lstResults.Size = new System.Drawing.Size(886, 280);
            lstResults.TabIndex = 24;
            lstResults.UseCompatibleStateImageBehavior = false;
            lstResults.View = System.Windows.Forms.View.Details;
            lstResults.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(lstResults_ColumnClick);
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Object Name";
            columnHeader1.Width = 303;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Type";
            columnHeader2.Width = 43;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "Results";
            columnHeader3.Width = 407;
            // 
            // columnHeader4
            // 
            columnHeader4.Text = "Status Type";
            columnHeader4.Width = 106;
            // 
            // contextMenu1
            // 
            contextMenu1.Items.AddRange(new System.Windows.Forms.ToolStripMenuItem[] {
            mnuCopy,
            mnuCopyInvalid,
            menuItem2,
            mnuViewObjectScript});
            // 
            // mnuCopy
            // 
            // this.mnuCopy.Index = 0;
            mnuCopy.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C;
            mnuCopy.Text = "Copy";
            mnuCopy.Click += new System.EventHandler(mnuCopy_Click);
            // 
            // mnuCopyInvalid
            // 
            // this.mnuCopyInvalid.Index = 1;
            mnuCopyInvalid.Text = "Copy Invalid Objects";
            mnuCopyInvalid.Click += new System.EventHandler(mnuCopyInvalid_Click);
            // 
            // menuItem2
            // 
            //this.menuItem2.Index = 2;
            menuItem2.Text = "-";
            // 
            // mnuViewObjectScript
            // 
            //this.mnuViewObjectScript.Index = 3;
            mnuViewObjectScript.Text = "View Object Script";
            mnuViewObjectScript.Click += new System.EventHandler(mnuViewObjectScript_Click);
            // 
            // pnlWarning
            // 
            pnlWarning.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            pnlWarning.BackColor = System.Drawing.Color.White;
            pnlWarning.Controls.Add(label1);
            pnlWarning.Controls.Add(pictureBox1);
            pnlWarning.Location = new System.Drawing.Point(421, 89);
            pnlWarning.Name = "pnlWarning";
            pnlWarning.Size = new System.Drawing.Size(324, 46);
            pnlWarning.TabIndex = 26;
            pnlWarning.Visible = false;
            // 
            // label1
            // 
            label1.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            label1.Location = new System.Drawing.Point(52, 12);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(270, 23);
            label1.TabIndex = 26;
            label1.Text = "Warning! Invalid Objects Detected.";
            // 
            // pictureBox1
            // 
            pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            pictureBox1.Location = new System.Drawing.Point(4, 2);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(44, 40);
            pictureBox1.TabIndex = 25;
            pictureBox1.TabStop = false;
            // 
            // bgworker
            // 
            bgworker.WorkerReportsProgress = true;
            bgworker.WorkerSupportsCancellation = true;
            bgworker.DoWork += new System.ComponentModel.DoWorkEventHandler(bgworker_DoWork);
            bgworker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(bgworker_RunWorkerCompleted);
            bgworker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(bgworker_ProgressChanged);
            // 
            // btnCancel
            // 
            btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            btnCancel.Location = new System.Drawing.Point(319, 110);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(70, 23);
            btnCancel.TabIndex = 27;
            btnCancel.Text = "Cancel";
            btnCancel.Click += new System.EventHandler(btnCancel_Click);
            // 
            // menuStrip1
            // 
            menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            actionToolStripMenuItem,
            toolStripMenuItem1});
            menuStrip1.Location = new System.Drawing.Point(0, 0);
            menuStrip1.Name = "menuStrip1";
            menuStrip1.Size = new System.Drawing.Size(910, 24);
            menuStrip1.TabIndex = 28;
            menuStrip1.Text = "menuStrip1";
            // 
            // actionToolStripMenuItem
            // 
            actionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            changeSqlServerConnectionToolStripMenuItem});
            actionToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Execute;
            actionToolStripMenuItem.Name = "actionToolStripMenuItem";
            actionToolStripMenuItem.Size = new System.Drawing.Size(65, 20);
            actionToolStripMenuItem.Text = "Action";
            // 
            // changeSqlServerConnectionToolStripMenuItem
            // 
            changeSqlServerConnectionToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Server1;
            changeSqlServerConnectionToolStripMenuItem.Name = "changeSqlServerConnectionToolStripMenuItem";
            changeSqlServerConnectionToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
            changeSqlServerConnectionToolStripMenuItem.Text = "Change Sql Server Connection";
            changeSqlServerConnectionToolStripMenuItem.Click += new System.EventHandler(mnuChangeSqlServer_Click);
            // 
            // settingsControl1
            // 
            settingsControl1.BackColor = System.Drawing.Color.White;
            settingsControl1.Dock = System.Windows.Forms.DockStyle.Top;
            settingsControl1.Location = new System.Drawing.Point(0, 24);
            settingsControl1.Name = "settingsControl1";
            settingsControl1.Project = "";
            settingsControl1.ProjectLabelText = "";
            settingsControl1.Server = "";
            settingsControl1.Size = new System.Drawing.Size(910, 54);
            settingsControl1.TabIndex = 20;
            settingsControl1.ServerChanged += new SqlSync.ServerChangedEventHandler(settingsControl1_ServerChanged);
            // 
            // toolStripMenuItem1
            // 
            toolStripMenuItem1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            toolStripMenuItem1.Image = global::SqlSync.Properties.Resources.Help_2;
            toolStripMenuItem1.Name = "toolStripMenuItem1";
            toolStripMenuItem1.Size = new System.Drawing.Size(28, 20);
            toolStripMenuItem1.Click += new System.EventHandler(toolStripMenuItem1_Click);
            // 
            // ObjectValidation
            // 
            AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            ClientSize = new System.Drawing.Size(910, 449);
            Controls.Add(btnCancel);
            Controls.Add(pnlWarning);
            Controls.Add(lstResults);
            Controls.Add(btnValidate);
            Controls.Add(statusBar1);
            Controls.Add(label2);
            Controls.Add(ddDatabaseList);
            Controls.Add(settingsControl1);
            Controls.Add(menuStrip1);
            Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            KeyPreview = true;
            MainMenuStrip = menuStrip1;
            Name = "ObjectValidation";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Sql Build Manager :: Database Object Validation";
            Load += new System.EventHandler(ObjectValidation_Load);
            KeyDown += new System.Windows.Forms.KeyEventHandler(ObjectValidation_KeyDown);
            pnlWarning.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(pictureBox1)).EndInit();
            menuStrip1.ResumeLayout(false);
            menuStrip1.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }
        #endregion
        private void mnuChangeSqlServer_Click(object sender, System.EventArgs e)
        {
            ConnectionForm frmConnect = new ConnectionForm("Sql Object Validator");
            DialogResult result = frmConnect.ShowDialog();
            if (result == DialogResult.OK)
            {
                connData = frmConnect.SqlConnection;
                ddDatabaseList.Items.Clear();
                for (int i = 0; i < frmConnect.DatabaseList.Count; i++)
                    ddDatabaseList.Items.Add(frmConnect.DatabaseList[i].DatabaseName);
                settingsControl1.Server = connData.SQLServerName;
            }
        }

        private void ObjectValidation_Load(object sender, System.EventArgs e)
        {
            DatabaseList dbs = SqlSync.DbInformation.InfoHelper.GetDatabaseList(connData);
            for (int i = 0; i < dbs.Count; i++)
                ddDatabaseList.Items.Add(dbs[i].DatabaseName);
            settingsControl1.Server = connData.SQLServerName;
        }

        private void btnValidate_Click(object sender, System.EventArgs e)
        {
            lstResults.Items.Clear();
            pnlWarning.Visible = false;
            hasValidationErrors = false;
            statStatus.Text = "Validating Objects Data";
            btnValidate.Enabled = false;
            Cursor = Cursors.AppStarting;
            try
            {
                connData.DatabaseName = ddDatabaseList.SelectedItem.ToString();
                bgworker.RunWorkerAsync(connData);
            }
            catch (Exception ex)
            {
                string m = ex.ToString();
                statStatus.Text = "ERROR: Unable to Retrieve Data";
                Cursor = Cursors.Default;
                btnValidate.Enabled = true;
            }
        }




        private void ObjectValidation_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyCode == Keys.C && e.Modifiers == Keys.Control)
            {
                CopySelectedItems();
            }
        }
        private void CopySelectedItems()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < lstResults.SelectedItems.Count; i++)
            {
                ListViewItem item = lstResults.SelectedItems[i];
                sb.Append("\"" + item.SubItems[0].Text + "\",\"" + item.SubItems[1].Text + "\",\"" + item.SubItems[2].Text + "\",\"" + item.SubItems[3].Text + "\"\r\n");
            }

            Clipboard.SetDataObject(sb.ToString().Trim(), true);
        }
        private void CopyInvalidObjects()
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < lstResults.Items.Count; i++)
            {
                ListViewItem item = lstResults.Items[i];
                if (item.BackColor != Color.White)
                {
                    sb.Append("\"" + item.SubItems[0].Text + "\",\"" + item.SubItems[1].Text + "\",\"" + item.SubItems[2].Text + "\",\"" + item.SubItems[3].Text + "\"\r\n");
                }
            }

            Clipboard.SetDataObject(sb.ToString().Trim(), true);
        }

        private void mnuCopy_Click(object sender, System.EventArgs e)
        {
            CopySelectedItems();
        }

        private void mnuCopyInvalid_Click(object sender, System.EventArgs e)
        {
            CopyInvalidObjects();
        }


        private void mnuViewObjectScript_Click(object sender, System.EventArgs e)
        {
            if (lstResults.SelectedItems.Count == 0)
                return;

            ListViewItem item = lstResults.SelectedItems[0];

            SqlSync.ObjectScript.ObjectScriptHelper helper = new ObjectScriptHelper(connData);
            string script = string.Empty;
            string desc = string.Empty;
            string message;
            string fullObjName = item.SubItems[0].Text;
            string name = fullObjName;
            string schema;
            InfoHelper.ExtractNameAndSchema(name, out name, out schema);

            switch (item.SubItems[1].Text.Trim())
            {
                case "V":
                    helper.ScriptDatabaseObject(SqlSync.Constants.DbObjectType.View, name, schema, ref script, ref desc, out message);
                    break;
                case "P":
                    helper.ScriptDatabaseObject(SqlSync.Constants.DbObjectType.StoredProcedure, name, schema, ref script, ref desc, out message);
                    break;
                case "FN":
                    helper.ScriptDatabaseObject(SqlSync.Constants.DbObjectType.UserDefinedFunction, name, schema, ref script, ref desc, out message);
                    break;
                default:
                    message = string.Empty;
                    break;
            }

            if (script.Length > 0 || message.Length > 0)
            {
                ScriptDisplayForm frmDisplay;
                if (script.Length > 0)
                    frmDisplay = new ScriptDisplayForm(script, connData.SQLServerName, item.SubItems[0].Text);
                else
                    frmDisplay = new ScriptDisplayForm(message, connData.SQLServerName, item.SubItems[0].Text);

                frmDisplay.ShowDialog();
            }

        }

        private void lstResults_ColumnClick(object sender, System.Windows.Forms.ColumnClickEventArgs e)
        {
            resultSorter.CurrentColumn = e.Column;
            lstResults.ListViewItemSorter = resultSorter;
            lstResults.Sort();
        }

        private void bgworker_DoWork(object sender, DoWorkEventArgs e)
        {
            SqlSync.ObjectScript.ObjectValidator validator = new SqlSync.ObjectScript.ObjectValidator();
            validator.Validate(connData, sender as BackgroundWorker, e);

        }

        private void bgworker_ProgressChanged(object sender, ProgressChangedEventArgs args)
        {

            if (args.UserState != null && args.UserState is ValidationResultEventArgs)
            {
                ValidationResultEventArgs e = args.UserState as ValidationResultEventArgs;
                ListViewItem item = new ListViewItem(new string[] { e.Name, e.Type, e.Message, e.ResultValue.ToString() });
                switch (e.ResultValue)
                {
                    case ValidationResultValue.Invalid:
                        item.BackColor = Color.IndianRed;
                        hasValidationErrors = true;
                        break;
                    case ValidationResultValue.Caution:
                        item.BackColor = Color.Yellow;
                        hasValidationErrors = true;
                        break;
                    case ValidationResultValue.CrossDatabaseJoin:
                        item.BackColor = Color.Beige;
                        hasValidationErrors = true;
                        break;
                    case ValidationResultValue.Valid:
                        item.BackColor = Color.White;
                        break;
                }
                lstResults.Items.Insert(0, item);
            }
        }

        private void bgworker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled == false)
            {
                pnlWarning.Visible = hasValidationErrors;
                statStatus.Text = "Complete";
                Cursor = Cursors.Default;
                Invalidate();
            }
            else
            {
                pnlWarning.Visible = hasValidationErrors;
                statStatus.Text = "Cancelled";
                Cursor = Cursors.Default;
                Invalidate();
            }
            btnCancel.Enabled = true;
            btnValidate.Enabled = true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            btnCancel.Enabled = false;
            bgworker.CancelAsync();
        }

        private void settingsControl1_ServerChanged(object sender, string serverName, string username, string password, AuthenticationType authType)
        {
            Connection.ConnectionData oldConnData = new Connection.ConnectionData();
            connData.Fill(oldConnData);
            Cursor = Cursors.WaitCursor;

            connData.SQLServerName = serverName;
            if (!string.IsNullOrWhiteSpace(username) && (!string.IsNullOrWhiteSpace(password)))
            {
                connData.UserId = username;
                connData.Password = password;
            }
            connData.AuthenticationType = authType;
            connData.ScriptTimeout = 5;
            try
            {
                DatabaseList dbList = SqlSync.DbInformation.InfoHelper.GetDatabaseList(connData);
                //this.LookUpTable_Load(null, EventArgs.Empty);
            }
            catch
            {
                MessageBox.Show("Error retrieving database list. Is the server running?", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                connData = oldConnData;
                settingsControl1.Server = oldConnData.SQLServerName;
            }


            Cursor = Cursors.Default;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SqlSync.SqlBuild.UtilityHelper.OpenManual("DatabaseObjectValidation");
        }

    }
    public delegate void SetWarningVisability(bool show);
}
