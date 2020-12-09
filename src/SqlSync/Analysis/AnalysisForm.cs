using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using SqlSync.DbInformation;
using System.Globalization;
using System.Text;
using System.Data;
using Microsoft.SqlServer.Management.Smo;

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
		protected override void Dispose( bool disposing )
		{
			if( disposing )
			{
				if(components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose( disposing );
		}

		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(AnalysisForm));
            this.mainMenu1 = new System.Windows.Forms.MenuStrip();
            this.statusBar1 = new System.Windows.Forms.StatusStrip();
            this.statStatus = new System.Windows.Forms.ToolStripStatusLabel();
            this.dataGrid1 = new System.Windows.Forms.DataGridView();
            //this.dataGridTableStyle1 = new System.Windows.Forms.DataGridViewTableStyle();
            this.dataGridTextBoxColumn1 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridTextBoxColumn2 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridTextBoxColumn4 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridTextBoxColumn5 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridTextBoxColumn6 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridTextBoxColumn3 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridTextBoxColumn7 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.dataGridTextBoxColumn8 = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.sizeAnalysisTable1 = new SqlSync.DbInformation.SizeAnalysisTable();
            this.btnCopy = new System.Windows.Forms.Button();
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.sizeAnalysisTable1)).BeginInit();
            this.SuspendLayout();
            // 
            // statusBar1
            // 
            this.statusBar1.Location = new System.Drawing.Point(0, 340);
            this.statusBar1.Name = "statusBar1";
            this.statusBar1.Items.AddRange(new System.Windows.Forms.ToolStripStatusLabel[] {
            this.statStatus});
            //this.statusBar1.ShowPanels = true;
            this.statusBar1.Size = new System.Drawing.Size(970, 22);
            this.statusBar1.TabIndex = 21;
            this.statusBar1.Text = "statusBar1";
            // 
            // statStatus
            // 
            this.statStatus.AutoSize = true;
            this.statStatus.Spring = true;
            this.statStatus.Name = "statStatus";
            this.statStatus.Text = "Ready";
            this.statStatus.Width = 953;
            // 
            // dataGrid1
            // 
            this.dataGrid1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.dataGrid1.BackgroundColor = System.Drawing.Color.LightGray;
            //this.dataGrid1.CaptionText = "Table Size Data (All sizes are in KB)";
            this.dataGrid1.DataMember = "";
            //this.dataGrid1.FlatMode = true;
           // this.dataGrid1.GridLineColor = System.Drawing.Color.DarkGray;
           // this.dataGrid1.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.dataGrid1.Location = new System.Drawing.Point(12, 12);
            this.dataGrid1.Name = "dataGrid1";
            this.dataGrid1.RightToLeft = System.Windows.Forms.RightToLeft.No;
            this.dataGrid1.Size = new System.Drawing.Size(946, 296);
            this.dataGrid1.TabIndex = 22;
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
            this.dataGridTextBoxColumn1.HeaderText = "Table Name";
            this.dataGridTextBoxColumn1.DataPropertyName = "Table Name";
            this.dataGridTextBoxColumn1.Width = 250;
            // 
            // dataGridTextBoxColumn2
            // 
            //this.dataGridTextBoxColumn2.Alignment = System.Windows.Forms.HorizontalAlignment.Right;
            //this.dataGridTextBoxColumn2.Format = "N0";
            //this.dataGridTextBoxColumn2.FormatInfo = null;
            this.dataGridTextBoxColumn2.HeaderText = "Row Count ";
            this.dataGridTextBoxColumn2.DataPropertyName = "Row Count";
            //this.dataGridTextBoxColumn2.NullText = "";
            this.dataGridTextBoxColumn2.Width = 95;
            // 
            // dataGridTextBoxColumn4
            // 
            //this.dataGridTextBoxColumn4.Alignment = System.Windows.Forms.HorizontalAlignment.Right;
            //this.dataGridTextBoxColumn4.Format = "N0";
            //this.dataGridTextBoxColumn4.FormatInfo = null;
            this.dataGridTextBoxColumn4.HeaderText = "Data ";
            this.dataGridTextBoxColumn4.DataPropertyName = "Data Size";
            this.dataGridTextBoxColumn4.Width = 85;
            // 
            // dataGridTextBoxColumn5
            //// 
            //this.dataGridTextBoxColumn5.Alignment = System.Windows.Forms.HorizontalAlignment.Right;
            //this.dataGridTextBoxColumn5.Format = "N0";
            //this.dataGridTextBoxColumn5.FormatInfo = null;
            this.dataGridTextBoxColumn5.HeaderText = "Indexes ";
            this.dataGridTextBoxColumn5.DataPropertyName = "Index Size";
            this.dataGridTextBoxColumn5.Width = 85;
            // 
            // dataGridTextBoxColumn6
            // 
            //this.dataGridTextBoxColumn6.Alignment = System.Windows.Forms.HorizontalAlignment.Right;
            //this.dataGridTextBoxColumn6.Format = "N0";
            //this.dataGridTextBoxColumn6.FormatInfo = null;
            this.dataGridTextBoxColumn6.HeaderText = "Unused ";
            this.dataGridTextBoxColumn6.DataPropertyName = "Unused Size";
            this.dataGridTextBoxColumn6.Width = 85;
            // 
            // dataGridTextBoxColumn3
            // 
            //this.dataGridTextBoxColumn3.Alignment = System.Windows.Forms.HorizontalAlignment.Right;
            //this.dataGridTextBoxColumn3.Format = "N0";
            //this.dataGridTextBoxColumn3.FormatInfo = null;
            this.dataGridTextBoxColumn3.HeaderText = "Total ";
            this.dataGridTextBoxColumn3.DataPropertyName = "Total Reserved Size";
            this.dataGridTextBoxColumn3.Width = 85;
            // 
            // dataGridTextBoxColumn7
            // 
            //this.dataGridTextBoxColumn7.Alignment = System.Windows.Forms.HorizontalAlignment.Right;
            //this.dataGridTextBoxColumn7.Format = "N3";
            //this.dataGridTextBoxColumn7.FormatInfo = null;
            this.dataGridTextBoxColumn7.HeaderText = "Average Data ";
            this.dataGridTextBoxColumn7.DataPropertyName = "Average Data Row Size";
            this.dataGridTextBoxColumn7.Width = 95;
            // 
            // dataGridTextBoxColumn8
            // 
            //this.dataGridTextBoxColumn8.Alignment = System.Windows.Forms.HorizontalAlignment.Right;
            //this.dataGridTextBoxColumn8.Format = "N3";
            //this.dataGridTextBoxColumn8.FormatInfo = null;
            this.dataGridTextBoxColumn8.HeaderText = "Average Index ";
            this.dataGridTextBoxColumn8.DataPropertyName = "Average Index Row Size";
            this.dataGridTextBoxColumn8.Width = 95;
            // 
            // sizeAnalysisTable1
            // 
            this.sizeAnalysisTable1.TableName = "sizeanalysis";
            // 
            // btnCopy
            // 
            this.btnCopy.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCopy.Location = new System.Drawing.Point(829, 311);
            this.btnCopy.Name = "btnCopy";
            this.btnCopy.Size = new System.Drawing.Size(129, 23);
            this.btnCopy.TabIndex = 23;
            this.btnCopy.Text = "Copy to Clipboard";
            this.btnCopy.UseVisualStyleBackColor = true;
            this.btnCopy.Click += new System.EventHandler(this.btnCopy_Click);
            // 
            // AnalysisForm
            // 
            this.AutoScaleBaseSize = new System.Drawing.Size(6, 14);
            this.ClientSize = new System.Drawing.Size(970, 362);
            this.Controls.Add(this.btnCopy);
            this.Controls.Add(this.dataGrid1);
            this.Controls.Add(this.statusBar1);
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.mainMenu1;
            this.Name = "AnalysisForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Database Size Analysis for {0}";
            this.Load += new System.EventHandler(this.AnalysisForm_Load);
            ((System.ComponentModel.ISupportInitialize)(this.dataGrid1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.sizeAnalysisTable1)).EndInit();
            this.ResumeLayout(false);

		}
		#endregion

		private void AnalysisForm_Load(object sender, System.EventArgs e)
		{
            this.Text = String.Format(this.Text, this.databaseName);

            this.statStatus.Text = "Retrieving Data";
            this.Cursor = Cursors.WaitCursor;
            try
            {
                this.connData.DatabaseName = this.databaseName;
                SizeAnalysisTable tbl = SqlSync.DbInformation.InfoHelper.GetDatabaseSizeAnalysis(this.connData);
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

                this.statStatus.Text = "Ready";
            }
            catch (Exception ex)
            {
                string m = ex.ToString();
                this.statStatus.Text = "ERROR: Unable to Retrieve Data";
            }
            finally
            {
                this.Cursor = Cursors.Default;
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
                sb.AppendLine("Server:\t" + this.connData.SQLServerName + "\tDatabase:\t" + this.databaseName);
                //foreach (DataGridColumnStyle style in dataGridTableStyle1.GridColumnStyles)
                //{
                //    sb.Append(style.HeaderText + "\t");
                //}
                sb.AppendLine();

            
                foreach (SizeAnalysisRow row in size.Rows)
                {
                    for(int i=0;i<size.Columns.Count;i++)
                        sb.Append(row[i] + "\t");

                    sb.AppendLine();
                }
                Clipboard.SetText(sb.ToString());

            }
        }

	
		
	}
}
