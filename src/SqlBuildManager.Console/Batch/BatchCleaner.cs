using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.Azure.Batch.Common;
using Microsoft.Extensions.Logging;
using MoreLinq;
using SqlBuildManager.Console.Aad;
using SqlBuildManager.Console.CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
namespace SqlBuildManager.Console.Batch
{
    public class BatchCleaner
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// Creates a BatchClient using either Managed Identity (preferred) or shared key credentials (fallback)
        /// </summary>
        private static BatchClient GetBatchClient(CommandLineArgs cmdLine)
        {
            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.BatchAccountKey))
            {
                // Legacy: Use shared key if provided
                log.LogDebug("Creating BatchClient with shared key credentials");
                var cred = new BatchSharedKeyCredentials(
                    cmdLine.ConnectionArgs.BatchAccountUrl,
                    cmdLine.ConnectionArgs.BatchAccountName,
                    cmdLine.ConnectionArgs.BatchAccountKey);
                return BatchClient.Open(cred);
            }
            else
            {
                // Use Managed Identity via Azure.Identity
                log.LogInformation("Creating BatchClient with Managed Identity (token credentials)");
                Func<Task<string>> tokenProvider = async () => await AadHelper.GetBatchTokenString();
                var cred = new BatchTokenCredentials(cmdLine.ConnectionArgs.BatchAccountUrl, tokenProvider);
                return BatchClient.Open(cred);
            }
        }

        public static int DeleteAllCompletedJobs(CommandLineArgs cmdLine)
        {
            try
            {
                List<Task> lstTasks = new List<Task>();
                var batchClient = GetBatchClient(cmdLine);
                var jobs = batchClient.JobOperations.ListJobs();
                var active = jobs.Where(j => j.State == JobState.Completed).ToList();

                //do in small sets to avoid hitting 429 errors
                var activeSets = active.Batch(5);
                foreach (var set in activeSets)
                {
                    set.ForEach(a =>
                     {
                         log.LogDebug($"Deleting Batch Job: '{a.Id}");
                         lstTasks.Add(a.DeleteAsync());
                     });
                    Task.WaitAll(lstTasks.ToArray());
                }
                return 0;
            }
            catch (Exception exe)
            {
                log.LogError($"Problem deleting Batch jobs: {exe.ToString()}");
                return 6859;
            }
        }
    }



}
