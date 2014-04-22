using System;
using System.Collections.Generic;
using System.Text;
using p = SqlBuildManager.Interfaces.ScriptHandling.Policy;
using System.Text.RegularExpressions;
using System.Linq;
using SqlBuildManager.ScriptHandling;
namespace SqlBuildManager.Enterprise.Policy
{
    class SelectStarPolicy : p.IScriptPolicyWithArguments
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #region IScriptPolicy Members
        public string PolicyId
        {
            get
            {
                return PolicyIdKey.SelectStarPolicy;
            }
        }
        public p.ViolationSeverity Severity
        {
            get
            {
                return p.ViolationSeverity.Medium;
            }
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
            get { return this.enforce; }
            set { this.enforce = value; }
        }
        public bool CheckPolicy(string script, List<Match> commentBlockMatches, out string message)
        {
            try
            {
                message = string.Empty;
                Regex selectStar = new Regex(@"(SELECT\s*\*)|(SELECT\s*.*\.\*)", RegexOptions.IgnoreCase | RegexOptions.Compiled); // Finds "SELECT *" and also "SELECT abc.*"
                List<string> regStrings = new List<string>();
                var tmpRegStrings = (from a in this.arguments select a.Value);
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
                            message = this.ErrorMessage.Replace(PolicyHelper.LineNumberToken, lineNumber.ToString());
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
                        message = this.ErrorMessage.Replace(PolicyHelper.LineNumberToken, lineNumber.ToString());
                        return false;
                    }
                }

                message = string.Empty;
                return true;
            }
            catch (Exception exe)
            {
                message = "Error processing script policy. See application log file for details";
                log.Error(message, exe);
                return false;
            }

        }

        #endregion

        #region IScriptPolicyWithArguments Members
        private List<p.IScriptPolicyArgument> arguments = new List<p.IScriptPolicyArgument>();
        public List<p.IScriptPolicyArgument> Arguments
        {
            get
            {
                return this.arguments;
            }
            set
            {
                this.arguments = value;
            }
        }


        public bool CheckPolicy(string script, string targetDatabase, List<Match> commentBlockMatches, out string message)
        {
            return CheckPolicy(script, commentBlockMatches, out message);
        }

        #endregion
    }
}
