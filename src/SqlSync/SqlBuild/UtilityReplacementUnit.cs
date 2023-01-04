using System.Windows.Forms;

namespace SqlSync.SqlBuild
{
    /// <summary>
    /// Summary description for UtilityReplacementUnit.
    /// </summary>
    public class UtilityReplacementUnit : System.Windows.Forms.UserControl
    {
        private System.Windows.Forms.Label lblKey;
        private System.Windows.Forms.TextBox txtValue;
        private string keyValue = string.Empty;
        private Keys functionKey;
        private System.Windows.Forms.Label lbFunctionKey;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.ComponentModel.IContainer components;

        public override string Text
        {
            set { if (txtValue != null) txtValue.Text = value; }
        }
        public string Key
        {
            get { return keyValue; }
            set { keyValue = value; }
        }
        public Keys FunctionKey
        {
            get { return functionKey; }
            set { functionKey = value; }
        }
        public string Value
        {
            get
            {
                return txtValue.Text;
            }
        }

        public UtilityReplacementUnit(string keyValue, Keys funcKey)
        {
            InitializeComponent();
            this.keyValue = keyValue;
            functionKey = funcKey;
        }
        public UtilityReplacementUnit()
        {
            // This call is required by the Windows.Forms Form Designer.
            InitializeComponent();
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

        #region Component Designer generated code
        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            components = new System.ComponentModel.Container();
            lblKey = new System.Windows.Forms.Label();
            txtValue = new System.Windows.Forms.TextBox();
            lbFunctionKey = new System.Windows.Forms.Label();
            toolTip1 = new System.Windows.Forms.ToolTip(components);
            SuspendLayout();
            // 
            // lblKey
            // 
            lblKey.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            lblKey.Location = new System.Drawing.Point(0, 0);
            lblKey.Name = "lblKey";
            lblKey.Size = new System.Drawing.Size(156, 20);
            lblKey.TabIndex = 0;
            lblKey.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // txtValue
            // 
            txtValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                | System.Windows.Forms.AnchorStyles.Right)));
            txtValue.Location = new System.Drawing.Point(206, 0);
            txtValue.Name = "txtValue";
            txtValue.Size = new System.Drawing.Size(298, 20);
            txtValue.TabIndex = 1;
            txtValue.Text = "";
            // 
            // lbFunctionKey
            // 
            lbFunctionKey.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
            lbFunctionKey.Location = new System.Drawing.Point(166, 0);
            lbFunctionKey.Name = "lbFunctionKey";
            lbFunctionKey.Size = new System.Drawing.Size(38, 20);
            lbFunctionKey.TabIndex = 2;
            lbFunctionKey.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // UtilityReplacementUnit
            // 
            Controls.Add(lbFunctionKey);
            Controls.Add(txtValue);
            Controls.Add(lblKey);
            Name = "UtilityReplacementUnit";
            Size = new System.Drawing.Size(506, 26);
            Load += new System.EventHandler(UtilityReplacementUnit_Load);
            ResumeLayout(false);

        }
        #endregion

        private void UtilityReplacementUnit_Load(object sender, System.EventArgs e)
        {
            try
            {
                lblKey.Text = keyValue.Replace("<", "").Replace(">", "");
                lbFunctionKey.Text = "<" + functionKey.ToString() + ">";
                toolTip1.SetToolTip(lbFunctionKey, "Click " + lbFunctionKey.Text + " to insert selected Scratch Pad text as value");
            }
            catch
            { }
        }
    }
}
