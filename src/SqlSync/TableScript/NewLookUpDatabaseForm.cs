using System.Collections;

namespace SqlSync.TableScript
{
    /// <summary>
    /// Summary description for NewLookUpDatabaseForm.
    /// </summary>
    public class NewLookUpDatabaseForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private string[] remainingDatabases;
        private System.Windows.Forms.ListView lstDatabases;
        private string[] selectedDatabases;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public string[] DatabaseList
        {
            get
            {
                if (selectedDatabases == null || selectedDatabases.Length == 0)
                {
                    ArrayList list = new ArrayList();
                    for (int i = 0; i < lstDatabases.CheckedItems.Count; i++)
                    {
                        list.Add(lstDatabases.CheckedItems[i].Text);
                    }
                    selectedDatabases = new string[list.Count];
                    list.CopyTo(selectedDatabases);
                }
                return selectedDatabases;
            }
        }
        public NewLookUpDatabaseForm(string[] remainingDatabases)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            this.remainingDatabases = remainingDatabases;

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(NewLookUpDatabaseForm));
            lstDatabases = new System.Windows.Forms.ListView();
            columnHeader1 = new System.Windows.Forms.ColumnHeader();
            button2 = new System.Windows.Forms.Button();
            button1 = new System.Windows.Forms.Button();
            SuspendLayout();
            // 
            // lstDatabases
            // 
            lstDatabases.Activation = System.Windows.Forms.ItemActivation.OneClick;
            lstDatabases.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            lstDatabases.CheckBoxes = true;
            lstDatabases.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader1});
            lstDatabases.FullRowSelect = true;
            lstDatabases.HideSelection = false;
            lstDatabases.Location = new System.Drawing.Point(19, 10);
            lstDatabases.Name = "lstDatabases";
            lstDatabases.Size = new System.Drawing.Size(223, 445);
            lstDatabases.TabIndex = 0;
            lstDatabases.UseCompatibleStateImageBehavior = false;
            lstDatabases.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Database Name";
            columnHeader1.Width = 203;
            // 
            // button2
            // 
            button2.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            button2.Location = new System.Drawing.Point(137, 465);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(67, 25);
            button2.TabIndex = 2;
            button2.Text = "Cancel";
            // 
            // button1
            // 
            button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            button1.Location = new System.Drawing.Point(60, 465);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(58, 25);
            button1.TabIndex = 1;
            button1.Text = "OK";
            // 
            // NewLookUpDatabaseForm
            // 
            AutoScaleBaseSize = new System.Drawing.Size(6, 16);
            ClientSize = new System.Drawing.Size(264, 504);
            Controls.Add(lstDatabases);
            Controls.Add(button2);
            Controls.Add(button1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            Name = "NewLookUpDatabaseForm";
            Text = "Add Database";
            Load += new System.EventHandler(NewLookUpDatabaseForm_Load);
            ResumeLayout(false);

        }
        #endregion

        private void NewLookUpDatabaseForm_Load(object sender, System.EventArgs e)
        {
            for (int i = 0; i < remainingDatabases.Length; i++)
            {
                lstDatabases.Items.Add(remainingDatabases[i]);
            }
        }
    }
}
