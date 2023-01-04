using System.Windows.Forms;

namespace SQLSync.SqlBuild.Objects
{
    /// <summary>
    /// Summary description for ActiveDbForm.
    /// </summary>
    public class ActiveDbForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.ComboBox ddDatabaseList;
        private System.Windows.Forms.Button btnOK;
        private string[] databaseList;
        public string SelectedDatabase
        {
            get
            {
                return ddDatabaseList.SelectedItem.ToString();
            }
        }
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public ActiveDbForm(string[] databaseList)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            this.databaseList = databaseList;


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
            System.Resources.ResourceManager resources = new System.Resources.ResourceManager(typeof(ActiveDbForm));
            label4 = new System.Windows.Forms.Label();
            ddDatabaseList = new System.Windows.Forms.ComboBox();
            btnOK = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // label4
            // 
            label4.Location = new System.Drawing.Point(8, 8);
            label4.Name = "label4";
            label4.Size = new System.Drawing.Size(96, 16);
            label4.TabIndex = 13;
            label4.Text = "Select Database:";
            // 
            // ddDatabaseList
            // 
            ddDatabaseList.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            ddDatabaseList.Location = new System.Drawing.Point(104, 8);
            ddDatabaseList.Name = "ddDatabaseList";
            ddDatabaseList.Size = new System.Drawing.Size(176, 21);
            ddDatabaseList.TabIndex = 12;
            // 
            // btnOK
            // 
            btnOK.FlatStyle = System.Windows.Forms.FlatStyle.System;
            btnOK.Location = new System.Drawing.Point(121, 40);
            btnOK.Name = "btnOK";
            btnOK.Size = new System.Drawing.Size(48, 23);
            btnOK.TabIndex = 14;
            btnOK.Text = "OK";
            btnOK.Click += new System.EventHandler(btnOK_Click);
            // 
            // ActiveDbForm
            // 
            AutoScaleBaseSize = new System.Drawing.Size(5, 13);
            ClientSize = new System.Drawing.Size(290, 72);
            Controls.Add(btnOK);
            Controls.Add(label4);
            Controls.Add(ddDatabaseList);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            Name = "ActiveDbForm";
            Text = "Set Active Database";
            Load += new System.EventHandler(ActiveDbForm_Load);
            ResumeLayout(false);

        }
        #endregion

        private void ActiveDbForm_Load(object sender, System.EventArgs e)
        {
            ddDatabaseList.Items.AddRange(databaseList);
            if (ddDatabaseList.Items.Count > 0)
                ddDatabaseList.SelectedIndex = 0;
        }

        private void btnOK_Click(object sender, System.EventArgs e)
        {
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}
