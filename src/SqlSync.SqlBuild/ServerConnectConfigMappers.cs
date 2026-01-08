using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlSync.SqlBuild
{
    public static class ServerConnectConfigMappers
    {
        public static ServerConnectConfigModel ToModel(this ServerConnectConfig ds)
        {
            var serverConfigs = ds?.ServerConfiguration?.Cast<ServerConnectConfig.ServerConfigurationRow>()
                .Select(r => r.ToModel())
                .ToList() ?? new List<ServerConfiguration>();

            var lastProgramUpdateChecks = ds?.LastProgramUpdateCheck?.Cast<ServerConnectConfig.LastProgramUpdateCheckRow>()
                .Select(r => r.ToModel())
                .ToList() ?? new List<LastProgramUpdateCheck>();

            var lastDirectories = ds?.LastDirectory?.Cast<ServerConnectConfig.LastDirectoryRow>()
                .Select(r => r.ToModel())
                .ToList() ?? new List<LastDirectory>();

            return new ServerConnectConfigModel(serverConfigs, lastProgramUpdateChecks, lastDirectories);
        }

        public static ServerConfiguration ToModel(this ServerConnectConfig.ServerConfigurationRow row)
        {
            var name = row.IsNameNull() ? string.Empty : row.Name;
            var lastAccessed = row.IsLastAccessedNull() ? DateTime.MinValue : row.LastAccessed;
            var userName = row.IsUserNameNull() ? null : row.UserName;
            var password = row.IsPasswordNull() ? null : row.Password;
            var authenticationType = row.IsAuthenticationTypeNull() ? null : row.AuthenticationType;
            return new ServerConfiguration(name, lastAccessed, userName, password, authenticationType);
        }

        public static LastProgramUpdateCheck ToModel(this ServerConnectConfig.LastProgramUpdateCheckRow row)
        {
            var checkTime = row.IsCheckTimeNull() ? (DateTime?)null : row.CheckTime;
            return new LastProgramUpdateCheck(checkTime);
        }

        public static LastDirectory ToModel(this ServerConnectConfig.LastDirectoryRow row)
        {
            var componentName = row.IsComponentNameNull() ? string.Empty : row.ComponentName;
            var directory = row.IsDirectoryNull() ? string.Empty : row.Directory;
            return new LastDirectory(componentName, directory);
        }

        public static ServerConnectConfig ToDataSet(this ServerConnectConfigModel model)
        {
            var ds = new ServerConnectConfig();

            foreach (var sc in model.ServerConfiguration)
            {
                ds.ServerConfiguration.AddServerConfigurationRow(
                    sc.Name ?? string.Empty,
                    sc.LastAccessed,
                    sc.UserName ?? string.Empty,
                    sc.Password ?? string.Empty,
                    sc.AuthenticationType ?? string.Empty);
            }

            foreach (var lpuc in model.LastProgramUpdateCheck)
            {
                if (lpuc.CheckTime.HasValue)
                {
                    ds.LastProgramUpdateCheck.AddLastProgramUpdateCheckRow(lpuc.CheckTime.Value);
                }
                else
                {
                    var row = ds.LastProgramUpdateCheck.NewLastProgramUpdateCheckRow();
                    row.SetCheckTimeNull();
                    ds.LastProgramUpdateCheck.AddLastProgramUpdateCheckRow(row);
                }
            }

            foreach (var ld in model.LastDirectory)
            {
                ds.LastDirectory.AddLastDirectoryRow(ld.ComponentName ?? string.Empty, ld.Directory ?? string.Empty);
            }

            return ds;
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
