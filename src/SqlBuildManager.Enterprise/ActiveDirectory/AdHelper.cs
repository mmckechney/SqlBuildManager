using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.DirectoryServices;
namespace SqlBuildManager.Enterprise.ActiveDirectory
{
    public class AdHelper
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
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
                        log.DebugFormat("Distinguished name for {0} is {1}", userName, distinguishedName);
                        return distinguishedName;
                    }

                }
            }
            catch (Exception exe)
            {
                log.Error("Failure to retrived Distinguished Name value. Returning empty string.", exe);
                return string.Empty;
            }

            log.WarnFormat("Unable to find distinguished name for {0}.", userName);
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
                        log.DebugFormat("Distinguished name for {0} is {1}", groupName, distinguishedName);
                        return distinguishedName;
                    }

                }
            }
            catch (Exception exe)
            {
                log.Error("Failure to retrived Distinguished Name value. Returning empty string.", exe);
                return string.Empty;
            }

            log.WarnFormat("Unable to find distinguished name for {0}.", groupName);
            return string.Empty;
        }
    }
}
