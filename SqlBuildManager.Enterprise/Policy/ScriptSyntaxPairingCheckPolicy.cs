using System;
using System.Collections.Generic;
using System.Text;
using p = SqlBuildManager.Interfaces.ScriptHandling.Policy;
using System.Text.RegularExpressions;
using System.Linq;
using SqlBuildManager.ScriptHandling;
namespace SqlBuildManager.Enterprise.Policy
{
    class ScriptSyntaxPairingCheckPolicy : p.IScriptPolicyMultiple
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
        public p.ViolationSeverity Severity { get; set; }
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
            get { return this.enforce; }
            set { this.enforce = value; }
        }
        public bool CheckPolicy(string script, List<Match> commentBlockMatches, out string message)
        {
            return CheckPolicy(script, string.Empty, commentBlockMatches, out message);
        }

        #endregion

        #region IScriptPolicyWithArguments Members
        private List<p.IScriptPolicyArgument> arguments = new List<p.IScriptPolicyArgument>();
        public List<p.IScriptPolicyArgument> Arguments
        {
            get
            {
                return this.arguments;
            }
            set
            {
                this.arguments = value;
            }
        }



        public bool CheckPolicy(string script, string targetDatabase, List<Match> commentBlockMatches, out string message)
        {
            try
            {

                string parentRegex = string.Empty;
                string policyTargetDatabase = string.Empty;
                List<p.IScriptPolicyArgument> childPairRegexList = new List<p.IScriptPolicyArgument>();
                List<p.IScriptPolicyArgument> childDontPairRegexList = new List<p.IScriptPolicyArgument>();
                //Parse out the arguments
                foreach (p.IScriptPolicyArgument argument in this.arguments)
                {
                    switch (argument.Name)
                    {
                        case parentRegexKey:
                            if (parentRegex.Length > 0)
                            {
                                log.WarnFormat("The ScriptSyntaxPairingCheckPolicy \"{0}\" already has a {3} value of \"{1}\". This is being overwritten by a value of \"{2}\"", this.ShortDescription, parentRegex, argument.Value, ScriptSyntaxPairingCheckPolicy.parentRegexKey);
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
                            log.WarnFormat("The ScriptSyntaxPairingCheckPolicy \"{0}\" has an unrecognized argument. Name: {1}; Value:{2}", this.ShortDescription, argument.Name, argument.Value);
                            break;
                    }
                }

                //Check to make sure we have some values set
                if (parentRegex.Length == 0)
                {
                    message = String.Format("The ScriptSyntaxPairingCheckPolicy \"{0}\" does not have a {1} argument/value. Unable to process policy.", this.ShortDescription, ScriptSyntaxPairingCheckPolicy.parentRegexKey);
                    log.WarnFormat(message);
                    return true;
                }

                if (childDontPairRegexList.Count == 0 && childPairRegexList.Count == 0)
                {
                    message = String.Format("The ScriptSyntaxPairingCheckPolicy \"{0}\" does not have any {1} or {2} argument/values. Unable to process policy.", this.ShortDescription, ScriptSyntaxPairingCheckPolicy.childPairRegexKey, ScriptSyntaxPairingCheckPolicy.childDontPairRegexKey);
                    log.WarnFormat(message);
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
                foreach (p.IScriptPolicyArgument child in childPairRegexList)
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
                foreach (p.IScriptPolicyArgument child in childDontPairRegexList)
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
                message = String.Format("Error processing script policy {0}. See application log file for details", this.ShortDescription);
                log.Error(message, exe);
                return false;
            }
        }

        #endregion
    }
}
