using System;

namespace SqlSync.SqlBuild.SqlLogging
{
	public class CommittedScript
	{
		public readonly System.Guid ScriptId;
		public readonly string FileHash;
		public readonly int Sequence;
		public readonly string ScriptText;
        public readonly string Tag;
        public readonly string ServerName;
        public readonly string DatabaseTarget;
        public DateTime RunStart
        {
            get;
            set;
        }
        public DateTime RunEnd
        {
            get;
            set;
        }
		public CommittedScript(System.Guid scriptId, string fileHash,int sequence, string scriptText, string tag, string serverName, string databaseTarget)
		{
			this.ScriptId = scriptId;
			this.FileHash = fileHash;
			this.Sequence = sequence;
			this.ScriptText = scriptText;
            this.Tag = tag;
            this.ServerName = serverName;
            this.DatabaseTarget = databaseTarget;
		}
	}
}
