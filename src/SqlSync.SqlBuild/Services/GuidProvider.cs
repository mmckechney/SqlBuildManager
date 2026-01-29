using System;

namespace SqlSync.SqlBuild.Services
{
    internal sealed class GuidProvider : IGuidProvider
    {
        public Guid NewGuid() => Guid.NewGuid();
    }
}
