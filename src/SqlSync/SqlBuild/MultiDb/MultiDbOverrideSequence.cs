using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using SqlSync.Connection;
namespace SqlSync.SqlBuild.MultiDb
{
    public partial class MultiDbOverrideSequence : UserControl
    {
        private string databaseName = string.Empty;

        public string DatabaseName
        {
            get { return databaseName; }
            set { databaseName = value; }
        }
        private int? sequence = null;

        public int? Sequence
        {
            get
            {
                if (!string.IsNullOrWhiteSpace(this.txtSequence.Text))
                {
                    int.TryParse(this.txtSequence.Text, out int val);
                    return val;
                }
                else
                {
                    return null;
                }
            }
            set
            {
                if(value.HasValue)
                {
                    this.txtSequence.Text = value.ToString();
                    sequence = value;
                }
            }
        }
        private bool isManualEntry;

        public bool IsManualEntry
        {
            get { return isManualEntry; }
            set { isManualEntry = value; }
        }

        private List<QueryRowItem> queryRowData = new List<QueryRowItem>();

        public List<QueryRowItem> QueryRowData
        {
            get { return queryRowData; }
            set { queryRowData = value; }
        }

        private MultiDbOverrideSequence()
        {
            InitializeComponent();
            this.txtSequence.TextChanged += new EventHandler(txtSequence_TextChanged);

        }
        public MultiDbOverrideSequence(string databaseName, bool isManualEntry) : this()
        {
            this.isManualEntry = isManualEntry;
            this.databaseName = databaseName;
            this.lblDatabase.Text = this.databaseName;
            if (this.isManualEntry)
            {
                this.lblDatabase.ForeColor = Color.Red;
                toolTip1.SetToolTip(this.lblDatabase, "WARNING!\r\nThis database was found in the configuration file but NOT on the target server.\r\nThis override run setting will be ignored unless fixed.");

            }
        }
        public MultiDbOverrideSequence(string databaseName, int? sequenceId, List<QueryRowItem> queryRowData, bool isManualEntry)
            : this(databaseName, isManualEntry)
        {
            this.txtSequence.Text = (sequenceId.HasValue) ? sequenceId.Value.ToString() : string.Empty;
            this.queryRowData = queryRowData;
        }

        void mnuAutoSequence_Click(object sender, System.EventArgs e)
        {
            MultiDbAutoSequence frmAuto = new MultiDbAutoSequence(this.databaseName);
            if (DialogResult.OK == frmAuto.ShowDialog())
            {
                string pattern = frmAuto.Pattern;
                if (this.AutoSequencePattern != null)
                    this.AutoSequencePattern(this, new AutoSequencePatternEventArgs(pattern, frmAuto.Start, frmAuto.Increment));
            }
            frmAuto.Dispose();
        }
        void txtSequence_TextChanged(object sender, EventArgs e)
        {
            if (this.ValueChanged != null)
                this.ValueChanged(this, EventArgs.Empty);
        }
        public event EventHandler ValueChanged;
        public event AutoSequencePatternEventHandler AutoSequencePattern;
    }
    public delegate void AutoSequencePatternEventHandler(object sender, AutoSequencePatternEventArgs e);
    public class AutoSequencePatternEventArgs : EventArgs
    {
        public readonly string Pattern;
        public readonly int Start;
        public readonly int Increment;
        public AutoSequencePatternEventArgs(string pattern, int start, int increment)
        {
            this.Pattern = pattern;
            this.Start = start;
            this.Increment = increment;
        }
    }
}
