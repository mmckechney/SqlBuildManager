using System.Data;
using System.IO;
using System.Windows.Forms;
namespace SqlSync
{
    /// <summary>
    /// Summary description for PopulateScriptDisplay.
    /// </summary>
    public class PopulateScriptDisplay : System.Windows.Forms.UserControl
    {
        private UrielGuy.SyntaxHighlighting.SyntaxHighlightingTextBox rtbScripts;
        private System.Windows.Forms.SaveFileDialog saveFileDialog1;
        private string scriptName = string.Empty;
        private DataTable scriptDataTable = null;
        private System.Windows.Forms.DataGridView dgTableView;
        private string _selectStatement;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblRecordsScripted;
        private System.Windows.Forms.ToolStrip toolBar1;
        private System.Windows.Forms.ImageList imageList1;
        private System.Windows.Forms.ToolStripButton tbbSwap;
        private System.Windows.Forms.ToolStripButton tbbSyntax;
        private System.Windows.Forms.ToolStripButton tbbCopy;
        private System.Windows.Forms.ToolStripButton tbbSave;
        private System.ComponentModel.IContainer components;
        private bool showScriptExport;

        public string SelectStatement
        {
            get { return _selectStatement; }
            set
            {
                _selectStatement = value;
                if (_selectStatement.Length > 0)
                {
                    //this.dgTableView.CaptionText = this._selectStatement;
                }
            }
        }
        public DataTable ScriptDataTable
        {
            get
            {
                return scriptDataTable;
            }
            set
            {
                scriptDataTable = value;
                if (scriptDataTable != null)
                {
                    dgTableView.DataSource = scriptDataTable;
                    dgTableView.BringToFront();
                    lblRecordsScripted.Text = scriptDataTable.Rows.Count.ToString();
                }
            }
        }
        public string ScriptText
        {
            get
            {
                return rtbScripts.Text;
            }
            set
            {
                rtbScripts.Text = value;
            }
        }
        public string ScriptName
        {
            get
            {
                return scriptName;
            }
            set
            {
                scriptName = value;
            }
        }


        public PopulateScriptDisplay(TableScriptData data, bool showScriptExport)
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
            ScriptDataTable = data.ValuesTable;
            ScriptText = data.InsertScript;
            ScriptName = data.TableName;
            SelectStatement = data.SelectStatement;
            this.showScriptExport = showScriptExport;
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

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            UrielGuy.SyntaxHighlighting.HighLightDescriptorCollection highLightDescriptorCollection1 = new UrielGuy.SyntaxHighlighting.HighLightDescriptorCollection();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PopulateScriptDisplay));
            rtbScripts = new UrielGuy.SyntaxHighlighting.SyntaxHighlightingTextBox();
            saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            dgTableView = new System.Windows.Forms.DataGridView();
            label1 = new System.Windows.Forms.Label();
            lblRecordsScripted = new System.Windows.Forms.Label();
            toolBar1 = new System.Windows.Forms.ToolStrip();
            tbbSwap = new System.Windows.Forms.ToolStripButton();
            tbbSyntax = new System.Windows.Forms.ToolStripButton();
            tbbCopy = new System.Windows.Forms.ToolStripButton();
            tbbSave = new System.Windows.Forms.ToolStripButton();
            imageList1 = new System.Windows.Forms.ImageList(components);
            ((System.ComponentModel.ISupportInitialize)(dgTableView)).BeginInit();
            SuspendLayout();
            // 
            // rtbScripts
            // 
            rtbScripts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            rtbScripts.CaseSensitive = false;
            rtbScripts.FilterAutoComplete = true;
            rtbScripts.HighlightDescriptors = highLightDescriptorCollection1;
            rtbScripts.HighlightType = SqlSync.Highlighting.SyntaxHightlightType.Sql;
            rtbScripts.Location = new System.Drawing.Point(8, 32);
            rtbScripts.MaxUndoRedoSteps = 50;
            rtbScripts.Name = "rtbScripts";
            rtbScripts.Size = new System.Drawing.Size(712, 472);
            rtbScripts.SuspendHighlighting = false;
            rtbScripts.TabIndex = 0;
            rtbScripts.Text = "";
            // 
            // saveFileDialog1
            // 
            saveFileDialog1.DefaultExt = "sql";
            saveFileDialog1.Filter = "SQL Files|*.sql|All Files|*.*";
            // 
            // dgTableView
            // 
            dgTableView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            dgTableView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            //this.dgTableView.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            dgTableView.DataMember = "";
            //this.dgTableView.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            dgTableView.Location = new System.Drawing.Point(8, 32);
            dgTableView.Name = "dgTableView";
            //this.dgTableView.PreferredColumnWidth = 80;
            dgTableView.ReadOnly = true;
            dgTableView.Size = new System.Drawing.Size(712, 472);
            dgTableView.TabIndex = 4;
            // 
            // label1
            // 
            label1.Location = new System.Drawing.Point(8, 8);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(104, 16);
            label1.TabIndex = 5;
            label1.Text = "Records Scripted:";
            // 
            // lblRecordsScripted
            // 
            lblRecordsScripted.Location = new System.Drawing.Point(112, 8);
            lblRecordsScripted.Name = "lblRecordsScripted";
            lblRecordsScripted.Size = new System.Drawing.Size(88, 16);
            lblRecordsScripted.TabIndex = 6;
            // 
            // toolBar1
            // 
            toolBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            //this.toolBar1.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
            toolBar1.Items.AddRange(new System.Windows.Forms.ToolStripButton[] {
            tbbSwap,
            tbbSyntax,
            tbbCopy,
            tbbSave});
            toolBar1.Dock = System.Windows.Forms.DockStyle.None;
            //this.toolBar1.DropDownArrows = true;
            toolBar1.ImageList = imageList1;
            toolBar1.Location = new System.Drawing.Point(618, 2);
            toolBar1.Name = "toolBar1";
            //this.toolBar1.ShowToolTips = true;
            toolBar1.Size = new System.Drawing.Size(100, 28);
            toolBar1.TabIndex = 8;
            toolBar1.ItemClicked += new System.Windows.Forms.ToolStripItemClickedEventHandler(toolBar1_ButtonClick);
            // 
            // tbbSwap
            // 
            tbbSwap.ImageIndex = 0;
            tbbSwap.Name = "tbbSwap";
            //this.tbbSwap.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
            tbbSwap.Tag = "Swap";
            tbbSwap.ToolTipText = "Swap View";
            // 
            // tbbSyntax
            // 
            tbbSyntax.ImageIndex = 4;
            tbbSyntax.Name = "tbbSyntax";
            tbbSyntax.Tag = "Syntax";
            tbbSyntax.ToolTipText = "Apply Syntax Coloring";
            // 
            // tbbCopy
            // 
            tbbCopy.ImageIndex = 1;
            tbbCopy.Name = "tbbCopy";
            tbbCopy.Tag = "Copy";
            tbbCopy.ToolTipText = "Copy to Clipboard";
            // 
            // tbbSave
            // 
            tbbSave.ImageIndex = 2;
            tbbSave.Name = "tbbSave";
            tbbSave.Tag = "Save";
            tbbSave.ToolTipText = "Save to File";
            // 
            // imageList1
            // 
            imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            imageList1.TransparentColor = System.Drawing.Color.Transparent;
            imageList1.Images.SetKeyName(0, "");
            imageList1.Images.SetKeyName(1, "Cut-2.png");
            imageList1.Images.SetKeyName(2, "Save.png");
            imageList1.Images.SetKeyName(3, "");
            imageList1.Images.SetKeyName(4, "");
            imageList1.Images.SetKeyName(5, "");
            // 
            // PopulateScriptDisplay
            // 
            Controls.Add(lblRecordsScripted);
            Controls.Add(label1);
            Controls.Add(dgTableView);
            Controls.Add(rtbScripts);
            Controls.Add(toolBar1);
            Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            Name = "PopulateScriptDisplay";
            Size = new System.Drawing.Size(728, 512);
            Load += new System.EventHandler(PopulateScriptDisplay_Load);
            ((System.ComponentModel.ISupportInitialize)(dgTableView)).EndInit();
            ResumeLayout(false);
            PerformLayout();

        }
        #endregion



        //		private void lnkSave_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        //		{
        //			saveFileDialog1.DefaultExt = ".sql";
        //			saveFileDialog1.FileName = this.ScriptName;
        //			DialogResult result = saveFileDialog1.ShowDialog();
        //			if(result == DialogResult.OK)
        //			{
        //				SaveScriptToDisk(saveFileDialog1.FileName);
        //			}
        //		}
        public string SaveScript(string directoryPath)
        {
            string name = Path.Combine(directoryPath, scriptName + SqlSync.Constants.DbObjectType.PopulateScript.ToLower());

            if (SaveScriptToDisk(name))
                return name;
            else
                return string.Empty;
        }
        private bool SaveScriptToDisk(string fileName)
        {
            using (StreamWriter sw = new StreamWriter(fileName, false))
            {
                sw.WriteLine(ScriptText);
                sw.Flush();
                sw.Close();
            }
            return true;
        }

        //		private void lnkViewSwap_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        //		{
        //			
        //			if(this.scriptDataTable != null && lnkViewSwap.Text == SwapLinkText.ShowTable)
        //			{
        //				dgTableView.BringToFront();
        //				lnkViewSwap.Text = SwapLinkText.ShowScript;
        //				this.lnkSyntaxHighlight.Visible = false;
        //			}
        //			else
        //			{
        //				rtbScripts.BringToFront();
        //				lnkViewSwap.Text = SwapLinkText.ShowTable;
        //				this.lnkSyntaxHighlight.Visible = true;
        //			}
        //		}

        private void PopulateScriptDisplay_Load(object sender, System.EventArgs e)
        {
            if (scriptDataTable != null)
            {
                dgTableView.Invalidate();
            }
        }

        //		private void lnkSyntaxHighlight_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
        //		{
        //			this.Cursor = Cursors.WaitCursor;
        //			this.rtbScripts.RefreshHighlighting();
        //			this.Cursor = Cursors.Default;
        //		}
        bool swapView = false;
        private void toolBar1_ButtonClick(object sender, System.Windows.Forms.ToolStripItemClickedEventArgs e)
        {
            switch (e.ClickedItem.Tag.ToString().ToLower())
            {
                case "swap":
                    if (swapView == false)
                    {
                        dgTableView.BringToFront();
                        swapView = true;
                    }
                    else
                    {
                        rtbScripts.BringToFront();
                        swapView = false;
                    }
                    break;
                case "save":
                    saveFileDialog1.DefaultExt = ".sql";
                    saveFileDialog1.FileName = ScriptName;
                    DialogResult result = saveFileDialog1.ShowDialog();
                    if (result == DialogResult.OK)
                    {
                        SaveScriptToDisk(saveFileDialog1.FileName);
                    }
                    break;
                case "copy":
                    Clipboard.SetDataObject(rtbScripts.Text, true);
                    break;
                case "syntax":
                    Cursor = Cursors.WaitCursor;
                    rtbScripts.RefreshHighlighting();
                    Cursor = Cursors.Default;
                    break;
                case "export":
                    string fileName = Path.GetTempPath() + ScriptName + SqlSync.Constants.DbObjectType.PopulateScript.ToLower();
                    SaveScriptToDisk(fileName);
                    if (SqlBuildManagerFileExport != null)
                        SqlBuildManagerFileExport(this, new SqlBuildManagerFileExportEventArgs(new string[] { fileName }));
                    break;

            }
        }
        public event SqlBuildManagerFileExportHandler SqlBuildManagerFileExport;

    }
}
