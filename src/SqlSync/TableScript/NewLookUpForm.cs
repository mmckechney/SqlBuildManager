using System.Collections.Generic;
namespace SqlSync
{
    /// <summary>
    /// Summary description for NewLookUpForm.
    /// </summary>
    public class NewLookUpForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.ListView lstTables;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private List<string> remainingTables;
        private List<string> selectedTables = null;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        public List<string> TableList
        {
            get
            {
                if (selectedTables == null || selectedTables.Count == 0)
                {
                    selectedTables = new List<string>();
                    for (int i = 0; i < lstTables.CheckedItems.Count; i++)
                    {
                        selectedTables.Add(lstTables.CheckedItems[i].Text);
                    }
                }
                return selectedTables;
            }
        }
        public NewLookUpForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            //

            //
        }

        public NewLookUpForm(List<string> remainingTables)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.remainingTables = remainingTables;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewLookUpForm));
            button1 = new System.Windows.Forms.Button();
            button2 = new System.Windows.Forms.Button();
            lstTables = new System.Windows.Forms.ListView();
            columnHeader1 = new System.Windows.Forms.ColumnHeader();
            SuspendLayout();
            // 
            // button1
            // 
            button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            button1.Location = new System.Drawing.Point(48, 470);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(58, 25);
            button1.TabIndex = 1;
            button1.Text = "OK";
            // 
            // button2
            // 
            button2.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            button2.Location = new System.Drawing.Point(125, 470);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(67, 25);
            button2.TabIndex = 2;
            button2.Text = "Cancel";
            // 
            // lstTables
            // 
            lstTables.Activation = System.Windows.Forms.ItemActivation.OneClick;
            lstTables.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            lstTables.CheckBoxes = true;
            lstTables.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader1});
            lstTables.FullRowSelect = true;
            lstTables.HideSelection = false;
            lstTables.Location = new System.Drawing.Point(17, 21);
            lstTables.Name = "lstTables";
            lstTables.Size = new System.Drawing.Size(207, 437);
            lstTables.TabIndex = 0;
            lstTables.UseCompatibleStateImageBehavior = false;
            lstTables.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Table Name";
            columnHeader1.Width = 185;
            // 
            // NewLookUpForm
            // 
            AutoScaleBaseSize = new System.Drawing.Size(6, 16);
            ClientSize = new System.Drawing.Size(241, 510);
            Controls.Add(lstTables);
            Controls.Add(button2);
            Controls.Add(button1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            Name = "NewLookUpForm";
            Text = "Add Table";
            Load += new System.EventHandler(NewLookUpForm_Load);
            ResumeLayout(false);

        }
        #endregion

        private void NewLookUpForm_Load(object sender, System.EventArgs e)
        {
            for (int i = 0; i < remainingTables.Count; i++)
            {
                lstTables.Items.Add(remainingTables[i]);
            }
        }
    }
}
