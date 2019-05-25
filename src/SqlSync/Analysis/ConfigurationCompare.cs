using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SqlSync.Compare;

namespace SqlSync.Analysis
{
    public partial class ConfigurationCompare : Form
    {
        FileCompareResults compare = null;
        private ConfigurationCompare()
        {
            InitializeComponent();
        }
        public ConfigurationCompare(FileCompareResults compare):this()
        {
            this.compare = compare;
        }

        private void ConfigurationCompare_Load(object sender, EventArgs e)
        {
            //if (compare.UnifiedDiffText.Length > 0)
            //{
            //    this.linkedBoxes.BringToFront();
            //    this.linkedBoxes.UnifiedDiffText = compare.UnifiedDiffText;
            //    this.linkedBoxes.LeftFileName = leftTempFilePath + compare.LeftScriptRow.FileName;
            //    this.linkedBoxes.RightFileName = rightTempFilePath + compare.RightScriptRow.FileName;
            //    this.linkedBoxes.SplitUnifiedDiffText();
            //    return;

            //}
            //this.rtbResultsHighlight.BringToFront();
            //if (results.LeftScriptText.Length > 0)
            //    rtbResultsHighlight.Text = results.LeftScriptText; //lstFiles.SelectedItems[0].Tag.ToString();
            //else
            //    rtbResultsHighlight.Text = results.RightSciptText;

            //rtbResultsHighlight.HighlightType = SqlSync.Highlighting.SyntaxHightlightType.Sql;
        }
    }
}