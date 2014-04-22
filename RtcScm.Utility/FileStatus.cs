using System;
using System.Collections.Generic;
using System.Text;
using SqlBuildManager.Interfaces.SourceControl;
namespace RtcScm.Utility
{
    public class FileStatus : IFileStatus
    {
        public void Add(string fileName, SourceControlStatus status)
        {
            switch (status)
            {
                case SourceControlStatus.Added:
                    addedToSource.Add(fileName);
                    break;
                case SourceControlStatus.CheckedOut:
                    checkedOutFromSource.Add(fileName);
                    break;
                case SourceControlStatus.Error:
                default:
                    sourceError.Add(fileName);
                    break;
            }
        }
        private List<string> addedToSource = new List<string>();

        public List<string> AddedToSource
        {
            get { return addedToSource; }
            set { addedToSource = value; }
        }
        private List<string> checkedOutFromSource = new List<string>();

        public List<string> CheckedOutFromSource
        {
            get { return checkedOutFromSource; }
            set { checkedOutFromSource = value; }
        }
        private List<string> sourceError = new List<string>();

        public List<string> SourceError
        {
            get { return sourceError; }
            set { sourceError = value; }
        }
    }
}
