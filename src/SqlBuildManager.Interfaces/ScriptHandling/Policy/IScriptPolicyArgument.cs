namespace SqlBuildManager.Interfaces.ScriptHandling.Policy
{
    public class IScriptPolicyArgument
    {
        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute("")]
        public string Name { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute("")]
        public string Value { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool IsLineException { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute(false)]
        public bool IsGlobalException { get; set; }

        [System.Xml.Serialization.XmlAttributeAttribute()]
        [System.ComponentModel.DefaultValueAttribute("")]
        public string FailureMessage { get; set; }
    }
}
