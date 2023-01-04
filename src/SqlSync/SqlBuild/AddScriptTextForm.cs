using SqlBuildManager.Enterprise;
using SqlBuildManager.Enterprise.Notification;
using SqlBuildManager.Enterprise.Policy;
using SqlBuildManager.ScriptHandling;
using SqlSync.Constants;
using SqlSync.DbInformation;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
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
        //private Controls.CodeReviewControl codeReviewControl1;
        private SqlSyncBuildData buildData;
        public string SqlText
        {
            get
            {
                return sqlText;
            }
        }
        public string SqlName
        {
            get
            {
                return sqlName;
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
        public AddScriptTextForm(ref SqlSyncBuildData buildData, ref SqlSyncBuildData.ScriptRow scriptCfgRow, SqlBuild.Utility.SqlSyncUtilityRegistry utilRegistry, DatabaseList databaseList, List<string> tagList, bool scriptTagRequired)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            this.utilRegistry = utilRegistry;
            scriptConfigCtrl1.ShowFull = true;
            this.scriptCfgRow = scriptCfgRow;
            this.databaseList = databaseList;
            scriptConfigCtrl1.ShowHighlightColor();
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
            txtScriptName.Text = shortFileName;
            txtScriptName.Enabled = false;
            scriptConfigCtrl1.ShowFull = false;
            Text = "Edit Sql Script Text";
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
            scriptConfigCtrl1.Visible = false;
            txtScriptName.Text = shortFileName;
            txtScriptName.Enabled = false;
            Text = "Import File Preview";
            this.fullFilePath = fullFilePath;
            cutCopyPastecontextMenuStrip1.Items.Clear();
            btnOK.Enabled = false;
            this.buildData = buildData;
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
            UrielGuy.SyntaxHighlighting.HighLightDescriptorCollection highLightDescriptorCollection1 = new UrielGuy.SyntaxHighlighting.HighLightDescriptorCollection();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AddScriptTextForm));
            txtScriptName = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            btnOK = new System.Windows.Forms.Button();
            button2 = new System.Windows.Forms.Button();
            label4 = new System.Windows.Forms.Label();
            lblHash = new System.Windows.Forms.Label();
            panel1 = new System.Windows.Forms.Panel();
            txtScriptId = new System.Windows.Forms.TextBox();
            panel2 = new System.Windows.Forms.Panel();
            txtFileHash = new System.Windows.Forms.TextBox();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            pictureBox1 = new System.Windows.Forms.PictureBox();
            lblAddDate = new System.Windows.Forms.Label();
            label10 = new System.Windows.Forms.Label();
            label9 = new System.Windows.Forms.Label();
            lblModBy = new System.Windows.Forms.Label();
            label6 = new System.Windows.Forms.Label();
            lblAddedBy = new System.Windows.Forms.Label();
            label7 = new System.Windows.Forms.Label();
            tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            lblModDate = new System.Windows.Forms.Label();
            panel3 = new System.Windows.Forms.Panel();
            scriptConfigCtrl1 = new SqlSync.SqlBuild.ScriptConfigCtrl();
            splitter1 = new System.Windows.Forms.Splitter();
            panel4 = new System.Windows.Forms.Panel();
            rtbSqlScript = new UrielGuy.SyntaxHighlighting.SyntaxHighlightingTextBox();
            cutCopyPastecontextMenuStrip1 = new SqlSync.CutCopyPasteContextMenuStrip();
            toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            panel5 = new System.Windows.Forms.Panel();
            label5 = new System.Windows.Forms.Label();
            lnkRunPolicyChecks = new System.Windows.Forms.LinkLabel();
            lblCharacterNumber = new System.Windows.Forms.Label();
            lblHighlightLimit = new System.Windows.Forms.Label();
            lblLineNumber = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            finderCtrl1 = new SqlSync.FinderCtrl();
            panel1.SuspendLayout();
            panel2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(pictureBox1)).BeginInit();
            tableLayoutPanel1.SuspendLayout();
            panel3.SuspendLayout();
            panel4.SuspendLayout();
            cutCopyPastecontextMenuStrip1.SuspendLayout();
            panel5.SuspendLayout();
            SuspendLayout();
            // 
            // txtScriptName
            // 
            txtScriptName.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            txtScriptName.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            txtScriptName.Location = new System.Drawing.Point(5, 19);
            txtScriptName.Name = "txtScriptName";
            txtScriptName.Size = new System.Drawing.Size(1196, 21);
            txtScriptName.TabIndex = 0;
            // 
            // label1
            // 
            label1.Location = new System.Drawing.Point(3, 3);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(120, 16);
            label1.TabIndex = 2;
            label1.Text = "Script Name <F1>:";
            // 
            // label2
            // 
            label2.Location = new System.Drawing.Point(12, 116);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(100, 16);
            label2.TabIndex = 3;
            label2.Text = "Sql Script:";
            // 
            // btnOK
            // 
            btnOK.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            btnOK.Location = new System.Drawing.Point(493, 7);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(75, 23);
            btnOK.TabIndex = 1;
            btnOK.Text = "Save";
            btnOK.Click += new System.EventHandler(btnOK_Click);
            // 
            // button2
            // 
            button2.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            button2.Location = new System.Drawing.Point(581, 7);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(75, 23);
            button2.TabIndex = 2;
            button2.Text = "Cancel";
            button2.Click += new System.EventHandler(button2_Click);
            // 
            // label4
            // 
            label4.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            label4.Location = new System.Drawing.Point(3, 3);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(76, 14);
            label4.TabIndex = 4;
            label4.Text = "Script Id:";
            // 
            // lblHash
            // 
            lblHash.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            lblHash.Location = new System.Drawing.Point(3, 6);
            lblHash.Name = "lblHash";
            lblHash.Size = new System.Drawing.Size(81, 16);
            lblHash.TabIndex = 5;
            lblHash.Text = "SHA1 Hash:";
            // 
            // panel1
            // 
            panel1.BackColor = System.Drawing.Color.White;
            panel1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panel1.Controls.Add(txtScriptId);
            panel1.Controls.Add(label4);
            panel1.Controls.Add(panel2);
            panel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            panel1.Location = new System.Drawing.Point(0, 653);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(1206, 24);
            panel1.TabIndex = 7;
            // 
            // txtScriptId
            // 
            txtScriptId.BorderStyle = System.Windows.Forms.BorderStyle.None;
            txtScriptId.Location = new System.Drawing.Point(79, 3);
            txtScriptId.Name = "txtScriptId";
            txtScriptId.Size = new System.Drawing.Size(344, 14);
            txtScriptId.TabIndex = 6;
            txtScriptId.TabStop = false;
            // 
            // panel2
            // 
            panel2.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            panel2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            panel2.Controls.Add(lblHash);
            panel2.Controls.Add(txtFileHash);
            panel2.Location = new System.Drawing.Point(429, -3);
            panel2.Name = "panel2";
            panel2.Size = new System.Drawing.Size(794, 33);
            panel2.TabIndex = 7;
            // 
            // txtFileHash
            // 
            txtFileHash.BorderStyle = System.Windows.Forms.BorderStyle.None;
            txtFileHash.Location = new System.Drawing.Point(84, 7);
            txtFileHash.Name = "txtFileHash";
            txtFileHash.Size = new System.Drawing.Size(354, 14);
            txtFileHash.TabIndex = 7;
            txtFileHash.TabStop = false;
            // 
            // pictureBox1
            // 
            pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            pictureBox1.Location = new System.Drawing.Point(1185, 2);
            pictureBox1.Name = "pictureBox1";
            pictureBox1.Size = new System.Drawing.Size(21, 16);
            pictureBox1.TabIndex = 3;
            pictureBox1.TabStop = false;
            toolTip1.SetToolTip(pictureBox1, "Click for Help");
            pictureBox1.Click += new System.EventHandler(pictureBox1_Click);
            // 
            // lblAddDate
            // 
            lblAddDate.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            lblAddDate.Location = new System.Drawing.Point(262, 1);
            lblAddDate.Name = "lblAddDate";
            lblAddDate.Size = new System.Drawing.Size(165, 22);
            lblAddDate.TabIndex = 20;
            lblAddDate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label10
            // 
            label10.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            label10.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            label10.Location = new System.Drawing.Point(626, 1);
            label10.Name = "label10";
            label10.Size = new System.Drawing.Size(96, 22);
            label10.TabIndex = 19;
            label10.Text = "Last Mod Date:";
            label10.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label9
            // 
            label9.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            label9.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            label9.Location = new System.Drawing.Point(186, 1);
            label9.Name = "label9";
            label9.Size = new System.Drawing.Size(69, 22);
            label9.TabIndex = 18;
            label9.Text = "Add Date:";
            label9.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblModBy
            // 
            lblModBy.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            lblModBy.Location = new System.Drawing.Point(527, 1);
            lblModBy.Name = "lblModBy";
            lblModBy.Size = new System.Drawing.Size(92, 22);
            lblModBy.TabIndex = 17;
            lblModBy.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label6
            // 
            label6.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            label6.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            label6.Location = new System.Drawing.Point(434, 1);
            label6.Name = "label6";
            label6.Size = new System.Drawing.Size(86, 22);
            label6.TabIndex = 16;
            label6.Text = "Last Mod By:";
            label6.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // lblAddedBy
            // 
            lblAddedBy.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            lblAddedBy.Location = new System.Drawing.Point(77, 1);
            lblAddedBy.Name = "lblAddedBy";
            lblAddedBy.Size = new System.Drawing.Size(102, 22);
            lblAddedBy.TabIndex = 15;
            lblAddedBy.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // label7
            // 
            label7.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            label7.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            label7.Location = new System.Drawing.Point(4, 1);
            label7.Name = "label7";
            label7.Size = new System.Drawing.Size(66, 22);
            label7.TabIndex = 14;
            label7.Text = "Added By:";
            label7.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // tableLayoutPanel1
            // 
            tableLayoutPanel1.BackColor = System.Drawing.Color.White;
            tableLayoutPanel1.CellBorderStyle = System.Windows.Forms.TableLayoutPanelCellBorderStyle.Single;
            tableLayoutPanel1.ColumnCount = 8;
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 72F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 108F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 75F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 171F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 92F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 98F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 102F));
            tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 510F));
            tableLayoutPanel1.Controls.Add(lblModDate, 7, 0);
            tableLayoutPanel1.Controls.Add(label10, 6, 0);
            tableLayoutPanel1.Controls.Add(lblAddDate, 3, 0);
            tableLayoutPanel1.Controls.Add(lblModBy, 5, 0);
            tableLayoutPanel1.Controls.Add(lblAddedBy, 1, 0);
            tableLayoutPanel1.Controls.Add(label9, 2, 0);
            tableLayoutPanel1.Controls.Add(label6, 4, 0);
            tableLayoutPanel1.Controls.Add(label7, 0, 0);
            tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Bottom;
            tableLayoutPanel1.Location = new System.Drawing.Point(0, 629);
            tableLayoutPanel1.Margin = new System.Windows.Forms.Padding(8, 3, 3, 3);
            tableLayoutPanel1.Name = "tableLayoutPanel1";
            tableLayoutPanel1.RowCount = 1;
            tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
            tableLayoutPanel1.Size = new System.Drawing.Size(1206, 24);
            tableLayoutPanel1.TabIndex = 18;
            // 
            // lblModDate
            // 
            lblModDate.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            lblModDate.Location = new System.Drawing.Point(729, 1);
            lblModDate.Name = "lblModDate";
            lblModDate.Size = new System.Drawing.Size(145, 22);
            lblModDate.TabIndex = 21;
            lblModDate.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // panel3
            // 
            panel3.AutoSize = true;
            panel3.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            panel3.Controls.Add(pictureBox1);
            panel3.Controls.Add(scriptConfigCtrl1);
            panel3.Controls.Add(txtScriptName);
            panel3.Controls.Add(label1);
            panel3.Dock = System.Windows.Forms.DockStyle.Top;
            panel3.Location = new System.Drawing.Point(0, 0);
            panel3.Name = "panel3";
            panel3.Size = new System.Drawing.Size(1206, 154);
            panel3.TabIndex = 19;
            // 
            // scriptConfigCtrl1
            // 
            scriptConfigCtrl1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            scriptConfigCtrl1.BackColor = System.Drawing.SystemColors.Control;
            scriptConfigCtrl1.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            scriptConfigCtrl1.BuildSequenceChanged = false;
            scriptConfigCtrl1.DatabaseList = null;
            scriptConfigCtrl1.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            scriptConfigCtrl1.HasChanged = false;
            scriptConfigCtrl1.Location = new System.Drawing.Point(5, 43);
            scriptConfigCtrl1.Margin = new System.Windows.Forms.Padding(0);
            scriptConfigCtrl1.Name = "scriptConfigCtrl1";
            scriptConfigCtrl1.ShowFull = false;
            scriptConfigCtrl1.Size = new System.Drawing.Size(1207, 111);
            scriptConfigCtrl1.TabIndex = 1;
            // 
            // splitter1
            // 
            splitter1.Dock = System.Windows.Forms.DockStyle.Top;
            splitter1.Location = new System.Drawing.Point(0, 154);
            splitter1.Name = "splitter1";
            splitter1.Size = new System.Drawing.Size(1206, 3);
            splitter1.TabIndex = 20;
            splitter1.TabStop = false;
            // 
            // panel4
            // 
            panel4.Controls.Add(rtbSqlScript);
            panel4.Controls.Add(panel5);
            panel4.Controls.Add(finderCtrl1);
            panel4.Dock = System.Windows.Forms.DockStyle.Fill;
            panel4.Location = new System.Drawing.Point(0, 157);
            panel4.Name = "panel4";
            panel4.Size = new System.Drawing.Size(1206, 472);
            panel4.TabIndex = 21;
            // 
            // rtbSqlScript
            // 
            rtbSqlScript.AcceptsTab = true;
            rtbSqlScript.CaseSensitive = false;
            rtbSqlScript.ContextMenuStrip = cutCopyPastecontextMenuStrip1;
            rtbSqlScript.Dock = System.Windows.Forms.DockStyle.Fill;
            rtbSqlScript.FilterAutoComplete = true;
            rtbSqlScript.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            rtbSqlScript.HighlightDescriptors = highLightDescriptorCollection1;
            rtbSqlScript.HighlightType = SqlSync.Highlighting.SyntaxHightlightType.Sql;
            rtbSqlScript.Location = new System.Drawing.Point(0, 0);
            rtbSqlScript.MaxUndoRedoSteps = 50;
            rtbSqlScript.Name = "rtbSqlScript";
            rtbSqlScript.Size = new System.Drawing.Size(1206, 405);
            rtbSqlScript.SuspendHighlighting = false;
            rtbSqlScript.TabIndex = 0;
            rtbSqlScript.Text = "";
            rtbSqlScript.WordWrap = false;
            rtbSqlScript.SelectionChanged += new System.EventHandler(rtbSqlScript_SelectionChanged);
            rtbSqlScript.TextChanged += new System.EventHandler(rtbSqlScript_TextChanged);
            rtbSqlScript.KeyUp += new System.Windows.Forms.KeyEventHandler(rtbSqlScript_KeyUp);
            rtbSqlScript.MouseUp += new System.Windows.Forms.MouseEventHandler(rtbSqlScript_MouseUp);
            // 
            // cutCopyPastecontextMenuStrip1
            // 
            cutCopyPastecontextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            toolStripSeparator1});
            cutCopyPastecontextMenuStrip1.Name = "mnuCopyPaste";
            cutCopyPastecontextMenuStrip1.Size = new System.Drawing.Size(103, 76);
            // 
            // toolStripSeparator1
            // 
            toolStripSeparator1.Name = "toolStripSeparator1";
            toolStripSeparator1.Size = new System.Drawing.Size(99, 6);
            // 
            // panel5
            // 
            panel5.Controls.Add(label5);
            panel5.Controls.Add(btnOK);
            panel5.Controls.Add(lnkRunPolicyChecks);
            panel5.Controls.Add(button2);
            panel5.Controls.Add(lblCharacterNumber);
            panel5.Controls.Add(lblHighlightLimit);
            panel5.Controls.Add(lblLineNumber);
            panel5.Controls.Add(label3);
            panel5.Dock = System.Windows.Forms.DockStyle.Bottom;
            panel5.Location = new System.Drawing.Point(0, 405);
            panel5.Name = "panel5";
            panel5.Size = new System.Drawing.Size(1206, 37);
            panel5.TabIndex = 10;
            // 
            // label5
            // 
            label5.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            label5.AutoSize = true;
            label5.Location = new System.Drawing.Point(1115, 12);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(69, 13);
            label5.TabIndex = 6;
            label5.Text = "Character:";
            // 
            // lnkRunPolicyChecks
            // 
            lnkRunPolicyChecks.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            lnkRunPolicyChecks.AutoSize = true;
            lnkRunPolicyChecks.Location = new System.Drawing.Point(930, 12);
            lnkRunPolicyChecks.Name = "lnkRunPolicyChecks";
            lnkRunPolicyChecks.Size = new System.Drawing.Size(122, 13);
            lnkRunPolicyChecks.TabIndex = 9;
            lnkRunPolicyChecks.TabStop = true;
            lnkRunPolicyChecks.Text = "[Run Policy Checks]";
            lnkRunPolicyChecks.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(lnkRunPolicyChecks_LinkClicked);
            // 
            // lblCharacterNumber
            // 
            lblCharacterNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            lblCharacterNumber.AutoSize = true;
            lblCharacterNumber.Location = new System.Drawing.Point(1180, 12);
            lblCharacterNumber.Name = "lblCharacterNumber";
            lblCharacterNumber.Size = new System.Drawing.Size(14, 13);
            lblCharacterNumber.TabIndex = 8;
            lblCharacterNumber.Text = "0";
            // 
            // lblHighlightLimit
            // 
            lblHighlightLimit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            lblHighlightLimit.AutoSize = true;
            lblHighlightLimit.Font = new System.Drawing.Font("Verdana", 6.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            lblHighlightLimit.ForeColor = System.Drawing.Color.Firebrick;
            lblHighlightLimit.Location = new System.Drawing.Point(4, 7);
            lblHighlightLimit.Name = "lblHighlightLimit";
            lblHighlightLimit.Size = new System.Drawing.Size(283, 24);
            lblHighlightLimit.TabIndex = 3;
            lblHighlightLimit.Text = "Due to the length of this script, SQL syntax highlighting\r\nhas been suspended to " +
    "improve performance.";
            lblHighlightLimit.Visible = false;
            // 
            // lblLineNumber
            // 
            lblLineNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            lblLineNumber.AutoSize = true;
            lblLineNumber.Location = new System.Drawing.Point(1090, 12);
            lblLineNumber.Name = "lblLineNumber";
            lblLineNumber.Size = new System.Drawing.Size(14, 13);
            lblLineNumber.TabIndex = 7;
            lblLineNumber.Text = "0";
            // 
            // label3
            // 
            label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            label3.AutoSize = true;
            label3.Location = new System.Drawing.Point(1058, 12);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(35, 13);
            label3.TabIndex = 5;
            label3.Text = "Line:";
            // 
            // finderCtrl1
            // 
            finderCtrl1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            finderCtrl1.Dock = System.Windows.Forms.DockStyle.Bottom;
            finderCtrl1.Location = new System.Drawing.Point(0, 442);
            finderCtrl1.Margin = new System.Windows.Forms.Padding(4, 3, 4, 3);
            finderCtrl1.Name = "finderCtrl1";
            finderCtrl1.Size = new System.Drawing.Size(1206, 30);
            finderCtrl1.TabIndex = 4;
            // 
            // AddScriptTextForm
            // 
            AutoScaleDimensions = new System.Drawing.SizeF(96F, 96F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Dpi;
            AutoSize = true;
            CancelButton = button2;
            ClientSize = new System.Drawing.Size(1206, 677);
            Controls.Add(panel4);
            Controls.Add(splitter1);
            Controls.Add(panel3);
            Controls.Add(tableLayoutPanel1);
            Controls.Add(panel1);
            Controls.Add(label2);
            Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            KeyPreview = true;
            Name = "AddScriptTextForm";
            Text = "Add SQL Script Text ";
            Load += new System.EventHandler(AddScriptTextForm_Load);
            KeyDown += new System.Windows.Forms.KeyEventHandler(AddScriptTextForm_KeyDown);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            panel2.ResumeLayout(false);
            panel2.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(pictureBox1)).EndInit();
            tableLayoutPanel1.ResumeLayout(false);
            panel3.ResumeLayout(false);
            panel3.PerformLayout();
            panel4.ResumeLayout(false);
            cutCopyPastecontextMenuStrip1.ResumeLayout(false);
            panel5.ResumeLayout(false);
            panel5.PerformLayout();
            ResumeLayout(false);
            PerformLayout();

        }
        #endregion

        private void btnOK_Click(object sender, System.EventArgs e)
        {
            if (txtScriptName.Text.Length == 0)
            {
                MessageBox.Show("Please enter a script name", "Script Name Needed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtScriptName.Focus();
                return;
            }

            if (rtbSqlScript.Text.Length == 0)
            {
                MessageBox.Show("Please enter a SQL script", "Script Needed", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                rtbSqlScript.Focus();
                return;
            }

            if (!scriptConfigCtrl1.ValidateValues(rtbSqlScript.Text, txtScriptName.Text))
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


            if (scriptCfgRow.FileName.EndsWith(DbObjectType.StoredProcedure, StringComparison.CurrentCultureIgnoreCase) ||
                scriptCfgRow.FileName.EndsWith(DbObjectType.UserDefinedFunction, StringComparison.CurrentCultureIgnoreCase) ||
                scriptCfgRow.FileName.EndsWith(DbObjectType.Trigger, StringComparison.CurrentCultureIgnoreCase))
                scriptCfgRow.StripTransactionText = false;


            Regex useStmt = new Regex("^\\s*use\\s+\\S+", RegexOptions.IgnoreCase | RegexOptions.Multiline);
            if (useStmt.Match(rtbSqlScript.Text).Success)
            {
                string msg = "A database \"USE\" statement was found in your SQL:\r\n\r\n" + useStmt.Match(rtbSqlScript.Text).Value.Trim() + "\r\n\r\n" +
                "This statement will be stripped out of the file!\r\nTo change your target database, use the \"Target DB\" list on the script form.\r\n\r\n" +
                "Click \"OK\" to save the script, minus the \"USE\" statement.\r\nClick \"Cancel\" to edit the script.";
                if (DialogResult.Cancel == MessageBox.Show(msg, "USE Statement detected", MessageBoxButtons.OKCancel, MessageBoxIcon.Warning))
                {
                    return;
                }
                else
                {
                    rtbSqlScript.Text = rtbSqlScript.Text.Replace(useStmt.Match(rtbSqlScript.Text).Value, string.Empty);
                }
            }


            // this.codeReviewControl1.SaveData(this.rtbSqlScript.Text);


            scriptConfigCtrl1.UpdateScriptConfigValues();
            sqlName = txtScriptName.Text;
            sqlText = rtbSqlScript.Text;
            DialogResult = DialogResult.OK;
            configurationChanged = scriptConfigCtrl1.HasChanged;
            buildSequenceChanged = scriptConfigCtrl1.BuildSequenceChanged;

            //if (this.codeReviewControl1.HasChanges)
            //    this.configurationChanged = true;

            Close();
        }

        private void button2_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void AddScriptTextForm_Load(object sender, System.EventArgs e)
        {
            if (txtScriptName.Text.Length > 0)
                scriptConfigCtrl1.ParentFileName = txtScriptName.Text;

            txtScriptName.TextChanged += new EventHandler(txtScriptName_TextChanged);
            finderCtrl1.AddControlToSearch(rtbSqlScript);
            rtbSqlScript.FilterAutoComplete = true;
            txtScriptId.Text = scriptCfgRow.ScriptId;
            txtFileHash.Text = fileHash;
            lblAddedBy.Text = scriptCfgRow.AddedBy;
            lblModBy.Text = scriptCfgRow.ModifiedBy;
            lblAddDate.Text = (scriptCfgRow.IsDateAddedNull()) ? "" : scriptCfgRow.DateAdded.ToString();
            lblModDate.Text = (scriptCfgRow.IsDateModifiedNull() || scriptCfgRow.DateModified == DateTime.MinValue) ? "" : scriptCfgRow.DateModified.ToString();

            if (scriptConfigCtrl1.Visible)
                scriptConfigCtrl1.SetConfigData(ref scriptCfgRow, databaseList, tagList, scriptTagRequired);

            if (fullFilePath.Length > 0)
            {
                string[] batch = SqlBuildHelper.ReadBatchFromScriptFile(fullFilePath, false, true);
                string full = String.Join("", batch);
                rtbSqlScript.AppendText(full);
                rtbSqlScript.Select(0, 0);
            }
            rtbSqlScript.AcceptsTab = true;

            if (utilRegistry != null)
            {
                List<ToolStripItem> list = new List<ToolStripItem>();
                for (int i = 0; i < utilRegistry.Items.Length; i++)
                    list.Add(SetUtilityRegistry(utilRegistry.Items[i]));

                if (list.Count > 0)
                    for (int i = 0; i < list.Count; i++)
                        cutCopyPastecontextMenuStrip1.Items.Add(list[i]);
            }

            ToolStripMenuItem mnuOptimizeWithNoLock = new ToolStripMenuItem("Optimize SELECT : Add \"WITH (NOLOCK)\" Directive", null, mnuOptimizeWithNoLock_Click);
            cutCopyPastecontextMenuStrip1.Items.Add(new ToolStripSeparator());
            cutCopyPastecontextMenuStrip1.Items.Add(mnuOptimizeWithNoLock);

            ToolStripMenuItem mnuConvertToAlter = new ToolStripMenuItem("Convert to ALTER COLUMN", null, mnuConvertToAlter_Click);
            cutCopyPastecontextMenuStrip1.Items.Add(new ToolStripSeparator());
            cutCopyPastecontextMenuStrip1.Items.Add(mnuConvertToAlter);

            ToolStripMenuItem mnuResyncTable = new ToolStripMenuItem("Transform to resync TABLE", null, mnuTransformtoResyncTable_Click);
            cutCopyPastecontextMenuStrip1.Items.Add(mnuResyncTable);


            toolTip1.SetToolTip(label1, "Click <F1> to insert Selected Sql Script text as Script Name\r\nClick <Shift><F1> to append to Script Name");

            if (scriptConfigCtrl1.ShowFull == false)
                rtbSqlScript.Select();

            if (!allowEdit)
                btnOK.Enabled = false;

            //if (EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig == null || !EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig.Enabled)
            //{
            //    this.codeReviewControl1.Height = 0;
            //    this.codeReviewControl1.Visible = false;
            //}
            //else
            //{
            //    //Add code review items
            //    //get the last editor
            //    string lastEditor = this.scriptCfgRow.ModifiedBy.Length == 0 ? this.scriptCfgRow.AddedBy : this.scriptCfgRow.ModifiedBy;
            //    this.codeReviewControl1.BindData(ref this.buildData, ref this.scriptCfgRow, this.rtbSqlScript.Text, lastEditor);
            //}
        }

        void txtScriptName_TextChanged(object sender, EventArgs e)
        {
            scriptConfigCtrl1.ParentFileName = txtScriptName.Text;
        }

        private ToolStripItem SetUtilityRegistry(object utilityRegItem)
        {
            if (utilityRegItem.GetType() == typeof(Utility.Replace))
            {
                Utility.Replace replace = (Utility.Replace)utilityRegItem;
                string text = replace.OldString + "-->" + replace.NewString;
                return new ToolStripMenuItem(text, null, new System.EventHandler(mnuReplaceandHighlight_Click));
            }
            else if (utilityRegItem.GetType() == typeof(Utility.SubMenu))
            {
                Utility.SubMenu sub = (Utility.SubMenu)utilityRegItem;
                ToolStripItem item;
                if (sub.Name == "-")
                    item = new ToolStripSeparator();
                else
                    item = new ToolStripMenuItem(sub.Name);

                if (sub.Items == null) return item;

                for (int i = 0; i < sub.Items.Length; i++)
                {
                    ((ToolStripMenuItem)item).DropDownItems.Add(SetUtilityRegistry(sub.Items[i]));
                }

                return item;
            }
            else if (utilityRegItem.GetType() == typeof(Utility.UtilityQuery))
            {
                Utility.UtilityQuery util = (Utility.UtilityQuery)utilityRegItem;
                return new ToolStripMenuItem(util.Description, null, new System.EventHandler(mnuUtilityQueryInsert_Click));
            }
            return new ToolStripSeparator();
        }

        private void lnkProcessBatch_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        {
            int batchNumber = 0;
            string[] lines = rtbSqlScript.Lines;
            rtbSqlScript.Clear();
            for (int i = 0; i < lines.Length; i++)
            {
                if (batchNumber % 2 == 0)
                    rtbSqlScript.SelectionColor = evenColor;
                else
                    rtbSqlScript.SelectionColor = oddColor;

                rtbSqlScript.AppendText(lines[i] + "\r\n");
                if (lines[i].Trim() == BatchParsing.Delimiter)
                    batchNumber++;
            }

        }

        private void mnuReplaceandHighlight_Click(object sender, System.EventArgs e)
        {
            string text = ((ToolStripItem)sender).Text;
            int arrowLoc = text.IndexOf("-->");
            string old = text.Substring(0, arrowLoc);
            string newStr = text.Substring(arrowLoc + 3);
            ReplaceAndHighlightString(old, newStr);
        }

        private void mnuUtilityQueryInsert_Click(object sender, System.EventArgs e)
        {
            string text = ((ToolStripItem)sender).Text;
            if (utilRegistry != null)
            {
                string fileName = string.Empty;
                for (int i = 0; i < utilRegistry.Items.Length; i++)
                {
                    fileName = GetUtilityQueryFileName(text, utilRegistry.Items[i]);
                    if (fileName != string.Empty)
                        break;
                }
                if (fileName != null && fileName != string.Empty)
                    InsertUtilityQuery(fileName, text);

            }
        }
        private string GetUtilityQueryFileName(string menuText, object registryItem)
        {
            if (registryItem.GetType() == typeof(Utility.UtilityQuery) &&
                ((Utility.UtilityQuery)registryItem).Description == menuText)
            {
                return ((Utility.UtilityQuery)registryItem).FileName;
            }
            else if (registryItem.GetType() == typeof(Utility.SubMenu))
            {
                string fName = string.Empty;
                Utility.SubMenu sub = (Utility.SubMenu)registryItem;
                if (sub.Items == null) return fName;

                for (int i = 0; i < sub.Items.Length; i++)
                {
                    fName = GetUtilityQueryFileName(menuText, sub.Items[i]);
                    if (fName != string.Empty)
                        return fName;
                }
            }
            return string.Empty;

        }
        private void InsertUtilityQuery(string fileLocation, string title)
        {
            if (System.IO.File.Exists(fileLocation) == false)
            {
                MessageBox.Show("Unable to locate utility file at:\r\n" + fileLocation);
                return;
            }

            string query = string.Empty;
            using (System.IO.StreamReader sr = System.IO.File.OpenText(fileLocation))
            {
                query = sr.ReadToEnd();
            }

            //automatic inserts...
            Regex regToday = new Regex(@"<\[today\]>", RegexOptions.IgnoreCase);
            query = regToday.Replace(query, DateTime.Now.ToString("MM/dd/yyyy"));

            Regex regScriptName = new Regex(@"<\[script_name\]>", RegexOptions.IgnoreCase);
            query = regScriptName.Replace(query, txtScriptName.Text);

            Regex regAuthor = new Regex(@"<\[author\]>", RegexOptions.IgnoreCase);
            query = regAuthor.Replace(query, System.Environment.UserName);


            //manual inserts
            Regex check = new Regex("<<[A-Za-z0-9 ]{1,}>>");
            MatchCollection matches = check.Matches(query);
            if (matches.Count > 0)
            {
                ArrayList keys = new ArrayList();
                for (int i = 0; i < matches.Count; i++)
                {
                    if (keys.Contains(matches[i].Value) == false)
                        keys.Add(matches[i].Value);
                }
                string[] arrKey = new string[keys.Count];
                keys.CopyTo(arrKey);
                string inputText = string.Empty;
                if (rtbSqlScript.SelectedText.Length > 0)
                    inputText = rtbSqlScript.SelectedText;

                UtilityReplacement frmRepl = new UtilityReplacement(arrKey, title, inputText);
                frmRepl.ShowDialog();

                if (frmRepl.DialogResult == DialogResult.OK)
                {
                    for (int i = 0; i < arrKey.Length; i++)
                        query = query.Replace(arrKey[i], frmRepl.Replacements[arrKey[i]]);
                }
                else
                    query = string.Empty;
            }

            if (query.Length == 0)
                return;

            //Add the default title if applicable
            Regex regTitle = new Regex(@"\$\{[Tt][Ii][Tt][Ll][Ee]: (.*?)\}");
            MatchCollection queryTitle = regTitle.Matches(query);
            Regex regRemoveTitle = new Regex(@"\$\{[Tt][Ii][Tt][Ll][Ee]:");
            MatchCollection remTitle = regRemoveTitle.Matches(query);

            if (txtScriptName.Text.Length == 0)
            {
                if (queryTitle.Count > 0)
                {
                    txtScriptName.Text = queryTitle[0].Value.Replace(remTitle[0].Value, "").Replace("}", "").Trim();
                    query = query.Replace(queryTitle[0].Value, "").TrimStart();
                }
            }
            else
            {
                for (int i = 0; i < queryTitle.Count; i++)
                    query = query.Replace(queryTitle[0].Value, "").TrimStart();
            }

            int selectionStart = rtbSqlScript.SelectionStart;
            int trueLength = query.Replace("\n", "").Length;
            rtbSqlScript.SelectedText = query;
            rtbSqlScript.Select(selectionStart, trueLength);
            rtbSqlScript.SelectionColor = Color.Orange;
            rtbSqlScript.Select(selectionStart + query.Length, 0);

        }
        private void ReplaceAndHighlightString(string oldString, string newString)
        {
            int selectionStart = rtbSqlScript.SelectionStart;
            string replaceVal = rtbSqlScript.SelectedText.Replace(oldString, newString);
            replaceVal = replaceVal.Replace(oldString.ToLower(), newString);
            rtbSqlScript.SelectedText = replaceVal;

            bool foundNewString = true;
            int findIndex = 0;
            int newStringLength = newString.Length;
            while (foundNewString)
            {
                int start = replaceVal.IndexOf(newString, findIndex);
                if (start > -1)
                {
                    rtbSqlScript.Select(selectionStart + start, newStringLength);
                    rtbSqlScript.SelectionColor = Color.Purple;
                    findIndex = start + 1;
                }
                else
                {
                    foundNewString = false;
                }
            }
            rtbSqlScript.Select(selectionStart + replaceVal.Length, 0);
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
            if (e.KeyCode == Keys.V && e.Modifiers == Keys.Control && txtScriptName.Text.Length == 0)
            {
                Regex table = new Regex(@"TABLE [A-Za-z0-9\[\] _]{1,}");
                Match found = table.Match(rtbSqlScript.Text, 0);
                if (found != null && found.Value.Length > 0)
                {
                    txtScriptName.Text = found.Value.Replace("TABLE [", "").Replace("]", "");
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
            ScriptWrapping.TransformCreateTableToAlterColumn(rawScript, schema, tableName, out changedScript);
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
                    rtbSqlScript.SelectedText = ScriptWrapping.TransformCreateTableToResyncTable(rawScript, schema, tableName);
            }
        }

        private void AddScriptTextForm_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {

            if (rtbSqlScript.SelectedText == string.Empty)
                return;

            switch (e.KeyCode)
            {
                case Keys.F1:
                    char[] nonPrintable = new char[] { '\r', '\n', '\t' };
                    string selection = rtbSqlScript.SelectedText.Trim();
                    if (selection.IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) > -1 ||
                        selection.IndexOfAny(nonPrintable) > -1)
                    {
                        MessageBox.Show("Selection contains invalid file name characters. Paste Failed", "Invalid Characters Found", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return;
                    }

                    if (e.Modifiers == Keys.Shift)
                        txtScriptName.Text += " " + rtbSqlScript.SelectedText.Trim();
                    else
                        txtScriptName.Text = rtbSqlScript.SelectedText.Trim();
                    break;
                case Keys.F12:
                    string val = ProcessAutomaticProcessing(rtbSqlScript.SelectedText.Trim());
                    if (val.Length > 0)
                        rtbSqlScript.SelectedText = val;
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
            try
            {

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
            catch { }
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
            int lineStartIndex = rtbSqlScript.GetFirstCharIndexOfCurrentLine();
            int lineNumber = rtbSqlScript.GetLineFromCharIndex(lineStartIndex) + 1;
            int absoluteCharNumber = rtbSqlScript.SelectionStart;
            int localCharNumber = absoluteCharNumber - lineStartIndex + 1;

            lblLineNumber.Text = lineNumber.ToString();
            lblCharacterNumber.Text = localCharNumber.ToString();
        }
        private void rtbSqlScript_TextChanged(object sender, EventArgs e)
        {
            //If the script is too long, the active SQL highlighting is a huge performance hit.
            //Turn it off instead.
            if (rtbSqlScript.Text.Length > 50000)
            {
                rtbSqlScript.HighlightType = SqlSync.Highlighting.SyntaxHightlightType.None;
                lblHighlightLimit.Visible = true;
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
                violations.ScriptName = txtScriptName.Text;
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
