using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;

namespace SqlSync.Test
{
    public partial class ParameterCtrl : UserControl
    {
        private SqlDbType dbType = SqlDbType.NVarChar;

        public SqlDbType DbType
        {
            get { return dbType; }
            set
            {
                dbType = value;
            }
        }
        private int dbLength;

        public int DbLength
        {
            get { return dbLength; }
            set { dbLength = value; }
        }
        private SqlSync.SprocTest.Configuration.Parameter param;
        private SqlSync.SprocTest.ParameterStatus parameterStatus;
        public string ParameterTextValue
        {
            get
            {
                return this.txtParameterValue.Text;
            }
            set
            {
                this.txtParameterValue.Text = value;
            }
        }
        public SqlSync.SprocTest.ParameterStatus ParameterStatus
        {
            get { return parameterStatus; }
            set { parameterStatus = value; }
        }

        public SqlSync.SprocTest.Configuration.Parameter Parameter
        {
            get
            {
                if (param == null)
                    param = new SqlSync.SprocTest.Configuration.Parameter();

                param.Name = lblParameterName.Text;
                param.Value = txtParameterValue.Text;
                param.UseAsQuery = chkUseQuery.Checked;

                return param;
            }
            set
            {
                param = value;
                this.txtParameterValue.Text = param.Value;
                this.lblParameterName.Text = param.Name;
            }
        }
        public ParameterCtrl()
        {
            InitializeComponent();
        }
        public ParameterCtrl(string name)
        {
            InitializeComponent();
            this.lblParameterName.Text = name;
        }
        public ParameterCtrl(ref SqlSync.SprocTest.Configuration.Parameter param)
        {
            InitializeComponent();
            this.param = param;
        }
       

        private void ParameterCtrl_Load(object sender, EventArgs e)
        {
            if (this.param != null)
            {
                this.txtParameterValue.Text = param.Value;
                this.lblParameterName.Text = param.Name;
                this.chkUseQuery.Checked = param.UseAsQuery;
            }

            switch (parameterStatus)
            {
                case SqlSync.SprocTest.ParameterStatus.NotInDatabase:
                    this.lblParameterName.ForeColor = Color.Red;
                    this.txtParameterValue.BackColor = Color.Tomato;
                    this.btnRemove.Visible = true;
                    break;
                case SqlSync.SprocTest.ParameterStatus.NotInTestCase:
                    this.lblParameterName.ForeColor = Color.Blue;
                    this.txtParameterValue.BackColor = Color.PowderBlue;
                    break;
            }

            switch (this.DbType)
            {
                case SqlDbType.Char:
                case SqlDbType.NChar:
                case SqlDbType.NVarChar:
                case SqlDbType.NText:
                case SqlDbType.Text:
                case SqlDbType.VarChar:
                    toolTip1.SetToolTip(this.lblParameterName, "Type: " + this.dbType.ToString() + "(" + this.dbLength.ToString() + ")");
                    break;
                default:
                    toolTip1.SetToolTip(this.lblParameterName, "Type: " + this.dbType.ToString());
                    break;
            }
        }
        public event EventHandler RemoveParameter;
        public event EventHandler DataChanged;

        private void btnRemove_Click(object sender, EventArgs e)
        {
            if(DialogResult.Yes == MessageBox.Show("Are you sure you want to remove the parameter?","Confirm Delete",MessageBoxButtons.YesNo,MessageBoxIcon.Question))
            {
                if(RemoveParameter != null)
                    RemoveParameter(this,EventArgs.Empty);
            }
        }

        private void txtParameterValue_TextChanged(object sender, EventArgs e)
        {
            if (DataChanged != null)
                DataChanged(this, EventArgs.Empty);
        }

        private void chkUseQuery_CheckedChanged(object sender, EventArgs e)
        {
            if (DataChanged != null)
                DataChanged(this, EventArgs.Empty);

        }
    }
}
