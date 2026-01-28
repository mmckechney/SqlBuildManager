using System.ComponentModel;

namespace SqlSync.SqlBuild
{
    public interface IProgressReporter
    {
        bool CancellationPending { get; }
        void ReportProgress(int percent, object userState);
        //void ReportProgress(object userState);
    }
}
