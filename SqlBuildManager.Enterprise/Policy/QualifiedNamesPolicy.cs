using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using p = SqlBuildManager.Interfaces.ScriptHandling.Policy;
using SqlBuildManager.ScriptHandling;
namespace SqlBuildManager.Enterprise.Policy
{
    class QualifiedNamesPolicy : p.IScriptPolicy  
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #region IScriptPolicy Members
        public string PolicyId
        {
            get
            {
                return PolicyIdKey.QualifiedNamesPolicy;
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
            get { return "Qualified Names (beta)"; }
        }

        public string LongDescription
        {
            get { return "Checks that object references are fully qualified (<schema>.<object name>) - in beta"; }
        }
        private bool enforce = true;
        public bool Enforce
        {
            get { return this.enforce; }
            set { this.enforce = value; }
        }
        public bool CheckPolicy(string script, List<Match> commentBlockMatches, out string message)
        {
            bool passed = true;
            try
            {
                List<string> badReferenceObjs = new List<string>();
                string rawScript = script;
                string subStr;
                int lengthToWhere;
                Match current;
                Match next;
                Regex regTokens = new Regex(@"(\bINNER JOIN\b)|(\bOUTER JOIN\b) |(\bFROM\b)|(\bWHERE\b)|(\bON\b)|(WITH *\(NOLOCK\))|(\bLEFT JOIN\b)|(\bRIGHT JOIN\b)|(\bINTO\b)|(\bJOIN\b)|(\bGROUP BY\b)|(\bDELETE FROM\b)|(\bUPDATE\b)|(\bset\b)|(\binserted\b)|(\bdeleted\b)|(\bAS\b)|(\bNO ACTION\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                Regex regSelects = new Regex(@"(\bINNER JOIN\b)|(\bOUTER JOIN\b) |(\bFROM\b)|(\bLEFT JOIN\b)|(\bRIGHT JOIN\b)|(\bJOIN\b)|(\bDELETE FROM\b)|(\bUPDATE\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                Regex regWheres = new Regex(@"(\bWHERE\b)|(\bON\b)|(\bINNER JOIN\b)|(\bOUTER JOIN\b)|(\bLEFT JOIN\b)|(\bRIGHT JOIN\b)|(\bJOIN\b)|(\bset\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                Regex regFrom = new Regex(@"(\bFROM\b)|(\bDELETE FROM\b)|(\bUPDATE\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                Regex regFromWithTableName = new Regex(@"(\bFROM\b\s*.*\s*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                Regex regNoLock = new Regex(@"(WITH *\(NOLOCK\))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                Regex regCursorInto = new Regex(@"(\bINTO\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                Regex regTriggerTables = new Regex(@"(\binserted\b)|(\bdeleted\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                Regex regTriggerUpdateAs = new Regex(@"(\bAS\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                Regex regForeignKeyAction = new Regex(@"(\bNO ACTION\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                MatchCollection coll = regTokens.Matches(rawScript);
                int textIndex = 0;
                for (int i = 0; i < coll.Count; i++)
                {
                    current = coll[i];



                    if (i == coll.Count - 1 && !regFrom.Match(current.Value).Success)
                        break;
                    else if (i == coll.Count - 1)// the last match is an unpaired FROM
                        next = current; //give it a fake value
                    else
                        next = coll[i + 1];

                    //Ignore trigger selectes "FROM inserted" or "FROM deleted" 
                    if (regFrom.Match(current.Value).Success && regTriggerTables.Match(next.Value).Success)
                        continue;

                    //Ignore trigger declaration "FOR UPDATE AS"
                    if (regFrom.Match(current.Value).Success && regTriggerUpdateAs.Match(next.Value).Success)
                        continue;

                    //Ignore foreign key action  "UPDATE  NO ACTION"
                    if (regFrom.Match(current.Value).Success && regForeignKeyAction.Match(next.Value).Success)
                        continue;

                    if (!ScriptHandlingHelper.IsInComment(current.Index, commentBlockMatches) && regSelects.Match(current.Value).Success && (regWheres.Match(next.Value).Success || regNoLock.Match(next.Value).Success)) // we have found our FROM .. WHERE or INNER JOIN ... ON limits
                    {
                        lengthToWhere = next.Index - textIndex;
                        int start = current.Index + current.Value.Length;
                        int length = next.Index - start;
                        string sub = rawScript.Substring(start, length);
                        if (sub.IndexOf('.') == -1 && !sub.Trim().StartsWith("@") && !sub.Trim().StartsWith("#"))
                        {
                            string regString = "(WITH *" + sub.Trim().Replace(")", "\\)").Replace("(", "\\(") + @"\s*\()";
                            try
                            {
                               
                                Regex regCTE = new Regex(regString); //Check for a Common Table Entity (CTE) declaration for this item. If it is declared as a CTE, don't fail the check, otherwise, fail it.

                                if (regCTE.Matches(script).Count == 0)
                                {
                                    passed = false;
                                    if (!badReferenceObjs.Contains(sub.Trim()))
                                    {
                                        int line = PolicyHelper.GetLineNumber(rawScript, start);
                                        badReferenceObjs.Add(sub.Trim() + " (line: " + line.ToString() + ")");
                                    }
                                }
                            }
                            catch (ArgumentException exe)
                            {
                                int line = PolicyHelper.GetLineNumber(rawScript, start);
                                log.Warn("Error validating QualifiedNamesPolicy. Issue on line "+ line.ToString() +". Problem with generated RegularExpression:  " + regString, exe);
                                message = "Error running Qualified Named Policy. This script will need to be manually checked. (See log file for details)";
                                    return false;
                            }
                        }

                    }
                    else if (!ScriptHandlingHelper.IsInComment(current.Index, commentBlockMatches) && regFrom.Match(current.Value).Success && !regNoLock.Match(next.Value).Success && !regCursorInto.Match(next.Value).Success) //handle the FROM that doesn't have a following WHERE or INNER or OUTER JOINS
                    {

                        if (regFromWithTableName.Match(rawScript, current.Index).Value.Length > 0)
                        {
                            Match subMatch = regFromWithTableName.Match(rawScript, current.Index);
                            if (subMatch != null & subMatch.Value.Length > 0)
                            {
                                subStr = subMatch.Value.Substring(4);
                                if (subStr.IndexOf('.') == -1 && !subStr.Trim().StartsWith("@") && !subStr.Trim().StartsWith("#") && !ScriptHandlingHelper.IsInComment(current.Index, commentBlockMatches))
                                {
                                    passed = false;
                                    if (!badReferenceObjs.Contains(subStr.Trim()))
                                    {
                                        int line = PolicyHelper.GetLineNumber(rawScript, subMatch.Index);
                                        badReferenceObjs.Add(subStr.Trim() + " (line: " + line.ToString() + ")");
                                    }
                                }
                            }
                        }
                    }
                }
                if (passed)
                {
                    message = string.Empty;
                }
                else
                {
                    message = "Missing schema qualifier on: " + String.Join(", ", badReferenceObjs.ToArray());
                }
            }
            catch (Exception exe)
            {
                message = "Error processing script policy. See application log file for details";
                log.Error(message, exe);
                passed = false;
            }

            return passed;
        }

        #endregion
    }
}
