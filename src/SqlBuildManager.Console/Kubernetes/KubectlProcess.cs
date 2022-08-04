using System;
using System.Collections.Generic;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.Management.Network.Fluent.Models;
using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace SqlBuildManager.Console.Kubernetes
{
    internal class KubectlProcess
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static int ApplyFile(string fileName)
        {
            (int resp, string output, string error) =  RunKubectl($"apply -f \"{fileName}\"");
            return resp;
        }

        public static int DeleteKubernetesResource(string resourceKind, string resourceName)
        {
            (int resp, string output, string error) =  RunKubectl($"delete  {resourceKind} {resourceName}");
            return resp;
        }

        public static PodStatus GetJobPodStatus()
        {
            bool hasError = false;
            bool hasRunning = false;
            bool hasCompleted = false;
            bool hasPending = false;
            (int resp, string output, string error) = RunKubectl($"get pods", false);
            if(resp == 0)
            {
                var arrOut = output.Split("\n", StringSplitOptions.RemoveEmptyEntries);
                foreach(var line in arrOut)
                {
                    if(line.Trim().StartsWith("sqlbuildmanager"))
                    {
                        var linearr = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                        if (line.ToLower().Contains("error") || line.ToLower().Contains("crashloopbackoff"))
                        {
                            log.LogError($"Pod {linearr[0]} is showing error status {linearr[2]}");
                            hasError = true;
                        }
                        if(line.ToLower().Contains("running"))
                        {
                            log.LogError($"Pod {linearr[0]} is running");
                            hasRunning = true;
                        }
                        if(line.ToLower().Contains("completed"))
                        {
                            log.LogError($"Pod {linearr[0]} is completed");
                            hasCompleted = true;
                        }
                        if (line.ToLower().Contains("pending"))
{
                            log.LogError($"Pod {linearr[0]} is pending");
                            hasPending = true;
                        }
                    }
                }
                if(hasRunning)
                {
                    return PodStatus.Running;
                }
                if(hasCompleted)
                {
                    return PodStatus.Completed;
                }
                if (hasPending)
                {
                    return PodStatus.Pending;
                }
                if (hasError)
                {
                    return PodStatus.Error;
                }
                return PodStatus.Unknown;
            }
            else
            {
                log.LogError("Unable to monitor the status of pods");
                return PodStatus.KubectlError;
            }
        }

        private static (int, string, string) RunKubectl(string arguments, bool logresults = true)
        {
            using (System.Diagnostics.Process pProcess = new System.Diagnostics.Process())
            {
                pProcess.StartInfo.FileName = "kubectl";
                pProcess.StartInfo.Arguments = arguments;
                pProcess.StartInfo.UseShellExecute = false;
                pProcess.StartInfo.RedirectStandardOutput = true;
                pProcess.StartInfo.RedirectStandardError = true;
                pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                pProcess.StartInfo.CreateNoWindow = true; //not diplay a windows
                pProcess.Start();
                string output = pProcess.StandardOutput.ReadToEnd();
                string error = pProcess.StandardError.ReadToEnd();

                pProcess.WaitForExit();
                if (!string.IsNullOrEmpty(output) && logresults)
                {
                    log.LogInformation($"Kubectl output: {output.TrimEnd()}");
                }
                if (!string.IsNullOrEmpty(error) && logresults)
                {
                    log.LogError($"Kubectl output: {error.TrimEnd()}");
                }
                return (pProcess.ExitCode, output.TrimEnd(), error.TrimEnd());
            }
        }
    }
}
