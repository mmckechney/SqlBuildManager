using Microsoft.Extensions.Logging;
using SqlBuildManager.ScriptHandling;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using shP = SqlBuildManager.Interfaces.ScriptHandling.Policy;
namespace SqlBuildManager.Enterprise.Policy
{
    class ScriptSyntaxPairingCheckPolicy : shP.IScriptPolicyMultiple
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public const string parentRegexKey = "ParentRegex";
        public const string childPairRegexKey = "ChildPairRegex";
        public const string childDontPairRegexKey = "ChildDontPairRegex";
        public const string targetDatabaseKey = "TargetDatabase";
        #region IScriptPolicy Members
        public string PolicyId
        {
            get
            {
                return PolicyIdKey.ScriptSyntaxPairingCheckPolicy;
            }
        }
        private shP.ViolationSeverity severity = shP.ViolationSeverity.High;
        public shP.ViolationSeverity Severity
        {
            get { return severity; }
            set { severity = value; }
        }
        public string ShortDescription
        {
            get;
            set;
        }

        public string LongDescription
        {
            get;
            set;
        }

        public string ErrorMessage
        {
            get;
            set;
        }
        private bool enforce = true;
        public bool Enforce
        {
            get { return enforce; }
            set { enforce = value; }
        }
        public bool CheckPolicy(string script, List<Match> commentBlockMatches, out string message)
        {
            return CheckPolicy(script, string.Empty, commentBlockMatches, out message);
        }

        #endregion

        #region IScriptPolicyWithArguments Members
        private List<shP.IScriptPolicyArgument> arguments = new List<shP.IScriptPolicyArgument>();
        public List<shP.IScriptPolicyArgument> Arguments
        {
            get
            {
                return arguments;
            }
            set
            {
                arguments = value;
            }
        }



        public bool CheckPolicy(string script, string targetDatabase, List<Match> commentBlockMatches, out string message)
        {
            try
            {

                string parentRegex = string.Empty;
                string policyTargetDatabase = string.Empty;
                List<shP.IScriptPolicyArgument> childPairRegexList = new List<shP.IScriptPolicyArgument>();
                List<shP.IScriptPolicyArgument> childDontPairRegexList = new List<shP.IScriptPolicyArgument>();
                //Parse out the arguments
                foreach (shP.IScriptPolicyArgument argument in arguments)
                {
                    switch (argument.Name)
                    {
                        case parentRegexKey:
                            if (parentRegex.Length > 0)
                            {
                                log.LogWarning($"The ScriptSyntaxPairingCheckPolicy \"{ShortDescription}\" already has a {ScriptSyntaxPairingCheckPolicy.parentRegexKey} value of \"{parentRegex}\". This is being overwritten by a value of \"{argument.Value}\"");
                            }
                            parentRegex = argument.Value;
                            break;
                        case childPairRegexKey:
                            childPairRegexList.Add(argument);
                            break;
                        case childDontPairRegexKey:
                            childDontPairRegexList.Add(argument);
                            break;
                        case targetDatabaseKey:
                            policyTargetDatabase = argument.Value;
                            break;
                        default:
                            log.LogWarning($"The ScriptSyntaxPairingCheckPolicy \"{ShortDescription}\" has an unrecognized argument. Name: {argument.Name}; Value:{argument.Value}");
                            break;
                    }
                }

                //Check to make sure we have some values set
                if (parentRegex.Length == 0)
                {
                    message = String.Format("The ScriptSyntaxPairingCheckPolicy \"{0}\" does not have a {1} argument/value. Unable to process policy.", ShortDescription, ScriptSyntaxPairingCheckPolicy.parentRegexKey);
                    log.LogWarning(message);
                    return true;
                }

                if (childDontPairRegexList.Count == 0 && childPairRegexList.Count == 0)
                {
                    message = String.Format("The ScriptSyntaxPairingCheckPolicy \"{0}\" does not have any {1} or {2} argument/values. Unable to process policy.", ShortDescription, ScriptSyntaxPairingCheckPolicy.childPairRegexKey, ScriptSyntaxPairingCheckPolicy.childDontPairRegexKey);
                    log.LogWarning(message);
                    return true;
                }

                //Does this policy apply to the current script?
                if (policyTargetDatabase.Trim().Length > 0 && targetDatabase.ToLower().Trim() != policyTargetDatabase.ToLower().Trim())
                {
                    message = String.Format("Script target database {0} does not match policy target database {1}. Policy passes.", targetDatabase, policyTargetDatabase);
                    return true;
                }

                //Match the parent regex
                Regex parentCheck = new Regex(parentRegex, RegexOptions.IgnoreCase);
                MatchCollection syntaxMatches = parentCheck.Matches(script);
                //No matches? Nevermind then...
                if (syntaxMatches.Count == 0)
                {
                    message = "No parent match found. Policy passes.";
                    return true;
                }

                //make sure at least one of the parent matches is not in a comment
                bool foundParent = false;
                foreach (Match parentMatch in syntaxMatches)
                {
                    if (ScriptHandlingHelper.IsInComment(parentMatch.Index, commentBlockMatches))
                    {
                        continue;
                    }
                    else
                    {
                        foundParent = true;
                    }
                }
                if (!foundParent)
                {
                    message = "No uncommented parent match found. Policy passes.";
                    return true;
                }

                //If we have a match.. let's make sure we have the child matches
                foreach (shP.IScriptPolicyArgument child in childPairRegexList)
                {
                    Regex childPair = new Regex(child.Value, RegexOptions.IgnoreCase);
                    MatchCollection childPairMatches = childPair.Matches(script);
                    if (childPairMatches.Count == 0)
                    {
                        if (child.FailureMessage.Length > 0)
                            message = child.FailureMessage;
                        else
                            message = String.Format("No child pair found with regular expression {0}", child.Value);

                        return false;
                    }
                    else
                    {
                        //If matches are found, make sure they are not in comments.
                        bool foundUncommentedChild = false;
                        foreach (Match cM in childPairMatches)
                        {
                            if (!ScriptHandlingHelper.IsInComment(cM.Index, commentBlockMatches))
                                foundUncommentedChild = true;
                        }
                        if (!foundUncommentedChild)
                        {
                            if (child.FailureMessage.Length > 0)
                                message = child.FailureMessage;
                            else
                                message = String.Format("No child pair found with regular expression {0}", child.Value);

                            return false;
                        }
                    }

                }

                //If we have a match.. let's make sure we have the child matches
                foreach (shP.IScriptPolicyArgument child in childDontPairRegexList)
                {
                    Regex childPair = new Regex(child.Value, RegexOptions.IgnoreCase);
                    MatchCollection childPairMatches = childPair.Matches(script);
                    if (childPairMatches.Count > 0)
                    {
                        //If matches are found, make sure they are not in comments.
                        bool hadUnCommentedChild = true;
                        foreach (Match cM in childPairMatches)
                        {
                            if (!ScriptHandlingHelper.IsInComment(cM.Index, commentBlockMatches))
                                hadUnCommentedChild = true;
                        }
                        if (hadUnCommentedChild)
                        {
                            if (child.FailureMessage.Length > 0)
                                message = child.FailureMessage;
                            else
                                message = String.Format("A child match was found for regular expression \"{0}\". This script pairing is not allowed.", child.Value);

                            return false;
                        }

                    }

                }

                message = string.Empty;
                return true;

            }
            catch (Exception exe)
            {
                message = String.Format("Error processing script policy {0}. See application log file for details", ShortDescription);
                log.LogError(exe, message);
                return false;
            }
        }

        #endregion
    }
}
