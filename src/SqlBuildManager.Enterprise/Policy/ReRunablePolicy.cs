using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using shP = SqlBuildManager.Interfaces.ScriptHandling.Policy;
namespace SqlBuildManager.Enterprise.Policy
{
    class ReRunablePolicy : shP.IScriptPolicy
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!);

        // PERF-006: Static cached regexes compiled once per process.
        private static readonly Regex _regExists = new Regex(@"(IF\s+EXISTS)|(IF\s+NOT\s+EXISTS)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _regSystemObject = new Regex(@"(IF.*is null)", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        #region IScriptPolicy Members
        public string PolicyId
        {
            get
            {
                return PolicyIdKey.ReRunablePolicy;
            }
        }
        public shP.ViolationSeverity Severity { get; set; }
        public string ShortDescription
        {
            get { return "Re-runable scripts"; }
        }

        public string LongDescription
        {
            get { return "Checks that scripts contain \"IF EXISTS\" or \"IF NOT EXISTS\" checks so they are potentially re-runable"; }
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
                if (_regExists.Match(script).Success || _regSystemObject.Match(script).Success)
                {
                    message = "";
                    return true;
                }
                message = "Script contains no \"IF EXISTS\" or \"IF NOT EXISTS\" checks";
                return false;
            }
            catch (Exception exe)
            {
                message = "Error processing script policy. See application log file for details";
                log.LogError(exe, message);
                return false;
            }

        }

        #endregion
    }
}
