using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.Azure.Batch.Common;
using Microsoft.Extensions.Logging;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.Threaded;
using SqlBuildManager.Interfaces.Console;
using SqlSync.Connection;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.MultiDb;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Linq;
using MoreLinq;
namespace SqlBuildManager.Console.Batch
{
    public class BatchCleaner
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public static int DeleteAllCompletedJobs(CommandLineArgs cmdLine)
        {
            try
            {
                List<Task> lstTasks = new List<Task>();
                BatchSharedKeyCredentials cred = new BatchSharedKeyCredentials(cmdLine.BatchArgs.BatchAccountUrl, cmdLine.BatchArgs.BatchAccountName, cmdLine.BatchArgs.BatchAccountKey);
                var batchClient = BatchClient.Open(cred);
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
            catch(Exception exe)
            {
                log.LogError($"Problem deleting Batch jobs: {exe.ToString()}");
                return 6859;
            }
        }
    }

   

}
