using SqlBuildManager.Enterprise;
using SqlBuildManager.Interfaces.ScriptHandling.Tags;
using SqlBuildManager.ScriptHandling;
using SqlSync.Constants;
using SqlSync.DbInformation;
using SqlSync.SqlBuild.Validator;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
namespace SqlSync.SqlBuild
{
    /// <summary>
    /// Summary description for NewBuildScriptForm.
    /// </summary>
    public class NewBuildScriptForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblFileName;
        private System.Windows.Forms.RichTextBox rtbDescription;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtBuildOrder;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.CheckBox chkRollBackScript;
        private System.Guid scriptId = System.Guid.Empty;


        private System.Windows.Forms.CheckBox chkRollBackBuild;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.CheckBox chkStripTransactions;
        private System.Windows.Forms.CheckBox chkAllowMultipleRuns;
        private System.Windows.Forms.Label lblAddedBy;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.TextBox txtScriptTimeout;
        private System.ComponentModel.IContainer components;
        private SqlSyncBuildData.ScriptRow[] scriptRows = null;
        //private SqlSyncBuildData.ScriptRow scriptRow = null;
        private bool scriptTimeoutChanged = false;
        private Label label6;
        private Panel panel2;
        private Label lblModBy;
        private Label label10;
        private Label label9;
        private Label lblModDate;
        private Label lblAddDate;
        private ComboBox cbTag;
        private Label label8;
        private bool selectedDatabaseChanged = false;
        private bool tagSelectionChanged = false;
        private bool inferSourceChanged = false;
        private string fullPathAndFileName = string.Empty;
        public bool TagSelectionChanged
        {
            get { return tagSelectionChanged; }

        }
        private List<string> tagList = new List<string>();
        private Color multipleTagsColor = Color.Coral;
        private DatabaseDropDown ddDatabaseList;
        private Label label11;
        private ComboBox ddInfer;

        public SqlSyncBuildData.ScriptRow[] ScriptRows
        {
            get
            {
                return scriptRows;
            }
        }
        public string FileName
        {
            get
            {
                return lblFileName.Text;
            }
        }
        public double BuildOrder
        {
            get
            {
                return double.Parse(txtBuildOrder.Text);
            }
        }
        public bool RollBackScript
        {
            get
            {
                return chkRollBackScript.Checked;
            }
        }
        public bool RollBackBuild
        {
            get
            {
                return chkRollBackBuild.Checked;
            }
        }
        public string Description
        {
            get
            {
                return rtbDescription.Text;
            }
        }
        public bool StripTransactions
        {
            get
            {
                return chkStripTransactions.Checked;
            }
        }
        public System.Guid ScriptID
        {
            get
            {
                return scriptId;
            }
        }
        public string DatabaseName
        {
            get
            {
                return ddDatabaseList.SelectedDatabase;
            }
        }
        public bool AllowMultipleRuns
        {
            get
            {
                return chkAllowMultipleRuns.Checked;
            }
        }
        public int ScriptTimeout
        {
            get
            {
                try
                {
                    return Int32.Parse(txtScriptTimeout.Text);
                }
                catch
                {
                    return EnterpriseConfigHelper.GetMinumumScriptTimeout(lblFileName.Text, SqlSync.Properties.Settings.Default.DefaultMinimumScriptTimeout);
                }
            }
        }
        public TagInferenceSource TagInferSource
        {
            get;
            set;
        }
        private string baseFilePath = string.Empty;
        private string scriptTag = string.Empty;

        public string ScriptTag
        {
            get
            {
                if (TagInferSource == TagInferenceSource.None)
                    return (cbTag.SelectedValue == null) ? cbTag.Text : cbTag.SelectedValue.ToString();
                else
                {
                    if (scriptTag.Length != 0)
                        return scriptTag;
                    else
                        return "Default";
                }
            }
        }

        private NewBuildScriptForm(DatabaseList databaseList, List<string> tagList, string lastDatabase, string baseFilePath)
        {
            InitializeComponent();
            this.tagList = tagList;
            this.baseFilePath = baseFilePath;
            ddInfer.SelectedIndex = 0;
            ddDatabaseList.SetData(databaseList, lastDatabase);
            for (int i = 0; i < this.tagList.Count; i++)
                cbTag.Items.Add(this.tagList[i]);

        }
        /// <summary>
        /// Use this constructor for new scripts being added from an "Add File" option (vs. add text)
        /// </summary>
        /// <param name="fullPathAndFileName"></param>
        /// <param name="databaseList"></param>
        /// <param name="lastBuildNumber"></param>
        /// <param name="lastDatabase"></param>
        /// <param name="addedBy"></param>
        public NewBuildScriptForm(string baseFilePath, string fullPathAndFileName, DatabaseList databaseList, double lastBuildNumber, string lastDatabase, string addedBy, List<string> tagList) : this(databaseList, tagList, lastDatabase, baseFilePath)
        {
            this.tagList = tagList;
            if (String.Equals(fullPathAndFileName, "<bulk>", StringComparison.CurrentCultureIgnoreCase))
            {
                lblFileName.Text = "<bulk>";
            }
            else
            {
                lblFileName.Text = Path.GetFileName(fullPathAndFileName);
                this.fullPathAndFileName = fullPathAndFileName;
            }

            lblAddedBy.Text = addedBy;


            txtBuildOrder.Text = Math.Floor(lastBuildNumber + 1).ToString();
            try
            {
                if (fullPathAndFileName.ToLower() == "<bulk>")
                {
                    txtBuildOrder.Enabled = false;
                    return;
                }

                string fileExtention = System.IO.Path.GetExtension(fullPathAndFileName).ToUpper();
                switch (fileExtention)
                {
                    case DbObjectType.StoredProcedure:
                    case DbObjectType.UserDefinedFunction:
                    case DbObjectType.Trigger:
                        chkAllowMultipleRuns.Checked = true;
                        chkStripTransactions.CheckedChanged -= new System.EventHandler(chkStripTransactions_CheckedChanged);
                        chkStripTransactions.Checked = false;
                        chkStripTransactions.CheckedChanged += new System.EventHandler(chkStripTransactions_CheckedChanged);
                        break;
                    case DbObjectType.View:
                        chkAllowMultipleRuns.Checked = true;
                        break;
                }
            }
            catch { }


        }

        /// <summary>
        /// Use this constuctor for editing an existing single script
        /// </summary>
        /// <param name="scriptRow"></param>
        /// <param name="databaseList"></param>
        /// <param name="tagList"></param>
        //public NewBuildScriptForm(ref SqlSyncBuildData.ScriptRow scriptRow, DatabaseList databaseList, List<string> tagList) : this(databaseList,tagList,"")
        //{

        //    this.scriptRow = scriptRow;
        //    this.scriptTag = scriptRow.Tag;
        //    this.lblFileName.Text = scriptRow.FileName;
        //    this.lblAddedBy.Text = scriptRow.AddedBy;
        //    this.rtbDescription.Text = scriptRow.Description;
        //    this.txtBuildOrder.Text = scriptRow.BuildOrder.ToString();
        //    this.chkRollBackScript.Checked = scriptRow.RollBackOnError;
        //    this.chkRollBackBuild.Checked = scriptRow.CausesBuildFailure;
        //    this.chkStripTransactions.Checked = scriptRow.StripTransactionText;
        //    this.scriptId = new Guid(scriptRow.ScriptId);
        //    this.chkAllowMultipleRuns.Checked = scriptRow.AllowMultipleRuns;
        //    this.txtScriptTimeout.Text = scriptRow.ScriptTimeOut.ToString();
        //    this.lblAddDate.Text = scriptRow.DateAdded.ToString();
        //    if (!scriptRow.IsDateModifiedNull() && scriptRow.DateModified != DateTime.MinValue && scriptRow.DateModified != scriptRow.DateAdded)
        //    {
        //        this.lblModDate.Text = scriptRow.DateModified.ToString();
        //        this.lblModBy.Text = scriptRow.ModifiedBy;
        //    }

        //    //Set the tag
        //    for(int i=0;i<this.cbTag.Items.Count;i++)
        //        if (this.cbTag.Items[i].ToString().ToLower() == scriptRow.Tag.ToLower()) cbTag.SelectedIndex = i;



        //}

        /// <summary>
        /// Use this constructor for editing multiple existing rows at a time. 
        /// </summary>
        /// <param name="scriptRows"></param>
        public NewBuildScriptForm(ref SqlSyncBuildData.ScriptRow[] scriptRows, string baseFilePath, DatabaseList databaseList, List<string> tagList)
            : this(databaseList, tagList, "", baseFilePath)
        {
            this.scriptRows = scriptRows;
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewBuildScriptForm));
            label1 = new System.Windows.Forms.Label();
            lblFileName = new System.Windows.Forms.Label();
            rtbDescription = new System.Windows.Forms.RichTextBox();
            label2 = new System.Windows.Forms.Label();
            txtBuildOrder = new System.Windows.Forms.TextBox();
            label3 = new System.Windows.Forms.Label();
            chkRollBackScript = new System.Windows.Forms.CheckBox();
            chkRollBackBuild = new System.Windows.Forms.CheckBox();
            button2 = new System.Windows.Forms.Button();
            button1 = new System.Windows.Forms.Button();
            label4 = new System.Windows.Forms.Label();
            chkStripTransactions = new System.Windows.Forms.CheckBox();
            chkAllowMultipleRuns = new System.Windows.Forms.CheckBox();
            lblAddedBy = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            panel1 = new System.Windows.Forms.Panel();
            lblModDate = new System.Windows.Forms.Label();
            lblAddDate = new System.Windows.Forms.Label();
            label10 = new System.Windows.Forms.Label();
            label9 = new System.Windows.Forms.Label();
            lblModBy = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            label5 = new System.Windows.Forms.Label();
            txtScriptTimeout = new System.Windows.Forms.TextBox();
            label8 = new System.Windows.Forms.Label();
            ddInfer = new System.Windows.Forms.ComboBox();
            panel2 = new System.Windows.Forms.Panel();
            label11 = new System.Windows.Forms.Label();
            ddDatabaseList = new SqlSync.DatabaseDropDown();
            cbTag = new System.Windows.Forms.ComboBox();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            label1.Location = new System.Drawing.Point(14, 10);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(87, 20);
            label1.TabIndex = 0;
            label1.Text = "File Name:";
            // 
            // lblFileName
            // 
            lblFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            lblFileName.Location = new System.Drawing.Point(101, 10);
            lblFileName.Name = "lblFileName";
            lblFileName.Size = new System.Drawing.Size(611, 20);
            lblFileName.TabIndex = 1;
            // 
            // rtbDescription
            // 
            rtbDescription.Location = new System.Drawing.Point(317, 80);
            rtbDescription.Name = "rtbDescription";
            rtbDescription.Size = new System.Drawing.Size(300, 98);
            rtbDescription.TabIndex = 8;
            rtbDescription.Text = "";
            // 
            // label2
            // 
            label2.Location = new System.Drawing.Point(317, 62);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(115, 19);
            label2.TabIndex = 3;
            label2.Text = "Script Description:";
            // 
            // txtBuildOrder
            // 
            txtBuildOrder.Location = new System.Drawing.Point(238, 32);
            txtBuildOrder.Name = "txtBuildOrder";
            txtBuildOrder.Size = new System.Drawing.Size(115, 23);
            txtBuildOrder.TabIndex = 1;
            toolTip1.SetToolTip(txtBuildOrder, "#\'s greater than 1000 can not be re-sequenced");
            // 
            // label3
            // 
            label3.Location = new System.Drawing.Point(238, 10);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(115, 20);
            label3.TabIndex = 5;
            label3.Text = "Build Sequence #:";
            toolTip1.SetToolTip(label3, "#\'s greater than 1000 can not be re-sequenced");
            // 
            // chkRollBackScript
            // 
            chkRollBackScript.Checked = true;
            chkRollBackScript.CheckState = System.Windows.Forms.CheckState.Checked;
            chkRollBackScript.Enabled = false;
            chkRollBackScript.Location = new System.Drawing.Point(18, 110);
            chkRollBackScript.Name = "chkRollBackScript";
            chkRollBackScript.Size = new System.Drawing.Size(308, 34);
            chkRollBackScript.TabIndex = 5;
            chkRollBackScript.Text = "Roll back full script file contents on partial failure";
            toolTip1.SetToolTip(chkRollBackScript, resources.GetString("chkRollBackScript.ToolTip"));
            // 
            // chkRollBackBuild
            // 
            chkRollBackBuild.Checked = true;
            chkRollBackBuild.CheckState = System.Windows.Forms.CheckState.Checked;
            chkRollBackBuild.Location = new System.Drawing.Point(18, 80);
            chkRollBackBuild.Name = "chkRollBackBuild";
            chkRollBackBuild.Size = new System.Drawing.Size(230, 30);
            chkRollBackBuild.TabIndex = 4;
            chkRollBackBuild.Text = "Roll back entire build on failure";
            toolTip1.SetToolTip(chkRollBackBuild, "Check to have any script failure cause the entire build to get rolled back.\r\nFor " +
        "most scripts, this should be left checked.");
            chkRollBackBuild.CheckedChanged += new System.EventHandler(chkRollBackBuild_CheckedChanged);
            // 
            // button2
            // 
            button2.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            button2.Location = new System.Drawing.Point(369, 220);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(68, 25);
            button2.TabIndex = 11;
            button2.Text = "Cancel";
            // 
            // button1
            // 
            button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            button1.Location = new System.Drawing.Point(293, 220);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(57, 25);
            button1.TabIndex = 10;
            button1.Text = "OK";
            button1.Click += new System.EventHandler(button1_Click);
            // 
            // label4
            // 
            label4.Location = new System.Drawing.Point(18, 10);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(115, 20);
            label4.TabIndex = 11;
            label4.Text = "Target Database:";
            // 
            // chkStripTransactions
            // 
            chkStripTransactions.Checked = true;
            chkStripTransactions.CheckState = System.Windows.Forms.CheckState.Checked;
            chkStripTransactions.Location = new System.Drawing.Point(18, 139);
            chkStripTransactions.Name = "chkStripTransactions";
            chkStripTransactions.Size = new System.Drawing.Size(230, 30);
            chkStripTransactions.TabIndex = 6;
            chkStripTransactions.Text = "Strip Transaction References";
            toolTip1.SetToolTip(chkStripTransactions, "Check to strip out transaction references in the file and allow Sql Buid Manager " +
        "to manage transactions. \r\nThis in generally only unchecked for Stored Procedures" +
        " and Function scripts.");
            chkStripTransactions.CheckedChanged += new System.EventHandler(chkStripTransactions_CheckedChanged);
            // 
            // chkAllowMultipleRuns
            // 
            chkAllowMultipleRuns.Checked = true;
            chkAllowMultipleRuns.CheckState = System.Windows.Forms.CheckState.Checked;
            chkAllowMultipleRuns.Location = new System.Drawing.Point(18, 169);
            chkAllowMultipleRuns.Name = "chkAllowMultipleRuns";
            chkAllowMultipleRuns.Size = new System.Drawing.Size(259, 49);
            chkAllowMultipleRuns.TabIndex = 7;
            chkAllowMultipleRuns.Text = "Allow Multiple Committed Runs on same Server";
            toolTip1.SetToolTip(chkAllowMultipleRuns, "Check to allow the script to be run more than once on the target server.");
            // 
            // lblAddedBy
            // 
            lblAddedBy.Location = new System.Drawing.Point(150, 39);
            lblAddedBy.Name = "lblAddedBy";
            lblAddedBy.Size = new System.Drawing.Size(155, 20);
            lblAddedBy.TabIndex = 15;
            // 
            // label7
            // 
            label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            label7.Location = new System.Drawing.Point(14, 39);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(87, 20);
            label7.TabIndex = 14;
            label7.Text = "Added By:";
            // 
            // panel1
            // 
            panel1.BackColor = System.Drawing.Color.White;
            panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panel1.Controls.Add(lblModDate);
            panel1.Controls.Add(lblAddDate);
            panel1.Controls.Add(label10);
            panel1.Controls.Add(label9);
            panel1.Controls.Add(lblModBy);
            panel1.Controls.Add(label6);
            panel1.Controls.Add(lblAddedBy);
            panel1.Controls.Add(label7);
            panel1.Controls.Add(label1);
            panel1.Controls.Add(lblFileName);
            panel1.Dock = System.Windows.Forms.DockStyle.Top;
            panel1.Location = new System.Drawing.Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(729, 91);
            panel1.TabIndex = 16;
            // 
            // lblModDate
            // 
            lblModDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            lblModDate.Location = new System.Drawing.Point(425, 59);
            lblModDate.Name = "lblModDate";
            lblModDate.Size = new System.Drawing.Size(287, 20);
            lblModDate.TabIndex = 21;
            // 
            // lblAddDate
            // 
            lblAddDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            lblAddDate.Location = new System.Drawing.Point(425, 39);
            lblAddDate.Name = "lblAddDate";
            lblAddDate.Size = new System.Drawing.Size(289, 20);
            lblAddDate.TabIndex = 20;
            // 
            // label10
            // 
            label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            label10.Location = new System.Drawing.Point(312, 59);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(119, 20);
            label10.TabIndex = 19;
            label10.Text = "Last Mod Date:";
            // 
            // label9
            // 
            label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            label9.Location = new System.Drawing.Point(312, 39);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(119, 20);
            label9.TabIndex = 18;
            label9.Text = "Add Date:";
            // 
            // lblModBy
            // 
            lblModBy.Location = new System.Drawing.Point(150, 59);
            lblModBy.Name = "lblModBy";
            lblModBy.Size = new System.Drawing.Size(155, 20);
            lblModBy.TabIndex = 17;
            // 
            // label6
            // 
            label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            label6.Location = new System.Drawing.Point(13, 59);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(135, 20);
            label6.TabIndex = 16;
            label6.Text = "Last Modified By:";
            // 
            // toolTip1
            // 
            toolTip1.AutoPopDelay = 10000;
            toolTip1.InitialDelay = 500;
            toolTip1.IsBalloon = true;
            toolTip1.ReshowDelay = 100;
            // 
            // label5
            // 
            label5.Location = new System.Drawing.Point(383, 10);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(169, 20);
            label5.TabIndex = 17;
            label5.Text = "Time Out (seconds):";
            toolTip1.SetToolTip(label5, "#\'s greater than 1000 can not be re-sequenced");
            // 
            // txtScriptTimeout
            // 
            txtScriptTimeout.Location = new System.Drawing.Point(383, 32);
            txtScriptTimeout.Name = "txtScriptTimeout";
            txtScriptTimeout.Size = new System.Drawing.Size(119, 23);
            txtScriptTimeout.TabIndex = 2;
            txtScriptTimeout.Text = "20";
            toolTip1.SetToolTip(txtScriptTimeout, "#\'s greater than 1000 can not be re-sequenced");
            txtScriptTimeout.Leave += new System.EventHandler(txtScriptTimeout_Leave);
            // 
            // label8
            // 
            label8.Location = new System.Drawing.Point(505, 10);
            label8.Name = "label8";
            label8.Size = new System.Drawing.Size(79, 20);
            label8.TabIndex = 27;
            label8.Text = "Script Tag:";
            toolTip1.SetToolTip(label8, "Target Database to run this script on");
            // 
            // ddInfer
            // 
            ddInfer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            ddInfer.FormattingEnabled = true;
            ddInfer.Items.AddRange(new object[] {
            "None",
            "Script/Name",
            "Name/Script",
            "Script Only",
            "Name Only"});
            ddInfer.Location = new System.Drawing.Point(472, 192);
            ddInfer.Name = "ddInfer";
            ddInfer.Size = new System.Drawing.Size(145, 23);
            ddInfer.TabIndex = 9;
            toolTip1.SetToolTip(ddInfer, resources.GetString("ddInfer.ToolTip"));
            // 
            // panel2
            // 
            panel2.Controls.Add(label11);
            panel2.Controls.Add(ddInfer);
            panel2.Controls.Add(ddDatabaseList);
            panel2.Controls.Add(cbTag);
            panel2.Controls.Add(label8);
            panel2.Controls.Add(rtbDescription);
            panel2.Controls.Add(label2);
            panel2.Controls.Add(button2);
            panel2.Controls.Add(button1);
            panel2.Controls.Add(txtScriptTimeout);
            panel2.Controls.Add(label3);
            panel2.Controls.Add(chkStripTransactions);
            panel2.Controls.Add(chkRollBackBuild);
            panel2.Controls.Add(label4);
            panel2.Controls.Add(chkRollBackScript);
            panel2.Controls.Add(chkAllowMultipleRuns);
            panel2.Controls.Add(txtBuildOrder);
            panel2.Controls.Add(label5);
            panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            panel2.Location = new System.Drawing.Point(0, 91);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(729, 262);
            panel2.TabIndex = 19;
            // 
            // label11
            // 
            label11.Location = new System.Drawing.Point(335, 196);
            label11.Name = "label11";
            label11.Size = new System.Drawing.Size(129, 19);
            label11.TabIndex = 31;
            label11.Text = "Infer tag value from:";
            // 
            // ddDatabaseList
            // 
            ddDatabaseList.DatabaseList = null;
            ddDatabaseList.FormattingEnabled = true;
            ddDatabaseList.Location = new System.Drawing.Point(22, 31);
            ddDatabaseList.Name = "ddDatabaseList";
            ddDatabaseList.SelectedDatabase = "";
            ddDatabaseList.Size = new System.Drawing.Size(208, 23);
            ddDatabaseList.TabIndex = 0;
            // 
            // cbTag
            // 
            cbTag.FormattingEnabled = true;
            cbTag.Location = new System.Drawing.Point(509, 31);
            cbTag.Name = "cbTag";
            cbTag.Size = new System.Drawing.Size(108, 23);
            cbTag.TabIndex = 3;
            cbTag.SelectionChangeCommitted += new System.EventHandler(cbTag_SelectionChangeCommitted);
            cbTag.TextUpdate += new System.EventHandler(cbTag_SelectionChangeCommitted);
            // 
            // NewBuildScriptForm
            // 
            AutoScaleBaseSize = new System.Drawing.Size(6, 16);
            ClientSize = new System.Drawing.Size(729, 353);
            Controls.Add(panel2);
            Controls.Add(panel1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            Name = "NewBuildScriptForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Add / Edit Build Script ";
            TopMost = true;
            Closing += new System.ComponentModel.CancelEventHandler(NewBuildScriptForm_Closing);
            Load += new System.EventHandler(NewBuildScriptForm_Load);
            panel1.ResumeLayout(false);
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ResumeLayout(false);

        }
        #endregion

        private void button1_Click(object sender, System.EventArgs e)
        {
            int minTimeout = EnterpriseConfigHelper.GetMinumumScriptTimeout(lblFileName.Text, SqlSync.Properties.Settings.Default.DefaultMinimumScriptTimeout);
            if (txtScriptTimeout.Enabled && ScriptSettingValidation.CheckScriptTimeoutValue(txtScriptTimeout.Text, minTimeout) != ScriptTimeoutValidationResult.Ok)
            {
                txtScriptTimeout_Leave(null, EventArgs.Empty);
                return;
            }

            bool showMessage = false;
            if (txtBuildOrder.Enabled == true)
            {
                if (txtBuildOrder.Text.Length != 0)
                {
                    try
                    {
                        Double.Parse(txtBuildOrder.Text);
                    }
                    catch
                    {
                        showMessage = true;
                    }
                }
                else
                {
                    showMessage = true;
                }
            }

            if (ddDatabaseList.Enabled == true)
            {
                if (ddDatabaseList.SelectedDatabase.Length == 0)
                {
                    showMessage = true;
                }
            }

            if (showMessage)
            {
                MessageBox.Show("A Target Database and Build Sequence number are required", "Missing Values", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else if (SqlSync.Properties.Settings.Default.RequireScriptTags && cbTag.Text.Length == 0 && cbTag.SelectedValue == null)
            {
                bool passedTagTest = false;
                if (!tagSelectionChanged && cbTag.BackColor == multipleTagsColor)
                    passedTagTest = true;

                if (cbTag.BackColor != multipleTagsColor && ddInfer.Text != "None")
                    passedTagTest = true;

                if (!passedTagTest)
                {
                    string message = "A Target Database, Build Sequence number and Tag are required";
                    if (SqlSync.Properties.Settings.Default.RequireScriptTagsMessage != null)
                        message = SqlSync.Properties.Settings.Default.RequireScriptTagsMessage;

                    showMessage = true;
                    MessageBox.Show(message, "Missing Tag Value", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            if (!showMessage)
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        private void chkStripTransactions_CheckedChanged(object sender, System.EventArgs e)
        {
            if (chkStripTransactions.Checked == false)
            {
                DialogResult result = MessageBox.Show("Are you certain you want to leave transaction references?\r\nGenerally, you should let Sql Build Manager handle them", "Leave References", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                if (result == DialogResult.No)
                {
                    chkStripTransactions.Checked = true;
                }
            }
        }

        private void txtScriptTimeout_Leave(object sender, System.EventArgs e)
        {
            int minDefault = EnterpriseConfigHelper.GetMinumumScriptTimeout(lblFileName.Text, SqlSync.Properties.Settings.Default.DefaultMinimumScriptTimeout);
            ScriptTimeoutValidationResult result = ScriptSettingValidation.CheckScriptTimeoutValue(txtScriptTimeout.Text, minDefault);
            switch (result)
            {
                case ScriptTimeoutValidationResult.TimeOutTooSmall:
                    string message = string.Format("The script timeout setting was smaller than the minimum setting of {0} seconds. The value has been increased accordingly.", minDefault.ToString());
                    txtScriptTimeout.Text = minDefault.ToString();
                    MessageBox.Show(message, "Timeout value too small", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtScriptTimeout.Focus();

                    break;
                case ScriptTimeoutValidationResult.NonIntegerValue:
                    MessageBox.Show("The Script Timeout value must be a valid 32 bit integer", "Bad Timeout value", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    txtScriptTimeout.Focus();
                    break;
            }

        }


        private void NewBuildScriptForm_Load(object sender, System.EventArgs e)
        {
            if (scriptRows == null)
            {
                if (EnterpriseConfigHelper.UserHasCustomTimeoutSetting())
                {
                    txtScriptTimeout.Enabled = false;
                    txtScriptTimeout.Text = "";
                }
                else
                {
                    txtScriptTimeout.Text = EnterpriseConfigHelper.GetMinumumScriptTimeout(lblFileName.Text, SqlSync.Properties.Settings.Default.DefaultMinimumScriptTimeout).ToString();
                }
                return;
            }

            lblFileName.Text = "<bulk>";
            //Loop through and find the users...
            StringDictionary users = new StringDictionary();
            for (int i = 0; i < scriptRows.Length; i++)
                if (!users.ContainsKey(scriptRows[i].AddedBy))
                {
                    lblAddedBy.Text += scriptRows[i].AddedBy + "; ";
                    users.Add(scriptRows[i].AddedBy, scriptRows[i].AddedBy);
                }

            users = null;


            rtbDescription.Enabled = false;
            txtBuildOrder.Enabled = false;

            //Set the script rollback state
            chkRollBackScript.Checked = scriptRows[0].RollBackOnError;
            for (int i = 1; i < scriptRows.Length; i++)
            {
                if (chkRollBackScript.Checked != scriptRows[i].RollBackOnError)
                {
                    chkRollBackScript.CheckState = CheckState.Indeterminate;
                    break;
                }
            }

            //Set the build rollback state
            chkRollBackBuild.Checked = scriptRows[0].CausesBuildFailure;
            for (int i = 1; i < scriptRows.Length; i++)
            {
                if (chkRollBackBuild.Checked != scriptRows[i].CausesBuildFailure)
                {
                    chkRollBackBuild.CheckState = CheckState.Indeterminate;
                    break;
                }
            }

            //Set the strip transactions state
            chkStripTransactions.Checked = scriptRows[0].StripTransactionText;
            for (int i = 1; i < scriptRows.Length; i++)
            {
                if (chkStripTransactions.Checked != scriptRows[i].StripTransactionText)
                {
                    chkStripTransactions.CheckState = CheckState.Indeterminate;
                    break;
                }
            }

            //this.scriptId = scriptId;
            chkAllowMultipleRuns.Checked = scriptRows[0].AllowMultipleRuns;
            for (int i = 1; i < scriptRows.Length; i++)
            {
                if (chkAllowMultipleRuns.Checked != scriptRows[i].AllowMultipleRuns)
                {
                    chkAllowMultipleRuns.CheckState = CheckState.Indeterminate;
                    break;
                }
            }

            //Set the script timeout to the max value
            //Are there mixed values?
            var vals = (from t in scriptRows
                        select t.ScriptTimeOut).Distinct();
            txtScriptTimeout.Text = vals.Max().ToString();
            if (vals.Count() > 1)
                txtScriptTimeout.ForeColor = Color.Red;

            txtScriptTimeout.TextChanged += new System.EventHandler(txtScriptTimeout_TextChanged);

            //Set the database
            string dbName = scriptRows[0].Database;
            for (int i = 1; i < scriptRows.Length; i++)
                if (dbName.ToLower() != scriptRows[i].Database.ToLower())
                {
                    ddDatabaseList.Text = string.Empty;
                    ddDatabaseList.Enabled = false;
                    break;
                }

            if (ddDatabaseList.Enabled)
                ddDatabaseList.SelectedDatabase = dbName;


            ddDatabaseList.SelectionChangeCommitted += new EventHandler(ddDatabaseList_SelectionChangeCommitted);

            ddInfer.SelectionChangeCommitted += new EventHandler(ddInfer_SelectionChangeCommitted);

            //Set the tag
            List<string> tmp = new List<string>();
            for (int i = 0; i < scriptRows.Length; i++)
                if (!tmp.Contains(scriptRows[i].Tag))
                    tmp.Add(scriptRows[i].Tag);

            if (tmp.Count > 1)
                cbTag.BackColor = multipleTagsColor;
            else if (tmp.Count > 0)
            {
                for (int i = 0; i < cbTag.Items.Count; i++)
                    if (cbTag.Items[i].ToString() == tmp[0])
                        cbTag.SelectedIndex = i;
            }

            BringToFront();

        }

        void ddInfer_SelectionChangeCommitted(object sender, EventArgs e)
        {
            inferSourceChanged = true;
        }

        private void txtScriptTimeout_TextChanged(object sender, System.EventArgs e)
        {
            scriptTimeoutChanged = true;
        }

        private void ddDatabaseList_SelectionChangeCommitted(object sender, EventArgs e)
        {
            selectedDatabaseChanged = true;
        }

        private void NewBuildScriptForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {

            switch (ddInfer.Text)
            {

                case "Name/Script":
                    TagInferSource = TagInferenceSource.NameOverText;
                    break;
                case "Script Only":
                    TagInferSource = TagInferenceSource.ScriptText;
                    break;
                case "Name Only":
                    TagInferSource = TagInferenceSource.ScriptName;
                    break;
                case "Script/Name":
                    TagInferSource = TagInferenceSource.TextOverName;
                    break;
                default:
                    TagInferSource = TagInferenceSource.None;
                    break;
            }


            if (DialogResult != DialogResult.OK)
                return;

            List<string> regexTag = new List<string>(SqlSync.Properties.Settings.Default.TagInferenceRegexList.Cast<string>());
            if (TagInferSource != TagInferenceSource.None)
            {
                if (fullPathAndFileName.Length > 0)
                    scriptTag = ScriptTagProcessing.InferScriptTag(TagInferSource, regexTag, Path.GetFileName(fullPathAndFileName), Path.GetDirectoryName(fullPathAndFileName));
                else
                    scriptTag = ScriptTagProcessing.InferScriptTag(TagInferSource, regexTag, FileName, baseFilePath);
            }


            if (scriptRows == null || scriptRows.Length == 0)
                return;


            for (int i = 0; i < scriptRows.Length; i++)
            {
                if (scriptTimeoutChanged)
                    scriptRows[i].ScriptTimeOut = Int32.Parse(txtScriptTimeout.Text);

                if (chkAllowMultipleRuns.CheckState != CheckState.Indeterminate)
                    scriptRows[i].AllowMultipleRuns = chkAllowMultipleRuns.Checked;

                if (chkRollBackBuild.CheckState != CheckState.Indeterminate)
                    scriptRows[i].CausesBuildFailure = chkRollBackBuild.Checked;

                if (chkRollBackScript.CheckState != CheckState.Indeterminate)
                    scriptRows[i].RollBackOnError = chkRollBackScript.Checked;

                if (chkStripTransactions.CheckState != CheckState.Indeterminate)
                    scriptRows[i].StripTransactionText = chkStripTransactions.Checked;

                if (ddDatabaseList.Enabled && selectedDatabaseChanged)
                    scriptRows[i].Database = ddDatabaseList.SelectedDatabase;

                if (tagSelectionChanged)
                {
                    scriptRows[i].Tag = cbTag.SelectedValue == null ? cbTag.Text : cbTag.SelectedValue.ToString();
                }
                else if ((cbTag.BackColor != multipleTagsColor || inferSourceChanged) && cbTag.SelectedValue == null &&
                    cbTag.Text.Length == 0 && TagInferSource != TagInferenceSource.None) //A new add that doesn't have a tag set AND infer is set...
                {
                    string tmpTag = ScriptTagProcessing.InferScriptTag(TagInferSource, regexTag, Path.GetFileName(scriptRows[i].FileName), Path.GetDirectoryName(scriptRows[i].FileName));
                    if (tmpTag.Length > 0)
                    {
                        scriptRows[i].Tag = tmpTag;
                    }
                    else
                    {
                        scriptRows[i].Tag = "Default";
                    }
                }
            }
        }

        private void chkRollBackBuild_CheckedChanged(object sender, EventArgs e)
        {
            if (chkRollBackBuild.Checked)
            {
                chkRollBackScript.Checked = true;
                chkRollBackScript.Enabled = false;
            }
            else
            {
                chkRollBackScript.Enabled = true;
            }
        }

        private void cbTag_SelectionChangeCommitted(object sender, EventArgs e)
        {
            tagSelectionChanged = true;
        }


    }
}
