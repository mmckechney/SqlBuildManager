using System;
using System.Threading;

namespace SqlBuildManager.Console.ExternalTest
{
    public class ProcessHelper
    {
        /// <summary> 
        private string output = string.Empty;
        public string Output
        {
            get { return output; }
            set { output = value; }
        }

        private string error = string.Empty;
        public string Error
        {
            get { return error; }
            set { error = value; }
        }

        private DateTime startTime;
        public System.DateTime StartTime
        {
            get { return startTime; }
            set { startTime = value; }
        }

        private DateTime endTime;
        public System.DateTime EndTime
        {
            get { return endTime; }
            set { endTime = value; }
        }

        private System.Diagnostics.Process prc;

        public ProcessHelper()
        {

        }

        public int ExecuteProcess(string processName)
        {
            return ExecuteProcess(processName, string.Empty);
        }
        public int ExecuteProcess(string processName, string arguments)
        {
            startTime = DateTime.Now;
            prc = new System.Diagnostics.Process();
            prc.StartInfo.FileName = processName;
            prc.StartInfo.Arguments = arguments;
            prc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            prc.StartInfo.CreateNoWindow = true;
            prc.StartInfo.UseShellExecute = false;
            prc.StartInfo.RedirectStandardOutput = true;
            prc.StartInfo.RedirectStandardError = true;
            prc.Start();
            Thread THRoutput = new Thread(new ThreadStart(StdOutReader));
            Thread THRerror = new Thread(new ThreadStart(StdErrorReader));
            THRoutput.Name = "StdOutReader";
            THRerror.Name = "StdErrorReader";
            THRoutput.Start();
            THRerror.Start();

            prc.WaitForExit();

            THRoutput.Join(new TimeSpan(0, 3, 0));
            THRerror.Join(new TimeSpan(0, 3, 0));

            endTime = DateTime.Now;

            return prc.ExitCode;

        }


        private void StdOutReader()
        {
            try
            {
                output = "";
                string stdOutput = prc.StandardOutput.ReadToEnd();
                lock (this)
                {
                    output = stdOutput;
                }
            }
            catch (Exception e)
            {
                output += e.ToString();
            }
        }
        private void StdErrorReader()
        {
            try
            {
                error = "";
                string stdError = prc.StandardError.ReadToEnd();
                lock (this)
                {
                    error = stdError;
                }
            }
            catch (Exception e)
            {
                error += e.ToString();
            }
        }

    }
}
