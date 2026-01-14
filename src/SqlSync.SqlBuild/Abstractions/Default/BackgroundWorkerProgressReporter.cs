using System.ComponentModel;

namespace SqlSync.SqlBuild
{
    internal sealed class BackgroundWorkerProgressReporter : IProgressReporter
    {
        private readonly BackgroundWorker _bg;
        public BackgroundWorkerProgressReporter(BackgroundWorker bg) => _bg = bg;
        public bool CancellationPending => _bg?.CancellationPending ?? false;
        public void ReportProgress(int percent, object userState) => _bg?.ReportProgress(percent, userState);
    }
}
