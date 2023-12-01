using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using shP = SqlBuildManager.Interfaces.ScriptHandling.Policy;
namespace SqlBuildManager.Enterprise.Policy
{
    class GrantExecuteToPublicPolicy : shP.IScriptPolicy
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #region IScriptPolicy Members
        public string PolicyId
        {
            get
            {
                return PolicyIdKey.GrantExecuteToPublicPolicy;
            }
        }
        public shP.ViolationSeverity Severity { get; set; }
        public string ShortDescription
        {
            get { return "Check for GRANT .. TO [public]"; }
        }

        public string LongDescription
        {
            get { return "Checks that scripts do not GRANT any privileges to the [public] group"; }
        }
        private bool enforce = true;
        public bool Enforce
        {
            get { return enforce; }
            set { enforce = value; }
        }
        public bool CheckPolicy(string script, List<Match> commentBlockMatches, out string message)
        {
            try
            {
                message = string.Empty;
                //With this regex, the SP name is always in the 4th group find...
                Regex regGrantPublic = new Regex(@"GRANT.+TO.+public", RegexOptions.IgnoreCase | RegexOptions.Compiled);

                if (regGrantPublic.Match(script).Success)
                {
                    message = "Script contains a GRANT statement to the [public] group";
                    return false;
                }
                else
                {
                    return true;
                }
            }
            catch (Exception exe)
            {
                message = "Error processing script policy. See application log file for details";
                log.LogError(exe, message); ;
                return false;
            }

        }
        #endregion

    }
}
