using System.Data;
using System.Linq;
using SqlSync.SqlBuild.Models;

namespace SqlSync.SqlBuild
{
    public class BuildDataHelper
    {
        public static void GetLastBuildNumberAndDb(SqlSyncBuildDataModel buildData, out double lastbuildNumber, out string lastDatabase)
        {
            if (buildData != null && buildData.Script.Count > 0)
            {
                var scripts = buildData.Script
                    .Where(s => s.BuildOrder.HasValue && s.BuildOrder.Value < (int)ResequenceIgnore.StartNumber)
                    .OrderByDescending(s => s.BuildOrder!.Value)
                    .ToList();
                if (scripts.Count > 0)
                {
                    lastbuildNumber = scripts[0].BuildOrder!.Value;
                    lastDatabase = scripts[0].Database ?? string.Empty;
                    return;
                }
            }

            lastbuildNumber = 0;
            lastDatabase = string.Empty;
            return;
        }
    }
}
