﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.269
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SqlBuildManager.Console.UnitTest.Properties {
    using System;
    
    
    /// <summary>
    ///   A strongly-typed resource class, for looking up localized strings, etc.
    /// </summary>
    // This class was auto-generated by the StronglyTypedResourceBuilder
    // class via a tool like ResGen or Visual Studio.
    // To add or remove a member, edit your .ResX file then rerun ResGen
    // with the /str option, or rebuild your VS project.
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("System.Resources.Tools.StronglyTypedResourceBuilder", "4.0.0.0")]
    [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    internal class Resources {
        
        private static global::System.Resources.ResourceManager resourceMan;
        
        private static global::System.Globalization.CultureInfo resourceCulture;
        
        [global::System.Diagnostics.CodeAnalysis.SuppressMessageAttribute("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal Resources() {
        }
        
        /// <summary>
        ///   Returns the cached ResourceManager instance used by this class.
        /// </summary>
        [global::System.ComponentModel.EditorBrowsableAttribute(global::System.ComponentModel.EditorBrowsableState.Advanced)]
        internal static global::System.Resources.ResourceManager ResourceManager {
            get {
                if (object.ReferenceEquals(resourceMan, null)) {
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("SqlBuildManager.Console.UnitTest.Properties.Resources", typeof(Resources).Assembly);
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
        internal static global::System.Globalization.CultureInfo Culture {
            get {
                return resourceCulture;
            }
            set {
                resourceCulture = value;
            }
        }
        
        internal static byte[] InsertForThreadedTest {
            get {
                object obj = ResourceManager.GetObject("InsertForThreadedTest", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        internal static byte[] NoTrans_MultiDb_multidb {
            get {
                object obj = ResourceManager.GetObject("NoTrans_MultiDb_multidb", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        internal static byte[] NoTrans_MultiDb_sbm {
            get {
                object obj = ResourceManager.GetObject("NoTrans_MultiDb_sbm", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        ///&lt;BuildSettings xmlns:xsi=&quot;http://www.w3.org/2001/XMLSchema-instance&quot; xmlns:xsd=&quot;http://www.w3.org/2001/XMLSchema&quot;&gt;
        ///  &lt;DistributionType&gt;EqualSplit&lt;/DistributionType&gt;
        ///  &lt;RemoteExecutionServers&gt;
        ///    &lt;ServerConfigData&gt;
        ///      &lt;ServerName&gt;localhost&lt;/ServerName&gt;
        ///      &lt;TcpServiceEndpoint&gt;net.tcp://localhost:8676/SqlBuildManager.Services/BuildService&lt;/TcpServiceEndpoint&gt;
        ///      &lt;HttpServiceEndpoint&gt;http://localhost:8675/SqlBuildManager.Services/BuildService&lt;/HttpServiceE [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string remote_execution_file {
            get {
                return ResourceManager.GetString("remote_execution_file", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        ///&lt;BuildSettings xmlns:xsi=&quot;http://www.w3.org/2001/XMLSchema-instance&quot; xmlns:xsd=&quot;http://www.w3.org/2001/XMLSchema&quot;&gt;
        ///  &lt;DistributionType&gt;EqualSplit&lt;/DistributionType&gt;
        ///  &lt;RemoteExecutionServers&gt;
        ///    &lt;ServerConfigData&gt;
        ///      &lt;ServerName&gt;badServer_name&lt;/ServerName&gt;
        ///      &lt;TcpServiceEndpoint&gt;net.tcp://badServer_name:8676/SqlBuildManager.Services/BuildService&lt;/TcpServiceEndpoint&gt;
        ///      &lt;HttpServiceEndpoint&gt;http://badServer_name:8675/SqlBuildManager.Services/BuildServic [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string remote_execution_file_bad_exeserver {
            get {
                return ResourceManager.GetString("remote_execution_file_bad_exeserver", resourceCulture);
            }
        }
        
        internal static byte[] SimpleSelect {
            get {
                object obj = ResourceManager.GetObject("SimpleSelect", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        internal static byte[] SimpleSelect_WithCodeReview {
            get {
                object obj = ResourceManager.GetObject("SimpleSelect_WithCodeReview", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        internal static byte[] SyntaxError {
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
        internal static string TableLockingScript {
            get {
                return ResourceManager.GetString("TableLockingScript", resourceCulture);
            }
        }
    }
}
