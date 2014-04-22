using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using p = SqlBuildManager.Interfaces.ScriptHandling.Policy;
namespace SqlBuildManager.Enterprise.Policy
{
    class GrantExecutePolicy : p.IScriptPolicy    
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #region IScriptPolicy Members

        public string PolicyId
        {
            get
            {
                return PolicyIdKey.GrantExecutePolicy;
            }
        }
        public p.ViolationSeverity Severity
        {
            get
            {
                return p.ViolationSeverity.High;
            }
        }
        public string ShortDescription
        {
            get { return "Check for GRANT EXECUTE"; }
        }

        public string LongDescription
        {
            get { return "Checks that Stored Procedure and Function scripts have at least one \"GRANT EXECUTE\" statement"; }
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
                //With this regex, the SP name is always in the 4th group find...
                Regex regFindProc = new Regex(@"((\bCREATE\b\s*\bPROCEDURE\b)|(\bALTER\b\s*\bPROCEDURE\b)) ([A-Za-z0-9\[\]\._]{1,})", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                //With this regex, the SP name is always in the 4th group find...
                Regex regFindFunc = new Regex(@"((\bCREATE\b\s*\bFUNCTION\b)|(\bALTER\b\s*\bFUNCTION\b)) ([A-Za-z0-9\[\]\._]{1,})", RegexOptions.IgnoreCase | RegexOptions.Compiled);

                List<string> names = new List<string>();
                string tmpVal;
                if (regFindProc.Match(script).Success && regFindProc.Match(script).Groups.Count > 3)
                {
                    tmpVal = regFindProc.Match(script).Groups[4].Value;
                    if (!names.Contains(tmpVal))
                        names.Add(tmpVal);
                }

                if (regFindFunc.Match(script).Success && regFindFunc.Match(script).Groups.Count > 3)
                {
                    tmpVal = regFindFunc.Match(script).Groups[4].Value;
                    if (!names.Contains(tmpVal))
                        names.Add(tmpVal);
                }

                if (names.Count == 0) //No SP's or Functions found.. passed!
                {
                    message = "No routines found";
                    return true;
                }

                //Now that we have the list of procs or functions in the script, let's see if there is a GRANT EXECUTE ON ... statement for each one...
                for (int i = 0; i < names.Count; i++)
                {
                    //Want to split up the schema name and table and remove brackets to make sure we get the best match...
                    string routine, schema = string.Empty;
                    if (names[i].IndexOf('.') > -1)
                    {
                        schema = names[i].Split('.')[0].Replace("[", "").Replace("]", "").Trim();
                        routine = names[i].Split('.')[1].Replace("[", "").Replace("]", "").Trim();
                    }
                    else
                    {
                        routine = names[i].Replace("[", "").Replace("]", "").Trim();
                    }

                    Regex regGrant = new Regex("GRANT EXECU?T?E? ON .*" + schema + ".*" + routine, RegexOptions.IgnoreCase);
                    if (!regGrant.Match(script).Success)
                    {
                        message += "Missing execute on " + ((schema.Length > 0) ? schema + "." + routine : routine);
                    }
                }
                if (message.Length > 0)
                    return false;
                else
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
    }
}
