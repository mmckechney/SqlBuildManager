using System.Collections.Specialized;
using System.Drawing;
using System.Windows.Forms;
namespace SqlSync.SqlBuild
{
    /// <summary>
    /// Summary description for UtilityReplacement.
    /// </summary>
    public class UtilityReplacement : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Label label1;
        private SqlSync.SqlBuild.UtilityReplacementUnit utilityReplacementUnit1;
        private System.Windows.Forms.Button btnSubmit;
        private string[] keyValues = null;
        public StringDictionary Replacements = new StringDictionary();
        System.Drawing.Point startLocation;
        private System.Windows.Forms.Label label2;
        private UrielGuy.SyntaxHighlighting.SyntaxHighlightingTextBox rtbSqlScript;
        private System.Windows.Forms.CheckBox chkInsert;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.CheckBox chkClipBoard;
        private System.Windows.Forms.Panel pnlReplacements;
        private System.Windows.Forms.Splitter splitter1;
        private string inputText;
        private Size startSize;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        public UtilityReplacement(string[] keyValues, string title, string inputText)
        {
            InitializeComponent();
            this.keyValues = keyValues;
            Text += " :: " + title;
            this.inputText = inputText;

            startLocation = utilityReplacementUnit1.Location;
            startSize = utilityReplacementUnit1.Size;
            pnlReplacements.Controls.Remove(utilityReplacementUnit1);

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
            UrielGuy.SyntaxHighlighting.HighLightDescriptorCollection highLightDescriptorCollection1 = new UrielGuy.SyntaxHighlighting.HighLightDescriptorCollection();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(UtilityReplacement));
            label1 = new System.Windows.Forms.Label();
            utilityReplacementUnit1 = new SqlSync.SqlBuild.UtilityReplacementUnit();
            btnSubmit = new System.Windows.Forms.Button();
            label2 = new System.Windows.Forms.Label();
            rtbSqlScript = new UrielGuy.SyntaxHighlighting.SyntaxHighlightingTextBox();
            chkInsert = new System.Windows.Forms.CheckBox();
            panel1 = new System.Windows.Forms.Panel();
            chkClipBoard = new System.Windows.Forms.CheckBox();
            pnlReplacements = new System.Windows.Forms.Panel();
            splitter1 = new System.Windows.Forms.Splitter();
            panel1.SuspendLayout();
            pnlReplacements.SuspendLayout();
            SuspendLayout();
            // 
            // label1
            // 
            label1.Font = new System.Drawing.Font("Verdana", 9.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            label1.Location = new System.Drawing.Point(200, 8);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(208, 24);
            label1.TabIndex = 0;
            label1.Text = "Add Replacement Values:";
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // utilityReplacementUnit1
            // 
            utilityReplacementUnit1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            utilityReplacementUnit1.FunctionKey = System.Windows.Forms.Keys.None;
            utilityReplacementUnit1.Key = null;
            utilityReplacementUnit1.Location = new System.Drawing.Point(8, 32);
            utilityReplacementUnit1.Name = "utilityReplacementUnit1";
            utilityReplacementUnit1.Size = new System.Drawing.Size(600, 26);
            utilityReplacementUnit1.TabIndex = 1;
            // 
            // btnSubmit
            // 
            btnSubmit.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            btnSubmit.FlatStyle = System.Windows.Forms.FlatStyle.System;
            btnSubmit.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            btnSubmit.Location = new System.Drawing.Point(529, 222);
            btnSubmit.Name = "btnSubmit";
            btnSubmit.Size = new System.Drawing.Size(75, 23);
            btnSubmit.TabIndex = 2;
            btnSubmit.Text = "Submit";
            btnSubmit.Click += new System.EventHandler(btnSubmit_Click);
            // 
            // label2
            // 
            label2.Location = new System.Drawing.Point(8, 8);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(100, 16);
            label2.TabIndex = 4;
            label2.Text = "Scratch Pad:";
            // 
            // rtbSqlScript
            // 
            rtbSqlScript.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            rtbSqlScript.CaseSensitive = false;
            rtbSqlScript.FilterAutoComplete = true;
            rtbSqlScript.Font = new System.Drawing.Font("Lucida Console", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            rtbSqlScript.HighlightDescriptors = highLightDescriptorCollection1;
            rtbSqlScript.HighlightType = SqlSync.Highlighting.SyntaxHightlightType.Sql;
            rtbSqlScript.Location = new System.Drawing.Point(12, 32);
            rtbSqlScript.MaxUndoRedoSteps = 50;
            rtbSqlScript.Name = "rtbSqlScript";
            rtbSqlScript.Size = new System.Drawing.Size(592, 182);
            rtbSqlScript.TabIndex = 8;
            rtbSqlScript.Text = "";
            // 
            // chkInsert
            // 
            chkInsert.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            chkInsert.Enabled = false;
            chkInsert.Location = new System.Drawing.Point(328, 222);
            chkInsert.Name = "chkInsert";
            chkInsert.Size = new System.Drawing.Size(192, 24);
            chkInsert.TabIndex = 9;
            chkInsert.Text = "Insert Scratch Pad Values";
            // 
            // panel1
            // 
            panel1.Controls.Add(chkClipBoard);
            panel1.Controls.Add(label2);
            panel1.Controls.Add(rtbSqlScript);
            panel1.Controls.Add(chkInsert);
            panel1.Controls.Add(btnSubmit);
            panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            panel1.Location = new System.Drawing.Point(0, 64);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(616, 254);
            panel1.TabIndex = 10;
            // 
            // chkClipBoard
            // 
            chkClipBoard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            chkClipBoard.Checked = true;
            chkClipBoard.CheckState = System.Windows.Forms.CheckState.Checked;
            chkClipBoard.Location = new System.Drawing.Point(16, 222);
            chkClipBoard.Name = "chkClipBoard";
            chkClipBoard.Size = new System.Drawing.Size(248, 24);
            chkClipBoard.TabIndex = 10;
            chkClipBoard.Text = "Add Scratch Pad to Clipboard on Close";
            // 
            // pnlReplacements
            // 
            pnlReplacements.Controls.Add(label1);
            pnlReplacements.Controls.Add(utilityReplacementUnit1);
            pnlReplacements.Dock = System.Windows.Forms.DockStyle.Top;
            pnlReplacements.Location = new System.Drawing.Point(0, 0);
            pnlReplacements.Name = "pnlReplacements";
            pnlReplacements.Size = new System.Drawing.Size(616, 64);
            pnlReplacements.TabIndex = 11;
            // 
            // splitter1
            // 
            splitter1.BackColor = System.Drawing.Color.LightGray;
            splitter1.Dock = System.Windows.Forms.DockStyle.Top;
            splitter1.Location = new System.Drawing.Point(0, 64);
            splitter1.Name = "splitter1";
            splitter1.Size = new System.Drawing.Size(616, 3);
            splitter1.TabIndex = 12;
            splitter1.TabStop = false;
            // 
            // UtilityReplacement
            // 
            AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            ClientSize = new System.Drawing.Size(616, 318);
            Controls.Add(splitter1);
            Controls.Add(panel1);
            Controls.Add(pnlReplacements);
            Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            KeyPreview = true;
            Name = "UtilityReplacement";
            Text = "Utility Scripts Replacements";
            Closing += new System.ComponentModel.CancelEventHandler(UtilityReplacement_Closing);
            KeyDown += new System.Windows.Forms.KeyEventHandler(UtilityReplacement_KeyDown);
            Load += new System.EventHandler(UtilityReplacement_Load);
            panel1.ResumeLayout(false);
            pnlReplacements.ResumeLayout(false);
            ResumeLayout(false);

        }
        #endregion

        private void UtilityReplacement_Load(object sender, System.EventArgs e)
        {

            //F1 int value = 112
            int functionKey = 112;
            for (int i = 0; i < keyValues.Length; i++)
            {
                if (keyValues[i].ToUpper() != "<<INSERT>>")
                {
                    UtilityReplacementUnit unit = new UtilityReplacementUnit(keyValues[i], (Keys)functionKey);
                    unit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
                    unit.Size = startSize;
                    functionKey++;
                    unit.Location = startLocation;
                    pnlReplacements.Controls.Add(unit);
                    if (i > 0)
                    {
                        Height += unit.Height;
                        pnlReplacements.Height += unit.Height;
                    }
                    startLocation = new Point(startLocation.X, startLocation.Y + unit.Height);
                }
                else
                {
                    chkInsert.Checked = true;
                    chkInsert.Enabled = true;
                }

            }

            if (inputText.Length == 0)
            {
                IDataObject ido = Clipboard.GetDataObject();
                rtbSqlScript.Text = (string)ido.GetData(DataFormats.Text);
            }
            else
            {
                rtbSqlScript.Text = inputText;
                chkClipBoard.Checked = false;
            }
        }

        private void btnSubmit_Click(object sender, System.EventArgs e)
        {
            Replacements = new StringDictionary();
            foreach (Control ctrl in pnlReplacements.Controls)
            {
                if (ctrl is UtilityReplacementUnit)
                {
                    UtilityReplacementUnit unit = (UtilityReplacementUnit)ctrl;
                    Replacements.Add(unit.Key, unit.Value);
                }
            }

            if (chkInsert.Checked)
                Replacements.Add("<<INSERT>>", rtbSqlScript.Text);
            else
                Replacements.Add("<<INSERT>>", "");


            DialogResult = DialogResult.OK;
            Close();
        }

        private void UtilityReplacement_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (chkClipBoard.Checked)
                Clipboard.SetDataObject(rtbSqlScript.Text);
        }

        private void UtilityReplacement_KeyDown(object sender, System.Windows.Forms.KeyEventArgs e)
        {
            //F1 == 112 --> F12 == 123
            if ((int)e.KeyCode >= 112 && (int)e.KeyCode <= 123)
            {
                string selected = rtbSqlScript.SelectedText.Trim();
                foreach (Control ctrl in pnlReplacements.Controls)
                {
                    if (ctrl is UtilityReplacementUnit && ((UtilityReplacementUnit)ctrl).FunctionKey == e.KeyCode)
                    {
                        ((UtilityReplacementUnit)ctrl).Text = selected;
                        break;
                    }
                }
            }

            if (e.KeyData == Keys.Escape)
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }




    }
}
