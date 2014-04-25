using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using p = SqlBuildManager.Interfaces.ScriptHandling.Policy;
namespace SqlBuildManager.Enterprise.Policy
{
    public class CommentHeaderPolicy : p.IScriptPolicy
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #region IScriptPolicy Members

        public string PolicyId
        {
            get
            {
                return PolicyIdKey.CommentHeaderPolicy;
            }
        }
        public p.ViolationSeverity Severity { get; set; }
        private int dayThreshold = 10;

        public int DayThreshold
        {
            get { return dayThreshold; }
            set { dayThreshold = value; }
        }


        public string ShortDescription
        {
            get { return "Check for Comments"; }
        }

        public string LongDescription
        {
            get { return "Checks that Stored Procedure and Function have a comments header and recent comments"; }
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
                Regex regCommentHeader = new Regex(@"(/\*\*\*\*\*\*\*\*\*)|(\*\*\s*Desc)|(\*\*\s*Auth)|(\*\*\s*Change History)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
                Regex regDate = new Regex(@"\d{1,2}\/\d{1,2}\/\d{2,4}", RegexOptions.IgnoreCase);

                List<string> names = new List<string>();
                string tmpVal = string.Empty;

                //Find a proc...
                if (regFindProc.Match(script).Success && regFindProc.Match(script).Groups.Count > 3)
                {
                    tmpVal = regFindProc.Match(script).Groups[4].Value;
                    if (!names.Contains(tmpVal))
                        names.Add(tmpVal);
                }

                //or find a function...
                if (regFindFunc.Match(script).Success && regFindFunc.Match(script).Groups.Count > 3)
                {
                    tmpVal = regFindFunc.Match(script).Groups[4].Value;
                    if (!names.Contains(tmpVal))
                        names.Add(tmpVal);
                }

                //No SP's or Functions found.. passed!
                if (names.Count == 0)
                {
                    message = "No routines found";
                    return true;
                }

                //Check for the header format...
                MatchCollection header = regCommentHeader.Matches(script);
                if (header.Count < 4)
                {
                    message = "No standard comment header found";
                    return false;
                }

                int lookForDateStart = header[0].Index;
                MatchCollection collDates = regDate.Matches(script, lookForDateStart);
                if (collDates.Count == 0)
                {
                    message = "No create date or change dates found.";
                    return false;
                }

                DateTime mostRecentEntry = DateTime.MinValue;
                foreach (Match date in collDates)
                {
                    DateTime changeDate = DateTime.MinValue;
                    if (DateTime.TryParse(date.Value, out changeDate))
                    {
                        if (changeDate > mostRecentEntry)
                            mostRecentEntry = changeDate;

                        //found a date with the last day, we're good!
                        if (changeDate.Date >= DateTime.Now.AddDays(-1 * this.dayThreshold).Date)
                        {
                            message = "";
                            return true;
                        }
                    }
                }

                message = "No recent comment found (last entry @ " + mostRecentEntry.ToString("MM/dd/yyy") + "). Please add a dated comment in mm/dd/yyyy format.";
            }
            catch (Exception exe)
            {
                message = "Error processing script policy. See application log file for details";
                log.Error(message, exe);
                return false;
            }
            return false;
            
        }
        #endregion
    }
}
