using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SqlSync.Constants;
using SqlSync.SqlBuild.Utilities;

namespace SqlSync.SqlBuild.Services
{
    public sealed class DefaultScriptBatcher : IScriptBatcher
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!);

        // PERF-006/009: All regex instances compiled once as static readonly fields.
        private static readonly Regex _regDelimiter = new Regex(Properties.Resources.RegexBatchParsingDelimiter, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
        private static readonly Regex _regNonWhiteSpace = new Regex(Properties.Resources.RegexNonWhiteSpace, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _regEndOfLine = new Regex(Properties.Resources.RegexEndOfLine, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _regEndOfLineRtl = new Regex(Properties.Resources.RegexEndOfLine, RegexOptions.IgnoreCase | RegexOptions.RightToLeft | RegexOptions.Compiled);
        private static readonly Regex _regUse = new Regex(Properties.Resources.RegexUseStatement, RegexOptions.IgnoreCase | RegexOptions.Multiline | RegexOptions.Compiled);
        private static readonly Regex _regTransaction = new Regex(Properties.Resources.RegexTransaction, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex _regTran = new Regex(Properties.Resources.RegexTran, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex _regCommit = new Regex(Properties.Resources.RegexCommit, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex _regTransactionLevel = new Regex(Properties.Resources.RegexTransactionLevel, RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex _regDoubleDash = new Regex(@"(--.*\n)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _regMultiLineComment = new Regex(@"(/\*.+?\*/)", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        private readonly record struct CommentSpan(int Start, int End, bool EndInclusive);

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

            int previousEndOfLine = 0;
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
                    // PERF-006: Use static _regEndOfLineRtl instead of creating a new regex.
                    previousEndOfLine = _regEndOfLineRtl.Match(scriptContents, m.Key.Index).Index;
                    if (previousEndOfLine > 0)
                    {
                        if (startIndex >= 2 && scriptContents.Substring(startIndex - 2, 2) == "\r\n")
                            startIndex = startIndex - 2;

                        modStartIndex = (startIndex == 0) ? startIndex : startIndex + "\r\n".Length;
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
                previousEndOfLine = _regEndOfLineRtl.Match(scriptContents, startIndex).Index;

                if (previousEndOfLine > 0)
                {
                    if (startIndex >= 2 && scriptContents.Substring(startIndex - 2, 2) == "\r\n")
                        startIndex = startIndex - 2;

                    modStartIndex = (startIndex == 0) ? startIndex : startIndex + "\r\n".Length;
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
            // PERF-006: Use static cached regexes instead of creating new instances per call.
            MatchCollection collDelimiter = _regDelimiter.Matches(scriptContents);
            IReadOnlyList<CommentSpan> commentSpans = GetCommentSpans(scriptContents);

            List<KeyValuePair<Match, int>> activeDelimiters = new List<KeyValuePair<Match, int>>();

            if (collDelimiter.Count == 0)
                return activeDelimiters;

            //Find the delimiters that are "real"
            foreach (Match delim in collDelimiter)
            {
                if (!IsInComment(commentSpans, delim.Index))
                {
                    //at the end of the string.
                    if (delim.Index + delim.Length == scriptContents.Length)
                    {
                        activeDelimiters.Add(new KeyValuePair<Match, int>(delim, -1));
                        continue;
                    }

                    int nextChar = _regNonWhiteSpace.Match(scriptContents, delim.Index + delim.Length).Index;
                    int endOfLine = _regEndOfLine.Match(scriptContents, delim.Index + delim.Length).Index;

                    if (endOfLine < nextChar || nextChar == 0 || endOfLine == 0)
                        activeDelimiters.Add(new KeyValuePair<Match, int>(delim, endOfLine));
                }
            }

            return activeDelimiters;
        }

        private string RemoveUseStatement(string script)
        {
            return RemoveMatchesNotInComments(script, new[] { _regUse });
        }

        public string RemoveTransactionReferences(string script)
        {
            return RemoveMatchesNotInComments(
                script,
                new[] { _regTransaction, _regTran, _regCommit, _regTransactionLevel });
        }

        /// <summary>Pre-compiled regex overload — avoids per-call regex construction.</summary>
        public string RegexRemoveIfNotInComments(Regex regRemoveTag, string script)
        {
            return RemoveMatchesNotInComments(script, new[] { regRemoveTag });
        }

        public string RegexRemoveIfNotInComments(string regexExpression, string script, RegexOptions options)
        {
            Regex regRemoveTag = new Regex(regexExpression, options);
            return RegexRemoveIfNotInComments(regRemoveTag, script);
        }

        public bool IsInComment(string rawScript, int index)
        {
            return IsInComment(GetCommentSpans(rawScript), index);
        }

        private static IReadOnlyList<CommentSpan> GetCommentSpans(string script)
        {
            var spans = new List<CommentSpan>();
            foreach (Match match in _regDoubleDash.Matches(script))
            {
                spans.Add(new CommentSpan(match.Index, match.Index + match.Length, EndInclusive: false));
            }
            foreach (Match match in _regMultiLineComment.Matches(script))
            {
                spans.Add(new CommentSpan(match.Index, match.Index + match.Length, EndInclusive: true));
            }
            spans.Sort((left, right) => left.Start.CompareTo(right.Start));
            return spans;
        }

        private static bool IsInComment(IReadOnlyList<CommentSpan> spans, int index)
        {
            foreach (CommentSpan span in spans)
            {
                if (span.Start >= index)
                    return false;

                if (index > span.Start && (span.EndInclusive ? index <= span.End : index < span.End))
                    return true;
            }
            return false;
        }

        private static string RemoveMatchesNotInComments(string script, IReadOnlyList<Regex> regexes)
        {
            // Overlapping matches are ordered longest-first at a shared start index so compound
            // transaction forms (for example, COMMIT TRANSACTION) are removed as one span.
            IReadOnlyList<CommentSpan> commentSpans = GetCommentSpans(script);
            var matches = regexes
                .SelectMany(regex => regex.Matches(script).Cast<Match>())
                .Where(match => !IsInComment(commentSpans, match.Index))
                .OrderBy(match => match.Index)
                .ThenByDescending(match => match.Length)
                .ToList();

            if (matches.Count == 0)
                return script;

            var result = new StringBuilder(script.Length);
            int currentIndex = 0;
            foreach (Match match in matches)
            {
                if (match.Index < currentIndex)
                    continue;

                result.Append(script, currentIndex, match.Index - currentIndex);
                currentIndex = match.Index + match.Length;
            }
            result.Append(script, currentIndex, script.Length - currentIndex);
            return result.ToString();
        }

    }
}