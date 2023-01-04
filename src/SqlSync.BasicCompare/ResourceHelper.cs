namespace SqlSync.BasicCompare
{
    using System;
    using System.IO;

    public class ResourceHelper
    {
        internal string GetFromResources(string resourceName)
        {
            string text;
            Stream manifestResourceStream = base.GetType().Assembly.GetManifestResourceStream(resourceName);
            try
            {
                using (StreamReader reader = new StreamReader(manifestResourceStream))
                {
                    text = reader.ReadToEnd();
                }
            }
            catch (Exception exception)
            {
                throw new Exception("Error retrieving from Resources. Tried '" + resourceName + "'\r\n" + exception.ToString());
            }
            finally
            {
                if (manifestResourceStream != null)
                {
                    ((IDisposable)manifestResourceStream).Dispose();
                }
            }
            return text;
        }
    }
}
