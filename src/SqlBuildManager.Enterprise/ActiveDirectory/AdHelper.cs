using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;
using Microsoft.Extensions.Logging;
namespace SqlBuildManager.Enterprise.ActiveDirectory
{
    public class AdHelper
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public AdHelper()
        {
        }
        public static IList<string> GetGroupMemberships(string userName)
        {
            List<string> groups = new List<string>();
            try
            {

                string distinguisheName = GetDistinguishedName(userName);
                if (distinguisheName.Length == 0)
                    return groups;


                DirectoryEntry de = new DirectoryEntry();
                using (DirectorySearcher ds = new DirectorySearcher(de))
                {
                    ds.Filter = "(&(objectClass=group)(Member=" + distinguisheName + "))";
                    ds.PropertiesToLoad.Add("name");
                    ds.PropertiesToLoad.Add("distinguishedname");
                    SearchResultCollection grpColl = ds.FindAll();
                    for (int i = 0; i < grpColl.Count; i++)
                    {
                        groups.Add(grpColl[i].Properties["name"][0].ToString());
                    }


                }


            }
            catch (Exception exe)
            {
                log.ErrorFormat("Failure to retrive groups", exe);
            }
            log.DebugFormat("Retrieved groups for {0}: {1}", userName, String.Join(", ", groups.ToArray()));

            return groups;
        }

        public static IList<string> GetMembersForGroup(string groupName)
        {
            List<string> groups = new List<string>();

            if (groupName.Length == 0)
                return groups;

            try
            {

                string distinguisheName = GetDistinguishedNameForGroup(groupName);
                if (distinguisheName.Length == 0)
                    return groups;


                DirectoryEntry de = new DirectoryEntry();
                using (DirectorySearcher ds = new DirectorySearcher(de))
                {
                    ds.Filter = String.Format("(&(objectClass=user)(memberOf={0}))", distinguisheName);
                    ds.PropertiesToLoad.Add("name");
                    ds.PropertiesToLoad.Add("distinguishedname");
                    SearchResultCollection grpColl = ds.FindAll();
                    for (int i = 0; i < grpColl.Count; i++)
                    {
                        groups.Add(grpColl[i].Properties["name"][0].ToString());
                    }


                }


            }
            catch (Exception exe)
            {
                log.ErrorFormat("Failure to retrive groups", exe);
            }
            log.DebugFormat("Retrieved members for {0}: {1}", groupName, String.Join(", ", groups.ToArray()));

            return groups;
        }
        internal static string GetDistinguishedName(string userName)
        {
            try
            {
                DirectoryEntry de = new DirectoryEntry();
                using (DirectorySearcher ds = new DirectorySearcher(de))
                {
                    ds.Filter = String.Format("(&(objectCategory=person)(objectClass=user)(samaccountname={0}))", userName);
                    ds.PropertiesToLoad.Add("distinguishedname");
                    SearchResult dnResult = ds.FindOne();
                    if (dnResult != null && dnResult.Properties.Contains("distinguishedname"))
                    {
                        string distinguishedName = dnResult.Properties["distinguishedname"][0].ToString();
                        log.LogDebug($"Distinguished name for {userName} is {distinguishedName}");
                        return distinguishedName;
                    }

                }
            }
            catch (Exception exe)
            {
                log.LogError(exe,"Failure to retrived Distinguished Name value. Returning empty string.");
                return string.Empty;
            }

            log.LogWarning("Unable to find distinguished name for {userName}.");
            return string.Empty;
        }
        internal static string GetDistinguishedNameForGroup(string groupName)
        {
            try
            {
                DirectoryEntry de = new DirectoryEntry();
                using (DirectorySearcher ds = new DirectorySearcher(de))
                {
                    ds.Filter = String.Format("(&(objectclass=group)(name={0}))", groupName);
                    ds.PropertiesToLoad.Add("distinguishedname");
                    SearchResult dnResult = ds.FindOne();
                    if (dnResult != null && dnResult.Properties.Contains("distinguishedname"))
                    {
                        string distinguishedName = dnResult.Properties["distinguishedname"][0].ToString();
                        log.LogDebug($"Distinguished name for {groupName} is {distinguishedName}");
                        return distinguishedName;
                    }

                }
            }
            catch (Exception exe)
            {
                log.LogError(exe,"Failure to retrived Distinguished Name value. Returning empty string.");
                return string.Empty;
            }

            log.LogWarning($"Unable to find distinguished name for {groupName}.");
            return string.Empty;
        }
    }
}
