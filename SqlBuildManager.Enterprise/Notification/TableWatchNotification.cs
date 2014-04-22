using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
namespace SqlBuildManager.Enterprise.Notification
{
    public class TableWatchNotification 
    {
        
        public TableWatchNotification()
        {
        }
        public static bool CheckForTableWatch(string script, out List<SqlBuildManager.Enterprise.TableWatch> matches)
        {
            matches = new List<SqlBuildManager.Enterprise.TableWatch>();

            //Don't have a configuration to check!
            if (EnterpriseConfigHelper.EnterpriseConfig == null || EnterpriseConfigHelper.EnterpriseConfig.TableWatch == null ||
                EnterpriseConfigHelper.EnterpriseConfig.TableWatch.Length == 0)
                return true;

            string baseRegexPattern = @"(\bALTER\b\s*\bTABLE\b\s+.{0,10}<<name>>)|(\bDROP\b\s*\bTABLE\b\s+.{0,10}<<name>>)";
            
            SqlBuildManager.Enterprise.TableWatch[] watches = EnterpriseConfigHelper.EnterpriseConfig.TableWatch;
            for (int i = 0; i < watches.Length; i++)
            {
                SqlBuildManager.Enterprise.TableWatch tmp = watches[i];
                tmp.FoundTables.Clear();
                tmp.Script = string.Empty;
                bool added = false;
                for (int j = 0; j < watches[i].Table.Length; j++)
                {
                    Regex regTable = new Regex(baseRegexPattern.Replace("<<name>>",watches[i].Table[j].Name), RegexOptions.IgnoreCase | RegexOptions.Singleline);
                    if (regTable.Match(script).Success)
                    {
                        if(!tmp.FoundTables.Contains(watches[i].Table[j].Name))
                            tmp.FoundTables.Add(watches[i].Table[j].Name);
                        if (!added)
                        {
                            tmp.Script = script;
                            matches.Add(tmp);
                            added = true;
                        }
                    }
                }
            }

            if (matches.Count == 0)
            {
                matches = null;
                return true;
            }
            else
            {
                return false;
            }
        }
        
    }
}
