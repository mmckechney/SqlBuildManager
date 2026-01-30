using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlSync.SqlBuild;

namespace SqlBuildManager.Interfaces.Console
{
    public static class EnumExtensions
    {
        public static RunnerReturn ToRunnerReturn(this BuildItemStatus status)
        {
            return status switch
            {
                BuildItemStatus.Committed => RunnerReturn.BuildCommitted,
                BuildItemStatus.CommittedWithTimeoutRetries => RunnerReturn.BuildCommitted,
                BuildItemStatus.CommittedWithCustomDacpac => RunnerReturn.CommittedWithCustomDacpac,
                BuildItemStatus.RolledBack => RunnerReturn.RolledBack,
                BuildItemStatus.TrialRolledBack => RunnerReturn.SuccessWithTrialRolledBack,
                BuildItemStatus.FailedNoTransaction => RunnerReturn.BuildErrorNonTransactional,
                BuildItemStatus.RolledBackAfterRetries => RunnerReturn.RolledBack,
                BuildItemStatus.AlreadyInSync => RunnerReturn.DacpacDatabasesInSync,
                BuildItemStatus.FailedDueToScriptTimeout => RunnerReturn.RolledBack,
                BuildItemStatus.FailedWithCustomDacpac => RunnerReturn.RolledBack,

                BuildItemStatus.Unknown => RunnerReturn.BuildResultInconclusive,
                BuildItemStatus.PendingRollBack => RunnerReturn.BuildResultInconclusive,
                BuildItemStatus.Pending => RunnerReturn.BuildResultInconclusive,
                _ => RunnerReturn.BuildResultInconclusive,
            };
        }
        public static RunnerReturn ToRunnerReturn(this ExecutionReturn status)
        {
            return status switch
            {
                ExecutionReturn.Successful => RunnerReturn.BuildCommitted,
                ExecutionReturn.DacpacDatabasesInSync => RunnerReturn.DacpacDatabasesInSync,
                ExecutionReturn.FinishingWithErrors => RunnerReturn.RolledBack,
                ExecutionReturn.OneOrMoreRemoteServersHadError => RunnerReturn.RolledBack,
                ExecutionReturn.ProcessBuildError => RunnerReturn.ProcessBuildError,
                _ => RunnerReturn.BuildResultInconclusive,
            };
        }
    }
}
