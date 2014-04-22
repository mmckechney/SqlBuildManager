using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlBuildManager.Enterprise.Tag
{
    public class EnterpriseTagHelper
    {
        public static List<string> GetEnterpriseTagRegexValues(List<ScriptTagInference> inferenceList, List<string> adGroupMembership)
        {
 
            if (inferenceList == null)
                return null;    
            if (adGroupMembership == null)
                return null;

            var hasNull = (from i in inferenceList where i == null select i);
            if (hasNull.Count() > 0)
                return null;

            List<string> regexValues = new List<string>();

            var r = (from i in inferenceList
                     from a in i.ApplyToGroup
                     join g in adGroupMembership on a.GroupName equals g
                     select i.TagRegex);

            if (r.Count() > 0)
            {
                var tmp = r.ToList();
                foreach (TagRegex[] tr in r)
                    regexValues.AddRange(from v in tr select v.RegexValue);

                return regexValues;
            }
            else
                return null;
        }
    }
}
