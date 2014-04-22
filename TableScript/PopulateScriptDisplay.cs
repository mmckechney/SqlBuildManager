using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.IO;
using SqlSync.Constants;
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
		private System.Windows.Forms.DataGrid dgTableView;
		private string _selectStatement;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label lblRecordsScripted;
		private System.Windows.Forms.ToolBar toolBar1;
		private System.Windows.Forms.ImageList imageList1;
		private System.Windows.Forms.ToolBarButton tbbSwap;
		private System.Windows.Forms.ToolBarButton tbbSyntax;
		private System.Windows.Forms.ToolBarButton tbbCopy;
		private System.Windows.Forms.ToolBarButton tbbSave;
		private System.ComponentModel.IContainer components;
		private bool showScriptExport;

		public string SelectStatement
		{
			get {  return _selectStatement; }
			set 
			{
				_selectStatement = value;
				if(this._selectStatement.Length > 0)
				{
					this.dgTableView.CaptionText = this._selectStatement;
				}
			}
		}
		public DataTable ScriptDataTable
		{
			get
			{
				return this.scriptDataTable;
			}
			set
			{
				this.scriptDataTable = value;
				if(this.scriptDataTable != null)
				{
					dgTableView.DataSource = this.scriptDataTable;
					dgTableView.BringToFront();
					this.lblRecordsScripted.Text = this.scriptDataTable.Rows.Count.ToString();
				}
			}
		}
		public string ScriptText
		{
			get
			{
				return this.rtbScripts.Text;
			}
			set
			{
				this.rtbScripts.Text = value;
			}
		}
		public string ScriptName
		{
			get
			{
				return this.scriptName;
			}
			set
			{
				this.scriptName = value;
			}
		}


		public PopulateScriptDisplay(TableScriptData data,bool showScriptExport)
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
			this.ScriptDataTable = data.ValuesTable;
			this.ScriptText = data.InsertScript;
			this.ScriptName = data.TableName;
			this.SelectStatement = data.SelectStatement;
			this.showScriptExport = showScriptExport;
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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            UrielGuy.SyntaxHighlighting.HighLightDescriptorCollection highLightDescriptorCollection1 = new UrielGuy.SyntaxHighlighting.HighLightDescriptorCollection();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PopulateScriptDisplay));
            this.rtbScripts = new UrielGuy.SyntaxHighlighting.SyntaxHighlightingTextBox();
            this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
            this.dgTableView = new System.Windows.Forms.DataGrid();
            this.label1 = new System.Windows.Forms.Label();
            this.lblRecordsScripted = new System.Windows.Forms.Label();
            this.toolBar1 = new System.Windows.Forms.ToolBar();
            this.tbbSwap = new System.Windows.Forms.ToolBarButton();
            this.tbbSyntax = new System.Windows.Forms.ToolBarButton();
            this.tbbCopy = new System.Windows.Forms.ToolBarButton();
            this.tbbSave = new System.Windows.Forms.ToolBarButton();
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            ((System.ComponentModel.ISupportInitialize)(this.dgTableView)).BeginInit();
            this.SuspendLayout();
            // 
            // rtbScripts
            // 
            this.rtbScripts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbScripts.CaseSensitive = false;
            this.rtbScripts.FilterAutoComplete = true;
            this.rtbScripts.HighlightDescriptors = highLightDescriptorCollection1;
            this.rtbScripts.HighlightType = SqlSync.Highlighting.SyntaxHightlightType.Sql;
            this.rtbScripts.Location = new System.Drawing.Point(8, 32);
            this.rtbScripts.MaxUndoRedoSteps = 50;
            this.rtbScripts.Name = "rtbScripts";
            this.rtbScripts.Size = new System.Drawing.Size(712, 472);
            this.rtbScripts.SuspendHighlighting = false;
            this.rtbScripts.TabIndex = 0;
            this.rtbScripts.Text = "";
            // 
            // saveFileDialog1
            // 
            this.saveFileDialog1.DefaultExt = "sql";
            this.saveFileDialog1.Filter = "SQL Files|*.sql|All Files|*.*";
            // 
            // dgTableView
            // 
            this.dgTableView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dgTableView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.dgTableView.CaptionFont = new System.Drawing.Font("Microsoft Sans Serif", 7F);
            this.dgTableView.DataMember = "";
            this.dgTableView.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dgTableView.Location = new System.Drawing.Point(8, 32);
            this.dgTableView.Name = "dgTableView";
            this.dgTableView.PreferredColumnWidth = 80;
            this.dgTableView.ReadOnly = true;
            this.dgTableView.Size = new System.Drawing.Size(712, 472);
            this.dgTableView.TabIndex = 4;
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(8, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(104, 16);
            this.label1.TabIndex = 5;
            this.label1.Text = "Records Scripted:";
            // 
            // lblRecordsScripted
            // 
            this.lblRecordsScripted.Location = new System.Drawing.Point(112, 8);
            this.lblRecordsScripted.Name = "lblRecordsScripted";
            this.lblRecordsScripted.Size = new System.Drawing.Size(88, 16);
            this.lblRecordsScripted.TabIndex = 6;
            // 
            // toolBar1
            // 
            this.toolBar1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.toolBar1.Appearance = System.Windows.Forms.ToolBarAppearance.Flat;
            this.toolBar1.Buttons.AddRange(new System.Windows.Forms.ToolBarButton[] {
            this.tbbSwap,
            this.tbbSyntax,
            this.tbbCopy,
            this.tbbSave});
            this.toolBar1.Dock = System.Windows.Forms.DockStyle.None;
            this.toolBar1.DropDownArrows = true;
            this.toolBar1.ImageList = this.imageList1;
            this.toolBar1.Location = new System.Drawing.Point(618, 2);
            this.toolBar1.Name = "toolBar1";
            this.toolBar1.ShowToolTips = true;
            this.toolBar1.Size = new System.Drawing.Size(100, 28);
            this.toolBar1.TabIndex = 8;
            this.toolBar1.ButtonClick += new System.Windows.Forms.ToolBarButtonClickEventHandler(this.toolBar1_ButtonClick);
            // 
            // tbbSwap
            // 
            this.tbbSwap.ImageIndex = 0;
            this.tbbSwap.Name = "tbbSwap";
            this.tbbSwap.Style = System.Windows.Forms.ToolBarButtonStyle.ToggleButton;
            this.tbbSwap.Tag = "Swap";
            this.tbbSwap.ToolTipText = "Swap View";
            // 
            // tbbSyntax
            // 
            this.tbbSyntax.ImageIndex = 4;
            this.tbbSyntax.Name = "tbbSyntax";
            this.tbbSyntax.Tag = "Syntax";
            this.tbbSyntax.ToolTipText = "Apply Syntax Coloring";
            // 
            // tbbCopy
            // 
            this.tbbCopy.ImageIndex = 1;
            this.tbbCopy.Name = "tbbCopy";
            this.tbbCopy.Tag = "Copy";
            this.tbbCopy.ToolTipText = "Copy to Clipboard";
            // 
            // tbbSave
            // 
            this.tbbSave.ImageIndex = 2;
            this.tbbSave.Name = "tbbSave";
            this.tbbSave.Tag = "Save";
            this.tbbSave.ToolTipText = "Save to File";
            // 
            // imageList1
            // 
            this.imageList1.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList1.ImageStream")));
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList1.Images.SetKeyName(0, "");
            this.imageList1.Images.SetKeyName(1, "Cut-2.png");
            this.imageList1.Images.SetKeyName(2, "Save.png");
            this.imageList1.Images.SetKeyName(3, "");
            this.imageList1.Images.SetKeyName(4, "");
            this.imageList1.Images.SetKeyName(5, "");
            // 
            // PopulateScriptDisplay
            // 
            this.Controls.Add(this.lblRecordsScripted);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.dgTableView);
            this.Controls.Add(this.rtbScripts);
            this.Controls.Add(this.toolBar1);
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Name = "PopulateScriptDisplay";
            this.Size = new System.Drawing.Size(728, 512);
            this.Load += new System.EventHandler(this.PopulateScriptDisplay_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dgTableView)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

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
			string name = directoryPath +@"\"+this.scriptName+SqlSync.Constants.DbObjectType.PopulateScript.ToLower();
			//string name = directoryPath +@"\"+this.scriptName+".sql";
			if( SaveScriptToDisk(name))
				return name;
			else
				return string.Empty;
		}
		private bool SaveScriptToDisk(string fileName)
		{
			using(StreamWriter sw = new StreamWriter(fileName,false))
			{
				sw.WriteLine(this.ScriptText);
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
			if(this.scriptDataTable != null)
			{
				this.dgTableView.Invalidate();
			}
		}

//		private void lnkSyntaxHighlight_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
//		{
//			this.Cursor = Cursors.WaitCursor;
//			this.rtbScripts.RefreshHighlighting();
//			this.Cursor = Cursors.Default;
//		}

		private void toolBar1_ButtonClick(object sender, System.Windows.Forms.ToolBarButtonClickEventArgs e)
		{
			switch(e.Button.Tag.ToString().ToLower())
			{
				case "swap":
					if(e.Button.Pushed == false)
					{
						dgTableView.BringToFront();
					}
					else
					{
						rtbScripts.BringToFront();
					}
					break;
				case "save":
					saveFileDialog1.DefaultExt = ".sql";
					saveFileDialog1.FileName = this.ScriptName;
					DialogResult result = saveFileDialog1.ShowDialog();
					if(result == DialogResult.OK)
					{
						SaveScriptToDisk(saveFileDialog1.FileName);
					}
					break;
				case "copy":
					Clipboard.SetDataObject(rtbScripts.Text,true);
					break;
				case "syntax":
					this.Cursor = Cursors.WaitCursor;
					this.rtbScripts.RefreshHighlighting();
					this.Cursor = Cursors.Default;
					break;
				case "export":
					string fileName = Path.GetTempPath()+ this.ScriptName+SqlSync.Constants.DbObjectType.PopulateScript.ToLower();
					SaveScriptToDisk(fileName);
					if(this.SqlBuildManagerFileExport != null)
						this.SqlBuildManagerFileExport(this,new SqlBuildManagerFileExportEventArgs(new string[]{fileName}));
					break;

			}
		}
		public event SqlBuildManagerFileExportHandler SqlBuildManagerFileExport;

	}
}
