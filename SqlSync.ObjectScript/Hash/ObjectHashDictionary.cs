using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

///  Code courtesy of Paul Welter @ http://weblogs.asp.net/pwelter34/archive/2006/05/03/444961.aspx
namespace SqlSync.ObjectScript.Hash
{



    [XmlRoot("dictionary")]

    public class ObjectHashDictionary : Dictionary<string, HashSet>, IXmlSerializable
    {

        public void ResetComparisonValues()
        {
            foreach (HashSet set in this.Values)
            {
                set.ComparisonValue = "Added";
            }
        }
        private string itemName = "item";

        public string ItemName
        {
            get { return itemName; }
            set { itemName = value; }
        }
        public ObjectHashDictionary(string itemName)
        {
            this.itemName = itemName;
        }
        public ObjectHashDictionary()
        {
        }

        public void Add(string objectName, string hashValue, string comparisonValue)
        {
            base.Add(objectName, new HashSet(hashValue, comparisonValue));
        }
        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {
            return null;
        }

        public void ReadXml(System.Xml.XmlReader reader)
        {
        }

        public void WriteXml(System.Xml.XmlWriter writer)
        {

            XmlSerializer valueSerializer = new XmlSerializer(typeof(string));

            foreach (string key in this.Keys)
            {

                writer.WriteStartElement(this.itemName);
                writer.WriteAttributeString("Name", key);
                writer.WriteAttributeString("HashValue", this[key].HashValue);
                writer.WriteAttributeString("ComparisonValue", this[key].ComparisonValue);
                writer.WriteEndElement();

            }

        }

    }

    

        #endregion
    public class HashSet
    {
        public HashSet(string hashValue, string comparisonValue)
        {
            this.hashValue = hashValue;
            this.comparisonValue = comparisonValue;
        }
        private string hashValue;

        public string HashValue
        {
            get { return hashValue; }
            set { hashValue = value; }
        }
        private string comparisonValue;

        public string ComparisonValue
        {
            get { return comparisonValue; }
            set { comparisonValue = value; }
        }

    }
}



