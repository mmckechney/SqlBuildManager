using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace SQLSync
{
	/// <summary>
	/// Summary description for NewLookUpForm.
	/// </summary>
	public class NewLookUpForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.ListView lstTables;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private string[] remainingTables;
		private string[] selectedTables = null;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;
		public string[] TableList
		{
			get
			{
				if(this.selectedTables == null || this.selectedTables.Length == 0)
				{
					ArrayList list = new ArrayList();
					for(int i=0;i<lstTables.CheckedItems.Count;i++)
					{
						list.Add(lstTables.CheckedItems[i].Text);
						}
					selectedTables = new string[list.Count];
					list.CopyTo(selectedTables);
				}
				return selectedTables;
			}
		}
		public NewLookUpForm()
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			//
			// TODO: Add any constructor code after InitializeComponent call
			//
		}

		public NewLookUpForm(string[] remainingTables)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.remainingTables = remainingTables;

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
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.lstTables = new System.Windows.Forms.ListView();
			this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
			this.SuspendLayout();
			// 
			// button1
			// 
			this.button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button1.Location = new System.Drawing.Point(60, 478);
			this.button1.Name = "button1";
			this.button1.Size = new System.Drawing.Size(48, 20);
			this.button1.TabIndex = 1;
			this.button1.Text = "OK";
			// 
			// button2
			// 
			this.button2.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
			this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.button2.Location = new System.Drawing.Point(124, 478);
			this.button2.Name = "button2";
			this.button2.Size = new System.Drawing.Size(56, 20);
			this.button2.TabIndex = 2;
			this.button2.Text = "Cancel";
			// 
			// lstTables
			// 
			this.lstTables.Activation = System.Windows.Forms.ItemActivation.OneClick;
			this.lstTables.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
				| System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.lstTables.CheckBoxes = true;
			this.lstTables.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
																						this.columnHeader1});
			this.lstTables.FullRowSelect = true;
			this.lstTables.Location = new System.Drawing.Point(14, 17);
			this.lstTables.Name = "lstTables";
			this.lstTables.Size = new System.Drawing.Size(213, 451);
			this.lstTables.TabIndex = 0;
			this.lstTables.View = System.Windows.Forms.View.Details;
			// 
			// columnHeader1
			// 
			this.columnHeader1.Text = "Table Name";
			this.columnHeader1.Width = 185;
			// 
			// NewLookUpForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(241, 510);
			this.Controls.Add(this.lstTables);
			this.Controls.Add(this.button2);
			this.Controls.Add(this.button1);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.Name = "NewLookUpForm";
			this.Text = "Add Table";
			this.Load += new System.EventHandler(this.NewLookUpForm_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void NewLookUpForm_Load(object sender, System.EventArgs e)
		{
			for(int i=0;i<this.remainingTables.Length;i++)
			{
				lstTables.Items.Add(this.remainingTables[i]);
			}
		}
	}
}
