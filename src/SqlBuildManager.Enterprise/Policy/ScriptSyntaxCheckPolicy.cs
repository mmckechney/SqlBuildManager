using Microsoft.Extensions.Logging;
using SqlBuildManager.ScriptHandling;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using shP = SqlBuildManager.Interfaces.ScriptHandling.Policy;
namespace SqlBuildManager.Enterprise.Policy
{
    class ScriptSyntaxCheckPolicy : shP.IScriptPolicyMultiple
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #region IScriptPolicy Members
        public string PolicyId
        {
            get
            {
                return PolicyIdKey.ScriptSyntaxCheckPolicy;
            }
        }
        private shP.ViolationSeverity severity = shP.ViolationSeverity.High;
        public shP.ViolationSeverity Severity
        {
            get { return severity; }
            set { severity = value; }
        }
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
            get { return enforce; }
            set { enforce = value; }
        }
        public bool CheckPolicy(string script, List<Match> commentBlockMatches, out string message)
        {
            try
            {
                message = string.Empty;
                Dictionary<int, bool> rulesLine = new Dictionary<int, bool>();
                foreach (shP.IScriptPolicyArgument argument in arguments)
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
                    List<int> line = f.ToList();
                    //Return an error for the first line...
                    int lineNumber;
                    if (line.Count() == 0)
                        lineNumber = 1;
                    else
                        lineNumber = PolicyHelper.GetLineNumber(script, line[0]);
                    message = ErrorMessage.Replace(PolicyHelper.LineNumberToken, lineNumber.ToString());
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
