namespace SqlSync.Analysis
{
    partial class SimpleDiffForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(SimpleDiffForm));
            this.linkedBoxes = new SqlSync.Analysis.LinkedRichTextBoxes();
            this.SuspendLayout();
            // 
            // linkedBoxes
            // 
            this.linkedBoxes.Dock = System.Windows.Forms.DockStyle.Fill;
            this.linkedBoxes.LeftFileName = null;
            this.linkedBoxes.Location = new System.Drawing.Point(0, 0);
            this.linkedBoxes.Name = "linkedBoxes";
            this.linkedBoxes.RightFileName = null;
            this.linkedBoxes.ShowMenuStrip = true;
            this.linkedBoxes.Size = new System.Drawing.Size(931, 672);
            this.linkedBoxes.TabIndex = 0;
            this.linkedBoxes.UnifiedDiffText = null;
            this.linkedBoxes.FileChanged += new System.EventHandler(this.linkedBoxes_FileChanged);
            // 
            // SimpleDiffForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(931, 672);
            this.Controls.Add(this.linkedBoxes);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "SimpleDiffForm";
            this.Text = "File to Server Compare ::  {0} <----> {1}.{2}.{3}";
            this.Load += new System.EventHandler(this.SimpleDiffForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private LinkedRichTextBoxes linkedBoxes;
    }
}