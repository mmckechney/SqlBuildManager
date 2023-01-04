using System.Collections.Generic;
using System.Linq;
namespace SqlBuildManager.Enterprise.Feature
{
    public class FeatureAccessHelper
    {
        public static bool IsFeatureEnabled(string featureKey, string loginId, List<string> adGroupMemberships, FeatureAccess[] accessCfg)
        {
            if (featureKey == null || featureKey.Length == 0)
                return false;

            if (accessCfg == null || accessCfg.Length == 0)
                return false;

            foreach (FeatureAccess feature in accessCfg)
            {
                if (feature.FeatureId.ToLower() == featureKey.ToLower())
                {
                    if (!feature.Enabled)
                        return false;

                    if (feature.Deny != null && feature.Deny.Length > 0)
                    {
                        var matchLogin = from d in feature.Deny
                                         where d.LoginId.ToLower() == loginId.ToLower()
                                         select d;
                        var matchGroup = from d in feature.Deny
                                         join g in adGroupMemberships on d.GroupName.ToLower() equals g.ToLower()
                                         select d;

                        if (matchGroup.Count() > 0 || matchLogin.Count() > 0)
                            return false;

                    }


                    if (feature.Allow == null)
                        return true;

                    if (feature.Allow != null && feature.Allow.Length > 0)
                    {
                        var matchLogin = from a in feature.Allow
                                         where a.LoginId.ToLower() == loginId.ToLower()
                                         select a;
                        var matchGroup = from a in feature.Allow
                                         join g in adGroupMemberships on a.GroupName.ToLower() equals g.ToLower()
                                         select a;

                        if (matchGroup.Count() > 0 || matchLogin.Count() > 0)
                            return true;
                    }
                }
            }


            return false;
        }
    }
}
