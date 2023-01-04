using System;
using System.ComponentModel;
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
            copyToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            pasteToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            cutToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();

            // 
            // mnuCopyPaste
            // 
            Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            cutToolStripMenuItem,
            copyToolStripMenuItem,
            pasteToolStripMenuItem});
            Name = "mnuCopyPaste";
            Size = new System.Drawing.Size(153, 92);
            Opening += new System.ComponentModel.CancelEventHandler(mnuCopyPaste_Opening);
            // 


            // 
            // copyToolStripMenuItem
            // 
            copyToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Copy;
            copyToolStripMenuItem.Name = "copyToolStripMenuItem";
            copyToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            copyToolStripMenuItem.Text = "Copy";
            copyToolStripMenuItem.Click += new System.EventHandler(copyToolStripMenuItem_Click);
            // 
            // pasteToolStripMenuItem
            // 
            pasteToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Paste;
            pasteToolStripMenuItem.Name = "pasteToolStripMenuItem";
            pasteToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            pasteToolStripMenuItem.Text = "Paste";
            pasteToolStripMenuItem.Click += new System.EventHandler(pasteToolStripMenuItem_Click);
            // 
            // cutToolStripMenuItem
            // 
            cutToolStripMenuItem.Image = global::SqlSync.Properties.Resources.Cut_2;
            cutToolStripMenuItem.Name = "cutToolStripMenuItem";
            cutToolStripMenuItem.Size = new System.Drawing.Size(152, 22);
            cutToolStripMenuItem.Text = "Cut";
            cutToolStripMenuItem.Click += new System.EventHandler(cutToolStripMenuItem_Click);
        }

        private void copyToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SourceControl is TextBoxBase)
            {
                TextBoxBase txtBox = (TextBoxBase)SourceControl;
                if (txtBox.SelectedText.Length > 0)
                    Clipboard.SetText(txtBox.SelectedText);

            }
        }

        private void pasteToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SourceControl is TextBoxBase)
            {
                TextBoxBase txtBox = (TextBoxBase)SourceControl;
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
            if (SourceControl is TextBoxBase)
            {
                TextBoxBase txtBox = (TextBoxBase)SourceControl;
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
            if (SourceControl is TextBoxBase)
            {
                TextBoxBase txtBox = (TextBoxBase)SourceControl;
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
            if (SourceControl is TextBoxBase)
            {
                TextBoxBase txtBox = (TextBoxBase)SourceControl;
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
