using Microsoft.Data.SqlClient;
using System;
using System.Linq;

namespace SqlBuildManager.Connection
{
    internal static class AzureDatabaseTls
    {
        internal static bool IsMySql(string server) => server.Split(',').Any(endpoint =>
            HasSuffix(endpoint.Trim().Split(':')[0], new[]
            {
                ".mysql.database.azure.com", ".mysql.database.usgovcloudapi.net",
                ".mysql.database.chinacloudapi.cn", ".mysql.database.microsoftazure.de"
            }));

        internal static void Apply(SqlConnectionStringBuilder builder)
        {
            var host = builder.DataSource.Trim();
            if (host.StartsWith("tcp:", StringComparison.OrdinalIgnoreCase))
                host = host[4..];
            host = host.Split(',')[0];
            if (!HasSuffix(host, new[]
            {
                ".database.windows.net", ".database.usgovcloudapi.net",
                ".database.chinacloudapi.cn", ".database.cloudapi.de"
            }))
                return;

            builder.Encrypt = SqlConnectionEncryptOption.Mandatory;
            builder.TrustServerCertificate = false;
        }

        private static bool HasSuffix(string host, string[] suffixes) =>
            suffixes.Any(suffix => host.Trim().TrimEnd('.').EndsWith(suffix, StringComparison.OrdinalIgnoreCase));
    }
}
