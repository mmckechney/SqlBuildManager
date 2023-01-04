using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Enterprise.Policy;
using SqlBuildManager.Interfaces.ScriptHandling.Policy;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
namespace SqlBuildManager.Enterprise.UnitTest
{


    /// <summary>
    ///This is a test class for PolicyHelperTest and is intended
    ///to contain all PolicyHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class PolicyHelperTest
    {
        [TestInitialize()]
        public void ClearPolicyList()
        {
            PolicyHelper.activePolicies = null;
        }

        [TestMethod()]
        public void PolicyHelper_AllPoliciesCollectionTest()
        {

            Dictionary<string, IScriptPolicy> actual = PolicyHelper.allPolicies;
            Assert.IsTrue(actual.Count == 12, "Expected 11 policies and got " + actual.Count.ToString());
            Assert.IsNotNull(actual["GrantExecutePolicy"], "Expected GrantExecutePolicy, got null");
            Assert.IsTrue(actual["GrantExecutePolicy"] is GrantExecutePolicy, "Expected GrantExecutePolicy got " + actual["GrantExecutePolicy"].GetType().ToString());

            Assert.IsNotNull(actual["GrantExecuteToPublicPolicy"], "Expected GrantExecuteToPublicPolicy, got null");
            Assert.IsTrue(actual["GrantExecuteToPublicPolicy"] is GrantExecuteToPublicPolicy, "Expected GrantExecuteToPublicPolicy got " + actual["GrantExecuteToPublicPolicy"].GetType().ToString());

            Assert.IsNotNull(actual["WithNoLockPolicy"], "Expected WithNoLockPolicy, got null");
            Assert.IsTrue(actual["WithNoLockPolicy"] is WithNoLockPolicy, "Expected WithNoLockPolicy got " + actual["WithNoLockPolicy"].GetType().ToString());

            Assert.IsNotNull(actual["ReRunablePolicy"], "Expected ReRunablePolicy, got null");
            Assert.IsTrue(actual["ReRunablePolicy"] is ReRunablePolicy, "Expected ReRunablePolicy got " + actual["ReRunablePolicy"].GetType().ToString());

            Assert.IsNotNull(actual["QualifiedNamesPolicy"], "Expected QualifiedNamesPolicy, got null");
            Assert.IsTrue(actual["QualifiedNamesPolicy"] is QualifiedNamesPolicy, "Expected QualifiedNamesPolicy got " + actual["QualifiedNamesPolicy"].GetType().ToString());

            Assert.IsNotNull(actual["CommentHeaderPolicy"], "Expected CommentHeaderPolicy, got null");
            Assert.IsTrue(actual["CommentHeaderPolicy"] is CommentHeaderPolicy, "Expected CommentHeaderPolicy got " + actual["CommentHeaderPolicy"].GetType().ToString());

            Assert.IsNotNull(actual["SelectStarPolicy"], "Expected SelectStarPolicy, got null");
            Assert.IsTrue(actual["SelectStarPolicy"] is SelectStarPolicy, "Expected SelectStarPolicy got " + actual["SelectStarPolicy"].GetType().ToString());

            Assert.IsNotNull(actual["ViewAlterPolicy"], "Expected ViewAlterPolicy, got null");
            Assert.IsTrue(actual["ViewAlterPolicy"] is ViewAlterPolicy, "Expected ViewAlterPolicy got " + actual["ViewAlterPolicy"].GetType().ToString());

            Assert.IsNotNull(actual["ConstraintNamePolicy"], "Expected ConstraintNamePolicy, got null");
            Assert.IsTrue(actual["ConstraintNamePolicy"] is ConstraintNamePolicy, "Expected ConstraintNamePolicy got " + actual["ConstraintNamePolicy"].GetType().ToString());

            Assert.IsNotNull(actual["ScriptSyntaxCheckPolicy"], "Expected ScriptSyntaxCheckPolicy, got null");
            Assert.IsTrue(actual["ScriptSyntaxCheckPolicy"] is ScriptSyntaxCheckPolicy, "Expected ScriptSyntaxCheckPolicy got " + actual["ScriptSyntaxCheckPolicy"].GetType().ToString());

            Assert.IsNotNull(actual["StoredProcParameterPolicy"], "Expected StoredProcParameterPolicy, got null");
            Assert.IsTrue(actual["StoredProcParameterPolicy"] is StoredProcParameterPolicy, "Expected StoredProcParameterPolicy got " + actual["StoredProcParameterPolicy"].GetType().ToString());

            Assert.IsNotNull(actual["ScriptSyntaxPairingCheckPolicy"], "Expected ScriptSyntaxPairingCheckPolicy, got null");
            Assert.IsTrue(actual["ScriptSyntaxPairingCheckPolicy"] is ScriptSyntaxPairingCheckPolicy, "Expected ScriptSyntaxPairingCheckPolicy got " + actual["ScriptSyntaxPairingCheckPolicy"].GetType().ToString());



        }
        /// <summary>
        ///A test for GetPolicies
        ///</summary>
        [TestMethod()]
        public void GetPoliciesTest_GetStandardIScriptPolicy()
        {
            EnterpriseConfigHelper.EnterpriseConfig = new EnterpriseConfiguration();
            List<IScriptPolicy> actual;
            actual = PolicyHelper.GetPolicies();
            Assert.IsTrue(actual.Count == 9, "Expected 9 policies and got " + actual.Count.ToString());
            Assert.IsTrue(actual[0] is GrantExecutePolicy, "Expected GrantExecutePolicy got " + actual[0].GetType().ToString());
            Assert.IsTrue(actual[1] is GrantExecuteToPublicPolicy, "Expected GrantExecuteToPublicPolicy got " + actual[0].GetType().ToString());
            Assert.IsTrue(actual[2] is WithNoLockPolicy, "Expected WithNoLockPolicy got " + actual[0].GetType().ToString());
            Assert.IsTrue(actual[3] is ReRunablePolicy, "Expected ReRunablePolicy got " + actual[0].GetType().ToString());
            Assert.IsTrue(actual[4] is QualifiedNamesPolicy, "Expected QualifiedNamesPolicy got " + actual[0].GetType().ToString());
            Assert.IsTrue(actual[5] is CommentHeaderPolicy, "Expected CommentHeaderPolicy got " + actual[0].GetType().ToString());
            Assert.IsTrue(actual[6] is SelectStarPolicy, "Expected SelectStarPolicy got " + actual[0].GetType().ToString());
            Assert.IsTrue(actual[7] is ViewAlterPolicy, "Expected ViewAlterPolicy got " + actual[0].GetType().ToString());
            Assert.IsTrue(actual[8] is ConstraintNamePolicy, "Expected ConstraintNamePolicy got " + actual[0].GetType().ToString());


        }

        [TestMethod()]
        public void GetPoliciesTest_AddDynamicIScriptPolicies()
        {
            EnterpriseConfiguration cfg = new EnterpriseConfiguration();

            ScriptPolicy pol1 = new ScriptPolicy();
            pol1.PolicyId = PolicyIdKey.WithNoLockPolicy;
            pol1.Enforce = true;

            ScriptPolicy pol2 = new ScriptPolicy();
            pol2.PolicyId = PolicyIdKey.GrantExecutePolicy;
            pol2.Enforce = true;

            cfg.ScriptPolicy = new ScriptPolicy[] { pol1, pol2 };
            EnterpriseConfigHelper.EnterpriseConfig = cfg;

            List<IScriptPolicy> actual;
            actual = PolicyHelper.GetPolicies();

            Assert.IsTrue(actual.Count == 2, "Expected 2 policies but got " + actual.Count.ToString());
            Assert.IsTrue(actual[0] is WithNoLockPolicy);
            Assert.IsTrue(actual[1] is GrantExecutePolicy);


        }

        [TestMethod()]
        public void GetPoliciesTest_AddDynamicIScriptPoliciesOneNotEnforced()
        {
            EnterpriseConfiguration cfg = new EnterpriseConfiguration();

            ScriptPolicy pol1 = new ScriptPolicy();
            pol1.PolicyId = PolicyIdKey.WithNoLockPolicy;
            pol1.Enforce = false;

            ScriptPolicy pol2 = new ScriptPolicy();
            pol2.PolicyId = PolicyIdKey.GrantExecutePolicy;
            pol2.Enforce = true;

            cfg.ScriptPolicy = new ScriptPolicy[] { pol1, pol2 };
            EnterpriseConfigHelper.EnterpriseConfig = cfg;

            List<IScriptPolicy> actual;
            actual = PolicyHelper.GetPolicies();

            Assert.IsTrue(actual.Count == 1, "Expected 1 policy but got " + actual.Count.ToString());
            Assert.IsTrue(actual[0] is GrantExecutePolicy);


        }
        [TestMethod()]
        public void GetPoliciesTest_AddDynamicStoredProcParamPolicies()
        {
            EnterpriseConfiguration cfg = new EnterpriseConfiguration();

            ScriptPolicyArgument arg = new ScriptPolicyArgument();
            arg.Name = "Schema";
            arg.Value = "dbo";

            ScriptPolicy pol1 = new ScriptPolicy();
            pol1.PolicyId = PolicyIdKey.StoredProcParameterPolicy;
            pol1.Enforce = true;
            pol1.Argument = new ScriptPolicyArgument[] { arg };

            ScriptPolicy pol2 = new ScriptPolicy();
            pol2.PolicyId = PolicyIdKey.StoredProcParameterPolicy;
            pol2.Enforce = true;
            pol2.Argument = new ScriptPolicyArgument[] { arg };

            cfg.ScriptPolicy = new ScriptPolicy[] { pol1, pol2 };
            EnterpriseConfigHelper.EnterpriseConfig = cfg;

            List<IScriptPolicy> actual;
            actual = PolicyHelper.GetPolicies();

            Assert.IsTrue(actual.Count == 2, "Expected 2 policies but got " + actual.Count.ToString());
            Assert.IsTrue(actual[0] is StoredProcParameterPolicy);
            Assert.IsTrue(actual[1] is StoredProcParameterPolicy);


        }

        [TestMethod]
        public void ValidateScriptAgainstPoliciesTest_NoViolations()
        {
            EnterpriseConfigHelper.EnterpriseConfig = null;
            string script = Properties.Resources.PolicyHelper_NoViolations;
            script = script.Replace("<<date>>", DateTime.Now.ToString("MM/dd/yyyy"));
            Script actual;

            actual = new PolicyHelper().ValidateScriptAgainstPolicies(script);
            Assert.IsNull(actual, "ScriptViolations object should be null for scripts with no violations.");
        }

        [TestMethod]
        public void ValidateScriptAgainstPoliciesTest_WithViolations()
        {
            PolicyHelper.activePolicies = (from p in PolicyHelper.allPolicies select p.Value).ToList();

            string script = Properties.Resources.PolicyHelper_WithViolations;
            Script actual;

            actual = new PolicyHelper().ValidateScriptAgainstPolicies(script);
            Assert.IsNotNull(actual, "ScriptViolations object should not be null for scripts with violations");

            Assert.AreEqual(6, actual.Count);
            Assert.AreEqual("Check for GRANT .. TO [public]", actual[0].Name);
            Assert.AreEqual("Script contains a GRANT statement to the [public] group", actual[0].Message);
            Assert.AreEqual("Check for Comments", actual[4].Name);
        }

        [TestMethod]
        public void ValidateScriptsAgainstPoliciesTest_NoViolations()
        {
            EnterpriseConfigHelper.EnterpriseConfig = null;
            string script = Properties.Resources.PolicyHelper_NoViolations;
            script = script.Replace("<<date>>", DateTime.Now.ToString("MM/dd/yyyy"));
            List<KeyValuePair<string, string>> scripts = new List<KeyValuePair<string, string>>();
            scripts.Add(new KeyValuePair<string, string>("TestScript", script));
            Package actual;

            actual = new PolicyHelper().ValidateScriptsAgainstPolicies(scripts);
            Assert.IsNull(actual, "ScriptViolations object should be null for scripts with no violations.");
        }

        [TestMethod]
        public void ValidateScriptsAgainstPoliciesTest_WithViolations()
        {
            PolicyHelper.activePolicies = (from p in PolicyHelper.allPolicies select p.Value).ToList();

            string script = Properties.Resources.PolicyHelper_WithViolations;
            List<KeyValuePair<string, string>> scripts = new List<KeyValuePair<string, string>>();
            scripts.Add(new KeyValuePair<string, string>("TestScript", script));
            Package actual;

            actual = new PolicyHelper().ValidateScriptsAgainstPolicies(scripts);
            Assert.IsNotNull(actual, "ScriptViolations object should not be null for scripts with violations");
            Assert.AreEqual(1, actual.Count);
            Assert.AreEqual("TestScript", actual[0].ScriptName);
            Assert.AreEqual(6, actual[0].Count);
            Assert.AreEqual("Check for Comments", actual[0][4].Name);
        }

        [TestMethod]
        public void ValidateFileAgainstPolicies_NoViolations()
        {
            EnterpriseConfigHelper.EnterpriseConfig = null;
            string script = Properties.Resources.PolicyHelper_NoViolations;
            script = script.Replace("<<date>>", DateTime.Now.ToString("MM/dd/yyyy"));
            string fileName = string.Empty;
            Script actual;

            try
            {
                fileName = Path.GetTempFileName();
                File.WriteAllText(fileName, script);

                actual = new PolicyHelper().ValidateFileAgainstPolicies(fileName);
                Assert.IsNull(actual, "ScriptViolations object should be null for scripts with no violations.");
            }
            finally
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }
        }

        [TestMethod]
        public void ValidateFileAgainstPolicies_WithViolations()
        {
            PolicyHelper.activePolicies = (from p in PolicyHelper.allPolicies select p.Value).ToList();

            string script = Properties.Resources.PolicyHelper_WithViolations;
            string fileName = string.Empty;
            Script actual;

            try
            {
                fileName = Path.GetTempFileName();
                File.WriteAllText(fileName, script);

                actual = new PolicyHelper().ValidateFileAgainstPolicies(fileName);
                Assert.AreEqual(6, actual.Count);
                Assert.AreEqual("Check for GRANT .. TO [public]", actual[0].Name);
                Assert.AreEqual("Check for Comments", actual[4].Name);
            }
            finally
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }
        }

        [TestMethod]
        public void ValidateFilesAgainstPolicies_WithViolations()
        {
            PolicyHelper.activePolicies = (from p in PolicyHelper.allPolicies select p.Value).ToList();

            string script = Properties.Resources.PolicyHelper_WithViolations;
            string fileName = string.Empty;
            Package actual;

            try
            {
                fileName = Path.GetTempFileName();
                File.WriteAllText(fileName, script);

                List<string> files = new List<string>();
                files.Add(fileName);

                actual = new PolicyHelper().ValidateFilesAgainstPolicies(files);
                Assert.IsNotNull(actual, "ScriptViolations object should not be null for scripts with violations");
                Assert.AreEqual(1, actual.Count);
                Assert.AreEqual(Path.GetFileName(fileName), actual[0].ScriptName);
                Assert.AreEqual(6, actual[0].Count);
                Assert.AreEqual("Check for Comments", actual[0][4].Name);
            }
            finally
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }
        }

        [TestMethod]
        public void ValidateFilesAgainstPolicies_NoViolations()
        {
            EnterpriseConfigHelper.EnterpriseConfig = null;
            string script = Properties.Resources.PolicyHelper_NoViolations;
            script = script.Replace("<<date>>", DateTime.Now.ToString("MM/dd/yyyy"));
            string fileName = string.Empty;
            Package actual;

            try
            {
                fileName = Path.GetTempFileName();
                File.WriteAllText(fileName, script);

                List<string> files = new List<string>();
                files.Add(fileName);

                actual = new PolicyHelper().ValidateFilesAgainstPolicies(files);
                Assert.IsNull(actual, "ScriptViolations object should be null for scripts with no violations.");
            }
            finally
            {
                if (File.Exists(fileName))
                    File.Delete(fileName);
            }
        }

        /// <summary>
        ///A test for ValidateScriptAgainstPolicy
        ///</summary>
        [TestMethod()]
        public void ValidateScriptAgainstPolicyTest_WithIScriptPolicy()
        {
            string script = Properties.Resources.PolicyHelper_WithViolations;
            string targetDatabase = "TestDb";
            IScriptPolicy policy = new CommentHeaderPolicy();
            Violation actual;
            actual = PolicyHelper.ValidateScriptAgainstPolicy(script, targetDatabase, policy);
            Assert.IsNotNull(actual);
        }

        /// <summary>
        ///A test for ValidateScriptAgainstPolicy
        ///</summary>
        [TestMethod()]
        public void ValidateScriptAgainstPolicyTest_WithIScriptPolicyWithArguments()
        {
            string script = Properties.Resources.PolicyHelper_WithViolations;
            string targetDatabase = "TestDb";
            IScriptPolicyWithArguments policy = new StoredProcParameterPolicy();
            policy.Arguments.Add(new IScriptPolicyArgument() { Name = "Schema", Value = "dbo" });
            policy.Arguments.Add(new IScriptPolicyArgument() { Name = "Parameter", Value = "@MissingParameter" });
            policy.Arguments.Add(new IScriptPolicyArgument() { Name = "TargetDatabase", Value = "TestDb" });

            Violation actual;
            actual = PolicyHelper.ValidateScriptAgainstPolicy(script, targetDatabase, policy);
            Assert.IsNotNull(actual);
        }

        /// <summary>
        ///A test for ValidateScriptAgainstPolicy
        ///</summary>
        [TestMethod()]
        public void ValidateScriptAgainstPolicyTest_NoViolation()
        {
            string script = Properties.Resources.PolicyHelper_NoViolations;
            string targetDatabase = "TestDb";
            IScriptPolicy policy = new SelectStarPolicy();

            Violation actual;
            actual = PolicyHelper.ValidateScriptAgainstPolicy(script, targetDatabase, policy);
            Assert.IsNull(actual);
        }
    }
}
