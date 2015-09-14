using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using SqlSync.SqlBuild;
using SqlBuildManager.Enterprise;
using SqlBuildManager.Enterprise.CodeReview;
using System.Diagnostics;
namespace SqlSync.Controls
{
    public partial class CodeReviewItemControl : UserControl
    {
        private SqlSyncBuildData.CodeReviewRow reviewRow;
        private bool dataChanged; 
        public CodeReviewItemControl()
        {
            InitializeComponent();
        }
        public CodeReviewItemControl(SqlSyncBuildData.CodeReviewRow reviewRow) : this()
        {
            this.reviewRow = reviewRow;
        }

        private void CodeReviewItemControl_Load(object sender, EventArgs e)
        {
            if (this.reviewRow != null)
            {
                lblReviewer.Text = this.reviewRow.ReviewBy;
                txtComment.Text = this.reviewRow.Comment;
                
                lblDate.Text = this.reviewRow.ReviewDate.ToString("MM/dd/yy HH:mm");
                txtReviewNumber.Text = this.reviewRow.ReviewNumber;

                if (this.reviewRow.ReviewStatus == (short)CodeReviewStatus.OutOfDate)
                {
                    ddStatus.Items.Add("Out of Date");
                    ddStatus.SelectedIndex = ddStatus.Items.Count - 1;
                }
                else
                {
                    ddStatus.SelectedIndex = this.reviewRow.ReviewStatus;
                }
            }
            else
            {
                lblReviewer.Text = System.Environment.UserName;
                txtComment.Text = string.Empty;
                ddStatus.SelectedIndex = 0;
                lblDate.Text = DateTime.Now.ToString("MM/dd/yy HH:mm");
            }


            if (lblReviewer.Text.ToLower() != System.Environment.UserName.ToLower())
            {
                this.Enabled = false;
            }


            if (EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig != null && EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig.LinkToUrlFormat.Length > 0)
            {
                this.txtReviewNumber.DoubleClick +=new EventHandler(txtReviewNumber_DoubleClick);
            }

            this.dataChanged = false;
        }
        public string ReviewBy
        {
            get
            {
                return this.lblReviewer.Text;
            }
        }
        public string Comment
        {
            get
            {
                return this.txtComment.Text;
            }
        }
        public short ReviewStatus
        {
            get
            {
                return (short)this.ddStatus.SelectedIndex;
            }
        }
        public string ReviewNumber
        {
            get
            {
                return this.txtReviewNumber.Text;
            }
        }
        public DateTime ReviewDate
        {
            get
            {
                return DateTime.Now;
            }
        }
        public bool HasChanges
        {
            get
            {
                return this.dataChanged;
            }
        }
        public SqlSyncBuildData.CodeReviewRow GetData()
        {
            if (this.reviewRow != null && this.dataChanged)
            {
                this.reviewRow.ReviewBy = System.Environment.UserName;
                this.reviewRow.ReviewDate = DateTime.Now;
                this.reviewRow.ReviewStatus = (short)ddStatus.SelectedIndex;
                this.reviewRow.Comment = txtComment.Text;
                this.reviewRow.ReviewNumber = txtReviewNumber.Text;
                return this.reviewRow;

            }
            else if (this.reviewRow != null)
            {
                return this.reviewRow;
            }
            else
            {
                return null;
            }
        }

        private void TextValue_Changed(object sender, EventArgs e)
        {
            this.dataChanged = true;
        }

        private void txtReviewNumber_DoubleClick(object sender, EventArgs e)
        {
            Process prc = new Process();
            prc.StartInfo.FileName = String.Format(EnterpriseConfigHelper.EnterpriseConfig.CodeReviewConfig.LinkToUrlFormat, this.txtReviewNumber.Text);
            prc.Start();
        }
    }
}
