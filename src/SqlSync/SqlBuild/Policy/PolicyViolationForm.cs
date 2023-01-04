using SqlBuildManager.Enterprise.Policy;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
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
            multiScriptViolations = true;
        }
        public PolicyViolationForm(Script violation, bool showSaveButtons)
            : this(new Package(new Script[] { violation }), showSaveButtons)
        {
            multiScriptViolations = false;
        }

        private void PolicyViolationForm_Load(object sender, EventArgs e)
        {
            flowMain.Controls.Clear();

            List<PolicyViolationControl> ctrls = new List<PolicyViolationControl>();
            int height = 0;
            for (int i = 0; i < lstViolations.Count; i++)
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
                Height = 400;
            else
                Height = Height + (height + 10 - flowMain.Height);

            if (multiScriptViolations)
            {
                lblYesButtonMessage.Text = "(Add files and fix later)";
                lblNoButtonMessage.Text = "(Cancel Add)";
                lblContinueMessage.Text = "Do you want to continue to add these files?";
            }
            if (showSaveButtons)
            {
                saveButtonsTablePanel.Visible = true;
                btnClose.Visible = false;
            }
            else
            {
                saveButtonsTablePanel.Visible = false;
                btnClose.Visible = true;
            }
        }

        private void btnYes_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Yes;
            Close();
        }

        private void btnNo_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.No;
            Close();
        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            SqlSync.SqlBuild.UtilityHelper.OpenManual("ScriptPolicyChecking");
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            DialogResult = System.Windows.Forms.DialogResult.Cancel;
            Close();
        }
    }
}
