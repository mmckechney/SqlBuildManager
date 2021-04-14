using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using p = SqlBuildManager.Interfaces.ScriptHandling.Policy;
using System.Linq;
using Microsoft.Extensions.Logging;
namespace SqlBuildManager.Enterprise.Policy
{
    class StoredProcParameterPolicy : p.IScriptPolicyMultiple
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #region IScriptPolicy Members
        public string PolicyId
        {
            get
            {
                return PolicyIdKey.StoredProcParameterPolicy;
            }
        }
        private  p.ViolationSeverity severity = p.ViolationSeverity.High;
        public p.ViolationSeverity Severity
        {
            get { return severity; }
            set { this.severity = value; }
        }
        public string ShortDescription
        {
            get
            {
                bool param, schema;
                string schemaValue, paramValue;
                GetSchemaAndParamValues(out param, out paramValue, out schema, out schemaValue);


                if (param && schema)
                {
                    return "Stored Proc Parameter - " + schemaValue + "/" + paramValue;
                }
                else if (param)
                {
                    return "Stored Proc Parameter - " + paramValue;
                }
                else
                {
                    return "Stored Proc Parameter";
                }
            }
            set { this.ShortDescription = value; }
        }
        public string ErrorMessage
        {
            get { return string.Empty; }
            set { }
        }
        private bool enforce = true;
        public bool Enforce
        {
            get { return this.enforce; }
            set { this.enforce = value; }
        }
        private void GetSchemaAndParamValues(out bool hasParameter, out string paramValue, out bool hasSchema, out string schemaValue)
        {
            var tmpHasParameter = (from a in this.arguments where a.Name == "Parameter" select true);
            hasParameter = (tmpHasParameter.Count() > 0);
 
            paramValue = string.Empty;
            if (hasParameter)
                paramValue = (from a in this.arguments where a.Name == "Parameter" select a.Value).First();

            var tmpHasSchema = (from a in this.arguments where a.Name == "Schema" select true);
            hasSchema = (tmpHasSchema.Count() > 0);
            
            schemaValue = string.Empty;
            if (hasSchema)
                schemaValue = (from a in this.arguments where a.Name == "Schema" select a.Value).First();
        }
        public string LongDescription
        {
            get { return "Makes certain that new stored procs have the proper default parameters"; }
            set { this.LongDescription = value; }
        }

        public bool CheckPolicy(string script, List<Match> commentBlockMatches, out string message)
        {
            return CheckPolicy(script, string.Empty, commentBlockMatches, out message);
        }
        public bool CheckPolicy(string script, string targetDatabase, List<Match> commentBlockMatches, out string message)
        {
            try
            {
                bool hasParam, hasSchema;
                string schemaValue, paramValue;
                GetSchemaAndParamValues(out hasParam, out paramValue, out hasSchema, out schemaValue);

                message = string.Empty;
                Regex regSchema = null;
                Regex regParameter = null;
                if (this.arguments.Count == 0)
                {
                    message = "Missing \"Schema\", \"Parameter\" arguments in setup. Please check your Enterprise configuration";
                    return false;
                }

                if (hasSchema)
                {
                    regSchema = new Regex(
                        String.Format(@"(procedure\s*\[{0})|(procedure\s*{0})", schemaValue), RegexOptions.IgnoreCase);
                }
                else
                {
                    message = "Missing \"Schema\" argument in setup. Please check your Enterprise configuration";
                    return false;

                }

                string paramVal = string.Empty;
                string paramReg = string.Empty;
                if (hasParam)
                {
                    paramVal = paramValue.Trim();
                }
                else
                {
                    message = "Missing \"Parameter\" argument in setup. Please check your Enterprise configuration";
                    return false;
                }

                bool hasType = false;
                var tmpHasType = (from a in this.arguments where a.Name == "SqlType" select true);
                if (tmpHasType.Count() > 0) hasType = true;

                string typeValue = string.Empty;
                if (hasType)
                    typeValue = (from a in this.arguments where a.Name == "SqlType" select a.Value).First();

                bool hasTarget = false;
                var tmpHasTarget = (from a in this.arguments where a.Name == "TargetDatabase" select true);
                if (tmpHasTarget.Count() > 0) hasTarget = true;
                string targetValue = string.Empty;
                if (hasTarget)
                    targetValue = (from a in this.arguments where a.Name == "TargetDatabase" select a.Value).First();


                if (paramVal.Length > 0 && hasType)
                {
                    paramReg = String.Format(@"({0}\s+{1}.*\bas\b)|({0}\s+\[{1}.*\bas\b)", paramVal, typeValue);
                }
                else
                    paramReg = paramVal + @".*\bas\b";

                if (paramReg.Length > 0)
                    regParameter = new Regex(paramReg, RegexOptions.IgnoreCase | RegexOptions.Singleline);

                //If a target database is set in the config and passed in, but the don't match, then this policy doesn't apply.
                if (hasTarget)
                    if (targetDatabase != null && targetDatabase.Length > 0 && targetDatabase.ToLower() != targetValue.ToLower())
                        return true;

                //If we get here, then we need to perform the policy check
                if (regSchema.Matches(script).Count > 0) //the procedure is in the controlled schema
                {
                    if (regParameter.Matches(script).Count == 0) //don't have the parameter? fail the check.
                    {
                        message = "The parameter \"" + paramValue + "\" is required for all procedures in the \"" + schemaValue + "\" schema.";
                        return false;

                    }
                }

                return true;
            } 
            catch (Exception exe)
            {
                message = "Error processing script policy. See application log file for details";
                log.Error(message, exe);
                return false;
            }
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


        #endregion
    }
}
