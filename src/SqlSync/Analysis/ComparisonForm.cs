using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.IO;
using SqlSync.Compare;
using System.Collections.Generic;
using SqlSync.SqlBuild;
using System.Text;
using System.Text.RegularExpressions;
namespace SqlSync.Analysis
{
	/// <summary>
	/// Summary description for ComparisonForm.
	/// </summary>
	public class ComparisonForm : System.Windows.Forms.Form
	{
        private SqlSync.ColumnSorter listSorter = new ColumnSorter();
		private System.Windows.Forms.TextBox txtLeftFile;
		private System.Windows.Forms.TextBox txtRightFile;
		private System.Windows.Forms.OpenFileDialog openFileDialog1;
		private System.Windows.Forms.Button btnSelectFile1;
		private System.Windows.Forms.Button btnSelectFile2;
		private System.Windows.Forms.Button btnCompare;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.Panel pnlControls;
        private System.Windows.Forms.Splitter splitter1;
        private Label label2;
        private Label label1;
        private SqlUnifiedDiff diff = null;
        private UrielGuy.SyntaxHighlighting.SyntaxHighlightingTextBox rtbResultsHighlight;

        private string leftZipFilePath = string.Empty;
        private string leftTempFilePath = string.Empty;
        private string rightZipFilePath = string.Empty;
        private string rightTempFilePath = string.Empty;
        private SqlSync.SqlBuild.SqlSyncBuildData buildData;
        private double lastBuildNumber;
        private ContextMenuStrip ctxImport;
        private ToolStripMenuItem mnuImportScripts;
        private IContainer components;
        private StatusStrip statusStrip1;
        private ToolStripStatusLabel statGeneral;
        private BackgroundWorker bgWorker;
        private bool refreshProjectList = false;
        private StatusStrip statusStrip2;
        private ToolStripStatusLabel statOnlyInLeft;
        private ToolStripStatusLabel statIdentical;
        private ToolStripStatusLabel statDiffering;
        private ToolStripStatusLabel statOnlyInRight;
        private ListView lstFiles;
        private ColumnHeader colLeftName;
        private ColumnHeader colRightName;
        private LinkedRichTextBoxes linkedBoxes;
        private ToolStripMenuItem removeSelectedScriptsFromTheLeftFileToolStripMenuItem;
        private ToolStripMenuItem mnuMergeResolve;
        private ColumnHeader Index;
        private ColumnHeader colRightIndex;
        private Panel pnlSingleResults;
        private FinderCtrl finderSingle;
        private ColumnHeader colLeftTag;
        private ColumnHeader colRightTag;
        private bool startImmediateCompare = false;

        public bool RefreshProjectList
        {
            get { return refreshProjectList; }
            set { refreshProjectList = value; }
        }

        public ComparisonForm(ref SqlBuild.SqlSyncBuildData buildData, double lastBuildNumber, string leftZipFilePath, string leftTempFilePath, string rightZipFilePath,bool startImmediateCompare)
        {
            InitializeComponent();
            this.leftTempFilePath = leftTempFilePath;
            this.leftZipFilePath = leftZipFilePath;
            this.rightZipFilePath = rightZipFilePath;
            this.buildData = buildData;
            this.lastBuildNumber = lastBuildNumber;
            this.startImmediateCompare = startImmediateCompare;
            this.linkedBoxes.FileChanged += new EventHandler(linkedBoxes_FileChanged);
        }

        void linkedBoxes_FileChanged(object sender, EventArgs e)
        {
            SqlBuildFileHelper.SaveSqlBuildProjectFile(ref buildData, this.leftTempFilePath + XmlFileNames.MainProjectFile, this.leftZipFilePath);
        }
		public ComparisonForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			
			//
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ComparisonForm));
            this.txtLeftFile = new System.Windows.Forms.TextBox();
            this.txtRightFile = new System.Windows.Forms.TextBox();
            this.btnSelectFile1 = new System.Windows.Forms.Button();
            this.btnSelectFile2 = new System.Windows.Forms.Button();
            this.openFileDialog1 = new System.Windows.Forms.OpenFileDialog();
            this.btnCompare = new System.Windows.Forms.Button();
            this.panel1 = new System.Windows.Forms.Panel();
            this.pnlSingleResults = new System.Windows.Forms.Panel();
            this.finderSingle = new SqlSync.FinderCtrl();
            this.rtbResultsHighlight = new UrielGuy.SyntaxHighlighting.SyntaxHighlightingTextBox();
            this.linkedBoxes = new SqlSync.Analysis.LinkedRichTextBoxes();
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statGeneral = new System.Windows.Forms.ToolStripStatusLabel();
            this.pnlControls = new System.Windows.Forms.Panel();
            this.statusStrip2 = new System.Windows.Forms.StatusStrip();
            this.statOnlyInLeft = new System.Windows.Forms.ToolStripStatusLabel();
            this.statIdentical = new System.Windows.Forms.ToolStripStatusLabel();
            this.statDiffering = new System.Windows.Forms.ToolStripStatusLabel();
            this.statOnlyInRight = new System.Windows.Forms.ToolStripStatusLabel();
            this.lstFiles = new System.Windows.Forms.ListView();
            this.Index = new System.Windows.Forms.ColumnHeader();
            this.colLeftTag = new System.Windows.Forms.ColumnHeader();
            this.colLeftName = new System.Windows.Forms.ColumnHeader();
            this.colRightIndex = new System.Windows.Forms.ColumnHeader();
            this.colRightTag = new System.Windows.Forms.ColumnHeader();
            this.colRightName = new System.Windows.Forms.ColumnHeader();
            this.ctxImport = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuImportScripts = new System.Windows.Forms.ToolStripMenuItem();
            this.removeSelectedScriptsFromTheLeftFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.mnuMergeResolve = new System.Windows.Forms.ToolStripMenuItem();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.bgWorker = new System.ComponentModel.BackgroundWorker();
            this.panel1.SuspendLayout();
            this.pnlSingleResults.SuspendLayout();
            this.statusStrip1.SuspendLayout();
            this.pnlControls.SuspendLayout();
            this.statusStrip2.SuspendLayout();
            this.ctxImport.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtLeftFile
            // 
            this.txtLeftFile.Location = new System.Drawing.Point(81, 16);
            this.txtLeftFile.Name = "txtLeftFile";
            this.txtLeftFile.Size = new System.Drawing.Size(687, 21);
            this.txtLeftFile.TabIndex = 0;
            // 
            // txtRightFile
            // 
            this.txtRightFile.Location = new System.Drawing.Point(81, 42);
            this.txtRightFile.Name = "txtRightFile";
            this.txtRightFile.Size = new System.Drawing.Size(687, 21);
            this.txtRightFile.TabIndex = 1;
            // 
            // btnSelectFile1
            // 
            this.btnSelectFile1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnSelectFile1.Location = new System.Drawing.Point(774, 15);
            this.btnSelectFile1.Name = "btnSelectFile1";
            this.btnSelectFile1.Size = new System.Drawing.Size(52, 23);
            this.btnSelectFile1.TabIndex = 2;
            this.btnSelectFile1.Text = "Select";
            this.btnSelectFile1.Visible = false;
            this.btnSelectFile1.Click += new System.EventHandler(this.btnSelectFile1_Click);
            // 
            // btnSelectFile2
            // 
            this.btnSelectFile2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnSelectFile2.Location = new System.Drawing.Point(774, 42);
            this.btnSelectFile2.Name = "btnSelectFile2";
            this.btnSelectFile2.Size = new System.Drawing.Size(52, 21);
            this.btnSelectFile2.TabIndex = 3;
            this.btnSelectFile2.Text = "Select";
            this.btnSelectFile2.Click += new System.EventHandler(this.btnSelectFile2_Click);
            // 
            // openFileDialog1
            // 
            this.openFileDialog1.Filter = "Sql Build Files *.sbm|*.sbm|All Files *.*|*.*";
            this.openFileDialog1.Title = "Select Sql Build File";
            // 
            // btnCompare
            // 
            this.btnCompare.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnCompare.Location = new System.Drawing.Point(19, 69);
            this.btnCompare.Name = "btnCompare";
            this.btnCompare.Size = new System.Drawing.Size(75, 23);
            this.btnCompare.TabIndex = 4;
            this.btnCompare.Text = "Compare";
            this.btnCompare.Click += new System.EventHandler(this.btnCompare_Click);
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.pnlSingleResults);
            this.panel1.Controls.Add(this.linkedBoxes);
            this.panel1.Controls.Add(this.statusStrip1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 315);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1111, 331);
            this.panel1.TabIndex = 5;
            // 
            // pnlSingleResults
            // 
            this.pnlSingleResults.Controls.Add(this.finderSingle);
            this.pnlSingleResults.Controls.Add(this.rtbResultsHighlight);
            this.pnlSingleResults.Dock = System.Windows.Forms.DockStyle.Fill;
            this.pnlSingleResults.Location = new System.Drawing.Point(0, 0);
            this.pnlSingleResults.Name = "pnlSingleResults";
            this.pnlSingleResults.Size = new System.Drawing.Size(1111, 309);
            this.pnlSingleResults.TabIndex = 11;
            // 
            // finderSingle
            // 
            this.finderSingle.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.finderSingle.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.finderSingle.Location = new System.Drawing.Point(0, 284);
            this.finderSingle.Name = "finderSingle";
            this.finderSingle.Size = new System.Drawing.Size(1111, 25);
            this.finderSingle.TabIndex = 9;
            // 
            // rtbResultsHighlight
            // 
            this.rtbResultsHighlight.AcceptsTab = true;
            this.rtbResultsHighlight.CaseSensitive = false;
            this.rtbResultsHighlight.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbResultsHighlight.FilterAutoComplete = true;
            this.rtbResultsHighlight.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbResultsHighlight.HighlightDescriptors = highLightDescriptorCollection1;
            this.rtbResultsHighlight.HighlightType = SqlSync.Highlighting.SyntaxHightlightType.Sql;
            this.rtbResultsHighlight.Location = new System.Drawing.Point(0, 0);
            this.rtbResultsHighlight.MaxUndoRedoSteps = 50;
            this.rtbResultsHighlight.Name = "rtbResultsHighlight";
            this.rtbResultsHighlight.Size = new System.Drawing.Size(1111, 309);
            this.rtbResultsHighlight.SuspendHighlighting = false;
            this.rtbResultsHighlight.TabIndex = 8;
            this.rtbResultsHighlight.Text = "";
            // 
            // linkedBoxes
            // 
            this.linkedBoxes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.linkedBoxes.LeftFileName = null;
            this.linkedBoxes.Location = new System.Drawing.Point(0, 0);
            this.linkedBoxes.Name = "linkedBoxes";
            this.linkedBoxes.RightFileName = null;
            this.linkedBoxes.ShowMenuStrip = true;
            this.linkedBoxes.Size = new System.Drawing.Size(1111, 309);
            this.linkedBoxes.TabIndex = 10;
            this.linkedBoxes.UnifiedDiffText = null;
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statGeneral});
            this.statusStrip1.Location = new System.Drawing.Point(0, 309);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(1111, 22);
            this.statusStrip1.TabIndex = 9;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statGeneral
            // 
            this.statGeneral.Margin = new System.Windows.Forms.Padding(0, 3, 150, 2);
            this.statGeneral.Name = "statGeneral";
            this.statGeneral.Size = new System.Drawing.Size(38, 17);
            this.statGeneral.Text = "Ready";
            // 
            // pnlControls
            // 
            this.pnlControls.Controls.Add(this.statusStrip2);
            this.pnlControls.Controls.Add(this.lstFiles);
            this.pnlControls.Controls.Add(this.label2);
            this.pnlControls.Controls.Add(this.label1);
            this.pnlControls.Controls.Add(this.txtRightFile);
            this.pnlControls.Controls.Add(this.btnSelectFile2);
            this.pnlControls.Controls.Add(this.btnCompare);
            this.pnlControls.Controls.Add(this.btnSelectFile1);
            this.pnlControls.Controls.Add(this.txtLeftFile);
            this.pnlControls.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlControls.Location = new System.Drawing.Point(0, 0);
            this.pnlControls.Name = "pnlControls";
            this.pnlControls.Size = new System.Drawing.Size(1111, 310);
            this.pnlControls.TabIndex = 6;
            // 
            // statusStrip2
            // 
            this.statusStrip2.Font = new System.Drawing.Font("Verdana", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.statusStrip2.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statOnlyInLeft,
            this.statIdentical,
            this.statDiffering,
            this.statOnlyInRight});
            this.statusStrip2.Location = new System.Drawing.Point(0, 288);
            this.statusStrip2.Name = "statusStrip2";
            this.statusStrip2.Size = new System.Drawing.Size(1111, 22);
            this.statusStrip2.SizingGrip = false;
            this.statusStrip2.TabIndex = 11;
            this.statusStrip2.Text = "statusStrip2";
            // 
            // statOnlyInLeft
            // 
            this.statOnlyInLeft.Margin = new System.Windows.Forms.Padding(0, 3, 30, 2);
            this.statOnlyInLeft.Name = "statOnlyInLeft";
            this.statOnlyInLeft.Size = new System.Drawing.Size(129, 17);
            this.statOnlyInLeft.Text = "Files Only In Left: 0";
            this.statOnlyInLeft.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statIdentical
            // 
            this.statIdentical.Margin = new System.Windows.Forms.Padding(0, 3, 30, 2);
            this.statIdentical.Name = "statIdentical";
            this.statIdentical.Size = new System.Drawing.Size(110, 17);
            this.statIdentical.Text = "Identical Files: 0";
            // 
            // statDiffering
            // 
            this.statDiffering.Margin = new System.Windows.Forms.Padding(0, 3, 30, 2);
            this.statDiffering.Name = "statDiffering";
            this.statDiffering.Size = new System.Drawing.Size(113, 17);
            this.statDiffering.Text = "Changed Files: 0";
            // 
            // statOnlyInRight
            // 
            this.statOnlyInRight.Margin = new System.Windows.Forms.Padding(0, 3, 30, 2);
            this.statOnlyInRight.Name = "statOnlyInRight";
            this.statOnlyInRight.Size = new System.Drawing.Size(137, 17);
            this.statOnlyInRight.Text = "Files Only In Right: 0";
            // 
            // lstFiles
            // 
            this.lstFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstFiles.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.Index,
            this.colLeftTag,
            this.colLeftName,
            this.colRightIndex,
            this.colRightTag,
            this.colRightName});
            this.lstFiles.ContextMenuStrip = this.ctxImport;
            this.lstFiles.FullRowSelect = true;
            this.lstFiles.GridLines = true;
            this.lstFiles.Location = new System.Drawing.Point(7, 98);
            this.lstFiles.Name = "lstFiles";
            this.lstFiles.Size = new System.Drawing.Size(1097, 187);
            this.lstFiles.TabIndex = 10;
            this.lstFiles.UseCompatibleStateImageBehavior = false;
            this.lstFiles.View = System.Windows.Forms.View.Details;
            this.lstFiles.DoubleClick += new System.EventHandler(this.lstFiles_DoubleClick);
            this.lstFiles.ColumnClick += new System.Windows.Forms.ColumnClickEventHandler(this.lstFiles_ColumnClick);
            // 
            // Index
            // 
            this.Index.Text = "Index";
            this.Index.Width = 50;
            // 
            // colLeftTag
            // 
            this.colLeftTag.Tag = "";
            this.colLeftTag.Text = "Tag";
            // 
            // colLeftName
            // 
            this.colLeftName.Text = "";
            this.colLeftName.Width = 431;
            // 
            // colRightIndex
            // 
            this.colRightIndex.Text = "Index";
            this.colRightIndex.Width = 50;
            // 
            // colRightTag
            // 
            this.colRightTag.Text = "Tag";
            // 
            // colRightName
            // 
            this.colRightName.Text = "";
            this.colRightName.Width = 416;
            // 
            // ctxImport
            // 
            this.ctxImport.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuImportScripts,
            this.removeSelectedScriptsFromTheLeftFileToolStripMenuItem,
            this.mnuMergeResolve});
            this.ctxImport.Name = "ctxImport";
            this.ctxImport.Size = new System.Drawing.Size(210, 70);
            this.ctxImport.Opening += new System.ComponentModel.CancelEventHandler(this.ctxImport_Opening);
            // 
            // mnuImportScripts
            // 
            this.mnuImportScripts.Image = ((System.Drawing.Image)(resources.GetObject("mnuImportScripts.Image")));
            this.mnuImportScripts.Name = "mnuImportScripts";
            this.mnuImportScripts.Size = new System.Drawing.Size(209, 22);
            this.mnuImportScripts.Text = "Import into Left File";
            this.mnuImportScripts.Click += new System.EventHandler(this.mnuImportScripts_Click);
            // 
            // removeSelectedScriptsFromTheLeftFileToolStripMenuItem
            // 
            this.removeSelectedScriptsFromTheLeftFileToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("removeSelectedScriptsFromTheLeftFileToolStripMenuItem.Image")));
            this.removeSelectedScriptsFromTheLeftFileToolStripMenuItem.Name = "removeSelectedScriptsFromTheLeftFileToolStripMenuItem";
            this.removeSelectedScriptsFromTheLeftFileToolStripMenuItem.Size = new System.Drawing.Size(209, 22);
            this.removeSelectedScriptsFromTheLeftFileToolStripMenuItem.Text = "Remove from the Left File";
            this.removeSelectedScriptsFromTheLeftFileToolStripMenuItem.Click += new System.EventHandler(this.removeSelectedScriptsFromTheLeftFileToolStripMenuItem_Click);
            // 
            // mnuMergeResolve
            // 
            this.mnuMergeResolve.Image = ((System.Drawing.Image)(resources.GetObject("mnuMergeResolve.Image")));
            this.mnuMergeResolve.Name = "mnuMergeResolve";
            this.mnuMergeResolve.Size = new System.Drawing.Size(209, 22);
            this.mnuMergeResolve.Text = "Merge / Resolve Conflict";
            this.mnuMergeResolve.Click += new System.EventHandler(this.mnuMergeResolve_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 46);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(64, 13);
            this.label2.TabIndex = 9;
            this.label2.Text = "Right File:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 20);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(56, 13);
            this.label1.TabIndex = 8;
            this.label1.Text = "Left File:";
            // 
            // splitter1
            // 
            this.splitter1.BackColor = System.Drawing.SystemColors.ControlLight;
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter1.Location = new System.Drawing.Point(0, 310);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(1111, 5);
            this.splitter1.TabIndex = 7;
            this.splitter1.TabStop = false;
            // 
            // bgWorker
            // 
            this.bgWorker.WorkerReportsProgress = true;
            this.bgWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgWorker_DoWork);
            this.bgWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgWorker_RunWorkerCompleted);
            this.bgWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgWorker_ProgressChanged);
            // 
            // ComparisonForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(1111, 646);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.pnlControls);
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "ComparisonForm";
            this.Text = "Sql Build Manager :: Build File Comparison";
            this.Load += new System.EventHandler(this.ComparisonForm_Load);
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.ComparisonForm_FormClosing);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.pnlSingleResults.ResumeLayout(false);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            this.pnlControls.ResumeLayout(false);
            this.pnlControls.PerformLayout();
            this.statusStrip2.ResumeLayout(false);
            this.statusStrip2.PerformLayout();
            this.ctxImport.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		private void btnSelectFile1_Click(object sender, System.EventArgs e)
		{
			if(DialogResult.OK == openFileDialog1.ShowDialog())
				txtLeftFile.Text = openFileDialog1.FileName;
		}

		private void btnSelectFile2_Click(object sender, System.EventArgs e)
		{
			if(DialogResult.OK == openFileDialog1.ShowDialog())
				txtRightFile.Text = openFileDialog1.FileName;
		}

		private void btnCompare_Click(object sender, System.EventArgs e)
		{
			if(! File.Exists(txtLeftFile.Text) || !File.Exists(txtRightFile.Text))
				return;

			this.Cursor = Cursors.WaitCursor;
            this.lstFiles.Items.Clear();
            this.rtbResultsHighlight.Clear();
            this.pnlSingleResults.BringToFront();

            lstFiles.Columns[2].Text = txtLeftFile.Text.Length > 60 ? "..." + txtLeftFile.Text.Substring(txtLeftFile.Text.Length - 60, 60) : txtLeftFile.Text;
            lstFiles.Columns[5].Text = txtRightFile.Text.Length > 60 ? "..." + txtRightFile.Text.Substring(txtRightFile.Text.Length - 60, 60) : txtRightFile.Text;

            statGeneral.Text = "Processing Comparison";
            pnlControls.Enabled = false;
            //Give the window a second to catch up...
            System.Threading.Thread.Sleep(200); 
            bgWorker.RunWorkerAsync();

            


		}



        private void lstFiles_DoubleClick(object sender, EventArgs e)
        {
            if (lstFiles.SelectedItems.Count == 0)
                return;

            FileCompareResults results = (FileCompareResults)lstFiles.SelectedItems[0].Tag;
            if (results.UnifiedDiffText.Length > 0)
            {
                this.linkedBoxes.BringToFront();
                this.linkedBoxes.UnifiedDiffText = results.UnifiedDiffText;
                this.linkedBoxes.LeftFileName = Path.Combine(leftTempFilePath, results.LeftScriptRow.FileName);
                this.linkedBoxes.RightFileName = Path.Combine(rightTempFilePath, results.RightScriptRow.FileName);
                this.linkedBoxes.SplitUnifiedDiffText();
                return;

            }
                this.pnlSingleResults.BringToFront();
                if (results.LeftScriptText.Length > 0)
                    rtbResultsHighlight.Text = results.LeftScriptText; //lstFiles.SelectedItems[0].Tag.ToString();
                else
                    rtbResultsHighlight.Text = results.RightSciptText;

                rtbResultsHighlight.HighlightType = SqlSync.Highlighting.SyntaxHightlightType.Sql;
        }

        private void ComparisonForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (diff != null)
            {
                if (this.leftTempFilePath.Length > 0)
                    diff.CleanUpTempFiles(false, true);
                else
                    diff.CleanUpTempFiles(true, true);
            }
        }

        private void ComparisonForm_Load(object sender, EventArgs e)
        {
            this.diff = new SqlUnifiedDiff();

            if (this.leftTempFilePath.Length > 0 && this.leftZipFilePath.Length > 0)
            {
                txtLeftFile.Text = this.leftZipFilePath;
                btnSelectFile1.Enabled = false;
                txtLeftFile.Enabled = false;
            }

            if (this.rightZipFilePath.Length > 0)
                txtRightFile.Text = this.rightZipFilePath;

            if (this.buildData == null)
                lstFiles.ContextMenuStrip = null;
            
            this.Show();

            if (this.startImmediateCompare)
                btnCompare_Click(null, EventArgs.Empty);

            this.finderSingle.AddControlToSearch(this.rtbResultsHighlight);
        }

        private void mnuImportScripts_Click(object sender, EventArgs e)
        {
            if (lstFiles.SelectedItems.Count == 0)
                return;

            List<string> files = new List<string>();
            for (int i = 0; i < lstFiles.SelectedItems.Count; i++)
            {
                if (lstFiles.SelectedItems[i].SubItems[3].Text.Length > 0)
                    files.Add(lstFiles.SelectedItems[i].SubItems[3].Text);
            }



            bool isValid;
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            SqlSync.Validator.SchemaValidator validator = new SqlSync.Validator.SchemaValidator();
            string importFileXml = string.Empty;
            if (File.Exists(this.rightTempFilePath + XmlFileNames.MainProjectFile))
            {
                importFileXml = this.rightTempFilePath + XmlFileNames.MainProjectFile;
            }
            string valErrorMessage;
            isValid = SqlBuildFileHelper.ValidateAgainstSchema(importFileXml, out valErrorMessage);
            if (isValid == false)
            {
                MessageBox.Show("Unable to Import. Invalid Build File Schema?");
                return;
            }

            SqlSyncBuildData importData;
            if (File.Exists(importFileXml))
            {
                importData= new SqlSyncBuildData();
                importData.ReadXml(this.rightTempFilePath + XmlFileNames.MainProjectFile);
                importData.AcceptChanges();

                foreach (SqlSyncBuildData.ScriptRow row in importData.Script)
                {
                    if (files.Contains(row.FileName) == false)
                        row.Delete();
                }
                importData.Script.AcceptChanges();
                importData.AcceptChanges();
            }
            else
                return;

            string[] arrFiles = new string[files.Count];
            files.CopyTo(arrFiles);

            SqlBuild.ImportListForm frmImport = new SqlBuild.ImportListForm(importData, this.rightTempFilePath, this.buildData, arrFiles);
            if (DialogResult.OK != frmImport.ShowDialog())
            {
                return;
            }

            string[] addedFileNames;
            double importStartNumber = SqlBuild.SqlBuildFileHelper.ImportSqlScriptFile(ref this.buildData,
                 importData, this.rightTempFilePath, lastBuildNumber, this.leftTempFilePath, this.leftTempFilePath + XmlFileNames.MainProjectFile, this.leftZipFilePath, false, out addedFileNames);


            if (importStartNumber > 0)
            {
                StringBuilder sb = new StringBuilder();
                sb.Append("Import Successful\r\n");
                sb.Append("New file indexes start at: " + importStartNumber.ToString() + "\r\n\r\n");
                MessageBox.Show(sb.ToString(), "Import Successful", MessageBoxButtons.OK, MessageBoxIcon.Information);
                this.refreshProjectList = true;
                this.lastBuildNumber = this.lastBuildNumber + addedFileNames.Length;
            }
            else
            {
                if (importStartNumber == (double)ImportFileStatus.Canceled)
                {
                    MessageBox.Show("Import Canceled", "Import Canceled", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (importStartNumber == (double)ImportFileStatus.UnableToImport)
                {
                    MessageBox.Show("Unable to Import the requested file", "Import Failed", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else if (importStartNumber == (double)ImportFileStatus.NoRowsImported)
                {
                    MessageBox.Show("No rows were selected for import", "None selected", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }


            btnCompare_Click(null, EventArgs.Empty);
        }

        private void ctxImport_Opening(object sender, CancelEventArgs e)
        {
            if (lstFiles.SelectedItems.Count == 0 || txtLeftFile.Enabled)
            {
                e.Cancel = true;
                return;
            }

            bool showRemove = true;
            bool showImport = true;
            bool showResolveConflict = true;
            for (int i = 0; i < lstFiles.SelectedItems.Count; i++)
            {
                if (lstFiles.SelectedItems[i].SubItems[1].Text.Length == 0)
                    showRemove = false;

                if (lstFiles.SelectedItems[i].SubItems[3].Text.Length == 0 || lstFiles.SelectedItems[i].SubItems[1].Text.Length > 0)
                    showImport = false;

                if (lstFiles.SelectedItems.Count > 1 || lstFiles.SelectedItems[0].ForeColor != Color.Red)
                    showResolveConflict = false;
            }

            removeSelectedScriptsFromTheLeftFileToolStripMenuItem.Enabled = showRemove;
            mnuImportScripts.Enabled = showImport;
            mnuMergeResolve.Enabled = showResolveConflict;
        }

        private void bgWorker_DoWork(object sender, DoWorkEventArgs e)
        {

            List<FileCompareResults> onlyInRight;
            List<FileCompareResults> onlyInLeft;
            List<FileCompareResults> modified;
            string rightTemp = string.Empty;

            //if (this.buildData == null)
            //{
            //    diff.GetUnifiedDiff(txtLeftFile.Text, txtRightFile.Text, out onlyInRight, out onlyInLeft, out modified);
            //}
            //else
            //{
                diff.GetUnifiedDiff(ref this.buildData, this.leftTempFilePath, txtRightFile.Text, out onlyInLeft, out onlyInRight, out modified, out rightTemp);
                this.rightTempFilePath = rightTemp;
            //}

            e.Result = new List<FileCompareResults>[3] { onlyInLeft, onlyInRight, modified };
        }

        private void bgWorker_ProgressChanged(object sender, ProgressChangedEventArgs e)
        {

        }

        private void bgWorker_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
        {
            int identical = 0;
            int diff = 0;
            List<FileCompareResults> onlyInLeft = ((List<FileCompareResults>[])e.Result)[0];
            List<FileCompareResults> onlyInRight = ((List<FileCompareResults>[])e.Result)[1];
            List<FileCompareResults> modified = ((List<FileCompareResults>[])e.Result)[2];


            //Add left only files to list.
            for(int i=0;i<onlyInLeft.Count;i++)
            {
                ListViewItem item = new ListViewItem(new string[] { onlyInLeft[i].LeftScriptRow.BuildOrder.ToString(),onlyInLeft[i].LeftScriptRow.Tag, onlyInLeft[i].LeftScriptRow.FileName, "","" });
                item.Tag = onlyInLeft[i];
                item.ForeColor = Color.Blue;
                lstFiles.Items.Add(item);
            }

            //Add right only files to list.
            for (int i = 0; i < onlyInRight.Count; i++)
            {
                ListViewItem item = new ListViewItem(new string[] { "", "", "", onlyInRight[i].RightScriptRow.BuildOrder.ToString(), onlyInRight[i].RightScriptRow.Tag, onlyInRight[i].RightScriptRow.FileName });
                item.Tag = onlyInRight[i];
                item.ForeColor = Color.Blue;
                lstFiles.Items.Add(item);
            }

            //Add common files and color code according to diff.
            for (int i = 0; i < modified.Count; i++)
            {
                ListViewItem item = new ListViewItem(new string[] { modified[i].LeftScriptRow.BuildOrder.ToString(),modified[i].LeftScriptRow.Tag, modified[i].LeftScriptRow.FileName, modified[i].RightScriptRow.BuildOrder.ToString(),modified[i].RightScriptRow.Tag, modified[i].RightScriptRow.FileName });
                item.Tag = modified[i];
                if (modified[i].UnifiedDiffText.Length > 0)
                {
                    item.ForeColor = Color.Red;
                    item.Font = new Font(item.Font, FontStyle.Bold);
                    diff++;
                }
                else
                {
                    item.ForeColor = Color.Black;
                    identical++;
                }

                lstFiles.Items.Add(item);
            }

            listSorter.CurrentColumn = 0;
            listSorter.Sort = SortOrder.Ascending;
            lstFiles.ListViewItemSorter = listSorter;
            lstFiles.Sort();

            this.Cursor = Cursors.Default;

            statDiffering.Text = ReplaceNumericValue(statDiffering.Text, diff);
            statIdentical.Text = ReplaceNumericValue(statIdentical.Text, identical);
            statOnlyInLeft.Text = ReplaceNumericValue(statOnlyInLeft.Text, onlyInLeft.Count);
            statOnlyInRight.Text = ReplaceNumericValue(statOnlyInRight.Text, onlyInRight.Count);
            statGeneral.Text = "Comparison Complete";


            pnlControls.Enabled = true;
        }


        private static string ReplaceNumericValue(string input, int newNumber)
        {
            Regex num = new Regex(@"\d{1,10}");
            return num.Replace(input, newNumber.ToString());
        }

        private void removeSelectedScriptsFromTheLeftFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //TODO: finish real delete!
            if (lstFiles.SelectedItems.Count == 0)
                return;

            this.Cursor = Cursors.WaitCursor;
            StringBuilder sb = new StringBuilder("Are you sure you want to remove the follow file(s)?\r\n\r\n"); ;
            for (int i = 0; i < lstFiles.SelectedItems.Count; i++)
                sb.Append("  " + ((FileCompareResults) lstFiles.SelectedItems[i].Tag).LeftScriptRow.FileName + "\r\n");


            if (DialogResult.No == MessageBox.Show(sb.ToString(), "Delete Confirmation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning))
            {
                this.Cursor = Cursors.Default;
                return;
            }


            //Get list of rows to remove then remove them.
            SqlSyncBuildData.ScriptRow[] rows = new SqlSyncBuildData.ScriptRow[lstFiles.SelectedItems.Count];
            for (int i = 0; i < lstFiles.SelectedItems.Count; i++)
            {
                FileCompareResults results = (FileCompareResults)lstFiles.SelectedItems[i].Tag;
                rows[i] = results.LeftScriptRow;
            }
            //TODO: Check for delete from an SBX file
            if(!SqlBuildFileHelper.RemoveScriptFilesFromBuild(ref this.buildData, Path.Combine(this.leftTempFilePath , XmlFileNames.MainProjectFile), this.leftZipFilePath, rows, true))
            {
                MessageBox.Show("Unable to remove file from list. Please try again", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }


            if (lstFiles.SelectedItems.Count > 0)
            {
                this.refreshProjectList = true;
            }


            btnCompare_Click(null, EventArgs.Empty);

            this.Cursor = Cursors.Default;
        }

        private void mnuMergeResolve_Click(object sender, EventArgs e)
        {
            lstFiles_DoubleClick(sender, e);
        }

        private void lstFiles_ColumnClick(object sender, ColumnClickEventArgs e)
        {
            listSorter.CurrentColumn = e.Column;
            lstFiles.ListViewItemSorter = listSorter;
            lstFiles.Sort();
        }
      




    }
}
