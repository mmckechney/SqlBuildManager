using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using SqlSync.DbInformation;
using System.Globalization;
using System.Text;
using SqlSync.ObjectScript;
using SqlSync.Connection;

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
            this.label2 = new System.Windows.Forms.Label();
            this.ddDatabaseList = new System.Windows.Forms.ComboBox();
            this.statusBar1 = new System.Windows.Forms.StatusStrip();
            this.statStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.btnValidate = new System.Windows.Forms.Button();
            this.lstResults = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader4 = new System.Windows.Forms.ColumnHeader();
            this.contextMenu1 = new System.Windows.Forms.ContextMenuStrip();
            this.mnuCopy = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuCopyInvalid = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem2 = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuViewObjectScript = new System.Windows.Forms.ToolStripMenuItem();
            this.pnlWarning = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.bgworker = new System.ComponentModel.BackgroundWorker();
            this.btnCancel = new System.Windows.Forms.Button();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.actionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.changeSqlServerConnectionToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.settingsControl1 = new SqlSync.SettingsControl();
            this.toolStripMenuItem1 = new System.Windows.Forms.ToolStripMenuItem();
            ((System.ComponentModel.ISupportInitialize)(this.statStatus)).BeginInit();
            this.pnlWarning.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 93);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(112, 16);
            this.label2.TabIndex = 19;
            this.label2.Text = "Select Database:";
            // 
            // ddDatabaseList
            // 
            this.ddDatabaseList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddDatabaseList.Location = new System.Drawing.Point(13, 112);
            this.ddDatabaseList.Name = "ddDatabaseList";
            this.ddDatabaseList.Size = new System.Drawing.Size(176, 21);
            this.ddDatabaseList.TabIndex = 18;
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(0, 427);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Items.AddRange(new System.Windows.Forms.ToolStripStatusLabel[] {
            this.statStatus});
            //this.statusBar1.ShowPanels = true;
            this.statusBar1.Size = new System.Drawing.Size(910, 22);
            this.statusBar1.TabIndex = 21;
            this.statusBar1.Text = "statusBar1";
            // 
            // statStatus
            // 
            this.statStatus.AutoSize = true;
            this.statStatus.Spring = true;
            this.statStatus.Name = "statStatus";
            this.statStatus.Text = "Ready";
            this.statStatus.Width = 893;
            // 
            // btnValidate
            // 
            this.btnValidate.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnValidate.Location = new System.Drawing.Point(201, 111);
            this.btnValidate.Name = "btnValidate";
            this.btnValidate.Size = new System.Drawing.Size(112, 23);
            this.btnValidate.TabIndex = 23;
            this.btnValidate.Text = "Validate Objects";
            this.btnValidate.Click += new System.EventHandler(this.btnValidate_Click);
            // 
            // lstResults
            // 
            this.lstResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstResults.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2,
            this.columnHeader3,
            this.columnHeader4});
            this.lstResults.ContextMenuStrip = this.contextMenu1;
            this.lstResults.FullRowSelect = true;
            this.lstResults.GridLines = true;
            this.lstResults.Location = new System.Drawing.Point(8, 141);
            this.lstResults.Name = "lstResults";
            this.lstResults.Size = new System.Drawing.Size(886, 280);
            this.lstResults.TabIndex = 24;
            this.lstResults.UseCompatibleStateImageBehavior = false;
            this.lstResults.View = System.Windows.Forms.View.Details;
            this.lstResults.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lstResults_ColumnClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Object Name";
            this.columnHeader1.Width = 303;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Type";
            this.columnHeader2.Width = 43;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Results";
            this.columnHeader3.Width = 407;
            // 
            // columnHeader4
            // 
            this.columnHeader4.Text = "Status Type";
            this.columnHeader4.Width = 106;
            // 
            // contextMenu1
            // 
            this.contextMenu1.Items.AddRange(new System.Windows.Forms.ToolStripMenuItem[] {
            this.mnuCopy,
            this.mnuCopyInvalid,
            this.menuItem2,
            this.mnuViewObjectScript});
            // 
            // mnuCopy
            // 
           // this.mnuCopy.Index = 0;
            this.mnuCopy.ShortcutKeys = System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.C;
            this.mnuCopy.Text = "Copy";
            this.mnuCopy.Click += new System.EventHandler(this.mnuCopy_Click);
            // 
            // mnuCopyInvalid
            // 
           // this.mnuCopyInvalid.Index = 1;
            this.mnuCopyInvalid.Text = "Copy Invalid Objects";
            this.mnuCopyInvalid.Click += new System.EventHandler(this.mnuCopyInvalid_Click);
            // 
            // menuItem2
            // 
            //this.menuItem2.Index = 2;
            this.menuItem2.Text = "-";
            // 
            // mnuViewObjectScript
            // 
            //this.mnuViewObjectScript.Index = 3;
            this.mnuViewObjectScript.Text = "View Object Script";
            this.mnuViewObjectScript.Click += new System.EventHandler(this.mnuViewObjectScript_Click);
            // 
            // pnlWarning
            // 
            this.pnlWarning.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlWarning.BackColor = System.Drawing.Color.White;
            this.pnlWarning.Controls.Add(this.label1);
            this.pnlWarning.Controls.Add(this.pictureBox1);
            this.pnlWarning.Location = new System.Drawing.Point(421, 89);
            this.pnlWarning.Name = "pnlWarning";
            this.pnlWarning.Size = new System.Drawing.Size(324, 46);
            this.pnlWarning.TabIndex = 26;
            this.pnlWarning.Visible = false;
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(52, 12);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(270, 23);
            this.label1.TabIndex = 26;
            this.label1.Text = "Warning! Invalid Objects Detected.";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox1.Image")));
            this.pictureBox1.Location = new System.Drawing.Point(4, 2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(44, 40);
            this.pictureBox1.TabIndex = 25;
            this.pictureBox1.TabStop = false;
            // 
            // bgworker
            // 
            this.bgworker.WorkerReportsProgress = true;
            this.bgworker.WorkerSupportsCancellation = true;
            this.bgworker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgworker_DoWork);
            this.bgworker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgworker_RunWorkerCompleted);
            this.bgworker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgworker_ProgressChanged);
            // 
            // btnCancel
            // 
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnCancel.Location = new System.Drawing.Point(319, 110);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(70, 23);
            this.btnCancel.TabIndex = 27;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.actionToolStripMenuItem,
            this.toolStripMenuItem1});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(910, 24);
            this.menuStrip1.TabIndex = 28;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // actionToolStripMenuItem
            // 
            this.actionToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.changeSqlServerConnectionToolStripMenuItem});
            this.actionToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Execute;
            this.actionToolStripMenuItem.Name = "actionToolStripMenuItem";
            this.actionToolStripMenuItem.Size = new System.Drawing.Size(65, 20);
            this.actionToolStripMenuItem.Text = "Action";
            // 
            // changeSqlServerConnectionToolStripMenuItem
            // 
            this.changeSqlServerConnectionToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Server1;
            this.changeSqlServerConnectionToolStripMenuItem.Name = "changeSqlServerConnectionToolStripMenuItem";
            this.changeSqlServerConnectionToolStripMenuItem.Size = new System.Drawing.Size(231, 22);
            this.changeSqlServerConnectionToolStripMenuItem.Text = "Change Sql Server Connection";
            this.changeSqlServerConnectionToolStripMenuItem.Click += new System.EventHandler(this.mnuChangeSqlServer_Click);
            // 
            // settingsControl1
            // 
            this.settingsControl1.BackColor = System.Drawing.Color.White;
            this.settingsControl1.Dock = System.Windows.Forms.DockStyle.Top;
            this.settingsControl1.Location = new System.Drawing.Point(0, 24);
            this.settingsControl1.Name = "settingsControl1";
            this.settingsControl1.Project = "";
            this.settingsControl1.ProjectLabelText = "";
            this.settingsControl1.Server = "";
            this.settingsControl1.Size = new System.Drawing.Size(910, 54);
            this.settingsControl1.TabIndex = 20;
            this.settingsControl1.ServerChanged += new SqlSync.ServerChangedEventHandler(this.settingsControl1_ServerChanged);
            // 
            // toolStripMenuItem1
            // 
            this.toolStripMenuItem1.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.toolStripMenuItem1.Image = global::SqlSync.Properties.Resources.Help_2;
            this.toolStripMenuItem1.Name = "toolStripMenuItem1";
            this.toolStripMenuItem1.Size = new System.Drawing.Size(28, 20);
            this.toolStripMenuItem1.Click += new System.EventHandler(this.toolStripMenuItem1_Click);
            // 
            // ObjectValidation
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(910, 449);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.pnlWarning);
            this.Controls.Add(this.lstResults);
            this.Controls.Add(this.btnValidate);
            this.Controls.Add(this.statusBar1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ddDatabaseList);
            this.Controls.Add(this.settingsControl1);
            this.Controls.Add(this.menuStrip1);
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.MainMenuStrip = this.menuStrip1;
            this.Name = "ObjectValidation";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Sql Build Manager :: Database Object Validation";
            this.Load += new System.EventHandler(this.ObjectValidation_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.ObjectValidation_KeyDown);
            ((System.ComponentModel.ISupportInitialize)(this.statStatus)).EndInit();
            this.pnlWarning.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        #endregion
        private void mnuChangeSqlServer_Click(object sender, System.EventArgs e)
        {
            ConnectionForm frmConnect = new ConnectionForm("Sql Object Validator");
            DialogResult result = frmConnect.ShowDialog();
            if (result == DialogResult.OK)
            {
                this.connData = frmConnect.SqlConnection;
                ddDatabaseList.Items.Clear();
                for (int i = 0; i < frmConnect.DatabaseList.Count; i++)
                    ddDatabaseList.Items.Add(frmConnect.DatabaseList[i].DatabaseName);
                this.settingsControl1.Server = this.connData.SQLServerName;
            }
        }

        private void ObjectValidation_Load(object sender, System.EventArgs e)
        {
            DatabaseList dbs = SqlSync.DbInformation.InfoHelper.GetDatabaseList(connData);
            for (int i = 0; i < dbs.Count; i++)
                ddDatabaseList.Items.Add(dbs[i].DatabaseName);
            this.settingsControl1.Server = this.connData.SQLServerName;
        }

        private void btnValidate_Click(object sender, System.EventArgs e)
        {
            this.lstResults.Items.Clear();
            this.pnlWarning.Visible = false;
            this.hasValidationErrors = false;
            this.statStatus.Text = "Validating Objects Data";
            this.btnValidate.Enabled = false;
            this.Cursor = Cursors.AppStarting;
            try
            {
                this.connData.DatabaseName = ddDatabaseList.SelectedItem.ToString();
                bgworker.RunWorkerAsync(this.connData);
            }
            catch (Exception ex)
            {
                string m = ex.ToString();
                this.statStatus.Text = "ERROR: Unable to Retrieve Data";
                this.Cursor = Cursors.Default;
                this.btnValidate.Enabled = true;
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

            SqlSync.ObjectScript.ObjectScriptHelper helper = new ObjectScriptHelper(this.connData);
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
                    frmDisplay = new ScriptDisplayForm(script, this.connData.SQLServerName, item.SubItems[0].Text);
                else
                    frmDisplay = new ScriptDisplayForm(message, this.connData.SQLServerName, item.SubItems[0].Text);

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
            validator.Validate(this.connData, sender as BackgroundWorker, e);

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
                        this.hasValidationErrors = true;
                        break;
                    case ValidationResultValue.Caution:
                        item.BackColor = Color.Yellow;
                        this.hasValidationErrors = true;
                        break;
                    case ValidationResultValue.CrossDatabaseJoin:
                        item.BackColor = Color.Beige;
                        this.hasValidationErrors = true;
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
                this.pnlWarning.Visible = this.hasValidationErrors;
                this.statStatus.Text = "Complete";
                this.Cursor = Cursors.Default;
                this.Invalidate();
            }
            else
            {
                this.pnlWarning.Visible = this.hasValidationErrors;
                this.statStatus.Text = "Cancelled";
                this.Cursor = Cursors.Default;
                this.Invalidate();
            }
            this.btnCancel.Enabled = true;
            this.btnValidate.Enabled = true;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.btnCancel.Enabled = false;
            this.bgworker.CancelAsync();
        }

        private void settingsControl1_ServerChanged(object sender, string serverName,string username, string password, AuthenticationType authType)
        {
            Connection.ConnectionData oldConnData = new Connection.ConnectionData();
            this.connData.Fill(oldConnData);
            this.Cursor = Cursors.WaitCursor;

            this.connData.SQLServerName = serverName;
            if (!string.IsNullOrWhiteSpace(username) && (!string.IsNullOrWhiteSpace(password)))
            {
                this.connData.UserId = username;
                this.connData.Password = password;
            }
            this.connData.AuthenticationType = authType;
            this.connData.ScriptTimeout = 5;
            try
            {
                DatabaseList dbList = SqlSync.DbInformation.InfoHelper.GetDatabaseList(this.connData);
                //this.LookUpTable_Load(null, EventArgs.Empty);
            }
            catch
            {
                MessageBox.Show("Error retrieving database list. Is the server running?", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.connData = oldConnData;
                this.settingsControl1.Server = oldConnData.SQLServerName;
            }


            this.Cursor = Cursors.Default;
        }

        private void toolStripMenuItem1_Click(object sender, EventArgs e)
        {
            SqlSync.SqlBuild.UtilityHelper.OpenManual("DatabaseObjectValidation");
        }

    }
    public delegate void SetWarningVisability(bool show);
}
