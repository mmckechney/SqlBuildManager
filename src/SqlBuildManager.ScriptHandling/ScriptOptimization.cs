using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
namespace SqlBuildManager.ScriptHandling
{
    public class ScriptOptimization
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!);

        // PERF-006: All regex instances compiled once as static readonly fields.
        public static readonly Regex regNoLock = new Regex(@"(WITH \(NOLOCK\))|(WITH \(READPAST\))", RegexOptions.IgnoreCase | RegexOptions.Compiled);

        private static readonly Regex _regTokens = new Regex(@"(\bINNER JOIN\b)|(\bOUTER JOIN\b) |(\bFROM\b)|(\bWHERE\b)|(\bON\b)|(WITH \(NOLOCK\))|(WITH \(READPAST\))|(\bLEFT JOIN\b)|(\bRIGHT JOIN\b)|(\bINTO\b)|(\bJOIN\b)|(\bGROUP BY\b)|(\bDELETE FROM\b)|(\bUPDATE\b)|(\bset\b)|(\bINSERT INTO\b)|(\bVALUES\b)|(\))|(\binserted\b)|(\bdeleted\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _regSelects = new Regex(@"(\bINNER JOIN\b)|(\bOUTER JOIN\b) |(\bFROM\b)|(\bLEFT JOIN\b)|(\bRIGHT JOIN\b)|(\bJOIN\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _regWheres = new Regex(@"(\bWHERE\b)|(\bON\b)|(\bINNER JOIN\b)|(\bOUTER JOIN\b)|(\bLEFT JOIN\b)|(\bRIGHT JOIN\b)|(\bJOIN\b)|(\))", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _regJoin = new Regex(@"(\bINNER JOIN\b)|(\bOUTER JOIN\b)|(\bJOIN\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _regFrom = new Regex(@"(\bFROM\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _regFromWithTableName = new Regex(@"(\bFROM\b\s*.*\s*)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _regEndWhiteSpace = new Regex(@"(\s*$)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _regNewLine = new Regex(@"(\r)|(\n)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _regInto = new Regex(@"(\bINTO\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _regDelete = new Regex(@"(\bDELETE FROM\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _regUpdate = new Regex(@"(\bUPDATE\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _regSet = new Regex(@"(\bSET\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _regInsertInto = new Regex(@"(\bINSERT INTO\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _regValues = new Regex(@"(\bVALUES\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _regFindFunction = new Regex(@"(\b\S+\(.*\)\s*)", RegexOptions.Compiled);
        private static readonly Regex _regTriggerTables = new Regex(@"(\binserted\b)|(\bdeleted\b)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _regIgnoreObjects = new Regex(@"(FROM\s+sys\.\w+)|(FROM\s+dbo\.sys\w+)|(FROM INFORMATION_SCHEMA)|(FROM sysobjects)|(FROM sysindexes)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _regWords = new Regex(@"\b\w*\b", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        public static string ProcessNoLockOptimization(string rawScript)
        {
            return ProcessNoLockOptimization(rawScript, null!);
        }

        public static string ProcessNoLockOptimization(string rawScript, List<Match> commentBlockMatches)
        {
            List<List<string>> tablesMissingNoLock;
            return ProcessNoLockOptimization(rawScript, commentBlockMatches, out tablesMissingNoLock);
        }

        /// <summary>
        /// Add WITH (NOLOCK) directive to all FROM and JOIN selects in a SQL query
        /// </summary>
        /// <param name="rawScript">Raw incomming script</param>
        /// <param name="commentBlockMatches"></param>
        /// <param name="tablesMissingNoLock"></param>
        /// <returns>Script updated with the WITH (NOLOCK) directive</returns>
        public static string ProcessNoLockOptimization(string rawScript, List<Match> commentBlockMatches, out List<List<string>> tablesMissingNoLock)
        {
            tablesMissingNoLock = new List<List<string>>();
            try
            {
                if (commentBlockMatches == null)
                    commentBlockMatches = ScriptHandlingHelper.GetScriptCommentBlocks(rawScript);

                StringBuilder sb = new StringBuilder();
                string noLock = " WITH (NOLOCK) ";
                string subStr;
                int lengthToWhere;
                Match current;
                Match next;
                Match previous = null!;
                // PERF-006: Use static cached regex instances defined above.
                MatchCollection coll = _regTokens.Matches(rawScript);
                int textIndex = 0;
                for (int i = 0; i < coll.Count; i++)
                {
                    current = coll[i];

                    if (i == coll.Count - 1 && !_regFrom.Match(current.Value).Success)
                        break;
                    else if (i == coll.Count - 1)// the last match is an unpaired FROM
                        next = current; //give it a fake value
                    else
                        next = coll[i + 1];

                    //Ignore DELETE FROM .. WHERE.
                    if (_regDelete.Match(current.Value).Success && _regWheres.Match(next.Value).Success)
                        continue;

                    //Ignore UPDATE .. SET.
                    if (_regUpdate.Match(current.Value).Success && _regSet.Match(next.Value).Success)
                        continue;

                    //Ignore INSERT INTO .. VALUES.
                    if (_regInsertInto.Match(current.Value).Success && _regValues.Match(next.Value).Success)
                        continue;
                    //Ignore select on Trigger tables "FROM inserted" and "FROM deleted"
                    if (_regFrom.Match(current.Value).Success && _regTriggerTables.Match(next.Value).Success)
                        continue;



                    if (_regSelects.Match(current.Value).Success && _regWheres.Match(next.Value).Success && !regNoLock.Match(next.Value).Success) // we have found our FROM .. WHERE or INNER JOIN ... ON limits
                    {
                        lengthToWhere = next.Index - textIndex;


                        //Have we found a FROM value that selects from one of the ignore objects??
                        //Are we selecting from a table variable or temp table??
                        //Are we selecting from a function?
                        //Is the match in a comment?
                        Match ignore = _regIgnoreObjects.Match(rawScript, current.Index);
                        string strCheck = rawScript.Substring(current.Index + current.Length).Trim();
                        string strFuncCheck = rawScript.Substring(current.Index, next.Index - current.Index);
                        if (next.Value == ")") strFuncCheck += ")";
                        if ((ignore.Success && ignore.Index == current.Index) || strCheck.StartsWith("@") ||
                            strCheck.StartsWith("#") || _regFindFunction.Match(strFuncCheck).Success || ScriptHandlingHelper.IsInComment(current.Index, commentBlockMatches))
                        {
                            if (ignore.Success && ignore.Index == current.Index)
                                lengthToWhere = next.Index - ignore.Index + 1;
                            else
                                lengthToWhere = next.Index - textIndex;

                            if (textIndex + lengthToWhere > rawScript.Length || lengthToWhere < 0)
                                continue;

                            subStr = rawScript.Substring(textIndex, lengthToWhere);
                            sb.Append(subStr);
                            textIndex += lengthToWhere;
                            continue;
                        }

                        //since astetically, we want the NOLOCK to be on the same line as the FROM table name, need to check for a line break...
                        if (lengthToWhere < 0)
                            continue;

                        subStr = rawScript.Substring(textIndex, lengthToWhere);
                        if (_regEndWhiteSpace.Match(subStr).Success &&
                            _regNewLine.Match(_regEndWhiteSpace.Match(subStr).Value).Success)
                        {
                            tablesMissingNoLock.Add(GetTableName(subStr));
                            string whiteSpace = _regEndWhiteSpace.Match(subStr).Value;
                            sb.Append(subStr.TrimEnd() + noLock + whiteSpace + next.Value);
                        }
                        else
                        {
                            tablesMissingNoLock.Add(GetTableName(subStr));
                            sb.Append(subStr.TrimEnd() + noLock + next.Value);
                        }
                        if (!_regJoin.Match(next.Value).Success) //if the next token is a JOIN, don't skip it.
                            i++;

                        textIndex = next.Index + next.Length;
                    }
                    else if (_regFrom.Match(current.Value).Success && _regInto.Match(next.Value).Success) // looks like a cursor... FROM xyx INTO
                    {
                        int len = next.Index - textIndex;
                        if (len < 0 || textIndex + len > rawScript.Length)
                            continue;

                        subStr = rawScript.Substring(textIndex, len);
                        sb.Append(subStr + next.Value);
                        textIndex = next.Index + next.Length;
                    }
                    else if (_regFrom.Match(current.Value).Success && !regNoLock.Match(next.Value).Success) //handle the FROM that doesn't have a following WHERE or INNER or OUTER JOINS
                    {
                        int len;
                        if (current.Index > textIndex)
                        {
                            len = current.Index - textIndex + 4;

                            if (len < 0 || textIndex + len > rawScript.Length)
                                continue;

                            sb.Append(rawScript.Substring(textIndex, len));
                        }

                        subStr = _regFromWithTableName.Match(rawScript, current.Index).Value.Substring(4);

                        len = next.Index - current.Index;
                        if (len < 0 || current.Index + len > rawScript.Length)
                            continue;

                        string strFuncCheck = rawScript.Substring(current.Index, len);
                        if (next.Value == ")") strFuncCheck += ")";

                        //selecting against a table variable or temp table?
                        //selecting against a function?
                        //in a comment?
                        if (subStr.Trim().StartsWith("@") || subStr.Trim().StartsWith("#")
                            || _regFindFunction.Match(strFuncCheck).Success || ScriptHandlingHelper.IsInComment(current.Index, commentBlockMatches))
                        {
                            sb.Append(subStr);
                        }
                        else if (previous != null &&
                                (previous.Value.ToLower().Trim().StartsWith("delete") ||
                                previous.Value.ToLower().Trim().StartsWith("insert") ||
                                previous.Value.ToLower().Trim().StartsWith("update"))) //Covers where there is a "delete, insert or update" that is followed by a FROM but no WHERE
                        {
                            sb.Append(subStr);
                        }
                        else
                        {
                            string append;
                            if (_regEndWhiteSpace.Match(subStr).Success && _regNewLine.Match(_regEndWhiteSpace.Match(subStr).Value).Success)
                            {
                                tablesMissingNoLock.Add(GetTableName(subStr));
                                append = subStr.TrimEnd() + noLock + "\r\n";
                                sb.Append(append);
                            }
                            else
                            {
                                tablesMissingNoLock.Add(GetTableName(subStr));
                                append = subStr.TrimEnd() + noLock;
                                sb.Append(append);
                            }
                        }

                        if (current.Index + subStr.Length + 4 == rawScript.Length) //catch if we're at the end
                            textIndex = rawScript.Length;
                        else
                        {
                            textIndex = current.Index + subStr.Length + 4;
                        }
                    }
                    else //got nothing.. just add the text.
                    {
                        int len = next.Index + next.Length - textIndex;
                        if (len < 0 || textIndex + len > rawScript.Length)
                            continue;

                        sb.Append(rawScript.Substring(textIndex, len));
                        textIndex = next.Index + next.Length;
                    }

                    previous = current;
                }
                sb.Append(rawScript.Substring(textIndex, rawScript.Length - textIndex));
                return sb.ToString();
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Error processing Script Optimization");
                throw;
            }
        }

        /// <summary>
        /// The table name will always be at the end of this substring...
        /// But grab the last 2 because the very last one might be the table alias.
        /// </summary>
        /// <param name="subString"></param>
        /// <returns></returns>
        private static List<string> GetTableName(string subString)
        {
            string tmpTrimmed = subString.Trim();
            // PERF-006: Use static cached _regWords instead of creating a new instance per call.
            MatchCollection coll = _regWords.Matches(tmpTrimmed);

            //remove any blank entries...
            var t = from c in coll.Cast<Match>()
                    where c.Value.Trim().Length > 0
                    select c.Value
            ;
            ;
            List<string> names = new List<string>();
            if (t.Count() > 0)
            {
                names.Add(t.Last());

                if (t.Count() > 1)
                    names.Add(t.ElementAt(t.Count() - 2))
                ;
            }
            return names;
        }


    }
}
