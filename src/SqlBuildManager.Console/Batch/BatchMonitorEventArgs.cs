using SqlBuildManager.Console.CommandLine;
using System;

namespace SqlBuildManager.Console.Batch
{
    public delegate void BatchMonitorEventHandler(object sender, BatchMonitorEventArgs e);
    public class BatchMonitorEventArgs : EventArgs
    {

        public BatchMonitorEventArgs(CommandLineArgs cmdLine, bool stream, bool unittest)
        {
            CmdLine = cmdLine;
            Stream = stream;
            UnitTest = unittest;
        }

        public CommandLineArgs CmdLine { get; }
        public bool Stream { get; }
        public bool UnitTest { get; }
    }

}