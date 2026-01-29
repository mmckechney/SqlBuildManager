using System;
using System.Collections.Generic;

#nullable enable

namespace SqlSync.SqlBuild.Models
{
    public sealed record class ServerConfiguration(
        string Name,
        DateTime LastAccessed,
        string? UserName,
        string? Password,
        string? AuthenticationType);

    public sealed record class LastProgramUpdateCheck(DateTime? CheckTime);

    public sealed record class LastDirectory(string ComponentName, string Directory);

    public sealed record class ServerConnectConfigModel(
        IReadOnlyList<ServerConfiguration> ServerConfiguration,
        IReadOnlyList<LastProgramUpdateCheck> LastProgramUpdateCheck,
        IReadOnlyList<LastDirectory> LastDirectory);
}
