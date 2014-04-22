using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace SqlSync
{
    public partial class CutCopyPasteContextMenuStrip : System.Windows.Forms.ContextMenuStrip
    {

        private System.Windows.Forms.ToolStripMenuItem copyToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem pasteToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem cutToolStripMenuItem;

        public CutCopyPasteContextMenuStrip()
        {
            InitializeComponent();
            CustomInitializeComponent();
        }

        private void CustomInitializeComponent()
        {
            this.copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            // 
            // mnuCopyPaste
            // 
            this.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.cutToolStripMenuItem,
            this.copyToolStripMenuItem,
            this.pasteToolStripMenuItem});
            this.Name = "mnuCopyPaste";
            this.Size = new System.Drawing.Size(153, 92);
            this.Opening += new System.ComponentModel.CancelEventHandler(this.mnuCopyPaste_Opening);
            // 


            // 
            // copyToolStripMenuItem
            // 
            this.copyToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Copy;
            this.copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            this.copyToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.copyToolStripMenuItem.Text = "Copy";
            this.copyToolStripMenuItem.Click += new System.EventHandler(this.copyToolStripMenuItem_Click);
            // 
            // pasteToolStripMenuItem
            // 
            this.pasteToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Paste;
            this.pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            this.pasteToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.pasteToolStripMenuItem.Text = "Paste";
            this.pasteToolStripMenuItem.Click += new System.EventHandler(this.pasteToolStripMenuItem_Click);
            // 
            // cutToolStripMenuItem
            // 
            this.cutToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Cut_2;
            this.cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            this.cutToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            this.cutToolStripMenuItem.Text = "Cut";
            this.cutToolStripMenuItem.Click += new System.EventHandler(this.cutToolStripMenuItem_Click);
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.SourceControl is TextBoxBase)
            {
                TextBoxBase txtBox = (TextBoxBase)this.SourceControl;
                if (txtBox.SelectedText.Length > 0)
                    Clipboard.SetText(txtBox.SelectedText);

            }
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.SourceControl is TextBoxBase)
            {
                TextBoxBase txtBox = (TextBoxBase)this.SourceControl;
                string value = Clipboard.GetText();

                int currentLocation = txtBox.SelectionStart;

                if (value != null && value.Length > 0)
                {
                    txtBox.Text = txtBox.Text.Substring(0, txtBox.SelectionStart) + value +
                        txtBox.Text.Substring(txtBox.SelectionStart);

                    txtBox.SelectionStart = currentLocation + value.Length;
                    txtBox.SelectionLength = 0;
                }

            }
        }

        private void cutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.SourceControl is TextBoxBase)
            {
                TextBoxBase txtBox = (TextBoxBase)this.SourceControl;
                string cutPiece = txtBox.SelectedText;
                Clipboard.SetText(cutPiece);

                int currentLocation = txtBox.SelectionStart;
                txtBox.Text = txtBox.Text.Substring(0, txtBox.SelectionStart) +
                      txtBox.Text.Substring(txtBox.SelectionStart + txtBox.SelectedText.Length);

                txtBox.SelectionStart = currentLocation;
                txtBox.SelectionLength = 0;
            }
        }

        private void contextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            if (this.SourceControl is TextBoxBase)
            {
                TextBoxBase txtBox = (TextBoxBase)this.SourceControl;
                if (txtBox.SelectedText.Length == 0)
                {
                    cutToolStripMenuItem.Enabled = false;
                    copyToolStripMenuItem.Enabled = false;
                }
                else
                {
                    cutToolStripMenuItem.Enabled = true;
                    copyToolStripMenuItem.Enabled = true;
                }
            }
        }

        private void mnuCopyPaste_Opening(object sender, CancelEventArgs e)
        {
            if (this.SourceControl is TextBoxBase)
            {
                TextBoxBase txtBox = (TextBoxBase)this.SourceControl;
                if (txtBox.SelectedText.Length == 0)
                {
                    cutToolStripMenuItem.Enabled = false;
                    copyToolStripMenuItem.Enabled = false;
                }
                else
                {
                    cutToolStripMenuItem.Enabled = true;
                    copyToolStripMenuItem.Enabled = true;
                }
            }
        }
    }
}
