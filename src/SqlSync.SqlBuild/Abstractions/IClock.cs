using System;

namespace SqlSync.SqlBuild.Abstractions
{
    public interface IClock
    {
        DateTime UtcNow { get; }
        DateTime Now { get; }
    }
}
