using System.Windows.Forms;

namespace SqlSync.SqlBuild
{
    /// <summary>
    /// Summary description for BulkAddListForm.
    /// </summary>
    public class BulkAddListForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnCancel;
        private string[] _SelectedFiles;
        private System.Windows.Forms.RichTextBox rtbFiles;

        public string[] SelectedFiles
        {
            get { return _SelectedFiles; }
            set { _SelectedFiles = value; }
        }
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public BulkAddListForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //

            //
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(BulkAddListForm));
            rtbFiles = new System.Windows.Forms.RichTextBox();
            label1 = new System.Windows.Forms.Label();
            btnAdd = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // rtbFiles
            // 
            rtbFiles.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            rtbFiles.Location = new System.Drawing.Point(16, 30);
            rtbFiles.Name = "rtbFiles";
            rtbFiles.Size = new System.Drawing.Size(691, 217);
            rtbFiles.TabIndex = 0;
            rtbFiles.Text = "";
            // 
            // label1
            // 
            label1.Location = new System.Drawing.Point(16, 10);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(825, 20);
            label1.TabIndex = 1;
            label1.Text = "Copy file names (one per line)";
            // 
            // btnAdd
            // 
            btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            btnAdd.FlatStyle = System.Windows.Forms.FlatStyle.System;
            btnAdd.Location = new System.Drawing.Point(336, 257);
            btnAdd.Name = "btnAdd";
            btnAdd.Size = new System.Drawing.Size(0, 28);
            btnAdd.TabIndex = 2;
            btnAdd.Text = "Add Files";
            btnAdd.Click += new System.EventHandler(btnAdd_Click);
            // 
            // btnCancel
            // 
            btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.System;
            btnCancel.Location = new System.Drawing.Point(442, 257);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(0, 28);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "Cancel";
            btnCancel.Click += new System.EventHandler(btnCancel_Click);
            // 
            // BulkAddListForm
            // 
            AutoScaleBaseSize = new System.Drawing.Size(6, 16);
            ClientSize = new System.Drawing.Size(722, 296);
            Controls.Add(btnCancel);
            Controls.Add(btnAdd);
            Controls.Add(label1);
            Controls.Add(rtbFiles);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            Name = "BulkAddListForm";
            Text = "Bulk Add From List ";
            ResumeLayout(false);

        }
        #endregion

        private void btnAdd_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
            SelectedFiles = rtbFiles.Lines;
            Close();
        }

        private void btnCancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
