using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
namespace SqlBuildManager.Interfaces.ScriptHandling.Policy
{
    public interface IScriptPolicy
    {
        /// <summary>
        /// Brief description of the policy
        /// </summary>
        string PolicyId { get; }
        /// <summary>
        /// Brief description of the policy
        /// </summary>
        string ShortDescription { get; }
        /// <summary>
        /// Detailed description of the policy
        /// </summary>
        string LongDescription  { get; }
        /// <summary>
        /// Executes the policy rule against the input script
        /// </summary>
        /// <param name="script">Input script</param>
        /// <param name="commentBlockMatches">The comment blocks for the incomming script</param>
        /// <param name="message">Details of why the script didn't pass the policy. Empty if the policy passed</param>
        /// <returns>True meaning script passed the policy check, False if it failed.</returns>
        bool CheckPolicy(string script, List<Match> commentBlockMatches, out string message);
        /// <summary>
        /// Determines as to whether or not this policy is turned on or not
        /// </summary>
        bool Enforce { get; set; }
        /// <summary>
        /// Determines how bad the violation is. 
        /// </summary>
        ViolationSeverity Severity { get; set; }
       
       
            
    }
}
