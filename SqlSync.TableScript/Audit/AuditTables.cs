﻿//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Runtime Version:2.0.50727.42
//
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

// 
// This source code was auto-generated by xsd, Version=2.0.50727.42.
// 
namespace SqlSync.TableScript.Audit {
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://www.mckechney.com/AuditTables.xsd", IsNullable=false)]
    public partial class SQLSyncAuditing {
        
        private SQLSyncAuditingDatabase[] itemsField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Database")]
        public SQLSyncAuditingDatabase[] Items {
            get {
                return this.itemsField;
            }
            set {
                this.itemsField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class SQLSyncAuditingDatabase {
        
        private SQLSyncAuditingDatabaseTableToAudit[] tableToAuditField;
        
        private string nameField;
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("TableToAudit")]
        public SQLSyncAuditingDatabaseTableToAudit[] TableToAudit {
            get {
                return this.tableToAuditField;
            }
            set {
                this.tableToAuditField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "2.0.50727.42")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(AnonymousType=true)]
    public partial class SQLSyncAuditingDatabaseTableToAudit {
        
        private string nameField;
        
        private string insertByColumnField;
        
        private string individualIDColumnField;
        
        private string objectTypeColumnField;
        
        public SQLSyncAuditingDatabaseTableToAudit() {
            this.insertByColumnField = "";
            this.individualIDColumnField = "";
            this.objectTypeColumnField = "";
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string Name {
            get {
                return this.nameField;
            }
            set {
                this.nameField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute("")]
        public string InsertByColumn {
            get {
                return this.insertByColumnField;
            }
            set {
                this.insertByColumnField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute("")]
        public string IndividualIDColumn {
            get {
                return this.individualIDColumnField;
            }
            set {
                this.individualIDColumnField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute("")]
        public string ObjectTypeColumn {
            get {
                return this.objectTypeColumnField;
            }
            set {
                this.objectTypeColumnField = value;
            }
        }
    }
}
