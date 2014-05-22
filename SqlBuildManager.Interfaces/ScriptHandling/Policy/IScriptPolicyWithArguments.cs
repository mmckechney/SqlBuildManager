using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
namespace SqlBuildManager.Interfaces.ScriptHandling.Policy
{
    public interface IScriptPolicyWithArguments : IScriptPolicy
    {
        /// <summary>
        /// Additional arguments to be passed into the policy engine
        /// </summary>
        List<IScriptPolicyArgument> Arguments { get; set; }
        /// <summary>
        /// 
        /// </summary>
        new ViolationSeverity Severity { get; set; }
        /// <summary>
        /// Executes the policy rule against the input script
        /// </summary>
        /// <param name="script">Input script</param>
        /// <param name="targetDatabase">The target database for the script</param>
        /// <param name="message">Details of why the script didn't pass the policy. Empty if the policy passed</param>
        /// <returns>True meaning script passed the policy check, False if it failed.</returns>
        bool CheckPolicy(string script, string targetDatabase, List<Match> commentBlockMatches, out string message);
    }
}
