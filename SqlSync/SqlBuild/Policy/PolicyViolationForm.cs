using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SqlBuildManager.Enterprise.Policy;
namespace SqlSync.SqlBuild.Policy
{
    public partial class PolicyViolationForm : Form
    {
        Package lstViolations;
        bool multiScriptViolations = false;
        bool showSaveButtons = true;
        private PolicyViolationForm(bool showSaveButtons)
        {
            this.showSaveButtons = showSaveButtons;
            InitializeComponent();
        }
        public PolicyViolationForm(Package lstViolations, bool showSaveButtons)
            : this(showSaveButtons)
        {
            this.lstViolations = lstViolations;
            this.multiScriptViolations = true;
        }
        public PolicyViolationForm(Script  violation, bool showSaveButtons)
            : this(new Package(new Script[] { violation }), showSaveButtons)
        {
            this.multiScriptViolations = false;
        }

        private void PolicyViolationForm_Load(object sender, EventArgs e)
        {
            flowMain.Controls.Clear();

            List<PolicyViolationControl> ctrls = new List<PolicyViolationControl>();
            int height = 0;
            for(int i=0;i<lstViolations.Count;i++)
            {
               
                PolicyViolationControl ctrl = new PolicyViolationControl();
                ctrl.AutoSize = false;
                //ctrl.Size =  new System.Drawing.Size(784, 52);
                ctrl.DataBind(lstViolations[i]);
                ctrls.Add(ctrl);
                height += ctrl.Height;
            }

            //int start = 0;
            for (int i = 0; i < ctrls.Count; i++)
            {
                //ctrls[i].Location = new Point(0, start);
                flowMain.Controls.Add(ctrls[i]);
               // ctrls[i].Size = new Size(908, ctrls[i].Height);
                //start += ctrls[i].Height;
            }

            if (height > 375)
                this.Height = 400;
            else
                this.Height = this.Height + (height + 10 - this.flowMain.Height);

            if (this.multiScriptViolations)
            {
                lblYesButtonMessage.Text = "(Add files and fix later)";
                lblNoButtonMessage.Text = "(Cancel Add)";
                lblContinueMessage.Text = "Do you want to continue to add these files?"; 
            }
            if (this.showSaveButtons)
            {
                this.saveButtonsTablePanel.Visible = true;
                this.btnClose.Visible = false;
            }
            else
            {
                this.saveButtonsTablePanel.Visible = false;
                this.btnClose.Visible = true;
            }
        }

        private void btnYes_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Yes;
            this.Close();
        }

        private void btnNo_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.No;
            this.Close();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            SqlSync.SqlBuild.UtilityHelper.OpenManual("ScriptPolicyChecking");
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Close();
        }
    }
}
