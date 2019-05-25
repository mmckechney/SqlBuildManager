namespace SqlSync.SqlBuild.Policy
{
    partial class PolicyViolationForm
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
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(PolicyViolationForm));
            this.label1 = new System.Windows.Forms.Label();
            this.lblContinueMessage = new System.Windows.Forms.Label();
            this.btnYes = new System.Windows.Forms.Button();
            this.btnNo = new System.Windows.Forms.Button();
            this.lblNoButtonMessage = new System.Windows.Forms.Label();
            this.lblYesButtonMessage = new System.Windows.Forms.Label();
            this.panel2 = new System.Windows.Forms.Panel();
            this.saveButtonsTablePanel = new System.Windows.Forms.TableLayoutPanel();
            this.panel3 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.flowMain = new System.Windows.Forms.FlowLayoutPanel();
            this.btnClose = new System.Windows.Forms.Button();
            this.panel2.SuspendLayout();
            this.saveButtonsTablePanel.SuspendLayout();
            this.panel3.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.label1.Location = new System.Drawing.Point(5, 3);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(309, 17);
            this.label1.TabIndex = 1;
            this.label1.Text = "Your script violates the following policies:";
            // 
            // lblContinueMessage
            // 
            this.lblContinueMessage.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lblContinueMessage.AutoSize = true;
            this.saveButtonsTablePanel.SetColumnSpan(this.lblContinueMessage, 2);
            this.lblContinueMessage.Font = new System.Drawing.Font("Microsoft Sans Serif", 10F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.lblContinueMessage.Location = new System.Drawing.Point(4, 0);
            this.lblContinueMessage.Name = "lblContinueMessage";
            this.lblContinueMessage.Size = new System.Drawing.Size(333, 17);
            this.lblContinueMessage.TabIndex = 2;
            this.lblContinueMessage.Text = "Do you want to correct these violations now?";
            // 
            // btnYes
            // 
            this.btnYes.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnYes.Location = new System.Drawing.Point(41, 20);
            this.btnYes.Name = "btnYes";
            this.btnYes.Size = new System.Drawing.Size(87, 23);
            this.btnYes.TabIndex = 0;
            this.btnYes.Text = "Yes";
            this.btnYes.UseVisualStyleBackColor = true;
            this.btnYes.Click += new System.EventHandler(this.btnYes_Click);
            // 
            // btnNo
            // 
            this.btnNo.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.btnNo.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnNo.Location = new System.Drawing.Point(212, 20);
            this.btnNo.Name = "btnNo";
            this.btnNo.Size = new System.Drawing.Size(87, 23);
            this.btnNo.TabIndex = 1;
            this.btnNo.Text = "No";
            this.btnNo.UseVisualStyleBackColor = true;
            this.btnNo.Click += new System.EventHandler(this.btnNo_Click);
            // 
            // lblNoButtonMessage
            // 
            this.lblNoButtonMessage.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lblNoButtonMessage.AutoSize = true;
            this.lblNoButtonMessage.Location = new System.Drawing.Point(192, 46);
            this.lblNoButtonMessage.Name = "lblNoButtonMessage";
            this.lblNoButtonMessage.Size = new System.Drawing.Size(126, 13);
            this.lblNoButtonMessage.TabIndex = 5;
            this.lblNoButtonMessage.Text = "(Continue with save)";
            // 
            // lblYesButtonMessage
            // 
            this.lblYesButtonMessage.Anchor = System.Windows.Forms.AnchorStyles.Top;
            this.lblYesButtonMessage.AutoSize = true;
            this.lblYesButtonMessage.Location = new System.Drawing.Point(21, 46);
            this.lblYesButtonMessage.Name = "lblYesButtonMessage";
            this.lblYesButtonMessage.Size = new System.Drawing.Size(127, 13);
            this.lblYesButtonMessage.TabIndex = 6;
            this.lblYesButtonMessage.Text = "(Cancel save to edit)";
            // 
            // panel2
            // 
            this.panel2.BackColor = System.Drawing.SystemColors.Control;
            this.panel2.Controls.Add(this.btnClose);
            this.panel2.Controls.Add(this.saveButtonsTablePanel);
            this.panel2.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.panel2.Location = new System.Drawing.Point(0, 348);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(939, 72);
            this.panel2.TabIndex = 8;
            // 
            // saveButtonsTablePanel
            // 
            this.saveButtonsTablePanel.ColumnCount = 2;
            this.saveButtonsTablePanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.saveButtonsTablePanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.saveButtonsTablePanel.Controls.Add(this.lblYesButtonMessage, 0, 2);
            this.saveButtonsTablePanel.Controls.Add(this.btnYes, 0, 1);
            this.saveButtonsTablePanel.Controls.Add(this.lblNoButtonMessage, 1, 2);
            this.saveButtonsTablePanel.Controls.Add(this.btnNo, 1, 1);
            this.saveButtonsTablePanel.Controls.Add(this.lblContinueMessage, 1, 0);
            this.saveButtonsTablePanel.Location = new System.Drawing.Point(300, 3);
            this.saveButtonsTablePanel.Name = "saveButtonsTablePanel";
            this.saveButtonsTablePanel.RowCount = 3;
            this.saveButtonsTablePanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.saveButtonsTablePanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.saveButtonsTablePanel.RowStyles.Add(new System.Windows.Forms.RowStyle());
            this.saveButtonsTablePanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.saveButtonsTablePanel.Size = new System.Drawing.Size(341, 67);
            this.saveButtonsTablePanel.TabIndex = 7;
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.pictureBox1);
            this.panel3.Controls.Add(this.label1);
            this.panel3.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel3.Location = new System.Drawing.Point(0, 0);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(939, 24);
            this.panel3.TabIndex = 9;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox1.Image = global::SqlSync.Properties.Resources.Help_2;
            this.pictureBox1.Location = new System.Drawing.Point(918, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(21, 16);
            this.pictureBox1.TabIndex = 4;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.pictureBox1_Click);
            // 
            // flowMain
            // 
            this.flowMain.AutoScroll = true;
            this.flowMain.Dock = System.Windows.Forms.DockStyle.Fill;
            this.flowMain.Location = new System.Drawing.Point(0, 24);
            this.flowMain.Name = "flowMain";
            this.flowMain.Size = new System.Drawing.Size(939, 324);
            this.flowMain.TabIndex = 10;
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(849, 22);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(75, 23);
            this.btnClose.TabIndex = 8;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // PolicyViolationForm
            // 
            this.AcceptButton = this.btnYes;
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoSize = true;
            this.CancelButton = this.btnNo;
            this.ClientSize = new System.Drawing.Size(939, 420);
            this.Controls.Add(this.flowMain);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel3);
            this.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.Name = "PolicyViolationForm";
            this.Text = "Script Policy Violations Found!";
            this.TopMost = true;
            this.Load += new System.EventHandler(this.PolicyViolationForm_Load);
            this.panel2.ResumeLayout(false);
            this.saveButtonsTablePanel.ResumeLayout(false);
            this.saveButtonsTablePanel.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label lblContinueMessage;
        private System.Windows.Forms.Button btnYes;
        private System.Windows.Forms.Button btnNo;
        private System.Windows.Forms.Label lblNoButtonMessage;
        private System.Windows.Forms.Label lblYesButtonMessage;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.TableLayoutPanel saveButtonsTablePanel;
        private System.Windows.Forms.FlowLayoutPanel flowMain;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Button btnClose;
    }
}