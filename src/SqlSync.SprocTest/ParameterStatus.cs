using System;


namespace SqlSync.SprocTest
{
   
    public enum ParameterStatus
    {
        [System.ComponentModel.Description("Test Case parameter matches a Database derived parameter")]
        Matching,
        
        [System.ComponentModel.Description("Specified Test Case parameter does not match a Database derived parameter")]
        NotInDatabase,

        [System.ComponentModel.Description("The Database derived parameter is not specified in the Test Case")]
        NotInTestCase,

    }
}
