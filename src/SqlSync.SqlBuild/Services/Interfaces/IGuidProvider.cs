using System;

namespace SqlSync.SqlBuild.Services
{
    public interface IGuidProvider
    {
        Guid NewGuid();
    }
}
