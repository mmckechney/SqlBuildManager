using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlBuildManager.Interfaces.SourceControl
{
    public interface ISourceControl
    {
        IFileStatus UpdateSourceControl(List<string> fileNames);

        SourceControlStatus UpdateSourceControl(string fileName);

        bool FileIsUnderSourceControl(string fileName);
    }
}
