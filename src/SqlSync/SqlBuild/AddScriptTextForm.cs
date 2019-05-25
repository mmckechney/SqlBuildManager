using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using UrielGuy.SyntaxHighlighting;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using SqlSync.DbInformation;
using System.Text;
using SqlBuildManager.ScriptHandling;
using SqlBuildManager.Enterprise.Policy;
using SqlBuildManager.Enterprise;
using SqlBuildManager.Enterprise.Notification;
using SqlSync.Constants;
using System.Threading.Tasks;
namespace SqlSync.SqlBuild
{
	/// <summary>
	/// Summary description for AddScriptTextForm.
	/// </summary>
	public class AddScriptTextForm : System.Windows.Forms.Form
	{
        
        List<string> tagList = new List<string>();
		private System.Drawing.Color oddColor = System.Drawing.Color.Black;
		private System.Drawing.Color evenColor = System.Drawing.Color.Blue;
		private string fullFilePath = string.Empty;
		private System.Windows.Forms.TextBox txtScriptName;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Button btnOK;
		private System.Windows.Forms.Button button2;
		private string sqlText = string.Empty;
        private string sqlName = string.Empty;
		private SqlBuild.Utility.SqlSyncUtilityRegistry utilRegistry = null;
		private UrielGuy.SyntaxHighlighting.SyntaxHighlightingTextBox rtbSqlScript;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Panel panel1;
        private SqlSyncBuildData.ScriptRow scriptCfgRow = null;
        private bool allowEdit = true;

        private bool configurationChanged = false;

        public bool ConfigurationChanged
        {
            get { return configurationChanged; }
        }
        private bool buildSequenceChanged = false;
        public bool BuildSequenceChanged
        {
            get { return buildSequenceChanged; }
        }

        public SqlSyncBuildData.ScriptRow ScriptCfgRow
        {
            get { return scriptCfgRow; }
            set { scriptCfgRow = value; }
        }
		private string fileHash = string.Empty;
		private System.Windows.Forms.Label lblHash;
		private System.Windows.Forms.TextBox txtScriptId;
		private System.Windows.Forms.TextBox txtFileHash;
		private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.ToolTip toolTip1;
        private Label lblAddDate;
        private Label label10;
        private Label label9;
        private Label lblModBy;
        private Label label6;
        private Label lblAddedBy;
        private Label label7;
        private TableLayoutPanel tableLayoutPanel1;
        private Label lblModDate;
        private Panel panel3;
        private Splitter splitter1;
        private Panel panel4;
        private ScriptConfigCtrl scriptConfigCtrl1;
		private System.ComponentModel.IContainer components;
        private DatabaseList databaseList;
        private Label lblHighlightLimit;
        private FinderCtrl finderCtrl1;
        private Label label3;
        private Label lblCharacterNumber;
        private Label lblLineNumber;
        private Label label5;
        private PictureBox pictureBox1;
        private CutCopyPasteContextMenuStrip cutCopyPastecontextMenuStrip1;
        private ToolStripSeparator toolStripSeparator1;
        private LinkLabel lnkRunPolicyChecks;
        private bool scriptTagRequired;
        private Panel panel5;
        private Controls.CodeReviewControl codeReviewControl1;
        private SqlSyncBuildData buildData;
		public string SqlText
		{
			get
			{
				return this.sqlText;
			}
		}
		public string SqlName
		{
			get
			{
				return this.sqlName;
			}
		}
        private AddScriptTextForm()
        {
            InitializeComponent();
        }
        /// <summary>
        /// Call this constructor when presenting a blank new script form for the user
        /// </summary>
        /// <param name="scriptCfgRow"></param>
        /// <param name="utilRegistry"></param>
        /// <param name="databaseList"></param>
        public AddScriptTextForm(ref SqlSyncBuildData buildData, ref SqlSyncBuildData.ScriptRow scriptCfgRow, SqlBuild.Utility.SqlSyncUtilityRegistry utilRegistry, DatabaseList databaseList,List<string> tagList, bool scriptTagRequired)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
            this.utilRegistry = utilRegistry; 
            this.scriptConfigCtrl1.ShowFull = true;
            this.scriptCfgRow = scriptCfgRow;
            this.databaseList = databaseList;
            this.scriptConfigCtrl1.ShowHighlightColor();
            this.tagList = tagList;
            this.scriptTagRequired = scriptTagRequired;
            this.buildData = buildData;
        }
        /// <summary>
        /// Use this contructor when presenting an existing script for editing
        /// </summary>
        /// <param name="shortFileName"></param>
        /// <param name="fullFilePath"></param>
        /// <param name="utilRegistry"></param>
        /// <param name="scriptCfgRow"></param>
        /// <param name="fileHash"></param>
        /// <param name="databaseList"></param>
        public AddScriptTextForm(ref SqlSyncBuildData buildData, string shortFileName, string fullFilePath, SqlBuild.Utility.SqlSyncUtilityRegistry utilRegistry, ref SqlSyncBuildData.ScriptRow scriptCfgRow, string fileHash, DatabaseList databaseList, List<string> tagList, bool scriptTagRequired, bool allowEdit)
		{
			InitializeComponent();
			this.utilRegistry = utilRegistry;
			this.txtScriptName.Text = shortFileName;
			this.txtScriptName.Enabled = false;
            this.scriptConfigCtrl1.ShowFull = false;
			this.Text = "Edit Sql Script Text";
			this.fullFilePath = fullFilePath;
            this.scriptCfgRow = scriptCfgRow;
			this.fileHash = fileHash;
            this.databaseList = databaseList;
            this.tagList = tagList;
            this.scriptTagRequired = scriptTagRequired;
            this.allowEdit = allowEdit;
            this.buildData = buildData;
		}
        public AddScriptTextForm(ref SqlSyncBuildData buildData, string shortFileName, string fullFilePath)
		{
			InitializeComponent();
            this.scriptConfigCtrl1.Visible = false;
			this.txtScriptName.Text = shortFileName;
			this.txtScriptName.Enabled = false;
			this.Text = "Import File Preview";
			this.fullFilePath = fullFilePath;
			this.cutCopyPastecontextMenuStrip1.Items.Clear();
			this.btnOK.Enabled = false;
            this.buildData = buildData;
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
            UrielGuy.SyntaxHighlighting.HighLightDescriptorCollection highLightDescriptorCollection1 = new UrielGuy.SyntaxHighlighting.HighLightDescriptorCollection();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddScriptTextForm));
            this.txtScriptName = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.btnOK = new System.Windows.Forms.Button();
            this.button2 = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.lblHash = new System.Windows.Forms.Label();
            this.panel1 = new System.Windows.Forms.Panel();
            this.txtScriptId = new System.Windows.Forms.TextBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.txtFileHash = new System.Windows.Forms.TextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblAddDate = new System.Windows.Forms.Label();
            this.label10 = new System.Windows.Forms.Label();
            this.label9 = new System.Windows.Forms.Label();
            this.lblModBy = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.lblAddedBy = new System.Windows.Forms.Label();
            this.label7 = new System.Windows.Forms.Label();
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.lblModDate = new System.Windows.Forms.Label();
            this.panel3 = new System.Windows.Forms.Panel();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.panel4 = new System.Windows.Forms.Panel();
            this.panel5 = new System.Windows.Forms.Panel();
            this.label5 = new System.Windows.Forms.Label();
            this.lnkRunPolicyChecks = new System.Windows.Forms.LinkLabel();
            this.lblCharacterNumber = new System.Windows.Forms.Label();
            this.lblHighlightLimit = new System.Windows.Forms.Label();
            this.lblLineNumber = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.rtbSqlScript = new UrielGuy.SyntaxHighlighting.SyntaxHighlightingTextBox();
            this.cutCopyPastecontextMenuStrip1 = new SqlSync.CutCopyPasteContextMenuStrip();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.codeReviewControl1 = new SqlSync.Controls.CodeReviewControl();
            this.finderCtrl1 = new SqlSync.FinderCtrl();
            this.scriptConfigCtrl1 = new SqlSync.SqlBuild.ScriptConfigCtrl();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.tableLayoutPanel1.SuspendLayout();
            this.panel3.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel5.SuspendLayout();
            this.cutCopyPastecontextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtScriptName
            // 
            this.txtScriptName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.txtScriptName.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.txtScriptName.Location = new System.Drawing.Point(5, 19);
            this.txtScriptName.Name = "txtScriptName";
            this.txtScriptName.Size = new System.Drawing.Size(1196, 21);
            this.txtScriptName.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(120, 16);
            this.label1.TabIndex = 2;
            this.label1.Text = "Script Name <F1>:";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(12, 116);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 16);
            this.label2.TabIndex = 3;
            this.label2.Text = "Sql Script:";
            // 
            // btnOK
            // 
            this.btnOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnOK.Location = new System.Drawing.Point(493, 7);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(75, 23);
            this.btnOK.TabIndex = 1;
            this.btnOK.Text = "Save";
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // button2
            // 
            this.button2.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button2.Location = new System.Drawing.Point(581, 7);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(75, 23);
            this.button2.TabIndex = 2;
            this.button2.Text = "Cancel";
            this.button2.Click += new System.EventHandler(this.button2_Click);
            // 
            // label4
            // 
            this.label4.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label4.Location = new System.Drawing.Point(3, 3);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(76, 14);
            this.label4.TabIndex = 4;
            this.label4.Text = "Script Id:";
            // 
            // lblHash
            // 
            this.lblHash.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHash.Location = new System.Drawing.Point(3, 6);
            this.lblHash.Name = "lblHash";
            this.lblHash.Size = new System.Drawing.Size(81, 16);
            this.lblHash.TabIndex = 5;
            this.lblHash.Text = "SHA1 Hash:";
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.White;
            this.panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel1.Controls.Add(this.txtScriptId);
            this.panel1.Controls.Add(this.label4);
            this.panel1.Controls.Add(this.panel2);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel1.Location = new System.Drawing.Point(0, 653);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1206, 24);
            this.panel1.TabIndex = 7;
            // 
            // txtScriptId
            // 
            this.txtScriptId.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtScriptId.Location = new System.Drawing.Point(79, 3);
            this.txtScriptId.Name = "txtScriptId";
            this.txtScriptId.Size = new System.Drawing.Size(344, 14);
            this.txtScriptId.TabIndex = 6;
            this.txtScriptId.TabStop = false;
            // 
            // panel2
            // 
            this.panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.panel2.Controls.Add(this.lblHash);
            this.panel2.Controls.Add(this.txtFileHash);
            this.panel2.Location = new System.Drawing.Point(429, -3);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(794, 33);
            this.panel2.TabIndex = 7;
            // 
            // txtFileHash
            // 
            this.txtFileHash.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.txtFileHash.Location = new System.Drawing.Point(84, 7);
            this.txtFileHash.Name = "txtFileHash";
            this.txtFileHash.Size = new System.Drawing.Size(354, 14);
            this.txtFileHash.TabIndex = 7;
            this.txtFileHash.TabStop = false;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Image = global::SqlSync.Properties.Resources.Help_2;
            this.pictureBox1.Location = new System.Drawing.Point(1185, 2);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(21, 16);
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            this.toolTip1.SetToolTip(this.pictureBox1, "Click for Help");
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // lblAddDate
            // 
            this.lblAddDate.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAddDate.Location = new System.Drawing.Point(262, 1);
            this.lblAddDate.Name = "lblAddDate";
            this.lblAddDate.Size = new System.Drawing.Size(165, 22);
            this.lblAddDate.TabIndex = 20;
            this.lblAddDate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label10
            // 
            this.label10.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label10.Location = new System.Drawing.Point(626, 1);
            this.label10.Name = "label10";
            this.label10.Size = new System.Drawing.Size(96, 22);
            this.label10.TabIndex = 19;
            this.label10.Text = "Last Mod Date:";
            this.label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label9
            // 
            this.label9.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label9.Location = new System.Drawing.Point(186, 1);
            this.label9.Name = "label9";
            this.label9.Size = new System.Drawing.Size(69, 22);
            this.label9.TabIndex = 18;
            this.label9.Text = "Add Date:";
            this.label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblModBy
            // 
            this.lblModBy.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblModBy.Location = new System.Drawing.Point(527, 1);
            this.lblModBy.Name = "lblModBy";
            this.lblModBy.Size = new System.Drawing.Size(92, 22);
            this.lblModBy.TabIndex = 17;
            this.lblModBy.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label6
            // 
            this.label6.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label6.Location = new System.Drawing.Point(434, 1);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(86, 22);
            this.label6.TabIndex = 16;
            this.label6.Text = "Last Mod By:";
            this.label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblAddedBy
            // 
            this.lblAddedBy.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lblAddedBy.Location = new System.Drawing.Point(77, 1);
            this.lblAddedBy.Name = "lblAddedBy";
            this.lblAddedBy.Size = new System.Drawing.Size(102, 22);
            this.lblAddedBy.TabIndex = 15;
            this.lblAddedBy.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label7
            // 
            this.label7.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label7.Location = new System.Drawing.Point(4, 1);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(66, 22);
            this.label7.TabIndex = 14;
            this.label7.Text = "Added By:";
            this.label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.BackColor = System.Drawing.Color.White;
            this.tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            this.tableLayoutPanel1.ColumnCount = 8;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 72F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 108F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 75F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 171F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 92F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 98F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 102F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 509F));
            this.tableLayoutPanel1.Controls.Add(this.lblModDate, 7, 0);
            this.tableLayoutPanel1.Controls.Add(this.label10, 6, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblAddDate, 3, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblModBy, 5, 0);
            this.tableLayoutPanel1.Controls.Add(this.lblAddedBy, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.label9, 2, 0);
            this.tableLayoutPanel1.Controls.Add(this.label6, 4, 0);
            this.tableLayoutPanel1.Controls.Add(this.label7, 0, 0);
            this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.tableLayoutPanel1.Location = new System.Drawing.Point(0, 629);
            this.tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(8, 3, 3, 3);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.RowCount = 1;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(1206, 24);
            this.tableLayoutPanel1.TabIndex = 18;
            // 
            // lblModDate
            // 
            this.lblModDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lblModDate.Location = new System.Drawing.Point(729, 1);
            this.lblModDate.Name = "lblModDate";
            this.lblModDate.Size = new System.Drawing.Size(145, 22);
            this.lblModDate.TabIndex = 21;
            this.lblModDate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel3
            // 
            this.panel3.AutoSize = true;
            this.panel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.panel3.Controls.Add(this.pictureBox1);
            this.panel3.Controls.Add(this.scriptConfigCtrl1);
            this.panel3.Controls.Add(this.txtScriptName);
            this.panel3.Controls.Add(this.label1);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(1206, 154);
            this.panel3.TabIndex = 19;
            // 
            // splitter1
            // 
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter1.Location = new System.Drawing.Point(0, 154);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(1206, 3);
            this.splitter1.TabIndex = 20;
            this.splitter1.TabStop = false;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.rtbSqlScript);
            this.panel4.Controls.Add(this.codeReviewControl1);
            this.panel4.Controls.Add(this.panel5);
            this.panel4.Controls.Add(this.finderCtrl1);
            this.panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel4.Location = new System.Drawing.Point(0, 157);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(1206, 472);
            this.panel4.TabIndex = 21;
            // 
            // panel5
            // 
            this.panel5.Controls.Add(this.label5);
            this.panel5.Controls.Add(this.btnOK);
            this.panel5.Controls.Add(this.lnkRunPolicyChecks);
            this.panel5.Controls.Add(this.button2);
            this.panel5.Controls.Add(this.lblCharacterNumber);
            this.panel5.Controls.Add(this.lblHighlightLimit);
            this.panel5.Controls.Add(this.lblLineNumber);
            this.panel5.Controls.Add(this.label3);
            this.panel5.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel5.Location = new System.Drawing.Point(0, 405);
            this.panel5.Name = "panel5";
            this.panel5.Size = new System.Drawing.Size(1206, 37);
            this.panel5.TabIndex = 10;
            // 
            // label5
            // 
            this.label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(1115, 12);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(69, 13);
            this.label5.TabIndex = 6;
            this.label5.Text = "Character:";
            // 
            // lnkRunPolicyChecks
            // 
            this.lnkRunPolicyChecks.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkRunPolicyChecks.AutoSize = true;
            this.lnkRunPolicyChecks.Location = new System.Drawing.Point(930, 12);
            this.lnkRunPolicyChecks.Name = "lnkRunPolicyChecks";
            this.lnkRunPolicyChecks.Size = new System.Drawing.Size(122, 13);
            this.lnkRunPolicyChecks.TabIndex = 9;
            this.lnkRunPolicyChecks.TabStop = true;
            this.lnkRunPolicyChecks.Text = "[Run Policy Checks]";
            this.lnkRunPolicyChecks.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkRunPolicyChecks_LinkClicked);
            // 
            // lblCharacterNumber
            // 
            this.lblCharacterNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblCharacterNumber.AutoSize = true;
            this.lblCharacterNumber.Location = new System.Drawing.Point(1180, 12);
            this.lblCharacterNumber.Name = "lblCharacterNumber";
            this.lblCharacterNumber.Size = new System.Drawing.Size(14, 13);
            this.lblCharacterNumber.TabIndex = 8;
            this.lblCharacterNumber.Text = "0";
            // 
            // lblHighlightLimit
            // 
            this.lblHighlightLimit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.lblHighlightLimit.AutoSize = true;
            this.lblHighlightLimit.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblHighlightLimit.ForeColor = System.Drawing.Color.Firebrick;
            this.lblHighlightLimit.Location = new System.Drawing.Point(4, 7);
            this.lblHighlightLimit.Name = "lblHighlightLimit";
            this.lblHighlightLimit.Size = new System.Drawing.Size(283, 24);
            this.lblHighlightLimit.TabIndex = 3;
            this.lblHighlightLimit.Text = "Due to the length of this script, SQL syntax highlighting\r\nhas been suspended to " +
    "improve performance.";
            this.lblHighlightLimit.Visible = false;
            // 
            // lblLineNumber
            // 
            this.lblLineNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.lblLineNumber.AutoSize = true;
            this.lblLineNumber.Location = new System.Drawing.Point(1090, 12);
            this.lblLineNumber.Name = "lblLineNumber";
            this.lblLineNumber.Size = new System.Drawing.Size(14, 13);
            this.lblLineNumber.TabIndex = 7;
            this.lblLineNumber.Text = "0";
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(1058, 12);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(35, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Line:";
            // 
            // rtbSqlScript
            // 
            this.rtbSqlScript.AcceptsTab = true;
            this.rtbSqlScript.CaseSensitive = false;
            this.rtbSqlScript.ContextMenuStrip = this.cutCopyPastecontextMenuStrip1;
            this.rtbSqlScript.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbSqlScript.FilterAutoComplete = true;
            this.rtbSqlScript.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbSqlScript.HighlightDescriptors = highLightDescriptorCollection1;
            this.rtbSqlScript.HighlightType = SqlSync.Highlighting.SyntaxHightlightType.Sql;
            this.rtbSqlScript.Location = new System.Drawing.Point(0, 0);
            this.rtbSqlScript.MaxUndoRedoSteps = 50;
            this.rtbSqlScript.Name = "rtbSqlScript";
            this.rtbSqlScript.Size = new System.Drawing.Size(1206, 334);
            this.rtbSqlScript.SuspendHighlighting = false;
            this.rtbSqlScript.TabIndex = 0;
            this.rtbSqlScript.Text = "";
            this.rtbSqlScript.WordWrap = false;
            this.rtbSqlScript.SelectionChanged += new System.EventHandler(this.rtbSqlScript_SelectionChanged);
            this.rtbSqlScript.TextChanged += new System.EventHandler(this.rtbSqlScript_TextChanged);
            this.rtbSqlScript.KeyUp += new System.Windows.Forms.KeyEventHandler(this.rtbSqlScript_KeyUp);
            this.rtbSqlScript.MouseUp += new System.Windows.Forms.MouseEventHandler(this.rtbSqlScript_MouseUp);
            // 
            // cutCopyPastecontextMenuStrip1
            // 
            this.cutCopyPastecontextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.toolStripSeparator1});
            this.cutCopyPastecontextMenuStrip1.Name = "mnuCopyPaste";
            this.cutCopyPastecontextMenuStrip1.Size = new System.Drawing.Size(61, 10);
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(57, 6);
            // 
            // codeReviewControl1
            // 
            this.codeReviewControl1.BackColor = System.Drawing.SystemColors.Control;
            this.codeReviewControl1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.codeReviewControl1.HasChanges = false;
            this.codeReviewControl1.Location = new System.Drawing.Point(0, 334);
            this.codeReviewControl1.Margin = new System.Windows.Forms.Padding(2);
            this.codeReviewControl1.Name = "codeReviewControl1";
            this.codeReviewControl1.Size = new System.Drawing.Size(1206, 71);
            this.codeReviewControl1.TabIndex = 11;
            // 
            // finderCtrl1
            // 
            this.finderCtrl1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.finderCtrl1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.finderCtrl1.Location = new System.Drawing.Point(0, 442);
            this.finderCtrl1.Name = "finderCtrl1";
            this.finderCtrl1.Size = new System.Drawing.Size(1206, 30);
            this.finderCtrl1.TabIndex = 4;
            // 
            // scriptConfigCtrl1
            // 
            this.scriptConfigCtrl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.scriptConfigCtrl1.BackColor = System.Drawing.SystemColors.Control;
            this.scriptConfigCtrl1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.scriptConfigCtrl1.BuildSequenceChanged = false;
            this.scriptConfigCtrl1.DatabaseList = null;
            this.scriptConfigCtrl1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.scriptConfigCtrl1.HasChanged = false;
            this.scriptConfigCtrl1.Location = new System.Drawing.Point(5, 43);
            this.scriptConfigCtrl1.Margin = new System.Windows.Forms.Padding(0);
            this.scriptConfigCtrl1.Name = "scriptConfigCtrl1";
            this.scriptConfigCtrl1.ShowFull = false;
            this.scriptConfigCtrl1.Size = new System.Drawing.Size(1207, 111);
            this.scriptConfigCtrl1.TabIndex = 1;
            // 
            // AddScriptTextForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            this.AutoSize = true;
            this.CancelButton = this.button2;
            this.ClientSize = new System.Drawing.Size(1206, 677);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.label2);
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "AddScriptTextForm";
            this.Text = "Add SQL Script Text ";
            this.Load += new System.EventHandler(this.AddScriptTextForm_Load);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.AddScriptTextForm_KeyDown);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.tableLayoutPanel1.ResumeLayout(false);
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel5.ResumeLayout(false);
            this.panel5.PerformLayout();
            this.cutCopyPastecontextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

        private void btnOK_Click(object sender, System.EventArgs e)
        {
            if (this.txtScriptName.Text.Length == 0)
            {
                MessageBox.Show("Please enter a script name", "Script Name Needed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.txtScriptName.Focus();
                return;
            }

            if (this.rtbSqlScript.Text.Length == 0)
            {
                MessageBox.Show("Please enter a SQL script", "Script Needed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                this.rtbSqlScript.Focus();
                return;
            }

            if (!this.scriptConfigCtrl1.ValidateValues(rtbSqlScript.Text, txtScriptName.Text))
                return;

            if (DialogResult.Yes == RunPolicyChecks(true))
                return;


            List<TableWatch> watchAlerts;
            if (!TableWatchNotification.CheckForTableWatch(rtbSqlScript.Text, out watchAlerts))
            {
                if (watchAlerts != null)
                {
                    Notification.NotificationForm frmNot = new SqlSync.SqlBuild.Notification.NotificationForm(watchAlerts);
                    frmNot.ShowDialog();
                }
            }


            if (this.scriptCfgRow.FileName.EndsWith(DbObjectType.StoredProcedure, StringComparison.CurrentCultureIgnoreCase) ||
                this.scriptCfgRow.FileName.EndsWith(DbObjectType.UserDefinedFunction, StringComparison.CurrentCultureIgnoreCase) ||
                this.scriptCfgRow.FileName.EndsWith(DbObjectType.Trigger, StringComparison.CurrentCultureIgnoreCase))
                this.scriptCfgRow.StripTransactionText = false;


            Regex useStmt = new Regex("^\\s*use\\s+\\S+", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (useStmt.Match(this.rtbSqlScript.Text).Success)
            {
                string msg = "A database \"USE\" statement was found in your SQL:\r\n\r\n" + useStmt.Match(this.rtbSqlScript.Text).Value.Trim() + "\r\n\r\n" +
                "This statement will be stripped out of the file!\r\nTo change your target database, use the \"Target DB\" list on the script form.\r\n\r\n" +
                "Click \"OK\" to save the script, minus the \"USE\" statement.\r\nClick \"Cancel\" to edit the script.";
                if (DialogResult.Cancel == MessageBox.Show(msg, "USE Statement detected", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning))
                {
                    return;
                }
                else
                {
                    this.rtbSqlScript.Text = this.rtbSqlScript.Text.Replace(useStmt.Match(this.rtbSqlScript.Text).Value, string.Empty);
                }
            }


            this.codeReviewControl1.SaveData(this.rtbSqlScript.Text);
            

            this.scriptConfigCtrl1.UpdateScriptConfigValues();
            this.sqlName = txtScriptName.Text;
            this.sqlText = rtbSqlScript.Text;
            this.DialogResult = DialogResult.OK;
            this.configurationChanged = this.scriptConfigCtrl1.HasChanged;
            this.buildSequenceChanged = this.scriptConfigCtrl1.BuildSequenceChanged;

            if (this.codeReviewControl1.HasChanges)
                this.configurationChanged = true;

            this.Close();
        }

		private void button2_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void AddScriptTextForm_Load(object sender, System.EventArgs e)
		{
            if (this.txtScriptName.Text.Length > 0)
                this.scriptConfigCtrl1.ParentFileName = this.txtScriptName.Text;

            this.txtScriptName.TextChanged  += new EventHandler(txtScriptName_TextChanged);
            this.finderCtrl1.AddControlToSearch(this.rtbSqlScript);
			this.rtbSqlScript.FilterAutoComplete = true;
            txtScriptId.Text = this.scriptCfgRow.ScriptId;
			txtFileHash.Text = this.fileHash;
            lblAddedBy.Text = this.scriptCfgRow.AddedBy;
            lblModBy.Text = this.scriptCfgRow.ModifiedBy;
            lblAddDate.Text = (this.scriptCfgRow.IsDateAddedNull()) ? "" : this.scriptCfgRow.DateAdded.ToString();
            lblModDate.Text = (this.scriptCfgRow.IsDateModifiedNull() || this.scriptCfgRow.DateModified == DateTime.MinValue) ? "" : this.scriptCfgRow.DateModified.ToString();

            if (this.scriptConfigCtrl1.Visible)
                this.scriptConfigCtrl1.SetConfigData(ref this.scriptCfgRow, this.databaseList,this.tagList,this.scriptTagRequired);

			if(this.fullFilePath.Length > 0)
			{
				string[] batch = SqlBuildHelper.ReadBatchFromScriptFile(this.fullFilePath,false,true);
				string full = String.Join("",batch);
				this.rtbSqlScript.AppendText(full);
				this.rtbSqlScript.Select(0,0);
			}
            this.rtbSqlScript.AcceptsTab = true;
    
			if(this.utilRegistry != null)
			{
                List<ToolStripItem> list = new List<ToolStripItem>();
                for (int i = 0; i < this.utilRegistry.Items.Length; i++)
                    list.Add(SetUtilityRegistry(this.utilRegistry.Items[i]));
					
				if(list.Count > 0)
					for(int i=0;i<list.Count;i++)
                        this.cutCopyPastecontextMenuStrip1.Items.Add(list[i]);
    		}

            ToolStripMenuItem mnuOptimizeWithNoLock = new ToolStripMenuItem("Optimize SELECT : Add \"WITH (NOLOCK)\" Directive", null, mnuOptimizeWithNoLock_Click);
            this.cutCopyPastecontextMenuStrip1.Items.Add(new ToolStripSeparator());
            this.cutCopyPastecontextMenuStrip1.Items.Add(mnuOptimizeWithNoLock);

            ToolStripMenuItem mnuConvertToAlter = new ToolStripMenuItem("Convert to ALTER COLUMN", null, mnuConvertToAlter_Click);
            this.cutCopyPastecontextMenuStrip1.Items.Add(new ToolStripSeparator());
            this.cutCopyPastecontextMenuStrip1.Items.Add(mnuConvertToAlter);

            ToolStripMenuItem mnuResyncTable= new ToolStripMenuItem("Transform to resync TABLE", null, mnuTransformtoResyncTable_Click);
            this.cutCopyPastecontextMenuStrip1.Items.Add(mnuResyncTable);


			this.toolTip1.SetToolTip(label1,"Click <F1> to insert Selected Sql Script text as Script Name\r\nClick <Shift><F1> to append to Script Name");

            if (this.scriptConfigCtrl1.ShowFull == false)
                rtbSqlScript.Select();

            if (!this.allowEdit)
                btnOK.Enabled = false;

            if (EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig == null || !EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig.Enabled)
            {
                this.codeReviewControl1.Height = 0;
                this.codeReviewControl1.Visible = false;
            }
            else
            {
                //Add code review items
                //get the last editor
                string lastEditor = this.scriptCfgRow.ModifiedBy.Length == 0 ? this.scriptCfgRow.AddedBy : this.scriptCfgRow.ModifiedBy;
                this.codeReviewControl1.BindData(ref this.buildData, ref this.scriptCfgRow, this.rtbSqlScript.Text, lastEditor);
            }
		}

        void txtScriptName_TextChanged(object sender, EventArgs e)
        {
            this.scriptConfigCtrl1.ParentFileName = txtScriptName.Text;
        }

        private ToolStripItem  SetUtilityRegistry(object utilityRegItem)
		{
			if(utilityRegItem.GetType() == typeof(Utility.Replace))
			{
				Utility.Replace replace = (Utility.Replace)utilityRegItem;
				string text = replace.OldString +"-->"+replace.NewString;
				return new ToolStripMenuItem(text,null,new System.EventHandler(mnuReplaceandHighlight_Click));
			}
			else if(utilityRegItem.GetType() == typeof(Utility.SubMenu))
			{
				Utility.SubMenu sub = (Utility.SubMenu)utilityRegItem;
                ToolStripItem item;
                if (sub.Name == "-")
                    item = new ToolStripSeparator();
                else
                    item = new ToolStripMenuItem(sub.Name);

				if(sub.Items == null) return item;

                for (int i = 0; i < sub.Items.Length; i++)
                {
                    ((ToolStripMenuItem)item).DropDownItems.Add(SetUtilityRegistry(sub.Items[i]));
                }
				
				return item;
			}
			else if(utilityRegItem.GetType() == typeof(Utility.UtilityQuery))
			{
				Utility.UtilityQuery util = (Utility.UtilityQuery)utilityRegItem;
                return new ToolStripMenuItem(util.Description, null, new System.EventHandler(mnuUtilityQueryInsert_Click));
			}
            return new ToolStripSeparator();
        }

		private void lnkProcessBatch_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			int batchNumber = 0;
			string[] lines = this.rtbSqlScript.Lines;
			this.rtbSqlScript.Clear();
			for(int i=0;i<lines.Length;i++)
			{
				if(batchNumber % 2 == 0)
					this.rtbSqlScript.SelectionColor = this.evenColor;
				else
					this.rtbSqlScript.SelectionColor = this.oddColor;
				
				this.rtbSqlScript.AppendText(lines[i]+"\r\n");
				if(lines[i].Trim() == BatchParsing.Delimiter)
					batchNumber++;
			}

		}

		private void mnuReplaceandHighlight_Click(object sender, System.EventArgs e)
		{
            string text = ((ToolStripItem)sender).Text;
			int arrowLoc = text.IndexOf("-->");
			string old = text.Substring(0,arrowLoc);
			string newStr = text.Substring(arrowLoc+3);
			ReplaceAndHighlightString(old,newStr);
		}

		private void mnuUtilityQueryInsert_Click(object sender, System.EventArgs e)
		{
			string text = ((ToolStripItem)sender).Text;
			if(this.utilRegistry != null)
			{
				string fileName = string.Empty;
				for(int i=0;i<this.utilRegistry.Items.Length;i++)
				{
					fileName = GetUtilityQueryFileName(text, this.utilRegistry.Items[i]);
					if(fileName != string.Empty)
						break;
				}
				if(fileName != null && fileName != string.Empty)
					InsertUtilityQuery(fileName, text);

			}
		}
		private string GetUtilityQueryFileName(string menuText,object registryItem)
		{
			if(registryItem.GetType() ==  typeof(Utility.UtilityQuery) &&
				((Utility.UtilityQuery)registryItem).Description == menuText)
			{
				return ((Utility.UtilityQuery)registryItem).FileName;
			}
			else if(registryItem.GetType() ==  typeof(Utility.SubMenu))
			{
				string fName = string.Empty;
				Utility.SubMenu sub = (Utility.SubMenu)registryItem;
				if(sub.Items == null) return fName;

				for(int i=0;i<sub.Items.Length;i++)
				{
					fName = GetUtilityQueryFileName(menuText,sub.Items[i]);
					if(fName != string.Empty)
						return fName;
				}
			}
			return string.Empty;

		}
		private void InsertUtilityQuery(string fileLocation, string title)
		{
			if(System.IO.File.Exists(fileLocation) == false)
			{
				MessageBox.Show("Unable to locate utility file at:\r\n"+fileLocation);
				return;
			}

			string query = string.Empty;
			using(System.IO.StreamReader sr = System.IO.File.OpenText(fileLocation))
			{
				query = sr.ReadToEnd();
			}

            //automatic inserts...
            Regex regToday = new Regex(@"<\[today\]>",RegexOptions.IgnoreCase);
            query = regToday.Replace(query, DateTime.Now.ToString("MM/dd/yyyy"));

            Regex regScriptName = new Regex(@"<\[script_name\]>",RegexOptions.IgnoreCase);
            query = regScriptName.Replace(query, this.txtScriptName.Text);

            Regex regAuthor = new Regex(@"<\[author\]>",RegexOptions.IgnoreCase);
            query = regAuthor.Replace(query, System.Environment.UserName);


            //manual inserts
			Regex check = new Regex("<<[A-Za-z0-9 ]{1,}>>");
			MatchCollection matches = check.Matches(query);
			if(matches.Count > 0)
			{
				ArrayList keys = new ArrayList();
				for(int i=0;i<matches.Count;i++)
				{
					if(keys.Contains(matches[i].Value) == false)
						keys.Add(matches[i].Value);
				}
				string[] arrKey = new string[keys.Count];
				keys.CopyTo(arrKey);
                string inputText = string.Empty;
                if (this.rtbSqlScript.SelectedText.Length > 0)
                    inputText = this.rtbSqlScript.SelectedText;

				UtilityReplacement frmRepl = new UtilityReplacement(arrKey,title,inputText);
				frmRepl.ShowDialog();

				if(frmRepl.DialogResult == DialogResult.OK)
				{
					for(int i=0;i<arrKey.Length;i++)
						query = query.Replace(arrKey[i],frmRepl.Replacements[arrKey[i]]);
				}else
					query = string.Empty;
			}

            if (query.Length == 0)
                return;

			//Add the default title if applicable
			Regex regTitle = new Regex(@"\$\{[Tt][Ii][Tt][Ll][Ee]: (.*?)\}");
			MatchCollection queryTitle = regTitle.Matches(query);
            Regex regRemoveTitle = new Regex(@"\$\{[Tt][Ii][Tt][Ll][Ee]:");
            MatchCollection remTitle = regRemoveTitle.Matches(query);

            if(this.txtScriptName.Text.Length == 0)
			{
				if(queryTitle.Count > 0)
				{
					this.txtScriptName.Text = queryTitle[0].Value.Replace(remTitle[0].Value,"").Replace("}","").Trim();
					query = query.Replace(queryTitle[0].Value,"").TrimStart();
				}
			}
			else
			{
				for(int i=0;i<queryTitle.Count;i++)
                    query = query.Replace(queryTitle[0].Value, "").TrimStart();
			}

			int selectionStart = this.rtbSqlScript.SelectionStart;
			int trueLength = query.Replace("\n","").Length;
			this.rtbSqlScript.SelectedText = query;
			this.rtbSqlScript.Select(selectionStart,trueLength);
			this.rtbSqlScript.SelectionColor = Color.Orange;
			this.rtbSqlScript.Select(selectionStart+query.Length,0);

		}
		private void ReplaceAndHighlightString(string oldString, string newString)
		{
			int selectionStart = this.rtbSqlScript.SelectionStart;
			string replaceVal = this.rtbSqlScript.SelectedText.Replace(oldString,newString);
			replaceVal = replaceVal.Replace(oldString.ToLower(),newString);
			this.rtbSqlScript.SelectedText = replaceVal;

			bool foundNewString = true;
			int findIndex = 0;
			int newStringLength = newString.Length;
			while(foundNewString)
			{
				int start = replaceVal.IndexOf(newString,findIndex);
				if(start > -1)
				{
					this.rtbSqlScript.Select(selectionStart+start,newStringLength);
					this.rtbSqlScript.SelectionColor = Color.Purple;
					findIndex = start+1;
				}
				else
				{
					foundNewString = false;
				}
			}
			this.rtbSqlScript.Select(selectionStart+replaceVal.Length,0);
		}

		
		/// <summary>
		/// Captures a paste function and looks for RegEx (TABLE [A-Za-z0-9\[\] _]{1,}) 
		/// to paste in a default script name
		/// </summary>
		/// <param name="sender"></param>
		/// <param name="e"></param>
		private void rtbSqlScript_KeyUp(object sender, System.Windows.Forms.KeyEventArgs e)
		{
            SetLineAndCharacterNumber();
			if(e.KeyCode == Keys.V && e.Modifiers == Keys.Control && txtScriptName.Text.Length == 0)
			{
				Regex table = new Regex(@"TABLE [A-Za-z0-9\[\] _]{1,}");
				Match found = table.Match(this.rtbSqlScript.Text,0);
				if(found != null && found.Value.Length > 0)
				{
					txtScriptName.Text = found.Value.Replace("TABLE [","").Replace("]","");
				}
			}

            SearchForAutoScriptingPattern();
		}


        private void mnuOptimizeWithNoLock_Click(object sender, EventArgs e)
        {
            string rawScript;
            if (rtbSqlScript.SelectedText.Length > 0)
                rawScript = rtbSqlScript.SelectedText;
            else
                rawScript = rtbSqlScript.Text;

            string processed = ScriptOptimization.ProcessNoLockOptimization(rawScript);
            if (rtbSqlScript.SelectedText.Length > 0)
                rtbSqlScript.SelectedText = processed;
            else
                rtbSqlScript.Text = processed;
        }
		private void mnuConvertToAlter_Click(object sender, EventArgs e)
		{
            string changedScript;
            string rawScript = rtbSqlScript.SelectedText;
            string tableName, schema;
            ScriptWrapping.ExtractTableNameFromScript(rawScript, out schema, out tableName);
            if (tableName == string.Empty)
            {
                GetTableNameForm frmName = new GetTableNameForm();
                if (DialogResult.OK == frmName.ShowDialog())
                {
                    tableName = frmName.TableName;
                }
                else
                {
                    return;
                }
            }
            ScriptWrapping.TransformCreateTableToAlterColumn(rawScript,schema, tableName,out changedScript);
            rtbSqlScript.SelectedText = changedScript;

		}
        private void mnuTransformtoResyncTable_Click(object sender, EventArgs e)
		{
            if (DialogResult.Yes == MessageBox.Show("This will generate a script to remove all columns not referenced in the highlighted text.\r\nAre you sure you want to continue?", "Are you sure?", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                string rawScript = rtbSqlScript.SelectedText;
                string tableName, schema;
                ScriptWrapping.ExtractTableNameFromScript(rawScript, out schema, out tableName);
                if (tableName.Length == 0)
                {
                    MessageBox.Show("Unable to determine table name, looked for \"CREATE TABLE\" in Regex", "Unable to Process", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                else
                    rtbSqlScript.SelectedText =  ScriptWrapping.TransformCreateTableToResyncTable(rawScript, schema, tableName);
            }
		}

		private void AddScriptTextForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
            
            if (this.rtbSqlScript.SelectedText == string.Empty)
                return;

            switch(e.KeyCode)
            {
                case Keys.F1:
    		        char[] nonPrintable = new char[]{'\r','\n','\t'};
			        string selection = this.rtbSqlScript.SelectedText.Trim();
			        if(selection.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) > -1 ||
				        selection.IndexOfAny(nonPrintable) > -1)
			        {
				        MessageBox.Show("Selection contains invalid file name characters. Paste Failed","Invalid Characters Found",MessageBoxButtons.OK,MessageBoxIcon.Error);
				        return;
			        }

			        if(e.Modifiers == Keys.Shift)
				        this.txtScriptName.Text += " "+this.rtbSqlScript.SelectedText.Trim();
			        else
				        this.txtScriptName.Text = this.rtbSqlScript.SelectedText.Trim();
                    break;
                case Keys.F12:
                    string val = ProcessAutomaticProcessing(this.rtbSqlScript.SelectedText.Trim());
                    if (val.Length > 0)
                        this.rtbSqlScript.SelectedText = val;
                    break;
			}
        }

        #region .: Automated Processing of Scripts :.

        
        /// <summary>
        /// Regex for CREATE <any> INDEX
        /// </summary>
        private static Regex findCreateIndex = new Regex(@"\bCREATE\b.*\bINDEX\b", RegexOptions.IgnoreCase);
        /// <summary>
        /// Regex for ALTER TABLE <any> ALTER COLUMN
        /// </summary>
        private static Regex findAlterTableAlterColumn = new Regex(@"\bALTER\b.*TABLE\b.*\s*\bALTER\b \bCOLUMN\b", RegexOptions.IgnoreCase);
        /// <summary>
        /// Regex for ALTER TABLE <any> ADD
        /// </summary>
        private static Regex findAlterTableAdd = new Regex(@"\bALTER\b.*\bTABLE .*\s*ADD", RegexOptions.IgnoreCase);
        private void SearchForAutoScriptingPattern()
        {
            try{

                if (rtbSqlScript.SelectedText.Length == 0)
                {
                    toolTip1.SetToolTip(rtbSqlScript, "");
                    return;
                }

                string[] lines = rtbSqlScript.SelectedText.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
                string rawScript = rtbSqlScript.SelectedText;
                //Regex for CREATE <any> INDEX
                if (findCreateIndex.Match(lines[0]).Success)
                {
                    toolTip1.SetToolTip(rtbSqlScript, "Click <F12> to wrap CREATE INDEX script with re-runable wrapper");
                    return;
                }

                ////Regex for ALTER TABLE <any> ALTER COLUMN
                if (findAlterTableAlterColumn.Match(rawScript).Success)
                {
                    toolTip1.SetToolTip(rtbSqlScript, "Click <F12> to wrap ALTER COLUMN script with re-runable wrapper");
                    return;
                }

                //Regex for ALTER TABLE <any> ADD
                if (findAlterTableAdd.Match(rawScript).Success)
                {
                    toolTip1.SetToolTip(rtbSqlScript, "Click <F12> to wrap ADD COLUMN script with re-runable wrapper");
                    return;
                }
            }
            catch{}
        }
        /// <summary>
        /// Detect if there is a matching regex available...
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void rtbSqlScript_MouseUp(object sender, MouseEventArgs e)
        {
            SearchForAutoScriptingPattern();
            SetLineAndCharacterNumber();
        }





        private string ProcessAutomaticProcessing(string rawScript)
        {
            string[] lines = rawScript.Trim().Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            if (lines.Length > 0)
            {
                //Regex for CREATE <any> INDEX
                if (findCreateIndex.Match(lines[0]).Success)
                    return FormatIndexHandling(lines[0]);

                ////Regex for ALTER TABLE <any> ALTER COLUMN
                if (findAlterTableAlterColumn.Match(rawScript).Success)
                    return FormatAlterColumnHandling(rawScript);

                //Regex for ALTER TABLE <any> ADD
                if (findAlterTableAdd.Match(rawScript).Success) 
                    return FormatAddColumnHandling(rawScript);
            }
            return string.Empty;
        }
        private string FormatIndexHandling(string rawScript)
        {
            try
            {
                /*Sample Input:
                 * CREATE  CLUSTERED  INDEX [IX_CreditRequest_EntityID] ON [CreditRequest]([CompanyMasterAcctEntityID]) ON [PRIMARY]
                 */

                //Regex for Index and Table Name. Will return: [IX_CreditRequest_EntityID], [CreditRequest], [CompanyMasterAcctEntityID],[PRIMARY]
                // - this first value will be the index name and the second the table
                Regex findCreateIndex = new Regex(@"[[A-Za-z0-9_]{1,}]", RegexOptions.IgnoreCase);
                MatchCollection matches = findCreateIndex.Matches(rawScript);
                string indexName = string.Empty;
                string tableName = string.Empty;
                if (matches.Count >= 2)
                {
                    indexName = matches[0].Value.Replace("[", "").Replace("]", "");
                    tableName = matches[1].Value.Replace("[", "").Replace("]", "");
                }
                if (tableName.Length > 0 && indexName.Length > 0)
                {
                    string template = "IF NOT  EXISTS (SELECT 1 FROM sysindexes WHERE name = '<<Index Name>>'  AND OBJECT_NAME(id) = N'<<Table Name>>')\r\n\t<<Script>>\r\n";
                    template = template.Replace("<<Index Name>>", indexName);
                    template = template.Replace("<<Table Name>>", tableName);
                    template = template.Replace("<<Script>>", rawScript);

                    return template;
                }
                else
                    return string.Empty;
            }
            finally
            {
                toolTip1.SetToolTip(rtbSqlScript, "");
            }
        }
        private string FormatAlterColumnHandling(string rawScript)
        {

            try
            {
                Regex findAlterTable = new Regex(@"\bALTER\b.*\bTABLE", RegexOptions.IgnoreCase);
                Regex findAlterColumn = new Regex(@"\bALTER\b \bCOLUMN\b", RegexOptions.IgnoreCase);
                Regex findColumnName = new Regex(@"\bALTER\b \bCOLUMN\b .*?\s", RegexOptions.IgnoreCase);
                string tableName = string.Empty;
                string columnName = string.Empty;

                if (findAlterTableAlterColumn.Match(rawScript).Success)
                    tableName = findAlterTableAlterColumn.Match(rawScript).Value.
                        Replace(findAlterTable.Match(rawScript).Value, "").
                        Replace(findAlterColumn.Match(rawScript).Value, "").
                        Replace("[", "").Replace("]", "").Trim();

                if (findColumnName.Match(rawScript).Success)
                    columnName = findColumnName.Match(rawScript).Value.
                        Replace(findAlterColumn.Match(rawScript).Value, "").
                        Replace("[", "").Replace("]", "").Trim();

                if (tableName.Length > 0 && columnName.Length > 0)
                {
                    string template = "IF EXISTS(SELECT 1 FROM information_schema.columns WHERE TABLE_NAME = '<<Table Name>>' AND COLUMN_NAME = '<<Column Name>>')\r\nBEGIN\r\n\t<<Script>>\r\nEND\r\n";
                    template = template.Replace("<<Table Name>>", tableName);
                    template = template.Replace("<<Column Name>>", columnName);
                    template = template.Replace("<<Script>>", rawScript);
                    return template;
                }
                else
                    return string.Empty;
            }
            finally
            {
                toolTip1.SetToolTip(rtbSqlScript, "");
            }
        }
        private string FormatAddColumnHandling(string rawScript)
        {
            try
            {
                Regex findAlterTable = new Regex(@"\bALTER\b.*\bTABLE", RegexOptions.IgnoreCase);
                Regex findAdd = new Regex(@"\bADD\b", RegexOptions.IgnoreCase);
                Regex findColumnName = new Regex(@"\bADD\b .*?\s", RegexOptions.IgnoreCase);
                string tableName = string.Empty;
                string columnName = string.Empty;

                if (findAlterTableAdd.Match(rawScript).Success)
                    tableName = findAlterTableAdd.Match(rawScript).Value.
                        Replace(findAlterTable.Match(rawScript).Value, "").
                        Replace(findAdd.Match(rawScript).Value, "").
                        Replace("[", "").Replace("]", "").Trim();

                if (findColumnName.Match(rawScript).Success)
                    columnName = findColumnName.Match(rawScript).Value.
                        Replace(findAdd.Match(rawScript).Value, "").
                        Replace("[", "").Replace("]", "").Trim();

                if (tableName.Length > 0 && columnName.Length > 0)
                {
                    string template = "IF NOT EXISTS(SELECT 1 FROM information_schema.columns WHERE TABLE_NAME = '<<Table Name>>' AND COLUMN_NAME = '<<Column Name>>')\r\nBEGIN\r\n\t<<Script>>\r\nEND\r\n";
                    template = template.Replace("<<Table Name>>", tableName);
                    template = template.Replace("<<Column Name>>", columnName);
                    template = template.Replace("<<Script>>", rawScript);
                    return template;
                }
                else
                    return string.Empty;
            }
            finally
            {
                toolTip1.SetToolTip(rtbSqlScript, "");
            }
        }

        #endregion

        private void SetLineAndCharacterNumber()
        {
            int lineStartIndex = this.rtbSqlScript.GetFirstCharIndexOfCurrentLine();
            int lineNumber = this.rtbSqlScript.GetLineFromCharIndex(lineStartIndex)+1;
            int absoluteCharNumber = this.rtbSqlScript.SelectionStart;
            int localCharNumber = absoluteCharNumber - lineStartIndex+1;

            lblLineNumber.Text = lineNumber.ToString();
            lblCharacterNumber.Text = localCharNumber.ToString();
        }
        private void rtbSqlScript_TextChanged(object sender, EventArgs e)
        {
            //If the script is too long, the active SQL highlighting is a huge performance hit.
            //Turn it off instead.
            if (this.rtbSqlScript.Text.Length > 50000)
            {
                this.rtbSqlScript.HighlightType = SqlSync.Highlighting.SyntaxHightlightType.None;
                this.lblHighlightLimit.Visible = true;
            }
        }

        private void rtbSqlScript_SelectionChanged(object sender, EventArgs e)
        {
            SetLineAndCharacterNumber();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            SqlSync.SqlBuild.UtilityHelper.OpenManual("AdvancedScriptHandling");
        }

        private void lnkRunPolicyChecks_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            RunPolicyChecks(false);
        }

        private DialogResult RunPolicyChecks(bool includeSaveButtons)
        {
            PolicyHelper policyHelp = new PolicyHelper();
            Script violations = policyHelp.ValidateScriptAgainstPolicies(rtbSqlScript.Text, scriptConfigCtrl1.SelectedDatabase);
            if (violations != null && violations.Count > 0)
            {
                violations.ScriptName = this.txtScriptName.Text;
                return new Policy.PolicyViolationForm(violations, includeSaveButtons).ShowDialog();

            }
            else
            {
                if (!includeSaveButtons)
                    MessageBox.Show("No policy violations found", "Looking Good!", MessageBoxButtons.OK);
      
                return DialogResult.No;
            }
        }

       
    }
}
