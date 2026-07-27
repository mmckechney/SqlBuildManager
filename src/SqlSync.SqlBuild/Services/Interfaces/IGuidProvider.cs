using System;

namespace SqlBuildManager.SqlBuild.Services
{
    public interface IGuidProvider
    {
        Guid NewGuid();
    }
}
