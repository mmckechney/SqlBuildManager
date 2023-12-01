using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using shP = SqlBuildManager.Interfaces.ScriptHandling.Policy;
namespace SqlBuildManager.Enterprise.Policy
{
    class ViewAlterPolicy : shP.IScriptPolicy
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #region IScriptPolicy Members

        public string PolicyId
        {
            get { return PolicyIdKey.ViewAlterPolicy; }
        }
        public shP.ViolationSeverity Severity { get; set; }
        public string ShortDescription
        {
            get { return "Alter View Reminder"; }
        }

        public string LongDescription
        {
            get { return "Creates a reminder to check for indexes dropped by SQL Server in the ALTER process."; }
        }
        private bool enforce = true;
        public bool Enforce
        {
            get { return enforce; }
            set { enforce = value; }
        }
        public bool CheckPolicy(string script, List<Match> commentBlockMatches, out string message)
        {
            message = string.Empty;
            //With this regex, the SP name is always in the 4th group find...
            Regex regFindAlterView = new Regex(@"\bALTER\b\s*\bVIEW\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            Regex regFindNoIndexTag = new Regex(@"\[No Indexes\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            if (regFindAlterView.Match(script).Success && !regFindNoIndexTag.Match(script).Success)
            {
                message = "An \"ALTER VIEW\" was found. Please make sure that no indexes were dropped by SQL Server in the process." +
                    "\r\nIf you have validated that no indexes were dropped, you can add a [No Indexes] tag to suppress this message.";
                return false;
            }

            return true;
        }

        #endregion
    }
}
