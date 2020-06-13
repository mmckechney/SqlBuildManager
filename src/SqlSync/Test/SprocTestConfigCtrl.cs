using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using SqlSync.SprocTest.Configuration;
using SqlSync.SprocTest;
using Microsoft.Data.SqlClient;
namespace SqlSync.Test
{
    public partial class SprocTestConfigCtrl : UserControl
    {
        TestCase tCase = null;
        private bool dataChanged = false;
        private bool isNew = false;
        private string sprocName = string.Empty;

        public string SprocName
        {
            get { return sprocName; }
            set { sprocName = value; }
        }
        private string databaseName = string.Empty;
        public bool DataChanged
        {            
            set { dataChanged = value;
            btnSaveChanges.Enabled = dataChanged;
        }
        }
       

        public TestCase TestCase
        {
            get { return tCase; }
            set
            {
                tCase = value;
                if(tCase != null)
                    BindData();
            }
        }
        SqlParameterCollection derivedParameters = null;
        public SprocTestConfigCtrl()
        {
            InitializeComponent();
            
        }

        internal void SetTestCaseData(TestCase tCase, SqlParameterCollection derivedParameters, string sprocName, string databaseName)
        {
            SetTestCaseData(tCase, derivedParameters, sprocName, databaseName,false);
        }
        internal void SetTestCaseData(TestCase tCase, SqlParameterCollection derivedParameters,string sprocName, string databaseName, bool isNew)
        {
            this.isNew = isNew;
            this.tCase = tCase;
            this.derivedParameters = derivedParameters;
            this.sprocName = sprocName;
            this.databaseName = databaseName;
            BindData();
        }
        private void BindData()
        {

            this.grpTestCase.Text = "Test Case Definition for " + this.sprocName;
            pnlParameters.Controls.Clear();
            pnlOutput.Controls.Clear();

            List<ParameterCtrl> paramCtrls = new List<ParameterCtrl>();
            this.txtCaseName.Text = this.tCase.Name;
            this.ddExecutionType.SelectedItem = this.tCase.ExecuteType;

            #region Add Parameter controls
            bool foundMatch;
            Point paramStart = new Point(3, 3);

            if (derivedParameters != null && derivedParameters.Count > 0)
            {
                //Loop through derived parameters and match to test case. 
                for (int i = 0; i < derivedParameters.Count; i++)
                {
                    foundMatch = false;
                    if (this.tCase.Parameter != null)
                        for (int j = 0; j < this.tCase.Parameter.Length; j++)
                        {
                            if (this.tCase.Parameter[j].Name.ToLower() == this.derivedParameters[i].ParameterName.ToLower())
                            {
                                this.tCase.Parameter[j].HasDerivedParameterMatch = true;
                                ParameterCtrl ctrl = new ParameterCtrl(ref this.tCase.Parameter[j]);
                                ctrl.ParameterStatus = ParameterStatus.Matching;
                                ctrl.DbType = this.derivedParameters[i].SqlDbType;
                                ctrl.DbLength = this.derivedParameters[i].Size;
                                paramCtrls.Add(ctrl);
                                foundMatch = true;
                                break;
                              // new Int32Converter().ConvertFrom(this.derivedParameters[i].Precision)
                            }
                        }

                    if (!foundMatch)
                    {
                        ParameterCtrl ctrl = new ParameterCtrl(this.derivedParameters[i].ParameterName);
                        ctrl.ParameterStatus = ParameterStatus.NotInTestCase;
                        ctrl.DbType = this.derivedParameters[i].SqlDbType;
                        ctrl.DbLength = this.derivedParameters[i].Size;
                        paramCtrls.Add(ctrl);
                    }
                }

                //Loop through the test case and add any "extra" parameters.
                if (this.tCase.Parameter != null)
                    for (int i = 0; i < this.tCase.Parameter.Length; i++)
                    {
                        if (!this.tCase.Parameter[i].HasDerivedParameterMatch)
                        {
                            ParameterCtrl ctrl = new ParameterCtrl(ref this.tCase.Parameter[i]);
                            ctrl.ParameterStatus = ParameterStatus.NotInDatabase;
                            paramCtrls.Add(ctrl);

                        }
                    }
            }
            else if (this.tCase.Parameter != null)
            {
                //If there are no derived parameters...
                for (int i = 0; i < this.tCase.Parameter.Length; i++)
                {
                    ParameterCtrl ctrl = new ParameterCtrl(ref this.tCase.Parameter[i]);
                    ctrl.ParameterStatus = ParameterStatus.NotInDatabase;
                    paramCtrls.Add(ctrl);
                }
            }


            for (int i = 0; i < paramCtrls.Count; i++)
            {
                paramCtrls[i].Location = paramStart;
                paramStart.Y += paramCtrls[i].Height;
                paramCtrls[i].Width = pnlParameters.Width - 10;
                paramCtrls[i].RemoveParameter += new EventHandler(SprocTestConfigCtrl_RemoveParameter);
                paramCtrls[i].DataChanged += new EventHandler(ChildData_DataChanged);
                paramCtrls[i].Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                pnlParameters.Controls.Add(paramCtrls[i]);
            }


            #endregion

            if (this.tCase.ExpectedResult != null)
            {
                if (this.tCase.ExpectedResult.RowCountSpecified)
                    this.txtRowCount.Text = this.tCase.ExpectedResult.RowCount.ToString();
                else
                    this.txtRowCount.Text = "";

                if (this.tCase.ExpectedResult.ColumnCountSpecified)
                    this.txtColumnCount.Text = this.tCase.ExpectedResult.ColumnCount.ToString();
                else
                    this.txtColumnCount.Text = "";

                if (this.tCase.ExpectedResult.RowCountOperatorSpecified)
                    this.ddRowCountOperator.SelectedItem = this.tCase.ExpectedResult.RowCountOperator;
                else
                    this.ddRowCountOperator.SelectedItem = RowCountOperator.EqualTo;

                this.ddResultType.SelectedItem = this.tCase.ExpectedResult.ResultType;
       
                #region Add Output Controls
                List<OutputResultCtrl> outputCtrls = new List<OutputResultCtrl>();
                Point outputStart = new Point(3, 3);
                if (this.tCase.ExpectedResult.OutputResult != null)
                {
                    for (int i = 0; i < this.tCase.ExpectedResult.OutputResult.Length; i++)
                    {
                        OutputResultCtrl ctrl = new OutputResultCtrl(ref this.tCase.ExpectedResult.OutputResult[i]);
                        outputCtrls.Add(ctrl);
                    }
                }
                for (int i = 0; i < outputCtrls.Count; i++)
                {
                    outputCtrls[i].Location = outputStart;
                    outputStart.Y += outputCtrls[i].Height;
                    outputCtrls[i].DataChanged += new EventHandler(ChildData_DataChanged);
                    outputCtrls[i].RemoveOutputResult += new EventHandler(SprocTestConfigCtrl_RemoveOutputResult);
                    outputCtrls[i].Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
                    outputCtrls[i].Width = pnlOutput.Width - 10;
                    pnlOutput.Controls.Add(outputCtrls[i]);
                }
                #endregion
            }
            ddExecutionType_SelectionChangeCommitted(null, EventArgs.Empty);
            ddResultType_SelectionChangeCommitted(null, EventArgs.Empty);
            this.DataChanged = false;
        }

        void SprocTestConfigCtrl_RemoveOutputResult(object sender, EventArgs e)
        {
            if (sender is OutputResultCtrl)
            {
                this.pnlOutput.Controls.Remove(sender as OutputResultCtrl);
                this.DataChanged = true;
            }
        }
        public TestCase RefreshTestCaseData()
        {
            //Top Level
            tCase.ExecuteType = (ExecuteType) ddExecutionType.SelectedItem;
            tCase.ExecuteTypeSpecified = true;
            tCase.Name = txtCaseName.Text;

            //Basic expected results
            if (tCase.ExpectedResult == null)
                tCase.ExpectedResult = new ExpectedResult();
            tCase.ExpectedResult.RowCountOperator = (RowCountOperator)ddRowCountOperator.SelectedItem;
            tCase.ExpectedResult.RowCountOperatorSpecified = true;
            tCase.ExpectedResult.ResultType = (ResultType)ddResultType.SelectedItem;

            int rowCount;
            if (Int32.TryParse(txtRowCount.Text, out rowCount))
            {
                tCase.ExpectedResult.RowCount = rowCount;
                tCase.ExpectedResult.RowCountSpecified = true;
            }

            int colCount;
            if(int.TryParse(txtColumnCount.Text,out colCount))
            {
                tCase.ExpectedResult.ColumnCount = colCount;
                tCase.ExpectedResult.ColumnCountSpecified = true;
            }

            //Parameters.
            List<Parameter> param = new List<Parameter>();
            foreach (Control ctrl in pnlParameters.Controls)
                if (ctrl is ParameterCtrl)
                    param.Add(((ParameterCtrl)ctrl).Parameter);

            tCase.Parameter = null;
            tCase.Parameter = new Parameter[param.Count];
            for (int i = 0; i < param.Count; i++)
                tCase.Parameter[i] = param[i];

            //Output result.
            List<OutputResult> output = new List<OutputResult>();
            foreach (Control ctrl in pnlOutput.Controls)
                if (ctrl is OutputResultCtrl)
                    output.Add(((OutputResultCtrl)ctrl).OutputResult);

            tCase.ExpectedResult.OutputResult = null;
            tCase.ExpectedResult.OutputResult = new OutputResult[output.Count];
            for (int i = 0; i < output.Count; i++)
                tCase.ExpectedResult.OutputResult[i] = output[i];

            if (isNew)
            {
                tCase.CreatedBy = System.Environment.UserName;
                tCase.CreatedDate = DateTime.Now;
                tCase.CreatedDateSpecified = true;
            }
            else
            {
                tCase.ModifiedBy = System.Environment.UserName;
                tCase.ModifiedDate = DateTime.Now;
                tCase.ModifiedDateSpecified = true;
            }
            
            return tCase;

        }
        private void SprocTestConfigCtrl_RemoveParameter(object sender, EventArgs e)
        {
            if (sender is ParameterCtrl)
            {
                this.pnlParameters.Controls.Remove(sender as ParameterCtrl);
                this.DataChanged = true;
            }
        }
         private void ChildData_DataChanged(object sender, EventArgs e)
        {
            this.DataChanged = true;
        }


        private void SprocTestConfigCtrl_Load(object sender, EventArgs e)
        {
            this.ddExecutionType.DataSource = Enum.GetValues(typeof(SqlSync.SprocTest.Configuration.ExecuteType));
            this.ddExecutionType.SelectedItem = ExecuteType.ReturnData;

            this.ddRowCountOperator.DataSource = Enum.GetValues(typeof(SqlSync.SprocTest.Configuration.RowCountOperator));
            this.ddRowCountOperator.SelectedItem = RowCountOperator.EqualTo;

            this.ddResultType.DataSource = Enum.GetValues(typeof(SqlSync.SprocTest.Configuration.ResultType));
            this.ddResultType.SelectedItem = ResultType.Success;


            this.DataChanged = false;

        }

        private void lnkAddOutput_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Point start;
            if (pnlOutput.Controls.Count > 0)
            {
                start = pnlOutput.Controls[pnlOutput.Controls.Count - 1].Location;
                start.Y = start.Y + pnlOutput.Controls[pnlOutput.Controls.Count - 1].Height;
            }
            else
                start = new Point(3, 3);
            
            OutputResultCtrl ctrl = new OutputResultCtrl();
            ctrl.Location = start;
            ctrl.DataChanged += new EventHandler(ChildData_DataChanged);
            ctrl.RemoveOutputResult += new EventHandler(SprocTestConfigCtrl_RemoveOutputResult);
            ctrl.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            ctrl.Width = pnlOutput.Width - 10;
            pnlOutput.Controls.Add(ctrl);
        }

        private void btnSaveChanges_Click(object sender, EventArgs e)
        {
            if (txtCaseName.Text.Length == 0)
            {
                MessageBox.Show("Please enter a name for this test", "Test Name Required", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }
            if(TestCaseChanged != null)
            {
                TestCase tmp = RefreshTestCaseData();
                TestCaseChanged(this,new TestConfigChangedEventArgs(tmp,this.isNew));
                this.DataChanged = false;
                this.isNew = false;
            }
        }


        public event TestCaseChangedEventHandler TestCaseChanged;

        private void lnkPasteParameters_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            PasteScriptForm frmScript = new PasteScriptForm();
            if(DialogResult.OK == frmScript.ShowDialog())
            {
                Dictionary<string, string> paramvalues = TestManager.ParseParameterValuesFromScript(this.sprocName, RetrieveParameterNames(), frmScript.ScriptText);
                foreach (Control ctrl in pnlParameters.Controls)
                {
                    if (ctrl is ParameterCtrl)
                    {   
                        ParameterCtrl pC = (ParameterCtrl)ctrl;
                        if (paramvalues.ContainsKey(pC.Parameter.Name))
                        {
                            pC.Parameter.Value = paramvalues[pC.Parameter.Name];
                            pC.ParameterTextValue = paramvalues[pC.Parameter.Name];

                        }
                    }
                }
            }
        }

        private List<string> RetrieveParameterNames()
        {
            List<string> paramNames = new List<string>();
            foreach (Control ctrl in pnlParameters.Controls)
            {
                if (ctrl is ParameterCtrl)
                    paramNames.Add(((ParameterCtrl)ctrl).Parameter.Name);
            }
            return paramNames;
        }

        private void lnkTypeDefault_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            if (DialogResult.Yes == MessageBox.Show("Are you sure you want to insert default values?", "Confirm Insert", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                foreach (Control ctrl in pnlParameters.Controls)
                {
                    if (ctrl is ParameterCtrl)
                    {
                        ParameterCtrl pC = (ParameterCtrl)ctrl;
                        pC.ParameterTextValue = TestManager.GetDefaultValueForSqlDbType(pC.DbType);
                        pC.Parameter.Value = pC.ParameterTextValue;
                    }
                }
            }
        }


        private void lnkGetSqlScript_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            TestCase updatedTC = RefreshTestCaseData();
            string script = TestManager.GenerateTestSql(this.sprocName, this.databaseName,updatedTC);
            PasteScriptForm frmScript = new PasteScriptForm();
            frmScript.ScriptText = script;
            frmScript.ShowDialog();
        }

        private void ddResultType_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (((ResultType)ddResultType.SelectedItem) != ResultType.Success)
            {
                txtRowCount.Text = "";
                txtRowCount.Enabled = false;
                ddRowCountOperator.SelectedItem = RowCountOperator.EqualTo;
                ddRowCountOperator.Enabled = false;
                txtColumnCount.Text = "";
                txtColumnCount.Enabled = false;
            }
            else
            {
                txtRowCount.Enabled = true;
                ddRowCountOperator.Enabled = true;
                txtColumnCount.Enabled = true;
                ddExecutionType_SelectionChangeCommitted(sender, e);

            }

           
            ChildData_DataChanged(sender, e);
        }

        private void ddExecutionType_SelectionChangeCommitted(object sender, EventArgs e)
        {
            if (((ResultType)ddResultType.SelectedItem) != ResultType.Success)
                return;

            switch ((ExecuteType)ddExecutionType.SelectedItem)
            {
                case ExecuteType.NonQuery:
                    txtRowCount.Text = "-1";
                    txtRowCount.Enabled = false;
                    txtColumnCount.Text = "";
                    txtColumnCount.Enabled = false;
                    ddRowCountOperator.SelectedItem = RowCountOperator.EqualTo;
                    ddRowCountOperator.Enabled = false;
                    break;
                case ExecuteType.Scalar:
                    txtRowCount.Text = "1";
                    txtRowCount.Enabled = false;
                    txtColumnCount.Text = "1";
                    txtColumnCount.Enabled = false;
                    ddRowCountOperator.SelectedItem = RowCountOperator.EqualTo;
                    ddRowCountOperator.Enabled = false;
                    break;
                case ExecuteType.ReturnData:
                default:
                    txtRowCount.Enabled = true;
                    txtColumnCount.Enabled = true;
                    ddRowCountOperator.Enabled = true;
                    break;

            }
            ChildData_DataChanged(sender, e);
        }


        
    }
    public delegate void TestCaseChangedEventHandler(object sender, TestConfigChangedEventArgs e);
    public class TestConfigChangedEventArgs : EventArgs
    {
        public readonly TestCase TestCaseConfig;
        public readonly bool IsNew;
        public TestConfigChangedEventArgs(TestCase testCaseConfig, bool isNew)
        {
            this.TestCaseConfig = testCaseConfig;
            this.IsNew = isNew;
        }
    }
}

