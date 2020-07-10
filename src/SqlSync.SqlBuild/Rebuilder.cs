using System;
using System.Collections.Generic;
using System.Text;
using SqlSync.Connection;
using Microsoft.Data.SqlClient;
using System.Data;
using System.IO;
using SqlSync.DbInformation;
using log4net;
namespace SqlSync.SqlBuild
{
    public class Rebuilder
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private ConnectionData connData;
        //private string committedBuildFileName;
        //private DateTime commitDate;
        private string newBuildFileName;
        private CommittedBuildData commitData;
        public Rebuilder(ConnectionData connData, CommittedBuildData commitData, string newBuildFileName)
        {
            this.connData = connData;
            this.commitData = commitData;
            this.newBuildFileName = newBuildFileName;
        }

        internal static List<RebuilderData> RetreiveBuildData(ConnectionData dbConnData, string buildFileHash,
                                                              DateTime commitDate)
        {
;
            string sql =
                @"SELECT ScriptFileName, ScriptId, Sequence,ScriptText, '' as [database], Tag FROM SqlBuild_Logging 
                    WHERE BuildProjectHash = @BuildFileHash AND CommitDate = @CommitDate
                    ORDER BY Sequence ";

            List<RebuilderData> data = new List<RebuilderData>();

            SqlConnection conn = SqlSync.Connection.ConnectionHelper.GetConnection(dbConnData);
            SqlCommand cmd = new SqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@BuildFileHash", buildFileHash);
            cmd.Parameters.AddWithValue("@CommitDate", commitDate.ToString("yyyy-MM-dd HH:mm:ss.FFF"));
            conn.Open();
            using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
            {
                bool filled;
                while (!reader.IsClosed)
                {
                    RebuilderData dat = new RebuilderData();
                    filled = dat.Fill(reader, false);
                    if (filled)
                    {
                        dat.Database = dbConnData.DatabaseName;
                        data.Add(dat);
                    }
                }
                conn.Close();
            }



            return data;

        }

        private List<RebuilderData> RetreiveBuildData()
        {
            string startinDb = connData.DatabaseName;
            string sql = @"SELECT ScriptFileName, ScriptId, Sequence,ScriptText, '' as [database], Tag FROM SqlBuild_Logging 
                    WHERE BuildFileName = @BuildFileName AND CommitDate = @CommitDate
                    ORDER BY Sequence ";

            List<RebuilderData> data = new List<RebuilderData>();
            string[] dbs = this.commitData.Database.Split(';');
            for (int i = 0; i < dbs.Length; i++)
            {
                this.connData.DatabaseName = dbs[i];
                SqlConnection conn = SqlSync.Connection.ConnectionHelper.GetConnection(this.connData);
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.Parameters.AddWithValue("@BuildFileName", this.commitData.BuildFileName);
                cmd.Parameters.AddWithValue("@CommitDate", this.commitData.CommitDate);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    bool filled;
                    while (!reader.IsClosed)
                    {
                        RebuilderData dat = new RebuilderData();
                        filled = dat.Fill(reader, false);
                        if (filled)
                        {
                            dat.Database = dbs[i];
                            data.Add(dat);
                        }
                    }
                    conn.Close();
                }
            }
            this.connData.DatabaseName = startinDb;

            return data;
            
        }
        internal static bool RebuildBuildManagerFile(int defaultTimeout, string buildFileName, List<RebuilderData> rebuildData)
        {
            string tempPath = System.IO.Path.GetTempPath() + System.Guid.NewGuid();
            Directory.CreateDirectory(tempPath);
            try
            {
                string projFileName = Path.Combine(tempPath, SqlSync.SqlBuild.XmlFileNames.MainProjectFile);
                
                for (int i = 0; i < rebuildData.Count; i++)
                {
                    File.WriteAllText( Path.Combine(tempPath , rebuildData[i].ScriptFileName), rebuildData[i].ScriptText);
                }

                SqlSyncBuildData buildData = SqlBuildFileHelper.CreateShellSqlSyncBuildDataObject();
                buildData.AcceptChanges();

                if (!SqlBuildFileHelper.PackageProjectFileIntoZip(buildData, tempPath, buildFileName))
                {
                    return false;
                }

                if(!ZipHelper.UnpackZipPackage(tempPath, buildFileName,false))
                {
                    return false;
                }

                for (int i = 0; i < rebuildData.Count; i++)
                {
                    SqlBuildFileHelper.AddScriptFileToBuild(ref buildData,
                        projFileName,
                        rebuildData[i].ScriptFileName,
                        rebuildData[i].Sequence + 1,
                        string.Empty,
                        true,
                        true,
                        rebuildData[i].Database,
                        false,
                        buildFileName,
                        false,
                        false,
                        System.Environment.UserName,
                        defaultTimeout,
                        rebuildData[i].ScriptId,
                        rebuildData[i].Tag);
                }

                SqlBuildFileHelper.SaveSqlBuildProjectFile(ref buildData, projFileName, buildFileName);

                return true;
            }
            finally
            {
                if (Directory.Exists(tempPath))
                    Directory.Delete(tempPath, true);
            }
        }
        public bool RebuildBuildManagerFile(int defaultTimeout)
        {
            List<RebuilderData> rebuildData = RetreiveBuildData();
            return RebuildBuildManagerFile(defaultTimeout, this.newBuildFileName, rebuildData);
        }

        #region .: Discovery Methods :.
        public static List<CommittedBuildData> GetCommitedBuildList(ConnectionData connData, DatabaseList dbList)
        {
           
                string startingDb = connData.DatabaseName;
                string sql = @"SELECT BuildFileName, count(ScriptFileName) as ScriptCount, commitDate, '' as [database] 
                    FROM SqlBuild_Logging GROUP BY BuildFileName,commitDate ORDER BY commitDate DESC";

                List<CommittedBuildData> data = new List<CommittedBuildData>();
                for (int i = 0; i < dbList.Count; i++)
                {
                    if (dbList[i].IsManuallyEntered)
                        continue;

                    connData.DatabaseName = dbList[i].DatabaseName;
                    SqlConnection conn = SqlSync.Connection.ConnectionHelper.GetConnection(connData);
                    try
                    {
                        SqlCommand cmd = new SqlCommand(sql, conn);
                        conn.Open();
                        using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                        {
                            bool filled;
                            while (!reader.IsClosed)
                            {
                                CommittedBuildData dat = new CommittedBuildData();
                                filled = dat.Fill(reader, false);
                                if (filled)
                                {
                                    dat.Database = dbList[i].DatabaseName;
                                    data.Add(dat);
                                }
                            }
                            conn.Close();
                        }
                    }
                    catch (SqlException)
                    {
                        //ignore
                    }
                    catch (Exception)
                    {
                        return new List<CommittedBuildData>();
                    }
                    finally
                    {
                        connData.DatabaseName = startingDb;
                    }
                }
                return MergeServerResults(data); ;

        }
        public static List<CommittedBuildData> GetCommitedBuildList(ConnectionData connData, string databaseName)
        {

            string startingDb = connData.DatabaseName;
            string sql = @"SELECT BuildFileName, count(ScriptFileName) as ScriptCount, commitDate, '' as [database] 
                    FROM SqlBuild_Logging GROUP BY BuildFileName,commitDate ORDER BY commitDate DESC";

            List<CommittedBuildData> data = new List<CommittedBuildData>();
            connData.DatabaseName = databaseName;
            SqlConnection conn = SqlSync.Connection.ConnectionHelper.GetConnection(connData);
            try
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    bool filled;
                    while (!reader.IsClosed)
                    {
                        CommittedBuildData dat = new CommittedBuildData();
                        filled = dat.Fill(reader, false);
                        if (filled)
                        {
                            dat.Database = databaseName;
                            data.Add(dat);
                        }
                    }
                    conn.Close();
                }
            }
            catch (SqlException sExe)
            {
                log.Error(string.Format("Unable to get committed SBM package list from {0}.{1}", connData.SQLServerName, databaseName), sExe);
            }
            catch (Exception exe)
            {
                log.Error(string.Format("Unable to get committed SBM package list from {0}.{1}", connData.SQLServerName, databaseName), exe);
                return new List<CommittedBuildData>();
            }
            finally
            {
                connData.DatabaseName = startingDb;
            }

            return data;

        }

        private static List<CommittedBuildData> MergeServerResults(List<CommittedBuildData> builds)
        {
            List<CommittedBuildData> merged = new List<CommittedBuildData>();
            for (int i = 0; i < builds.Count; i++)
            {
                if (builds[i] != null)
                {
                    for (int j = 0; j < builds.Count; j++)
                    {
                        if (i != j && builds[j] != null)
                        {
                            if (builds[j].BuildFileName == builds[i].BuildFileName && builds[j].CommitDate == builds[i].CommitDate)
                            {
                                builds[i].ScriptCount += builds[j].ScriptCount;
                                builds[i].Database += ";" + builds[j].Database;
                                builds[j] = null;
                            }
                        }
                    }
                    merged.Add(builds[i]);
                }
            }
            return merged;
        }
        #endregion
    }
}
