using System;

namespace SqlSync.SqlBuild.Abstractions
{
    public interface IGuidProvider
    {
        Guid NewGuid();
    }
}
