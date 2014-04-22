using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using SqlSync.ObjectScript.Hash;
namespace SqlSync.SqlBuild.MultiDb
{
    public partial class ObjectComparisonAnalysisForm : Form
    {
        ObjectScriptHashReportData rawReportData;
        public ObjectComparisonAnalysisForm(ObjectScriptHashReportData rawReportData)
        {
            InitializeComponent();
            this.rawReportData = rawReportData;
        }

        private void ObjectComparisonAnalysisForm_Load(object sender, EventArgs e)
        {
            foreach(ObjectScriptHashData data in rawReportData.DatabaseData)
            {
                ListViewItem item = new ListViewItem(data.ToString());
                item.Tag = data;
                item.Checked = false;
                lstDbs.Items.Add(item);
            }
        }

        private void lstDbs_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (e.Item.Checked)
            {
                foreach (ListViewItem item in lstDbs.Items)
                {
                    if (item != e.Item)
                        item.Checked = false;
                }
            }
        }

        private void btnReport_Click(object sender, EventArgs e)
        {
            if (lstDbs.CheckedItems.Count == 0)
            {
                MessageBox.Show("Please check a database to use as a baseline", "Selection Required", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }
            this.rawReportData.ResetComparisonValues();

            List<ObjectScriptHashData> hashes = new List<ObjectScriptHashData>();

            ObjectScriptHashData baseline = (ObjectScriptHashData)lstDbs.CheckedItems[0].Tag;
            foreach (ObjectScriptHashData data in rawReportData.DatabaseData)
            {
                if (!data.Equals(baseline))
                    hashes.Add(data);
            }

            HashCollector coll = new HashCollector();
            rawReportData = coll.ProcessHashDifferences(baseline, hashes);
            string tempFile = System.IO.Path.GetTempFileName();
            tempFile = System.IO.Path.GetFileNameWithoutExtension(tempFile) + ".html";
            coll.GenerateReport(tempFile, SqlSync.SqlBuild.Status.ReportType.Summary, this.rawReportData);
            this.rawReportData.DatabaseData.Add(baseline);

            System.Diagnostics.Process prc = new System.Diagnostics.Process();
            prc.StartInfo.FileName = tempFile;
            prc.Start();
        }

      
    }
}