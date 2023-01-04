namespace SqlSync.SprocTest.Configuration
{
    partial class TestCase
    {
        [System.Xml.Serialization.XmlIgnore]
        private bool selectedForRun = false;

        [System.Xml.Serialization.XmlIgnore]
        public bool SelectedForRun
        {
            get { return selectedForRun; }
            set { selectedForRun = value; }
        }
    }
}
