using System.Collections.Generic;

namespace SqlSync.SqlBuild.Models
{
    /// <summary>
    /// Result of preparing a build for execution. Contains filtered scripts,
    /// the build record, and the package hash.
    /// </summary>
    public sealed record BuildPreparationResult(
        IList<Script> FilteredScripts,
        Build Build,
        string BuildPackageHash
    );
}
