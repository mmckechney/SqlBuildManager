using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;
using System.IO;
namespace SQLSync
{
	/// <summary>
	/// Summary description for PopulateScriptDisplay.
	/// </summary>
	public class PopulateScriptDisplay : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.RichTextBox rtbScripts;
		private System.Windows.Forms.LinkLabel lnkCopy;
		private System.Windows.Forms.LinkLabel lnkSave;
		private System.Windows.Forms.SaveFileDialog saveFileDialog1;
		private string scriptName = string.Empty;
		private DataTable scriptDataTable = null;
		private System.Windows.Forms.LinkLabel lnkViewSwap;
		private System.Windows.Forms.DataGrid dgTableView;
		private string _selectStatement;
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label lblRecordsScripted;

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
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
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
					this.lnkViewSwap.Visible = true;
					this.lnkViewSwap.Text = SwapLinkText.ShowScript;
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


		public PopulateScriptDisplay(TableScriptData data)
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
			this.ScriptDataTable = data.ValuesTable;
			this.ScriptText = data.InsertScript;
			this.ScriptName = data.TableName;
			this.SelectStatement = data.SelectStatement;
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
			this.rtbScripts = new System.Windows.Forms.RichTextBox();
			this.lnkCopy = new System.Windows.Forms.LinkLabel();
			this.lnkSave = new System.Windows.Forms.LinkLabel();
			this.saveFileDialog1 = new System.Windows.Forms.SaveFileDialog();
			this.lnkViewSwap = new System.Windows.Forms.LinkLabel();
			this.dgTableView = new System.Windows.Forms.DataGrid();
			this.label1 = new System.Windows.Forms.Label();
			this.lblRecordsScripted = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.dgTableView)).BeginInit();
			this.SuspendLayout();
			// 
			// rtbScripts
			// 
			this.rtbScripts.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.rtbScripts.Location = new System.Drawing.Point(8, 32);
			this.rtbScripts.Name = "rtbScripts";
			this.rtbScripts.Size = new System.Drawing.Size(712, 472);
			this.rtbScripts.TabIndex = 0;
			this.rtbScripts.Text = "";
			// 
			// lnkCopy
			// 
			this.lnkCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lnkCopy.Location = new System.Drawing.Point(472, 8);
			this.lnkCopy.Name = "lnkCopy";
			this.lnkCopy.Size = new System.Drawing.Size(136, 16);
			this.lnkCopy.TabIndex = 1;
			this.lnkCopy.TabStop = true;
			this.lnkCopy.Text = "Copy Script to Clip Board";
			this.lnkCopy.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkCopy_LinkClicked);
			// 
			// lnkSave
			// 
			this.lnkSave.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lnkSave.Location = new System.Drawing.Point(616, 8);
			this.lnkSave.Name = "lnkSave";
			this.lnkSave.Size = new System.Drawing.Size(104, 16);
			this.lnkSave.TabIndex = 2;
			this.lnkSave.TabStop = true;
			this.lnkSave.Text = "Save Script to File";
			this.lnkSave.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkSave_LinkClicked);
			// 
			// saveFileDialog1
			// 
			this.saveFileDialog1.DefaultExt = "sql";
			this.saveFileDialog1.Filter = "SQL Files|*.sql|All Files|*.*";
			// 
			// lnkViewSwap
			// 
			this.lnkViewSwap.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.lnkViewSwap.Location = new System.Drawing.Point(360, 8);
			this.lnkViewSwap.Name = "lnkViewSwap";
			this.lnkViewSwap.Size = new System.Drawing.Size(100, 16);
			this.lnkViewSwap.TabIndex = 3;
			this.lnkViewSwap.TabStop = true;
			this.lnkViewSwap.Text = "Show Script View";
			this.lnkViewSwap.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkViewSwap_LinkClicked);
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
			this.label1.Location = new System.Drawing.Point(32, 8);
			this.label1.Name = "label1";
			this.label1.Size = new System.Drawing.Size(100, 16);
			this.label1.TabIndex = 5;
			this.label1.Text = "Records Scripted:";
			// 
			// lblRecordsScripted
			// 
			this.lblRecordsScripted.Location = new System.Drawing.Point(128, 8);
			this.lblRecordsScripted.Name = "lblRecordsScripted";
			this.lblRecordsScripted.Size = new System.Drawing.Size(88, 16);
			this.lblRecordsScripted.TabIndex = 6;
			// 
			// PopulateScriptDisplay
			// 
			this.Controls.Add(this.lblRecordsScripted);
			this.Controls.Add(this.label1);
			this.Controls.Add(this.lnkViewSwap);
			this.Controls.Add(this.lnkSave);
			this.Controls.Add(this.lnkCopy);
			this.Controls.Add(this.dgTableView);
			this.Controls.Add(this.rtbScripts);
			this.Name = "PopulateScriptDisplay";
			this.Size = new System.Drawing.Size(728, 512);
			this.Load += new System.EventHandler(this.PopulateScriptDisplay_Load);
			((System.ComponentModel.ISupportInitialize)(this.dgTableView)).EndInit();
			this.ResumeLayout(false);

		}
		#endregion

		private void lnkCopy_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			Clipboard.SetDataObject(rtbScripts.Text,true);
		}

		private void lnkSave_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			saveFileDialog1.DefaultExt = ".sql";
			saveFileDialog1.FileName = this.ScriptName;
			DialogResult result = saveFileDialog1.ShowDialog();
			if(result == DialogResult.OK)
			{
				SaveScriptToDisk(saveFileDialog1.FileName);
			}
		}
		public bool SaveScript(string directoryPath)
		{
			string name = directoryPath +@"\"+this.scriptName+".sql";
			return SaveScriptToDisk(name);
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

		private void lnkViewSwap_LinkClicked(object sender, System.Windows.Forms.LinkLabelLinkClickedEventArgs e)
		{
			
			if(this.scriptDataTable != null && lnkViewSwap.Text == SwapLinkText.ShowTable)
			{
				dgTableView.BringToFront();
				lnkViewSwap.Text = SwapLinkText.ShowScript;
			}
			else
			{
				rtbScripts.BringToFront();
				lnkViewSwap.Text = SwapLinkText.ShowTable;
			}
		}

		private void PopulateScriptDisplay_Load(object sender, System.EventArgs e)
		{
			if(this.scriptDataTable != null)
			{
				this.lnkViewSwap.Visible = true;
				this.lnkViewSwap.Text = SwapLinkText.ShowScript;
				this.dgTableView.Invalidate();
			}
		}
	}
}
