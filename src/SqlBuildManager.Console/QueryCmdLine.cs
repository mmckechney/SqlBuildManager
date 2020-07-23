using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
namespace SqlBuildManager.Console
{
    public class QueryCmdLine
    {
        public SqlSync.Connection.AuthenticationType authType { get; set; }
        public string username { get; set; }
        public string password { get; set; }
        public FileInfo queryFile { get; set; }
        public FileInfo Override { get; set; }
        public FileInfo outputFile { get; set; }
        public bool silent { get; set; }
        public int timeout { get; set; }
    }
}
