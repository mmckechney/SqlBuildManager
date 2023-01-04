using SqlSync.SqlBuild;
using System;
using System.Windows.Forms;
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

            lblDatabase.Text = runRow.Database;
            lblEnd.Text = runRow.RunEnd.ToString();
            lblRunOrder.Text = runRow.RunOrder.ToString();
            lblStart.Text = runRow.RunStart.ToString();
            rtbResults.Text = runRow.Results;
            lblSuccess.Text = runRow.Success.ToString();
            Text = String.Format(Text, new object[] { runRow.FileName });
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

        #region Windows Form Designer generated code
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ScriptRunResultsForm));
            label1 = new System.Windows.Forms.Label();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            label4 = new System.Windows.Forms.Label();
            label5 = new System.Windows.Forms.Label();
            rtbResults = new System.Windows.Forms.RichTextBox();
            lblDatabase = new System.Windows.Forms.Label();
            lblRunOrder = new System.Windows.Forms.Label();
            lblStart = new System.Windows.Forms.Label();
            lblEnd = new System.Windows.Forms.Label();
            lblSuccess = new System.Windows.Forms.Label();
            label11 = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // label1
            // 
            label1.Location = new System.Drawing.Point(19, 49);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(148, 20);
            label1.TabIndex = 0;
            label1.Text = "Start:";
            // 
            // label2
            // 
            label2.Location = new System.Drawing.Point(19, 69);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(148, 20);
            label2.TabIndex = 1;
            label2.Text = "End:";
            // 
            // label3
            // 
            label3.Location = new System.Drawing.Point(19, 30);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(148, 19);
            label3.TabIndex = 2;
            label3.Text = "Run Order:";
            // 
            // label4
            // 
            label4.Location = new System.Drawing.Point(19, 10);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(148, 20);
            label4.TabIndex = 3;
            label4.Text = "Destination Database:";
            // 
            // label5
            // 
            label5.Location = new System.Drawing.Point(19, 89);
            label5.Name = "label5";
            label5.Size = new System.Drawing.Size(148, 19);
            label5.TabIndex = 4;
            label5.Text = "Successful:";
            // 
            // rtbResults
            // 
            rtbResults.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            rtbResults.BackColor = System.Drawing.SystemColors.Control;
            rtbResults.BorderStyle = System.Windows.Forms.BorderStyle.None;
            rtbResults.Location = new System.Drawing.Point(67, 128);
            rtbResults.Name = "rtbResults";
            rtbResults.ReadOnly = true;
            rtbResults.Size = new System.Drawing.Size(609, 236);
            rtbResults.TabIndex = 5;
            rtbResults.Text = "";
            // 
            // lblDatabase
            // 
            lblDatabase.Location = new System.Drawing.Point(173, 10);
            lblDatabase.Name = "lblDatabase";
            lblDatabase.Size = new System.Drawing.Size(269, 20);
            lblDatabase.TabIndex = 6;
            // 
            // lblRunOrder
            // 
            lblRunOrder.Location = new System.Drawing.Point(173, 30);
            lblRunOrder.Name = "lblRunOrder";
            lblRunOrder.Size = new System.Drawing.Size(269, 19);
            lblRunOrder.TabIndex = 7;
            // 
            // lblStart
            // 
            lblStart.Location = new System.Drawing.Point(173, 49);
            lblStart.Name = "lblStart";
            lblStart.Size = new System.Drawing.Size(269, 20);
            lblStart.TabIndex = 8;
            // 
            // lblEnd
            // 
            lblEnd.Location = new System.Drawing.Point(173, 69);
            lblEnd.Name = "lblEnd";
            lblEnd.Size = new System.Drawing.Size(269, 20);
            lblEnd.TabIndex = 9;
            // 
            // lblSuccess
            // 
            lblSuccess.Location = new System.Drawing.Point(173, 89);
            lblSuccess.Name = "lblSuccess";
            lblSuccess.Size = new System.Drawing.Size(269, 19);
            lblSuccess.TabIndex = 10;
            // 
            // label11
            // 
            label11.Location = new System.Drawing.Point(19, 108);
            label11.Name = "label11";
            label11.Size = new System.Drawing.Size(148, 20);
            label11.TabIndex = 11;
            label11.Text = "Results:";
            // 
            // ScriptRunResultsForm
            // 
            AutoScaleBaseSize = new System.Drawing.Size(6, 16);
            ClientSize = new System.Drawing.Size(688, 374);
            Controls.Add(label11);
            Controls.Add(lblSuccess);
            Controls.Add(lblEnd);
            Controls.Add(lblStart);
            Controls.Add(lblRunOrder);
            Controls.Add(lblDatabase);
            Controls.Add(rtbResults);
            Controls.Add(label5);
            Controls.Add(label4);
            Controls.Add(label3);
            Controls.Add(label2);
            Controls.Add(label1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            KeyPreview = true;
            Name = "ScriptRunResultsForm";
            Text = "Script Run Results for file \"{0}\"";
            KeyUp += new System.Windows.Forms.KeyEventHandler(ScriptRunResultsForm_KeyUp);
            ResumeLayout(false);

        }
        #endregion

        private void ScriptRunResultsForm_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Escape)
                Close();
        }
    }
}
