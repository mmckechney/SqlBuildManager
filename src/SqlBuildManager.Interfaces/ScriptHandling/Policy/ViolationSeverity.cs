namespace SqlBuildManager.Interfaces.ScriptHandling.Policy
{
    /// <summary>
    /// Keep in sync with SqlBuildManager.Enterprise EnterpriseConfiguration.xsd
    /// </summary>
    public enum ViolationSeverity
    {
        High = 1,
        Medium = 2,
        Low = 3,
        ReviewWarning = 4
    }
}
