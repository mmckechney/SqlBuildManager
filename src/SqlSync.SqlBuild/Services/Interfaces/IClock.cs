using System;

namespace SqlSync.SqlBuild.Services
{
    public interface IClock
    {
        DateTime UtcNow { get; }
        DateTime Now { get; }
    }
}
