using System;

namespace SqlSync.SqlBuild
{
    public interface IClock
    {
        DateTime UtcNow { get; }
        DateTime Now { get; }
    }
}
