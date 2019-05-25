namespace SqlSync.Analysis
{
    partial class LinkedRichTextBoxes
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(LinkedRichTextBoxes));
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.leftBox = new System.Windows.Forms.RichTextBox();
            this.rightBox = new System.Windows.Forms.RichTextBox();
            this.menuStrip1 = new System.Windows.Forms.MenuStrip();
            this.saveChangeToLeftFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.refreshToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.moveContentsToLeftFileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.nextDiffToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectDiffColorSchemeToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectForegroundColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.selectBackgroundColorToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.finderCtrl1 = new SqlSync.FinderCtrl();
            this.colorDialog1 = new System.Windows.Forms.ColorDialog();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.menuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(0, 24);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.leftBox);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.rightBox);
            this.splitContainer1.Size = new System.Drawing.Size(925, 564);
            this.splitContainer1.SplitterDistance = 463;
            this.splitContainer1.TabIndex = 0;
            // 
            // leftBox
            // 
            this.leftBox.AcceptsTab = true;
            this.leftBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.leftBox.Location = new System.Drawing.Point(0, 0);
            this.leftBox.Name = "leftBox";
            this.leftBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedBoth;
            this.leftBox.Size = new System.Drawing.Size(463, 564);
            this.leftBox.TabIndex = 0;
            this.leftBox.Text = "";
            this.leftBox.WordWrap = false;
            this.leftBox.ContentsResized += new System.Windows.Forms.ContentsResizedEventHandler(this.leftBox_ContentsResized);
            this.leftBox.KeyUp += new System.Windows.Forms.KeyEventHandler(this.leftBox_KeyUp);
            // 
            // rightBox
            // 
            this.rightBox.AcceptsTab = true;
            this.rightBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rightBox.Location = new System.Drawing.Point(0, 0);
            this.rightBox.Name = "rightBox";
            this.rightBox.ScrollBars = System.Windows.Forms.RichTextBoxScrollBars.ForcedBoth;
            this.rightBox.Size = new System.Drawing.Size(458, 564);
            this.rightBox.TabIndex = 1;
            this.rightBox.Text = "";
            this.rightBox.WordWrap = false;
            this.rightBox.ContentsResized += new System.Windows.Forms.ContentsResizedEventHandler(this.rightBox_ContentsResized);
            // 
            // menuStrip1
            // 
            this.menuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.saveChangeToLeftFileToolStripMenuItem,
            this.refreshToolStripMenuItem,
            this.moveContentsToLeftFileToolStripMenuItem,
            this.nextDiffToolStripMenuItem,
            this.selectDiffColorSchemeToolStripMenuItem});
            this.menuStrip1.Location = new System.Drawing.Point(0, 0);
            this.menuStrip1.Name = "menuStrip1";
            this.menuStrip1.Size = new System.Drawing.Size(925, 24);
            this.menuStrip1.TabIndex = 1;
            this.menuStrip1.Text = "menuStrip1";
            // 
            // saveChangeToLeftFileToolStripMenuItem
            // 
            this.saveChangeToLeftFileToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("saveChangeToLeftFileToolStripMenuItem.Image")));
            this.saveChangeToLeftFileToolStripMenuItem.Name = "saveChangeToLeftFileToolStripMenuItem";
            this.saveChangeToLeftFileToolStripMenuItem.Size = new System.Drawing.Size(158, 20);
            this.saveChangeToLeftFileToolStripMenuItem.Text = "Save Changes to Left File";
            this.saveChangeToLeftFileToolStripMenuItem.Click += new System.EventHandler(this.saveChangeToLeftFileToolStripMenuItem_Click);
            // 
            // refreshToolStripMenuItem
            // 
            this.refreshToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.refreshToolStripMenuItem.Image = ((System.Drawing.Image)(resources.GetObject("refreshToolStripMenuItem.Image")));
            this.refreshToolStripMenuItem.Name = "refreshToolStripMenuItem";
            this.refreshToolStripMenuItem.Size = new System.Drawing.Size(93, 20);
            this.refreshToolStripMenuItem.Text = "Refresh Diff";
            this.refreshToolStripMenuItem.Click += new System.EventHandler(this.refreshToolStripMenuItem_Click);
            // 
            // moveContentsToLeftFileToolStripMenuItem
            // 
            this.moveContentsToLeftFileToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.moveContentsToLeftFileToolStripMenuItem.Image = global::SqlSync.Properties.Resources.left;
            this.moveContentsToLeftFileToolStripMenuItem.Name = "moveContentsToLeftFileToolStripMenuItem";
            this.moveContentsToLeftFileToolStripMenuItem.Size = new System.Drawing.Size(160, 20);
            this.moveContentsToLeftFileToolStripMenuItem.Text = "Save Contents to Left File";
            this.moveContentsToLeftFileToolStripMenuItem.Click += new System.EventHandler(this.saveContentsToLeftFileToolStripMenuItem_Click);
            // 
            // nextDiffToolStripMenuItem
            // 
            this.nextDiffToolStripMenuItem.Alignment = System.Windows.Forms.ToolStripItemAlignment.Right;
            this.nextDiffToolStripMenuItem.Image = global::SqlSync.Properties.Resources.downarrow_white;
            this.nextDiffToolStripMenuItem.ImageAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.nextDiffToolStripMenuItem.Name = "nextDiffToolStripMenuItem";
            this.nextDiffToolStripMenuItem.Padding = new System.Windows.Forms.Padding(4, 0, 100, 0);
            this.nextDiffToolStripMenuItem.Size = new System.Drawing.Size(174, 20);
            this.nextDiffToolStripMenuItem.Text = "Next Diff";
            this.nextDiffToolStripMenuItem.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            this.nextDiffToolStripMenuItem.Click += new System.EventHandler(this.nextDiffToolStripMenuItem_Click);
            // 
            // selectDiffColorSchemeToolStripMenuItem
            // 
            this.selectDiffColorSchemeToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.selectForegroundColorToolStripMenuItem,
            this.selectBackgroundColorToolStripMenuItem});
            this.selectDiffColorSchemeToolStripMenuItem.Name = "selectDiffColorSchemeToolStripMenuItem";
            this.selectDiffColorSchemeToolStripMenuItem.Size = new System.Drawing.Size(168, 20);
            this.selectDiffColorSchemeToolStripMenuItem.Text = "Change Highlight Color Scheme";
            // 
            // selectForegroundColorToolStripMenuItem
            // 
            this.selectForegroundColorToolStripMenuItem.Name = "selectForegroundColorToolStripMenuItem";
            this.selectForegroundColorToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.selectForegroundColorToolStripMenuItem.Text = "Foreground color";
            this.selectForegroundColorToolStripMenuItem.Click += new System.EventHandler(this.selectForegroundColorToolStripMenuItem_Click);
            // 
            // selectBackgroundColorToolStripMenuItem
            // 
            this.selectBackgroundColorToolStripMenuItem.Name = "selectBackgroundColorToolStripMenuItem";
            this.selectBackgroundColorToolStripMenuItem.Size = new System.Drawing.Size(167, 22);
            this.selectBackgroundColorToolStripMenuItem.Text = "Background color";
            this.selectBackgroundColorToolStripMenuItem.Click += new System.EventHandler(this.selectBackgroundColorToolStripMenuItem_Click);
            // 
            // finderCtrl1
            // 
            this.finderCtrl1.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
            this.finderCtrl1.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.finderCtrl1.Location = new System.Drawing.Point(0, 588);
            this.finderCtrl1.Name = "finderCtrl1";
            this.finderCtrl1.Size = new System.Drawing.Size(925, 30);
            this.finderCtrl1.TabIndex = 2;
            // 
            // LinkedRichTextBoxes
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.menuStrip1);
            this.Controls.Add(this.finderCtrl1);
            this.Name = "LinkedRichTextBoxes";
            this.Size = new System.Drawing.Size(925, 618);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.menuStrip1.ResumeLayout(false);
            this.menuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.RichTextBox leftBox;
        private System.Windows.Forms.RichTextBox rightBox;
        private System.Windows.Forms.MenuStrip menuStrip1;
        private System.Windows.Forms.ToolStripMenuItem saveChangeToLeftFileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem refreshToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem moveContentsToLeftFileToolStripMenuItem;
        private FinderCtrl finderCtrl1;
        private System.Windows.Forms.ToolStripMenuItem nextDiffToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectDiffColorSchemeToolStripMenuItem;
        private System.Windows.Forms.ColorDialog colorDialog1;
        private System.Windows.Forms.ToolStripMenuItem selectForegroundColorToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem selectBackgroundColorToolStripMenuItem;
    }
}
