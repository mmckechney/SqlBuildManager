﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SqlBuildManager.Console.Dependent.UnitTest.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "16.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    public class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("SqlBuildManager.Console.Dependent.UnitTest.Properties.Resources", typeof(Resources).Assembly);
                    resourceMan = temp;
                }
                return resourceMan;
            }
        }
        
        /// <summary>
        ///   Overrides the current thread's CurrentUICulture property for all
        ///   resource lookups using this strongly typed resource class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        public static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        public static byte[] dbconfig {
            get {
                object obj = ResourceManager.GetObject("dbconfig", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        public static byte[] dbconfig_long {
            get {
                object obj = ResourceManager.GetObject("dbconfig_long", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        public static byte[] InsertForThreadedTest {
            get {
                object obj = ResourceManager.GetObject("InsertForThreadedTest", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        public static byte[] InsertForThreadedTest_ForceTimeout {
            get {
                object obj = ResourceManager.GetObject("InsertForThreadedTest_ForceTimeout", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        public static byte[] NoTrans_MultiDb {
            get {
                object obj = ResourceManager.GetObject("NoTrans_MultiDb", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        public static byte[] NoTrans_MultiDb1 {
            get {
                object obj = ResourceManager.GetObject("NoTrans_MultiDb1", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        public static byte[] SimpleSelect {
            get {
                object obj = ResourceManager.GetObject("SimpleSelect", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        public static byte[] SimpleSelect_WithCodeReview {
            get {
                object obj = ResourceManager.GetObject("SimpleSelect_WithCodeReview", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to BEGIN TRANSACTION
        ///
        ///SELECT * FROM dbo.TransactionTest WITH (TABLOCKX, HOLDLOCK) WHERE 1 = 1
        ///
        ///WAITFOR DELAY &apos;00:30&apos;
        ///
        ///ROLLBACK TRANSACTION.
        /// </summary>
        public static string sql_waitfor_createtimeout {
            get {
                return ResourceManager.GetString("sql_waitfor_createtimeout", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        public static byte[] SyntaxError {
            get {
                object obj = ResourceManager.GetObject("SyntaxError", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to BEGIN TRANSACTION
        ///INSERT INTO dbo.TransactionTest VALUES (&apos;PROCESS LOCK&apos;, newid(), getdate())
        ///DECLARE @Count INT
        ///SET @Count=0
        ///WHILE @Count &lt; {0}   --near infinite loop...
        ///BEGIN  
        ///       SELECT TOP 1 *  FROM dbo.TransactionTest WITH (TABLOCKX)
        ///       SET @Count = @Count+1
        ///END  
        ///COMMIT TRANSACTION.
        /// </summary>
        public static string TableLockingScript {
            get {
                return ResourceManager.GetString("TableLockingScript", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized resource of type System.Byte[].
        /// </summary>
        public static byte[] ThreadedTest_OnePassOneFail {
            get {
                object obj = ResourceManager.GetObject("ThreadedTest_OnePassOneFail", resourceCulture);
                return ((byte[])(obj));
            }
        }
    }
}
