using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Linq;
namespace SqlBuildManager.ScriptHandling
{
    public class ScriptHandlingHelper
    {
        #region Comments by Regex Match
        public static List<Match> GetScriptCommentBlocks(string rawScript)
        {
            List<Match> commentList = new List<Match>();

            Regex regDoubleDash = new Regex(@"(--.*\n)", RegexOptions.IgnoreCase);
            MatchCollection comment = regDoubleDash.Matches(rawScript);
            foreach (Match c in comment)
                commentList.Add(c);

            Regex regMultiLineComment = new Regex(@"(/\*.+?\*/)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            MatchCollection multiLineComment = regMultiLineComment.Matches(rawScript);
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

            Regex regDoubleDash = new Regex(@"(--.*\n)", RegexOptions.IgnoreCase | RegexOptions.Compiled);
            MatchCollection comment = regDoubleDash.Matches(rawScript);
            foreach (Match c in comment)
            {
                int start = c.Index;
                for (int i = c.Index; i <= start + c.Length; i++)
                    commentList.Add(i);
            }

            Regex regMultiLineComment = new Regex(@"(/\*.+?\*/)", RegexOptions.IgnoreCase | RegexOptions.Singleline | RegexOptions.Compiled);
            MatchCollection multiLineComment = regMultiLineComment.Matches(rawScript);
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
            Regex regCommentHeader = new Regex(@"(/\*\*.*\*\*/)", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            Match headerMatch = regCommentHeader.Match(rawScript);
            if (!headerMatch.Success)
                return false;

            if (index > headerMatch.Index &&
                index < headerMatch.Index + headerMatch.Length)
                return true;
          
            return false;
        }
    }
}
