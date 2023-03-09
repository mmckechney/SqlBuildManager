﻿using Microsoft.Extensions.Logging;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.ContainerApp;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SqlBuildManager.Console
{
    internal partial class Worker
    {
        internal static int SaveAndEncryptContainerAppSettings(CommandLineArgs cmdLine, bool clearText)
        {
            cmdLine.BatchArgs = null;
            cmdLine.ConnectionArgs.BatchAccountKey = null;
            cmdLine.ConnectionArgs.BatchAccountName = null;
            cmdLine.ConnectionArgs.BatchAccountUrl = null;
            cmdLine.AciArgs = null;
            cmdLine.IdentityArgs.PrincipalId = null;
            cmdLine.IdentityArgs.ResourceId = null;
            cmdLine.KubernetesArgs = null;
            return SaveAndEncryptSettings(cmdLine, clearText);
        }
        internal static async Task<int> ContainerAppsRun(CommandLineArgs cmdLine, bool unittest, bool stream, bool monitor, bool deleteWhenDone, bool force)
        {
            FileInfo packageFileInfo = string.IsNullOrWhiteSpace(cmdLine.BuildFileName) ? null : new FileInfo(cmdLine.BuildFileName);
            FileInfo dacpacFileInfo = string.IsNullOrWhiteSpace(cmdLine.DacpacName) ? null : new FileInfo(cmdLine.DacpacName);
            var res = await PrepAndUploadContainerBuildPackage(cmdLine, packageFileInfo, dacpacFileInfo, force);
            if (res != 0)
            {
                log.LogError("Failed to upload build package to Blob storage");
                return 1;
            }

            res = await EnqueueOverrideTargets(cmdLine);
            if (res != 0)
            {
                log.LogError("Failed to enqueue override targets");
                return 1;
            }

            res = await DeployContainerApp(cmdLine, unittest, stream, true, deleteWhenDone);
            if (res != -7)
            {
                log.LogError("Failed to deploy container app");
                log.LogInformation("Cleaning up any remaining queue messages...");
                await DeQueueOverrideTargets(cmdLine);
                return res;
            }
            else
            {
                return res;
            }
        }
        internal static async Task<int> DeployContainerApp(CommandLineArgs cmdLine, bool unittest, bool stream, bool monitor, bool deleteWhenDone)
        {
            bool initSuccess;
            (initSuccess, cmdLine) = Init(cmdLine);
            var validationErrors = Validation.ValidateContainerAppArgs(cmdLine);

            if (validationErrors.Count > 0)
            {
                validationErrors.ForEach(m => log.LogError(m));
                return -1;
            }
            int retVal = 0;
            var utcMonitorStart = DateTime.UtcNow;
            var success = await ContainerApp.ContainerAppHelper.DeployContainerApp(cmdLine);
            if (!success) retVal = -7;


            if (success && monitor)
            {
                retVal = await MonitorContainerAppRuntimeProgress(cmdLine, stream, utcMonitorStart, unittest);
            }
            if (deleteWhenDone)
            {
                success = await ContainerAppHelper.DeleteContainerApp(cmdLine);
                if (!success) retVal = -6;
            }

            return retVal;

        }

      

        internal static async Task<int> ContainerAppTestSettings(CommandLineArgs cmdLine)
        {
            bool initSuccess;
            (initSuccess, cmdLine) = Init(cmdLine);
            var validationErrors = Validation.ValidateContainerAppArgs(cmdLine);

            if (validationErrors.Count > 0)
            {
                validationErrors.ForEach(m => log.LogError(m));
            }
            ContainerAppHelper.SetEnvVariablesForTest(cmdLine);
            return await RunContainerAppWorker(cmdLine);
        }

        internal static async Task<int> MonitorContainerAppRuntimeProgress(CommandLineArgs cmdLine, bool stream, DateTime? utcMonitorStart, bool unitest)
        {

            if (string.IsNullOrWhiteSpace(cmdLine.JobName))
            {
                log.LogError("A --jobname value is required.");
                return 1;
            }

            var retVal = await MonitorServiceBusRuntimeProgress(cmdLine, stream, utcMonitorStart, unitest, false);
            ConsolidateRuntimeLogFiles(cmdLine);

            return retVal;
        }
    }
}
