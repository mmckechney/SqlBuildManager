namespace SqlSync.SqlBuild.MultiDb
{
    partial class StatusReportForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StatusReportForm));
            this.statusStrip1 = new System.Windows.Forms.StatusStrip();
            this.statGeneral = new System.Windows.Forms.ToolStripStatusLabel();
            this.statDbProcessed = new System.Windows.Forms.ToolStripStatusLabel();
            this.statProgressBar = new System.Windows.Forms.ToolStripProgressBar();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.label1 = new System.Windows.Forms.Label();
            this.ddOutputType = new System.Windows.Forms.ComboBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.lblServerCount = new System.Windows.Forms.Label();
            this.lblDatabaseCount = new System.Windows.Forms.Label();
            this.bgWorker = new System.ComponentModel.BackgroundWorker();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.saveOutputFileDialog = new System.Windows.Forms.SaveFileDialog();
            this.statExecutionTime = new System.Windows.Forms.ToolStripStatusLabel();
            this.timer1 = new System.Windows.Forms.Timer(this.components);
            this.statusStrip1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // statusStrip1
            // 
            this.statusStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.statGeneral,
            this.statExecutionTime,
            this.statDbProcessed,
            this.statProgressBar});
            this.statusStrip1.Location = new System.Drawing.Point(0, 95);
            this.statusStrip1.Name = "statusStrip1";
            this.statusStrip1.Size = new System.Drawing.Size(545, 22);
            this.statusStrip1.TabIndex = 0;
            this.statusStrip1.Text = "statusStrip1";
            // 
            // statGeneral
            // 
            this.statGeneral.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.statGeneral.Name = "statGeneral";
            this.statGeneral.Size = new System.Drawing.Size(135, 17);
            this.statGeneral.Spring = true;
            this.statGeneral.Text = "Ready.";
            this.statGeneral.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statDbProcessed
            // 
            this.statDbProcessed.BorderSides = System.Windows.Forms.ToolStripStatusLabelBorderSides.Right;
            this.statDbProcessed.Name = "statDbProcessed";
            this.statDbProcessed.Size = new System.Drawing.Size(127, 17);
            this.statDbProcessed.Text = "Databases Processed: 0";
            this.statDbProcessed.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // statProgressBar
            // 
            this.statProgressBar.Name = "statProgressBar";
            this.statProgressBar.Size = new System.Drawing.Size(100, 16);
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(273, 11);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(109, 23);
            this.btnGenerate.TabIndex = 1;
            this.btnGenerate.Text = "Generate Report";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(12, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(96, 13);
            this.label1.TabIndex = 2;
            this.label1.Text = "Select output type:";
            // 
            // ddOutputType
            // 
            this.ddOutputType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddOutputType.FormattingEnabled = true;
            this.ddOutputType.Items.AddRange(new object[] {
            "Summary",
            "HTML",
            "CSV",
            "XML"});
            this.ddOutputType.Location = new System.Drawing.Point(123, 12);
            this.ddOutputType.Name = "ddOutputType";
            this.ddOutputType.Size = new System.Drawing.Size(121, 21);
            this.ddOutputType.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 43);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(102, 13);
            this.label2.TabIndex = 4;
            this.label2.Text = "# Servers to Check:";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(12, 66);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(150, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Total # Databases to process:";
            // 
            // lblServerCount
            // 
            this.lblServerCount.AutoSize = true;
            this.lblServerCount.Location = new System.Drawing.Point(169, 43);
            this.lblServerCount.Name = "lblServerCount";
            this.lblServerCount.Size = new System.Drawing.Size(0, 13);
            this.lblServerCount.TabIndex = 6;
            // 
            // lblDatabaseCount
            // 
            this.lblDatabaseCount.AutoSize = true;
            this.lblDatabaseCount.Location = new System.Drawing.Point(169, 66);
            this.lblDatabaseCount.Name = "lblDatabaseCount";
            this.lblDatabaseCount.Size = new System.Drawing.Size(0, 13);
            this.lblDatabaseCount.TabIndex = 7;
            // 
            // bgWorker
            // 
            this.bgWorker.WorkerReportsProgress = true;
            this.bgWorker.DoWork += new System.ComponentModel.DoWorkEventHandler(this.bgWorker_DoWork);
            this.bgWorker.ProgressChanged += new System.ComponentModel.ProgressChangedEventHandler(this.bgWorker_ProgressChanged);
            this.bgWorker.RunWorkerCompleted += new System.ComponentModel.RunWorkerCompletedEventHandler(this.bgWorker_RunWorkerCompleted);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::SqlSync.Properties.Resources.Help_2;
            this.pictureBox1.Location = new System.Drawing.Point(524, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(21, 16);
            this.pictureBox1.TabIndex = 12;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // statExecutionTime
            // 
            this.statExecutionTime.Name = "statExecutionTime";
            this.statExecutionTime.Size = new System.Drawing.Size(135, 17);
            this.statExecutionTime.Spring = true;
            this.statExecutionTime.Text = "Execution Time - 00:00";
            this.statExecutionTime.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // timer1
            // 
            this.timer1.Interval = 1000;
            this.timer1.Tick += new System.EventHandler(this.timer1_Tick);
            // 
            // StatusReportForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(545, 117);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.lblDatabaseCount);
            this.Controls.Add(this.lblServerCount);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ddOutputType);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.btnGenerate);
            this.Controls.Add(this.statusStrip1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "StatusReportForm";
            this.Text = "Multi-Database Script Status Reporting";
            this.Load += new System.EventHandler(this.StatusReport_Load);
            this.statusStrip1.ResumeLayout(false);
            this.statusStrip1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.StatusStrip statusStrip1;
        protected System.Windows.Forms.Button btnGenerate;
        protected System.Windows.Forms.Label label1;
        protected System.Windows.Forms.ComboBox ddOutputType;
        protected System.Windows.Forms.ToolStripStatusLabel statDbProcessed;
        protected System.Windows.Forms.ToolStripProgressBar statProgressBar;
        protected System.Windows.Forms.Label label2;
        protected System.Windows.Forms.Label label3;
        protected System.Windows.Forms.ToolStripStatusLabel statGeneral;
        protected System.ComponentModel.BackgroundWorker bgWorker;
        protected System.Windows.Forms.Label lblServerCount;
        protected System.Windows.Forms.Label lblDatabaseCount;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.SaveFileDialog saveOutputFileDialog;
        private System.Windows.Forms.ToolStripStatusLabel statExecutionTime;
        protected System.Windows.Forms.Timer timer1;
    }
}