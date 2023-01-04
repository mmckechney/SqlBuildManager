namespace SqlSync.SprocTest.Configuration
{
    public partial class StoredProcedure
    {
        [System.Xml.Serialization.XmlIgnore]
        private Microsoft.Data.SqlClient.SqlParameterCollection derivedParameters = null;

        [System.Xml.Serialization.XmlIgnore]
        public Microsoft.Data.SqlClient.SqlParameterCollection DerivedParameters
        {
            get
            {
                return derivedParameters;
            }
            set
            {
                derivedParameters = value;
            }
        }
        public override string ToString()
        {
            return Name;
        }
    }
}
