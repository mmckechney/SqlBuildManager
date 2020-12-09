using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace SQLSync
{
	/// <summary>
	/// Summary description for NewLookUpDatabaseForm.
	/// </summary>
	public class NewLookUpDatabaseForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.Button button2;
		private System.Windows.Forms.Button button1;
		private string[] remainingDatabases;
		private System.Windows.Forms.ListView lstDatabases;
		private string[] selectedDatabases;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public string[] DatabaseList
		{
			get
			{
				if(this.selectedDatabases == null || this.selectedDatabases.Length == 0)
				{
					ArrayList list = new ArrayList();
					for(int i=0;i<lstDatabases.CheckedItems.Count;i++)
					{
						list.Add(lstDatabases.CheckedItems[i].Text);
					}
					this.selectedDatabases = new string[list.Count];
					list.CopyTo(this.selectedDatabases);
				}
				return this.selectedDatabases;
			}
		}
		public NewLookUpDatabaseForm(string[] remainingDatabases)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			this.remainingDatabases = remainingDatabases;

			//
			// TODO: Add any constructor code after InitializeComponent call
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
            this.lstDatabases = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.button2 = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // lstDatabases
            // 
            this.lstDatabases.Activation = System.Windows.Forms.ItemActivation.OneClick;
            this.lstDatabases.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.lstDatabases.CheckBoxes = true;
            this.lstDatabases.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1});
            this.lstDatabases.FullRowSelect = true;
            this.lstDatabases.HideSelection = false;
            this.lstDatabases.Location = new System.Drawing.Point(19, 10);
            this.lstDatabases.Name = "lstDatabases";
            this.lstDatabases.Size = new System.Drawing.Size(223, 445);
            this.lstDatabases.TabIndex = 0;
            this.lstDatabases.UseCompatibleStateImageBehavior = false;
            this.lstDatabases.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Database Name";
            this.columnHeader1.Width = 203;
            // 
            // button2
            // 
            this.button2.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button2.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button2.Location = new System.Drawing.Point(137, 465);
            this.button2.Name = "button2";
            this.button2.Size = new System.Drawing.Size(67, 25);
            this.button2.TabIndex = 2;
            this.button2.Text = "Cancel";
            // 
            // button1
            // 
            this.button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Location = new System.Drawing.Point(60, 465);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(58, 25);
            this.button1.TabIndex = 1;
            this.button1.Text = "OK";
            // 
            // NewLookUpDatabaseForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 16);
            this.ClientSize = new System.Drawing.Size(264, 504);
            this.Controls.Add(this.lstDatabases);
            this.Controls.Add(this.button2);
            this.Controls.Add(this.button1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Name = "NewLookUpDatabaseForm";
            this.Text = "Add Database";
            this.Load += new System.EventHandler(this.NewLookUpDatabaseForm_Load);
            this.ResumeLayout(false);

		}
		#endregion

		private void NewLookUpDatabaseForm_Load(object sender, System.EventArgs e)
		{
			for(int i=0;i<this.remainingDatabases.Length;i++)
			{
				this.lstDatabases.Items.Add(this.remainingDatabases[i]);
			}
		}
	}
}
