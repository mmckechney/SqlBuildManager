using SqlSync.Connection;
using SqlSync.TableScript;
namespace SqlSync
{
    /// <summary>
    /// Summary description for WhereClauseForm.
    /// </summary>
    public class WhereClauseForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.Button button2;
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.RichTextBox rtbWhereClause;
        private System.Windows.Forms.ListView lstColumns;
        private System.Windows.Forms.ColumnHeader columnHeader1;
        private ConnectionData data;
        private string tableName;
        private System.Windows.Forms.CheckBox chkUseFullSelect;
        private System.Windows.Forms.Label label2;
        private string[] checkKeyColumns;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.ListView lstUpdateKeyColumns;
        private System.Windows.Forms.ColumnHeader columnHeader3;
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.Container components = null;
        public string WhereClause
        {
            get
            {
                string clause = rtbWhereClause.Text.Replace("\"", "'");
                if (clause.Length > 0 &&
                    clause.Trim().ToUpper().StartsWith("WHERE") == false &&
                    chkUseFullSelect.Checked == false)
                {
                    return " WHERE " + clause;
                }
                else
                {
                    return clause;
                }
            }
        }
        public bool UseAsFullSelect
        {
            get
            {
                return chkUseFullSelect.Checked;
            }
        }
        public string[] CheckKeyColumns
        {
            get
            {
                string[] items = new string[lstUpdateKeyColumns.CheckedItems.Count];
                for (int i = 0; i < items.Length; i++)
                {
                    items[i] = lstUpdateKeyColumns.CheckedItems[i].Text;
                }
                return items;
            }
        }

        public WhereClauseForm(ConnectionData data, string sourceTableName, string existingQuery, bool useAsFullSelect, string[] checkKeyColumns)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();

            this.data = data;
            Text = Text += " " + sourceTableName;
            rtbWhereClause.Text = existingQuery;
            tableName = sourceTableName;
            chkUseFullSelect.Checked = useAsFullSelect;
            this.checkKeyColumns = checkKeyColumns;

            //this.rtbWhereClause.DragEnter += new System.Windows.Forms.DragEventHandler(this.richTextBox1_DragEnter);
            //this.rtbWhereClause.DragDrop += new System.Windows.Forms.DragEventHandler(this.richTextBox1_DragDrop);

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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(WhereClauseForm));
            button2 = new System.Windows.Forms.Button();
            button1 = new System.Windows.Forms.Button();
            rtbWhereClause = new System.Windows.Forms.RichTextBox();
            lstColumns = new System.Windows.Forms.ListView();
            columnHeader1 = new System.Windows.Forms.ColumnHeader();
            chkUseFullSelect = new System.Windows.Forms.CheckBox();
            label2 = new System.Windows.Forms.Label();
            label3 = new System.Windows.Forms.Label();
            lstUpdateKeyColumns = new System.Windows.Forms.ListView();
            columnHeader3 = new System.Windows.Forms.ColumnHeader();
            SuspendLayout();
            // 
            // button2
            // 
            button2.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            button2.FlatStyle = System.Windows.Forms.FlatStyle.System;
            button2.Location = new System.Drawing.Point(377, 240);
            button2.Name = "button2";
            button2.Size = new System.Drawing.Size(67, 25);
            button2.TabIndex = 3;
            button2.Text = "Cancel";
            // 
            // button1
            // 
            button1.Anchor = System.Windows.Forms.AnchorStyles.Bottom;
            button1.DialogResult = System.Windows.Forms.DialogResult.OK;
            button1.FlatStyle = System.Windows.Forms.FlatStyle.System;
            button1.Location = new System.Drawing.Point(300, 240);
            button1.Name = "button1";
            button1.Size = new System.Drawing.Size(58, 25);
            button1.TabIndex = 2;
            button1.Text = "OK";
            button1.Click += new System.EventHandler(button1_Click);
            // 
            // rtbWhereClause
            // 
            rtbWhereClause.AllowDrop = true;
            rtbWhereClause.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            rtbWhereClause.Location = new System.Drawing.Point(250, 49);
            rtbWhereClause.Name = "rtbWhereClause";
            rtbWhereClause.Size = new System.Drawing.Size(393, 246);
            rtbWhereClause.TabIndex = 1;
            rtbWhereClause.Text = "";
            // 
            // lstColumns
            // 
            lstColumns.AllowDrop = true;
            lstColumns.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            lstColumns.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            lstColumns.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader1});
            lstColumns.FullRowSelect = true;
            lstColumns.HideSelection = false;
            lstColumns.Location = new System.Drawing.Point(19, 49);
            lstColumns.Name = "lstColumns";
            lstColumns.Size = new System.Drawing.Size(211, 211);
            lstColumns.TabIndex = 0;
            lstColumns.UseCompatibleStateImageBehavior = false;
            lstColumns.View = System.Windows.Forms.View.Details;
            lstColumns.DoubleClick += new System.EventHandler(lstColumns_DoubleClick);
            // 
            // columnHeader1
            // 
            columnHeader1.Text = "Column Name";
            columnHeader1.Width = 171;
            // 
            // chkUseFullSelect
            // 
            chkUseFullSelect.Location = new System.Drawing.Point(250, 20);
            chkUseFullSelect.Name = "chkUseFullSelect";
            chkUseFullSelect.Size = new System.Drawing.Size(259, 19);
            chkUseFullSelect.TabIndex = 4;
            chkUseFullSelect.Text = "Use query as a full SELECT statement";
            // 
            // label2
            // 
            label2.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label2.Location = new System.Drawing.Point(19, 10);
            label2.Name = "label2";
            label2.Size = new System.Drawing.Size(211, 39);
            label2.TabIndex = 7;
            label2.Text = "Click Column Name to add to \"WHERE\" Query";
            // 
            // label3
            // 
            label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            label3.Location = new System.Drawing.Point(662, 10);
            label3.Name = "label3";
            label3.Size = new System.Drawing.Size(212, 39);
            label3.TabIndex = 9;
            label3.Text = "Check Column Names to use as \"EXISTS\" and \"UPDATE WHERE\" Keys";
            // 
            // lstUpdateKeyColumns
            // 
            lstUpdateKeyColumns.AllowDrop = true;
            lstUpdateKeyColumns.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            lstUpdateKeyColumns.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            lstUpdateKeyColumns.CheckBoxes = true;
            lstUpdateKeyColumns.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            columnHeader3});
            lstUpdateKeyColumns.FullRowSelect = true;
            lstUpdateKeyColumns.HideSelection = false;
            lstUpdateKeyColumns.Location = new System.Drawing.Point(662, 49);
            lstUpdateKeyColumns.Name = "lstUpdateKeyColumns";
            lstUpdateKeyColumns.Size = new System.Drawing.Size(212, 211);
            lstUpdateKeyColumns.TabIndex = 8;
            lstUpdateKeyColumns.UseCompatibleStateImageBehavior = false;
            lstUpdateKeyColumns.View = System.Windows.Forms.View.Details;
            // 
            // columnHeader3
            // 
            columnHeader3.Text = "Column Name";
            columnHeader3.Width = 171;
            // 
            // WhereClauseForm
            // 
            AllowDrop = true;
            AutoScaleBaseSize = new System.Drawing.Size(6, 16);
            ClientSize = new System.Drawing.Size(738, 280);
            Controls.Add(label3);
            Controls.Add(lstUpdateKeyColumns);
            Controls.Add(label2);
            Controls.Add(chkUseFullSelect);
            Controls.Add(lstColumns);
            Controls.Add(rtbWhereClause);
            Controls.Add(button2);
            Controls.Add(button1);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedToolWindow;
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            Name = "WhereClauseForm";
            Text = "\"Where\" Clause for ";
            Load += new System.EventHandler(WhereClauseForm_Load);
            ResumeLayout(false);

        }
        #endregion

        private void WhereClauseForm_Load(object sender, System.EventArgs e)
        {
            PopulateHelper helper = new PopulateHelper(data, null);
            string[] columns = SqlSync.DbInformation.InfoHelper.GetColumnNames(tableName, data);
            for (int i = 0; i < columns.Length; i++)
            {
                lstColumns.Items.Add(columns[i]);
            }

            //Pre-check exising "Update key selections"
            for (int i = 0; i < columns.Length; i++)
            {
                lstUpdateKeyColumns.Items.Add(columns[i]);
                for (int j = 0; j < checkKeyColumns.Length; j++)
                {
                    if (columns[i].Trim().ToUpper() == checkKeyColumns[j].Trim().ToUpper())
                    {
                        lstUpdateKeyColumns.Items[lstUpdateKeyColumns.Items.Count - 1].Checked = true;
                        break;
                    }
                }
            }

        }

        private void lstColumns_DoubleClick(object sender, System.EventArgs e)
        {
            if (lstColumns.SelectedItems.Count > 0)
            {
                rtbWhereClause.Text += " [" + lstColumns.SelectedItems[0].Text + "] ";
                //this.rtbWhereClause.Focus();
            }
        }

        private void button1_Click(object sender, System.EventArgs e)
        {

        }



        #region Drag Drop (not used)
#if (true == false)
				private void lstColumns_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
				{
					if(lstColumns.SelectedItems.Count > 0)
					{
						string text = lstColumns.SelectedItems[0].Text;
						lstColumns.DoDragDrop(text,DragDropEffects.Copy | DragDropEffects.Move);
					}

				}

				private void richTextBox1_DragEnter(object sender,System.Windows.Forms.DragEventArgs e)
				{
					if(e.Data.GetDataPresent(DataFormats.Text))
					{
						e.Effect = DragDropEffects.Copy;
					}
					else
					{
						e.Effect = DragDropEffects.None;
					}
				}

				private void richTextBox1_DragDrop(object sender, System.Windows.Forms.DragEventArgs e)
				{
					this.rtbWhereClause.Text += e.Data.GetData(DataFormats.Text).ToString();
					this.lstColumnsMouseDown = false;
				}

				
#endif
        #endregion


    }
}
