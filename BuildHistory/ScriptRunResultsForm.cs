using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using SqlSync.SqlBuild;
namespace SqlSync.BuildHistory
{
	/// <summary>
	/// Summary description for ScriptRunResultsForm.
	/// </summary>
	public class ScriptRunResultsForm : System.Windows.Forms.Form
	{
		private System.Windows.Forms.Label label1;
		private System.Windows.Forms.Label label2;
		private System.Windows.Forms.Label label3;
		private System.Windows.Forms.Label label4;
		private System.Windows.Forms.Label label5;
		private System.Windows.Forms.RichTextBox rtbResults;
		private System.Windows.Forms.Label lblDatabase;
		private System.Windows.Forms.Label lblRunOrder;
		private System.Windows.Forms.Label lblStart;
		private System.Windows.Forms.Label lblEnd;
		private System.Windows.Forms.Label lblSuccess;
		private System.Windows.Forms.Label label11;
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.Container components = null;

		public ScriptRunResultsForm(SqlSyncBuildData.ScriptRunRow runRow)
		{
			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			this.lblDatabase.Text = runRow.Database;
			this.lblEnd.Text = runRow.RunEnd.ToString();
			this.lblRunOrder.Text = runRow.RunOrder.ToString();
			this.lblStart.Text = runRow.RunStart.ToString();
			this.rtbResults.Text = runRow.Results;
			this.lblSuccess.Text = runRow.Success.ToString();
			this.Text = String.Format(this.Text,new object[]{runRow.FileName});
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScriptRunResultsForm));
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.rtbResults = new System.Windows.Forms.RichTextBox();
            this.lblDatabase = new System.Windows.Forms.Label();
            this.lblRunOrder = new System.Windows.Forms.Label();
            this.lblStart = new System.Windows.Forms.Label();
            this.lblEnd = new System.Windows.Forms.Label();
            this.lblSuccess = new System.Windows.Forms.Label();
            this.label11 = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.Location = new System.Drawing.Point(16, 40);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(123, 16);
            this.label1.TabIndex = 0;
            this.label1.Text = "Start:";
            // 
            // label2
            // 
            this.label2.Location = new System.Drawing.Point(16, 56);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(123, 16);
            this.label2.TabIndex = 1;
            this.label2.Text = "End:";
            // 
            // label3
            // 
            this.label3.Location = new System.Drawing.Point(16, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(123, 16);
            this.label3.TabIndex = 2;
            this.label3.Text = "Run Order:";
            // 
            // label4
            // 
            this.label4.Location = new System.Drawing.Point(16, 8);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(123, 16);
            this.label4.TabIndex = 3;
            this.label4.Text = "Destination Database:";
            // 
            // label5
            // 
            this.label5.Location = new System.Drawing.Point(16, 72);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(123, 16);
            this.label5.TabIndex = 4;
            this.label5.Text = "Successful:";
            // 
            // rtbResults
            // 
            this.rtbResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.rtbResults.BackColor = System.Drawing.SystemColors.Control;
            this.rtbResults.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.rtbResults.Location = new System.Drawing.Point(56, 104);
            this.rtbResults.Name = "rtbResults";
            this.rtbResults.ReadOnly = true;
            this.rtbResults.Size = new System.Drawing.Size(622, 262);
            this.rtbResults.TabIndex = 5;
            this.rtbResults.Text = "";
            // 
            // lblDatabase
            // 
            this.lblDatabase.Location = new System.Drawing.Point(144, 8);
            this.lblDatabase.Name = "lblDatabase";
            this.lblDatabase.Size = new System.Drawing.Size(224, 16);
            this.lblDatabase.TabIndex = 6;
            // 
            // lblRunOrder
            // 
            this.lblRunOrder.Location = new System.Drawing.Point(144, 24);
            this.lblRunOrder.Name = "lblRunOrder";
            this.lblRunOrder.Size = new System.Drawing.Size(224, 16);
            this.lblRunOrder.TabIndex = 7;
            // 
            // lblStart
            // 
            this.lblStart.Location = new System.Drawing.Point(144, 40);
            this.lblStart.Name = "lblStart";
            this.lblStart.Size = new System.Drawing.Size(224, 16);
            this.lblStart.TabIndex = 8;
            // 
            // lblEnd
            // 
            this.lblEnd.Location = new System.Drawing.Point(144, 56);
            this.lblEnd.Name = "lblEnd";
            this.lblEnd.Size = new System.Drawing.Size(224, 16);
            this.lblEnd.TabIndex = 9;
            // 
            // lblSuccess
            // 
            this.lblSuccess.Location = new System.Drawing.Point(144, 72);
            this.lblSuccess.Name = "lblSuccess";
            this.lblSuccess.Size = new System.Drawing.Size(224, 16);
            this.lblSuccess.TabIndex = 10;
            // 
            // label11
            // 
            this.label11.Location = new System.Drawing.Point(16, 88);
            this.label11.Name = "label11";
            this.label11.Size = new System.Drawing.Size(123, 16);
            this.label11.TabIndex = 11;
            this.label11.Text = "Results:";
            // 
            // ScriptRunResultsForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            this.ClientSize = new System.Drawing.Size(688, 374);
            this.Controls.Add(this.label11);
            this.Controls.Add(this.lblSuccess);
            this.Controls.Add(this.lblEnd);
            this.Controls.Add(this.lblStart);
            this.Controls.Add(this.lblRunOrder);
            this.Controls.Add(this.lblDatabase);
            this.Controls.Add(this.rtbResults);
            this.Controls.Add(this.label5);
            this.Controls.Add(this.label4);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.KeyPreview = true;
            this.Name = "ScriptRunResultsForm";
            this.Text = "Script Run Results for file \"{0}\"";
            this.KeyUp += new System.Windows.Forms.KeyEventHandler(this.ScriptRunResultsForm_KeyUp);
            this.ResumeLayout(false);

		}
		#endregion

        private void ScriptRunResultsForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                this.Close();
        }
	}
}
