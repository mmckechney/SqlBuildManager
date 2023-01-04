namespace SqlBuildManager.Interfaces.ScriptHandling.Policy
{
    /// <summary>
    /// Use this interface for policies that can have multiple instances loaded at a time.
    /// </summary>
    public interface IScriptPolicyMultiple : IScriptPolicyWithArguments
    {

        /// <summary>
        /// Dynamic Short description as read from the configuration file
        /// </summary>
        new string ShortDescription { get; set; }
        /// <summary>
        /// Dynamic Long description as read from the configuration file
        /// </summary>
        new string LongDescription { get; set; }
        /// <summary>
        /// Dynamic Error Message as read from the configuration file
        /// </summary>
        string ErrorMessage { get; set; }
    }
}
