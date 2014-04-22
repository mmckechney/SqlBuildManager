namespace SqlSync.Controls
{
    partial class CodeReviewControl
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.picToggle = new System.Windows.Forms.PictureBox();
            this.label1 = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.tblPanel = new System.Windows.Forms.TableLayoutPanel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.panel4 = new System.Windows.Forms.Panel();
            this.lblLastEditor = new System.Windows.Forms.Label();
            this.codeReviewItemControl1 = new SqlSync.Controls.CodeReviewItemControl();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.picToggle)).BeginInit();
            this.tblPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.BackColor = System.Drawing.Color.DimGray;
            this.panel1.Controls.Add(this.lblLastEditor);
            this.panel1.Controls.Add(this.picToggle);
            this.panel1.Controls.Add(this.label1);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1032, 19);
            this.panel1.TabIndex = 1;
            // 
            // picToggle
            // 
            this.picToggle.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.picToggle.Image = global::SqlSync.Properties.Resources.downarrow_white;
            this.picToggle.Location = new System.Drawing.Point(1009, 2);
            this.picToggle.Name = "picToggle";
            this.picToggle.Size = new System.Drawing.Size(20, 18);
            this.picToggle.TabIndex = 23;
            this.picToggle.TabStop = false;
            this.picToggle.Click += new System.EventHandler(this.picToggle_Click);
            // 
            // label1
            // 
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(3, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(92, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Code Reviews:";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.Color.DimGray;
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 61);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(1032, 10);
            this.panel2.TabIndex = 2;
            // 
            // tblPanel
            // 
            this.tblPanel.ColumnCount = 1;
            this.tblPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tblPanel.Controls.Add(this.codeReviewItemControl1, 0, 0);
            this.tblPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tblPanel.Location = new System.Drawing.Point(10, 19);
            this.tblPanel.Name = "tblPanel";
            this.tblPanel.RowCount = 1;
            this.tblPanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.tblPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 42F));
            this.tblPanel.Size = new System.Drawing.Size(1012, 42);
            this.tblPanel.TabIndex = 3;
            // 
            // panel3
            // 
            this.panel3.BackColor = System.Drawing.Color.DimGray;
            this.panel3.Dock = System.Windows.Forms.DockStyle.Left;
            this.panel3.Location = new System.Drawing.Point(0, 19);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(10, 42);
            this.panel3.TabIndex = 3;
            // 
            // panel4
            // 
            this.panel4.BackColor = System.Drawing.Color.DimGray;
            this.panel4.Dock = System.Windows.Forms.DockStyle.Right;
            this.panel4.Location = new System.Drawing.Point(1022, 19);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(10, 42);
            this.panel4.TabIndex = 4;
            // 
            // lblLastEditor
            // 
            this.lblLastEditor.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblLastEditor.ForeColor = System.Drawing.Color.MistyRose;
            this.lblLastEditor.Location = new System.Drawing.Point(97, 3);
            this.lblLastEditor.Name = "lblLastEditor";
            this.lblLastEditor.Size = new System.Drawing.Size(435, 13);
            this.lblLastEditor.TabIndex = 24;
            this.lblLastEditor.Text = "Your were the last editor of this script. Please have someone else review it.";
            this.lblLastEditor.Visible = false;
            // 
            // codeReviewItemControl1
            // 
            this.codeReviewItemControl1.BackColor = System.Drawing.Color.LightSteelBlue;
            this.codeReviewItemControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.codeReviewItemControl1.Location = new System.Drawing.Point(3, 3);
            this.codeReviewItemControl1.Name = "codeReviewItemControl1";
            this.codeReviewItemControl1.Size = new System.Drawing.Size(1006, 36);
            this.codeReviewItemControl1.TabIndex = 0;
            // 
            // CodeReviewControl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.tblPanel);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel1);
            this.Controls.Add(this.panel2);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "CodeReviewControl";
            this.Size = new System.Drawing.Size(1032, 71);
            this.Load += new System.EventHandler(this.CodeReviewControl_Load);
            this.panel1.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.picToggle)).EndInit();
            this.tblPanel.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private CodeReviewItemControl codeReviewItemControl1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.TableLayoutPanel tblPanel;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Panel panel4;
        public System.Windows.Forms.PictureBox picToggle;
        private System.Windows.Forms.Label lblLastEditor;
      

    }
}
