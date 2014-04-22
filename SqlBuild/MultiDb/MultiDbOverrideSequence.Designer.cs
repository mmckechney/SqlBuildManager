namespace SqlSync.SqlBuild.MultiDb
{
    partial class MultiDbOverrideSequence
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.txtSequence = new System.Windows.Forms.TextBox();
            this.lblDatabase = new System.Windows.Forms.Label();
            this.contextMenuStrip1 = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.mnuAutoSequence = new System.Windows.Forms.ToolStripMenuItem();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.contextMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtSequence
            // 
            this.txtSequence.Location = new System.Drawing.Point(3, 0);
            this.txtSequence.Name = "txtSequence";
            this.txtSequence.Size = new System.Drawing.Size(43, 20);
            this.txtSequence.TabIndex = 0;
            // 
            // lblDatabase
            // 
            this.lblDatabase.AutoSize = true;
            this.lblDatabase.ContextMenuStrip = this.contextMenuStrip1;
            this.lblDatabase.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblDatabase.Location = new System.Drawing.Point(53, 4);
            this.lblDatabase.Name = "lblDatabase";
            this.lblDatabase.Size = new System.Drawing.Size(41, 13);
            this.lblDatabase.TabIndex = 1;
            this.lblDatabase.Text = "label1";
            // 
            // contextMenuStrip1
            // 
            this.contextMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.mnuAutoSequence});
            this.contextMenuStrip1.Name = "contextMenuStrip1";
            this.contextMenuStrip1.Size = new System.Drawing.Size(248, 26);
            // 
            // mnuAutoSequence
            // 
            this.mnuAutoSequence.Name = "mnuAutoSequence";
            this.mnuAutoSequence.Size = new System.Drawing.Size(247, 22);
            this.mnuAutoSequence.Text = "Auto Sequence Target Databases";
            this.mnuAutoSequence.Click += new System.EventHandler(this.mnuAutoSequence_Click);
            // 
            // MultiDbOverrideSequence
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.lblDatabase);
            this.Controls.Add(this.txtSequence);
            this.Name = "MultiDbOverrideSequence";
            this.Size = new System.Drawing.Size(194, 20);
            this.contextMenuStrip1.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }



        #endregion

        private System.Windows.Forms.TextBox txtSequence;
        private System.Windows.Forms.Label lblDatabase;
        private System.Windows.Forms.ContextMenuStrip contextMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem mnuAutoSequence;
        private System.Windows.Forms.ToolTip toolTip1;
    }
}
