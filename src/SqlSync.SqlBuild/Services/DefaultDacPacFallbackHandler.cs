using Microsoft.Extensions.Logging;
using SqlBuildManager.Interfaces.Console;
using SqlSync.SqlBuild.Models;
using System.IO;

namespace SqlSync.SqlBuild.Services
{
    /// <summary>
    /// Default implementation of IDacPacFallbackHandler.
    /// </summary>
    internal sealed class DefaultDacPacFallbackHandler : IDacPacFallbackHandler
    {
        private static readonly ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(typeof(DefaultDacPacFallbackHandler));

        public bool IsCandidateForDacPacFallback(BuildItemStatus status)
        {
            switch (status)
            {
                case BuildItemStatus.Committed:
                case BuildItemStatus.CommittedWithTimeoutRetries:
                case BuildItemStatus.AlreadyInSync:
                case BuildItemStatus.TrialRolledBack:
                case BuildItemStatus.CommittedWithCustomDacpac:
                case BuildItemStatus.Pending:
                case BuildItemStatus.FailedDueToScriptTimeout:
                case BuildItemStatus.FailedWithCustomDacpac:
                    return false;

                case BuildItemStatus.RolledBack:
                case BuildItemStatus.PendingRollBack:
                case BuildItemStatus.FailedNoTransaction:
                case BuildItemStatus.RolledBackAfterRetries:
                    return true;

                default:
                    log.LogWarning($"Unrecognized Build Item status of {status}");
                    return true;
            }
        }

        public DacPacFallbackResult TryDacPacFallback(DacPacFallbackContext context, Build buildResult)
        {
            var result = new DacPacFallbackResult { WasAttempted = false };
            var runData = context.RunData;

            // Check if we should attempt DacPac fallback
            if (string.IsNullOrEmpty(runData.PlatinumDacPacFileName) || 
                !File.Exists(runData.PlatinumDacPacFileName) || 
                (runData.ForceCustomDacpac ?? false))
            {
                return result;
            }

            if (context.Prep?.FilteredScripts == null || context.Prep.FilteredScripts.Count == 0)
            {
                return result;
            }

            var database = context.Prep.FilteredScripts[0].Database;
            string targetDatabase = context.GetTargetDatabaseCallback(database);
            
            log.LogWarning($"Custom dacpac required for {context.ServerName} : {targetDatabase}. Generating file.");
            
            var (stat, updatedRunData) = DacPacHelper.UpdateBuildRunDataForDacPacSync(
                runData, 
                context.ServerName, 
                targetDatabase, 
                context.ConnectionData.AuthenticationType, 
                context.ConnectionData.UserId, 
                context.ConnectionData.Password, 
                context.ProjectFilePath, 
                runData.BuildRevision ?? string.Empty, 
                runData.DefaultScriptTimeout, 
                runData.AllowObjectDelete ?? false, 
                context.ConnectionData.ManagedIdentityClientId);

            result.WasAttempted = true;

            if (stat == DacpacDeltasStatus.Success)
            {
                log.LogInformation($"Executing custom dacpac on {targetDatabase}");
                var dacBuild = context.ProcessBuildCallback(
                    updatedRunData, 
                    context.ServerName, 
                    context.IsMultiDbRun, 
                    context.ScriptBatchColl, 
                    context.AllowableTimeoutRetries);
                
                var dacFinalStatus = dacBuild.FinalStatus;
                if (dacFinalStatus == BuildItemStatus.Committed || dacFinalStatus == BuildItemStatus.CommittedWithTimeoutRetries)
                {
                    result.NewStatus = BuildItemStatus.CommittedWithCustomDacpac;
                    context.RaiseBuildCommittedEvent?.Invoke(RunnerReturn.CommittedWithCustomDacpac);
                }
                else
                {
                    result.NewStatus = BuildItemStatus.FailedWithCustomDacpac;
                }
            }
            else if (stat == DacpacDeltasStatus.InSync || stat == DacpacDeltasStatus.OnlyPostDeployment)
            {
                result.NewStatus = BuildItemStatus.AlreadyInSync;
                context.RaiseBuildCommittedEvent?.Invoke(RunnerReturn.DacpacDatabasesInSync);
            }

            return result;
        }
    }
}
