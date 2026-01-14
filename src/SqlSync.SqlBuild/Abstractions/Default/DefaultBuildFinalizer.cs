using SqlSync.SqlBuild.Models;
using System.ComponentModel;

namespace SqlSync.SqlBuild
{
    internal sealed class DefaultBuildFinalizer : IBuildFinalizer
    {
        public Build PerformRunScriptFinalization(bool buildFailure, Build myBuild, SqlSyncBuildDataModel buildDataModel, ref DoWorkEventArgs workEventArgs)
        {
            // Delegate to existing SqlBuildHelper behavior; placeholder for seam.
            return myBuild;
        }
    }
}
