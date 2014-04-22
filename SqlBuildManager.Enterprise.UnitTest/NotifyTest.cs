using SqlBuildManager.Enterprise;
using Microsoft.VisualStudio.TestTools.UnitTesting;
namespace SqlBuildManager.Enterprise.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for NotifyTest and is intended
    ///to contain all NotifyTest Unit Tests
    ///</summary>
    [TestClass()]
    public class NotifyTest
    {
        /// <summary>
        ///A test for Name
        ///</summary>
        [TestMethod()]
        public void NameTest()
        {
            Notify target = new Notify();
            string expected = "This is my Name";
            string actual;
            target.Name = expected;
            actual = target.Name;
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for EMail
        ///</summary>
        [TestMethod()]
        public void EMailTest()
        {
            Notify target = new Notify();
            string expected = "myEmail@mail.com";
            string actual;
            target.EMail = expected;
            actual = target.EMail;
            Assert.AreEqual(expected, actual);
        }
    }
}
