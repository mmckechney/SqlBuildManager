using System;

namespace SqlBuildManager.SqlBuild.Services
{
    internal sealed class GuidProvider : IGuidProvider
    {
        public Guid NewGuid() => Guid.NewGuid();
    }
}
