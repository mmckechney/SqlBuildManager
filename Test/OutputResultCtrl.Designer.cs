namespace SqlSync.Test
{
    partial class OutputResultCtrl
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
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OutputResultCtrl));
            this.label1 = new System.Windows.Forms.Label();
            this.txtColumnName = new System.Windows.Forms.TextBox();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.label2 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.txtExpectedValue = new System.Windows.Forms.TextBox();
            this.txtRowNumber = new System.Windows.Forms.TextBox();
            this.btnRemove = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(1, 6);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(76, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Column Name:";
            this.toolTip1.SetToolTip(this.label1, "Leave blank for Execute Scalar");
            // 
            // txtColumnName
            // 
            this.txtColumnName.Location = new System.Drawing.Point(77, 2);
            this.txtColumnName.Name = "txtColumnName";
            this.txtColumnName.Size = new System.Drawing.Size(118, 20);
            this.txtColumnName.TabIndex = 1;
            this.txtColumnName.TextChanged += new System.EventHandler(this.txtColumnName_TextChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(199, 6);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(37, 13);
            this.label2.TabIndex = 2;
            this.label2.Text = "Value:";
            this.toolTip1.SetToolTip(this.label2, "Leave blank for Execute Scalar");
            // 
            // label3
            // 
            this.label3.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(356, 6);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(42, 13);
            this.label3.TabIndex = 4;
            this.label3.Text = "Row #:";
            this.toolTip1.SetToolTip(this.label3, "Leave blank for Execute Scalar");
            // 
            // txtExpectedValue
            // 
            this.txtExpectedValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtExpectedValue.Location = new System.Drawing.Point(236, 2);
            this.txtExpectedValue.Name = "txtExpectedValue";
            this.txtExpectedValue.Size = new System.Drawing.Size(118, 20);
            this.txtExpectedValue.TabIndex = 3;
            this.txtExpectedValue.TextChanged += new System.EventHandler(this.txtColumnName_TextChanged);
            // 
            // txtRowNumber
            // 
            this.txtRowNumber.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.txtRowNumber.Location = new System.Drawing.Point(393, 2);
            this.txtRowNumber.Name = "txtRowNumber";
            this.txtRowNumber.Size = new System.Drawing.Size(40, 20);
            this.txtRowNumber.TabIndex = 5;
            this.txtRowNumber.TextChanged += new System.EventHandler(this.txtColumnName_TextChanged);
            // 
            // btnRemove
            // 
            this.btnRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemove.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnRemove.Image = ((System.Drawing.Image)(resources.GetObject("btnRemove.Image")));
            this.btnRemove.Location = new System.Drawing.Point(440, 2);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(21, 20);
            this.btnRemove.TabIndex = 6;
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // OutputResultCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.txtRowNumber);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.txtExpectedValue);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.txtColumnName);
            this.Controls.Add(this.label1);
            this.Name = "OutputResultCtrl";
            this.Size = new System.Drawing.Size(464, 24);
            this.Load += new System.EventHandler(this.OutputResultCtrl_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.TextBox txtColumnName;
        private System.Windows.Forms.TextBox txtExpectedValue;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.TextBox txtRowNumber;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button btnRemove;
    }
}
