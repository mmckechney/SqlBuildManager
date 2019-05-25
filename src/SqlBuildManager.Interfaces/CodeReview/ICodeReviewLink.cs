using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlBuildManager.Interfaces.CodeReview
{
    interface ICodeReviewLink
    {
        int ReviewID
        {
            get;
            set;
        }
        string HostServer
        {
            get;
            set;
        }
        string DisplayUrlFormat
        {
            get;
            set;
        }
    }
}
