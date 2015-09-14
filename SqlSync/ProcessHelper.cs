using System;
using System.Collections;
using System.Data;
using System.IO;
using System.Threading;
namespace SqlSync
{
    /// <summary>
    /// Summary description for ExecuteProcessHelper.
    /// </summary>
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
            return this.ExecuteProcess(processName, string.Empty);
        }
        public int ExecuteProcess(string processName, string arguments)
        {
            this.startTime = DateTime.Now;
            this.prc = new System.Diagnostics.Process();
            this.prc.StartInfo.FileName = processName;
            this.prc.StartInfo.Arguments = arguments;
            this.prc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            this.prc.StartInfo.CreateNoWindow = true;
            this.prc.StartInfo.UseShellExecute = false;
            this.prc.StartInfo.RedirectStandardOutput = true;
            this.prc.StartInfo.RedirectStandardError = true;
            prc.Start();
            Thread THRoutput = new Thread(new ThreadStart(StdOutReader));
            Thread THRerror = new Thread(new ThreadStart(StdErrorReader));
            THRoutput.Name = "StdOutReader";
            THRerror.Name = "StdErrorReader";
            THRoutput.Start();
            THRerror.Start();

            this.prc.WaitForExit();

            if (!THRoutput.Join(new TimeSpan(0, 3, 0))) THRoutput.Abort();
            if (!THRerror.Join(new TimeSpan(0, 3, 0))) THRerror.Abort();

            this.endTime = DateTime.Now;

			using(StreamWriter sw = File.AppendText(@"C:\SqlSync_AutoScript.log"))
			{
				sw.WriteLine("["+DateTime.Now.ToString()+"] Start Log");
				sw.WriteLine("["+DateTime.Now.ToString()+"] "+processName +" "+arguments);
				sw.WriteLine("["+DateTime.Now.ToString()+"] "+this.Output);
				sw.WriteLine("["+DateTime.Now.ToString()+"] "+this.Error);
				sw.WriteLine("["+DateTime.Now.ToString()+"] End Log\r\n");
			}
            return this.prc.ExitCode;

        }


        private void StdOutReader()
        {
            try
            {
                this.output = "";
                string stdOutput = this.prc.StandardOutput.ReadToEnd();
                lock (this)
                {
                    this.output = stdOutput;
                }
            }
            catch (Exception e)
            {
                this.output += e.ToString();
            }
        }
        private void StdErrorReader()
        {
            try
            {
                this.error = "";
                string stdError = this.prc.StandardError.ReadToEnd();
                lock (this)
                {
                    this.error = stdError;
                }
            }
            catch (Exception e)
            {
                this.error += e.ToString();
            }
        }

    }
}
