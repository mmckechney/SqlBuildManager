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
namespace SqlSync.SprocTest.Configuration {
    using System.Xml.Serialization;
    
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.mckechney.com/SprocTest.xsd")]
    [System.Xml.Serialization.XmlRootAttribute(Namespace="http://schemas.mckechney.com/SprocTest.xsd", IsNullable=false)]
    public partial class Database {
        
        private StoredProcedure[] storedProcedureField;
        
        private string nameField;
        
        public Database() {
            this.nameField = "";
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("StoredProcedure")]
        public StoredProcedure[] StoredProcedure {
            get {
                return this.storedProcedureField;
            }
            set {
                this.storedProcedureField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute("")]
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
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.mckechney.com/SprocTest.xsd")]
    public partial class StoredProcedure {
        
        private TestCase[] testCaseField;
        
        private string nameField;
        
        private string schemaOwnerField;
        
        private string idField;
        
        public StoredProcedure() {
            this.nameField = "";
            this.schemaOwnerField = "dbo";
            this.idField = "";
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("TestCase")]
        public TestCase[] TestCase {
            get {
                return this.testCaseField;
            }
            set {
                this.testCaseField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute("")]
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
        [System.ComponentModel.DefaultValueAttribute("dbo")]
        public string SchemaOwner {
            get {
                return this.schemaOwnerField;
            }
            set {
                this.schemaOwnerField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute("")]
        public string ID {
            get {
                return this.idField;
            }
            set {
                this.idField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.mckechney.com/SprocTest.xsd")]
    public partial class TestCase {
        
        private Parameter[] parameterField;
        
        private ExpectedResult expectedResultField;
        
        private string nameField;
        
        private ExecuteType executeTypeField;
        
        private bool executeTypeFieldSpecified;
        
        private string testCaseIdField;
        
        private string createdByField;
        
        private System.DateTime createdDateField;
        
        private bool createdDateFieldSpecified;
        
        private string modifiedByField;
        
        private System.DateTime modifiedDateField;
        
        private bool modifiedDateFieldSpecified;
        
        public TestCase() {
            this.nameField = "";
            this.createdByField = "";
            this.modifiedByField = "";
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("Parameter")]
        public Parameter[] Parameter {
            get {
                return this.parameterField;
            }
            set {
                this.parameterField = value;
            }
        }
        
        /// <remarks/>
        public ExpectedResult ExpectedResult {
            get {
                return this.expectedResultField;
            }
            set {
                this.expectedResultField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute("")]
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
        public ExecuteType ExecuteType {
            get {
                return this.executeTypeField;
            }
            set {
                this.executeTypeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ExecuteTypeSpecified {
            get {
                return this.executeTypeFieldSpecified;
            }
            set {
                this.executeTypeFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public string TestCaseId {
            get {
                return this.testCaseIdField;
            }
            set {
                this.testCaseIdField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute("")]
        public string CreatedBy {
            get {
                return this.createdByField;
            }
            set {
                this.createdByField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime CreatedDate {
            get {
                return this.createdDateField;
            }
            set {
                this.createdDateField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool CreatedDateSpecified {
            get {
                return this.createdDateFieldSpecified;
            }
            set {
                this.createdDateFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute("")]
        public string ModifiedBy {
            get {
                return this.modifiedByField;
            }
            set {
                this.modifiedByField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public System.DateTime ModifiedDate {
            get {
                return this.modifiedDateField;
            }
            set {
                this.modifiedDateField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ModifiedDateSpecified {
            get {
                return this.modifiedDateFieldSpecified;
            }
            set {
                this.modifiedDateFieldSpecified = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.mckechney.com/SprocTest.xsd")]
    public partial class Parameter {
        
        private string nameField;
        
        private string valueField;
        
        private bool useAsQueryField;
        
        public Parameter() {
            this.nameField = "";
            this.valueField = "";
            this.useAsQueryField = false;
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute("")]
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
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool UseAsQuery {
            get {
                return this.useAsQueryField;
            }
            set {
                this.useAsQueryField = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.mckechney.com/SprocTest.xsd")]
    public partial class OutputResult {
        
        private string columnNameField;
        
        private string valueField;
        
        private int rowNumberField;
        
        private bool rowNumberFieldSpecified;
        
        public OutputResult() {
            this.columnNameField = "";
            this.valueField = "";
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute("")]
        public string ColumnName {
            get {
                return this.columnNameField;
            }
            set {
                this.columnNameField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute("")]
        public string Value {
            get {
                return this.valueField;
            }
            set {
                this.valueField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int RowNumber {
            get {
                return this.rowNumberField;
            }
            set {
                this.rowNumberField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RowNumberSpecified {
            get {
                return this.rowNumberFieldSpecified;
            }
            set {
                this.rowNumberFieldSpecified = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Diagnostics.DebuggerStepThroughAttribute()]
    [System.ComponentModel.DesignerCategoryAttribute("code")]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.mckechney.com/SprocTest.xsd")]
    public partial class ExpectedResult {
        
        private OutputResult[] outputResultField;
        
        private int rowCountField;
        
        private bool rowCountFieldSpecified;
        
        private RowCountOperator rowCountOperatorField;
        
        private bool rowCountOperatorFieldSpecified;
        
        private ResultType resultTypeField;
        
        private int columnCountField;
        
        private bool columnCountFieldSpecified;
        
        public ExpectedResult() {
            this.resultTypeField = ResultType.Success;
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlElementAttribute("OutputResult")]
        public OutputResult[] OutputResult {
            get {
                return this.outputResultField;
            }
            set {
                this.outputResultField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int RowCount {
            get {
                return this.rowCountField;
            }
            set {
                this.rowCountField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RowCountSpecified {
            get {
                return this.rowCountFieldSpecified;
            }
            set {
                this.rowCountFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public RowCountOperator RowCountOperator {
            get {
                return this.rowCountOperatorField;
            }
            set {
                this.rowCountOperatorField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool RowCountOperatorSpecified {
            get {
                return this.rowCountOperatorFieldSpecified;
            }
            set {
                this.rowCountOperatorFieldSpecified = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(ResultType.Success)]
        public ResultType ResultType {
            get {
                return this.resultTypeField;
            }
            set {
                this.resultTypeField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlAttributeAttribute()]
        public int ColumnCount {
            get {
                return this.columnCountField;
            }
            set {
                this.columnCountField = value;
            }
        }
        
        /// <remarks/>
        [System.Xml.Serialization.XmlIgnoreAttribute()]
        public bool ColumnCountSpecified {
            get {
                return this.columnCountFieldSpecified;
            }
            set {
                this.columnCountFieldSpecified = value;
            }
        }
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.mckechney.com/SprocTest.xsd")]
    public enum RowCountOperator {
        
        /// <remarks/>
        GreaterThan,
        
        /// <remarks/>
        LessThan,
        
        /// <remarks/>
        EqualTo,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.mckechney.com/SprocTest.xsd")]
    public enum ResultType {
        
        /// <remarks/>
        Success,
        
        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("Generic SqlException")]
        GenericSqlException,
        
        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("PK Violation")]
        PKViolation,
        
        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("FK Violation")]
        FKViolation,
    }
    
    /// <remarks/>
    [System.CodeDom.Compiler.GeneratedCodeAttribute("xsd", "4.8.3928.0")]
    [System.SerializableAttribute()]
    [System.Xml.Serialization.XmlTypeAttribute(Namespace="http://schemas.mckechney.com/SprocTest.xsd")]
    public enum ExecuteType {
        
        /// <remarks/>
        [System.Xml.Serialization.XmlEnumAttribute("Return Data")]
        ReturnData,
        
        /// <remarks/>
        NonQuery,
        
        /// <remarks/>
        Scalar,
    }
}
