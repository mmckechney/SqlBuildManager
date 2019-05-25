using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.Xml.Serialization;
using System.Data.SqlClient;
namespace SqlSync.SprocTest.Configuration
{
    public partial class StoredProcedure
    {
        [System.Xml.Serialization.XmlIgnore]
        private System.Data.SqlClient.SqlParameterCollection derivedParameters = null;

        [System.Xml.Serialization.XmlIgnore]
        public System.Data.SqlClient.SqlParameterCollection DerivedParameters
        {
            get
            {
                return this.derivedParameters;
            }
            set
            {
                this.derivedParameters = value;
            }
        }
        public override string ToString()
        {
            return this.Name;
        }
    }
}
