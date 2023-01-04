using System.Data;

namespace SqlSync.SqlBuild
{
    public class BuildDataHelper
    {
        public static void GetLastBuildNumberAndDb(SqlSyncBuildData buildData, out double lastbuildNumber, out string lastDatabase)
        {
            //Use the dataset to get the last build number and Db
            if (buildData != null && buildData.Script.Rows.Count > 0)
            {
                DataView view = buildData.Script.DefaultView;
                view.RowFilter = buildData.Script.BuildOrderColumn.ColumnName + " < " + ((int)ResequenceIgnore.StartNumber).ToString();
                view.Sort = buildData.Script.BuildOrderColumn + " DESC";
                if (view.Count > 0)
                {
                    lastbuildNumber = ((SqlSyncBuildData.ScriptRow)view[0].Row).BuildOrder;
                    lastDatabase = ((SqlSyncBuildData.ScriptRow)view[0].Row).Database;
                }
                else
                {
                    lastbuildNumber = 0;
                    lastDatabase = "";
                }

                return;
            }

            lastbuildNumber = 0;
            lastDatabase = string.Empty;
            return;
        }
    }
}
