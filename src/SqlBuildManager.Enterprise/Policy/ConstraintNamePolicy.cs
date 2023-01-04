using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using p = SqlBuildManager.Interfaces.ScriptHandling.Policy;
namespace SqlBuildManager.Enterprise.Policy
{
    class ConstraintNamePolicy : p.IScriptPolicy
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #region IScriptPolicy Members

        public string PolicyId
        {
            get
            {
                return PolicyIdKey.ConstraintNamePolicy;
            }
        }
        public p.ViolationSeverity Severity { get; set; }
        public string ShortDescription
        {
            get { return "Constraint Naming (beta)"; }
        }

        public string LongDescription
        {
            get { return "Checks that constraints contain the name of the table they are applied to."; }
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
                Regex regCheckForConstraint = new Regex(@"(\bALTER\s*TABLE\b.*\bCONSTRAINT\b)|(\bCREATE\s*TABLE\b.*\bCONSTRAINT\b)|(\bALTER\s*TABLE\b.*\bDEFAULT\b)|(\bCREATE\s*TABLE\b.*\bDEFAULT\b)", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
                //Regex regLocateTableName = new Regex(@"(\bALTER\s*TABLE\b)|(\bCREATE\s*TABLE\b)|(\bADD\b)|(\()|(\bCHECK\b)|(\bCONSTRAINT\b)|(\bWITH\b)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                Regex regLocateTableChange = new Regex(@"(\bALTER\s*TABLE\b)|(\bCREATE\s*TABLE\b)", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
                Regex regLocateTableChangeEnd = new Regex(@"(\b\()|(\bWITH\b)|(\bADD\b)|(\bCHECK\b)|(\bCONSTRAINT\b)|(\()", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
                Regex regNoDefaultName = new Regex(@"(\bADD\b\s*\bDEFAULT\b)", RegexOptions.Singleline | RegexOptions.IgnoreCase);
                Regex regDefaultConstraintName = new Regex(@"(\bCONSTRAINT\b(.*)\bDEFAULT\b)", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
                Regex regPrimaryKeyName = new Regex(@"(\bCONSTRAINT\b(.*)\bPRIMARY KEY\b)", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
                Regex regForeignKeyName = new Regex(@"(\bCONSTRAINT\b(.*)\bFOREIGN KEY\b)", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
                Regex regCheckConstraint = new Regex(@"(\bCONSTRAINT\b(.*)\bCHECK\b)", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);
                Regex regEnableConstraint = new Regex(@"(\bCHECK\b\s*\bCONSTRAINT\b)", RegexOptions.Singleline | RegexOptions.IgnoreCase | RegexOptions.Compiled);

                if (!regCheckForConstraint.Match(script).Success)
                {
                    message = "No constraints found";
                    return true;
                }

                MatchCollection coll = regLocateTableChange.Matches(script);
                for (int i = 0; i < coll.Count; i++)
                {
                    Match tableStart = coll[i];
                    //Find the table name...
                    string tableName = string.Empty;
                    int start = tableStart.Index + tableStart.Length;
                    int length = 0;
                    Match tableEnd = regLocateTableChangeEnd.Match(script, start);
                    if (tableEnd.Success)
                    {
                        length = tableEnd.Index - start;
                        if (start + length > script.Length)
                            continue;

                        tableName = script.Substring(start, length);
                    }
                    tableName = tableName.Replace("[", "").Replace("]", "").Trim();
                    if (tableName.IndexOf(".") > 0)
                        tableName = tableName.Split('.')[1];

                    tableName = tableName.Trim();

                    //Get the sub-script that is after the ALTER TABLE...
                    string subScript;
                    if (i + 1 < coll.Count)
                    {
                        int len = coll[i + 1].Index - tableStart.Index;
                        subScript = script.Substring(tableStart.Index, len);
                    }
                    else
                        subScript = script.Substring(tableStart.Index);

                    //Check for unnamed default
                    if (regNoDefaultName.Match(subScript).Success)
                    {
                        message = "No constraint name specified. Default constraint names not allowed.";
                        return false;
                    }

                    //Check for named default
                    if (regDefaultConstraintName.Match(subScript).Success)
                    {
                        string constraint = regDefaultConstraintName.Match(subScript).Value;
                        if (constraint.IndexOf(tableName, 0, StringComparison.CurrentCultureIgnoreCase) == -1)
                        {
                            message = "The default constraint name '" + regDefaultConstraintName.Match(subScript).Groups[2].Value.Trim() + "' does not contain the referenced table name '" + tableName + "'.";
                            return false;
                        }
                    }

                    //Check for named primary key
                    if (regPrimaryKeyName.Match(subScript).Success)
                    {
                        string constraint = regPrimaryKeyName.Match(subScript).Value;
                        if (constraint.IndexOf(tableName, 0, StringComparison.CurrentCultureIgnoreCase) == -1)
                        {
                            message = "The primary key name '" + regPrimaryKeyName.Match(subScript).Groups[2].Value.Trim() + "' does not contain the referenced table name '" + tableName + "'.";
                            return false;
                        }
                    }

                    //Check for named foreign key
                    if (regForeignKeyName.Match(subScript).Success)
                    {
                        string constraint = regForeignKeyName.Match(subScript).Value;
                        if (constraint.IndexOf(tableName, 0, StringComparison.CurrentCultureIgnoreCase) == -1)
                        {
                            message = "The foreign key name '" + regForeignKeyName.Match(subScript).Groups[2].Value.Trim() + "' does not contain the referenced table name '" + tableName + "'.";
                            return false;
                        }
                    }

                    //Check for named 'check' constraint
                    if (regCheckConstraint.Match(subScript).Success)
                    {
                        string constraint = regCheckConstraint.Match(subScript).Value;
                        if (constraint.IndexOf(tableName, 0, StringComparison.CurrentCultureIgnoreCase) == -1)
                        {
                            message = "The check constraint name '" + regCheckConstraint.Match(subScript).Groups[2].Value.Trim() + "' does not contain the referenced table name '" + tableName + "'.";
                            return false;
                        }
                    }

                    //Check for then enabling of a constraint...
                    if (regEnableConstraint.Match(subScript).Success)
                    {
                        Match constraintMatch = regEnableConstraint.Match(subScript);
                        string constraint = constraintMatch.Value;

                        if (subScript.Substring(constraintMatch.Index + constraintMatch.Length).IndexOf(tableName, 0, StringComparison.CurrentCultureIgnoreCase) == -1)
                        //if (constraint.IndexOf(tableName, 0, StringComparison.CurrentCultureIgnoreCase) == -1)
                        {
                            message = "An existing constraint enabled by your CHECK CONSTRAINT script does not contain the referenced table name '" + tableName + "'.";
                            return false;
                        }
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
    }
}
