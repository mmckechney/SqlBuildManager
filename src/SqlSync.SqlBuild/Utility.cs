using Microsoft.Extensions.Logging;
using SqlSync.Connection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SqlSync.SqlBuild.Models;
namespace SqlSync.SqlBuild
{
    public class UtilityHelper
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public const string ConfigFileName = "SqlSync.cfg";
        [Obsolete("Use GetRecentServers(out IReadOnlyList<ServerConfiguration> serverConfigs)")]
        public static List<string> GetRecentServers(out ServerConnectConfig.ServerConfigurationDataTable serverConfigTbl)
        {
            var recent = GetRecentServers(out IReadOnlyList<ServerConfiguration> serverConfigs);
            serverConfigTbl = serverConfigs.ToDataTable();
            return recent;
        }

        public static List<string> GetRecentServers(out IReadOnlyList<ServerConfiguration> serverConfigs)
        {
            serverConfigs = Array.Empty<ServerConfiguration>();
            string homePath = SqlBuildManager.Logging.Configure.AppDataPath;
            string cfgPath = Path.Combine(homePath, ConfigFileName);
            List<string> recentDbs = new();

            if (File.Exists(cfgPath))
            {
                try
                {
                    var model = ServerConnectConfigPersistence.Load(cfgPath);
                    serverConfigs = model.ServerConfiguration
                        .OrderByDescending(s => s.LastAccessed)
                        .ToList();
                    recentDbs.AddRange(serverConfigs.Select(s => s.Name));
                }
                catch (Exception ex)
                {
                    log.LogError(ex, "Error reading recent servers from {CfgPath}", cfgPath);
                }
            }
            return recentDbs;
        }
        public static void UpdateRecentServerList(string databaseName, string userName, string password, AuthenticationType authType)
        {
            try
            {
                userName = Cryptography.EncryptText(userName, ConnectionHelper.ConnectCryptoKey);
                password = Cryptography.EncryptText(password, ConnectionHelper.ConnectCryptoKey);
                string configFileFullPath = Path.Combine(SqlBuildManager.Logging.Configure.AppDataPath, ConfigFileName);
                var model = ServerConnectConfigPersistence.Load(configFileFullPath);
                var list = model.ServerConfiguration.ToList();
                var existing = list.FirstOrDefault(x => string.Equals(x.Name, databaseName, StringComparison.OrdinalIgnoreCase));
                var now = DateTime.UtcNow;
                if (existing is null)
                {
                    list.Add(new ServerConfiguration(databaseName, now, userName, password, authType.ToString()));
                }
                else
                {
                    var updated = existing with { LastAccessed = now, UserName = userName, Password = password, AuthenticationType = authType.ToString() };
                    var idx = list.IndexOf(existing);
                    list[idx] = updated;
                }

                var updatedModel = new ServerConnectConfigModel(list, model.LastProgramUpdateCheck, model.LastDirectory);
                ServerConnectConfigPersistence.Save(configFileFullPath, updatedModel);
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Error updating Recent Server List");
            }


        }
        public static AuthenticationType GetServerCredentials(ServerConnectConfig.ServerConfigurationDataTable serverConfigTbl, string serverName, out string username, out string password)
        {
            var pojo = serverConfigTbl.Cast<ServerConnectConfig.ServerConfigurationRow>()
                .Select(r => r.ToModel())
                .ToList();
            return GetServerCredentials(pojo, serverName, out username, out password);
        }

        public static AuthenticationType GetServerCredentials(IReadOnlyList<ServerConfiguration> serverConfigs, string serverName, out string username, out string password)
        {
            bool s;
            if (serverConfigs != null)
            {
                var row = serverConfigs.Where(r => r.Name.Trim().ToLower() == serverName.Trim().ToLower());
                if (row.Any())
                {
                    var r = row.First();
                    if (!string.IsNullOrWhiteSpace(r.UserName))
                    {

                        (s, username) = Cryptography.DecryptText(r.UserName, ConnectionHelper.ConnectCryptoKey, "UserName");
                    }
                    else
                    {
                        username = string.Empty;
                    }

                    if (!string.IsNullOrWhiteSpace(r.Password))
                    {
                        (s, password) = Cryptography.DecryptText(r.Password, ConnectionHelper.ConnectCryptoKey, "Password");
                    }
                    else
                    {
                        password = string.Empty;
                    }


                    AuthenticationType authType;
                    if (Enum.TryParse<AuthenticationType>(r.AuthenticationType, out authType))
                    {
                        return authType;
                    }
                    else
                    {
                        return AuthenticationType.Password;
                    }

                }
            }

            password = string.Empty;
            username = string.Empty;
            return AuthenticationType.Password;
        }

        public static void OpenManual(string anchor)
        {
            if (anchor == null)
                anchor = string.Empty;

            if (anchor.Length > 0)
            {
                if (!anchor.StartsWith("#")) anchor = "#" + anchor;
            }
            System.Diagnostics.Process prc = new Process();
            prc.StartInfo.UseShellExecute = true;
            prc.StartInfo.FileName = $"https://github.com/mmckechney/SqlBuildManager/blob/master/docs/SqlBuildManagerManual.md{anchor}"; // Utility.DefaultBrowser;

            prc.Start();
        }


    }
}

namespace SqlSync.SqlBuild
{
    public static class ServerConnectConfigExtensions
    {
        public static ServerConfiguration ToModel(this ServerConnectConfig.ServerConfigurationRow row)
        {
            var name = row.IsNameNull() ? string.Empty : row.Name;
            var lastAccessed = row.IsLastAccessedNull() ? DateTime.MinValue : row.LastAccessed;
            var userName = row.IsUserNameNull() ? null : row.UserName;
            var password = row.IsPasswordNull() ? null : row.Password;
            var authenticationType = row.IsAuthenticationTypeNull() ? null : row.AuthenticationType;
            return new ServerConfiguration(name, lastAccessed, userName, password, authenticationType);
        }

        public static ServerConnectConfig.ServerConfigurationDataTable ToDataTable(this IReadOnlyList<ServerConfiguration> configs)
        {
            var ds = new ServerConnectConfig();
            foreach (var sc in configs)
            {
                ds.ServerConfiguration.AddServerConfigurationRow(
                    sc.Name ?? string.Empty,
                    sc.LastAccessed,
                    sc.UserName ?? string.Empty,
                    sc.Password ?? string.Empty,
                    sc.AuthenticationType ?? string.Empty);
            }
            return ds.ServerConfiguration;
        }
    }
}
