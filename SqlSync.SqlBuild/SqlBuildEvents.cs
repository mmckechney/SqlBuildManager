using System;

namespace SqlSync.SqlBuild
{
	public delegate void ScriptRunProjectFileSavedEventHandler(object sender,bool saved);

	public delegate void ScriptRunStatusEventHandler(object sender,ScriptRunStatusEventArgs e); 
	public class ScriptRunStatusEventArgs : EventArgs
	{
		public readonly string Status;
        public readonly TimeSpan Duration;
        public ScriptRunStatusEventArgs(string status, TimeSpan duration)
		{
			this.Status = status;
            this.Duration = duration;
		}
	}

	#region ## Scripting Event Definition ##
	public delegate void BuildScriptEventHandler(object sender,BuildScriptEventArgs e); 
	public class BuildScriptEventArgs : EventArgs
	{
		public readonly string FileName;
		public readonly string Status;
		public readonly string Database;
		public readonly double BuildOrder;
		public readonly string ScriptId;
		public readonly double OriginalBuildOrder;
        public readonly bool StripTransactionText;
        public readonly Guid BuildScriptId;
        public BuildScriptEventArgs(double buildOrder, string fileName, string database, double originalBuildOrder, string status, string scriptId, bool stripTransactionText, Guid buildScriptId)
		{
			this.BuildOrder = buildOrder;
			this.Status = status;
			this.FileName = fileName;
			this.Database = database;
			this.ScriptId = scriptId;
			this.OriginalBuildOrder = originalBuildOrder;
            this.StripTransactionText = stripTransactionText;
            this.BuildScriptId = buildScriptId;
		}
	}
	#endregion

	#region ##Status Event Definition ##
	public delegate void GeneralStatusEventHandler(object sender, GeneralStatusEventArgs e);
	public class GeneralStatusEventArgs : EventArgs
	{
		public readonly string StatusMessage;
		public GeneralStatusEventArgs(string statusMessage)
		{
			this.StatusMessage = statusMessage;
		}
	}

	public delegate void CommitFailureEventHandler(object sender, CommitFailureEventArgs e);
	public class CommitFailureEventArgs : EventArgs
	{
		public readonly string ErrorMessage;
		public CommitFailureEventArgs(string errorMessage)
		{
			this.ErrorMessage = errorMessage;
		}
	}

	public delegate void ScriptLogWriteEventHandler(object sender, ScriptLogEventArgs e);
	public class ScriptLogEventArgs: EventArgs
	{
		public readonly int ScriptIndex;
		public readonly string SqlScript;
		public readonly string Results;
		public readonly string Database;
		public readonly string SourceFile;
        public readonly bool InsertStartTransaction;
		public ScriptLogEventArgs(int scriptIndex, string sqlScript,string database, string sourceFile, string results) :
            this(scriptIndex,sqlScript,database,sourceFile,results,false)
		{
		}
        public ScriptLogEventArgs(int scriptIndex, string sqlScript, string database, string sourceFile, string results, bool insertTransactionStart)
        {
            this.ScriptIndex = scriptIndex;
            this.SqlScript = sqlScript;
            this.Results = results;
            this.Database = database;
            this.SourceFile = sourceFile;
            this.InsertStartTransaction = insertTransactionStart;
        }

	}
    public class ScriptRunProjectFileSavedEventArgs : EventArgs
    {
        public bool Saved;
        public ScriptRunProjectFileSavedEventArgs(bool saved)
        {
            this.Saved = saved;
        }
    }
	#endregion


    public class BuildStartedEventArgs : EventArgs
    {
        public BuildStartedEventArgs()
        {
        }
    }

	public delegate void ScriptingErrorEventHandler(object sender, string message);

}
