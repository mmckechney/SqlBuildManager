using SqlBuildManager.Enterprise;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;

namespace SqlBuildManager.Enterprise.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for TableWatchTest and is intended
    ///to contain all TableWatchTest Unit Tests
    ///</summary>
    [TestClass()]
    public class TableWatchTest
    {

        /// <summary>
        ///A test for EmailSubject
        ///</summary>
        [TestMethod()]
        public void EmailSubjectTest()
        {
            TableWatch target = new TableWatch(); 
            string expected = "Here is the e-mail subject";
            string actual;
            target.EmailSubject = expected;
            actual = target.EmailSubject;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for EmailBody
        ///</summary>
        [TestMethod()]
        public void EmailBodyTest()
        {
            TableWatch target = new TableWatch();
            string expected = "Here is the e-mail body";
            string actual;
            target.EmailBody = expected;
            actual = target.EmailBody;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for Description
        ///</summary>
        [TestMethod()]
        public void DescriptionTest()
        {
            TableWatch target = new TableWatch();
            string expected = "Here is the description";
            string actual;
            target.Description = expected;
            actual = target.Description;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for FoundTables
        ///</summary>
        [TestMethod()]
        public void FoundTablesTest()
        {
            TableWatch target = new TableWatch(); // TODO: Initialize to an appropriate value
            List<string> expected = new List<string>();
            expected.Add("Table1");
            expected.Add("Table2");
            expected.Add("Table3");
            List<string> actual;
            target.FoundTables = expected;
            actual = target.FoundTables;
            Assert.AreEqual(expected, actual);
        }
    }
}
