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
