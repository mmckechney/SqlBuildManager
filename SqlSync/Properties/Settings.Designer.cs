﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace SqlSync.Properties {
    
    
    [global::System.Runtime.CompilerServices.CompilerGeneratedAttribute()]
    [global::System.CodeDom.Compiler.GeneratedCodeAttribute("Microsoft.VisualStudio.Editors.SettingsDesigner.SettingsSingleFileGenerator", "14.0.0.0")]
    internal sealed partial class Settings : global::System.Configuration.ApplicationSettingsBase {
        
        private static Settings defaultInstance = ((Settings)(global::System.Configuration.ApplicationSettingsBase.Synchronized(new Settings())));
        
        public static Settings Default {
            get {
                return defaultInstance;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ScriptPermissions {
            get {
                return ((bool)(this["ScriptPermissions"]));
            }
            set {
                this["ScriptPermissions"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ScriptAsAlter {
            get {
                return ((bool)(this["ScriptAsAlter"]));
            }
            set {
                this["ScriptAsAlter"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Windows.Forms.AutoCompleteStringCollection CmdLineMultiDbFileNameList {
            get {
                return ((global::System.Windows.Forms.AutoCompleteStringCollection)(this["CmdLineMultiDbFileNameList"]));
            }
            set {
                this["CmdLineMultiDbFileNameList"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Windows.Forms.AutoCompleteStringCollection CmdLineSbmFileNameList {
            get {
                return ((global::System.Windows.Forms.AutoCompleteStringCollection)(this["CmdLineSbmFileNameList"]));
            }
            set {
                this["CmdLineSbmFileNameList"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Windows.Forms.AutoCompleteStringCollection CmdLineRootLoggingPath {
            get {
                return ((global::System.Windows.Forms.AutoCompleteStringCollection)(this["CmdLineRootLoggingPath"]));
            }
            set {
                this["CmdLineRootLoggingPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Windows.Forms.AutoCompleteStringCollection Description {
            get {
                return ((global::System.Windows.Forms.AutoCompleteStringCollection)(this["Description"]));
            }
            set {
                this["Description"] = value;
            }
        }
        
        [global::System.Configuration.ApplicationScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("i:\\mmckechney\\Sql Build Manager\\Enterprise Settings")]
        public string EnterpriseSettingsPath {
            get {
                return ((string)(this["EnterpriseSettingsPath"]));
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string LastRemoteExecutionConfigFile {
            get {
                return ((string)(this["LastRemoteExecutionConfigFile"]));
            }
            set {
                this["LastRemoteExecutionConfigFile"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("500")]
        public int DefaultMinimumScriptTimeout {
            get {
                return ((int)(this["DefaultMinimumScriptTimeout"]));
            }
            set {
                this["DefaultMinimumScriptTimeout"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Windows.Forms.AutoCompleteStringCollection RemoteLoggingPath {
            get {
                return ((global::System.Windows.Forms.AutoCompleteStringCollection)(this["RemoteLoggingPath"]));
            }
            set {
                this["RemoteLoggingPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Windows.Forms.AutoCompleteStringCollection RemoteBuildDescription {
            get {
                return ((global::System.Windows.Forms.AutoCompleteStringCollection)(this["RemoteBuildDescription"]));
            }
            set {
                this["RemoteBuildDescription"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Windows.Forms.AutoCompleteStringCollection RemoteTargetOverrideSettings {
            get {
                return ((global::System.Windows.Forms.AutoCompleteStringCollection)(this["RemoteTargetOverrideSettings"]));
            }
            set {
                this["RemoteTargetOverrideSettings"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        public global::System.Windows.Forms.AutoCompleteStringCollection RemoteUsername {
            get {
                return ((global::System.Windows.Forms.AutoCompleteStringCollection)(this["RemoteUsername"]));
            }
            set {
                this["RemoteUsername"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("False")]
        public bool RequireScriptTags {
            get {
                return ((bool)(this["RequireScriptTags"]));
            }
            set {
                this["RequireScriptTags"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("A script Tag are required. Please add.")]
        public string RequireScriptTagsMessage {
            get {
                return ((string)(this["RequireScriptTagsMessage"]));
            }
            set {
                this["RequireScriptTagsMessage"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("<?xml version=\"1.0\" encoding=\"utf-16\"?>\r\n<ArrayOfString xmlns:xsi=\"http://www.w3." +
            "org/2001/XMLSchema-instance\" xmlns:xsd=\"http://www.w3.org/2001/XMLSchema\">\r\n  <s" +
            "tring>\\bCR *-*#* *\\d{3,10}</string>\r\n  <string>\\bP *-*#* *\\d{3,10}</string>\r\n</A" +
            "rrayOfString>")]
        public global::System.Collections.Specialized.StringCollection TagInferenceRegexList {
            get {
                return ((global::System.Collections.Specialized.StringCollection)(this["TagInferenceRegexList"]));
            }
            set {
                this["TagInferenceRegexList"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("White")]
        public global::System.Drawing.Color DiffForegroundColor {
            get {
                return ((global::System.Drawing.Color)(this["DiffForegroundColor"]));
            }
            set {
                this["DiffForegroundColor"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Maroon")]
        public global::System.Drawing.Color DiffBackgroundColor {
            get {
                return ((global::System.Drawing.Color)(this["DiffBackgroundColor"]));
            }
            set {
                this["DiffBackgroundColor"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("True")]
        public bool ScriptPkWithTables {
            get {
                return ((bool)(this["ScriptPkWithTables"]));
            }
            set {
                this["ScriptPkWithTables"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("http://tfspnv01:8080/tfs")]
        public string SourceControlServerUrl {
            get {
                return ((string)(this["SourceControlServerUrl"]));
            }
            set {
                this["SourceControlServerUrl"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("LightBlue")]
        public global::System.Drawing.Color ScriptDontStripTransactions {
            get {
                return ((global::System.Drawing.Color)(this["ScriptDontStripTransactions"]));
            }
            set {
                this["ScriptDontStripTransactions"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Thistle")]
        public global::System.Drawing.Color ScriptAllowMultipleRunsAndLeaveTransactions {
            get {
                return ((global::System.Drawing.Color)(this["ScriptAllowMultipleRunsAndLeaveTransactions"]));
            }
            set {
                this["ScriptAllowMultipleRunsAndLeaveTransactions"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("BlanchedAlmond")]
        public global::System.Drawing.Color ScriptWillBeSkippedMarkedAsRunOnce {
            get {
                return ((global::System.Drawing.Color)(this["ScriptWillBeSkippedMarkedAsRunOnce"]));
            }
            set {
                this["ScriptWillBeSkippedMarkedAsRunOnce"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("Gray")]
        public global::System.Drawing.Color ScriptReadOnly {
            get {
                return ((global::System.Drawing.Color)(this["ScriptReadOnly"]));
            }
            set {
                this["ScriptReadOnly"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("DarkSalmon")]
        public global::System.Drawing.Color ScriptAllowMultipleRuns {
            get {
                return ((global::System.Drawing.Color)(this["ScriptAllowMultipleRuns"]));
            }
            set {
                this["ScriptAllowMultipleRuns"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("")]
        public string DACPACPath {
            get {
                return ((string)(this["DACPACPath"]));
            }
            set {
                this["DACPACPath"] = value;
            }
        }
        
        [global::System.Configuration.UserScopedSettingAttribute()]
        [global::System.Diagnostics.DebuggerNonUserCodeAttribute()]
        [global::System.Configuration.DefaultSettingValueAttribute("-1")]
        public int DBAuthenticationType {
            get {
                return ((int)(this["DBAuthenticationType"]));
            }
            set {
                this["DBAuthenticationType"] = value;
            }
        }
    }
}
