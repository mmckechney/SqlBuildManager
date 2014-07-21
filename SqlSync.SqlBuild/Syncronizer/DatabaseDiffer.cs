using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using SqlSync.Connection;
using log4net;
namespace SqlSync.SqlBuild.Syncronizer
{
    public class DatabaseDiffer
    {
        private static ILog log = log4net.LogManager.GetLogger(typeof(DatabaseDiffer));

        public DatabaseRunHistory GetDatabaseHistoryDifference(ConnectionData goldenCopy, ConnectionData toBeUpdated)
        {
            //TODO: find the latest common run and have the unique start after that
            var golden = GetDatabaseRunHistory(goldenCopy).BuildFileHistory.OrderByDescending(x => x.CommitDate);
            var toUpdate = GetDatabaseRunHistory(toBeUpdated).BuildFileHistory.OrderByDescending(x => x.CommitDate);

            var unique = golden.Where(p => !toUpdate.Any(p2 => p2.BuildFileHash == p.BuildFileHash));
           
            DatabaseRunHistory uniqueHistory = new DatabaseRunHistory();
            uniqueHistory.BuildFileHistory.AddRange(unique);

            return uniqueHistory;
        }

        public DatabaseRunHistory GetDatabaseRunHistory(ConnectionData dbConnData)
        {
            SqlConnection conn = SqlSync.Connection.ConnectionHelper.GetConnection(dbConnData);

            //Get the latest run of all the unique build hashes along with the build file name...
            string sql = @"SELECT DISTINCT 
	                        BuildProjectHash, 
	                        FIRST_VALUE(CommitDate) OVER (PARTITION BY BuildProjectHash ORDER BY CommitDate DESC) as [CommitDate], 
	                        FIRST_VALUE(BuildFileName) OVER (PARTITION BY BuildProjectHash ORDER BY CommitDate DESC) as [BuildFileName]
                        FROM SqlBuild_Logging 
                        WHERE BuildProjectHash <> '' AND BuildProjectHash IS NOT NULL
                        ORDER BY CommitDate DESC";

            DatabaseRunHistory history = new DatabaseRunHistory();

            try
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    bool filled;
                    while (reader.Read())
                    {
                      
                            history.BuildFileHistory.Add(new BuildFileHistory()
                            {
                                BuildFileHash = reader["BuildProjectHash"].ToString(),
                                BuildFileName = reader["BuildFileName"].ToString(),
                                CommitDate = (DateTime)reader["CommitDate"]
                            });
                            
                        
                    }
                    conn.Close();

                }
            }
            catch (Exception exe)
            {
                if (exe.Message.IndexOf("FIRST_VALUE",0, StringComparison.InvariantCultureIgnoreCase) > -1 )
                    return GetDatabaseRunHistoryOldSqlServer(dbConnData);

                log.Error(String.Format("Unable to get build history for {0}.{1}", dbConnData.SQLServerName, dbConnData.DatabaseName), exe);
            }
            return history;
        }

        /// <summary>
        /// This method will only be used if the target SQL server is an older version that does
        /// not have the FIRST_VALUE analytical command. This method will be slower as it makes multiple calls 
        /// to the database to get the same information
        /// </summary>
        /// <param name="dbConnData"></param>
        /// <returns></returns>
        public DatabaseRunHistory GetDatabaseRunHistoryOldSqlServer(ConnectionData dbConnData)
        {
            SqlConnection conn = SqlSync.Connection.ConnectionHelper.GetConnection(dbConnData);

            //Get the latest run of all the unique build hashes along with the build file name...
            string sql = @"SELECT BuildProjectHash, max(CommitDate) as CommitDate
                        FROM SqlBuild_Logging 
                        WHERE BuildProjectHash <> '' AND BuildProjectHash IS NOT NULL
                        GROUP BY BuildProjectHash
                        ORDER BY CommitDate DESC";

            DatabaseRunHistory history = new DatabaseRunHistory();

            try
            {
                SqlCommand cmd = new SqlCommand(sql, conn);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    bool filled;
                    while (reader.Read())
                    {

                        history.BuildFileHistory.Add(new BuildFileHistory()
                        {
                            BuildFileHash = reader["BuildProjectHash"].ToString(),
                            CommitDate = (DateTime)reader["CommitDate"]
                        });


                    }
                    conn.Close();
                }

                var dates = "'" + 
                    history.BuildFileHistory.Select(d => d.CommitDate.ToString("yyyy-MM-dd HH:mm:ss.FFF")).Aggregate((root,add) => root + "','" + add) +"'";
                          

                string sql2 =
                    String.Format(
                        "SELECT DISTINCT BuildProjectHash, BuildFileName FROM SqlBuild_Logging WHERE CommitDate IN ({0})",
                        dates);

                cmd = new SqlCommand(sql2, conn);
                conn.Open();
                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    bool filled;
                    while (reader.Read())
                    {

                        history.BuildFileHistory.FirstOrDefault(h => h.BuildFileHash == reader["BuildProjectHash"].ToString())
                               .BuildFileName = reader["BuildFileName"].ToString();
                    }
                    conn.Close();
                }


            }
            catch (Exception exe)
            {
                log.Error(String.Format("Unable to get build history for {0}.{1}", dbConnData.SQLServerName, dbConnData.DatabaseName), exe);
            }
            return history;
        }


//        public DatabaseRunHistory GetDatabaseRunHistory(ConnectionData dbConnData)
//        {
//            SqlConnection conn = SqlSync.Connection.ConnectionHelper.GetConnection(dbConnData);

//            string sql = @" SELECT BuildProjectHash, BuildFileName,  CommitDate, ScriptFileHash, ScriptId, ScriptFileName, Sequence 
//                    FROM SqlBuild_Logging 
//                    WHERE BuildProjectHash <> '' AND BuildProjectHash IS NOT NULL
//                    ORDER BY commitDate DESC , sequence ASC";

     
//            DatabaseRunHistory history = new DatabaseRunHistory();

//            try
//            {
//                SqlCommand cmd = new SqlCommand(sql, conn);
//                conn.Open();
//                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
//                {
//                    bool filled;
//                    while (reader.Read())
//                    {
//                        //TODO: Change this to a loop since we know they are in order?
//                        var record = history.BuildFileHistory.FirstOrDefault(x => x.BuildFileHash == (string) reader["BuildProjectHash"]);

//                        if (record == null)
//                        {
//                            BuildFileHistory h = new BuildFileHistory()
//                               {
//                                   BuildFileHash = reader["BuildProjectHash"].ToString(),
//                                  // BuildFileName =  reader["BuildFileName"].ToString(),
//                                   CommitDate = (DateTime)reader["CommitDate"]
//                               };
//                            h.ScriptHistory.Add(new ScriptHistory()
//                                {
//                                    ScriptHash = reader["ScriptFileHash"].ToString(),
//                                    ScriptId = reader["ScriptId"].ToString(),
//                                    ScriptName = reader["ScriptFileName"].ToString(),
//                                    Sequence = (int)reader["Sequence"]
//                                });
//                           history.BuildFileHistory.Add(h);
//                        }
                     
//                        else if (record.CommitDate == (DateTime) reader["CommitDate"])
//                       {
//                           record.ScriptHistory.Add(new ScriptHistory()
//                               {
//                                   ScriptHash = reader["ScriptFileHash"].ToString(),
//                                   ScriptId = reader["ScriptId"].ToString(),
//                                   ScriptName = reader["ScriptFileName"].ToString(),
//                                   Sequence = (int) reader["Sequence"]
//                               });
//                       }
//                       else
//                       {
//                           //this is an older record for the same build file, so ignore it...
//                       }
//                    }
//                    conn.Close();

//                }
//            }
//            catch (Exception exe)
//            {
//                log.Error(String.Format("Unable to get build history for {0}.{1}", dbConnData.SQLServerName, dbConnData.DatabaseName),exe);
//            }
//            return history;
//        }
    }
}
