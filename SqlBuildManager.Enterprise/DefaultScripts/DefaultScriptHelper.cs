using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlSync.SqlBuild;
using System.IO;
using sb = SqlSync.SqlBuild.DefaultScripts;
namespace SqlBuildManager.Enterprise.DefaultScripts
{
    public class DefaultScriptHelper
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static bool SetEnterpriseDefaultScripts(List<DefaultScriptRegistryFile> defaultScriptRegs, List<string> groupMemberships)
        {
            DefaultScriptRegistryFile defaultReg = GetApplicableDefaultScriptReg(defaultScriptRegs, groupMemberships);
            if (defaultReg == null)
                return false;

            if (!ValidateLocalToEnterprise(SqlBuildFileHelper.DefaultScriptXmlFile, defaultReg.Path + defaultReg.FileName))
            {
                if (!CopyEnterpriseToLocal(SqlBuildFileHelper.DefaultScriptXmlFile, defaultReg.Path + defaultReg.FileName))
                {
                    return false;
                }
            }

            sb.DefaultScriptRegistry reg = GetEnterpriseRegistrySetting(SqlBuildFileHelper.DefaultScriptXmlFile);
            if (reg == null)
                return false;

            if (reg.Items == null)
            {
                log.WarnFormat("The enterprise default script registry file contains no default script items! ({0})", defaultReg.Path + defaultReg.FileName);
                return false;
            }

            string localScriptPath = Path.GetDirectoryName(SqlBuildFileHelper.DefaultScriptXmlFile) + @"\";
            foreach (sb.DefaultScript item in reg.Items)
            {
                if (!ValidateLocalToEnterprise(localScriptPath + item.ScriptName, defaultReg.Path + item.ScriptName))
                    CopyEnterpriseToLocal(localScriptPath + item.ScriptName, defaultReg.Path + item.ScriptName);
            }


            return true;
        }

        private static sb.DefaultScriptRegistry GetEnterpriseRegistrySetting(string filePath)
        {
            sb.DefaultScriptRegistry registry = null;
            log.DebugFormat("Deserializing DefaultScriptRegistry file from '{0}'", filePath);
            try
            {

                using (StreamReader sr = new StreamReader(filePath))
                {
                    System.Xml.Serialization.XmlSerializer serializer = new System.Xml.Serialization.XmlSerializer(typeof(sb.DefaultScriptRegistry));
                    object obj = serializer.Deserialize(sr);
                    registry = (sb.DefaultScriptRegistry)obj;
                    sr.Close();
                }
            }
            catch (Exception exe)
            {
                log.Error("Unable to deserialize the DefaultScriptRegistry object XML file at " + filePath, exe);
            }
            return registry;
        }

        private static bool ValidateLocalToEnterprise(string localFilePath, string enterpriseFilePath)
        {
            if (!File.Exists(localFilePath))
            {
                log.InfoFormat("Unable to validate local default script. File does not exist at {0}", localFilePath);
                return false;
            }

            if (!File.Exists(enterpriseFilePath))
            {
                log.WarnFormat("Unable to validate enterprise default script. File does not exist at {0}", enterpriseFilePath);
                return true;
            }

            string localHash = string.Empty;
            try
            {
                localHash = SqlBuildFileHelper.GetSHA1Hash(File.ReadAllText(localFilePath));
                log.DebugFormat("Local file name and hash: {0} | {1}", localFilePath, localHash);
            }
            catch (Exception exe)
            {
                log.Error("Unable to get file hash on local default script: " + localFilePath, exe);
            }


            string enterpriseHash = string.Empty;
            try
            {
                enterpriseHash = SqlBuildFileHelper.GetSHA1Hash(File.ReadAllText(enterpriseFilePath));
                log.DebugFormat("Enterprise file name and hash: {0} | {1}", enterpriseFilePath, enterpriseHash);
            }
            catch (Exception exe)
            {
                log.Error("Unable to get file hash on enterprise default script: " + enterpriseFilePath, exe);
            }

            bool match = localHash == enterpriseHash;
            if (!match)
                log.DebugFormat("File mismatch found: {0} <--> {1}", enterpriseHash, localHash);

            return match;

        }
        private static bool CopyEnterpriseToLocal(string localFilePath, string enterpriseFilePath)
        {
            if (!File.Exists(enterpriseFilePath))
            {
                log.WarnFormat("Unable to move enterprise file to local. Enterprise file does not exist: {0}", enterpriseFilePath);
                return false;
            }

            if (File.Exists(localFilePath))
            {
                try
                {
                    FileInfo inf = new FileInfo(localFilePath);
                    inf.Attributes = FileAttributes.Normal;
                    inf.Delete();
                }
                catch (Exception exe)
                {
                    log.Error("Unable to delete local file at:" + localFilePath, exe);
                    return false;
                }
            }

            try
            {
                File.Copy(enterpriseFilePath, localFilePath, true);
                log.DebugFormat("Copied enterprise file to local path:  '{0}' --> '{1}'", enterpriseFilePath, localFilePath);
                return true;
            }
            catch(Exception exe)
            {
                log.Error("Unable to move enterprise file '" + enterpriseFilePath + "' to local file '" + localFilePath + "'", exe);
                return false;
            }
        }
        private static DefaultScriptRegistryFile GetApplicableDefaultScriptReg(List<DefaultScriptRegistryFile> defaultScriptRegs, List<string> groupMemberships)
        {
            try
            {
                if(defaultScriptRegs == null || groupMemberships == null)
                    return null;

                var myDefaults = from defReg in defaultScriptRegs 
                                 join grp in groupMemberships on defReg.ApplyToGroup.ToLower() equals grp.ToLower() 
                                 select new { defReg, grp };

                if (myDefaults.Count() == 0)
                {
                    log.InfoFormat("No DefaultScriptRegistryFile matches found for groups: {0}", String.Join("; ", groupMemberships.ToArray()));
                    return null;
                }
                else
                {
                    DefaultScriptRegistryFile tmp = myDefaults.ToList()[0].defReg;
                    log.DebugFormat("Matched default script registry: {0} to {1}", tmp.FileName, myDefaults.ToList()[0].grp);
                    if(!tmp.Path.EndsWith(@"\"))
                        tmp.Path = tmp.Path + @"\";

                    return tmp;
                }
            }
            catch (Exception exe)
            {
                log.Error("Error matching DefaultScriptRegistryFile to group memberships", exe);
                return null;
            }

        }
    }
}
