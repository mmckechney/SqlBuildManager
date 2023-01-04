using System.Collections;
using System.Collections.Specialized;

namespace UrielGuy.SyntaxHighlighting
{
    /// <summary>
    /// Summary description for AutoCompleteForm.
    /// </summary>
    public class AutoCompleteForm : System.Windows.Forms.Form
    {
        private StringCollection mItems = new StringCollection();
        private System.Windows.Forms.ListView lstCompleteItems;
        private System.Windows.Forms.ColumnHeader columnHeader1;

        public StringCollection Items
        {
            get
            {
                return mItems;
            }
        }

        internal int ItemHeight
        {
            get
            {
                return 18;
            }
        }

        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;

        public AutoCompleteForm()
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
        }

        public string SelectedItem
        {
            get
            {
                if (lstCompleteItems.SelectedItems.Count == 0) return null;
                return (string)lstCompleteItems.SelectedItems[0].Text;
            }
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
            lstCompleteItems = new System.Windows.Forms.ListView();
            columnHeader1 = new System.Windows.Forms.ColumnHeader();
            SuspendLayout();
            // 
            // lstCompleteItems
            // 
            lstCompleteItems.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader1});
            lstCompleteItems.Dock = System.Windows.Forms.DockStyle.Fill;
            lstCompleteItems.FullRowSelect = true;
            lstCompleteItems.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            lstCompleteItems.HideSelection = false;
            lstCompleteItems.LabelWrap = false;
            lstCompleteItems.Location = new System.Drawing.Point(0, 0);
            lstCompleteItems.MultiSelect = false;
            lstCompleteItems.Name = "lstCompleteItems";
            lstCompleteItems.Size = new System.Drawing.Size(128, 136);
            lstCompleteItems.Sorting = System.Windows.Forms.SortOrder.Ascending;
            lstCompleteItems.TabIndex = 1;
            lstCompleteItems.UseCompatibleStateImageBehavior = false;
            lstCompleteItems.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader1
            // 
            columnHeader1.Width = 148;
            // 
            // AutoCompleteForm
            // 
            AutoScaleBaseSize = new System.Drawing.Size(6, 16);
            ClientSize = new System.Drawing.Size(128, 136);
            ControlBox = false;
            Controls.Add(lstCompleteItems);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            MaximizeBox = false;
            MaximumSize = new System.Drawing.Size(154, 217);
            MinimizeBox = false;
            Name = "AutoCompleteForm";
            ShowInTaskbar = false;
            StartPosition = System.Windows.Forms.FormStartPosition.Manual;
            Text = "AutoCompleteForm";
            TopMost = true;
            VisibleChanged += new System.EventHandler(AutoCompleteForm_VisibleChanged);
            Resize += new System.EventHandler(AutoCompleteForm_Resize);
            ResumeLayout(false);

        }
        #endregion

        private void lstCompleteItems_SelectedIndexChanged(object sender, System.EventArgs e)
        {
        }

        internal int SelectedIndex
        {
            get
            {
                if (lstCompleteItems.SelectedIndices.Count == 0)
                {
                    return -1;
                }
                return lstCompleteItems.SelectedIndices[0];
            }
            set
            {
                lstCompleteItems.Items[value].Selected = true;
            }
        }
        private void AutoCompleteForm_Resize(object sender, System.EventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine(string.Format("Size x:{0} y:{1}\r\n {2}", Size.Width , Size.Height, Environment.StackTrace));
        }

        internal void UpdateView()
        {
            lstCompleteItems.Items.Clear();
            foreach (string item in mItems)
            {
                lstCompleteItems.Items.Add(item);
            }
        }

        private void AutoCompleteForm_VisibleChanged(object sender, System.EventArgs e)
        {
            ArrayList items = new ArrayList(mItems);
            items.Sort(new CaseInsensitiveComparer());
            mItems = new StringCollection();
            mItems.AddRange((string[])items.ToArray(typeof(string)));
            columnHeader1.Width = lstCompleteItems.Width - 20;

        }

        private void lstCompleteItems_Resize(object sender, System.EventArgs e)
        {
            if (Size != lstCompleteItems.Size)
            {

            }
        }

        //		/// <summary>
        //		/// Taking care of Keyboard events
        //		/// </summary>
        //		/// <param name="m"></param>
        //		/// <remarks>
        //		/// Since even when overriding the OnKeyDown methoed and not calling the base function 
        //		/// you don't have full control of the input, I've decided to catch windows messages to handle them.
        //		/// </remarks>
        //		protected override void WndProc(ref Message m)
        //		{
        //			switch (m.Msg)
        //			{				
        //				case Win32.WM_LBUTTONDOWN:
        //					return;
        //			}
        //			base.WndProc (ref m);
        //		}

    }
}
