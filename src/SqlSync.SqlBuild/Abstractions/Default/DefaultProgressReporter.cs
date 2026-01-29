using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using SqlSync.SqlBuild.Status;

namespace SqlSync.SqlBuild.Abstractions.Default
{
    internal class DefaultProgressReporter : IProgressReporter
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public bool CancellationPending => throw new NotImplementedException();

        public void ReportProgress(int percent, object userState)
        {
            if (userState is ScriptRunStatusEventArgs scriptRunStatus)
            {
                if (scriptRunStatus.Duration == TimeSpan.Zero)
                {
                    log.LogInformation(scriptRunStatus.Status);
                } else
                {
                    log.LogInformation($"{scriptRunStatus.Status} - Duration: {scriptRunStatus.Duration}");
                }
            }
            else if (userState is CommitFailureEventArgs failureEventArgs)
            {
                log.LogError($"Commit Failure: {failureEventArgs.ErrorMessage}");
            }
            else
            {
                log.LogInformation(userState.ToString());
            }
        }
 
    }
}
