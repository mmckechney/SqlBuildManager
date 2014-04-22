﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.235
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SqlSync.SqlBuild.UnitTest.Properties {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("SqlSync.SqlBuild.UnitTest.Properties.Resources", typeof(Resources).Assembly);
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
        
        /// <summary>
        ///   Looks up a localized string similar to USE [master]
        ///GO
        ///CREATE DATABASE [{0}] ON  PRIMARY 
        ///( NAME = N&apos;{0}&apos;, FILENAME = N&apos;{1}\{0}.mdf&apos; , SIZE = 3072KB , MAXSIZE = UNLIMITED, FILEGROWTH = 1024KB )
        /// LOG ON 
        ///( NAME = N&apos;{0}_log&apos;, FILENAME = N&apos;{1}\{0}_log.ldf&apos; , SIZE = 1024KB , MAXSIZE = 2048GB , FILEGROWTH = 10%)
        ///GO
        ///IF (1 = FULLTEXTSERVICEPROPERTY(&apos;IsFullTextInstalled&apos;))
        ///begin
        ///EXEC [{0}].[dbo].[sp_fulltext_database] @action = &apos;disable&apos;
        ///end
        ///GO
        ///ALTER DATABASE [{0}] SET ANSI_NULL_DEFAULT OFF 
        ///GO
        ///ALTER DATABASE [{0}] SET ANSI_NULLS OFF 
        ///GO        /// [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string CreateDatabaseScript {
            get {
                return ResourceManager.GetString("CreateDatabaseScript", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to IF NOT EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = &apos;TransactionTest&apos; AND TABLE_SCHEMA = &apos;dbo&apos;)
        ///BEGIN
        ///	CREATE TABLE [dbo].[TransactionTest](
        ///	[ID] [int] IDENTITY(1,1) NOT NULL,
        ///	[Message] [varchar](200) NULL,
        ///	[Guid] [uniqueidentifier] NULL,
        ///	[DateTimeStamp] [datetime] NULL
        ///) ON [PRIMARY]
        ///END.
        /// </summary>
        internal static string CreateTestTablesScript {
            get {
                return ResourceManager.GetString("CreateTestTablesScript", resourceCulture);
            }
        }
        
        internal static byte[] DBList {
            get {
                object obj = ResourceManager.GetObject("DBList", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to IF NOT EXISTS(SELECT 1 FROM SqlBuild_Logging WHERE [ScriptId] = &apos;{0}&apos;) BEGIN INSERT INTO SqlBuild_Logging([BuildFileName],[ScriptFileName],[ScriptId],[ScriptFileHash],[CommitDate],[Sequence],[UserId],[AllowScriptBlock],[ScriptText],[Tag],[TargetDatabase])
        ///VALUES(&apos;TestPreRun&apos;,&apos;TestPreRunScript&apos;,&apos;{0}&apos;,&apos;MadeUpHash&apos;,getdate(),1, &apos;TestUser&apos;, 1,&apos;Testing&apos;,&apos;&apos;,&apos;TestDatabase&apos;) END.
        /// </summary>
        internal static string InsertPreRunScriptLogEntryScript {
            get {
                return ResourceManager.GetString("InsertPreRunScriptLogEntryScript", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to IF NOT EXISTS (SELECT 1 FROM sys.objects WHERE name = &apos;SqlBuild_Logging&apos; AND type = &apos;U&apos;)
        ///BEGIN
        ///
        ///	CREATE TABLE SqlBuild_Logging
        ///	(
        ///		[BuildFileName] varchar(300) NOT NULL,
        ///		[ScriptFileName] varchar(300) NOT NULL,
        ///		[ScriptId] uniqueidentifier,
        ///		[ScriptFileHash] varchar(100),
        ///		[CommitDate] datetime NOT NULL,
        ///		[Sequence] int,
        ///		[UserId]	varchar(50),
        ///		[AllowScriptBlock] bit DEFAULT (1),
        ///		[AllowBlockUpdateId] varchar(50)
        ///	)
        ///	
        ///	CREATE NONCLUSTERED INDEX IX_SqlBuild_Logging ON dbo.SqlBuild_Lo [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string LoggingTable {
            get {
                return ResourceManager.GetString("LoggingTable", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to --Add extra index to improve hash check performance
        ///IF NOT  EXISTS (SELECT 1 FROM sysindexes WHERE name = &apos;IX_SqlBuild_Logging_CommitCheck&apos;  AND OBJECT_NAME(id) = N&apos;SqlBuild_Logging&apos;)
        ///BEGIN
        ///	 CREATE NONCLUSTERED INDEX [IX_SqlBuild_Logging_CommitCheck] ON [dbo].[SqlBuild_Logging] 
        ///	(
        ///		[ScriptId] ASC,
        ///		[CommitDate] DESC
        ///	)
        ///	INCLUDE ( [ScriptFileHash],	[AllowScriptBlock]) 
        ///	WITH (SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, IGNORE_DUP_KEY = OFF, ONLINE = OFF) ON [PRIMARY]
        ///
        ///END
        ///.
        /// </summary>
        internal static string LoggingTableCommitCheckIndex {
            get {
                return ResourceManager.GetString("LoggingTableCommitCheckIndex", resourceCulture);
            }
        }
        
        internal static byte[] multi_query {
            get {
                object obj = ResourceManager.GetObject("multi_query", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot;?&gt;
        ///&lt;ArrayOfServerData xmlns:xsi=&quot;http://www.w3.org/2001/XMLSchema-instance&quot; xmlns:xsd=&quot;http://www.w3.org/2001/XMLSchema&quot;&gt;
        ///  &lt;ServerData&gt;
        ///    &lt;ServerName&gt;Server1\Instance_1&lt;/ServerName&gt;
        ///    &lt;OverrideSequence&gt;
        ///      &lt;item&gt;
        ///        &lt;key&gt;
        ///          &lt;string&gt;0&lt;/string&gt;
        ///        &lt;/key&gt;
        ///        &lt;value&gt;
        ///          &lt;ArrayOfDatabaseOverride&gt;
        ///            &lt;DatabaseOverride&gt;
        ///              &lt;DefaultDbTarget&gt;Default&lt;/DefaultDbTarget&gt;
        ///              &lt;OverrideDbTarget&gt;Db_0001&lt;/Ove [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string MultiDb_WithQueryRowData {
            get {
                return ResourceManager.GetString("MultiDb_WithQueryRowData", resourceCulture);
            }
        }
        
        internal static byte[] NoTrans_MultiDb {
            get {
                object obj = ResourceManager.GetObject("NoTrans_MultiDb", resourceCulture);
                return ((byte[])(obj));
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ExeServer1
        ///ExeServer2.
        /// </summary>
        internal static string remote_server_list {
            get {
                return ResourceManager.GetString("remote_server_list", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ExeServer3           
        ///ExeServer4						.
        /// </summary>
        internal static string remote_server_list_with_spaces {
            get {
                return ResourceManager.GetString("remote_server_list_with_spaces", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to ExeServer5           
        ///ExeServer6						
        ///
        ///
        ///
        ///.
        /// </summary>
        internal static string remote_server_list_with_spaces_and_blank_lines {
            get {
                return ResourceManager.GetString("remote_server_list_with_spaces_and_blank_lines", resourceCulture);
            }
        }
        
        internal static byte[] sbx_package_tester {
            get {
                object obj = ResourceManager.GetObject("sbx_package_tester", resourceCulture);
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
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; standalone=&quot;yes&quot;?&gt;
        ///&lt;SqlSyncBuildData xmlns=&quot;http://INVALID.mckechney.com/SqlSyncBuildProject.xsd&quot;&gt;
        ///  &lt;SqlSyncBuildProject ProjectName=&quot;&quot; ScriptTagRequired=&quot;false&quot;&gt;
        ///    &lt;Scripts&gt;
        ///      &lt;Script FileName=&quot;Create table.sql&quot; BuildOrder=&quot;3&quot; Description=&quot;&quot; RollBackOnError=&quot;true&quot; CausesBuildFailure=&quot;true&quot; DateAdded=&quot;2008-06-26T14:28:15.2685768-04:00&quot; ScriptId=&quot;bd58d42b-51f9-4052-85c8-44f247a690f4&quot; Database=&quot;Client&quot; StripTransactionText=&quot;true&quot; AllowMultipleRuns=&quot;true&quot; AddedBy=&quot;mmckechn&quot; Scrip [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string XmlWithInvalidNamespace {
            get {
                return ResourceManager.GetString("XmlWithInvalidNamespace", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; standalone=&quot;yes&quot;?&gt;
        ///&lt;SqlSyncBuildData&gt;
        ///  &lt;SqlSyncBuildProject ProjectName=&quot;&quot; ScriptTagRequired=&quot;false&quot;&gt;
        ///    &lt;Scripts&gt;
        ///      &lt;Script FileName=&quot;Create table.sql&quot; BuildOrder=&quot;3&quot; Description=&quot;&quot; RollBackOnError=&quot;true&quot; CausesBuildFailure=&quot;true&quot; DateAdded=&quot;2008-06-26T14:28:15.2685768-04:00&quot; ScriptId=&quot;bd58d42b-51f9-4052-85c8-44f247a690f4&quot; Database=&quot;Client&quot; StripTransactionText=&quot;true&quot; AllowMultipleRuns=&quot;true&quot; AddedBy=&quot;mmckechn&quot; ScriptTimeOut=&quot;20&quot; DateModified=&quot;2008-08-27T12:53:18.9312747-04:00 [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string XmlWithNoNamespace {
            get {
                return ResourceManager.GetString("XmlWithNoNamespace", resourceCulture);
            }
        }
    }
}
