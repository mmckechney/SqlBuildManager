using System;
using System.Text;
using SqlSync.Connection;
using SqlSync.DbInformation;
using System.Collections;
using System.Collections.Generic;
using System.IO;
namespace SqlSync.TableScript.Audit
{
	/// <summary>
	/// Summary description for AuditHelper.
	/// </summary>
	public class AuditHelper
	{
		private static string auditTableNameFormat = "{0}_Audit";
        //private static string auditTableNameFormatwithSchema = "[{1}].[{0}_Audit]";
		public static string AuditTableNameFormat
		{
			get {  return auditTableNameFormat; }
			set { auditTableNameFormat = value; }    
		}
	
		public static string triggerNameFormat = "{0}_AuditTrig_{1}";

		public AuditHelper()
		{

		}
		

		#region .: Audit Table and Audit Trigger Scripting :.

        public static string GetAuditScript(TableConfig tableCfg, AuditScriptType type, ConnectionData connData)
        {
            switch (type)
            {
                case AuditScriptType.CreateAuditTable:
                    return AuditHelper.ScriptForAuditTableCreation(tableCfg, connData);

                case AuditScriptType.CreateInsertTrigger:
                case AuditScriptType.CreateUpdateTrigger:
                case AuditScriptType.CreateDeleteTrigger:
                    return AuditHelper.ScriptForAuditTriggers(tableCfg, connData, type, false);

                case AuditScriptType.TriggerDisable:
                    return AuditHelper.ScriptForAuditTriggerEnableDisable(tableCfg.TableName, connData, false);

                case AuditScriptType.TriggerEnable:
                    return AuditHelper.ScriptForAuditTriggerEnableDisable(tableCfg.TableName, connData, true);

                case AuditScriptType.MasterTable:
                    return AuditHelper.ScriptForAuditMasterTable(false);

            }
            return string.Empty;
        }
        private static string ScriptForAuditTableCreation(TableConfig parentTable, ConnectionData connData)
        {
            SqlSync.TableScript.ResourceHelper resHelper = new SqlSync.TableScript.ResourceHelper();
            SqlSync.DbInformation.ColumnInfo[] columns = SqlSync.DbInformation.InfoHelper.GetColumnNamesWithTypes(parentTable.TableName, connData);

            string schema;
            string parentTableName;

            InfoHelper.ExtractNameAndSchema(parentTable.TableName, out parentTableName, out schema);
            string auditTableName = String.Format(auditTableNameFormat, parentTableName);
            //InfoHelper.ExtractNameAndSchema(auditTableName, out auditTableName, out schema);

            StringBuilder sb = new StringBuilder();
            //If Table Exists
            sb.Append(Properties.Resources.AuditTableCreate);
            TableTemplateReplacements(ref sb, auditTableName, "", "", "", parentTableName, schema);
            bool addCharSize;
            for (int i = 0; i < columns.Length; i++)
            {
                if (columns[i].DataType.ToLower() == "text" || columns[i].DataType.ToLower() == "ntext" || columns[i].DataType.ToLower() == "image")
                    continue;

                addCharSize = columns[i].CharMaximum > 0 &&
                    (columns[i].DataType.ToLower() == "varchar" ||
                    columns[i].DataType.ToLower() == "char" ||
                    columns[i].DataType.ToLower() == "nvarchar" ||
                    columns[i].DataType.ToLower() == "nchar");

                if (columns[i].DataType.ToLower() == "timestamp")
                {
                    addCharSize = true;
                    columns[i].DataType = "binary";
                    columns[i].CharMaximum = 8;
                }

                sb.Append(Properties.Resources.AuditColumnCreate);

                TableTemplateReplacements(ref sb, auditTableName, columns[i].ColumnName, columns[i].DataType, (addCharSize) ? "(" + columns[i].CharMaximum + ")" : string.Empty, parentTable.TableName, schema);

                if (addCharSize)
                {
                    sb.Append(Properties.Resources.AuditColumnCharSize);
                    TableTemplateReplacements(ref sb, auditTableName, columns[i].ColumnName, columns[i].DataType, columns[i].CharMaximum.ToString(), parentTableName, schema);
                }
            }

            return sb.ToString();

        }
        private static string ScriptForAuditTriggers(TableConfig parentTable, ConnectionData connData,AuditScriptType type, bool useDS)
        {
            SqlSync.TableScript.ResourceHelper resHelper = new SqlSync.TableScript.ResourceHelper();

            ColumnInfo[] columns = SqlSync.DbInformation.InfoHelper.GetColumnNamesWithTypes(parentTable.TableName, connData);
            StringBuilder cols = new StringBuilder();
            for (int i = 0; i < columns.Length; i++)
            {
                if (columns[i].DataType.ToLower() == "text" || columns[i].DataType.ToLower() == "ntext" || columns[i].DataType.ToLower() == "image")
                    continue;
                cols.AppendFormat("[{0}],",columns[i].ColumnName);
            }
            cols.Length = cols.Length - 1;
            string colList = cols.ToString();

            string auditTableName = String.Format(auditTableNameFormat, parentTable.TableName);
            string schema, parentTableName;
            InfoHelper.ExtractNameAndSchema(auditTableName, out auditTableName, out schema);
            InfoHelper.ExtractNameAndSchema(parentTable.TableName, out parentTableName, out schema);

            StringBuilder sb = new StringBuilder();
            if (type == AuditScriptType.CreateInsertTrigger)
            {
                //Insert Trigger
                if (!useDS)
                    sb.Append(Properties.Resources.AuditInsertTrigger);
                else
                    sb.Append(Properties.Resources.AuditInsertTriggerDS);

                TriggerTemplateReplacements(ref sb, auditTableName, parentTableName, colList, String.Format(triggerNameFormat, parentTableName, "INSERT"), schema);
            }

            //Update Trigger
            if (type == AuditScriptType.CreateUpdateTrigger)
            {
                if (!useDS)
                    sb.Append(Properties.Resources.AuditUpdateTrigger);
                else
                    sb.Append(Properties.Resources.AuditUpdateTriggerDS);

                TriggerTemplateReplacements(ref sb, auditTableName, parentTableName, colList, String.Format(triggerNameFormat, parentTableName, "UPDATE"), schema);
            }

            //Delete Trigger
            if (type == AuditScriptType.CreateDeleteTrigger)
            {
                if (!useDS)
                    sb.Append(Properties.Resources.AuditDeleteTrigger);
                else
                    sb.Append(Properties.Resources.AuditDeleteTriggerDS);

                TriggerTemplateReplacements(ref sb, auditTableName, parentTableName, colList, String.Format(triggerNameFormat, parentTableName, "DELETE"), schema);
            }
            return sb.ToString();

        }
        private static string ScriptForAuditTriggerEnableDisable(string parentTable, ConnectionData connData, bool isEnable)
        {
            string schema;
            InfoHelper.ExtractNameAndSchema(parentTable, out parentTable, out schema);
            SqlSync.TableScript.ResourceHelper resHelper = new SqlSync.TableScript.ResourceHelper();
            StringBuilder sb = new StringBuilder();
            string template;

            if (isEnable)
                template = Properties.Resources.AuditEnableTrigger;
            else
                template = Properties.Resources.AuditDisableTrigger;

            //Insert
            sb.Append(template);
            TriggerTemplateReplacements(ref sb, String.Format(auditTableNameFormat, parentTable), parentTable, "", String.Format(triggerNameFormat, parentTable, "INSERT"), schema);

            //Update
            sb.Append(template);
            TriggerTemplateReplacements(ref sb, String.Format(auditTableNameFormat, parentTable), parentTable, "", String.Format(triggerNameFormat, parentTable, "UPDATE"), schema);

            //Delete
            sb.Append(template);
            TriggerTemplateReplacements(ref sb, String.Format(auditTableNameFormat, parentTable), parentTable, "", String.Format(triggerNameFormat, parentTable, "DELETE"), schema);

            return sb.ToString();
        }
		/// <summary>
		/// Returns string for the audit master table
		/// </summary>
		/// <returns></returns>
		private static string ScriptForAuditMasterTable(bool useDS)
		{
			SqlSync.TableScript.ResourceHelper resHelper = new SqlSync.TableScript.ResourceHelper();
            if (!useDS)
                return Properties.Resources.AuditTrxMaster;
            else
                return Properties.Resources.AuditTrxMasterDS;

		}

		
		public static bool ScriptForCompleteAudit(List<TableConfig> tables, ConnectionData connData, string destFolder, bool useDS)
		{
            string master = ScriptForAuditMasterTable(useDS);
            using (System.IO.StreamWriter sw = System.IO.File.CreateText(Path.Combine(destFolder, "Audit Master Table.sql")))
            {
                sw.WriteLine(master);
                sw.Flush();
                sw.Close();
            }

            string audTable;
			string sudTrig;
			for(int i=0;i<tables.Count;i++)
			{
				audTable = ScriptForAuditTableCreation(tables[i],connData);

                using (System.IO.StreamWriter sw = System.IO.File.CreateText(Path.Combine(destFolder, tables[i].TableName + " Audit Table.sql")))
                {
                    sw.WriteLine(audTable);
                    sw.Flush();
                    sw.Close();
                }

                sudTrig = ScriptForAuditTriggers(tables[i], connData, AuditScriptType.CreateInsertTrigger, useDS);
                using (System.IO.StreamWriter sw = System.IO.File.CreateText(Path.Combine(destFolder, tables[i].TableName + " Audit Insert Trigger.sql")))
                {
                    sw.WriteLine(sudTrig);
                    sw.Flush();
                    sw.Close();
                }

                sudTrig = ScriptForAuditTriggers(tables[i], connData, AuditScriptType.CreateUpdateTrigger, useDS);
                using (System.IO.StreamWriter sw = System.IO.File.CreateText(Path.Combine(destFolder, tables[i].TableName + " Audit Update Trigger.sql")))
                {
                    sw.WriteLine(sudTrig);
                    sw.Flush();
                    sw.Close();
                }

                sudTrig = ScriptForAuditTriggers(tables[i], connData, AuditScriptType.CreateDeleteTrigger, useDS);
                using (System.IO.StreamWriter sw = System.IO.File.CreateText(Path.Combine(destFolder, tables[i].TableName + " Audit Delete Trigger.sql")))
                {
                    sw.WriteLine(sudTrig);
                    sw.Flush();
                    sw.Close();
                }

			}
			return true;
		}
        public static bool ScriptForCompleteTriggerEnableDisable(string[] tables, ConnectionData connData, bool isEnable, string destFolder)
		{

			string aud;
			string fileName;
			for(int i=0;i<tables.Length;i++)
			{
				aud = ScriptForAuditTriggerEnableDisable(tables[i],connData,isEnable);
				if(isEnable)
					fileName = tables[i] +" Enable Triggers.sql";
				else
					fileName = tables[i] +" Disable Triggers.sql";

                using (System.IO.StreamWriter sw = System.IO.File.CreateText(Path.Combine(destFolder, fileName)))
                {
                    sw.WriteLine(aud);
                    sw.Flush();
                    sw.Close();
                }
			}
			return true;
		}
		
		public static AuditAutoDetectData[] AutoDetectDataAuditing(ConnectionData connData)
		{
            List<AuditAutoDetectData> lst = new List<AuditAutoDetectData>();
			string[] auditTables = SqlSync.DbInformation.InfoHelper.GetDatabaseTableList(connData,String.Format(auditTableNameFormat,"%"));
			TableSize[] allTable = SqlSync.DbInformation.InfoHelper.GetDatabaseTableListWithRowCount(connData);
			string baseName;
			int rowCount;
			for(int i=0;i<auditTables.Length;i++)
			{
				baseName = auditTables[i].Replace(String.Format(auditTableNameFormat,""),"");

				rowCount =  SqlSync.DbInformation.InfoHelper.DbContainsTableWithRowcount(baseName,allTable);
				if(rowCount > -1)
				{
					AuditAutoDetectData dat = new AuditAutoDetectData();
					dat.TableName = baseName;
					dat.RowCount = rowCount;
					dat.HasAuditTable = true;
					lst.Add(dat);
				}
			}
			string[] allTriggers = SqlSync.DbInformation.InfoHelper.GetTriggers(connData);
			for(int i=0;i<lst.Count;i++)
			{
                IAuditInfo item = lst[i];
                CheckForStandardAuditTriggers(ref item, allTriggers);
			}
			
			AuditAutoDetectData[] data = new AuditAutoDetectData[lst.Count];
			lst.CopyTo(data);
			return data;


		}
        public static void CheckForStandardAuditTriggers(ref IAuditInfo tableInf, ConnectionData connData)
        {
            string[] allTriggers = SqlSync.DbInformation.InfoHelper.GetTriggers(connData);
            CheckForStandardAuditTriggers(ref tableInf, allTriggers);
        }
        public static void CheckForStandardAuditTriggers(ref IAuditInfo tableInf, string[] allTriggers)
        {
            for (int j = 0; j < allTriggers.Length; j++)
            {
                if (allTriggers[j].IndexOf(String.Format(triggerNameFormat, tableInf.TableName, "INSERT")) > -1)
                {
                    tableInf.HasAuditInsertTrigger = true;
                }
                else if (allTriggers[j].IndexOf(String.Format(triggerNameFormat, tableInf.TableName, "UPDATE")) > -1)
                {
                    tableInf.HasAuditUpdateTrigger = true;
                }
                else if (allTriggers[j].IndexOf(String.Format(triggerNameFormat, tableInf.TableName, "DELETE")) > -1)
                {
                    tableInf.HasAuditDeleteTrigger = true;
                }
            }
        }
		#endregion
		
		public static void TableTemplateReplacements(ref StringBuilder sb,string auditTable,string colName, string colType, string charLength,string tableName, string schema)
		{
			sb.Replace(ReplaceConstants.AuditTableName,auditTable);
			sb.Replace(ReplaceConstants.CharLength,charLength);
			sb.Replace(ReplaceConstants.ColumnName,colName);
			sb.Replace(ReplaceConstants.ColumnType,colType);
			sb.Replace(ReplaceConstants.TableName,tableName);
            sb.Replace(ReplaceConstants.Schema, schema);
		}
		public static void TriggerTemplateReplacements(ref StringBuilder sb,string auditTable,string masterTableName, string columnList,string triggerName, string schema)
		{
			sb.Replace(ReplaceConstants.AuditTableName,auditTable);
			sb.Replace(ReplaceConstants.TableName,masterTableName);
			sb.Replace(ReplaceConstants.ColumnList,columnList);
			sb.Replace(ReplaceConstants.TriggerName,triggerName);
            sb.Replace(ReplaceConstants.Schema, schema);
		}

        public static void TriggerTemplateReplacements(ref StringBuilder sb, string auditTable, string masterTableName, string columnList, string triggerName, TableConfig cfg)
        {
            string schema;
            InfoHelper.ExtractNameAndSchema(masterTableName, out masterTableName, out schema);
            TriggerTemplateReplacements(ref sb, auditTable, masterTableName, columnList, triggerName,schema);
            if (cfg.ConfigData != null)
            {
                if (cfg.ConfigData.IndividualIDColumn.Length == 0)
                    sb.Replace(ReplaceConstants.IndividualIDColumn, "NULL");
                else
                    sb.Replace(ReplaceConstants.IndividualIDColumn, cfg.ConfigData.IndividualIDColumn);

                if (cfg.ConfigData.InsertByColumn.Length == 0)
                    sb.Replace(ReplaceConstants.InsertByColumn, "NULL");
                else
                    sb.Replace(ReplaceConstants.InsertByColumn, cfg.ConfigData.InsertByColumn);

                if (cfg.ConfigData.ObjectTypeColumn.Length == 0)
                    sb.Replace(ReplaceConstants.ObjectTypeColumn, "NULL");
                else
                    sb.Replace(ReplaceConstants.ObjectTypeColumn, cfg.ConfigData.ObjectTypeColumn);

            }
        }



        
    }
    
}
