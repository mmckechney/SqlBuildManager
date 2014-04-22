using SqlBuildManager.Enterprise.CodeReview;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using SqlSync.SqlBuild;
using System.Data.EntityClient;

namespace SqlBuildManager.Enterprise.UnitTest
{


    /// <summary>
    ///This is a test class for CodeReviewManagerTest and is intended
    ///to contain all CodeReviewManagerTest Unit Tests
    ///</summary>
    [TestClass()]
    public class CodeReviewManagerTest
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }

        #region Additional test attributes
        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        //[ClassCleanup()]
        //public static void MyClassCleanup()
        //{
        //}
        //
        //Use TestInitialize to run code before running each test
        //[TestInitialize()]
        //public void MyTestInitialize()
        //{
        //}
        //
        //Use TestCleanup to run code after each test has run
        //[TestCleanup()]
        //public void MyTestCleanup()
        //{
        //}
        //
        #endregion

        #region CodeReviewManagerConstructorTest
        /// <summary>
        ///A test for CodeReviewManager Constructor
        ///</summary>
        [TestMethod()]
        public void CodeReviewManagerConstructorTest()
        {
            CodeReviewManager target = new CodeReviewManager();
            Assert.IsNotNull(target);
            Assert.IsInstanceOfType(target, typeof(CodeReviewManager));
        }
        #endregion

        #region CalculateReviewCheckSumTest
        /// <summary>
        ///A test for CalculateReviewCheckSum
        ///</summary>
        [TestMethod()]
        public void CalculateReviewCheckSumTest_WithParameters()
        {
            Guid codeReviewId = new Guid("1C2095DF-C1F6-406D-9345-A46B3E799006");
            string reviewer = "MyReviewerName";
            DateTime reviewDate = new DateTime(1999, 12, 31);
            string comment = "Awesome Comment";
            string reviewNumber = "123456";
            short reviewStatus = (short) CodeReviewStatus.Accepted;
            string scriptText = @"This
is the script
of some awesome script 
text";

            string expected = "CA9415C33F2F909E6F1F95643BCB088AB5A616DD";
            string actual;
            actual = CodeReviewManager.CalculateReviewCheckSum(codeReviewId, reviewer, reviewDate, comment, reviewNumber, reviewStatus, scriptText);
            Assert.AreEqual(expected, actual);
            
        }

        ///A test for CalculateReviewCheckSum
        ///</summary>
        [TestMethod()]
        public void CalculateReviewCheckSumTest_WithDefaultedParameters()
        {
            Guid codeReviewId = new Guid();
            string reviewer = string.Empty;
            DateTime reviewDate = DateTime.MinValue;
            string comment = string.Empty;
            string reviewNumber = string.Empty;
            short reviewStatus = 0;
            string scriptText = string.Empty;

            string expected = "CF32664CEA5BFB763D5BBEF98DD143D598C9FCCC";
            string actual;
            actual = CodeReviewManager.CalculateReviewCheckSum(codeReviewId, reviewer, reviewDate, comment, reviewNumber, reviewStatus, scriptText);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for CalculateReviewCheckSum
        ///</summary>
        [TestMethod()]
        public void CalculateReviewCheckSumTest_WithReviewRow()
        {
            SqlSyncBuildData buildData = new SqlSyncBuildData();
            SqlSyncBuildData.CodeReviewRow codeReviewRow = buildData.CodeReview.NewCodeReviewRow();
            codeReviewRow.CodeReviewId = new Guid("827B9C2C-9952-4AEE-83CB-285789C41B1F");
            codeReviewRow.ReviewBy = "CoolReviewer";
            codeReviewRow.ReviewDate = new DateTime(2013, 6, 17);
            codeReviewRow.ReviewStatus = (short) CodeReviewStatus.OutOfDate;
            codeReviewRow.Comment = "Nothing to see here";
            string scriptText = @"Here is some 
very awesome 
and quite unexpected value for 
a SQL script";
            string expected = "17C8FB25CA33B46B0B7133155C4B00FC335ABAEA";
            string actual;
            actual = CodeReviewManager.CalculateReviewCheckSum(codeReviewRow, scriptText);
            Assert.AreEqual(expected, actual);
            
        }

        /// <summary>
        ///A test for CalculateReviewCheckSum
        ///</summary>
        [TestMethod()]
        public void CalculateReviewCheckSumTest_WithReviewRow2()
        {
            SqlSyncBuildData buildData = new SqlSyncBuildData();
            SqlSyncBuildData.CodeReviewRow codeReviewRow = buildData.CodeReview.NewCodeReviewRow();
            codeReviewRow.CodeReviewId = new Guid("61DD1DA5-FBC2-440C-9554-11E4D78CA739");
            codeReviewRow.ReviewBy = "CoolReviewer";
            codeReviewRow.ReviewDate = new DateTime(2013, 6, 17);
            codeReviewRow.ReviewStatus = (short)CodeReviewStatus.OutOfDate;
            codeReviewRow.Comment = "Nothing to see here - really!!";
            string scriptText = @"Here is some 
very awesome 
and quite unexpected value for 
a SQL script
SELECT * FROM dbo.Everywhere";
            string expected = "4BFB19D5EA9708E7F6BB7B38AF422A406BED9C11";
            string actual;
            actual = CodeReviewManager.CalculateReviewCheckSum(codeReviewRow, scriptText);
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for CalculateReviewCheckSum
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(NullReferenceException))]
        public void CalculateReviewCheckSumTest_WithReviewRow_ThrowException()
        {
            SqlSyncBuildData buildData = new SqlSyncBuildData();
            SqlSyncBuildData.CodeReviewRow codeReviewRow = null;
            string scriptText = @"Here is some 
very awesome 
and quite unexpected value for 
a SQL script
SELECT * FROM dbo.Everywhere";
            CodeReviewManager.CalculateReviewCheckSum(codeReviewRow, scriptText);

        }
        #endregion
        
        #region GetConsolidatedBaseTest

        /// <summary>
        ///A test for GetConsolidatedBase
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Enterprise.dll")]
        public void GetConsolidatedBaseTest()
        {
            Guid codeReviewId = new Guid("61DD1DA5-FBC2-440C-9554-11E4D78CA739");
            string reviewer = "reviewer 1234";
            DateTime reviewDate = new DateTime(2000, 1, 1);
            string comment = "Appropriate Comment";
            string reviewNumber = "987654321";
            short reviewStatus = (short) CodeReviewStatus.Accepted;
            string scriptText = @"This
is the script
for my 
test";
            string expected =
                @"61dd1da5-fbc2-440c-9554-11e4d78ca739|^%EGHYUqwqdq3qsa``08-|reviewer 1234|1/1/2000 12:00:00 AM|Appropriate Comment|<>?JKTYrthdfgwrt,./.,UK>><<??>|987654321|1|This
is the script
for my 
test";
            string actual;
            actual = CodeReviewManager_Accessor.GetConsolidatedBase(codeReviewId, reviewer, reviewDate, comment, reviewNumber, reviewStatus, scriptText);
            Assert.AreEqual(expected, actual);

        }

        #endregion
        
        #region GetValidationKeyTest

        /// <summary>
        ///A test for GetValidationKey
        ///</summary>
        [TestMethod()]
        public void GetValidationKeyTest()
        {
            SqlSyncBuildData buildData = new SqlSyncBuildData();
            SqlSyncBuildData.CodeReviewRow codeReviewRow = buildData.CodeReview.NewCodeReviewRow();
            codeReviewRow.CodeReviewId = new Guid("3C25EC49-B73E-4140-BAC4-3F543199F447");
            codeReviewRow.ReviewBy = "CoolReviewerABC";
            codeReviewRow.ReviewDate = new DateTime(2013, 6, 15);
            codeReviewRow.ReviewStatus = (short)CodeReviewStatus.Defect;
            codeReviewRow.Comment = "Nothing to see here - REALLY!";
            codeReviewRow.CheckSum = "MY_CHECK_SUM_IS_HERE";

            string expected = "CFFF19EF9E5D504D2F519AE168C6CCB8769E033C";
            string actual;
            actual = CodeReviewManager.GetValidationKey(ref codeReviewRow);
            Assert.AreEqual(expected, actual);
     
        }

        [TestMethod()]
        public void GetValidationKeyTest_DetectChange()
        {
            SqlSyncBuildData buildData = new SqlSyncBuildData();
            SqlSyncBuildData.CodeReviewRow codeReviewRow = buildData.CodeReview.NewCodeReviewRow();
            codeReviewRow.CodeReviewId = new Guid("3C25EC49-B73E-4140-BAC4-3F543199F447");
            codeReviewRow.ReviewBy = "CoolReviewerABC";
            codeReviewRow.ReviewDate = new DateTime(2013, 6, 15);
            codeReviewRow.ReviewStatus = (short)CodeReviewStatus.Defect;
            codeReviewRow.Comment = "Nothing to see here - REALLY!";
            codeReviewRow.CheckSum = "MY_CHECK_SUM_IS_HERE";

            string actual1, actual2;
            actual1 = CodeReviewManager.GetValidationKey(ref codeReviewRow);

            codeReviewRow.CheckSum = "MY_CHECK_SUM_IS_DONE";
            actual2 = CodeReviewManager.GetValidationKey(ref codeReviewRow);

            Assert.AreNotEqual(actual1, actual2);

        }

        [TestMethod()]
        public void GetValidationKeyTest_DetectChangeInStatus()
        {
            SqlSyncBuildData buildData = new SqlSyncBuildData();
            SqlSyncBuildData.CodeReviewRow codeReviewRow = buildData.CodeReview.NewCodeReviewRow();
            codeReviewRow.CodeReviewId = new Guid("3C25EC49-B73E-4140-BAC4-3F543199F447");
            codeReviewRow.ReviewBy = "CoolReviewerABC";
            codeReviewRow.ReviewDate = new DateTime(2013, 6, 15);
            codeReviewRow.ReviewStatus = (short)CodeReviewStatus.Defect;
            codeReviewRow.Comment = "Nothing to see here - REALLY!";
            codeReviewRow.CheckSum = "MY_CHECK_SUM_IS_HERE";

            string actual1, actual2;
            actual1 = CodeReviewManager.GetValidationKey(ref codeReviewRow);

            codeReviewRow.ReviewStatus = (short) CodeReviewStatus.Accepted;
            actual2 = CodeReviewManager.GetValidationKey(ref codeReviewRow);

            Assert.AreNotEqual(actual1, actual2);

        }

        #endregion
        
        #region SetValidationKeyTest

        /// <summary>
        ///A test for SetValidationKey
        ///</summary>
        [TestMethod()]
        public void SetValidationKeyTest()
        {
            SqlSyncBuildData buildData = new SqlSyncBuildData();
            SqlSyncBuildData.CodeReviewRow codeReviewRow = buildData.CodeReview.NewCodeReviewRow();
            Guid reviewId = new Guid("50F8716F-94C5-45A2-9E1B-8EA957FDA7C8");
            codeReviewRow.CodeReviewId = reviewId;
            codeReviewRow.ReviewBy = "Vaidation Reviewer";
            codeReviewRow.ReviewDate = new DateTime(1997, 9, 28);
            codeReviewRow.ReviewStatus = (short)CodeReviewStatus.None;
            codeReviewRow.Comment = "Nothing to see here - REALLY!";
            string checkSum = "MY_CHECK_SUM_IS_HERE";
            codeReviewRow.CheckSum = checkSum;

            string validation = CodeReviewManager.GetValidationKey(ref codeReviewRow);
            Assert.IsTrue(codeReviewRow.ValidationKey == string.Empty);

            CodeReviewManager.SetValidationKey(ref codeReviewRow);

            Assert.AreEqual(validation,codeReviewRow.ValidationKey);
            Assert.AreEqual(checkSum,codeReviewRow.CheckSum);
            Assert.AreEqual(reviewId,codeReviewRow.CodeReviewId);
        }

        #endregion
        
        #region ValidateReviewCheckSumTest
        /// <summary>
        ///A test for ValidateReviewCheckSum
        ///</summary>
        [TestMethod()]
        public void ValidateReviewCheckSumTest()
        {
            SqlSyncBuildData buildData = new SqlSyncBuildData();
            SqlSyncBuildData.CodeReviewRow codeReviewRow = buildData.CodeReview.NewCodeReviewRow();
            codeReviewRow.CodeReviewId = new Guid("8134AF14-9898-4898-A6C8-0BCE6AAC0B41");
            codeReviewRow.ReviewBy = "CheckSummer";
            codeReviewRow.ReviewDate = new DateTime(2001, 2, 7);
            codeReviewRow.ReviewStatus = (short)CodeReviewStatus.Accepted;
            codeReviewRow.Comment = "This Looks Awesome!!";
            codeReviewRow.CheckSum = "160073124A09A2F7AE8C1D87D7DB10050F5F302A";
            string scriptText = @"This is my
yet another
attempt to type up
a script";
            bool expected = true;
            bool actual;
            actual = CodeReviewManager.ValidateReviewCheckSum(codeReviewRow, scriptText);
            Assert.AreEqual(expected, actual);
     
        }

        /// <summary>
        ///A test for ValidateReviewCheckSum
        ///</summary>
        [TestMethod()]
        public void ValidateReviewCheckSumTest_Failure()
        {
            SqlSyncBuildData buildData = new SqlSyncBuildData();
            SqlSyncBuildData.CodeReviewRow codeReviewRow = buildData.CodeReview.NewCodeReviewRow();
            codeReviewRow.CodeReviewId = new Guid("8134AF14-9898-4898-A6C8-0BCE6AAC0B41");
            codeReviewRow.ReviewBy = "CheckSummer";
            codeReviewRow.ReviewDate = new DateTime(2001, 2, 7);
            codeReviewRow.ReviewStatus = (short)CodeReviewStatus.Defect;
            codeReviewRow.Comment = "This Looks Awesome!!";
            codeReviewRow.CheckSum = "160073124A09A2F7AE8C1D87D7DB10050F5F302A";
            string scriptText = @"This is my
yet another
attempt to type up
a script";
            bool expected = false;
            bool actual;
            actual = CodeReviewManager.ValidateReviewCheckSum(codeReviewRow, scriptText);
            Assert.AreEqual(expected, actual);

        }

        #endregion
        
        #region ValidationKeyTest

        /// <summary>
        ///A test for ValidationKey
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Enterprise.dll")]
        public void ValidationKeyTest()
        {
            SqlSyncBuildData buildData = new SqlSyncBuildData();
            SqlSyncBuildData.CodeReviewRow codeReviewRow = buildData.CodeReview.NewCodeReviewRow();
            codeReviewRow.CodeReviewId = new Guid("44946011-8026-4187-82EA-6FC77BC29115");
            codeReviewRow.ReviewBy = "CheckSummer2";
            codeReviewRow.ReviewDate = new DateTime(2003, 1, 17);
            codeReviewRow.ReviewStatus = (short)CodeReviewStatus.Accepted;
            codeReviewRow.Comment = "This Looks Super Awesome!!";
            codeReviewRow.CheckSum = "160073124A49A2F7AE831D87D53430050F54642A";

            string expected = "9CC8C5F6E0A4E17AFE55B5B7CBCB95A018E77CBE";
            string actual;
            actual = CodeReviewManager_Accessor.ValidationKey(codeReviewRow);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ValidationKey
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Enterprise.dll")]
        public void ValidationKeyTest_Failure()
        {
            SqlSyncBuildData buildData = new SqlSyncBuildData();
            SqlSyncBuildData.CodeReviewRow codeReviewRow = buildData.CodeReview.NewCodeReviewRow();
            codeReviewRow.CodeReviewId = new Guid("44946011-8026-4187-82EA-6FC77BC29115");
            codeReviewRow.ReviewBy = "CheckSummer2";
            codeReviewRow.ReviewDate = new DateTime(2003, 1, 17);
            codeReviewRow.ReviewStatus = (short)CodeReviewStatus.Accepted;
            codeReviewRow.Comment = "This Looks Super Awesome!!";
            codeReviewRow.CheckSum = "F60073124A49A2F7AE831D87D53430050F54642A";

            string expected = "9CC8C5F6E0A4E17AFE55B5B7CBCB95A018E77CBE";
            string actual;
            actual = CodeReviewManager_Accessor.ValidationKey(codeReviewRow);
            Assert.AreNotEqual(expected, actual);
        }

        #endregion

        #region SyncXmlReviewDatatoEfReviewTest
        /// <summary>
        ///A test for SyncXmlReviewDatatoEfReview
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Enterprise.dll")]
        public void SyncXmlReviewDatatoEfReviewTest()
        {
            SqlSyncBuildData buildData = new SqlSyncBuildData();
            SqlSyncBuildData.CodeReviewRow codeReviewRow = buildData.CodeReview.NewCodeReviewRow();
            codeReviewRow.CodeReviewId = new Guid("8134AF14-9898-4898-A6C8-0BCE6AAC0B41");
            codeReviewRow.ReviewBy = "CheckSummer";
            codeReviewRow.ReviewDate = new DateTime(2001, 2, 7);
            codeReviewRow.ReviewStatus = (short)CodeReviewStatus.Accepted;
            codeReviewRow.Comment = "This Looks Awesome!!";
            codeReviewRow.CheckSum = "160073124A09A2F7AE8C1D87D7DB10050F5F302A";
            codeReviewRow.ValidationKey = "NOT_REALLY_VALID_BUT_IM_NOT_CHECKING";
            codeReviewRow.ScriptId = "B42B8EE5-6C09-4E53-A6CC-425BD525FF98";
            CodeReview.CodeReview review = new CodeReview.CodeReview();
            CodeReviewManager_Accessor.SyncXmlReviewDatatoEfReview(codeReviewRow, review);


            Assert.AreEqual(codeReviewRow.CodeReviewId,review.CodeReviewId);
            Assert.AreEqual(codeReviewRow.ReviewBy, review.ReviewBy);
            Assert.AreEqual(codeReviewRow.ReviewStatus, review.ReviewStatus);
            Assert.AreEqual(codeReviewRow.CheckSum, review.CheckSum);
            Assert.AreEqual(codeReviewRow.Comment, review.Comment);
            Assert.AreEqual(codeReviewRow.ReviewDate, review.ReviewDate);
            Assert.AreEqual(codeReviewRow.ReviewNumber, review.ReviewNumber);
            Assert.AreEqual(codeReviewRow.ValidationKey, review.ValidationKey);
            Assert.AreEqual(codeReviewRow.ScriptId, review.ScriptId);
        }


        #endregion

        #region MarkCodeReviewOutOfDateTest
        /// <summary>
        ///A test for MarkCodeReviewOutOfDate
        ///</summary>
        [TestMethod()]
        public void MarkCodeReviewOutOfDateTest()
        {
            SqlSyncBuildData buildData = new SqlSyncBuildData();
            SqlSyncBuildData.CodeReviewRow codeReviewRow = buildData.CodeReview.NewCodeReviewRow();
            codeReviewRow.CodeReviewId = new Guid("3C25EC49-B73E-4140-BAC4-3F543199F447");
            codeReviewRow.ReviewBy = "CoolReviewerABC";
            codeReviewRow.ReviewDate = new DateTime(2013, 6, 15);
            codeReviewRow.ReviewStatus = (short)CodeReviewStatus.Accepted;
            codeReviewRow.Comment = "Nothing to see here - REALLY!";
            codeReviewRow.CheckSum = "MY_CHECK_SUM_IS_HERE";
            bool expected = true;
            bool actual;
            actual = CodeReviewManager.MarkCodeReviewOutOfDate(ref buildData, ref codeReviewRow);
            Assert.AreEqual(expected, actual);
            Assert.AreEqual((short)CodeReviewStatus.OutOfDate, codeReviewRow.ReviewStatus);
        }

        #endregion

        ///// <summary>
        /////A test for UpdateCodeReview
        /////</summary>
        //[TestMethod()]
        //public void UpdateCodeReviewTest()
        //{
        //    SqlSyncBuildData buildData = null; // TODO: Initialize to an appropriate value
        //    SqlSyncBuildData buildDataExpected = null; // TODO: Initialize to an appropriate value
        //    SqlSyncBuildData.CodeReviewRow reviewRow = null; // TODO: Initialize to an appropriate value
        //    SqlSyncBuildData.CodeReviewRow reviewRowExpected = null; // TODO: Initialize to an appropriate value
        //    string scriptText = string.Empty; // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    actual = CodeReviewManager.UpdateCodeReview(ref buildData, ref reviewRow, scriptText);
        //    Assert.AreEqual(buildDataExpected, buildData);
        //    Assert.AreEqual(reviewRowExpected, reviewRow);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for UpdateCodeReviewToDatabase
        /////</summary>
        //[TestMethod()]
        //public void UpdateCodeReviewToDatabaseTest()
        //{
        //    SqlSyncBuildData.CodeReviewRow codeReviewRow = null; // TODO: Initialize to an appropriate value
        //    CodeReviewManager.UpdateCodeReviewToDatabase(codeReviewRow);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for ValidateReviewCheckSum
        /////</summary>
        //[TestMethod()]
        //public void ValidateReviewCheckSumTest1()
        //{
        //    SqlSyncBuildData sqlSyncBuildData = null; // TODO: Initialize to an appropriate value
        //    string baseDirectory = string.Empty; // TODO: Initialize to an appropriate value
        //    CodeReviewManager.ValidateReviewCheckSum(sqlSyncBuildData, baseDirectory);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for LoadCodeReviewData
        /////</summary>
        //[TestMethod()]
        //public void LoadCodeReviewDataTest()
        //{
        //    SqlSyncBuildData buildData = null; // TODO: Initialize to an appropriate value
        //    bool databaseSuccess = false; // TODO: Initialize to an appropriate value
        //    bool databaseSuccessExpected = false; // TODO: Initialize to an appropriate value
        //    SqlSyncBuildData expected = null; // TODO: Initialize to an appropriate value
        //    SqlSyncBuildData actual;
        //    actual = CodeReviewManager.LoadCodeReviewData(buildData, out databaseSuccess);
        //    Assert.AreEqual(databaseSuccessExpected, databaseSuccess);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        

        ///// <summary>
        /////A test for SaveCodeReview
        /////</summary>
        //[TestMethod()]
        //public void SaveCodeReviewTest()
        //{
        //    SqlSyncBuildData buildData = null; // TODO: Initialize to an appropriate value
        //    SqlSyncBuildData buildDataExpected = null; // TODO: Initialize to an appropriate value
        //    SqlSyncBuildData.ScriptRow scriptRow = null; // TODO: Initialize to an appropriate value
        //    SqlSyncBuildData.ScriptRow scriptRowExpected = null; // TODO: Initialize to an appropriate value
        //    string scriptText = string.Empty; // TODO: Initialize to an appropriate value
        //    string comment = string.Empty; // TODO: Initialize to an appropriate value
        //    string reviewBy = string.Empty; // TODO: Initialize to an appropriate value
        //    DateTime reviewDate = new DateTime(); // TODO: Initialize to an appropriate value
        //    string reviewNumber = string.Empty; // TODO: Initialize to an appropriate value
        //    int reviewStatus = 0; // TODO: Initialize to an appropriate value
        //    bool expected = false; // TODO: Initialize to an appropriate value
        //    bool actual;
        //    actual = CodeReviewManager.SaveCodeReview(ref buildData, ref scriptRow, scriptText, comment, reviewBy, reviewDate, reviewNumber, reviewStatus);
        //    Assert.AreEqual(buildDataExpected, buildData);
        //    Assert.AreEqual(scriptRowExpected, scriptRow);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for SaveCodeReviewToDatabase
        /////</summary>
        //[TestMethod()]
        //public void SaveCodeReviewToDatabaseTest()
        //{
        //    SqlSyncBuildData.CodeReviewRow codeReviewRow = null; // TODO: Initialize to an appropriate value
        //    CodeReviewManager.SaveCodeReviewToDatabase(codeReviewRow);
        //    Assert.Inconclusive("A method that does not return a value cannot be verified.");
        //}

        ///// <summary>
        /////A test for Connection
        /////</summary>
        //[TestMethod()]
        //[DeploymentItem("SqlBuildManager.Enterprise.dll")]
        //public void ConnectionTest()
        //{
        //    EntityConnection actual;
        //    actual = CodeReviewManager_Accessor.Connection;
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}

        ///// <summary>
        /////A test for GetNewEntity
        /////</summary>
        //[TestMethod()]
        //[DeploymentItem("SqlBuildManager.Enterprise.dll")]
        //public void GetNewEntityTest()
        //{
        //    SqlCodeReviewEntities expected = null; // TODO: Initialize to an appropriate value
        //    SqlCodeReviewEntities actual;
        //    actual = CodeReviewManager_Accessor.GetNewEntity();
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}
    }
}
