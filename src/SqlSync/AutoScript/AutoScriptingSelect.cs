using SqlSync.ObjectScript;
using System.Windows.Forms;
namespace SqlSync
{
    /// <summary>
    /// Summary description for AutoScriptingSelect.
    /// </summary>
    public class AutoScriptingSelect : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button btnProceed;
        public AutoScriptingConfig config;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.ListView lstToScript;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.ColumnHeader columnHeader2;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public AutoScriptingSelect(AutoScriptingConfig config)
        {

            InitializeComponent();
            this.config = config;
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AutoScriptingSelect));
            label1 = new System.Windows.Forms.Label();
            btnProceed = new System.Windows.Forms.Button();
            button1 = new System.Windows.Forms.Button();
            lstToScript = new System.Windows.Forms.ListView();
            columnHeader1 = new System.Windows.Forms.ColumnHeader();
            columnHeader2 = new System.Windows.Forms.ColumnHeader();
            SuspendLayout();
            // 
            // label1
            // 
            label1.Location = new System.Drawing.Point(24, 8);
            label1.Name = "label1";
            label1.Size = new System.Drawing.Size(128, 16);
            label1.TabIndex = 1;
            label1.Text = "Select Items to Script";
            // 
            // btnProceed
            // 
            btnProceed.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            btnProceed.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnProceed.Location = new System.Drawing.Point(83, 208);
            btnProceed.Name = "btnProceed";
            btnProceed.Size = new System.Drawing.Size(75, 23);
            btnProceed.TabIndex = 2;
            btnProceed.Text = "Proceed";
            btnProceed.Click += new System.EventHandler(btnProceed_Click);
            // 
            // button1
            // 
            button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            button1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            button1.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            button1.Location = new System.Drawing.Point(171, 208);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(75, 23);
            button1.TabIndex = 3;
            button1.Text = "Cancel";
            button1.Click += new System.EventHandler(button1_Click);
            // 
            // lstToScript
            // 
            lstToScript.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            lstToScript.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            lstToScript.CheckBoxes = true;
            lstToScript.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader1,
            columnHeader2});
            lstToScript.Location = new System.Drawing.Point(24, 24);
            lstToScript.Name = "lstToScript";
            lstToScript.Size = new System.Drawing.Size(280, 168);
            lstToScript.TabIndex = 4;
            lstToScript.UseCompatibleStateImageBehavior = false;
            lstToScript.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Server";
            columnHeader1.Width = 128;
            // 
            // columnHeader2
            // 
            columnHeader2.Text = "Database";
            columnHeader2.Width = 150;
            // 
            // AutoScriptingSelect
            // 
            AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            ClientSize = new System.Drawing.Size(328, 238);
            Controls.Add(lstToScript);
            Controls.Add(button1);
            Controls.Add(btnProceed);
            Controls.Add(label1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            Name = "AutoScriptingSelect";
            Text = "Auto Scripting Select ";
            Load += new System.EventHandler(AutoScriptingSelect_Load);
            ResumeLayout(false);

        }
        #endregion

        private void AutoScriptingSelect_Load(object sender, System.EventArgs e)
        {
            foreach (AutoScriptingConfig.DatabaseScriptConfigRow row in config.DatabaseScriptConfig)
            {
                lstToScript.Items.Add(new ListViewItem(new string[] { row.ServerName, row.DatabaseName }));
            }
        }

        private void button1_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
        }

        private void btnProceed_Click(object sender, System.EventArgs e)
        {
            string dbName;
            string serverName;

            for (int i = 0; i < lstToScript.Items.Count; i++)
            {
                if (lstToScript.Items[i].Checked == false)
                {
                    serverName = lstToScript.Items[i].SubItems[0].Text;
                    dbName = lstToScript.Items[i].SubItems[1].Text;

                    for (short j = 0; j < config.DatabaseScriptConfig.Rows.Count; j++)
                    {
                        if (config.DatabaseScriptConfig[j].DatabaseName == dbName &&
                            config.DatabaseScriptConfig[j].ServerName == serverName)
                        {
                            config.DatabaseScriptConfig[j].Delete();
                            break;
                        }
                    }
                    config.DatabaseScriptConfig.AcceptChanges();
                }
            }
            DialogResult = DialogResult.OK;
            Close();

        }

    }
}
