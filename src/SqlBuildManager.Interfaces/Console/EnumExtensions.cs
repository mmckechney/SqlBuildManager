using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlBuildManager.Interfaces.Console
{
    public static class EnumExtensions
    {
        public static RunnerReturn ToRunnerReturn(this BuildItemStatus status)
        {
            return status switch
            {
                BuildItemStatus.Committed => RunnerReturn.Committed,
                BuildItemStatus.CommittedWithTimeoutRetries => RunnerReturn.CommittedWithTimeoutRetries,
                BuildItemStatus.CommittedWithCustomDacpac => RunnerReturn.CommittedWithCustomDacpac,
                BuildItemStatus.Skipped => RunnerReturn.Skipped,
                BuildItemStatus.Canceled => RunnerReturn.Canceled,
                BuildItemStatus.Failed => RunnerReturn.Failed,
                _ => RunnerReturn.Failed,
            };
        }
    }
}
