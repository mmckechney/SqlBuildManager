using System;
using System.Collections.Generic;
using System.Text;

namespace SqlBuildManager.Interfaces.SourceControl
{
    public enum SourceControlStatus
    {
        Added,
        CheckedOut,
        Error,
        NotUnderSourceControl,
        AlreadyPending, 
        Unknown
    }
}
