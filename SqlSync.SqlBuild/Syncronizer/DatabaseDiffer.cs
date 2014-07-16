using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using SqlSync.Connection;
namespace SqlSync.SqlBuild.Syncronizer
{
    public class DatabaseDiffer
    {

        public void GetDatabaseRunHistory(ConnectionData dbConnData)
        {
            SqlConnection conn = SqlSync.Connection.ConnectionHelper.GetConnection(dbConnData);

            string sql = @" SELECT BuildProjectHash, BuildFileName,  CommitDate, ScriptFileHash, ScriptId, ScriptFileName, Sequence 
                    FROM SqlBuild_Logging ORDER BY commitDate DESC , sequence ASC";

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
                        var record =
                            history.BuildFileHistory.FirstOrDefault(
                                x => x.BuildFileHash == (string) reader["BuildProjectHash"]);
                       if (record != null)
                       {
                           record.ScriptHistory.Add(new ScriptHistory()
                               {
                                   ScriptHash = (string) reader["ScriptFileHash"],
                                   ScriptId = (string) reader["ScriptId"],
                                   ScriptName = (string) reader["ScriptFileName"],
                                   Sequence = (int) reader["Sequence"]
                               });
                       }
                       else
                       {
                           BuildFileHistory h = new BuildFileHistory()
                               {
                                   BuildFileHash =(string)reader["BuildProjectHash"],
                                   BuildFileName = (string)reader["BuildFileName"],
                                   CommitDate = (DateTime)reader["CommitDate"]
                               };
                           h.ScriptHistory.Add(new ScriptHistory()
                               {
                                   ScriptHash = (string) reader["ScriptFileHash"],
                                   ScriptId = (string) reader["ScriptId"],
                                   ScriptName = (string) reader["ScriptFileName"],
                                   Sequence = (int) reader["Sequence"]
                               });
                           history.BuildFileHistory.Add(h);
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
                
            }
            finally
            {
                
            }
        }
    }
}
