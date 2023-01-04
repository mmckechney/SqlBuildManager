using SqlBuildManager.Enterprise.Policy;
using System.Windows.Forms;
namespace SqlSync.SqlBuild.Policy
{
    public partial class PolicyViolationControl : UserControl
    {

        public PolicyViolationControl()
        {
            InitializeComponent();
        }
        public void DataBind(Script item)
        {
            lblScriptName.Text = item.ScriptName;

            tableLayoutPanel1.RowCount = item.Count + 1;
            tableLayoutPanel1.Height = tableLayoutPanel1.Height + (27 * item.Count);
            Height = tableLayoutPanel1.Height + 35;

            for (int i = 0; i < item.Count; i++)
            {
                Violation vol = item[i];
                tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
                Label tmpViolation = new Label();
                tmpViolation.AutoSize = true;
                tmpViolation.Text = vol.Name;
                tableLayoutPanel1.Controls.Add(tmpViolation, 0, i + 1);
                Label tmpMessage = new Label();
                tmpMessage.AutoSize = true;
                tmpMessage.Text = vol.Message;
                tableLayoutPanel1.Controls.Add(tmpMessage, 1, i + 1);
            }
            tableLayoutPanel1.Anchor = AnchorStyles.Top | AnchorStyles.Left;
        }
    }
}
