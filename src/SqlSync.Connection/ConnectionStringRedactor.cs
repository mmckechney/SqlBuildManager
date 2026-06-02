using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace SqlSync.Connection
{
    internal static class ConnectionStringRedactor
    {
        private const string RedactedValue = "***REDACTED***";

        private static readonly HashSet<string> SensitiveKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Password",
            "Pwd",
            "Access Token",
            "Token"
        };

        public static string Redact(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                return string.Empty;
            }

            try
            {
                var builder = new DbConnectionStringBuilder
                {
                    ConnectionString = connectionString
                };

                foreach (string key in builder.Keys.Cast<string>().Where(IsSensitiveKey).ToList())
                {
                    builder[key] = RedactedValue;
                }

                return builder.ConnectionString;
            }
            catch (ArgumentException)
            {
                return RedactedValue;
            }
        }

        private static bool IsSensitiveKey(string key)
        {
            return SensitiveKeys.Contains(key)
                || key.EndsWith("Password", StringComparison.OrdinalIgnoreCase)
                || key.EndsWith("Token", StringComparison.OrdinalIgnoreCase);
        }
    }
}
