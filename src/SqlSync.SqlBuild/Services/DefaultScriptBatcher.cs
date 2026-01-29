using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SqlSync.Constants;

namespace SqlSync.SqlBuild.Services
{
    public sealed class DefaultScriptBatcher : IScriptBatcher
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public List<string> ReadBatchFromScriptText(string scriptContents, bool stripTransaction, bool maintainBatchDelimiter)
        {
            List<string> list = new List<string>();

            //Convert \n to \r\n
            scriptContents = scriptContents.ConvertNewLinetoCarriageReturnNewLine();

            //Find the "GO" delimiters that are not commented out or embedded in scripts
            List<KeyValuePair<Match, int>> activeDelimiters = FindActiveBatchDelimiters(scriptContents);
            if (activeDelimiters.Count == 0)
            {
                //A trim hack for backward compatability...
                scriptContents = scriptContents.ClearTrailingCarriageReturn();
                list.Add(scriptContents);
                return list;
            }

            //Needed when we are going to be removing the batch delimiter text.
            Regex regBackwardEndOfLine = null;
            int previousEndOfLine = 0;
            if (!maintainBatchDelimiter)
                regBackwardEndOfLine = new Regex(Properties.Resources.RegexEndOfLine, RegexOptions.IgnoreCase | RegexOptions.RightToLeft);

            int startIndex = 0;
            int modStartIndex = 0;
            string scriptSubstring = string.Empty;
            foreach (KeyValuePair<Match, int> m in activeDelimiters)
            {
                if (maintainBatchDelimiter)
                {
                    //Want to include any whitespace after the delimiter up to the end of line...
                    if (m.Value <= 0)
                    {
                        scriptSubstring = scriptContents.Substring(startIndex, m.Key.Index + m.Key.Length - startIndex);
                        list.Add(scriptSubstring);
                        startIndex = m.Key.Index + m.Key.Length;
                    }
                    else
                    {
                        list.Add(scriptContents.Substring(startIndex, m.Value + 2 - startIndex));
                        startIndex = m.Value + 2;
                    }
                }
                else
                {
                    previousEndOfLine = regBackwardEndOfLine.Match(scriptContents, m.Key.Index).Index;
                    if (previousEndOfLine > 0)
                    {
                        if (startIndex >= 2 && scriptContents.Substring(startIndex - 2, 2) == "\r\n")
                            startIndex = startIndex - 2;

                        modStartIndex = (startIndex == 0) ? startIndex : startIndex + Environment.NewLine.Length;
                        scriptSubstring = scriptContents.Substring(modStartIndex, m.Key.Index - modStartIndex);
                        scriptSubstring = scriptSubstring.ClearTrailingSpacesAndTabs();
                        list.Add(scriptSubstring);
                        startIndex = m.Key.Index + m.Key.Length;
                    }
                    else
                    {
                        scriptSubstring = scriptContents.Substring(startIndex, m.Key.Index - startIndex);
                        list.Add(scriptSubstring);
                        startIndex = m.Value + 2;
                    }
                }
            }

            //Get the last item into the collection...
            if (maintainBatchDelimiter)
            {
                list.Add(scriptContents.Substring(startIndex));
            }
            else
            {
                previousEndOfLine = regBackwardEndOfLine.Match(scriptContents, startIndex).Index;

                if (previousEndOfLine > 0)
                {
                    if (startIndex >= 2 && scriptContents.Substring(startIndex - 2, 2) == "\r\n")
                        startIndex = startIndex - 2;

                    modStartIndex = (startIndex == 0) ? startIndex : startIndex + Environment.NewLine.Length;
                    modStartIndex = (modStartIndex > scriptContents.Length) ? scriptContents.Length : modStartIndex;
                    string lastItem = scriptContents.Substring(modStartIndex);
                    lastItem = lastItem.ClearTrailingCarriageReturn();
                    list.Add(lastItem);
                }
                else
                {
                    string lastItem = scriptContents.Substring(startIndex).ClearTrailingCarriageReturn();
                    list.Add(lastItem);
                }
            }

            //If the last item is actually just whitespace, remove it..
            if (list[list.Count - 1].Trim().Length == 0)
                list.RemoveAt(list.Count - 1);

            //Remove trailing \r\n in the last item...(can't remember why, but the old methods do this so it needs to be done for backward compatability
            if (scriptContents.Trim().EndsWith("GO", StringComparison.CurrentCultureIgnoreCase))
                list[list.Count - 1] = list[list.Count - 1].ClearTrailingCarriageReturn();

            //Remove transaction references if applicable
            if (stripTransaction)
            {
                for (int i = 0; i < list.Count; i++)
                    list[i] = RemoveTransactionReferences(list[i]);
            }

            //Remove any "USE" statements
            for (int i = 0; i < list.Count; i++)
                list[i] = RemoveUseStatement(list[i]);

            //Remove anything that is completely empty
            list = list.Where(l => l.Length > 0).ToList();
            log.LogDebug($"Batched build package into {list.Count.ToString()} scripts");
            return list;
        }

        public string[] ReadBatchFromScriptFile(string fileName, bool stripTransaction, bool maintainBatchDelimiter)
        {
            //Procedured and functions should never have transaction text stripped..they may need it as part of their definition
            if (fileName.EndsWith(DbObjectType.StoredProcedure, StringComparison.CurrentCultureIgnoreCase) ||
                fileName.EndsWith(DbObjectType.UserDefinedFunction, StringComparison.CurrentCultureIgnoreCase) ||
                fileName.EndsWith(DbObjectType.Trigger, StringComparison.CurrentCultureIgnoreCase))
                stripTransaction = false;

            string scriptContents = File.ReadAllText(fileName);
            string[] batchNew = ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter).ToArray();
            return batchNew;
        }

        public ScriptBatchCollection LoadAndBatchSqlScripts(SqlSync.SqlBuild.Models.SqlSyncBuildDataModel model, string projectFilePath)
        {
            ScriptBatchCollection coll = new ScriptBatchCollection();
            var scripts = model.Script.OrderBy(s => s.BuildOrder ?? double.MaxValue).ToList();
            foreach (var s in scripts)
            {
                var fileName = s.FileName ?? string.Empty;
                var strip = s.StripTransactionText ?? false;
                var batchScripts = ReadBatchFromScriptFile(Path.Combine(projectFilePath, fileName), strip, false);
                var batch = new ScriptBatch(fileName, batchScripts, s.ScriptId ?? string.Empty);
                coll.Add(batch);
            }
            return coll;
        }

        public Task<List<string>> ReadBatchFromScriptTextAsync(string scriptContents, bool stripTransaction, bool maintainBatchDelimiter, CancellationToken cancellationToken = default)
            => Task.FromResult(ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter));

        public async Task<string[]> ReadBatchFromScriptFileAsync(string fileName, bool stripTransaction, bool maintainBatchDelimiter, CancellationToken cancellationToken = default)
        {
            var contents = await File.ReadAllTextAsync(fileName, cancellationToken).ConfigureAwait(false);
            var batches = ReadBatchFromScriptText(contents, stripTransaction, maintainBatchDelimiter);
            return batches.ToArray();
        }

        private List<KeyValuePair<Match, int>> FindActiveBatchDelimiters(string scriptContents)
        {
            //Regex for delimiter
            Regex regDelimiter = new Regex(Properties.Resources.RegexBatchParsingDelimiter, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            //Regex for seeing if delimiter is the only thing on the line
            Regex regNonWhiteSpace = new Regex(Properties.Resources.RegexNonWhiteSpace, RegexOptions.IgnoreCase);
            Regex regEndOfLine = new Regex(Properties.Resources.RegexEndOfLine, RegexOptions.IgnoreCase);

            //First, identify all of the delimiters...
            MatchCollection collDelimiter = regDelimiter.Matches(scriptContents);

            List<KeyValuePair<Match, int>> activeDelimiters = new List<KeyValuePair<Match, int>>();

            if (collDelimiter.Count == 0)
                return activeDelimiters;

            //Find the delimiters that are "real"
            foreach (Match delim in collDelimiter)
            {
                if (!IsInComment(scriptContents, delim.Index))
                {
                    //at the end of the string.
                    if (delim.Index + delim.Length == scriptContents.Length)
                    {
                        activeDelimiters.Add(new KeyValuePair<Match, int>(delim, -1));
                        continue;
                    }

                    int nextChar = regNonWhiteSpace.Match(scriptContents, delim.Index + delim.Length).Index;
                    int endOfLine = regEndOfLine.Match(scriptContents, delim.Index + delim.Length).Index;

                    if (endOfLine < nextChar || nextChar == 0 || endOfLine == 0)
                        activeDelimiters.Add(new KeyValuePair<Match, int>(delim, endOfLine));
                }
            }

            return activeDelimiters;
        }

        private string RemoveUseStatement(string script)
        {
            Regex regUse = new Regex(Properties.Resources.RegexUseStatement, RegexOptions.IgnoreCase | RegexOptions.Multiline);
            int startAt = 0;
            while (regUse.Match(script, startAt).Success)
            {
                Match m = regUse.Match(script, startAt);
                if (!IsInComment(script, m.Index))
                {
                    script = regUse.Replace(script, "", 1, m.Index);
                }
                else
                {
                    startAt = m.Index + m.Length;
                }
            }
            return script;
        }

        private string RemoveTransactionReferences(string script)
        {
            RegexOptions options = RegexOptions.IgnoreCase | RegexOptions.Singleline;
            script = RegexRemoveIfNotInComments(Properties.Resources.RegexTransaction, script, options);
            script = RegexRemoveIfNotInComments(Properties.Resources.RegexTran, script, options);
            script = RegexRemoveIfNotInComments(Properties.Resources.RegexCommit, script, options);
            script = RegexRemoveIfNotInComments(Properties.Resources.RegexTransactionLevel, script, options);
            return script;
        }

        private string RegexRemoveIfNotInComments(string regexExpression, string script, RegexOptions options)
        {
            Regex regRemoveTag = new Regex(regexExpression, options);
            int startAt = 0;
            while (regRemoveTag.Match(script, startAt).Success)
            {
                Match m = regRemoveTag.Match(script, startAt);
                if (!IsInComment(script, m.Index))
                {
                    script = regRemoveTag.Replace(script, "", 1, m.Index);
                }
                else
                {
                    startAt = m.Index + m.Length;
                }
            }
            return script;
        }

        private bool IsInComment(string script, int position)
        {
            return SqlBuildHelper.IsInComment(script, position);
        }
    }
}