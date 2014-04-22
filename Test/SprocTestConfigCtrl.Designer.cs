namespace SqlSync.Test
{
    partial class SprocTestConfigCtrl
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
            this.grpTestCase = new System.Windows.Forms.GroupBox();
            this.lnkGetSqlScript = new System.Windows.Forms.LinkLabel();
            this.btnSaveChanges = new System.Windows.Forms.Button();
            this.ddExecutionType = new System.Windows.Forms.ComboBox();
            this.txtCaseName = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.grpParameters = new System.Windows.Forms.GroupBox();
            this.lnkTypeDefault = new System.Windows.Forms.LinkLabel();
            this.lnkPasteParameters = new System.Windows.Forms.LinkLabel();
            this.pnlParameters = new System.Windows.Forms.Panel();
            this.groupBox3 = new System.Windows.Forms.GroupBox();
            this.txtColumnCount = new System.Windows.Forms.TextBox();
            this.label5 = new System.Windows.Forms.Label();
            this.ddResultType = new System.Windows.Forms.ComboBox();
            this.ddRowCountOperator = new System.Windows.Forms.ComboBox();
            this.txtRowCount = new System.Windows.Forms.TextBox();
            this.label3 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.lnkAddOutput = new System.Windows.Forms.LinkLabel();
            this.pnlOutput = new System.Windows.Forms.Panel();
            this.grpTestCase.SuspendLayout();
            this.grpParameters.SuspendLayout();
            this.groupBox3.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // grpTestCase
            // 
            this.grpTestCase.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grpTestCase.Controls.Add(this.lnkGetSqlScript);
            this.grpTestCase.Controls.Add(this.btnSaveChanges);
            this.grpTestCase.Controls.Add(this.ddExecutionType);
            this.grpTestCase.Controls.Add(this.txtCaseName);
            this.grpTestCase.Controls.Add(this.label2);
            this.grpTestCase.Controls.Add(this.label1);
            this.grpTestCase.Location = new System.Drawing.Point(5, 3);
            this.grpTestCase.Name = "grpTestCase";
            this.grpTestCase.Size = new System.Drawing.Size(605, 69);
            this.grpTestCase.TabIndex = 0;
            this.grpTestCase.TabStop = false;
            this.grpTestCase.Text = "Test Case Definition";
            // 
            // lnkGetSqlScript
            // 
            this.lnkGetSqlScript.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkGetSqlScript.AutoSize = true;
            this.lnkGetSqlScript.Location = new System.Drawing.Point(521, 48);
            this.lnkGetSqlScript.Name = "lnkGetSqlScript";
            this.lnkGetSqlScript.Size = new System.Drawing.Size(78, 13);
            this.lnkGetSqlScript.TabIndex = 3;
            this.lnkGetSqlScript.TabStop = true;
            this.lnkGetSqlScript.Text = "Get SQL Script";
            this.lnkGetSqlScript.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkGetSqlScript_LinkClicked);
            // 
            // btnSaveChanges
            // 
            this.btnSaveChanges.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.btnSaveChanges.Enabled = false;
            this.btnSaveChanges.Location = new System.Drawing.Point(507, 18);
            this.btnSaveChanges.Name = "btnSaveChanges";
            this.btnSaveChanges.Size = new System.Drawing.Size(92, 23);
            this.btnSaveChanges.TabIndex = 4;
            this.btnSaveChanges.Text = "Save Changes";
            this.btnSaveChanges.UseVisualStyleBackColor = true;
            this.btnSaveChanges.Click += new System.EventHandler(this.btnSaveChanges_Click);
            // 
            // ddExecutionType
            // 
            this.ddExecutionType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddExecutionType.FormattingEnabled = true;
            this.ddExecutionType.Location = new System.Drawing.Point(106, 44);
            this.ddExecutionType.Name = "ddExecutionType";
            this.ddExecutionType.Size = new System.Drawing.Size(165, 21);
            this.ddExecutionType.TabIndex = 3;
            this.ddExecutionType.SelectionChangeCommitted += new System.EventHandler(this.ddExecutionType_SelectionChangeCommitted);
            // 
            // txtCaseName
            // 
            this.txtCaseName.Location = new System.Drawing.Point(106, 18);
            this.txtCaseName.Name = "txtCaseName";
            this.txtCaseName.Size = new System.Drawing.Size(375, 20);
            this.txtCaseName.TabIndex = 2;
            this.txtCaseName.TextChanged += new System.EventHandler(this.ChildData_DataChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(16, 48);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(84, 13);
            this.label2.TabIndex = 1;
            this.label2.Text = "Execution Type:";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(16, 22);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(38, 13);
            this.label1.TabIndex = 0;
            this.label1.Text = "Name:";
            // 
            // grpParameters
            // 
            this.grpParameters.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.grpParameters.Controls.Add(this.lnkTypeDefault);
            this.grpParameters.Controls.Add(this.lnkPasteParameters);
            this.grpParameters.Controls.Add(this.pnlParameters);
            this.grpParameters.Location = new System.Drawing.Point(5, 74);
            this.grpParameters.Name = "grpParameters";
            this.grpParameters.Size = new System.Drawing.Size(605, 291);
            this.grpParameters.TabIndex = 1;
            this.grpParameters.TabStop = false;
            this.grpParameters.Text = "Parameters";
            // 
            // lnkTypeDefault
            // 
            this.lnkTypeDefault.AutoSize = true;
            this.lnkTypeDefault.Location = new System.Drawing.Point(6, 11);
            this.lnkTypeDefault.Name = "lnkTypeDefault";
            this.lnkTypeDefault.Size = new System.Drawing.Size(132, 13);
            this.lnkTypeDefault.TabIndex = 2;
            this.lnkTypeDefault.TabStop = true;
            this.lnkTypeDefault.Text = "Insert Type Default Values";
            this.lnkTypeDefault.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkTypeDefault_LinkClicked);
            // 
            // lnkPasteParameters
            // 
            this.lnkPasteParameters.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkPasteParameters.AutoSize = true;
            this.lnkPasteParameters.Location = new System.Drawing.Point(484, 11);
            this.lnkPasteParameters.Name = "lnkPasteParameters";
            this.lnkPasteParameters.Size = new System.Drawing.Size(114, 13);
            this.lnkPasteParameters.TabIndex = 1;
            this.lnkPasteParameters.TabStop = true;
            this.lnkPasteParameters.Text = "Paste Execution Script";
            this.lnkPasteParameters.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkPasteParameters_LinkClicked);
            // 
            // pnlParameters
            // 
            this.pnlParameters.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlParameters.AutoScroll = true;
            this.pnlParameters.Location = new System.Drawing.Point(3, 27);
            this.pnlParameters.Name = "pnlParameters";
            this.pnlParameters.Size = new System.Drawing.Size(595, 254);
            this.pnlParameters.TabIndex = 0;
            // 
            // groupBox3
            // 
            this.groupBox3.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox3.Controls.Add(this.txtColumnCount);
            this.groupBox3.Controls.Add(this.label5);
            this.groupBox3.Controls.Add(this.ddResultType);
            this.groupBox3.Controls.Add(this.ddRowCountOperator);
            this.groupBox3.Controls.Add(this.txtRowCount);
            this.groupBox3.Controls.Add(this.label3);
            this.groupBox3.Controls.Add(this.label4);
            this.groupBox3.Location = new System.Drawing.Point(3, 371);
            this.groupBox3.Name = "groupBox3";
            this.groupBox3.Size = new System.Drawing.Size(608, 48);
            this.groupBox3.TabIndex = 2;
            this.groupBox3.TabStop = false;
            this.groupBox3.Text = "Expected Results";
            // 
            // txtColumnCount
            // 
            this.txtColumnCount.Location = new System.Drawing.Point(553, 21);
            this.txtColumnCount.Name = "txtColumnCount";
            this.txtColumnCount.Size = new System.Drawing.Size(47, 20);
            this.txtColumnCount.TabIndex = 10;
            this.txtColumnCount.TextChanged += new System.EventHandler(this.ChildData_DataChanged);
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(477, 25);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(76, 13);
            this.label5.TabIndex = 9;
            this.label5.Text = "Column Count:";
            // 
            // ddResultType
            // 
            this.ddResultType.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddResultType.FormattingEnabled = true;
            this.ddResultType.Location = new System.Drawing.Point(9, 20);
            this.ddResultType.Name = "ddResultType";
            this.ddResultType.Size = new System.Drawing.Size(129, 21);
            this.ddResultType.TabIndex = 8;
            this.ddResultType.SelectionChangeCommitted += new System.EventHandler(this.ddResultType_SelectionChangeCommitted);
            // 
            // ddRowCountOperator
            // 
            this.ddRowCountOperator.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.ddRowCountOperator.FormattingEnabled = true;
            this.ddRowCountOperator.Location = new System.Drawing.Point(355, 21);
            this.ddRowCountOperator.Name = "ddRowCountOperator";
            this.ddRowCountOperator.Size = new System.Drawing.Size(122, 21);
            this.ddRowCountOperator.TabIndex = 7;
            this.ddRowCountOperator.SelectionChangeCommitted += new System.EventHandler(this.ChildData_DataChanged);
            // 
            // txtRowCount
            // 
            this.txtRowCount.Location = new System.Drawing.Point(201, 20);
            this.txtRowCount.Name = "txtRowCount";
            this.txtRowCount.Size = new System.Drawing.Size(47, 20);
            this.txtRowCount.TabIndex = 6;
            this.txtRowCount.TextChanged += new System.EventHandler(this.ChildData_DataChanged);
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Location = new System.Drawing.Point(248, 24);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(107, 13);
            this.label3.TabIndex = 5;
            this.label3.Text = "Row Count Operator:";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(138, 24);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(63, 13);
            this.label4.TabIndex = 4;
            this.label4.Text = "Row Count:";
            // 
            // groupBox2
            // 
            this.groupBox2.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox2.Controls.Add(this.lnkAddOutput);
            this.groupBox2.Controls.Add(this.pnlOutput);
            this.groupBox2.Location = new System.Drawing.Point(3, 425);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(608, 95);
            this.groupBox2.TabIndex = 3;
            this.groupBox2.TabStop = false;
            this.groupBox2.Text = "Expected Data Output ";
            // 
            // lnkAddOutput
            // 
            this.lnkAddOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.lnkAddOutput.AutoSize = true;
            this.lnkAddOutput.Location = new System.Drawing.Point(491, 10);
            this.lnkAddOutput.Name = "lnkAddOutput";
            this.lnkAddOutput.Size = new System.Drawing.Size(109, 13);
            this.lnkAddOutput.TabIndex = 1;
            this.lnkAddOutput.TabStop = true;
            this.lnkAddOutput.Text = "Add Expected Output";
            this.lnkAddOutput.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.lnkAddOutput_LinkClicked);
            // 
            // pnlOutput
            // 
            this.pnlOutput.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlOutput.AutoScroll = true;
            this.pnlOutput.Location = new System.Drawing.Point(3, 26);
            this.pnlOutput.Name = "pnlOutput";
            this.pnlOutput.Size = new System.Drawing.Size(602, 63);
            this.pnlOutput.TabIndex = 0;
            // 
            // SprocTestConfigCtrl
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.AutoScroll = true;
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox3);
            this.Controls.Add(this.grpParameters);
            this.Controls.Add(this.grpTestCase);
            this.Name = "SprocTestConfigCtrl";
            this.Size = new System.Drawing.Size(614, 523);
            this.Load += new System.EventHandler(this.SprocTestConfigCtrl_Load);
            this.grpTestCase.ResumeLayout(false);
            this.grpTestCase.PerformLayout();
            this.grpParameters.ResumeLayout(false);
            this.grpParameters.PerformLayout();
            this.groupBox3.ResumeLayout(false);
            this.groupBox3.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox grpTestCase;
        private System.Windows.Forms.ComboBox ddExecutionType;
        private System.Windows.Forms.TextBox txtCaseName;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.GroupBox grpParameters;
        private System.Windows.Forms.GroupBox groupBox3;
        private System.Windows.Forms.ComboBox ddRowCountOperator;
        private System.Windows.Forms.TextBox txtRowCount;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Panel pnlParameters;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.Panel pnlOutput;
        private System.Windows.Forms.LinkLabel lnkAddOutput;
        private System.Windows.Forms.Button btnSaveChanges;
        private System.Windows.Forms.LinkLabel lnkPasteParameters;
        private System.Windows.Forms.LinkLabel lnkTypeDefault;
        private System.Windows.Forms.LinkLabel lnkGetSqlScript;
        private System.Windows.Forms.ComboBox ddResultType;
        private System.Windows.Forms.TextBox txtColumnCount;
        private System.Windows.Forms.Label label5;

    }
}
