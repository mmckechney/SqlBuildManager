using System;

namespace SqlSync.ObjectScript
{
	#region ## Scripting Event Definition ##
	public delegate void DatabaseScriptEventHandler(object sender,DatabaseScriptEventArgs e); 
	public class DatabaseScriptEventArgs : EventArgs
	{
		public readonly string SourceFile;
		public readonly string Status;
		public readonly bool IsNew;
		public readonly string FullPath;
		public DatabaseScriptEventArgs(string sourceFile, string status,string fullPath,bool isNew)
		{
			this.SourceFile = sourceFile;
			this.Status = status;
			this.IsNew =isNew;
			this.FullPath = fullPath;
		}
	}
	#endregion

	#region ##Status Event Definition
	public delegate void StatusEventHandler(object sender, StatusEventArgs e);
	public class StatusEventArgs : EventArgs
	{
		public readonly string StatusMessage;
		public StatusEventArgs(string statusMessage)
		{
			this.StatusMessage = statusMessage;
		}
	}
	#endregion

}
