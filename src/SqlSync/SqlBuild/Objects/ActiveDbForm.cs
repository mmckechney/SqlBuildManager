using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace SQLSync.SqlBuild.Objects
{
	/// <summary>
	/// Summary description for ActiveDbForm.
	/// </summary>
	public class ActiveDbForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.ComboBox ddDatabaseList;
		private System.Windows.Forms.Button btnOK;
		private string[] databaseList;
		public string SelectedDatabase
		{
			get
			{
				return this.ddDatabaseList.SelectedItem.ToString();
			}
		}
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ActiveDbForm(string[] databaseList)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();
			this.databaseList = databaseList;


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
			System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ActiveDbForm));
			this.label4 = new System.Windows.Forms.Label();
			this.ddDatabaseList = new System.Windows.Forms.ComboBox();
			this.btnOK = new System.Windows.Forms.Button();
			this.SuspendLayout();
			// 
			// label4
			// 
			this.label4.Location = new System.Drawing.Point(8, 8);
			this.label4.Name = "label4";
			this.label4.Size = new System.Drawing.Size(96, 16);
			this.label4.TabIndex = 13;
			this.label4.Text = "Select Database:";
			// 
			// ddDatabaseList
			// 
			this.ddDatabaseList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.ddDatabaseList.Location = new System.Drawing.Point(104, 8);
			this.ddDatabaseList.Name = "ddDatabaseList";
			this.ddDatabaseList.Size = new System.Drawing.Size(176, 21);
			this.ddDatabaseList.TabIndex = 12;
			// 
			// btnOK
			// 
			this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.btnOK.Location = new System.Drawing.Point(121, 40);
			this.btnOK.Name = "btnOK";
			this.btnOK.Size = new System.Drawing.Size(48, 23);
			this.btnOK.TabIndex = 14;
			this.btnOK.Text = "OK";
			this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
			// 
			// ActiveDbForm
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.ClientSize = new System.Drawing.Size(290, 72);
			this.Controls.Add(this.btnOK);
			this.Controls.Add(this.label4);
			this.Controls.Add(this.ddDatabaseList);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
			this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
			this.Name = "ActiveDbForm";
			this.Text = "Set Active Database";
			this.Load += new System.EventHandler(this.ActiveDbForm_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void ActiveDbForm_Load(object sender, System.EventArgs e)
		{
			this.ddDatabaseList.Items.AddRange(this.databaseList);
			if(this.ddDatabaseList.Items.Count > 0)
				this.ddDatabaseList.SelectedIndex = 0;
		}

		private void btnOK_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.OK;
			this.Close();
		}
	}
}
