namespace SqlSync.Test
{
    partial class ParameterCtrl
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(ParameterCtrl));
            this.txtParameterValue = new System.Windows.Forms.TextBox();
            this.lblParameterName = new System.Windows.Forms.Label();
            this.btnRemove = new System.Windows.Forms.Button();
            this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
            this.chkUseQuery = new System.Windows.Forms.CheckBox();
            this.SuspendLayout();
            // 
            // txtParameterValue
            // 
            this.txtParameterValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.txtParameterValue.Location = new System.Drawing.Point(137, 2);
            this.txtParameterValue.Name = "txtParameterValue";
            this.txtParameterValue.Size = new System.Drawing.Size(228, 20);
            this.txtParameterValue.TabIndex = 0;
            this.txtParameterValue.TextChanged += new System.EventHandler(this.txtParameterValue_TextChanged);
            // 
            // lblParameterName
            // 
            this.lblParameterName.AutoEllipsis = true;
            this.lblParameterName.Location = new System.Drawing.Point(0, 2);
            this.lblParameterName.Name = "lblParameterName";
            this.lblParameterName.Size = new System.Drawing.Size(137, 20);
            this.lblParameterName.TabIndex = 1;
            this.lblParameterName.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            // 
            // btnRemove
            // 
            this.btnRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnRemove.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnRemove.Image = ((System.Drawing.Image)(resources.GetObject("btnRemove.Image")));
            this.btnRemove.Location = new System.Drawing.Point(443, 2);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(21, 20);
            this.btnRemove.TabIndex = 2;
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Visible = false;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // chkUseQuery
            // 
            this.chkUseQuery.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.chkUseQuery.AutoSize = true;
            this.chkUseQuery.Font = new System.Drawing.Font("Microsoft Sans Serif", 7F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.chkUseQuery.Location = new System.Drawing.Point(371, 4);
            this.chkUseQuery.Name = "chkUseQuery";
            this.chkUseQuery.Size = new System.Drawing.Size(72, 17);
            this.chkUseQuery.TabIndex = 3;
            this.chkUseQuery.Text = "Sql Query";
            this.toolTip1.SetToolTip(this.chkUseQuery, "For multi-dimentional, dynamic tests, use  Sql query\r\nthat returns a single colum" +
                    "n value. \r\nThese values will be blended with other parameter values\r\nto create a" +
                    " multi-dimentional test.");
            this.chkUseQuery.UseVisualStyleBackColor = true;
            this.chkUseQuery.CheckedChanged += new System.EventHandler(this.chkUseQuery_CheckedChanged);
            // 
            // ParameterCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.chkUseQuery);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.lblParameterName);
            this.Controls.Add(this.txtParameterValue);
            this.Name = "ParameterCtrl";
            this.Size = new System.Drawing.Size(469, 25);
            this.Load += new System.EventHandler(this.ParameterCtrl_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.TextBox txtParameterValue;
        private System.Windows.Forms.Label lblParameterName;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.ToolTip toolTip1;
        private System.Windows.Forms.CheckBox chkUseQuery;
    }
}
