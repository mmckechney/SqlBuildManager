using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using SqlSync.ScrollBarSync;
using System.IO;
namespace SqlSync.Analysis
{
    public partial class LinkedRichTextBoxes : UserControl
    {
        List<int> leftBoxHighlightingIndexes = new List<int>();
        List<int> rightBoxHighligtingIndexes = new List<int>();
        SortedDictionary<int, char> combinedHighligting = new SortedDictionary<int, char>();
        private Color diffColor = Color.Maroon;
        private Color diffForeColor = Color.White;
        private bool leftTextChanged = false;
        private string unifiedDiffText;
        private SqlSync.ScrollBarSync.RTFScrollSync scrollSync = new RTFScrollSync();
        public string UnifiedDiffText
        {
            get { return unifiedDiffText; }
            set { unifiedDiffText = value; }
        }
        public LinkedRichTextBoxes()
        {
            InitializeComponent();

            if (Properties.Settings.Default.DiffBackgroundColor != null)
            {
                this.diffColor = Properties.Settings.Default.DiffBackgroundColor;
            }
            else
            {
                this.diffColor = Color.Maroon;
                Properties.Settings.Default.DiffBackgroundColor = Color.Maroon;
                Properties.Settings.Default.Save();

            }

            if (Properties.Settings.Default.DiffForegroundColor != null)
            {
                this.diffColor = Properties.Settings.Default.DiffForegroundColor;
            }
            else
            {
                this.diffForeColor = Color.White;
                Properties.Settings.Default.DiffForegroundColor = Color.White;
                Properties.Settings.Default.Save();

            }
            scrollSync.ScrollBarToSync = ScrollBars.Both;
            scrollSync.AddControl(leftBox);
            scrollSync.AddControl(rightBox);

            this.finderCtrl1.AddControlToSearch(this.leftBox);
            this.finderCtrl1.AddControlToSearch(this.rightBox);
        }

        

        private string leftFileName;
        private string rightFileName;
        public void UpdateDiffColorScheme(Color backColor, Color foreColor)
        {
            this.diffColor = backColor;
            this.diffForeColor = foreColor;
            SplitUnifiedDiffText();
        }

        public bool ShowMenuStrip
        {
            get { return this.menuStrip1.Visible; }
            set { this.menuStrip1.Visible = value; }
        }

        public string RightFileName
        {
            get { return rightFileName; }
            set { rightFileName = value; }
        }
        public string LeftFileName
        {
            get { return leftFileName; }
            set { leftFileName = value; }
        }
        public LinkedRichTextBoxes(string unifiedDiffText) :this()
        {
            this.unifiedDiffText = unifiedDiffText;
        }

        public void SplitUnifiedDiffText(string unifiedDiffText, string leftFileName, string rightFileName)
        {
            this.unifiedDiffText = unifiedDiffText;
            this.leftFileName = leftFileName;
            this.rightFileName = rightFileName;
            SplitUnifiedDiffText();
        }
        public void SplitUnifiedDiffText()
        {
            this.diffColor = Properties.Settings.Default.DiffBackgroundColor;
            this.diffForeColor = Properties.Settings.Default.DiffForegroundColor;
            this.leftBox.Clear();
            this.rightBox.Clear();
            this.lastSelectedHighlightLine = 0;

            string[] arrUnified = this.unifiedDiffText.Split(new string[]{"\r\n"}, StringSplitOptions.None);
            for (int i = 0; i < arrUnified.Length; i++)
            {
                string current = arrUnified[i];
                if(i < 3 && (current.StartsWith("+++") || current.StartsWith("---")))
                    continue;

                if (current.Length> 0 && current.Substring(0, 1) != "+" && current.Substring(0, 1) != "-")
                {
                    this.leftBox.AppendText(current.Substring(1) + "\r\n");
                    this.rightBox.AppendText(current.Substring(1) + "\r\n");
                    continue;
                }

                if (current.Length == 0)
                {
                    this.leftBox.AppendText("\r\n");
                    this.rightBox.AppendText("\r\n");
                    continue;
                }
                if (current.Substring(0, 1) == "+")
                {
                    this.leftBox.SelectionBackColor = diffColor;
                    this.leftBox.SelectionColor = diffForeColor;
                    this.rightBox.SelectionBackColor = diffColor;
                    this.rightBox.SelectionColor = diffForeColor;

                    this.leftBox.AppendText("\r\n");
                    this.rightBox.AppendText(current.Substring(1) + "\r\n");

                    
                    continue;
                }

                if (current.Substring(0, 1) == "-")
                {
                    ProcessRemoveSplit(current, ref arrUnified, ref i);
                    continue;
                }
            }
            this.leftBox.Select(0, 0);
            this.leftBox.ScrollToCaret();
            this.CombineHighlighting();

        }
        private void CombineHighlighting()
        {
            for (int i = 0; i < this.leftBoxHighlightingIndexes.Count; i++)
            {
                if (!this.combinedHighligting.ContainsKey(this.leftBox.GetLineFromCharIndex( this.leftBoxHighlightingIndexes[i])))
                    this.combinedHighligting.Add(this.leftBox.GetLineFromCharIndex( this.leftBoxHighlightingIndexes[i]), 'L');
            }
            for (int i = 0; i < this.rightBoxHighligtingIndexes.Count; i++)
            {
                if (!this.combinedHighligting.ContainsKey(this.rightBox.GetLineFromCharIndex(this.rightBoxHighligtingIndexes[i])))
                    this.combinedHighligting.Add(this.rightBox.GetLineFromCharIndex(this.rightBoxHighligtingIndexes[i]), 'R');
            }

        }
        private void ProcessRemoveSplit(string current, ref string[] arrUnified, ref int i)
        {
            
            this.leftBox.SelectionBackColor = diffColor;
            this.leftBox.SelectionColor = diffForeColor;
            this.leftBoxHighlightingIndexes.Add(this.leftBox.SelectionStart);
            

            if (current.Substring(0, 1) == "-")
            {
                if(!this.leftBoxHighlightingIndexes.Contains(this.leftBox.SelectionStart))
                    this.leftBoxHighlightingIndexes.Add(this.leftBox.SelectionStart);
                
                this.leftBox.SelectionBackColor = diffColor;
                this.leftBox.SelectionColor = diffForeColor;
                this.leftBox.AppendText(current.Substring(1) + "\r\n");
                if (arrUnified.Length >= i + 1)
                {
                    if (arrUnified[i + 1].Length > 0 && (arrUnified[i + 1].Substring(0, 1) == "-" || arrUnified[i + 1].Substring(0, 1) == "+"))
                    {
                        i++;
                        ProcessRemoveSplit(arrUnified[i], ref arrUnified, ref i);
                    }
                    else
                    {
                        this.rightBox.SelectionBackColor = diffColor;
                        this.rightBox.SelectionColor = diffForeColor;
                        this.rightBoxHighligtingIndexes.Add(this.rightBox.SelectionStart);

                        if (this.rightBox.SelectedText.Trim() == "")
                            this.rightBox.SelectedText = "          ";
                        this.rightBox.AppendText("\r\n");
                    }
                }
            }
            if (current.Substring(0, 1) == "+")
            {
                this.rightBox.SelectionBackColor = diffColor;
                this.rightBox.SelectionColor = diffForeColor;

                this.rightBoxHighligtingIndexes.Add(this.rightBox.SelectionStart);
                this.rightBox.AppendText(current.Substring(1) + "\r\n");
                 if (arrUnified.Length >= i + 1)
                {
                    if (arrUnified[i + 1].Length > 0 && (arrUnified[i + 1].Substring(0, 1) == "+"))
                    {
                        i++;
                        ProcessRemoveSplit(arrUnified[i], ref arrUnified, ref i);
                    }
                 }

            }
        }


        private void saveChangeToLeftFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            if (this.leftFileName != null && this.leftFileName.Length > 0)
            {
                if(System.IO.File.Exists(this.leftFileName))
                    System.IO.File.WriteAllLines(this.leftFileName,this.leftBox.Lines);

                if (this.FileChanged != null)
                    this.FileChanged(null, EventArgs.Empty);
            }
            this.Cursor = Cursors.Default;
            this.leftTextChanged = false;
        }

        public event EventHandler FileChanged;

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (this.leftTextChanged)
            {
                DialogResult result = MessageBox.Show("You haven't saved your change to the left file. Do you want to save your changes before refreshing?", "Unsaved Changes", MessageBoxButtons.YesNoCancel);
                switch( result)
                {
                    case DialogResult.Yes:
                        this.saveChangeToLeftFileToolStripMenuItem_Click(sender, e);
                        break;
                    case DialogResult.Cancel:
                        return;
                    case DialogResult.No:
                    default:
                        break;

                }
            }
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);

            Algorithm.Diff.UnifiedDiff.WriteUnifiedDiff(this.leftFileName,this.rightFileName,sw, 500, false, false);
            this.unifiedDiffText = sb.ToString();
            this.SplitUnifiedDiffText();
        }

        private void leftBox_KeyUp(object sender, KeyEventArgs e)
        {
            this.leftTextChanged = true;
        }

        private void saveContentsToLeftFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            
            if (File.Exists(this.rightFileName))
                File.Copy(this.rightFileName, this.leftFileName, true);
           
            if (this.FileChanged != null)
                this.FileChanged(null, EventArgs.Empty);

            this.leftBox.Text = this.rightBox.Text;

            this.Cursor = Cursors.Default;
            this.leftTextChanged = false;
        }

        int lastSelectedHighlightLine = 0;
        private void nextDiffToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SortedDictionary<int, char>.KeyCollection coll = this.combinedHighligting.Keys;
            foreach (int key in coll)
            {
                if (key > this.lastSelectedHighlightLine)
                {
                    this.lastSelectedHighlightLine = key;
                    if (this.combinedHighligting[key] == 'L')
                    {
                        int start = this.leftBox.GetFirstCharIndexFromLine(this.lastSelectedHighlightLine);
                        this.leftBox.Select(start, 1);
                        this.leftBox.ScrollToCaret();
                        break;

                    }
                    else if (this.combinedHighligting[key] == 'R')
                    {
                        int start = this.rightBox.GetFirstCharIndexFromLine(this.lastSelectedHighlightLine);
                        this.rightBox.Select(start, 1);
                        this.rightBox.ScrollToCaret();
                        break;

                    }
                }
            }
        }

        private void selectForegroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            
            colorDialog1.FullOpen = true;
            colorDialog1.AnyColor = true;
            colorDialog1.Color = Properties.Settings.Default.DiffForegroundColor;
            if (DialogResult.OK == colorDialog1.ShowDialog())
            {
                this.diffForeColor = colorDialog1.Color;
                Properties.Settings.Default.DiffForegroundColor = colorDialog1.Color;
                Properties.Settings.Default.Save();
                UpdateDiffColorScheme(this.diffColor,this.diffForeColor);
            }
        }

        

        private void selectBackgroundColorToolStripMenuItem_Click(object sender, EventArgs e)
        {
            colorDialog1.FullOpen = true;
            colorDialog1.AnyColor = true;
            colorDialog1.Color = Properties.Settings.Default.DiffBackgroundColor;
            if (DialogResult.OK == colorDialog1.ShowDialog())
            {
                this.diffColor = colorDialog1.Color;
                Properties.Settings.Default.DiffBackgroundColor = colorDialog1.Color;
                Properties.Settings.Default.Save();
                UpdateDiffColorScheme(this.diffColor, this.diffForeColor);
            }
        }

        

        private void rightBox_ContentsResized(object sender, ContentsResizedEventArgs e)
        {
            this.leftBox.ZoomFactor = this.rightBox.ZoomFactor;
        }

        private void leftBox_ContentsResized(object sender, ContentsResizedEventArgs e)
        {
            this.rightBox.ZoomFactor = this.leftBox.ZoomFactor;
        }

    }
}
