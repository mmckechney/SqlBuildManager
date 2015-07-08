using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Linq;
namespace SqlSync.SqlBuild
{
    public class ProcessHelper
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static string keyValueSeparator = ":";
        public static void SetKeyValueSeparator(string separator)
        { keyValueSeparator = separator; }
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
        private class arguments
        {
            public string key;
            public string value;
            public string separator;
        }
        private List<arguments> argumentList = new List<arguments>();
        public void AddArgument(string key, string value)
        {
            AddArgument(key, value, keyValueSeparator);
        }
        public void AddArgument(string key, string value, string separator)
        {
            argumentList.Add(new arguments() { key = key, value = value, separator = separator });
        }
        public void ClearArguments()
        {
            argumentList.Clear();
        }

        public int ExecuteProcess(string processName)
        {
            if (argumentList.Count > 0)
            {
                StringBuilder sb = new StringBuilder();
                foreach (var item in argumentList)
                {
                    sb.AppendFormat("{0}{1}\"{2}\" ", item.key, item.separator, item.value);// (item.Key + ":\"" + item.Value + "\" ");
                }
                return this.ExecuteProcess(processName, sb.ToString());
            }
            else
            {
                return this.ExecuteProcess(processName, string.Empty);
            }
        }
        public int ExecuteProcess(string processName, string arguments)
        {
            log.InfoFormat("Executing process {0} with arguments {1}", processName, arguments);
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
