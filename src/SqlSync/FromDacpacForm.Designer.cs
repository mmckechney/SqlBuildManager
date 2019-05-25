namespace SqlSync
{
    partial class FromDacpacForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FromDacpacForm));
            this.label1 = new System.Windows.Forms.Label();
            this.DACPACPathTextBox = new System.Windows.Forms.TextBox();
            this.BrowseForDACPACButton = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBox1 = new System.Windows.Forms.ComboBox();
            this.browseForDacpacButtonToolTip = new System.Windows.Forms.ToolTip(this.components);
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(12, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(74, 18);
            this.label1.TabIndex = 0;
            this.label1.Text = "DACPAC:";
            // 
            // DACPACPathTextBox
            // 
            this.DACPACPathTextBox.Location = new System.Drawing.Point(99, 20);
            this.DACPACPathTextBox.Name = "DACPACPathTextBox";
            this.DACPACPathTextBox.Size = new System.Drawing.Size(307, 22);
            this.DACPACPathTextBox.TabIndex = 1;
            this.DACPACPathTextBox.TextChanged += new System.EventHandler(this.DACPACPathTextBox_TextChanged);
            // 
            // BrowseForDACPACButton
            // 
            this.BrowseForDACPACButton.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.BrowseForDACPACButton.Location = new System.Drawing.Point(412, 20);
            this.BrowseForDACPACButton.Name = "BrowseForDACPACButton";
            this.BrowseForDACPACButton.Size = new System.Drawing.Size(32, 22);
            this.BrowseForDACPACButton.TabIndex = 2;
            this.BrowseForDACPACButton.Text = "...";
            this.browseForDacpacButtonToolTip.SetToolTip(this.BrowseForDACPACButton, "Browse for DACPAC");
            this.BrowseForDACPACButton.UseVisualStyleBackColor = true;
            this.BrowseForDACPACButton.Click += new System.EventHandler(this.BrowseForDACPACButton_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Tahoma", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label2.Location = new System.Drawing.Point(12, 78);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(83, 18);
            this.label2.TabIndex = 3;
            this.label2.Text = "Database:";
            // 
            // comboBox1
            // 
            this.comboBox1.FormattingEnabled = true;
            this.comboBox1.Location = new System.Drawing.Point(106, 78);
            this.comboBox1.Name = "comboBox1";
            this.comboBox1.Size = new System.Drawing.Size(299, 24);
            this.comboBox1.TabIndex = 4;
            // 
            // browseForDacpacButtonToolTip
            // 
            this.browseForDacpacButtonToolTip.Tag = "";
            // 
            // FromDacpacForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(482, 405);
            this.Controls.Add(this.comboBox1);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.BrowseForDACPACButton);
            this.Controls.Add(this.DACPACPathTextBox);
            this.Controls.Add(this.label1);
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "FromDacpacForm";
            this.Text = "SQB Build from DACPAC/DB Delta";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox DACPACPathTextBox;
        private System.Windows.Forms.Button BrowseForDACPACButton;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBox1;
        private System.Windows.Forms.ToolTip browseForDacpacButtonToolTip;
    }
}