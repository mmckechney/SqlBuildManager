using System.ComponentModel;

namespace SqlSync.SqlBuild.Services
{
    public interface IProgressReporter
    {
        bool CancellationPending { get; }
        void ReportProgress(int percent, object userState);
    }
}
