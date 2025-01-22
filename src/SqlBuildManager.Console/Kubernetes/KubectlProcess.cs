using Microsoft.Extensions.Logging;
using System;

namespace SqlBuildManager.Console.Kubernetes
{
   internal class KubectlProcess
   {
      private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
      public static int ApplyFile(string fileName)
      {
         (int resp, string output, string error) = RunKubectl($"apply -f \"{fileName}\"");
         return resp;
      }

      public static int DeleteKubernetesResource(string resourceKind, string resourceName, string k8namespace = "")
      {
         if (!string.IsNullOrWhiteSpace(k8namespace))
         {
            k8namespace = $" -n {k8namespace}";
         }
         (int resp, string output, string error) = RunKubectl($"delete  {resourceKind} {resourceName} {k8namespace}");
         return resp;
      }
      public static int CreateKubernetesResource(string resourceKind, string resourceName, string k8namespace = "")
      {
         if (!string.IsNullOrWhiteSpace(k8namespace))
         {
            k8namespace = $" -n {k8namespace}";
         }
         (int resp, string output, string error) = RunKubectl($"create  {resourceKind} {resourceName} {k8namespace}");
         return resp;
      }

      public static string GetKubernetesResourcesInNamespace(string resourceKind, string k8namespace = "")
      {
         if (!string.IsNullOrWhiteSpace(k8namespace))
         {
            k8namespace = $" -n {k8namespace}";
         }
         (int resp, string output, string error) = RunKubectl($"get  {resourceKind} {k8namespace} -o json");
         return output;
      }
      public static int DescribeKubernetesResource(string resourceKind, string resourceName, string k8namespace = "")
      {
         if (!string.IsNullOrWhiteSpace(k8namespace))
         {
            k8namespace = $" -n {k8namespace}";
         }
         (int resp, string output, string error) = RunKubectl($"describe  {resourceKind} {resourceName} {k8namespace}", false);
         return resp;
      }

      public static PodStatus GetJobPodStatus(string k8jobName, bool logStatus)
      {
         int inError = 0;
         int inRunning = 0;
         int inCompleted = 0;
         int inPending = 0;
         int inCreating = 0;
         int totalPods = 0;

         (int resp, string output, string error) = RunKubectl($"get pods -n {KubernetesManager.SbmNamespace}", false);
         if (resp == 0)
         {
            var arrOut = output.Split("\n", StringSplitOptions.RemoveEmptyEntries);
            foreach (var line in arrOut)
            {
               if (line.Trim().StartsWith(k8jobName))
               {
                  totalPods++;

                  var linearr = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                  if (line.ToLower().Contains("error") || line.ToLower().Contains("crashloopbackoff"))
                  {
                     if (logStatus) log.LogError($"Pod {linearr[0]} is showing error status {linearr[2]}");
                     inError++;
                  }
                  if (line.ToLower().Contains("running"))
                  {
                     if (logStatus) log.LogInformation($"Pod {linearr[0]} is running");
                     inRunning++;
                  }
                  if (line.ToLower().Contains("completed"))
                  {
                     if (logStatus) log.LogInformation($"Pod {linearr[0]} is completed");
                     inCompleted++;
                  }
                  if (line.ToLower().Contains("creating"))
                  {
                     if (logStatus) log.LogInformation($"Pod {linearr[0]} is creating");
                     inCreating++;
                  }
                  if (line.ToLower().Contains("pending"))
                  {
                     if (logStatus) log.LogWarning($"Pod {linearr[0]} is pending");
                     inPending++;
                  }
               }
            }


            //If any pods are running or creating, return as such..
            if (inRunning > 0)
            {
               return PodStatus.Running;
            }
            else if (inCreating > 0)
            {
               return PodStatus.Creating;
            }
            else if (inCompleted > 0)  //The process may have completed even if there are pods in pending or error state, but not if some are in running or getting created.
            {
               return PodStatus.Completed;
            }

            //Check for completed first since there could be pods waiting to start but the overall process may be complete.
            if (inPending > 0)
            {
               return PodStatus.Pending;
            }
            //Only return error if all pods report an error
            if (inError > 0 && inError == totalPods)
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
