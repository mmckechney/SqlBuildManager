using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
namespace SqlSync.SqlBuild
{
    /// <summary>
    /// Summary description for BulkAddConfirmation.
    /// </summary>
    public class BulkAddConfirmation : System.Windows.Forms.Form
    {
        private System.Windows.Forms.ListView lstBulkAdd;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        private System.Windows.Forms.Button button1;
        private string projectFilePath;
        private List<String> incommingFileList;
        private string[] _SelectedFiles = new string[0];
        private bool createNewEntries = false;
        private SqlSyncBuildData buildData = null;
        public bool CreateNewEntries
        {
            get { return createNewEntries; }
            set { createNewEntries = value; }
        }

        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.RadioButton radUseCurrent;
        private System.Windows.Forms.RadioButton radCreateNew;

        public string[] SelectedFiles
        {
            get { return _SelectedFiles; }
            set { _SelectedFiles = value; }
        }
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public BulkAddConfirmation(List<string> fileList, string projectFilePath, SqlSyncBuildData buildData)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            this.projectFilePath = projectFilePath;
            incommingFileList = fileList;
            this.buildData = buildData;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BulkAddConfirmation));
            lstBulkAdd = new System.Windows.Forms.ListView();
            columnHeader1 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            columnHeader2 = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            button1 = new System.Windows.Forms.Button();
            button2 = new System.Windows.Forms.Button();
            radUseCurrent = new System.Windows.Forms.RadioButton();
            radCreateNew = new System.Windows.Forms.RadioButton();
            label1 = new System.Windows.Forms.Label();
            SuspendLayout();
            // 
            // lstBulkAdd
            // 
            lstBulkAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            lstBulkAdd.CheckBoxes = true;
            lstBulkAdd.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader1,
            columnHeader2});
            lstBulkAdd.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            lstBulkAdd.GridLines = true;
            lstBulkAdd.Location = new System.Drawing.Point(16, 16);
            lstBulkAdd.Name = "lstBulkAdd";
            lstBulkAdd.Size = new System.Drawing.Size(688, 248);
            lstBulkAdd.TabIndex = 0;
            lstBulkAdd.UseCompatibleStateImageBehavior = false;
            lstBulkAdd.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "File Name";
            columnHeader1.Width = 191;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Current Path";
            columnHeader2.Width = 488;
            // 
            // button1
            // 
            button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            button1.Location = new System.Drawing.Point(236, 328);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(128, 23);
            button1.TabIndex = 1;
            button1.Text = "Add Checked Files";
            button1.Click += new System.EventHandler(button1_Click);
            // 
            // button2
            // 
            button2.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            button2.Location = new System.Drawing.Point(372, 328);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(112, 23);
            button2.TabIndex = 2;
            button2.Text = "Cancel";
            button2.Click += new System.EventHandler(button2_Click);
            // 
            // radUseCurrent
            // 
            radUseCurrent.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            radUseCurrent.Checked = true;
            radUseCurrent.Location = new System.Drawing.Point(108, 296);
            radUseCurrent.Name = "radUseCurrent";
            radUseCurrent.Size = new System.Drawing.Size(248, 16);
            radUseCurrent.TabIndex = 3;
            radUseCurrent.TabStop = true;
            radUseCurrent.Text = "Use current script entry for existing files";
            // 
            // radCreateNew
            // 
            radCreateNew.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            radCreateNew.Location = new System.Drawing.Point(356, 296);
            radCreateNew.Name = "radCreateNew";
            radCreateNew.Size = new System.Drawing.Size(256, 16);
            radCreateNew.TabIndex = 4;
            radCreateNew.Text = "Create new script entry for existing files";
            // 
            // label1
            // 
            label1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            label1.Location = new System.Drawing.Point(16, 264);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(256, 23);
            label1.TabIndex = 5;
            label1.Text = "* Colored items denote pre-existing files";
            label1.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // BulkAddConfirmation
            // 
            AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            ClientSize = new System.Drawing.Size(720, 366);
            Controls.Add(label1);
            Controls.Add(radCreateNew);
            Controls.Add(radUseCurrent);
            Controls.Add(button2);
            Controls.Add(button1);
            Controls.Add(lstBulkAdd);
            Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            Name = "BulkAddConfirmation";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Bulk Add Confirmation";
            TopMost = true;
            Closing += new System.ComponentModel.CancelEventHandler(BulkAddConfirmation_Closing);
            Load += new System.EventHandler(BulkAddConfirmation_Load);
            ResumeLayout(false);

        }
        #endregion

        private void BulkAddConfirmation_Load(object sender, System.EventArgs e)
        {
            for (int i = 0; i < incommingFileList.Count; i++)
            {
                System.Drawing.Color bgColor = Color.White;

                var val = from s in buildData.Script.AsEnumerable().Cast<SqlSyncBuildData.ScriptRow>()
                          where s.FileName == Path.GetFileName(incommingFileList[i])
                          select s.FileName;

                bool inProject = (val != null && val.Count() > 0);

                string newLocalFile = Path.Combine(projectFilePath, Path.GetFileName(incommingFileList[i]));
                if (File.Exists(newLocalFile) && inProject)
                    bgColor = Color.PeachPuff;

                ListViewItem item;
                if (incommingFileList[i].StartsWith("Error."))
                {
                    item = new ListViewItem(new string[] { "Error!", incommingFileList[i] });
                    item.Checked = false;
                    item.BackColor = Color.Red;
                }
                else
                {
                    item = new ListViewItem(new string[] { Path.GetFileName(newLocalFile), incommingFileList[i] });
                    item.Checked = true;
                    item.BackColor = bgColor;
                }
                lstBulkAdd.Items.Add(item);

            }

        }

        private void BulkAddConfirmation_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            ArrayList files = new ArrayList();
            for (int i = 0; i < lstBulkAdd.Items.Count; i++)
            {
                if (lstBulkAdd.Items[i].Checked)
                    files.Add(lstBulkAdd.Items[i].SubItems[1].Text);
            }
            _SelectedFiles = new string[files.Count];
            files.CopyTo(_SelectedFiles);

            if (radCreateNew.Checked)
                createNewEntries = true;
            else
                createNewEntries = false;
        }

        private void button2_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }



    }
}
