using System;

namespace SqlSync.SqlBuild
{
    internal sealed class GuidProvider : IGuidProvider
    {
        public Guid NewGuid() => Guid.NewGuid();
    }
}
