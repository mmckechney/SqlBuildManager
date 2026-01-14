using SqlSync.SqlBuild.Models;
using System.ComponentModel;

namespace SqlSync.SqlBuild
{
    public interface IBuildFinalizer
    {
        Build PerformRunScriptFinalization(bool buildFailure, Build myBuild, SqlSyncBuildDataModel buildDataModel, ref DoWorkEventArgs workEventArgs);
    }
}
