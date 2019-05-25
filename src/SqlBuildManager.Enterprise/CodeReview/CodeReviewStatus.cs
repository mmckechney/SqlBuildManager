using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlBuildManager.Enterprise.CodeReview
{
    public enum CodeReviewStatus : short
    {
        None = 0,
        Accepted = 1,
        Defect = 2,
        OutOfDate = 3
    }
}
