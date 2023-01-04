using SqlSync.DbInformation;
using System;
using System.ComponentModel;
using System.Text;
using System.Windows.Forms;

namespace SqlSync.Analysis
{
    /// <summary>
    /// Summary description for AnalysisForm.
    /// </summary>
    public class AnalysisForm : System.Windows.Forms.Form
    {
        private System.Windows.Forms.MenuStrip mainMenu1;
        private Connection.ConnectionData connData = null;
        private System.Windows.Forms.StatusStrip statusBar1;
        private System.Windows.Forms.ToolStripStatusLabel statStatus;
        private System.Windows.Forms.DataGridView dataGrid1;
        //private System.Windows.Forms.DataGridViewTableStyle dataGridTableStyle1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridTextBoxColumn1;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridTextBoxColumn2;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridTextBoxColumn3;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridTextBoxColumn4;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridTextBoxColumn5;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridTextBoxColumn6;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridTextBoxColumn7;
        private System.Windows.Forms.DataGridViewTextBoxColumn dataGridTextBoxColumn8;
        private SizeAnalysisTable sizeAnalysisTable1;
        private string databaseName;
        private Button btnCopy;
        private IContainer components;

        public AnalysisForm(Connection.ConnectionData connData, string databaseName)
        {
            //
            // Required for Windows Form Designer support
            //
            InitializeComponent();
            this.connData = connData;
            this.databaseName = databaseName;
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
            components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AnalysisForm));
            mainMenu1 = new System.Windows.Forms.MenuStrip();
            statusBar1 = new System.Windows.Forms.StatusStrip();
            statStatus = new System.Windows.Forms.ToolStripStatusLabel();
            dataGrid1 = new System.Windows.Forms.DataGridView();
            //this.dataGridTableStyle1 = new System.Windows.Forms.DataGridViewTableStyle();
            dataGridTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            dataGridTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            dataGridTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            dataGridTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            dataGridTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            dataGridTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            dataGridTextBoxColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            dataGridTextBoxColumn8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            sizeAnalysisTable1 = new SqlSync.DbInformation.SizeAnalysisTable();
            btnCopy = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(dataGrid1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(sizeAnalysisTable1)).BeginInit();
            SuspendLayout();
            // 
            // statusBar1
            // 
            statusBar1.Location = new System.Drawing.Point(0, 340);
            statusBar1.Name = "statusBar1";
            statusBar1.Items.AddRange(new System.Windows.Forms.ToolStripStatusLabel[] {
            statStatus});
            //this.statusBar1.ShowPanels = true;
            statusBar1.Size = new System.Drawing.Size(970, 22);
            statusBar1.TabIndex = 21;
            statusBar1.Text = "statusBar1";
            // 
            // statStatus
            // 
            statStatus.AutoSize = true;
            statStatus.Spring = true;
            statStatus.Name = "statStatus";
            statStatus.Text = "Ready";
            statStatus.Width = 953;
            // 
            // dataGrid1
            // 
            dataGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            dataGrid1.BackgroundColor = System.Drawing.Color.LightGray;
            //this.dataGrid1.CaptionText = "Table Size Data (All sizes are in KB)";
            dataGrid1.DataMember = "";
            //this.dataGrid1.FlatMode = true;
            // this.dataGrid1.GridLineColor = System.Drawing.Color.DarkGray;
            // this.dataGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            dataGrid1.Location = new System.Drawing.Point(12, 12);
            dataGrid1.Name = "dataGrid1";
            dataGrid1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            dataGrid1.Size = new System.Drawing.Size(946, 296);
            dataGrid1.TabIndex = 22;
            //this.dataGrid1.TableStyles.AddRange(new System.Windows.Forms.DataGridTableStyle[] {
            //this.dataGridTableStyle1});
            // 
            // dataGridTableStyle1
            // 
            //this.dataGridTableStyle1.DataGrid = this.dataGrid1;
            //this.dataGridTableStyle1.GridColumnStyles.AddRange(new System.Windows.Forms.DataGridColumnStyle[] {
            //this.dataGridTextBoxColumn1,
            //this.dataGridTextBoxColumn2,
            //this.dataGridTextBoxColumn4,
            //this.dataGridTextBoxColumn5,
            //this.dataGridTextBoxColumn6,
            //this.dataGridTextBoxColumn3,
            //this.dataGridTextBoxColumn7,
            //this.dataGridTextBoxColumn8});
            //this.dataGridTableStyle1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            // 
            // dataGridTextBoxColumn1
            // 
            //this.dataGridTextBoxColumn1.Format = "";
            //this.dataGridTextBoxColumn1.FormatInfo = null;
            dataGridTextBoxColumn1.HeaderText = "Table Name";
            dataGridTextBoxColumn1.DataPropertyName = "Table Name";
            dataGridTextBoxColumn1.Width = 250;
            // 
            // dataGridTextBoxColumn2
            // 
            //this.dataGridTextBoxColumn2.Alignment = System.Windows.Forms.HorizontalAlignment.Right;
            //this.dataGridTextBoxColumn2.Format = "N0";
            //this.dataGridTextBoxColumn2.FormatInfo = null;
            dataGridTextBoxColumn2.HeaderText = "Row Count ";
            dataGridTextBoxColumn2.DataPropertyName = "Row Count";
            //this.dataGridTextBoxColumn2.NullText = "";
            dataGridTextBoxColumn2.Width = 95;
            // 
            // dataGridTextBoxColumn4
            // 
            //this.dataGridTextBoxColumn4.Alignment = System.Windows.Forms.HorizontalAlignment.Right;
            //this.dataGridTextBoxColumn4.Format = "N0";
            //this.dataGridTextBoxColumn4.FormatInfo = null;
            dataGridTextBoxColumn4.HeaderText = "Data ";
            dataGridTextBoxColumn4.DataPropertyName = "Data Size";
            dataGridTextBoxColumn4.Width = 85;
            // 
            // dataGridTextBoxColumn5
            //// 
            //this.dataGridTextBoxColumn5.Alignment = System.Windows.Forms.HorizontalAlignment.Right;
            //this.dataGridTextBoxColumn5.Format = "N0";
            //this.dataGridTextBoxColumn5.FormatInfo = null;
            dataGridTextBoxColumn5.HeaderText = "Indexes ";
            dataGridTextBoxColumn5.DataPropertyName = "Index Size";
            dataGridTextBoxColumn5.Width = 85;
            // 
            // dataGridTextBoxColumn6
            // 
            //this.dataGridTextBoxColumn6.Alignment = System.Windows.Forms.HorizontalAlignment.Right;
            //this.dataGridTextBoxColumn6.Format = "N0";
            //this.dataGridTextBoxColumn6.FormatInfo = null;
            dataGridTextBoxColumn6.HeaderText = "Unused ";
            dataGridTextBoxColumn6.DataPropertyName = "Unused Size";
            dataGridTextBoxColumn6.Width = 85;
            // 
            // dataGridTextBoxColumn3
            // 
            //this.dataGridTextBoxColumn3.Alignment = System.Windows.Forms.HorizontalAlignment.Right;
            //this.dataGridTextBoxColumn3.Format = "N0";
            //this.dataGridTextBoxColumn3.FormatInfo = null;
            dataGridTextBoxColumn3.HeaderText = "Total ";
            dataGridTextBoxColumn3.DataPropertyName = "Total Reserved Size";
            dataGridTextBoxColumn3.Width = 85;
            // 
            // dataGridTextBoxColumn7
            // 
            //this.dataGridTextBoxColumn7.Alignment = System.Windows.Forms.HorizontalAlignment.Right;
            //this.dataGridTextBoxColumn7.Format = "N3";
            //this.dataGridTextBoxColumn7.FormatInfo = null;
            dataGridTextBoxColumn7.HeaderText = "Average Data ";
            dataGridTextBoxColumn7.DataPropertyName = "Average Data Row Size";
            dataGridTextBoxColumn7.Width = 95;
            // 
            // dataGridTextBoxColumn8
            // 
            //this.dataGridTextBoxColumn8.Alignment = System.Windows.Forms.HorizontalAlignment.Right;
            //this.dataGridTextBoxColumn8.Format = "N3";
            //this.dataGridTextBoxColumn8.FormatInfo = null;
            dataGridTextBoxColumn8.HeaderText = "Average Index ";
            dataGridTextBoxColumn8.DataPropertyName = "Average Index Row Size";
            dataGridTextBoxColumn8.Width = 95;
            // 
            // sizeAnalysisTable1
            // 
            sizeAnalysisTable1.TableName = "sizeanalysis";
            // 
            // btnCopy
            // 
            btnCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            btnCopy.Location = new System.Drawing.Point(829, 311);
            btnCopy.Name = "btnCopy";
            btnCopy.Size = new System.Drawing.Size(129, 23);
            btnCopy.TabIndex = 23;
            btnCopy.Text = "Copy to Clipboard";
            btnCopy.UseVisualStyleBackColor = true;
            btnCopy.Click += new System.EventHandler(btnCopy_Click);
            // 
            // AnalysisForm
            // 
            AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            ClientSize = new System.Drawing.Size(970, 362);
            Controls.Add(btnCopy);
            Controls.Add(dataGrid1);
            Controls.Add(statusBar1);
            Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            MainMenuStrip = mainMenu1;
            Name = "AnalysisForm";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "Database Size Analysis for {0}";
            Load += new System.EventHandler(AnalysisForm_Load);
            ((System.ComponentModel.ISupportInitialize)(dataGrid1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(sizeAnalysisTable1)).EndInit();
            ResumeLayout(false);

        }
        #endregion

        private void AnalysisForm_Load(object sender, System.EventArgs e)
        {
            Text = String.Format(Text, databaseName);

            statStatus.Text = "Retrieving Data";
            Cursor = Cursors.WaitCursor;
            try
            {
                connData.DatabaseName = databaseName;
                SizeAnalysisTable tbl = SqlSync.DbInformation.InfoHelper.GetDatabaseSizeAnalysis(connData);
                dataGrid1.DataSource = null;
                //dataGridTableStyle1.MappingName = tbl.TableName;
                dataGrid1.DataSource = tbl;
                double rowCount;
                foreach (SizeAnalysisRow row in tbl.Rows)
                {
                    rowCount = (row.RowCount == 0) ? 1.0 : Convert.ToDouble(row.RowCount);
                    row.AverageDataRowSize = row.DataSize / rowCount;
                    row.AverageIndexRowSize = row.IndexSize / rowCount;
                }

                statStatus.Text = "Ready";
            }
            catch (Exception ex)
            {
                string m = ex.ToString();
                statStatus.Text = "ERROR: Unable to Retrieve Data";
            }
            finally
            {
                Cursor = Cursors.Default;
            }

        }

        private void btnCopy_Click(object sender, EventArgs e)
        {
            if (dataGrid1.DataSource == null)
                return;

            if (dataGrid1.DataSource is SizeAnalysisTable)
            {
                SizeAnalysisTable size = (SizeAnalysisTable)dataGrid1.DataSource;
                StringBuilder sb = new StringBuilder();
                sb.AppendLine("Database Size Analysis");
                sb.AppendLine("Server:\t" + connData.SQLServerName + "\tDatabase:\t" + databaseName);
                //foreach (DataGridColumnStyle style in dataGridTableStyle1.GridColumnStyles)
                //{
                //    sb.Append(style.HeaderText + "\t");
                //}
                sb.AppendLine();


                foreach (SizeAnalysisRow row in size.Rows)
                {
                    for (int i = 0; i < size.Columns.Count; i++)
                        sb.Append(row[i] + "\t");

                    sb.AppendLine();
                }
                Clipboard.SetText(sb.ToString());

            }
        }



    }
}
