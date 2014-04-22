using System;
using System.Collections.Generic;
using System.Text;

namespace SqlSync.SqlBuild
{
    /// <summary>
    /// This is closely tied to the image index for the Icon in the SqlSync.SqlBuild.SqlBuildForm.
    /// Before updating ensure that they are aligned.
    /// </summary>
    public enum ScriptStatusType
    {
        NotRun = 0,
        Locked = 1,
        UpToDate = 2,
        ChangedSinceCommit = 3,
        ServerChange = 4,
        NotRunButOlderVersion = 5,
        FileMissing = 6,
        DocumentProtect = 7,
        PolicyNotRun = 8,
        PolicyPass = 9,
        PolicyFail = 10,
        PolicyWarning = 11,
        CodeReviewNotStarted = 12,
        CodeReviewInProgress = 13,
        CodeReviewAccepted = 14,
        CodeReviewAcceptedDba = 15,
        CodeReviewStatusWaiting = 16,
        Unknown = 99
    }

   
}
