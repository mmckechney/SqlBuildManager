using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SqlSync.Connection;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Xml;
using System.Xml.Serialization;

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
            catch
            {
                try
                {
                    string contents = File.ReadAllText(fileName);
                    data = DeserializeMultiDbConfigurationString(contents);
                }
                catch { }
            }
            return data;

        }

        internal static MultiDbData DeserializeMultiDbConfigurationString(string contents)
        {
            return JsonSerializer.Deserialize<MultiDbData>(contents, new JsonSerializerOptions() { PropertyNameCaseInsensitive = true });
        }

        public static MultiDbData ImportMultiDbTextConfig(string fileName)
        {
            if (File.Exists(fileName))
            {
                string[] contents = File.ReadAllLines(fileName);
                return ImportMultiDbTextConfig(contents);
            }
            else
            {
                log.LogError($"The specified database override configuration file '{fileName}' does not exist");
            }

            return null;
        }

        public static string ConvertMultiDbDataToTextConfig(MultiDbData cfg)
        {
            StringBuilder sb = new StringBuilder();
            StringBuilder sbOvr = new StringBuilder();
            foreach (ServerData srv in cfg)
            {
                foreach (var ovr in srv.Overrides)
                {
                    sbOvr.Append($"{ovr.DefaultDbTarget},{ovr.OverrideDbTarget};");

                    if (ovr.ConcurrencyTag.Length > 0)
                        sbOvr.Append($"#{ovr.ConcurrencyTag}");
                }

                sbOvr.Length = sbOvr.Length - 1;
                sb.AppendLine($"{srv.ServerName}:{sbOvr.ToString()}");
                sbOvr.Length = 0;
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
                string tag = string.Empty;

                if(dbs.IndexOf("#") > -1)
                {
                   var tmp =  dbs.Split('#', StringSplitOptions.RemoveEmptyEntries);
                    dbs = tmp[0];
                    tag = tmp[1];
                }

                var sData = new ServerData();
                sData.ServerName = server.Trim();

                string[] arrDb = dbs.Split(';');
                List<DatabaseOverride> tmpDb = new List<DatabaseOverride>();
                for (int j = 0; j < arrDb.Length; j++)
                {
                    //Changing so that a default setting is not required...
                    //if (arrDb[j].IndexOf(',') == -1)
                    //    throw new MultiDbConfigurationException("Error in configuration file line #" + i + 1 + ". Missing \",\" separator. This is needed to separate default and override database targets.");

                    string[] over = arrDb[j].Split(',');
                    DatabaseOverride ovr;
                    if (over.Length > 1)
                        ovr = new DatabaseOverride(server, over[0].Trim().Replace("'", ""), over[1].Trim(), tag);
                    else
                        ovr = new DatabaseOverride(server, "", over[0].Trim(), tag);

                    tmpDb.Add(ovr);
                }
                if (tmpDb.Count > 0)
                {
                    sData.Overrides.AddRange(tmpDb);
                    dummySequence++;
                }
                cfg.Add(sData);
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
                if(tbl.Columns.Count <2)
                {
                    message = "Your SQL query didn't return enough columns. It should return at least 2 columns: target server and target database.";
                    log.LogError(message);
                    return null;
                }
                foreach (DataRow row in tbl.Rows)
                {
                    var ser = new ServerData();
                    ser.ServerName = row[0].ToString().Trim();

                    DatabaseOverride ovr;
                    if (tbl.Columns.Count == 2)
                    {
                        ovr = new DatabaseOverride(ser.ServerName, "client", row[1].ToString().Trim());
                    }
                    else if(tbl.Columns.Count == 3)
                    {
                        ovr = new DatabaseOverride(ser.ServerName , row[1].ToString().Trim(), row[2].ToString().Trim());
                        ovr.AppendedQueryRowData(row.ItemArray, 3, tbl.Columns);
                    }
                    else
                    {
                        //add Tag if retrieved
                        ovr = new DatabaseOverride(ser.ServerName, row[1].ToString().Trim(), row[2].ToString().Trim(), row[3].ToString().Trim());
                        ovr.AppendedQueryRowData(row.ItemArray, 3, tbl.Columns);
                    }

                    ser.Overrides.Add(ovr);
                    counter++;
                    multi.Add(ser);
                }

                message = string.Empty;
                var dbs = multi.Sum(m => m.Overrides.Count());
                var servers = multi.Select(m => m.ServerName).Distinct().Count();
                log.LogInformation($"Found {dbs} target databases across {servers} target servers");
                return multi;
            }
            catch (Exception exe)
            {
                message = exe.Message;
                return null;
            }
        }

        public static bool SaveMultiDbConfigToFile(string fileName, MultiDbData cfg, bool asXml = false)
        {
            try
            {
                string contents;
                if (asXml)
                {
                    contents = SerializeMultiDbConfigurationToXml(cfg);
                }
                else
                {
                    contents = SerializeMultiDbConfigurationToJson(cfg);
                }
                File.WriteAllText(fileName, contents);
                return true;

            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                return false;
            }
        }
        public static string SerializeMultiDbConfigurationToXml(MultiDbData cfg)
        {
            XmlSerializer ser = new XmlSerializer(typeof(MultiDbData));
            StringBuilder sb = new StringBuilder();
            using (StringWriter sw = new StringWriter(sb))
            {
                ser.Serialize(sw, cfg);
            }
            return sb.ToString();
        }
        public static string SerializeMultiDbConfigurationToJson(MultiDbData cfg)
        {

            var output = JsonSerializer.Serialize(cfg, new JsonSerializerOptions() { WriteIndented = true, PropertyNameCaseInsensitive = true });
            return output;
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
            //for (int i = 0; i < dbData.Count; i++)
            foreach (var svr in dbData)
            {
                if (svr.Overrides == null)
                    return false;


                if (!ConnectionHelper.ValidateDatabaseOverrides(svr.Overrides))
                    return false;

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
