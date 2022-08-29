using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.Specialized;
using System.Collections.Generic;
using SqlSync.DbInformation;
using SqlSync.SqlBuild.Validator;
using SqlBuildManager.Interfaces.ScriptHandling.Tags;
using SqlBuildManager.ScriptHandling;
using System.IO;
using System.Linq;
using SqlSync.Constants;
using SqlBuildManager.Enterprise;
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
				return this.scriptRows;
			}
		}
		public string FileName
		{
			get
			{
				return this.lblFileName.Text;
			}
		}
		public double BuildOrder
		{
			get
			{
				return double.Parse(this.txtBuildOrder.Text);
			}
		}
		public bool RollBackScript
		{
			get
			{
				return this.chkRollBackScript.Checked;
			}
		}
		public bool RollBackBuild
		{
			get
			{
				return this.chkRollBackBuild.Checked;
			}
		}
		public string Description
		{
			get
			{
				return this.rtbDescription.Text;
			}
		}
		public bool StripTransactions
		{
			get
			{
				return this.chkStripTransactions.Checked;
			}
		}
		public System.Guid ScriptID
		{
			get
			{
				return this.scriptId;
			}
		}
		public string DatabaseName
		{
			get
			{
                return this.ddDatabaseList.SelectedDatabase;
			}
		}
		public bool AllowMultipleRuns
		{
			get
			{
				return this.chkAllowMultipleRuns.Checked;
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
                if (this.TagInferSource == TagInferenceSource.None)
                    return (cbTag.SelectedValue == null) ? cbTag.Text : cbTag.SelectedValue.ToString();
                else
                {
                    if (this.scriptTag.Length != 0)
                        return this.scriptTag;
                    else
                        return "Default";
                }
            }
        }

        private NewBuildScriptForm(DatabaseList databaseList, List<string> tagList, string lastDatabase, string baseFilePath)
        {
            this.InitializeComponent();
            this.tagList = tagList;
            this.baseFilePath = baseFilePath;
            this.ddInfer.SelectedIndex = 0;
            this.ddDatabaseList.SetData(databaseList, lastDatabase);
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
		public NewBuildScriptForm(string baseFilePath, string fullPathAndFileName, DatabaseList databaseList, double lastBuildNumber,string lastDatabase, string addedBy, List<string> tagList) : this(databaseList,tagList, lastDatabase, baseFilePath)
		{
            this.tagList = tagList;
            if (String.Equals(fullPathAndFileName, "<bulk>", StringComparison.CurrentCultureIgnoreCase))
            {
                this.lblFileName.Text = "<bulk>";
            }
            else
            {
                this.lblFileName.Text = Path.GetFileName(fullPathAndFileName);
                this.fullPathAndFileName = fullPathAndFileName;
            }

			this.lblAddedBy.Text = addedBy;
            

			txtBuildOrder.Text = Math.Floor(lastBuildNumber+1).ToString();
			try
			{
                if (fullPathAndFileName.ToLower() == "<bulk>")
                {
                    this.txtBuildOrder.Enabled = false;
                    return;
                }

				string fileExtention = System.IO.Path.GetExtension(fullPathAndFileName).ToUpper();
				switch(fileExtention)
				{
					case DbObjectType.StoredProcedure:
					case DbObjectType.UserDefinedFunction:
                    case DbObjectType.Trigger:
						chkAllowMultipleRuns.Checked = true;
						this.chkStripTransactions.CheckedChanged -= new System.EventHandler(this.chkStripTransactions_CheckedChanged);
						chkStripTransactions.Checked = false;
						this.chkStripTransactions.CheckedChanged += new System.EventHandler(this.chkStripTransactions_CheckedChanged);
						break;
					case DbObjectType.View:
						chkAllowMultipleRuns.Checked = true;
						break;
				}
			}
			catch{}

	
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
            : this(databaseList, tagList, "",baseFilePath)
		{
			this.scriptRows = scriptRows;
		}
		/// <summary>
		/// Clean up any resources being used.
		/// </summary>
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewBuildScriptForm));
            this.label1 = new System.Windows.Forms.Label();
            this.lblFileName = new System.Windows.Forms.Label();
            this.rtbDescription = new System.Windows.Forms.RichTextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.txtBuildOrder = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.chkRollBackScript = new System.Windows.Forms.CheckBox();
            this.chkRollBackBuild = new System.Windows.Forms.CheckBox();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.chkStripTransactions = new System.Windows.Forms.CheckBox();
            this.chkAllowMultipleRuns = new System.Windows.Forms.CheckBox();
            this.lblAddedBy = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.lblModDate = new System.Windows.Forms.Label();
            this.lblAddDate = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.lblModBy = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.label5 = new System.Windows.Forms.Label();
            this.txtScriptTimeout = new System.Windows.Forms.TextBox();
            this.label8 = new System.Windows.Forms.Label();
            this.ddInfer = new System.Windows.Forms.ComboBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label11 = new System.Windows.Forms.Label();
            this.ddDatabaseList = new SqlSync.DatabaseDropDown();
            this.cbTag = new System.Windows.Forms.ComboBox();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(14, 10);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(87, 20);
            this.label1.TabIndex = 0;
            this.label1.Text = "File Name:";
            // 
            // lblFileName
            // 
            this.lblFileName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblFileName.Location = new System.Drawing.Point(101, 10);
            this.lblFileName.Name = "lblFileName";
            this.lblFileName.Size = new System.Drawing.Size(611, 20);
            this.lblFileName.TabIndex = 1;
            // 
            // rtbDescription
            // 
            this.rtbDescription.Location = new System.Drawing.Point(317, 80);
            this.rtbDescription.Name = "rtbDescription";
            this.rtbDescription.Size = new System.Drawing.Size(300, 98);
            this.rtbDescription.TabIndex = 8;
            this.rtbDescription.Text = "";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(317, 62);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(115, 19);
            this.label2.TabIndex = 3;
            this.label2.Text = "Script Description:";
            // 
            // txtBuildOrder
            // 
            this.txtBuildOrder.Location = new System.Drawing.Point(238, 32);
            this.txtBuildOrder.Name = "txtBuildOrder";
            this.txtBuildOrder.Size = new System.Drawing.Size(115, 23);
            this.txtBuildOrder.TabIndex = 1;
            this.toolTip1.SetToolTip(this.txtBuildOrder, "#\'s greater than 1000 can not be re-sequenced");
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(238, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(115, 20);
            this.label3.TabIndex = 5;
            this.label3.Text = "Build Sequence #:";
            this.toolTip1.SetToolTip(this.label3, "#\'s greater than 1000 can not be re-sequenced");
            // 
            // chkRollBackScript
            // 
            this.chkRollBackScript.Checked = true;
            this.chkRollBackScript.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRollBackScript.Enabled = false;
            this.chkRollBackScript.Location = new System.Drawing.Point(18, 110);
            this.chkRollBackScript.Name = "chkRollBackScript";
            this.chkRollBackScript.Size = new System.Drawing.Size(308, 34);
            this.chkRollBackScript.TabIndex = 5;
            this.chkRollBackScript.Text = "Roll back full script file contents on partial failure";
            this.toolTip1.SetToolTip(this.chkRollBackScript, resources.GetString("chkRollBackScript.ToolTip"));
            // 
            // chkRollBackBuild
            // 
            this.chkRollBackBuild.Checked = true;
            this.chkRollBackBuild.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkRollBackBuild.Location = new System.Drawing.Point(18, 80);
            this.chkRollBackBuild.Name = "chkRollBackBuild";
            this.chkRollBackBuild.Size = new System.Drawing.Size(230, 30);
            this.chkRollBackBuild.TabIndex = 4;
            this.chkRollBackBuild.Text = "Roll back entire build on failure";
            this.toolTip1.SetToolTip(this.chkRollBackBuild, "Check to have any script failure cause the entire build to get rolled back.\r\nFor " +
        "most scripts, this should be left checked.");
            this.chkRollBackBuild.CheckedChanged += new System.EventHandler(this.chkRollBackBuild_CheckedChanged);
            // 
            // button2
            // 
            this.button2.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button2.Location = new System.Drawing.Point(369, 220);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(68, 25);
            this.button2.TabIndex = 11;
            this.button2.Text = "Cancel";
            // 
            // button1
            // 
            this.button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button1.Location = new System.Drawing.Point(293, 220);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(57, 25);
            this.button1.TabIndex = 10;
            this.button1.Text = "OK";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(18, 10);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(115, 20);
            this.label4.TabIndex = 11;
            this.label4.Text = "Target Database:";
            // 
            // chkStripTransactions
            // 
            this.chkStripTransactions.Checked = true;
            this.chkStripTransactions.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkStripTransactions.Location = new System.Drawing.Point(18, 139);
            this.chkStripTransactions.Name = "chkStripTransactions";
            this.chkStripTransactions.Size = new System.Drawing.Size(230, 30);
            this.chkStripTransactions.TabIndex = 6;
            this.chkStripTransactions.Text = "Strip Transaction References";
            this.toolTip1.SetToolTip(this.chkStripTransactions, "Check to strip out transaction references in the file and allow Sql Buid Manager " +
        "to manage transactions. \r\nThis in generally only unchecked for Stored Procedures" +
        " and Function scripts.");
            this.chkStripTransactions.CheckedChanged += new System.EventHandler(this.chkStripTransactions_CheckedChanged);
            // 
            // chkAllowMultipleRuns
            // 
            this.chkAllowMultipleRuns.Checked = true;
            this.chkAllowMultipleRuns.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkAllowMultipleRuns.Location = new System.Drawing.Point(18, 169);
            this.chkAllowMultipleRuns.Name = "chkAllowMultipleRuns";
            this.chkAllowMultipleRuns.Size = new System.Drawing.Size(259, 49);
            this.chkAllowMultipleRuns.TabIndex = 7;
            this.chkAllowMultipleRuns.Text = "Allow Multiple Committed Runs on same Server";
            this.toolTip1.SetToolTip(this.chkAllowMultipleRuns, "Check to allow the script to be run more than once on the target server.");
            // 
            // lblAddedBy
            // 
            this.lblAddedBy.Location = new System.Drawing.Point(150, 39);
            this.lblAddedBy.Name = "lblAddedBy";
            this.lblAddedBy.Size = new System.Drawing.Size(155, 20);
            this.lblAddedBy.TabIndex = 15;
            // 
            // label7
            // 
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label7.Location = new System.Drawing.Point(14, 39);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(87, 20);
            this.label7.TabIndex = 14;
            this.label7.Text = "Added By:";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.lblModDate);
            this.panel1.Controls.Add(this.lblAddDate);
            this.panel1.Controls.Add(this.label10);
            this.panel1.Controls.Add(this.label9);
            this.panel1.Controls.Add(this.lblModBy);
            this.panel1.Controls.Add(this.label6);
            this.panel1.Controls.Add(this.lblAddedBy);
            this.panel1.Controls.Add(this.label7);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Controls.Add(this.lblFileName);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(729, 91);
            this.panel1.TabIndex = 16;
            // 
            // lblModDate
            // 
            this.lblModDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblModDate.Location = new System.Drawing.Point(425, 59);
            this.lblModDate.Name = "lblModDate";
            this.lblModDate.Size = new System.Drawing.Size(287, 20);
            this.lblModDate.TabIndex = 21;
            // 
            // lblAddDate
            // 
            this.lblAddDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAddDate.Location = new System.Drawing.Point(425, 39);
            this.lblAddDate.Name = "lblAddDate";
            this.lblAddDate.Size = new System.Drawing.Size(289, 20);
            this.lblAddDate.TabIndex = 20;
            // 
            // label10
            // 
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label10.Location = new System.Drawing.Point(312, 59);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(119, 20);
            this.label10.TabIndex = 19;
            this.label10.Text = "Last Mod Date:";
            // 
            // label9
            // 
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label9.Location = new System.Drawing.Point(312, 39);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(119, 20);
            this.label9.TabIndex = 18;
            this.label9.Text = "Add Date:";
            // 
            // lblModBy
            // 
            this.lblModBy.Location = new System.Drawing.Point(150, 59);
            this.lblModBy.Name = "lblModBy";
            this.lblModBy.Size = new System.Drawing.Size(155, 20);
            this.lblModBy.TabIndex = 17;
            // 
            // label6
            // 
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label6.Location = new System.Drawing.Point(13, 59);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(135, 20);
            this.label6.TabIndex = 16;
            this.label6.Text = "Last Modified By:";
            // 
            // toolTip1
            // 
            this.toolTip1.AutoPopDelay = 10000;
            this.toolTip1.InitialDelay = 500;
            this.toolTip1.IsBalloon = true;
            this.toolTip1.ReshowDelay = 100;
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(383, 10);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(169, 20);
            this.label5.TabIndex = 17;
            this.label5.Text = "Time Out (seconds):";
            this.toolTip1.SetToolTip(this.label5, "#\'s greater than 1000 can not be re-sequenced");
            // 
            // txtScriptTimeout
            // 
            this.txtScriptTimeout.Location = new System.Drawing.Point(383, 32);
            this.txtScriptTimeout.Name = "txtScriptTimeout";
            this.txtScriptTimeout.Size = new System.Drawing.Size(119, 23);
            this.txtScriptTimeout.TabIndex = 2;
            this.txtScriptTimeout.Text = "20";
            this.toolTip1.SetToolTip(this.txtScriptTimeout, "#\'s greater than 1000 can not be re-sequenced");
            this.txtScriptTimeout.Leave += new System.EventHandler(this.txtScriptTimeout_Leave);
            // 
            // label8
            // 
            this.label8.Location = new System.Drawing.Point(505, 10);
            this.label8.Name = "label8";
            this.label8.Size = new System.Drawing.Size(79, 20);
            this.label8.TabIndex = 27;
            this.label8.Text = "Script Tag:";
            this.toolTip1.SetToolTip(this.label8, "Target Database to run this script on");
            // 
            // ddInfer
            // 
            this.ddInfer.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddInfer.FormattingEnabled = true;
            this.ddInfer.Items.AddRange(new object[] {
            "None",
            "Script/Name",
            "Name/Script",
            "Script Only",
            "Name Only"});
            this.ddInfer.Location = new System.Drawing.Point(472, 192);
            this.ddInfer.Name = "ddInfer";
            this.ddInfer.Size = new System.Drawing.Size(145, 23);
            this.ddInfer.TabIndex = 9;
            this.toolTip1.SetToolTip(this.ddInfer, resources.GetString("ddInfer.ToolTip"));
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label11);
            this.panel2.Controls.Add(this.ddInfer);
            this.panel2.Controls.Add(this.ddDatabaseList);
            this.panel2.Controls.Add(this.cbTag);
            this.panel2.Controls.Add(this.label8);
            this.panel2.Controls.Add(this.rtbDescription);
            this.panel2.Controls.Add(this.label2);
            this.panel2.Controls.Add(this.button2);
            this.panel2.Controls.Add(this.button1);
            this.panel2.Controls.Add(this.txtScriptTimeout);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Controls.Add(this.chkStripTransactions);
            this.panel2.Controls.Add(this.chkRollBackBuild);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.chkRollBackScript);
            this.panel2.Controls.Add(this.chkAllowMultipleRuns);
            this.panel2.Controls.Add(this.txtBuildOrder);
            this.panel2.Controls.Add(this.label5);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel2.Location = new System.Drawing.Point(0, 91);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(729, 262);
            this.panel2.TabIndex = 19;
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(335, 196);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(129, 19);
            this.label11.TabIndex = 31;
            this.label11.Text = "Infer tag value from:";
            // 
            // ddDatabaseList
            // 
            this.ddDatabaseList.DatabaseList = null;
            this.ddDatabaseList.FormattingEnabled = true;
            this.ddDatabaseList.Location = new System.Drawing.Point(22, 31);
            this.ddDatabaseList.Name = "ddDatabaseList";
            this.ddDatabaseList.SelectedDatabase = "";
            this.ddDatabaseList.Size = new System.Drawing.Size(208, 23);
            this.ddDatabaseList.TabIndex = 0;
            // 
            // cbTag
            // 
            this.cbTag.FormattingEnabled = true;
            this.cbTag.Location = new System.Drawing.Point(509, 31);
            this.cbTag.Name = "cbTag";
            this.cbTag.Size = new System.Drawing.Size(108, 23);
            this.cbTag.TabIndex = 3;
            this.cbTag.SelectionChangeCommitted += new System.EventHandler(this.cbTag_SelectionChangeCommitted);
            this.cbTag.TextUpdate += new System.EventHandler(this.cbTag_SelectionChangeCommitted);
            // 
            // NewBuildScriptForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 16);
            this.ClientSize = new System.Drawing.Size(729, 353);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "NewBuildScriptForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Add / Edit Build Script ";
            this.TopMost = true;
            this.Closing += new System.ComponentModel.CancelEventHandler(this.NewBuildScriptForm_Closing);
            this.Load += new System.EventHandler(this.NewBuildScriptForm_Load);
            this.panel1.ResumeLayout(false);
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.ResumeLayout(false);

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
			if(txtBuildOrder.Enabled == true)
			{
				if(txtBuildOrder.Text.Length != 0)
				{
					try
					{
						Double.Parse(txtBuildOrder.Text);}
					catch
					{
						showMessage=true;}
				}
				else
				{
					showMessage = true;
				}
			}

			if(ddDatabaseList.Enabled == true)
			{
				if(ddDatabaseList.SelectedDatabase.Length == 0)
				{
					showMessage = true;
				}
			}

			if(showMessage)
			{
				MessageBox.Show("A Target Database and Build Sequence number are required","Missing Values",MessageBoxButtons.OK,MessageBoxIcon.Error);
            }
            else if (SqlSync.Properties.Settings.Default.RequireScriptTags && cbTag.Text.Length == 0 && cbTag.SelectedValue == null)
            {
                bool passedTagTest = false;
                if(!this.tagSelectionChanged && this.cbTag.BackColor == multipleTagsColor)
                    passedTagTest = true;

                if (this.cbTag.BackColor != multipleTagsColor && ddInfer.Text != "None")
                    passedTagTest = true;

                if(!passedTagTest)
                {
                    string message = "A Target Database, Build Sequence number and Tag are required";
                    if (SqlSync.Properties.Settings.Default.RequireScriptTagsMessage != null)
                        message = SqlSync.Properties.Settings.Default.RequireScriptTagsMessage;

                    showMessage = true;
                    MessageBox.Show(message, "Missing Tag Value", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            
            if(!showMessage)
            {
                this.DialogResult = DialogResult.OK;
                this.Close();
            }
		}

		private void chkStripTransactions_CheckedChanged(object sender, System.EventArgs e)
		{
			if(chkStripTransactions.Checked == false)
			{
				DialogResult result =  MessageBox.Show("Are you certain you want to leave transaction references?\r\nGenerally, you should let Sql Build Manager handle them","Leave References",MessageBoxButtons.YesNo,MessageBoxIcon.Question);
				if(result == DialogResult.No)
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
            if (this.scriptRows == null)
            {
                if(EnterpriseConfigHelper.UserHasCustomTimeoutSetting())
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

            this.lblFileName.Text = "<bulk>";
			//Loop through and find the users...
			StringDictionary users = new StringDictionary();
			for(int i=0;i<this.scriptRows.Length;i++)
				if(!users.ContainsKey(this.scriptRows[i].AddedBy))
				{
					this.lblAddedBy.Text += this.scriptRows[i].AddedBy +"; ";
					users.Add(this.scriptRows[i].AddedBy,this.scriptRows[i].AddedBy);
				}

			users = null;


			this.rtbDescription.Enabled = false;
			this.txtBuildOrder.Enabled = false;

			//Set the script rollback state
			this.chkRollBackScript.Checked = this.scriptRows[0].RollBackOnError;
			for(int i=1;i<this.scriptRows.Length;i++)
			{
				if(this.chkRollBackScript.Checked != this.scriptRows[i].RollBackOnError)
				{
					this.chkRollBackScript.CheckState = CheckState.Indeterminate;
					break;
				}
			}
			
			//Set the build rollback state
			this.chkRollBackBuild.Checked = this.scriptRows[0].CausesBuildFailure;
			for(int i=1;i<this.scriptRows.Length;i++)
			{
				if(this.chkRollBackBuild.Checked != this.scriptRows[i].CausesBuildFailure)
				{
					this.chkRollBackBuild.CheckState = CheckState.Indeterminate;
					break;
				}
			}

			//Set the strip transactions state
			this.chkStripTransactions.Checked = this.scriptRows[0].StripTransactionText;
			for(int i=1;i<this.scriptRows.Length;i++)
			{
				if(this.chkStripTransactions.Checked != this.scriptRows[i].StripTransactionText)
				{
					this.chkStripTransactions.CheckState = CheckState.Indeterminate;
					break;
				}
			}

			//this.scriptId = scriptId;
			this.chkAllowMultipleRuns.Checked = this.scriptRows[0].AllowMultipleRuns;
			for(int i=1;i<this.scriptRows.Length;i++)
			{
				if(this.chkAllowMultipleRuns.Checked != this.scriptRows[i].AllowMultipleRuns)
				{
					this.chkAllowMultipleRuns.CheckState = CheckState.Indeterminate;
					break;
				}
			}

			//Set the script timeout to the max value
            //Are there mixed values?
            var vals = (from t in this.scriptRows
                        select t.ScriptTimeOut).Distinct();
            this.txtScriptTimeout.Text = vals.Max().ToString();
            if (vals.Count() > 1)
                this.txtScriptTimeout.ForeColor = Color.Red;

            this.txtScriptTimeout.TextChanged += new System.EventHandler(this.txtScriptTimeout_TextChanged);

			//Set the database
			string dbName = this.scriptRows[0].Database;
			for(int i=1;i<this.scriptRows.Length;i++)
				if(dbName.ToLower() != this.scriptRows[i].Database.ToLower())
				{
					ddDatabaseList.Text = string.Empty;
					ddDatabaseList.Enabled = false;
					break;
				}

            if (ddDatabaseList.Enabled)
                ddDatabaseList.SelectedDatabase = dbName;


			this.ddDatabaseList.SelectionChangeCommitted += new EventHandler(ddDatabaseList_SelectionChangeCommitted);

            this.ddInfer.SelectionChangeCommitted += new EventHandler(ddInfer_SelectionChangeCommitted);

            //Set the tag
            List<string> tmp = new  List<string>();
            for (int i = 0; i < this.scriptRows.Length; i++)
                if(!tmp.Contains(this.scriptRows[i].Tag))
                    tmp.Add(this.scriptRows[i].Tag);

            if (tmp.Count > 1)
                cbTag.BackColor = multipleTagsColor;
            else if (tmp.Count > 0)
            {
                for (int i = 0; i < cbTag.Items.Count; i++)
                    if (cbTag.Items[i].ToString() == tmp[0])
                        cbTag.SelectedIndex = i;
            }

            this.BringToFront();

		}

        void ddInfer_SelectionChangeCommitted(object sender, EventArgs e)
        {
            this.inferSourceChanged = true;
        }

        private void txtScriptTimeout_TextChanged(object sender, System.EventArgs e)
		{
			this.scriptTimeoutChanged = true;
		}

		private void ddDatabaseList_SelectionChangeCommitted(object sender, EventArgs e)
		{
			this.selectedDatabaseChanged = true;
		}

		private void NewBuildScriptForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
           
            switch (ddInfer.Text)
            {

                case "Name/Script":
                    this.TagInferSource = TagInferenceSource.NameOverText;
                    break;
                case "Script Only":
                    this.TagInferSource = TagInferenceSource.ScriptText;
                    break;
                case "Name Only":
                    this.TagInferSource = TagInferenceSource.ScriptName;
                    break;
                case "Script/Name":
                    this.TagInferSource = TagInferenceSource.TextOverName;
                    break;
                default:
                    this.TagInferSource = TagInferenceSource.None;
                    break;
            }


			if(this.DialogResult != DialogResult.OK)
				return;

            List<string> regexTag = new List<string>(SqlSync.Properties.Settings.Default.TagInferenceRegexList.Cast<string>());
            if (this.TagInferSource != TagInferenceSource.None)
            {
                if(this.fullPathAndFileName.Length > 0)
                    this.scriptTag = ScriptTagProcessing.InferScriptTag(this.TagInferSource, regexTag, Path.GetFileName(this.fullPathAndFileName), Path.GetDirectoryName(this.fullPathAndFileName));
                else
                    this.scriptTag = ScriptTagProcessing.InferScriptTag(this.TagInferSource, regexTag, this.FileName, this.baseFilePath);
            }


            if (this.scriptRows == null || this.scriptRows.Length == 0)
                return;

           
			for(int i=0;i<this.scriptRows.Length;i++)
			{
				if(this.scriptTimeoutChanged)
					this.scriptRows[i].ScriptTimeOut = Int32.Parse( this.txtScriptTimeout.Text);

				if(this.chkAllowMultipleRuns.CheckState != CheckState.Indeterminate)
					this.scriptRows[i].AllowMultipleRuns = this.chkAllowMultipleRuns.Checked;

				if(this.chkRollBackBuild.CheckState !=  CheckState.Indeterminate)
					this.scriptRows[i].CausesBuildFailure = this.chkRollBackBuild.Checked;

				if(this.chkRollBackScript.CheckState !=  CheckState.Indeterminate)
					this.scriptRows[i].RollBackOnError = this.chkRollBackScript.Checked;

				if(this.chkStripTransactions.CheckState !=  CheckState.Indeterminate)
					this.scriptRows[i].StripTransactionText = this.chkStripTransactions.Checked;

                if (this.ddDatabaseList.Enabled && this.selectedDatabaseChanged)
                    this.scriptRows[i].Database = this.ddDatabaseList.SelectedDatabase;

                if (this.tagSelectionChanged)
                {
                    this.scriptRows[i].Tag = cbTag.SelectedValue == null ? cbTag.Text : cbTag.SelectedValue.ToString();
                }
                else if ((this.cbTag.BackColor != multipleTagsColor || this.inferSourceChanged) && cbTag.SelectedValue == null &&
                    cbTag.Text.Length == 0 && this.TagInferSource != TagInferenceSource.None) //A new add that doesn't have a tag set AND infer is set...
                {
                    string tmpTag = ScriptTagProcessing.InferScriptTag(this.TagInferSource, regexTag, Path.GetFileName(this.scriptRows[i].FileName), Path.GetDirectoryName(this.scriptRows[i].FileName));
                    if (tmpTag.Length > 0)
                    {
                        this.scriptRows[i].Tag = tmpTag;
                    }
                    else
                    {
                        this.scriptRows[i].Tag = "Default";
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
            this.tagSelectionChanged = true;
        }

       
	}
}
