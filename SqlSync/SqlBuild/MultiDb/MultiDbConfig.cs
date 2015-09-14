using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Forms;
using SqlSync.DbInformation;
using SqlSync.Connection;
namespace SqlSync.SqlBuild.MultiDb
{
    public partial class MultiDbConfig : UserControl
    {
        private DbOverrideSequence sequence = null;
        private DatabaseItem defaultDatabase;

        /// <summary>
        /// The database as set in the configuration file
        /// </summary>
        public DatabaseItem DefaultDatabase
        {
            get { return defaultDatabase; }
            set { defaultDatabase = value; }
        }

        private DatabaseList databaseList = new DatabaseList();

        public DatabaseList DatabaseList
        {
            get { return databaseList; }
            set { databaseList = value; }
        }
        private Dictionary<string, DatabaseOverride> databaseOverrideSequence;

        public Dictionary<string, DatabaseOverride> DatabaseOverrideSequence
        {
            get
            {

                databaseOverrideSequence = new Dictionary<string, DatabaseOverride>();
                for (int i = 0; i < this.flowDbContainer.Controls.Count; i++)
                {
                    if (this.flowDbContainer.Controls[i] is MultiDbOverrideSequence)
                    {
                        MultiDbOverrideSequence tmp = (MultiDbOverrideSequence)this.flowDbContainer.Controls[i];
                        if (tmp.Sequence.Length > 0)
                            this.databaseOverrideSequence.Add(tmp.Sequence, new DatabaseOverride() { DefaultDbTarget = this.defaultDatabase.DatabaseName, OverrideDbTarget = tmp.DatabaseName, QueryRowData = tmp.QueryRowData });
                    }
                }
                return this.databaseOverrideSequence;
            }
       
        }


        public MultiDbConfig(DatabaseItem defaultDatabase, DatabaseList databases)
            : this()
        {
            this.databaseList = databases;
            this.defaultDatabase = defaultDatabase;
        }
        public MultiDbConfig(DatabaseItem defaultDatabase, DatabaseList databases, DbOverrideSequence sequence)
            : this(defaultDatabase, databases)
        {
            this.sequence = sequence;
        }
        public MultiDbConfig()
        {
            InitializeComponent();
        }
        public void DataBind()
        {
            //Set the text of the default database name
            this.lblConfigDb.Text = this.defaultDatabase.DatabaseName;
            toolTip1.SetToolTip(this.lblConfigDb, this.defaultDatabase.DatabaseName);
            if (this.defaultDatabase.IsManuallyEntered)
            {
                this.lblConfigDb.ForeColor = Color.Red;
                toolTip1.SetToolTip(this.lblConfigDb, this.defaultDatabase.DatabaseName +"\r\nWARNING!\r\nThis database was found in the configuration file but NOT in the current Build Manager File.\r\nThis override run setting will be ignored unless fixed.");

            }


            //XCombine the list of databases from the server list with the list of Db's from the configuration
            List<string> configOverrides = null; ;
            if(sequence != null)
                configOverrides = sequence.GetOverrideDatabaseNameList();
            DatabaseList lstCombined = new DatabaseList();
            lstCombined.AddManualList(configOverrides); //adds Db's as "Manually Entered"
            lstCombined.AddRangeUnique(this.databaseList); //adds the range but ensures no duplicates
            lstCombined.Sort(new DatabaseListComparer());

            for (int i = 0; i < lstCombined.Count; i++)
            {
                MultiDbOverrideSequence tmp;
                if(sequence != null)
                {
                    tmp = new MultiDbOverrideSequence(lstCombined[i].DatabaseName, 
                        sequence.GetSequenceId(this.defaultDatabase.DatabaseName,lstCombined[i].DatabaseName), 
                        sequence.GetQueryRowData(this.defaultDatabase.DatabaseName,lstCombined[i].DatabaseName), lstCombined[i].IsManuallyEntered);
                }
                else
                {
                    tmp = new MultiDbOverrideSequence(lstCombined[i].DatabaseName,lstCombined[i].IsManuallyEntered);
                }
                this.flowDbContainer.Controls.Add(tmp);
                tmp.ValueChanged += new EventHandler(tmp_ValueChanged);
                tmp.AutoSequencePattern += new AutoSequencePatternEventHandler(tmp_AutoSequencePattern);
            }
        }

        void tmp_AutoSequencePattern(object sender, AutoSequencePatternEventArgs e)
        {
            string pattern = e.Pattern;
            double start = e.Start;
            double increment = e.Increment;

            Regex dbMatch = new Regex(pattern, RegexOptions.IgnoreCase);
            foreach (Control ctrl in this.flowDbContainer.Controls)
            {
                if (ctrl is MultiDbOverrideSequence)
                {
                    MultiDbOverrideSequence tmp = (MultiDbOverrideSequence)ctrl;
                    if (dbMatch.Match(tmp.DatabaseName).Success)
                    {
                        tmp.Sequence = start.ToString();
                        start += increment;
                    }
                }
            }


        }

        void tmp_ValueChanged(object sender, EventArgs e)
        {
            if (this.ValueChanged != null)
                this.ValueChanged(this, EventArgs.Empty);
        }

        public event EventHandler ValueChanged;
    }
}
