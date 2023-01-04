using System.Windows.Forms;

namespace SqlSync.SqlBuild
{
    /// <summary>
    /// Summary description for FindForm.
    /// </summary>
    public class FindForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.TextBox txtScriptName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        public string ScriptName
        {
            get
            {
                return txtScriptName.Text;
            }
        }
        public FindForm(string priorSearch)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            txtScriptName.Text = priorSearch;
            txtScriptName.SelectAll();
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FindForm));
            txtScriptName = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            btnOK = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // txtScriptName
            // 
            txtScriptName.Location = new System.Drawing.Point(17, 24);
            txtScriptName.Name = "txtScriptName";
            txtScriptName.Size = new System.Drawing.Size(320, 20);
            txtScriptName.TabIndex = 0;
            // 
            // label1
            // 
            label1.Location = new System.Drawing.Point(17, 8);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(100, 16);
            label1.TabIndex = 1;
            label1.Text = "Script Name:";
            // 
            // btnOK
            // 
            btnOK.Location = new System.Drawing.Point(96, 56);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(75, 23);
            btnOK.TabIndex = 2;
            btnOK.Text = "OK";
            btnOK.Click += new System.EventHandler(btnOK_Click);
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.Location = new System.Drawing.Point(184, 56);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(75, 23);
            btnCancel.TabIndex = 3;
            btnCancel.Text = "Cancel";
            btnCancel.Click += new System.EventHandler(btnCancel_Click);
            // 
            // FindForm
            // 
            AcceptButton = btnOK;
            AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(354, 88);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(label1);
            Controls.Add(txtScriptName);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            Name = "FindForm";
            Text = "Find Script By Name";
            ResumeLayout(false);
            PerformLayout();

        }
        #endregion

        private void btnOK_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        private void btnCancel_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }
    }
}
