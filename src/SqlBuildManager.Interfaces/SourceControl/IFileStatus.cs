﻿using System.Collections.Generic;

namespace SqlBuildManager.Interfaces.SourceControl
{
    public interface IFileStatus
    {
        void Add(string fileName, SourceControlStatus status);


        List<string> AddedToSource
        {
            get;
            set;
        }

        List<string> CheckedOutFromSource
        {
            get;
            set;
        }
        List<string> SourceError
        {
            get;
            set;
        }
    }
}
