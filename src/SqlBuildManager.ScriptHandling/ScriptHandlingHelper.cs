using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
namespace SqlBuildManager.ScriptHandling
{
    public class ScriptHandlingHelper
    {
        // PERF-006: Cached static regex instances – compilation cost is paid once.
        private static readonly Regex _regDoubleDash = new Regex(@"(--.*\n)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
        private static readonly Regex _regMultiLineComment = new Regex(@"(/\*.+?\*/)", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
        private static readonly Regex _regLargeCommentHeader = new Regex(@"(/\*\*.*\*\*/)", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);

        #region Comments by Regex Match
        public static List<Match> GetScriptCommentBlocks(string rawScript)
        {
            List<Match> commentList = new List<Match>();

            MatchCollection comment = _regDoubleDash.Matches(rawScript);
            foreach (Match c in comment)
                commentList.Add(c);

            MatchCollection multiLineComment = _regMultiLineComment.Matches(rawScript);
            foreach (Match c in multiLineComment)
                commentList.Add(c);

            return commentList;
        }



        public static bool IsInComment(int index, List<Match> commentBlockMatches)
        {
            if (commentBlockMatches.Count > 0)
            {
                var c = (from m in commentBlockMatches
                         where index > m.Index && index < m.Index + m.Length
                         select m);

                if (c.Count() > 0)
                {
                    return true;
                }
            }
            return false;
        }
        #endregion

        #region Comments by int index

        public static List<int> GetScriptCommentIndexes(string rawScript)
        {
            List<int> commentList = new List<int>();

            MatchCollection comment = _regDoubleDash.Matches(rawScript);
            foreach (Match c in comment)
            {
                int start = c.Index;
                for (int i = c.Index; i <= start + c.Length; i++)
                    commentList.Add(i);
            }

            MatchCollection multiLineComment = _regMultiLineComment.Matches(rawScript);
            foreach (Match c in multiLineComment)
            {
                int start = c.Index;
                for (int i = c.Index; i <= start + c.Length; i++)
                    commentList.Add(i); ;
            }

            return commentList;
        }

        public static bool IsInComment(int index, List<int> commentIndexes)
        {
            return commentIndexes.Contains(index);
        }

        #endregion


        public static bool IsInLargeCommentHeader(string rawScript, int index)
        {
            Match headerMatch = _regLargeCommentHeader.Match(rawScript);
            if (!headerMatch.Success)
                return false;

            if (index > headerMatch.Index &&
                index < headerMatch.Index + headerMatch.Length)
                return true;

            return false;
        }
    }
}
