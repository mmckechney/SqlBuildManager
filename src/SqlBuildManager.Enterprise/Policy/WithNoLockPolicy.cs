using Microsoft.Extensions.Logging;
using SqlBuildManager.ScriptHandling;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using shP = SqlBuildManager.Interfaces.ScriptHandling.Policy;
namespace SqlBuildManager.Enterprise.Policy
{
    public class WithNoLockPolicy : shP.IScriptPolicy
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #region IScriptPolicy Members
        public string PolicyId
        {
            get
            {
                return PolicyIdKey.WithNoLockPolicy;
            }
        }
        public shP.ViolationSeverity Severity { get; set; }
        public string ShortDescription
        {
            get { return "WITH (NOLOCK)"; }
        }

        public string LongDescription
        {
            get { return "Checks that select scripts include WITH (NOLOCK) directive or have a [NOLOCK Exception: <table name> <reason>] tag"; }
        }

        public const string FoundExceptionMessage =
            "The script is missing WITH (NOLOCK) directives, but does contain a [NOLOCK Exception: <table name> <reason>] tag.";

        public const string MissingNoLockMessage = "The script is missing one or more WITH (NOLOCK) directives or [NOLOCK Exception: <table name> <reason>] tag";

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
                List<List<string>> tablesMissingNoLock;
                string start = script;
                string end = ScriptOptimization.ProcessNoLockOptimization(script, commentBlockMatches, out tablesMissingNoLock);

                if (ScriptOptimization.regNoLock.Matches(start).Count == ScriptOptimization.regNoLock.Matches(end).Count)
                {
                    message = string.Empty;
                    return true;
                }
                else
                {
                    bool isOK = true;

                    foreach (List<string> t in tablesMissingNoLock)
                    {
                        bool hasException = HasNoLockExceptionTag(script, t);
                        if (!hasException)
                        {
                            isOK = false;
                        }
                    }

                    if (isOK)
                    {
                        message = FoundExceptionMessage;
                        return true;
                    }
                    else
                    {
                        message = MissingNoLockMessage;
                        return false;
                    }


                }
            }


            catch (Exception exe)
            {
                message = "Error processing script policy. See application log file for details"; ;
                log.LogError(exe, message);
                return false;
            }
        }
        private bool HasNoLockExceptionTag(string script, List<string> tablesMissingNoLock)
        {
            Regex regNoLockException = new Regex(@"\[NOLOCK Exception:.+\]", RegexOptions.IgnoreCase | RegexOptions.Compiled);

            if (regNoLockException.Match(script).Success)
            {
                string format = @"(\b{0}\b)|";
                string tables = string.Empty;

                //Catch oddball case here where there are no contents to the list... 
                if (tablesMissingNoLock.Count == 0)
                    return true;

                foreach (var tbl in tablesMissingNoLock)
                {
                    tables += tables + string.Format(format, tbl);
                }

                tables = tables.Substring(0, tables.Length - 1);
                Regex regTableNames = new Regex(tables);

                string match = regNoLockException.Match(script).Value;


                if (regTableNames.Match(match).Success)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        #endregion
    }
}
