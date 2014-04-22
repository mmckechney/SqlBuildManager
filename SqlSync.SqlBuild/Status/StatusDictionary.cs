using System;
using System.Collections.Generic;
using System.Text;
using System.Xml.Serialization;

///  Code courtesy of Paul Welter @ http://weblogs.asp.net/pwelter34/archive/2006/05/03/444961.aspx
namespace SqlSync.SqlBuild.Status
{



    [XmlRoot("dictionary")]

    public class StatusDictionary<TValue> : Dictionary<string, TValue>, IXmlSerializable
    {
        private string keyName = "key";

        public string KeyName
        {
            get { return keyName; }
            set { keyName = value; }
        }
        private string valueName = "value";

        public string ValueName
        {
            get { return valueName; }
            set { valueName = value; }
        }

        private string itemName = "item";

        public string ItemName
        {
            get { return itemName; }
            set { itemName = value; }
        }
        public StatusDictionary(string itemName, string keyName, string valueName)
        {
            this.keyName = keyName;
            this.valueName = valueName;
            this.itemName = itemName;
        }
        public StatusDictionary()
        {
        }
        #region IXmlSerializable Members

        public System.Xml.Schema.XmlSchema GetSchema()
        {

            return null;

        }



        public void ReadXml(System.Xml.XmlReader reader)
        {

            XmlSerializer keySerializer = new XmlSerializer(typeof(string));
            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));

            bool wasEmpty = reader.IsEmptyElement;

            reader.Read();

            if (wasEmpty)
                return;

            while (reader.NodeType != System.Xml.XmlNodeType.EndElement)
            {

                reader.ReadStartElement(this.itemName);
                reader.ReadStartElement(this.keyName);
                string key = (string)keySerializer.Deserialize(reader);
                reader.ReadStartElement(this.valueName);

                TValue value = (TValue)valueSerializer.Deserialize(reader);

                reader.ReadEndElement();

                this.Add(key, value);

                reader.ReadEndElement();
                reader.MoveToContent();

            }

            reader.ReadEndElement();

        }



        public void WriteXml(System.Xml.XmlWriter writer)
        {

            XmlSerializer valueSerializer = new XmlSerializer(typeof(TValue));



            foreach (string key in this.Keys)
            {

                writer.WriteStartElement(this.itemName);
                writer.WriteAttributeString("Name", key);

                TValue value = this[key];

                valueSerializer.Serialize(writer, value);
                writer.WriteEndElement();

            }

        }

    }

        #endregion

}



