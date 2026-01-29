using System;

namespace SqlSync.SqlBuild.Abstractions.Default
{
    internal sealed class GuidProvider : IGuidProvider
    {
        public Guid NewGuid() => Guid.NewGuid();
    }
}
