using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Windows.Forms;

namespace SqlSync.SqlBuild
{
	/// <summary>
	/// Summary description for UtilityReplacementUnit.
	/// </summary>
	public class UtilityReplacementUnit : System.Windows.Forms.UserControl
	{
		private System.Windows.Forms.Label lblKey;
		private System.Windows.Forms.TextBox txtValue;
		private string keyValue = string.Empty;
		private Keys functionKey;
		private System.Windows.Forms.Label lbFunctionKey;
		private System.Windows.Forms.ToolTip toolTip1;
		private System.ComponentModel.IContainer components;

		public override string Text
		{
			set {  if(this.txtValue != null) this.txtValue.Text = value; }    
		}
		public string Key
		{
			get {  return keyValue; }
			set { keyValue = value; }    
		}
		public Keys FunctionKey
		{
			get {  return functionKey; }
			set { functionKey = value; }    
		}
		public string Value
		{
			get
			{
				return this.txtValue.Text;
			}
		}

		public UtilityReplacementUnit(string keyValue,Keys funcKey)
		{
			InitializeComponent();
			this.keyValue = keyValue;
			this.functionKey = funcKey;
		}
		public UtilityReplacementUnit()
		{
			// This call is required by the Windows.Forms Form Designer.
			InitializeComponent();
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

		#region Component Designer generated code
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.lblKey = new System.Windows.Forms.Label();
			this.txtValue = new System.Windows.Forms.TextBox();
			this.lbFunctionKey = new System.Windows.Forms.Label();
			this.toolTip1 = new System.Windows.Forms.ToolTip(this.components);
			this.SuspendLayout();
			// 
			// lblKey
			// 
			this.lblKey.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lblKey.Location = new System.Drawing.Point(0, 0);
			this.lblKey.Name = "lblKey";
			this.lblKey.Size = new System.Drawing.Size(156, 20);
			this.lblKey.TabIndex = 0;
			this.lblKey.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// txtValue
			// 
			this.txtValue.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
				| System.Windows.Forms.AnchorStyles.Right)));
			this.txtValue.Location = new System.Drawing.Point(206, 0);
			this.txtValue.Name = "txtValue";
			this.txtValue.Size = new System.Drawing.Size(298, 20);
			this.txtValue.TabIndex = 1;
			this.txtValue.Text = "";
			// 
			// lbFunctionKey
			// 
			this.lbFunctionKey.Font = new System.Drawing.Font("Verdana", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((System.Byte)(0)));
			this.lbFunctionKey.Location = new System.Drawing.Point(166, 0);
			this.lbFunctionKey.Name = "lbFunctionKey";
			this.lbFunctionKey.Size = new System.Drawing.Size(38, 20);
			this.lbFunctionKey.TabIndex = 2;
			this.lbFunctionKey.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// UtilityReplacementUnit
			// 
			this.Controls.Add(this.lbFunctionKey);
			this.Controls.Add(this.txtValue);
			this.Controls.Add(this.lblKey);
			this.Name = "UtilityReplacementUnit";
			this.Size = new System.Drawing.Size(506, 26);
			this.Load += new System.EventHandler(this.UtilityReplacementUnit_Load);
			this.ResumeLayout(false);

		}
		#endregion

		private void UtilityReplacementUnit_Load(object sender, System.EventArgs e)
		{
			try
			{
				this.lblKey.Text = this.keyValue.Replace("<","").Replace(">","");
				this.lbFunctionKey.Text = "<"+this.functionKey.ToString()+">";
				this.toolTip1.SetToolTip(this.lbFunctionKey,"Click "+this.lbFunctionKey.Text+" to insert selected Scratch Pad text as value");
			}
			catch
			{}
		}
	}
}
