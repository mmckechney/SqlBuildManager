using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Xml.Serialization;
using SqlSync.DbInformation;
using SqlSync.Connection;
using System.Data;
using Microsoft.Data.SqlClient;
using System.Xml;
using Microsoft.Extensions.Logging;
using System.Linq;
namespace SqlSync.SqlBuild.MultiDb
{
    public class MultiDbHelper
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static MultiDbData DeserializeMultiDbConfiguration(string fileName)
        {
            MultiDbData data = null;
            try
            {
                using (TextReader reader = new StreamReader(fileName))
                {
                    XmlSerializer xmlS = new XmlSerializer(typeof(MultiDbData));
                    object tmp = xmlS.Deserialize(reader);
                    if (tmp != null)
                        data = (MultiDbData)tmp;
                }
            }
            catch { }
            return data;

        }

        public static MultiDbData ImportMultiDbTextConfig(string fileName)
        {
            if(File.Exists(fileName))
            {
                string[] contents = File.ReadAllLines(fileName);
                return ImportMultiDbTextConfig(contents);
            }
            
            return null;
        }

        public static string ConvertMultiDbDataToTextConfig(MultiDbData cfg)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder sbOvr = new StringBuilder();
            foreach (ServerData srv in cfg)
            {
                foreach (var seq in srv.OverrideSequence)
                {
                    sbOvr.Length = 0;
                    foreach (DatabaseOverride ovr in seq.Value)
                    {
                        sbOvr.Append(ovr.DefaultDbTarget + "," + ovr.OverrideDbTarget + ";");
                    }
                    sbOvr.Length = sbOvr.Length-1;
                    sb.AppendLine(srv.ServerName + ":" + sbOvr.ToString());
                }
            }
            return sb.ToString();
        }
        /// <summary>
        /// Takes a simplified delimited text format to create a multi-database run configuration.
        /// <example>
        /// Expects each line to be in a server: override format below:
        ///     SERVER:defaultDb,override;default2,override2
        /// </example>
        /// </summary>
        /// <param name="fileContents"></param>
        /// <returns></returns>
        public static MultiDbData ImportMultiDbTextConfig(string[] fileContents)
        {
            int dummySequence = 0;
            MultiDbData cfg = new MultiDbData();
            for (int i = 0; i < fileContents.Length; i++)
            {
                
                string line = fileContents[i];

                //Skip empty lines...
                if (line.Trim().Length == 0)
                    continue;

                //need to have that colon!
                if (line.IndexOf(':') == -1)
                    throw new MultiDbConfigurationException("Error in configuration file line #" + i + 1 + ". Missing \":\" separator. This is needed to separate server from database override values.");

                string server = line.Split(':')[0];
                string dbs = line.Split(':')[1];
                ServerData sData = cfg[server];
                if (sData == null)
                {
                    sData = new ServerData();
                    sData.ServerName = server.Trim();
                    cfg[server] = sData;
                }

                string[] arrDb = dbs.Split(';');
                List<DatabaseOverride> tmpDb = new List<DatabaseOverride>();
                for (int j = 0; j < arrDb.Length; j++)
                {
                    //Changing so that a default setting is not required...
                    //if (arrDb[j].IndexOf(',') == -1)
                    //    throw new MultiDbConfigurationException("Error in configuration file line #" + i + 1 + ". Missing \",\" separator. This is needed to separate default and override database targets.");

                    string[] over = arrDb[j].Split(',');
                    DatabaseOverride ovr;
                    if(over.Length > 1)
                        ovr= new DatabaseOverride(over[0].Trim().Replace("'",""), over[1].Trim());
                    else
                        ovr = new DatabaseOverride("", over[0].Trim());

                    tmpDb.Add(ovr);
                }
                if (tmpDb.Count > 0)
                {
                    sData.OverrideSequence.Add(dummySequence.ToString(), tmpDb);
                    dummySequence++;
                }
                //cfg.Add(sData);
            }
            return cfg;
        }

        public static MultiDbData CreateMultiDbConfigFromQueryFile(string fileName, out string message)
        {
            try
            {
                MultiDbQueryConfig cfg = MultiDbHelper.LoadMultiDbQueryConfiguration(fileName);
                ConnectionData connData = new ConnectionData(cfg.SourceServer, cfg.Database);
                return CreateMultiDbConfigFromQuery(connData, cfg.Query, out message);
            }
            catch (Exception exe)
            {
                message = exe.Message;
                return null;
            }
        }
        public static MultiDbData CreateMultiDbConfigFromQuery(ConnectionData connData, string query, out string message)
        {
            try
            {
                log.LogInformation($"Generating database override configuation from {connData.SQLServerName} : {connData.DatabaseName}");
                log.LogDebug($"Override generation script: {query}");

                SqlConnection conn = ConnectionHelper.GetConnection(connData);
                SqlCommand cmd = new SqlCommand(query, conn);
                cmd.CommandType = CommandType.Text;

                DataTable tbl = new DataTable();
                SqlDataAdapter adapt = new SqlDataAdapter(cmd);
                adapt.Fill(tbl);

                MultiDbData multi = new MultiDbData();
                int counter = 0;
                foreach (DataRow row in tbl.Rows)
                {
                    ServerData ser = multi[row[0].ToString().Trim()];
                    if (ser == null)
                    {
                        ser = new ServerData();
                        ser.ServerName = row[0].ToString().Trim();
                    }

                    DatabaseOverride ovr;
                    if (tbl.Columns.Count == 2)
                    {
                        ovr = new DatabaseOverride("client", row[1].ToString().Trim());
                    }
                    else
                    {
                        ovr = new DatabaseOverride(row[1].ToString().Trim(), row[2].ToString().Trim());
                        ovr.AppendedQueryRowData(row.ItemArray, 3, tbl.Columns);
                    }
                    ser.OverrideSequence.Add(counter.ToString(), ovr);
                    counter++;
                    multi[ser.ServerName] = ser;
                }

                message = string.Empty;
                var dbs = multi.Sum(m => m.OverrideSequence.Count());
                log.LogInformation($"Found {dbs} target databases across {multi.Count()} target servers");
                return multi;
            }
            catch(Exception exe)
            {
                message = exe.Message;
                return null;
            }
        }

        public static bool SaveMultiDbQueryConfiguration(string fileName, MultiDbQueryConfig cfg)
        {
            try
            {
                using (XmlTextWriter tw = new XmlTextWriter(fileName, Encoding.UTF8))
                {
                    tw.Formatting = Formatting.Indented;
                    XmlSerializer xmlS = new XmlSerializer(typeof(MultiDb.MultiDbQueryConfig));
                    xmlS.Serialize(tw, cfg);
                }
                return true;
            }
            catch
            {
                return false;
            }
        
        }

        public static MultiDbQueryConfig LoadMultiDbQueryConfiguration(string fileName)
        {
            MultiDbQueryConfig data = null;
            try
            {
                using (TextReader reader = new StreamReader(fileName))
                {
                    XmlSerializer xmlS = new XmlSerializer(typeof(MultiDbQueryConfig));
                    object tmp = xmlS.Deserialize(reader);
                    if (tmp != null)
                        data = (MultiDbQueryConfig)tmp;
                }
            }
            catch
            {
            }
            return data;

        }

        public static bool ValidateMultiDatabaseData(MultiDbData dbData)
        {
            for (int i = 0; i < dbData.Count; i++)
            {
                if (dbData[i].OverrideSequence == null)
                    return false;

               SerializableDictionary<string, List<DatabaseOverride>>.Enumerator enumer = dbData[i].OverrideSequence.GetEnumerator();
               while (enumer.MoveNext())
               {
                   if (!ConnectionHelper.ValidateDatabaseOverrides(dbData[i].OverrideSequence[enumer.Current.Key]))
                       return false;
               }
            }
            return true;
        }



    }
    public class MultiDbConfigurationException : Exception
    {
        public MultiDbConfigurationException(string message) : base(message)
        {
        }
    }
}
