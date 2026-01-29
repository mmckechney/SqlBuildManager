using System.ComponentModel;

namespace SqlSync.SqlBuild.Abstractions
{
    public interface IProgressReporter
    {
        bool CancellationPending { get; }
        void ReportProgress(int percent, object userState);
    }
}
