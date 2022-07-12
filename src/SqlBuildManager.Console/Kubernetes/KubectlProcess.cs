using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
namespace SqlBuildManager.Console.Kubernetes
{
    internal class KubectlProcess
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static int ApplyFile(string fileName)
        {
            using (System.Diagnostics.Process pProcess = new System.Diagnostics.Process())
            {
                pProcess.StartInfo.FileName = "kubectl";
                pProcess.StartInfo.Arguments = $"apply -f \"{fileName}\""; 
                pProcess.StartInfo.UseShellExecute = false;
                pProcess.StartInfo.RedirectStandardOutput = true;
                pProcess.StartInfo.RedirectStandardError = true;
                pProcess.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                pProcess.StartInfo.CreateNoWindow = true; //not diplay a windows
                pProcess.Start();
                string output = pProcess.StandardOutput.ReadToEnd(); 
                string error = pProcess.StandardError.ReadToEnd();

                pProcess.WaitForExit();
                if(!string.IsNullOrEmpty(output))
                {
                    log.LogInformation(output);
                }
                if (!string.IsNullOrEmpty(error))
                {
                    log.LogError(error);
                }
                return pProcess.ExitCode;
            }
        }
    }
}
