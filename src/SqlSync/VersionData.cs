using System;

namespace SqlSync
{
    public class VersionData
    {
        Version yourVersion = null;

        public Version YourVersion
        {
            get { return yourVersion; }
            set { yourVersion = value; }
        }
        Version latestVersion = null;

        public Version LatestVersion
        {
            get { return latestVersion; }
            set { latestVersion = value; }
        }
        Version lastCompatableVersion = null;

        public Version LastCompatableVersion
        {
            get { return lastCompatableVersion; }
            set { lastCompatableVersion = value; }
        }
        string contact = string.Empty;

        public string Contact
        {
            get { return contact; }
            set { contact = value; }
        }
        string updateFolder = string.Empty;

        public string UpdateFolder
        {
            get { return updateFolder; }
            set { updateFolder = value; }
        }

        private bool manualCheck = false;

        public bool ManualCheck
        {
            get { return manualCheck; }
            set { manualCheck = value; }
        }

        private string releaseNotes = string.Empty;

        public string ReleaseNotes
        {
            get { return releaseNotes; }
            set { releaseNotes = value; }
        }

        private string contactEMail = string.Empty;

        public string ContactEMail
        {
            get { return contactEMail; }
            set { contactEMail = value; }
        }

        private bool updateFileReadError = false;

        public bool UpdateFileReadError
        {
            get { return updateFileReadError; }
            set { updateFileReadError = value; }
        }
        private bool checkIntervalElapsed = false;

        public bool CheckIntervalElapsed
        {
            get { return checkIntervalElapsed; }
            set { checkIntervalElapsed = value; }
        }

        public bool ReleaseNotesAreHtml
        {
            get;
            set;
        }

    }
}
