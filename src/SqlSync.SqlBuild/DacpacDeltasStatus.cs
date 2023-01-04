namespace SqlSync.SqlBuild
{
    public enum DacpacDeltasStatus
    {
        /// <summary>
        /// successfully determined the database differences from dacpac comparison
        /// </summary>
        Success,
        /// <summary>
        /// Failed to compare dacpacs
        /// </summary>
        Failure,
        /// <summary>
        /// Compared dacpacs and found no differences. Databases are already in sync
        /// </summary>
        InSync,
        /// <summary>
        /// Unable to extract dacpac from database
        /// </summary>
        ExtractionFailure,
        /// <summary>
        /// failure in the processing of the SBM package creation of deltas script
        /// </summary>
        SbmProcessingFailure,
        /// <summary>
        /// Status gathering in process
        /// </summary>
        Processing,
        /// <summary>
        /// Script contains only post-deployment steps and will not be used as a change source
        /// </summary>
        OnlyPostDeployment
    }
}
