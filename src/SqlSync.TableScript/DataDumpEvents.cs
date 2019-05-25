using System;

namespace SqlSync.TableScript
{
	public class ProcessingTableDataEventArgs : EventArgs
	{
		public readonly string TableName;
		public readonly string Message;
		public readonly bool ProcessingComplete;
		public ProcessingTableDataEventArgs(string tableName,string message,bool processingComplete)
		{
			this.TableName = tableName;
			this.Message = message;
			this.ProcessingComplete = processingComplete;
		}
	}
	public class FileWrittenEventArgs : EventArgs
	{
		public readonly string FileName;
		public readonly long Size;
		public FileWrittenEventArgs(string fileName, long size)
		{
			this.FileName = fileName;
			this.Size = size;
		}
	}
}
