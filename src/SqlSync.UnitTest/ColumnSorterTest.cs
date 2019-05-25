using SqlSync;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SqlSync.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for ColumnSorterTest and is intended
    ///to contain all ColumnSorterTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ColumnSorterTest
    {


      

        /// <summary>
        ///A test for CurrentColumn
        ///</summary>
        [TestMethod()]
        public void CurrentColumnTest()
        {
            ColumnSorter target = new ColumnSorter(); // TODO: Initialize to an appropriate value
            int expected = 0; // TODO: Initialize to an appropriate value
            target.CurrentColumn = expected;
            
        }
    }
}
