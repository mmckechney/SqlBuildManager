using System.Windows.Forms;

namespace SqlSync.SqlBuild
{
    /// <summary>
    /// Summary description for FindForm.
    /// </summary>
    public class PackageSignatureForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.TextBox txtScriptName;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.Button btnCancel;
        private Panel panel1;
        private Label label2;
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
        public PackageSignatureForm(string packageHashSignature)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            txtScriptName.Text = packageHashSignature;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PackageSignatureForm));
            txtScriptName = new System.Windows.Forms.TextBox();
            label1 = new System.Windows.Forms.Label();
            btnOK = new System.Windows.Forms.Button();
            btnCancel = new System.Windows.Forms.Button();
            panel1 = new System.Windows.Forms.Panel();
            label2 = new System.Windows.Forms.Label();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // txtScriptName
            // 
            txtScriptName.Location = new System.Drawing.Point(18, 121);
            txtScriptName.Name = "txtScriptName";
            txtScriptName.Size = new System.Drawing.Size(384, 23);
            txtScriptName.TabIndex = 2;
            // 
            // label1
            // 
            label1.Location = new System.Drawing.Point(20, 101);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(120, 20);
            label1.TabIndex = 1;
            label1.Text = "Signature Value:";
            // 
            // btnOK
            // 
            btnOK.Location = new System.Drawing.Point(113, 154);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(90, 28);
            btnOK.TabIndex = 0;
            btnOK.Text = "Copy";
            btnOK.Click += new System.EventHandler(btnOK_Click);
            // 
            // btnCancel
            // 
            btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            btnCancel.Location = new System.Drawing.Point(218, 154);
            btnCancel.Name = "btnCancel";
            btnCancel.Size = new System.Drawing.Size(90, 28);
            btnCancel.TabIndex = 1;
            btnCancel.Text = "Close";
            btnCancel.Click += new System.EventHandler(btnCancel_Click);
            // 
            // panel1
            // 
            panel1.BackColor = System.Drawing.Color.White;
            panel1.Controls.Add(label2);
            panel1.Dock = System.Windows.Forms.DockStyle.Top;
            panel1.Location = new System.Drawing.Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new System.Drawing.Size(350, 97);
            panel1.TabIndex = 3;
            // 
            // label2
            // 
            label2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(0)))), ((int)(((byte)(4)))));
            label2.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            label2.Dock = System.Windows.Forms.DockStyle.Fill;
            label2.Location = new System.Drawing.Point(0, 0);
            label2.Name = "label2";
            label2.Padding = new System.Windows.Forms.Padding(5);
            label2.Size = new System.Drawing.Size(350, 97);
            label2.TabIndex = 4;
            label2.Text = resources.GetString("label2.Text");
            // 
            // PackageSignatureForm
            // 
            AcceptButton = btnOK;
            AutoScaleBaseSize = new System.Drawing.Size(6, 16);
            CancelButton = btnCancel;
            ClientSize = new System.Drawing.Size(350, 155);
            Controls.Add(panel1);
            Controls.Add(btnCancel);
            Controls.Add(btnOK);
            Controls.Add(label1);
            Controls.Add(txtScriptName);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            Name = "PackageSignatureForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Script Package SHA1 Hash Signature";
            panel1.ResumeLayout(false);
            ResumeLayout(false);
            PerformLayout();

        }
        #endregion

        private void btnOK_Click(object sender, System.EventArgs e)
        {
            Clipboard.SetText(txtScriptName.Text);
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
