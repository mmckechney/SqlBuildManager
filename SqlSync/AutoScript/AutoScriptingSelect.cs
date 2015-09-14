using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using SqlSync.AutoScript;
using SqlSync.ObjectScript;
namespace SqlSync
{
	/// <summary>
	/// Summary description for AutoScriptingSelect.
	/// </summary>
	public class AutoScriptingSelect : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Button btnProceed;
		public AutoScriptingConfig config;
		private System.Windows.Forms.Button button1;
		private System.Windows.Forms.ListView lstToScript;
		private System.Windows.Forms.ColumnHeader columnHeader1;
		private System.Windows.Forms.ColumnHeader columnHeader2;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public AutoScriptingSelect(AutoScriptingConfig config)
		{
			
			InitializeComponent();
			this.config = config;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AutoScriptingSelect));
            this.label1 = new System.Windows.Forms.Label();
            this.btnProceed = new System.Windows.Forms.Button();
            this.button1 = new System.Windows.Forms.Button();
            this.lstToScript = new System.Windows.Forms.ListView();
            this.columnHeader1 = new System.Windows.Forms.ColumnHeader();
            this.columnHeader2 = new System.Windows.Forms.ColumnHeader();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(24, 8);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(128, 16);
            this.label1.TabIndex = 1;
            this.label1.Text = "Select Items to Script";
            // 
            // btnProceed
            // 
            this.btnProceed.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.btnProceed.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnProceed.Location = new System.Drawing.Point(83, 208);
            this.btnProceed.Name = "btnProceed";
            this.btnProceed.Size = new System.Drawing.Size(75, 23);
            this.btnProceed.TabIndex = 2;
            this.btnProceed.Text = "Proceed";
            this.btnProceed.Click += new System.EventHandler(this.btnProceed_Click);
            // 
            // button1
            // 
            this.button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            this.button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.button1.Location = new System.Drawing.Point(171, 208);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(75, 23);
            this.button1.TabIndex = 3;
            this.button1.Text = "Cancel";
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // lstToScript
            // 
            this.lstToScript.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.lstToScript.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.lstToScript.CheckBoxes = true;
            this.lstToScript.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader1,
            this.columnHeader2});
            this.lstToScript.Location = new System.Drawing.Point(24, 24);
            this.lstToScript.Name = "lstToScript";
            this.lstToScript.Size = new System.Drawing.Size(280, 168);
            this.lstToScript.TabIndex = 4;
            this.lstToScript.UseCompatibleStateImageBehavior = false;
            this.lstToScript.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            this.columnHeader1.Text = "Server";
            this.columnHeader1.Width = 128;
            // 
            // columnHeader2
            // 
            this.columnHeader2.Text = "Database";
            this.columnHeader2.Width = 150;
            // 
            // AutoScriptingSelect
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(328, 238);
            this.Controls.Add(this.lstToScript);
            this.Controls.Add(this.button1);
            this.Controls.Add(this.btnProceed);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "AutoScriptingSelect";
            this.Text = "Auto Scripting Select ";
            this.Load += new System.EventHandler(this.AutoScriptingSelect_Load);
            this.ResumeLayout(false);

		}
		#endregion

		private void AutoScriptingSelect_Load(object sender, System.EventArgs e)
		{
			foreach(AutoScriptingConfig.DatabaseScriptConfigRow row in this.config.DatabaseScriptConfig)
			{
				this.lstToScript.Items.Add(new ListViewItem(new string[]{row.ServerName,row.DatabaseName}));
			}
		}

		private void button1_Click(object sender, System.EventArgs e)
		{
			this.DialogResult = DialogResult.Cancel;
			this.Close();
		}

		private void btnProceed_Click(object sender, System.EventArgs e)
		{
			string dbName;
			string serverName;

			for(int i=0;i<this.lstToScript.Items.Count;i++)
			{
				if(this.lstToScript.Items[i].Checked == false)
				{
					serverName = this.lstToScript.Items[i].SubItems[0].Text;
					dbName = this.lstToScript.Items[i].SubItems[1].Text;

					for(short j=0;j<this.config.DatabaseScriptConfig.Rows.Count;j++)
					{
						if(this.config.DatabaseScriptConfig[j].DatabaseName == dbName &&
							this.config.DatabaseScriptConfig[j].ServerName == serverName)
						{
							this.config.DatabaseScriptConfig[j].Delete();
							break;
						}
					}
					this.config.DatabaseScriptConfig.AcceptChanges();
				}
			}
			this.DialogResult = DialogResult.OK;
			this.Close();
			
		}

	}
}
