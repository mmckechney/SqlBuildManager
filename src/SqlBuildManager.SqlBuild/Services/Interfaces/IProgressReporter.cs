using System.ComponentModel;

namespace SqlBuildManager.SqlBuild.Services
{
    public interface IProgressReporter
    {
        bool CancellationPending { get; }
        void ReportProgress(int percent, object userState);
    }
}
