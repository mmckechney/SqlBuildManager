using System;


namespace SqlSync.TableScript
{
	public delegate bool HighRowCountEventHandler(object sender, HighRowCountEventArgs e);
	public class HighRowCountEventArgs : EventArgs
	{
		public readonly int RowCount;
		public readonly string TableName;
		public HighRowCountEventArgs(string tableName, int rowCount)
		{
			this.TableName = tableName;
			this.RowCount = rowCount;
		}

	}

	public delegate void ScriptingStatusUpdateEventHandler(object sender, ScriptingStatusUpdateEventArgs e);
	public class ScriptingStatusUpdateEventArgs :EventArgs
	{
		public readonly int RowCount;
		public readonly string TableName;
		public readonly string Status;
		public ScriptingStatusUpdateEventArgs(string tableName, int rowCount, string status)
		{
			this.TableName = tableName;
			this.RowCount = rowCount;
			this.Status = status;
		}

	}


	public delegate void ScriptingCompletedEventHandler(object sender, ScriptingCompletedEventArgs e);
	public class ScriptingCompletedEventArgs : EventArgs
	{
		public readonly TableScriptData[] ScriptsCollection;
		public readonly System.Windows.Forms.TabPage[] TabPages;
		public ScriptingCompletedEventArgs(TableScriptData[] scriptsCollection, System.Windows.Forms.TabPage[] tabPages)
		{
			this.ScriptsCollection = scriptsCollection;
			this.TabPages = tabPages;
		}
	}
}
