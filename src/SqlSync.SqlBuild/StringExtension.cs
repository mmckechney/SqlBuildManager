using System.Text.RegularExpressions;
namespace SqlSync.SqlBuild
{
    public static class StringExtension
    {
        public static string ClearTrailingSpacesAndTabs(this string val)
        {
            while (val.EndsWith(" ") || val.EndsWith("\t"))
            {
                val = val.Substring(0, val.Length - 1);
            }
            return val;
        }
        public static string ClearTrailingCarriageReturn(this string val)
        {

            if (val.EndsWith("\r\n"))
                val = val.Substring(0, val.Length - 2);
            else if (val.EndsWith("\n"))
                val = val.Substring(0, val.Length - 1);
            return val;
        }
        public static string ConvertNewLinetoCarriageReturnNewLine(this string val)
        {
            Regex regNewLine = new Regex("(?<!\r)\n", RegexOptions.IgnoreCase);
            val = regNewLine.Replace(val, "\r\n");

            return val;
        }
    }
}
