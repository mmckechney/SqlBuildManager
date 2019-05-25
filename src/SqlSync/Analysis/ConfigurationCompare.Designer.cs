namespace SqlSync.Analysis
{
    partial class ConfigurationCompare
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
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
            this.linkedRichTextBoxes1 = new SqlSync.Analysis.LinkedRichTextBoxes();
            this.SuspendLayout();
            // 
            // linkedRichTextBoxes1
            // 
            this.linkedRichTextBoxes1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.linkedRichTextBoxes1.LeftFileName = null;
            this.linkedRichTextBoxes1.Location = new System.Drawing.Point(0, 140);
            this.linkedRichTextBoxes1.Name = "linkedRichTextBoxes1";
            this.linkedRichTextBoxes1.RightFileName = null;
            this.linkedRichTextBoxes1.Size = new System.Drawing.Size(926, 517);
            this.linkedRichTextBoxes1.TabIndex = 0;
            this.linkedRichTextBoxes1.UnifiedDiffText = null;
            // 
            // ConfigurationCompare
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(926, 657);
            this.Controls.Add(this.linkedRichTextBoxes1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Name = "ConfigurationCompare";
            this.Text = "ConfigurationCompare";
            this.Load += new System.EventHandler(this.ConfigurationCompare_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private LinkedRichTextBoxes linkedRichTextBoxes1;
    }
}