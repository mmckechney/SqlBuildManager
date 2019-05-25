using System;
using System.Collections.Generic;
using System.Text;
using SqlSync.Connection;
namespace SqlSync.SqlBuild
{
   public class OverrideData
    {
        private static List<DatabaseOverride> targetDatabaseOverrides = null;
        public static List<DatabaseOverride> TargetDatabaseOverrides
        {
            get { return targetDatabaseOverrides; }
            set { targetDatabaseOverrides = value; }
        }

        //public static string GetTargetDatabase(string defaultDatabase)
        //{
        //    if (String.IsNullOrEmpty(defaultDatabase) && OverrideData.targetDatabaseOverrides != null)
        //        return OverrideData.targetDatabaseOverrides[0].OverrideDbTarget;


        //    if (OverrideData.targetDatabaseOverrides != null)
        //        for (int z = 0; z < OverrideData.targetDatabaseOverrides.Count; z++)
        //            if (OverrideData.targetDatabaseOverrides[z].DefaultDbTarget.ToLower() == defaultDatabase.ToLower())
        //                return OverrideData.targetDatabaseOverrides[z].OverrideDbTarget;

            
        //    return defaultDatabase;
        //}
    }
}
