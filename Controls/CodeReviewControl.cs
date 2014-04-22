using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using SqlSync.SqlBuild;
using SqlBuildManager.Enterprise.CodeReview;
using System.Linq;
using System.Threading.Tasks;
using SqlBuildManager.Enterprise;
namespace SqlSync.Controls
{
    public partial class CodeReviewControl : UserControl
    {
        public CodeReviewControl()
        {
            InitializeComponent();
        }
        public bool HasChanges
        {
            get;
            set;
        }

        SqlSyncBuildData.ScriptRow scriptRow = null;
        SqlSyncBuildData buildData = null;
        private void CodeReviewControl_Load(object sender, EventArgs e)
        {
           
           
        }

        private int fullHeight;
        private int collapsedHeight = 19;
        bool showFull = true;
        private string startingScriptText = string.Empty;
        internal void BindData(ref SqlSyncBuildData buildData, ref SqlSyncBuildData.ScriptRow scriptRow, string scriptText, string lastEditor)
        {
            this.startingScriptText = scriptText;

            //Can't create a new review for yourself unless you are a "SelfReviewer"
            var isSelfReviewer = from s in EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig.SelfReviewer
                                 where s.LoginId.ToLower() == lastEditor.ToLower()
                                 select s.LoginId;

            if (  (lastEditor.Length == 0 || lastEditor.Trim().ToLower() == System.Environment.UserName.ToLower().Trim()) && !isSelfReviewer.Any())
            {
                this.Height = collapsedHeight;
                this.tblPanel.Controls.Clear();
                if (lastEditor.Length > 0)
                    lblLastEditor.Visible = true;
            }


            this.scriptRow = scriptRow;
            this.buildData = buildData;

            SqlSyncBuildData.CodeReviewRow[] rows = scriptRow.GetCodeReviewRows();
            if (rows != null && rows.Length > 0)
            {


                var sorted = from r in rows
                             orderby r.ReviewDate descending
                             select r;

                foreach (SqlSyncBuildData.CodeReviewRow row in sorted)
                {
                    if (!CodeReviewManager.ValidateReviewCheckSum(row, scriptText))
                    {
                        row.ReviewStatus = (short)CodeReviewStatus.OutOfDate; ;
                        this.buildData.AcceptChanges();
                    }

                    CodeReviewItemControl itemCtrl = new CodeReviewItemControl(row);
                    itemCtrl.Dock = DockStyle.Fill;
                    this.tblPanel.Controls.Add(itemCtrl);
                    this.Height = this.Height + itemCtrl.Height + 7;
                }
            }
            this.fullHeight = this.Height;
        }

        internal void SaveData(string scriptText)
        {
            foreach (Control ctrl in this.tblPanel.Controls)
            {
                try
                {
                    if (ctrl is CodeReviewItemControl)
                    {
                        CodeReviewItemControl tmp = (CodeReviewItemControl)ctrl;
                        SqlSyncBuildData.CodeReviewRow existingRow = tmp.GetData();
                        if (existingRow == null && tmp.HasChanges)
                        {
                            CodeReviewManager.SaveCodeReview(ref buildData, ref scriptRow, scriptText, tmp.Comment, tmp.ReviewBy, tmp.ReviewDate, tmp.ReviewNumber, tmp.ReviewStatus);
                            this.HasChanges = true;
                        }
                        else if (existingRow != null && tmp.HasChanges)
                        {
                            CodeReviewManager.UpdateCodeReview(ref buildData, ref existingRow, scriptText);
                            this.HasChanges = true;
                        }
                        else if (this.startingScriptText != scriptText)
                        {
                            CodeReviewManager.MarkCodeReviewOutOfDate(ref buildData, ref existingRow);
                            this.HasChanges = true;
                        }
                    }
                }
                catch
                {
                }
            }
        }
        

        private void picToggle_Click(object sender, EventArgs e)
        {
            if (sender is System.Windows.Forms.PictureBox)
                this.showFull = !this.showFull;

            if (showFull)
            {
                this.Height = fullHeight;
                this.picToggle.Image = global::SqlSync.Properties.Resources.downarrow_white;
            }
            else
            {
                this.Height = collapsedHeight;
                this.picToggle.Image = global::SqlSync.Properties.Resources.uparrow_white;
            }


        }
    }
    

}
