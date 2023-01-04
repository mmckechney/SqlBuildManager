namespace SqlSync.SprocTest.Configuration
{
    public partial class Parameter
    {
        [System.Xml.Serialization.XmlIgnore]
        private bool hasDerivedParameterMatch = false;

        [System.Xml.Serialization.XmlIgnore]
        public bool HasDerivedParameterMatch
        {
            get { return hasDerivedParameterMatch; }
            set { hasDerivedParameterMatch = value; }
        }


    }
}
