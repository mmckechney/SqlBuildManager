﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.1
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SqlBuildManager.Enterprise.UnitTest.Properties {
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
                    global::System.Resources.ResourceManager temp = new global::System.Resources.ResourceManager("SqlBuildManager.Enterprise.UnitTest.Properties.Resources", typeof(Resources).Assembly);
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
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;DefaultScriptRegistry xmlns=&quot;http://schemas.mckechney.com/DefaultScriptRegistry.xsd&quot;&gt;
        ///	&lt;DefaultScript ScriptName=&quot;Versions Table Update.sql&quot; AllowMultipleRuns=&quot;true&quot; RollBackBuild=&quot;true&quot; Description=&quot;Updates versions table&quot;
        ///				   BuildOrder=&quot;1000&quot; StripTransactions=&quot;true&quot; /&gt;
        ///&lt;/DefaultScriptRegistry&gt; 
        ///.
        /// </summary>
        internal static string DefaultScriptRegistry {
            get {
                return ResourceManager.GetString("DefaultScriptRegistry", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;DefaultScriptRegistry xmlns=&quot;http://schemas.mckechney.com/DefaultScriptRegistry.xsd&quot;&gt;
        ///	&lt;DefaultScript ScriptName=&quot;{0}&quot; AllowMultipleRuns=&quot;true&quot; RollBackBuild=&quot;true&quot; Description=&quot;Updates versions table&quot;
        ///				   BuildOrder=&quot;1000&quot; StripTransactions=&quot;true&quot; /&gt;
        ///&lt;/DefaultScriptRegistry&gt; .
        /// </summary>
        internal static string DefaultScriptRegistryWithToken {
            get {
                return ResourceManager.GetString("DefaultScriptRegistryWithToken", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to &lt;?xml version=&quot;1.0&quot; encoding=&quot;utf-8&quot; ?&gt;
        ///&lt;EnterpriseConfiguration xmlns=&quot;http://www.mckechney.com/EnterpriseConfiguration.xsd&quot;&gt;
        ///	&lt;TableWatch Description=&quot;Sample Table Watch&quot; EmailBody=&quot;One of your watched tables has changed&quot; EmailSubject=&quot;Alert Notice&quot;&gt;
        ///		&lt;Table Name=&quot;SqlBuild_logging&quot; /&gt;
        ///		&lt;Table Name=&quot;TransactionTest&quot; /&gt;
        ///		&lt;Notify EMail=&quot;michael@mckechney.com&quot; Name=&quot;Michael McKechney&quot;/&gt;
        ///		&lt;Notify EMail=&quot;help@sqlbuildmanager.com&quot; Name=&quot;Sql Build Admin&quot;/&gt;
        ///	&lt;/TableWatch&gt;
        ///	
        ///&lt;/EnterpriseConfiguration&gt;        /// [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string EnterpriseConfig {
            get {
                return ResourceManager.GetString("EnterpriseConfig", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to USE [AdventureWorks]
        ///GO
        ////****** Object:  StoredProcedure [dbo].[uspGetBillOfMaterials]    Script Date: 03/31/2009 11:31:33 ******/
        ///SET ANSI_NULLS ON
        ///GO
        ///SET QUOTED_IDENTIFIER ON
        ///GO
        ///
        ///ALTER PROCEDURE [dbo].[uspGetBillOfMaterials]
        ///    @StartProductID [int],
        ///    @CheckDate [datetime]
        ///AS
        ///BEGIN
        ///    SET NOCOUNT ON;
        ///
        ///    -- Use recursive query to generate a multi-level Bill of Material (i.e. all level 1 
        ///    -- components of a level 0 assembly, all level 2 components of a level 1 assembly)
        ///    -- Th [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string MissingQualifierBeforeGroupBy {
            get {
                return ResourceManager.GetString("MissingQualifierBeforeGroupBy", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to USE [AdventureWorks]
        ///GO
        ////****** Object:  StoredProcedure [dbo].[uspGetBillOfMaterials]    Script Date: 03/31/2009 11:31:33 ******/
        ///SET ANSI_NULLS ON
        ///GO
        ///SET QUOTED_IDENTIFIER ON
        ///GO
        ///
        ///ALTER PROCEDURE [dbo].[uspGetBillOfMaterials]
        ///    @StartProductID [int],
        ///    @CheckDate [datetime]
        ///AS
        ///BEGIN
        ///    SET NOCOUNT ON;
        ///
        ///    -- Use recursive query to generate a multi-level Bill of Material (i.e. all level 1 
        ///    -- components of a level 0 assembly, all level 2 components of a level 1 assembly)
        ///    -- Th [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string MissingQualifierWithBrackets {
            get {
                return ResourceManager.GetString("MissingQualifierWithBrackets", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to USE [AdventureWorks]
        ///GO
        ////****** Object:  StoredProcedure [dbo].[uspGetBillOfMaterials]    Script Date: 03/31/2009 11:31:33 ******/
        ///SET ANSI_NULLS ON
        ///GO
        ///SET QUOTED_IDENTIFIER ON
        ///GO
        ///
        ///ALTER PROCEDURE [dbo].[uspGetBillOfMaterials]
        ///    @StartProductID [int],
        ///    @CheckDate [datetime]
        ///AS
        ///BEGIN
        ///    SET NOCOUNT ON;
        ///
        ///    -- Use recursive query to generate a multi-level Bill of Material (i.e. all level 1 
        ///    -- components of a level 0 assembly, all level 2 components of a level 1 assembly)
        ///    -- Th [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string NoGrant {
            get {
                return ResourceManager.GetString("NoGrant", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to USE [AdventureWorks]
        ///GO
        ////****** Object:  StoredProcedure [dbo].[uspGetBillOfMaterials]    Script Date: 03/31/2009 11:31:33 ******/
        ///SET ANSI_NULLS ON
        ///GO
        ///SET QUOTED_IDENTIFIER ON
        ///GO
        ///
        ///ALTER PROCEDURE [dbo].[uspGetBillOfMaterials]
        ///    @StartProductID [int],
        ///    @CheckDate [datetime]
        ///AS
        ///BEGIN
        ///    SET NOCOUNT ON;
        ///
        ///    -- Use recursive query to generate a multi-level Bill of Material (i.e. all level 1 
        ///    -- components of a level 0 assembly, all level 2 components of a level 1 assembly)
        ///    -- Th [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string NonPublicGrant {
            get {
                return ResourceManager.GetString("NonPublicGrant", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to IF EXISTS(SELECT 1 FROM INFORMATION_SCHEMA.ROUTINES WHERE ROUTINE_NAME = &apos;MyTestProcedure&apos; AND ROUTINE_SCHEMA = &apos;dbo&apos; AND ROUTINE_TYPE = &apos;PROCEDURE&apos;)
        ///BEGIN
        ///	DROP PROCEDURE [dbo].[MyTestProcedure]
        ///END
        ///GO
        ///
        ///CREATE PROCEDURE [dbo].[MyTestProcedure]
        ///    @StartProductID [int],
        ///    @CheckDate [datetime]
        ///AS
        ///BEGIN
        ///	/******************************************************************************
        ///	**		Name: [MyTestProcedure]
        ///	**		Desc: Sample Procedure for Unit test
        ///	**              
        ///	**		Auth: me
        ///	**		 [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string PolicyHelper_NoViolations {
            get {
                return ResourceManager.GetString("PolicyHelper_NoViolations", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to CREATE PROCEDURE [dbo].[MyTestProcedure]
        ///    @StartProductID [int],
        ///    @CheckDate [datetime]
        ///AS
        ///BEGIN
        ///	/******************************************************************************
        ///	**		Name: [MyTestProcedure]
        ///	**		Desc: Sample Procedure for Unit test
        ///	**              
        ///	**		Auth: me
        ///	**		Date: 1/12/2009
        ///	*******************************************************************************
        ///	**		Change History
        ///	*******************************************************************************
        ///	**		Dat [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string PolicyHelper_WithViolations {
            get {
                return ResourceManager.GetString("PolicyHelper_WithViolations", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to USE [AdventureWorks]
        ///GO
        ////****** Object:  StoredProcedure [dbo].[uspGetBillOfMaterials]    Script Date: 03/31/2009 11:31:33 ******/
        ///SET ANSI_NULLS ON
        ///GO
        ///SET QUOTED_IDENTIFIER ON
        ///GO
        ///
        ///ALTER PROCEDURE [dbo].[uspGetBillOfMaterials]
        ///    @StartProductID [int],
        ///    @CheckDate [datetime]
        ///AS
        ///BEGIN
        ///    SET NOCOUNT ON;
        ///
        ///    -- Use recursive query to generate a multi-level Bill of Material (i.e. all level 1 
        ///    -- components of a level 0 assembly, all level 2 components of a level 1 assembly)
        ///    -- Th [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string PublicGrant {
            get {
                return ResourceManager.GetString("PublicGrant", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to USE [AdventureWorks]
        ///GO
        ////****** Object:  StoredProcedure [dbo].[uspGetBillOfMaterials]    Script Date: 03/31/2009 11:31:33 ******/
        ///SET ANSI_NULLS ON
        ///GO
        ///SET QUOTED_IDENTIFIER ON
        ///GO
        ///
        ///ALTER PROCEDURE [dbo].[uspGetBillOfMaterials]
        ///    @StartProductID [int],
        ///    @CheckDate [datetime]
        ///AS
        ///BEGIN
        ///    SET NOCOUNT ON;
        ///
        ///    -- Use recursive query to generate a multi-level Bill of Material (i.e. all level 1 
        ///    -- components of a level 0 assembly, all level 2 components of a level 1 assembly)
        ///    -- Th [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string PublicGrant_nobracket {
            get {
                return ResourceManager.GetString("PublicGrant_nobracket", resourceCulture);
            }
        }
        
        /// <summary>
        ///   Looks up a localized string similar to USE [AdventureWorks]
        ///GO
        ////****** Object:  StoredProcedure [dbo].[uspGetBillOfMaterials]    Script Date: 03/31/2009 11:31:33 ******/
        ///SET ANSI_NULLS ON
        ///GO
        ///SET QUOTED_IDENTIFIER ON
        ///GO
        ///
        ///ALTER PROCEDURE [dbo].[uspGetBillOfMaterials]
        ///    @StartProductID [int],
        ///    @CheckDate [datetime]
        ///AS
        ///BEGIN
        ///    SET NOCOUNT ON;
        ///
        ///    -- Use recursive query to generate a multi-level Bill of Material (i.e. all level 1 
        ///    -- components of a level 0 assembly, all level 2 components of a level 1 assembly)
        ///    -- Th [rest of string was truncated]&quot;;.
        /// </summary>
        internal static string QualifiedWithBrackets {
            get {
                return ResourceManager.GetString("QualifiedWithBrackets", resourceCulture);
            }
        }
    }
}
