using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SqlSync.ObjectScript
{
    public static partial class SanitizerExtensions
    {
        public static string Sanitize(this string input)
        {
            return input.IsValidSqlIdentifier().EscapeSqlIdentifier();
        }
        private static string IsValidSqlIdentifier(this string identifier)
        {
            return SqlTextRegex().Match(identifier).Success ? identifier : throw new ArgumentException($"Invalid SQL identifier: {identifier}");
        }
        private static string EscapeSqlIdentifier(this string identifier)
        {
            return $"[{identifier.Replace("]", "]]")}]";
        }

        [GeneratedRegex(@"^\[?[A-Za-z_][A-Za-z0-9_]*\]?$")]
        private static partial Regex SqlTextRegex();
    }
}
