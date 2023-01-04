using System;
using System.Drawing;
using System.Windows.Forms;

namespace SqlSync
{
    public partial class NewVersion : Form
    {
        //internal System.Security.Principal.WindowsImpersonationContext impersonatedUser = null;
        VersionData verData = null;
        bool isBreaking;
        Size startSize = new Size(0, 0);
        public NewVersion(VersionData verData, bool isBreaking) //, System.Security.Principal.WindowsImpersonationContext impersonatedUser)
        {
            InitializeComponent();
            //this.impersonatedUser = impersonatedUser;
            this.verData = verData;
            this.isBreaking = isBreaking;

        }
        private void NewVersion_Load(object sender, EventArgs e)
        {
            startSize = pnlTop.Size;
            if (verData.LatestVersion != null)
                lblLatestVersionVal.Text = verData.LatestVersion.ToString();
            else
                lblLatestVersionVal.Text = "Unable to read";

            lblYourVersionVal.Text = verData.YourVersion.ToString();
            lnkUpdatePath.Text = verData.UpdateFolder;
            lblContact.Text = verData.Contact;
            lnkContactEMail.Text = verData.ContactEMail;
            rtbReleaseNotes.Text = verData.ReleaseNotes;
            webBrowser1.DocumentText = verData.ReleaseNotes;
            if (!verData.ReleaseNotesAreHtml)
            {
                rtbReleaseNotes.BringToFront();
                webBrowser1.Visible = false;
            }
            else
            {
                rtbReleaseNotes.Visible = false;
            }

            if (verData.LatestVersion != null && verData.ManualCheck && verData.YourVersion >= verData.LatestVersion)
            {
                lblChangeType.Text = "You have the most current version of Sql Build Manager.";
            }
            else if (verData.UpdateFileReadError || verData.LatestVersion == null)
            {
                lblChangeType.Text = @"There was an error contacting the update site. 
If this persists, please alert the contact person below for assistance.";
            }
            else if (isBreaking)
            {
                lblChangeType.ForeColor = Color.Red;
                lblChangeType.Text = @"An important update to Sql Build Manager is available.
Please install the new version as soon as possible.";
            }

            if (verData.YourVersion < verData.LatestVersion)
            {
                lblLatestVersionVal.ForeColor = Color.Green;
                lblYourVersionVal.ForeColor = Color.Red;
            }
            lblCurrentLogin.Text = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
        }

        private void lnkUpdatePath_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            try
            {

                System.Diagnostics.Process.Start(@"C:\Program Files (x86)\Microsoft\Edge\Application\msedge.exe", lnkUpdatePath.Text);
            }
            catch (Exception ex)
            {
                System.Windows.Forms.MessageBox.Show(ex.Message);
            }
        }

        private void lnkContactEMail_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            System.Diagnostics.Process.Start("mailto:" + lnkContactEMail.Text);
        }


        private void lblSqlBuildManager_Click(object sender, EventArgs e)
        {

            if ((int)Control.ModifierKeys == (int)Keys.Control + (int)Keys.Shift)
            {
                if (pnlTop.Size.Height == startSize.Height)
                    pnlTop.Size = new Size(pnlTop.Width, groupBox1.Height + 60);
                else
                    pnlTop.Size = startSize;

            }

        }

        //private void btnImpersonate_Click(object sender, System.EventArgs e)
        //{
        //    try
        //    {
        //        this.impersonatedUser = ImpersonationManager.ImpersonateUser(txtUserId.Text, txtDomain.Text, txtPassword.Text);
        //    }
        //    catch { }

        //    if (this.impersonatedUser != null)
        //        MessageBox.Show("Impersonation Successful", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
        //    else
        //        MessageBox.Show("Impersonation NOT Successful", "Failure", MessageBoxButtons.OK, MessageBoxIcon.Error);

        //    this.lblCurrentLogin.Text = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
        //}

        //private void btnUndo_Click(object sender, System.EventArgs e)
        //{
        //    if (this.impersonatedUser != null)
        //        this.impersonatedUser.Undo();

        //    this.lblCurrentLogin.Text = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
        //}











    }
}