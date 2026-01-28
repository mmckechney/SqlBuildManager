using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSync.SqlBuild
{
    internal class DefaultProgressReporter : IProgressReporter
    {
        public bool CancellationPending => throw new NotImplementedException();

        public void ReportProgress(int percent, object userState)
        {
            throw new NotImplementedException();
        }
        public void ReportProgress(object userState)
        {
            throw new NotImplementedException();
        }
    }
}
