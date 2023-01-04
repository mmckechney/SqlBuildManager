using System;
using System.IO;
namespace SqlSync.TableScript
{
    /// <summary>
    /// Summary description for ResourceHelper.
    /// </summary>
    public class ResourceHelper
    {
        internal string GetFromResources(string resourceName)
        {
            System.Reflection.Assembly assem = GetType().Assembly;
            using (System.IO.Stream stream = assem.GetManifestResourceStream(resourceName))
            {
                try
                {
                    using (System.IO.StreamReader reader = new StreamReader(stream))
                    {
                        return reader.ReadToEnd();
                    }
                }
                catch (Exception e)
                {
                    throw new Exception("Error retrieving from Resources. Tried '"
                        + resourceName + "'\r\n" + e.ToString());
                }
            }
        }
    }
}
