using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
namespace SqlBuildManager.Enterprise
{
    public partial class EnterpriseConfiguration 
    {
    }

    public partial class TableWatch
    {
        [XmlIgnore()]
        private string script = string.Empty;

        [XmlIgnore()]
        public string Script
        {
            get { return script; }
            set { script = value; }
        }

        [XmlIgnore()]
        private List<string> foundTables = new List<string>();

        [XmlIgnore()]
        public List<string> FoundTables
        {
            get { return foundTables; }
            set { foundTables = value; }
        }
    }
}
