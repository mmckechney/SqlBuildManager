using SqlSync.SqlBuild;
using SqlSync.SqlBuild.Objects;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using System.Xml.XPath;
using System.Xml.Xsl;
using p = SqlBuildManager.Interfaces.ScriptHandling.Policy;
namespace SqlBuildManager.Enterprise.Policy
{
    public class PolicyHelper
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public const string LineNumberToken = "{lineNumber}";
        internal static Dictionary<string, p.IScriptPolicy> allPolicies;
        internal static List<p.IScriptPolicy> activePolicies = null;
        static PolicyHelper()
        {
            allPolicies = new Dictionary<string, p.IScriptPolicy>();

            GrantExecutePolicy grant = new GrantExecutePolicy();
            allPolicies.Add(grant.PolicyId, grant);

            GrantExecuteToPublicPolicy pub = new GrantExecuteToPublicPolicy();
            allPolicies.Add(pub.PolicyId, pub);

            WithNoLockPolicy noLock = new WithNoLockPolicy();
            allPolicies.Add(noLock.PolicyId, noLock);

            ReRunablePolicy rerun = new ReRunablePolicy();
            allPolicies.Add(rerun.PolicyId, rerun);

            QualifiedNamesPolicy qual = new QualifiedNamesPolicy();
            allPolicies.Add(qual.PolicyId, qual);

            CommentHeaderPolicy comment = new CommentHeaderPolicy();
            allPolicies.Add(comment.PolicyId, comment);

            SelectStarPolicy selectStart = new SelectStarPolicy();
            allPolicies.Add(selectStart.PolicyId, selectStart);

            ViewAlterPolicy viewAlter = new ViewAlterPolicy();
            allPolicies.Add(viewAlter.PolicyId, viewAlter);

            ConstraintNamePolicy constraintName = new ConstraintNamePolicy();
            allPolicies.Add(constraintName.PolicyId, constraintName);

            ScriptSyntaxCheckPolicy syntax = new ScriptSyntaxCheckPolicy();
            syntax.Enforce = false;
            allPolicies.Add(syntax.PolicyId, syntax);

            StoredProcParameterPolicy spPolicy = new StoredProcParameterPolicy();
            spPolicy.Enforce = false;
            allPolicies.Add(spPolicy.PolicyId, spPolicy);

            ScriptSyntaxPairingCheckPolicy pairPolicy = new ScriptSyntaxPairingCheckPolicy();
            pairPolicy.Enforce = false;
            allPolicies.Add(pairPolicy.PolicyId, pairPolicy);

            //Initialize for first use
            GetPolicies();

        }
        public static List<p.IScriptPolicy> GetPolicies()
        {
            if (activePolicies != null && activePolicies.Count > 0)
                return activePolicies;

            activePolicies = new List<p.IScriptPolicy>();

            EnterpriseConfiguration cfg = EnterpriseConfigHelper.EnterpriseConfig;
            if (cfg.ScriptPolicy != null && cfg.ScriptPolicy.Length > 0)
            {
                foreach (ScriptPolicy policy in cfg.ScriptPolicy)
                {
                    if (!policy.Enforce)
                        continue;

                    if (allPolicies.ContainsKey(policy.PolicyId))
                    {
                        if ((allPolicies[policy.PolicyId] is p.IScriptPolicyMultiple)) //Create new instances for "Multiple" items...
                        {
                            p.IScriptPolicyMultiple tmpNew = (p.IScriptPolicyMultiple)Activator.CreateInstance(allPolicies[policy.PolicyId].GetType());
                            p.ViolationSeverity severity;
                            p.ViolationSeverity.TryParse(policy.Severity.ToString(), true, out severity);
                            tmpNew.Severity = severity;
                            if (policy.ScriptPolicyDescription != null)
                            {
                                if (policy.ScriptPolicyDescription.ErrorMessage.Length > 0)
                                    tmpNew.ErrorMessage = policy.ScriptPolicyDescription.ErrorMessage;
                                if (policy.ScriptPolicyDescription.LongDescription.Length > 0)
                                    tmpNew.LongDescription = policy.ScriptPolicyDescription.LongDescription;
                                if (policy.ScriptPolicyDescription.ShortDescription.Length > 0)
                                    tmpNew.ShortDescription = policy.ScriptPolicyDescription.ShortDescription;
                            }

                            tmpNew.Severity = (SqlBuildManager.Interfaces.ScriptHandling.Policy.ViolationSeverity)
                                Enum.Parse(typeof(SqlBuildManager.Interfaces.ScriptHandling.Policy.ViolationSeverity),policy.Severity.ToString());

                            if (policy.Argument != null)
                            {
                                foreach (ScriptPolicyArgument argument in policy.Argument)
                                {
                                    tmpNew.Arguments.Add(new p.IScriptPolicyArgument()
                                    { 
                                       Name = argument.Name, 
                                       Value = argument.Value, 
                                       IsGlobalException = argument.IsGlobalException, 
                                       IsLineException = argument.IsLineException, 
                                       FailureMessage = argument.FailureMessage,
                                    });
                                   
                                }
                            }
                            activePolicies.Add(tmpNew);

                        }
                        else if ((allPolicies[policy.PolicyId] is p.IScriptPolicyWithArguments) && policy.Argument != null) //Add arguments as needed
                        {

                            foreach (ScriptPolicyArgument argument in policy.Argument)
                            {
                                ((p.IScriptPolicyWithArguments)allPolicies[policy.PolicyId]).Arguments.Add(new p.IScriptPolicyArgument()
                                {
                                    Name = argument.Name,
                                    Value = argument.Value,
                                    IsGlobalException = argument.IsGlobalException,
                                    IsLineException = argument.IsLineException,
                                    FailureMessage = argument.FailureMessage
                                });
                            }

                            allPolicies[policy.PolicyId].Severity = (SqlBuildManager.Interfaces.ScriptHandling.Policy.ViolationSeverity)
                                Enum.Parse(typeof(SqlBuildManager.Interfaces.ScriptHandling.Policy.ViolationSeverity), policy.Severity.ToString());

                            activePolicies.Add(allPolicies[policy.PolicyId]);
                        }
                        else if (allPolicies[policy.PolicyId] is p.IScriptPolicy)
                        {
                            allPolicies[policy.PolicyId].Severity = (SqlBuildManager.Interfaces.ScriptHandling.Policy.ViolationSeverity)
                              Enum.Parse(typeof(SqlBuildManager.Interfaces.ScriptHandling.Policy.ViolationSeverity), policy.Severity.ToString());

                            activePolicies.Add(allPolicies[policy.PolicyId]);
                        }

                        
                    }
                    else
                    {
                        log.WarnFormat("Unable to load unknown ScriptPolicy \"{0}\"", policy.PolicyId);
                    }

                }
                log.DebugFormat("Loaded {0} script policy objects from EnterpriseConfiguration", activePolicies.Count.ToString());
            }
            else
            {
                log.Warn("No EnterpriseConfiguration settings found for ScriptPolicies. Loading all default policies");
                activePolicies.AddRange(allPolicies.Values);
            }
            
            //Get only those that are "turned on"
            var a = from p in activePolicies where p.Enforce == true select p;
            if(a.Count() > 0)
                activePolicies = a.ToList();

            log.DebugFormat("Loaded {0} script policy objects", activePolicies.Count.ToString());

            return activePolicies;
        }

        public Script ValidateScriptAgainstPolicies( string script)
        {
            return ValidateScriptAgainstPolicies(script, string.Empty);
        }
        public Script ValidateScriptAgainstPolicies(string scriptText, string targetDatabase)
        {
            return ValidateScriptAgainstPolicies(string.Empty, string.Empty, scriptText, targetDatabase, 10);
        }
        public Script ValidateScriptAgainstPolicies(string scriptName, string scriptGuid, string scriptText, string targetDatabase, int commentDayThreshold)
        {
            Script violations = new Script(scriptName, scriptGuid);
            List<p.IScriptPolicy> policies = GetPolicies();

            foreach (p.IScriptPolicy policy in policies)
            {
                if (policy is CommentHeaderPolicy)
                    ((CommentHeaderPolicy)policy).DayThreshold = commentDayThreshold;

                Violation tmp = ValidateScriptAgainstPolicy(scriptText, targetDatabase, policy);
                if (tmp != null)
                    violations.AddViolation(tmp);
            }

            if (violations.Count > 0)
                return violations;
            else
                return null;

        }
        public static Violation ValidateScriptAgainstPolicy(string script, string targetDatabase, p.IScriptPolicy policy)
        {
            string message;
            List<Match> commentBlockMatches = ScriptHandling.ScriptHandlingHelper.GetScriptCommentBlocks(script);
            if (policy is p.IScriptPolicyWithArguments && policy.Enforce)
            {
                if (!((p.IScriptPolicyWithArguments)policy).CheckPolicy(script, targetDatabase,commentBlockMatches,  out message))
                    return new Violation(policy.ShortDescription, message,Enum.GetName(typeof(p.ViolationSeverity),policy.Severity));
            }
            else if (policy.Enforce)
            {
                if (!policy.CheckPolicy(script, commentBlockMatches, out message))
                    return new Violation(policy.ShortDescription, message,Enum.GetName(typeof(p.ViolationSeverity),policy.Severity));
            }
            return null;
        }
        public Package ValidateScriptsAgainstPolicies(List<UpdatedObject> namesAndScripts)
        {
            List<KeyValuePair<string,string>> tmpLst = new List<KeyValuePair<string,string>>();
            foreach (UpdatedObject obj in namesAndScripts)
                tmpLst.Add(new KeyValuePair<string, string>(obj.ScriptName, obj.ScriptContents));

            return ValidateScriptsAgainstPolicies(tmpLst);

        }
        public Package ValidateScriptsAgainstPolicies(List<KeyValuePair<string, string>> namesAndScripts)
        {
            Package lstViolations = new Package();
            foreach (KeyValuePair<string,string> pair in namesAndScripts)
            {
                Script violations = ValidateScriptAgainstPolicies(pair.Value);
                if (violations != null)
                {
                    violations.ScriptName = pair.Key;
                    lstViolations.Add(violations);
                }

            }
            if (lstViolations.Count > 0)
                return lstViolations;
            else
                return null;

        }
        public Script ValidateFileAgainstPolicies(string fileName)
        {
          
            if (File.Exists(fileName))
            {
                string script = File.ReadAllText(fileName);
                Script violation = ValidateScriptAgainstPolicies(script);
                if (violation != null)
                {
                    violation.ScriptName = Path.GetFileName(fileName);
                    return violation;
                }
            }

            return null;
        }
        public Package ValidateFilesAgainstPolicies(List<string> fileNames)
        {
            Package lstViolations = new Package();
            foreach (string fileName in fileNames)
            {
                Script violations = ValidateFileAgainstPolicies(fileName);
                if (violations != null)
                    lstViolations.Add(violations);

            }

            if (lstViolations.Count > 0)
                return lstViolations;
            else
                return null;
        }


        public Package CreateScriptPolicyPackage(SqlSyncBuildData buildData, string extractedProjectPath)
        {
            Package scriptPackage = new Package();
            Script scriptItem;
            foreach (SqlSyncBuildData.ScriptRow row in buildData.Script)
            {
                try
                {
                    string scriptContents = File.ReadAllText(extractedProjectPath + row.FileName);
                    scriptItem = this.ValidateScriptAgainstPolicies(row.FileName, row.ScriptId, scriptContents, row.Database, 80);
                    if (scriptItem == null)
                    {
                        row.PolicyCheckState = ScriptStatusType.PolicyPass;
                    }
                    else
                    {
                        scriptItem.LastChangeDate = (row.DateModified == DateTime.MinValue) ? row.DateAdded.ToString() : row.DateModified.ToString();
                        scriptItem.LastChangeUserId = (row.ModifiedBy.Length == 0) ? row.AddedBy : row.ModifiedBy;
                        scriptPackage.Add(scriptItem);
                        row.PolicyCheckState = ScriptStatusType.PolicyFail;
                    }
                }
                catch (Exception exe)
                {
                    log.Error(String.Format("Unable to read file '{0}' for policy check validation", extractedProjectPath + row.FileName), exe);
                }
            }
            return scriptPackage;

        }
        public static string TransformViolationstoXml(Package currentViolations)
        {
            try
            {
                if (currentViolations != null && currentViolations.Count > 0)
                {
                    XmlSerializer xmlS = new XmlSerializer(typeof(Package));
                    StringBuilder sb = new StringBuilder();
                    StringWriter sw = new StringWriter(sb);
                    xmlS.Serialize(sw, currentViolations);

                    return sb.ToString();
                }

                log.Info("No violations to serialize");
                return string.Empty;
            }
            catch (Exception exe)
            {
                log.Error("Unable to serialize violations", exe);
                return string.Empty;
            }
                
            
        }
        public static bool TransformViolationstoCsv(string fileName, Package currentViolations)
        {
            try
            {
                string xsltContents = SqlBuildManager.Enterprise.Properties.Resources.PolicyViolation_CSV;

                if (currentViolations != null && currentViolations.Count > 0)
                {
                    StringBuilder sb = new StringBuilder(TransformViolationstoXml(currentViolations));
                    sb.Replace(',', ';');

                    XmlTextReader xsltReader;

                    XslCompiledTransform trans = new XslCompiledTransform();
                    StringReader srContents = new StringReader(sb.ToString());
                    StringReader xsltText;
                    XmlTextReader xmlReader = new XmlTextReader(srContents);
                    XPathDocument xPathDoc = new XPathDocument(xmlReader);

                    StringBuilder sbCSV = new StringBuilder();
                    StringWriter swCSV = new StringWriter(sbCSV);
                    xsltText = new StringReader(xsltContents);
                    xsltReader = new XmlTextReader(xsltText);
                    trans.Load(xsltReader);
                    trans.Transform(xPathDoc, null, swCSV);

                    File.WriteAllText(fileName, sbCSV.ToString());
                }
                return true;
            }
            catch (Exception exe)
            {
                log.Error("Error saving violations", exe);
                return false;
            }
        }
        internal static int GetLineNumber(string fullScript, int targetIndex)
        {
            Regex lineCounter = new Regex("\n");
            MatchCollection cnt = lineCounter.Matches(fullScript);
            int line = 0;
            for (int z = 0; z < cnt.Count; z++)
            {
                if (cnt[z].Index > targetIndex)
                {
                    line = z + 1;
                    break;
                }
            }
            if (line == 0) line++;
            return line;
        }

        public List<string> CommandLinePolicyCheck(string buildPackageName, out bool passed)
        {
            string highSeverity = ViolationSeverity.High.ToString();
            passed = true;
            List<string> policyReturns = new List<string>();
            SqlSyncBuildData buildData = null;

            if (String.IsNullOrEmpty(buildPackageName))
                return policyReturns;

            string projFileName = string.Empty;
            string projectFilePath = string.Empty;
            string workingDirectory = string.Empty;

            string extension = Path.GetExtension(buildPackageName).ToLower();
            switch ((extension))
            {
                case ".sbm":
                    string result;
                    SqlBuildFileHelper.ExtractSqlBuildZipFile(buildPackageName, ref workingDirectory, ref projectFilePath,
                                           ref projFileName,
                                           out result);
                    SqlBuildFileHelper.LoadSqlBuildProjectFile(out buildData, projFileName, false);
                    break;
                case ".sbx":
                    projectFilePath = Path.GetDirectoryName(buildPackageName);
                    SqlBuildFileHelper.LoadSqlBuildProjectFile(out buildData, buildPackageName, false);
                    break;
                default:
                    return policyReturns;
                    break;
            }

            if (buildData != null)
            {
                Package pkg = CreateScriptPolicyPackage(buildData, projectFilePath);
                passed = !pkg.Select(p => p.Violations.Select(v => v.Severity == highSeverity)).Any();

                var violationMessages = from s in pkg
                                     from v in s.Violations
                                        select new {v.Severity, s.ScriptName, v.Message}; 

                foreach (var violation in violationMessages)
                {
                    string message = string.Format("Severity [{0}]; Script: {1}; Message: {2}",violation.Severity, violation.ScriptName, violation.Message);
                    policyReturns.Add(message);

                    //Add messages to log
                    if(violation.Severity == ViolationSeverity.High.ToString())
                        log.Error(message);   
                    else if(violation.Severity == ViolationSeverity.Medium.ToString())
                        log.Warn(message);
                    else 
                        log.Info(message);
                }
                return policyReturns;   
            }
            else
            {
                return policyReturns;
            }
        }

    }
}
