﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:4.0.30319.42000
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by xsd, Version=4.8.3928.0.
// 
namespace SqlSync.SqlBuild.Objects {
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://www.mckechney.com/ObjectUpdates.xsd")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://www.mckechney.com/ObjectUpdates.xsd", IsNullable=false)]
    public partial class ObjectUpdates {
        
        private string shortFileNameField;
        
        private string sourceObjectField;
        
        private string sourceDatabaseField;
        
        private string sourceServerField;
        
        private string objectTypeField;
        
        private bool includePermissionsField;
        
        private bool includePermissionsFieldSpecified;
        
        private bool scriptAsAlterField;
        
        private bool scriptAsAlterFieldSpecified;
        
        private bool scriptPkWithTableField;
        
        private bool scriptPkWithTableFieldSpecified;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ShortFileName {
            get {
                return this.shortFileNameField;
            }
            set {
                this.shortFileNameField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SourceObject {
            get {
                return this.sourceObjectField;
            }
            set {
                this.sourceObjectField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SourceDatabase {
            get {
                return this.sourceDatabaseField;
            }
            set {
                this.sourceDatabaseField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string SourceServer {
            get {
                return this.sourceServerField;
            }
            set {
                this.sourceServerField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string ObjectType {
            get {
                return this.objectTypeField;
            }
            set {
                this.objectTypeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool IncludePermissions {
            get {
                return this.includePermissionsField;
            }
            set {
                this.includePermissionsField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool IncludePermissionsSpecified {
            get {
                return this.includePermissionsFieldSpecified;
            }
            set {
                this.includePermissionsFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool ScriptAsAlter {
            get {
                return this.scriptAsAlterField;
            }
            set {
                this.scriptAsAlterField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ScriptAsAlterSpecified {
            get {
                return this.scriptAsAlterFieldSpecified;
            }
            set {
                this.scriptAsAlterFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public bool ScriptPkWithTable {
            get {
                return this.scriptPkWithTableField;
            }
            set {
                this.scriptPkWithTableField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ScriptPkWithTableSpecified {
            get {
                return this.scriptPkWithTableFieldSpecified;
            }
            set {
                this.scriptPkWithTableFieldSpecified = value;
            }
        }
    }
}
