using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

///  Code courtesy of Paul Welter @ http://weblogs.asp.net/pwelter34/archive/2006/05/03/444961.aspx
namespace SqlSync.SqlBuild.AdHocQuery
{



    [XmlRoot("dictionary")]

    public class ResultsDictionary : Dictionary<string, string>, IXmlSerializable
    {

        
        private string itemName = "item";

        public string ItemName
        {
            get { return itemName; }
            set { itemName = value; }
        }
        public ResultsDictionary(string itemName)
        {
            this.itemName = itemName;
        }
        public ResultsDictionary()
        {
        }

        public new void Add(string columnName, string value)
        {
            base.Add(columnName, value);
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
                writer.WriteAttributeString("Value", this[key]);
                writer.WriteEndElement();

            }

        }

    }

    

        #endregion
  
}



