using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Collections.Specialized;
namespace SqlSync.SqlBuild
{
	/// <summary>
	/// Summary description for UtilityReplacement.
	/// </summary>
	public class UtilityReplacement : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private SqlSync.SqlBuild.UtilityReplacementUnit utilityReplacementUnit1;
		private System.Windows.Forms.Button btnSubmit;
		private string[] keyValues = null;
		public StringDictionary Replacements = new StringDictionary();
		System.Drawing.Point startLocation;
		private System.Windows.Forms.Label label2;
		private UrielGuy.SyntaxHighlighting.SyntaxHighlightingTextBox rtbSqlScript;
		private System.Windows.Forms.CheckBox chkInsert;
		private System.Windows.Forms.Panel panel1;
		private System.Windows.Forms.CheckBox chkClipBoard;
		private System.Windows.Forms.Panel pnlReplacements;
		private System.Windows.Forms.Splitter splitter1;
        private string inputText;
		private Size startSize;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		public UtilityReplacement(string[] keyValues, string title,string inputText)
		{
			InitializeComponent();
			this.keyValues = keyValues;
			this.Text += " :: "+title;
            this.inputText = inputText;

			startLocation = utilityReplacementUnit1.Location;
			startSize = utilityReplacementUnit1.Size;
			this.pnlReplacements.Controls.Remove(utilityReplacementUnit1);

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
            UrielGuy.SyntaxHighlighting.HighLightDescriptorCollection highLightDescriptorCollection1 = new UrielGuy.SyntaxHighlighting.HighLightDescriptorCollection();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UtilityReplacement));
            this.label1 = new System.Windows.Forms.Label();
            this.utilityReplacementUnit1 = new SqlSync.SqlBuild.UtilityReplacementUnit();
            this.btnSubmit = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.rtbSqlScript = new UrielGuy.SyntaxHighlighting.SyntaxHighlightingTextBox();
            this.chkInsert = new System.Windows.Forms.CheckBox();
            this.panel1 = new System.Windows.Forms.Panel();
            this.chkClipBoard = new System.Windows.Forms.CheckBox();
            this.pnlReplacements = new System.Windows.Forms.Panel();
            this.splitter1 = new System.Windows.Forms.Splitter();
            this.panel1.SuspendLayout();
            this.pnlReplacements.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(200, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(208, 24);
            this.label1.TabIndex = 0;
            this.label1.Text = "Add Replacement Values:";
            this.label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // utilityReplacementUnit1
            // 
            this.utilityReplacementUnit1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.utilityReplacementUnit1.FunctionKey = System.Windows.Forms.Keys.None;
            this.utilityReplacementUnit1.Key = null;
            this.utilityReplacementUnit1.Location = new System.Drawing.Point(8, 32);
            this.utilityReplacementUnit1.Name = "utilityReplacementUnit1";
            this.utilityReplacementUnit1.Size = new System.Drawing.Size(600, 26);
            this.utilityReplacementUnit1.TabIndex = 1;
            // 
            // btnSubmit
            // 
            this.btnSubmit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSubmit.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.btnSubmit.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.btnSubmit.Location = new System.Drawing.Point(529, 222);
            this.btnSubmit.Name = "btnSubmit";
            this.btnSubmit.Size = new System.Drawing.Size(75, 23);
            this.btnSubmit.TabIndex = 2;
            this.btnSubmit.Text = "Submit";
            this.btnSubmit.Click += new System.EventHandler(this.btnSubmit_Click);
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(8, 8);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(100, 16);
            this.label2.TabIndex = 4;
            this.label2.Text = "Scratch Pad:";
            // 
            // rtbSqlScript
            // 
            this.rtbSqlScript.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbSqlScript.CaseSensitive = false;
            this.rtbSqlScript.FilterAutoComplete = true;
            this.rtbSqlScript.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.rtbSqlScript.HighlightDescriptors = highLightDescriptorCollection1;
            this.rtbSqlScript.HighlightType = SqlSync.Highlighting.SyntaxHightlightType.Sql;
            this.rtbSqlScript.Location = new System.Drawing.Point(12, 32);
            this.rtbSqlScript.MaxUndoRedoSteps = 50;
            this.rtbSqlScript.Name = "rtbSqlScript";
            this.rtbSqlScript.Size = new System.Drawing.Size(592, 182);
            this.rtbSqlScript.TabIndex = 8;
            this.rtbSqlScript.Text = "";
            // 
            // chkInsert
            // 
            this.chkInsert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.chkInsert.Enabled = false;
            this.chkInsert.Location = new System.Drawing.Point(328, 222);
            this.chkInsert.Name = "chkInsert";
            this.chkInsert.Size = new System.Drawing.Size(192, 24);
            this.chkInsert.TabIndex = 9;
            this.chkInsert.Text = "Insert Scratch Pad Values";
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.chkClipBoard);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.rtbSqlScript);
            this.panel1.Controls.Add(this.chkInsert);
            this.panel1.Controls.Add(this.btnSubmit);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 64);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(616, 254);
            this.panel1.TabIndex = 10;
            // 
            // chkClipBoard
            // 
            this.chkClipBoard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.chkClipBoard.Checked = true;
            this.chkClipBoard.CheckState = System.Windows.Forms.CheckState.Checked;
            this.chkClipBoard.Location = new System.Drawing.Point(16, 222);
            this.chkClipBoard.Name = "chkClipBoard";
            this.chkClipBoard.Size = new System.Drawing.Size(248, 24);
            this.chkClipBoard.TabIndex = 10;
            this.chkClipBoard.Text = "Add Scratch Pad to Clipboard on Close";
            // 
            // pnlReplacements
            // 
            this.pnlReplacements.Controls.Add(this.label1);
            this.pnlReplacements.Controls.Add(this.utilityReplacementUnit1);
            this.pnlReplacements.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlReplacements.Location = new System.Drawing.Point(0, 0);
            this.pnlReplacements.Name = "pnlReplacements";
            this.pnlReplacements.Size = new System.Drawing.Size(616, 64);
            this.pnlReplacements.TabIndex = 11;
            // 
            // splitter1
            // 
            this.splitter1.BackColor = System.Drawing.Color.LightGray;
            this.splitter1.Dock = System.Windows.Forms.DockStyle.Top;
            this.splitter1.Location = new System.Drawing.Point(0, 64);
            this.splitter1.Name = "splitter1";
            this.splitter1.Size = new System.Drawing.Size(616, 3);
            this.splitter1.TabIndex = 12;
            this.splitter1.TabStop = false;
            // 
            // UtilityReplacement
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(616, 318);
            this.Controls.Add(this.splitter1);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.pnlReplacements);
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "UtilityReplacement";
            this.Text = "Utility Scripts Replacements";
            this.Closing += new System.ComponentModel.CancelEventHandler(this.UtilityReplacement_Closing);
            this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.UtilityReplacement_KeyDown);
            this.Load += new System.EventHandler(this.UtilityReplacement_Load);
            this.panel1.ResumeLayout(false);
            this.pnlReplacements.ResumeLayout(false);
            this.ResumeLayout(false);

		}
		#endregion

		private void UtilityReplacement_Load(object sender, System.EventArgs e)
		{

			//F1 int value = 112
			int functionKey = 112;
			for(int i=0;i<this.keyValues.Length;i++)
			{
				if(this.keyValues[i].ToUpper() != "<<INSERT>>")
				{
					UtilityReplacementUnit unit = new UtilityReplacementUnit(this.keyValues[i],(Keys)functionKey);
					unit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
						| System.Windows.Forms.AnchorStyles.Right)));
					unit.Size = startSize;
					functionKey++;
					unit.Location = startLocation;
					this.pnlReplacements.Controls.Add(unit);
					if(i>0)
					{
						this.Height += unit.Height;
						this.pnlReplacements.Height +=unit.Height;
					}
					startLocation = new Point(startLocation.X,startLocation.Y+unit.Height);
				}
				else
				{
					chkInsert.Checked = true;
					chkInsert.Enabled = true;
				}

			}

            if (this.inputText.Length == 0)
            {
                IDataObject ido = Clipboard.GetDataObject();
                this.rtbSqlScript.Text = (string)ido.GetData(DataFormats.Text);
            }
            else
            {
                this.rtbSqlScript.Text = this.inputText;
                this.chkClipBoard.Checked = false;
            }
		}

		private void btnSubmit_Click(object sender, System.EventArgs e)
		{
			this.Replacements = new StringDictionary();
			foreach(Control ctrl in this.pnlReplacements.Controls)
			{
				if(ctrl is UtilityReplacementUnit)
				{
					UtilityReplacementUnit unit = (UtilityReplacementUnit)ctrl;
					this.Replacements.Add(unit.Key,unit.Value);
				}
			}

			if(this.chkInsert.Checked)
				this.Replacements.Add("<<INSERT>>",this.rtbSqlScript.Text);
			else
				this.Replacements.Add("<<INSERT>>","");


			this.DialogResult = DialogResult.OK;
			this.Close();
		}

		private void UtilityReplacement_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if(this.chkClipBoard.Checked)
				Clipboard.SetDataObject(this.rtbSqlScript.Text);
		}

		private void UtilityReplacement_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
		{
			//F1 == 112 --> F12 == 123
			if((int)e.KeyCode >=112 && (int)e.KeyCode <= 123)
			{
				string selected = this.rtbSqlScript.SelectedText.Trim();
				foreach(Control ctrl in this.pnlReplacements.Controls)
				{
					if(ctrl is UtilityReplacementUnit && ((UtilityReplacementUnit)ctrl).FunctionKey == e.KeyCode)
					{
						((UtilityReplacementUnit)ctrl).Text = selected;
						break;
					}
				}
			}

            if(e.KeyData == Keys.Escape)
            {
                this.DialogResult = DialogResult.Cancel;
                this.Close();
            }
		}

		

		
	}
}
