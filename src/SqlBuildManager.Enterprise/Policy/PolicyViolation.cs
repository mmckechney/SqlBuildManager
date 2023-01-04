using System;
using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
namespace SqlBuildManager.Enterprise.Policy
{

    [Serializable()]
    [XmlRoot("PackageInventory")]
    public class PackageInventory : List<Package>
    {
        public PackageInventory() { }
        public PackageInventory(string inventoryDate)
           : this()
        {
            InventoryDate = inventoryDate;
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
            PackageName = packageName;
        }
        public Package(Script[] scripts)
        {
            AddRange(scripts);
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
            ScriptName = scriptName;
            Guid = scriptGuid;
        }
        public void AddViolation(Violation item)
        {
            violations.Add(item);
        }
        public int Count
        {
            get
            {
                return violations.Count;
            }
        }
        public Violation this[int index]
        {
            get
            {
                return violations[index];
            }
            set
            {
                violations[index] = value;
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
                return violations;
            }
            set
            {
                violations = value;
            }
        }

    }

    [Serializable()]
    public class Violation
    {
        public Violation()
        {
        }
        public Violation(string violationName, string message, string severity) : this()
        {
            Name = violationName;
            Message = message;
            Severity = severity;
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
