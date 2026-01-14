using System;

namespace SqlSync.SqlBuild
{
    public interface IGuidProvider
    {
        Guid NewGuid();
    }
}
