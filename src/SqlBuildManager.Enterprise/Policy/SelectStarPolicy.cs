using Microsoft.Extensions.Logging;
using SqlBuildManager.ScriptHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using shP = SqlBuildManager.Interfaces.ScriptHandling.Policy;
namespace SqlBuildManager.Enterprise.Policy
{
    class SelectStarPolicy : shP.IScriptPolicyWithArguments
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #region IScriptPolicy Members
        public string PolicyId
        {
            get
            {
                return PolicyIdKey.SelectStarPolicy;
            }
        }
        private shP.ViolationSeverity severity = shP.ViolationSeverity.Medium;
        public shP.ViolationSeverity Severity
        {
            get { return severity; }
            set { severity = value; }
        }
        public string ShortDescription
        {
            get { return "SELECT *"; }
        }

        public string LongDescription
        {
            get { return "Checks that no queries in the script use \"SELECT *\" but rather explicitly lists columns"; }
        }

        public string ErrorMessage
        {
            get
            {
                return "A SELECT using \"*\" was found on line " + PolicyHelper.LineNumberToken + ". Please remove this wildcard and use explicit column names";
            }

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
                Regex selectStar = new Regex(@"(SELECT\s*\*)|(SELECT\s*.*\.\*)", RegexOptions.IgnoreCase | RegexOptions.Compiled); // Finds "SELECT *" and also "SELECT abc.*"
                List<string> regStrings = new List<string>();
                var tmpRegStrings = (from a in arguments select a.Value);
                if (tmpRegStrings.Count() > 0)
                    regStrings = tmpRegStrings.ToList();

                List<Regex> selectStarExceptions = new List<Regex>();
                foreach (string regStr in regStrings)
                    selectStarExceptions.Add(new Regex(regStr, RegexOptions.IgnoreCase));

                MatchCollection starMatches = selectStar.Matches(script);
                if (starMatches.Count == 0)
                    return true;

                foreach (Match star in starMatches)
                {
                    if (ScriptHandlingHelper.IsInComment(star.Index, commentBlockMatches))
                        continue;

                    bool foundException = false;
                    foreach (Regex regExcept in selectStarExceptions)
                    {
                        MatchCollection exceptionMatches = regExcept.Matches(script, star.Index);
                        if (exceptionMatches.Count == 0)
                        {
                            int lineNumber = PolicyHelper.GetLineNumber(script, star.Index);
                            message = ErrorMessage.Replace(PolicyHelper.LineNumberToken, lineNumber.ToString());
                        }
                        else if (exceptionMatches[0].Index == star.Index)
                        {
                            //this "star" match is an exception case, so it's ok.
                            foundException = true;
                            break;
                        }
                    }
                    if (!foundException)
                    {
                        int lineNumber = PolicyHelper.GetLineNumber(script, star.Index);
                        message = ErrorMessage.Replace(PolicyHelper.LineNumberToken, lineNumber.ToString());
                        return false;
                    }
                }

                message = string.Empty;
                return true;
            }
            catch (Exception exe)
            {
                message = "Error processing script policy. See application log file for details";
                log.LogError(exe, message);
                return false;
            }

        }

        #endregion

        #region IScriptPolicyWithArguments Members
        private List<shP.IScriptPolicyArgument> arguments = new List<shP.IScriptPolicyArgument>();
        public List<shP.IScriptPolicyArgument> Arguments
        {
            get
            {
                return arguments;
            }
            set
            {
                arguments = value;
            }
        }


        public bool CheckPolicy(string script, string targetDatabase, List<Match> commentBlockMatches, out string message)
        {
            return CheckPolicy(script, commentBlockMatches, out message);
        }

        #endregion
    }
}
