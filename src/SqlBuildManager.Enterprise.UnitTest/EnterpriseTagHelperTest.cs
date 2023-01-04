using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Enterprise.Tag;
using System.Collections.Generic;

namespace SqlBuildManager.Enterprise.UnitTest
{


    /// <summary>
    ///This is a test class for EnterpriseTagHelperTest and is intended
    ///to contain all EnterpriseTagHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class EnterpriseTagHelperTest
    {
        private static List<ScriptTagInference> inferenceList;
        private static List<string> standardRegexValues;

        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            List<string> regexValues = new List<string>() { @"\bCR *-*#* *\d{3,10}", @"\bCR *-*#* *\d{3,10}" };
            standardRegexValues = regexValues;

            ApplyToGroup[] applyToGroup = new ApplyToGroup[] {
                new ApplyToGroup(){ GroupName="MyGroup1"},
                new ApplyToGroup(){GroupName = "MyGroup2"}
            };
            TagRegex[] tagRegex = new TagRegex[] {
                new TagRegex(){ RegexValue= regexValues[0]},
                new TagRegex(){ RegexValue= regexValues[1] }
            };
            inferenceList = new List<ScriptTagInference>();
            inferenceList.Add(new ScriptTagInference() { TagRegex = tagRegex, ApplyToGroup = applyToGroup });


        }


        /// <summary>
        ///A test for GetEnterpriseTagRegexValues
        ///</summary>
        [TestMethod()]
        public void GetEnterpriseTagRegexValuesTest_GoodTest()
        {
            List<ScriptTagInference> inferenceList = EnterpriseTagHelperTest.inferenceList;
            List<string> adGroupMembership = new List<string>() { "NotMyGroup", "MyGroup1" };
            List<string> expected = EnterpriseTagHelperTest.standardRegexValues;
            List<string> actual;
            actual = EnterpriseTagHelper.GetEnterpriseTagRegexValues(inferenceList, adGroupMembership);
            Assert.AreEqual(expected[0], actual[0]);
            Assert.AreEqual(expected[1], actual[1]);

        }

        /// <summary>
        ///A test for GetEnterpriseTagRegexValues
        ///</summary>
        [TestMethod()]
        public void GetEnterpriseTagRegexValuesTest_NoMatchingGroup()
        {
            List<ScriptTagInference> inferenceList = EnterpriseTagHelperTest.inferenceList;
            List<string> adGroupMembership = new List<string>() { "NotMyGroup", "NotMyGroupAgain" };
            List<string> expected = EnterpriseTagHelperTest.standardRegexValues;
            List<string> actual;
            actual = EnterpriseTagHelper.GetEnterpriseTagRegexValues(inferenceList, adGroupMembership);
            Assert.IsNull(actual);

        }

        /// <summary>
        ///A test for GetEnterpriseTagRegexValues
        ///</summary>
        [TestMethod()]
        public void GetEnterpriseTagRegexValuesTest_NullInferenceList()
        {
            List<ScriptTagInference> inferenceList = null;
            List<string> adGroupMembership = new List<string>() { "NotMyGroup", "MyGroup1" };
            List<string> expected = EnterpriseTagHelperTest.standardRegexValues;
            List<string> actual;
            actual = EnterpriseTagHelper.GetEnterpriseTagRegexValues(inferenceList, adGroupMembership);
            Assert.IsNull(actual);
        }

        /// <summary>
        ///A test for GetEnterpriseTagRegexValues
        ///</summary>
        [TestMethod()]
        public void GetEnterpriseTagRegexValuesTest_EmptyInferenceList()
        {
            List<ScriptTagInference> inferenceList = new List<ScriptTagInference>();
            List<string> adGroupMembership = new List<string>() { "NotMyGroup", "MyGroup1" };
            List<string> expected = EnterpriseTagHelperTest.standardRegexValues;
            List<string> actual;
            actual = EnterpriseTagHelper.GetEnterpriseTagRegexValues(inferenceList, adGroupMembership);
            Assert.IsNull(actual);
        }

        /// <summary>
        ///A test for GetEnterpriseTagRegexValues
        ///</summary>
        [TestMethod()]
        public void GetEnterpriseTagRegexValuesTest_NullADGroupMembership()
        {
            List<ScriptTagInference> inferenceList = EnterpriseTagHelperTest.inferenceList;
            List<string> adGroupMembership = null;
            List<string> expected = EnterpriseTagHelperTest.standardRegexValues;
            List<string> actual;
            actual = EnterpriseTagHelper.GetEnterpriseTagRegexValues(inferenceList, adGroupMembership);
            Assert.IsNull(actual);

        }

        /// <summary>
        ///A test for GetEnterpriseTagRegexValues
        ///</summary>
        [TestMethod()]
        public void GetEnterpriseTagRegexValuesTest_EmptyADGroupMembership()
        {
            List<ScriptTagInference> inferenceList = EnterpriseTagHelperTest.inferenceList;
            List<string> adGroupMembership = new List<string>();
            List<string> expected = EnterpriseTagHelperTest.standardRegexValues;
            List<string> actual;
            actual = EnterpriseTagHelper.GetEnterpriseTagRegexValues(inferenceList, adGroupMembership);
            Assert.IsNull(actual);

        }

        /// <summary>
        ///A test for GetEnterpriseTagRegexValues
        ///</summary>
        [TestMethod()]
        public void GetEnterpriseTagRegexValuesTest_NullInferenceInGroup()
        {
            List<ScriptTagInference> inferenceList = EnterpriseTagHelperTest.inferenceList;
            inferenceList.Add(null);
            List<string> adGroupMembership = new List<string>() { "NotMyGroup", "MyGroup1" };
            List<string> expected = EnterpriseTagHelperTest.standardRegexValues;
            List<string> actual;
            actual = EnterpriseTagHelper.GetEnterpriseTagRegexValues(inferenceList, adGroupMembership);
            Assert.IsNull(actual);

        }

    }
}
