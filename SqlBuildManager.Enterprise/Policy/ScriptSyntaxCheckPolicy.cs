using System;
using System.Collections.Generic;
using System.Text;
using p = SqlBuildManager.Interfaces.ScriptHandling.Policy;
using System.Text.RegularExpressions;
using System.Linq;
using SqlBuildManager.ScriptHandling;
namespace SqlBuildManager.Enterprise.Policy
{
    class ScriptSyntaxCheckPolicy : p.IScriptPolicyMultiple
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #region IScriptPolicy Members
        public string PolicyId
        {
            get
            {
                return PolicyIdKey.ScriptSyntaxCheckPolicy;
            }
        }
        public p.ViolationSeverity Severity { get; set; }
        public string ShortDescription
        {
            get;
            set;
        }

        public string LongDescription
        {
            get;
            set;
        }

        public string ErrorMessage
        {
            get;
            set;
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
                Dictionary<int, bool> rulesLine = new Dictionary<int, bool>();
                foreach (p.IScriptPolicyArgument argument in this.arguments)
                {
                    Regex syntaxCheck = new Regex(argument.Value, RegexOptions.IgnoreCase);
                    MatchCollection syntaxMatches = syntaxCheck.Matches(script);
                    if (syntaxMatches.Count == 0)
                        continue;

                    //Check match and add each line to a collection. A true value means it passes with an exception.
                    foreach (Match syn in syntaxMatches)
                    {
                        if (!argument.IsGlobalException && ScriptHandlingHelper.IsInComment(syn.Index, commentBlockMatches)) //don't care about matches in comments unless it's a global exception.
                            continue;

                        if (argument.IsGlobalException) //found a global exception, so pass the test.
                            return true;

                        if (rulesLine.ContainsKey(syn.Index))
                        {
                            if (argument.IsLineException)
                                rulesLine[syn.Index] = true;
                        }
                        else
                        {
                            if (argument.IsLineException)
                                rulesLine.Add(syn.Index, true);
                            else
                                rulesLine.Add(syn.Index, false);
                        }
                    }

                }

                //Don't have any matches, so we must pass :-)
                if (rulesLine.Count == 0)
                    return true;

                //See if we have any that are set to false...
                var f = from r in rulesLine where r.Value == false select r.Key;

                if (f.Any())
                {
                    return true;
                }
                else
                {
                    List<int> line = f.ToList();
                    //Return an error for the first line...
                    int lineNumber = PolicyHelper.GetLineNumber(script, line[0]);
                    message = this.ErrorMessage.Replace(PolicyHelper.LineNumberToken, lineNumber.ToString());
                    return false;

                }
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
