using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;
using System.Xml;
namespace SqlBuildManager.Enterprise.Policy
{

    [Serializable()]
    [XmlRoot("PackageInventory")]
    public class PackageInventory :List<Package>
    {
         public PackageInventory() { }
         public PackageInventory(string inventoryDate)
            : this()
        {
            this.InventoryDate = inventoryDate;
        }
        [XmlAttribute()]
        public string InventoryDate
        {
            get;
            set;
        }
    }

    [Serializable()]
    [XmlRoot("Package")]
    public class Package : List<Script>
    {
        public Package() { }
        public Package(string packageName)
            : this()
        {
            this.PackageName = packageName;
        }
        public Package(Script[] scripts)
        {
            this.AddRange(scripts);
        }

        [XmlAttribute()]
        public string PackageName
        {
            get;
            set;
        }
    }


    [Serializable()]
    [XmlRoot("Script")]
    public class Script
    {
        public Script()
        {
        }
        public Script(string scriptName, string scriptGuid)
            : this()
        {
            this.ScriptName = scriptName;
            this.Guid = scriptGuid;
        }
        public void AddViolation(Violation item)
        {
            this.violations.Add(item);
        }
        public int Count
        {
            get
            {
                return this.violations.Count;
            }
        }
        public Violation this[int index]
        {
            get
            {
                return this.violations[index];
            }
            set
            {
                this.violations[index] = value;
            }
        }

        List<Violation> violations = new List<Violation>();


        [XmlAttribute()]
        public string ScriptName
        {
            get;
            set;
        }
        [XmlAttribute()]
        public string Guid
        {
            get;
            set;
        }
        [XmlAttribute()]
        public string LastChangeDate
        {
            get;
            set;
        }
        [XmlAttribute()]
        public string LastChangeUserId
        {
            get;
            set;
        }

        public List<Violation> Violations
        {
            get
            {
                return this.violations;
            }
            set
            {
                this.violations = value;
            }
        }

    }

    [Serializable()]
    public class Violation
    {
        public Violation()
        {
        }
        public Violation(string violationName, string message, string severity) :this()
        {
            this.Name  = violationName;
            this.Message = message;
            this.Severity = severity;
        }


        [XmlAttribute()]
        public string Name
        {
            get;
            set;
        }

        [XmlAttribute()]
        public string Message
        {
            get;
            set;
        }
        [XmlAttribute()]
        public string Severity
        {
            get;
            set;
        }
    }

}
