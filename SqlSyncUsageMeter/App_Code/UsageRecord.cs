using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using System.Xml.Serialization;
namespace SqlSync.UsageMeter
{
    /// <summary>
    /// Summary description for UsageRecord
    /// </summary>
    public class UsageRecord
    {
        private string buildFileName = string.Empty;

        public string BuildFileName
        {
            get { return buildFileName; }
            set { buildFileName = value; }
        }
        private string user = string.Empty;

        public string User
        {
            get { return user; }
            set { user = value; }
        }
        private string hostMachineName = string.Empty;

        public string HostMachineName
        {
            get { return hostMachineName; }
            set { hostMachineName = value; }
        }
        private string sqlSyncVersion = string.Empty;

        public string SqlSyncVersion
        {
            get { return sqlSyncVersion; }
            set { sqlSyncVersion = value; }
        }

        private DateTime fileOpenUTC = DateTime.MinValue;


        public DateTime FileOpenUTC
        {
            get { return fileOpenUTC; }
            set { fileOpenUTC = value; }
        }
        

        public UsageRecord()
        {
            
        }
    }
}
