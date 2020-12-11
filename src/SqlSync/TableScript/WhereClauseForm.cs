using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using SqlSync.Connection;
using SqlSync.TableScript;
namespace SqlSync
{
	/// <summary>
	/// Summary description for WhereClauseForm.
	/// </summary>
	public class WhereClauseForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.RichTextBox rtbWhereClause;
		private System.Windows.Forms.ListView lstColumns;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private ConnectionData data;
		private string tableName;
		private System.Windows.Forms.CheckBox chkUseFullSelect;
		private System.Windows.Forms.Label label2;
		private string[] checkKeyColumns;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.ListView lstUpdateKeyColumns;
		private System.Windows.Forms.ColumnHeader columnHeader3;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		public string WhereClause
		{
			get
			{
				string clause = rtbWhereClause.Text.Replace("\"","'");
				if(clause.Length > 0 &&  
					clause.Trim().ToUpper().StartsWith("WHERE") == false && 
					chkUseFullSelect.Checked == false)
				{
					return " WHERE "+clause;
				}
				else
				{
					return clause;
				}
			}
		}
		public bool UseAsFullSelect
		{
			get
			{
				return this.chkUseFullSelect.Checked;
			}
		}
		public string[] CheckKeyColumns
		{
			get
			{
				string[] items = new string[this.lstUpdateKeyColumns.CheckedItems.Count];
				for(int i=0;i<items.Length;i++)
				{
					items[i] = this.lstUpdateKeyColumns.CheckedItems[i].Text;
				}
				return items;
			}
		}

		public WhereClauseForm(ConnectionData data, string sourceTableName,string existingQuery,bool useAsFullSelect, string[] checkKeyColumns)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.data = data;
			this.Text = this.Text += " "+ sourceTableName;
			this.rtbWhereClause.Text = existingQuery;
			this.tableName = sourceTableName;
			this.chkUseFullSelect.Checked = useAsFullSelect;
			this.checkKeyColumns = checkKeyColumns;

			//this.rtbWhereClause.DragEnter += new System.Windows.Forms.DragEventHandler(this.richTextBox1_DragEnter);
			//this.rtbWhereClause.DragDrop += new System.Windows.Forms.DragEventHandler(this.richTextBox1_DragDrop);

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WhereClauseForm));
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.rtbWhereClause = new System.Windows.Forms.RichTextBox();
            this.lstColumns = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.chkUseFullSelect = new System.Windows.Forms.CheckBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lstUpdateKeyColumns = new System.Windows.Forms.ListView();
            this.columnHeader3 = new System.Windows.Forms.ColumnHeader();
            this.SuspendLayout();
            // 
            // button2
            // 
            this.button2.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button2.Location = new System.Drawing.Point(377, 240);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(67, 25);
            this.button2.TabIndex = 3;
            this.button2.Text = "Cancel";
            // 
            // button1
            // 
            this.button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.button1.Location = new System.Drawing.Point(300, 240);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(58, 25);
            this.button1.TabIndex = 2;
            this.button1.Text = "OK";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // rtbWhereClause
            // 
            this.rtbWhereClause.AllowDrop = true;
            this.rtbWhereClause.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.rtbWhereClause.Location = new System.Drawing.Point(250, 49);
            this.rtbWhereClause.Name = "rtbWhereClause";
            this.rtbWhereClause.Size = new System.Drawing.Size(393, 246);
            this.rtbWhereClause.TabIndex = 1;
            this.rtbWhereClause.Text = "";
            // 
            // lstColumns
            // 
            this.lstColumns.AllowDrop = true;
            this.lstColumns.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lstColumns.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstColumns.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.lstColumns.FullRowSelect = true;
            this.lstColumns.HideSelection = false;
            this.lstColumns.Location = new System.Drawing.Point(19, 49);
            this.lstColumns.Name = "lstColumns";
            this.lstColumns.Size = new System.Drawing.Size(211, 211);
            this.lstColumns.TabIndex = 0;
            this.lstColumns.UseCompatibleStateImageBehavior = false;
            this.lstColumns.View = System.Windows.Forms.View.Details;
            this.lstColumns.DoubleClick += new System.EventHandler(this.lstColumns_DoubleClick);
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Column Name";
            this.columnHeader1.Width = 171;
            // 
            // chkUseFullSelect
            // 
            this.chkUseFullSelect.Location = new System.Drawing.Point(250, 20);
            this.chkUseFullSelect.Name = "chkUseFullSelect";
            this.chkUseFullSelect.Size = new System.Drawing.Size(259, 19);
            this.chkUseFullSelect.TabIndex = 4;
            this.chkUseFullSelect.Text = "Use query as a full SELECT statement";
            // 
            // label2
            // 
            this.label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label2.Location = new System.Drawing.Point(19, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(211, 39);
            this.label2.TabIndex = 7;
            this.label2.Text = "Click Column Name to add to \"WHERE\" Query";
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label3.Location = new System.Drawing.Point(662, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(212, 39);
            this.label3.TabIndex = 9;
            this.label3.Text = "Check Column Names to use as \"EXISTS\" and \"UPDATE WHERE\" Keys";
            // 
            // lstUpdateKeyColumns
            // 
            this.lstUpdateKeyColumns.AllowDrop = true;
            this.lstUpdateKeyColumns.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lstUpdateKeyColumns.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstUpdateKeyColumns.CheckBoxes = true;
            this.lstUpdateKeyColumns.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader3});
            this.lstUpdateKeyColumns.FullRowSelect = true;
            this.lstUpdateKeyColumns.HideSelection = false;
            this.lstUpdateKeyColumns.Location = new System.Drawing.Point(662, 49);
            this.lstUpdateKeyColumns.Name = "lstUpdateKeyColumns";
            this.lstUpdateKeyColumns.Size = new System.Drawing.Size(212, 211);
            this.lstUpdateKeyColumns.TabIndex = 8;
            this.lstUpdateKeyColumns.UseCompatibleStateImageBehavior = false;
            this.lstUpdateKeyColumns.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader3
            // 
            this.columnHeader3.Text = "Column Name";
            this.columnHeader3.Width = 171;
            // 
            // WhereClauseForm
            // 
            this.AllowDrop = true;
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 16);
            this.ClientSize = new System.Drawing.Size(738, 280);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.lstUpdateKeyColumns);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.chkUseFullSelect);
            this.Controls.Add(this.lstColumns);
            this.Controls.Add(this.rtbWhereClause);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "WhereClauseForm";
            this.Text = "\"Where\" Clause for ";
            this.Load += new System.EventHandler(this.WhereClauseForm_Load);
            this.ResumeLayout(false);

		}
		#endregion

		private void WhereClauseForm_Load(object sender, System.EventArgs e)
		{
			PopulateHelper helper = new PopulateHelper(data,null);
			string[] columns = SqlSync.DbInformation.InfoHelper.GetColumnNames(this.tableName,this.data);
			for(int i=0;i<columns.Length;i++)
			{
				lstColumns.Items.Add(columns[i]);
			}

			//Pre-check exising "Update key selections"
			for(int i=0;i<columns.Length;i++)
			{
				lstUpdateKeyColumns.Items.Add(columns[i]);
				for(int j=0;j<this.checkKeyColumns.Length;j++)
				{
					if(columns[i].Trim().ToUpper() == this.checkKeyColumns[j].Trim().ToUpper())
					{
						lstUpdateKeyColumns.Items[lstUpdateKeyColumns.Items.Count-1].Checked = true;
						break;
					}
				}
			}

		}

		private void lstColumns_DoubleClick(object sender, System.EventArgs e)
		{
			if(lstColumns.SelectedItems.Count > 0)
			{
				this.rtbWhereClause.Text += " ["+lstColumns.SelectedItems[0].Text+"] ";
				//this.rtbWhereClause.Focus();
			}
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
		
		}



		#region Drag Drop (not used)
		#if(true==false)
				private void lstColumns_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
				{
					if(lstColumns.SelectedItems.Count > 0)
					{
						string text = lstColumns.SelectedItems[0].Text;
						lstColumns.DoDragDrop(text,DragDropEffects.Copy | DragDropEffects.Move);
					}

				}

				private void richTextBox1_DragEnter(object sender,System.Windows.Forms.DragEventArgs e)
				{
					if(e.Data.GetDataPresent(DataFormats.Text))
					{
						e.Effect = DragDropEffects.Copy;
					}
					else
					{
						e.Effect = DragDropEffects.None;
					}
				}

				private void richTextBox1_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
				{
					this.rtbWhereClause.Text += e.Data.GetData(DataFormats.Text).ToString();
					this.lstColumnsMouseDown = false;
				}

				
		#endif
		#endregion

	
	}
}
