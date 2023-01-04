using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text.RegularExpressions;
using System.Windows.Forms;
namespace SqlSync
{
    public partial class FinderCtrl : UserControl
    {
        private List<RichTextBox> controlsToSearch = new List<RichTextBox>();
        private MatchCollection[] lastMatches = new MatchCollection[0];
        public void AddControlToSearch(RichTextBox rtb)
        {
            controlsToSearch.Add(rtb);
            lastMatches = new MatchCollection[controlsToSearch.Count];
        }

        Regex regExp = null;
        int lastCollectionIndex = -1;

        private int lastFindStartIndex = 0;
        private int lastFindLength = 0;

        public FinderCtrl()
        {
            InitializeComponent();
        }
        public FinderCtrl(RichTextBox controlToSearch) : this()
        {
            controlsToSearch.Add(controlToSearch);
        }
        public FinderCtrl(List<RichTextBox> controlsToSearch) : this()
        {
            this.controlsToSearch = controlsToSearch;
            lastMatches = new MatchCollection[this.controlsToSearch.Count];
        }

        private void txtFind_TextChanged(object sender, EventArgs e)
        {
            PopulateMatchCollection();
            if (lastMatches != null && lastMatches.Length > 0)
            {
                lastCollectionIndex = 0;
                SetSelectedFind(null);
            }

        }
        private void PopulateMatchCollection()
        {
            if (!chkMatchCase.Checked)
                regExp = new Regex(txtFind.Text, RegexOptions.IgnoreCase);
            else
                regExp = new Regex(txtFind.Text, RegexOptions.None);

            lastMatches = new MatchCollection[controlsToSearch.Count];
            for (int i = 0; i < controlsToSearch.Count; i++)
            {
                lastMatches[i] = regExp.Matches(controlsToSearch[i].Text);
            }
        }
        private bool SetSelectedFind(Nullable<bool> forward)
        {
            bool found = false;
            for (int i = 0; i < lastMatches.Length; i++)
            {

                int currentCaretLocation = controlsToSearch[i].SelectionStart;
                if (lastMatches[i] != null && lastMatches[i].Count > 0) //lastCollectionIndex && lastCollectionIndex > -1)
                {
                    if (forward.HasValue == false || forward.Value == true)
                    {
                        if (forward.HasValue)
                            currentCaretLocation++;

                        for (int j = 0; j < lastMatches[i].Count; j++)
                        {
                            if (lastMatches[i][j].Success && lastMatches[i][j].Index >= currentCaretLocation)
                            {
                                lastFindStartIndex = lastMatches[i][j].Index;
                                lastFindLength = lastMatches[i][j].Length;
                                found = true;
                                break;
                            }
                        }
                    }
                    else
                    {
                        currentCaretLocation--;
                        for (int j = lastMatches[i].Count - 1; j >= 0; j--)
                        {
                            if (lastMatches[i][j].Success && lastMatches[i][j].Index < currentCaretLocation)
                            {
                                lastFindStartIndex = lastMatches[i][j].Index;
                                lastFindLength = lastMatches[i][j].Length;
                                found = true;
                                break;
                            }
                        }
                    }

                    if (found)
                    {
                        //Clear the last highlighting
                        ClearHighlighting(i);

                        if (controlsToSearch[i] is UrielGuy.SyntaxHighlighting.SyntaxHighlightingTextBox)
                            ((UrielGuy.SyntaxHighlighting.SyntaxHighlightingTextBox)controlsToSearch[i]).SuspendHighlighting = true;

                        //Set the next highlighting
                        controlsToSearch[i].Select(lastFindStartIndex, lastFindLength);
                        controlsToSearch[i].SelectionBackColor = Color.LawnGreen;
                        controlsToSearch[i].ScrollToCaret();

                    }

                }
            }

            return found;
        }
        private void ClearHighlighting(int controlIndex)
        {
            int currentLocation = controlsToSearch[controlIndex].SelectionStart;
            //Clear the last highlighting
            controlsToSearch[controlIndex].SelectAll();
            controlsToSearch[controlIndex].SelectionBackColor = System.Drawing.SystemColors.Window;
            controlsToSearch[controlIndex].Select(currentLocation, 1);

        }
        private void btnNext_Click(object sender, EventArgs e)
        {
            lastCollectionIndex++;
            if (!SetSelectedFind(true))
                lastCollectionIndex--;
        }

        private void btnPrevious_Click(object sender, EventArgs e)
        {
            lastCollectionIndex--;
            if (!SetSelectedFind(false))
                lastCollectionIndex++;
        }

        private void FinderCtrl_Leave(object sender, EventArgs e)
        {
            for (int i = 0; i < controlsToSearch.Count; i++)
            {
                ClearHighlighting(i);

                if (controlsToSearch[i] is UrielGuy.SyntaxHighlighting.SyntaxHighlightingTextBox &&
                    ((UrielGuy.SyntaxHighlighting.SyntaxHighlightingTextBox)controlsToSearch[i]).HighlightType != Highlighting.SyntaxHightlightType.None)
                    ((UrielGuy.SyntaxHighlighting.SyntaxHighlightingTextBox)controlsToSearch[i]).SuspendHighlighting = false;

                lastCollectionIndex = -1;
                lastFindStartIndex = 0;
                lastFindLength = 0;
            }


        }

        private void chkMatchCase_CheckedChanged(object sender, EventArgs e)
        {
            PopulateMatchCollection();
        }

        private void txtLineNumber_TextChanged(object sender, EventArgs e)
        {
            int lineNumber;
            if (Int32.TryParse(txtLineNumber.Text, out lineNumber))
            {
                for (int i = 0; i < controlsToSearch.Count; i++)
                {

                    if (controlsToSearch[i] is RichTextBox)
                    {
                        if (lineNumber >= 1 && controlsToSearch[i].Lines.Length > lineNumber)
                        {
                            int start = 0;
                            int location = 0;

                            Regex regNewLine = new Regex("\n");
                            MatchCollection matches = regNewLine.Matches(controlsToSearch[i].Text);
                            if (matches.Count > lineNumber)
                            {
                                location = matches[lineNumber - 1].Index;
                                if (lineNumber - 1 > 0)
                                    start = matches[lineNumber - 2].Index + 1;
                                else
                                    start = 0;
                            }
                            if ((lineNumber - 1) == controlsToSearch[i].GetLineFromCharIndex(location))
                            {
                                ClearHighlighting(i);

                                if (controlsToSearch[i] is UrielGuy.SyntaxHighlighting.SyntaxHighlightingTextBox)
                                    ((UrielGuy.SyntaxHighlighting.SyntaxHighlightingTextBox)controlsToSearch[i]).SuspendHighlighting = true;

                                controlsToSearch[i].Select(start, location - start);
                            }
                        }
                        else
                        {
                            controlsToSearch[i].Select(controlsToSearch[i].Text.Length - 1, 0);
                        }
                        controlsToSearch[i].SelectionBackColor = Color.LawnGreen;
                        controlsToSearch[i].ScrollToCaret();

                    }
                }
            }
        }
    }


}
