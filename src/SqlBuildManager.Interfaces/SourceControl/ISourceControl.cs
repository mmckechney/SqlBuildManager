using System.Collections.Generic;

namespace SqlBuildManager.Interfaces.SourceControl
{
    public interface ISourceControl
    {
        IFileStatus UpdateSourceControl(List<string> fileNames);

        SourceControlStatus UpdateSourceControl(string fileName);

        bool FileIsUnderSourceControl(string fileName);
    }
}
