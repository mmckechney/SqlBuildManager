using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;

namespace SqlSync.Connection
{
    /// <summary>
    /// Centralized helper for masking secrets before they are written to logs or other diagnostic output.
    /// Masking is partial so an operator can still visually validate which credential is in use without
    /// the full secret being exposed in logs:
    ///  - <see cref="MaskKey"/> (storage/account/shared-access keys): keep the first 4 characters,
    ///    replace the remainder with lowercase 'x'.
    ///  - <see cref="MaskPassword"/> (passwords): keep the first and last character, replace the middle
    ///    with lowercase 'x'.
    ///  - <see cref="RedactConnectionString"/>: mask each embedded sensitive value using the key rule
    ///    (first 4 characters preserved, remainder lowercase 'x'). Works for SQL, Storage, Service Bus
    ///    and Event Hub style connection strings.
    /// </summary>
    public static class ConnectionStringRedactor
    {
        private static readonly HashSet<string> SensitiveKeys = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "Password",
            "Pwd",
            "Access Token",
            "Token",
            "SharedAccessKey",
            "AccountKey",
            "SharedAccessSignature"
        };

        /// <summary>
        /// Masks a key/secret value, preserving the first 4 characters so an operator can visually verify
        /// which key is in use. The remainder is replaced with lowercase 'x'. Values of 4 characters or
        /// fewer are fully masked.
        /// </summary>
        public static string MaskKey(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            if (value.Length <= 4)
            {
                return new string('x', value.Length);
            }
            return value.Substring(0, 4) + new string('x', value.Length - 4);
        }

        /// <summary>
        /// Masks a password, preserving the first and last character with the middle replaced by lowercase
        /// 'x'. Values of 2 characters or fewer are fully masked.
        /// </summary>
        public static string MaskPassword(string value)
        {
            if (string.IsNullOrEmpty(value))
            {
                return value;
            }
            if (value.Length <= 2)
            {
                return new string('x', value.Length);
            }
            return value[0] + new string('x', value.Length - 2) + value[value.Length - 1];
        }

        /// <summary>
        /// Redacts sensitive values embedded in a connection string. Each sensitive value is masked using
        /// the key rule (first 4 characters preserved, remainder lowercase 'x'). If the connection string
        /// cannot be parsed it is masked in its entirety so a raw secret is never emitted.
        /// </summary>
        public static string RedactConnectionString(string connectionString)
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
                    builder[key] = MaskKey(builder[key]?.ToString() ?? string.Empty);
                }

                return builder.ConnectionString;
            }
            catch (ArgumentException)
            {
                return MaskKey(connectionString);
            }
        }

        /// <summary>
        /// Backward-compatible alias for <see cref="RedactConnectionString"/>.
        /// </summary>
        public static string Redact(string connectionString) => RedactConnectionString(connectionString);

        private static bool IsSensitiveKey(string key)
        {
            return SensitiveKeys.Contains(key)
                || key.EndsWith("Password", StringComparison.OrdinalIgnoreCase)
                || key.EndsWith("Token", StringComparison.OrdinalIgnoreCase)
                || key.EndsWith("AccountKey", StringComparison.OrdinalIgnoreCase)
                || key.EndsWith("AccessKey", StringComparison.OrdinalIgnoreCase);
        }
    }
}
