using System;

namespace SqlBuildManager.SqlBuild.Services
{
    public interface IClock
    {
        DateTime UtcNow { get; }
        DateTime Now { get; }
    }
}
