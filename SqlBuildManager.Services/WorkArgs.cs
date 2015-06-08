using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using SqlSync.SqlBuild.MultiDb;
using SqlBuildManager.Services.History;
namespace SqlBuildManager.Services
{
    internal class WorkArgs
    {
        public WorkArgs(string buildZipFileName, string platinumDacPacFileName, MultiDbData multiDbData, string rootLoggingPath, string description, BuildRecord record)
        {
            this.MultiDbData = multiDbData;
            this.BuildZipFileName = buildZipFileName;
            this.RootLoggingPath = rootLoggingPath;
            this.Description = description;
            this.Record = record;
            this.PlatinumDacPacFileName = platinumDacPacFileName;
        }
        public readonly string BuildZipFileName;
        public readonly string RootLoggingPath;
        public readonly MultiDbData MultiDbData;
        public readonly string Description;
        public readonly BuildRecord Record;
        public readonly string PlatinumDacPacFileName;

    }
}
