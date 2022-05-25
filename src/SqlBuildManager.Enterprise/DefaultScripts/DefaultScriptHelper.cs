using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SqlSync.SqlBuild;
using System.IO;
using sb = SqlSync.SqlBuild.DefaultScripts;
using Microsoft.Extensions.Logging;
namespace SqlBuildManager.Enterprise.DefaultScripts
{
    public class DefaultScriptHelper
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static bool SetEnterpriseDefaultScripts(List<DefaultScriptRegistryFile> defaultScriptRegs, List<string> groupMemberships)
        {
            DefaultScriptRegistryFile defaultReg = GetApplicableDefaultScriptReg(defaultScriptRegs, groupMemberships);
            if (defaultReg == null)
                return false;

            if (!ValidateLocalToEnterprise(SqlBuildFileHelper.DefaultScriptXmlFile, Path.Combine(defaultReg.Path, defaultReg.FileName)))
            {
                if (!CopyEnterpriseToLocal(SqlBuildFileHelper.DefaultScriptXmlFile, Path.Combine(defaultReg.Path, defaultReg.FileName)))
                {
                    return false;
                }
            }

            sb.DefaultScriptRegistry reg = GetEnterpriseRegistrySetting(SqlBuildFileHelper.DefaultScriptXmlFile);
            if (reg == null)
                return false;

            if (reg.Items == null)
            {
                log.LogWarning($"The enterprise default script registry file contains no default script items! ({Path.Combine(defaultReg.Path, defaultReg.FileName)}");
                return false;
            }

            string localScriptPath = Path.GetDirectoryName(SqlBuildFileHelper.DefaultScriptXmlFile);
            foreach (sb.DefaultScript item in reg.Items)
            {
                if (!ValidateLocalToEnterprise(Path.Combine(localScriptPath, item.ScriptName), Path.Combine(defaultReg.Path, item.ScriptName)))
                {
                    CopyEnterpriseToLocal(Path.Combine(localScriptPath, item.ScriptName), Path.Combine(defaultReg.Path, item.ScriptName));
                }
            }


            return true;
        }

        internal static sb.DefaultScriptRegistry GetEnterpriseRegistrySetting(string filePath)
        {
            sb.DefaultScriptRegistry registry = null;
            log.LogDebug($"Deserializing DefaultScriptRegistry file from '{filePath}'");
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
                log.LogError(exe,$"Unable to deserialize the DefaultScriptRegistry object XML file at {filePath}");
            }
            return registry;
        }

        internal static bool ValidateLocalToEnterprise(string localFilePath, string enterpriseFilePath)
        {
            if (!File.Exists(localFilePath))
            {
                log.LogInformation($"Unable to validate local default script. File does not exist at {localFilePath}" );
                return false;
            }

            if (!File.Exists(enterpriseFilePath))
            {
                log.LogWarning($"Unable to validate enterprise default script. File does not exist at {enterpriseFilePath}" );
                return true;
            }

            string localHash = string.Empty;
            try
            {
                SqlBuildFileHelper.GetSHA1Hash(new string[] { File.ReadAllText(localFilePath) }, out localHash);
                log.LogDebug($"Local file name and hash: {localFilePath} | {localHash}");
            }
            catch (Exception exe)
            {
                log.LogError(exe, $"Unable to get file hash on local default script: {localFilePath}" );
            }


            string enterpriseHash = string.Empty;
            try
            {
                SqlBuildFileHelper.GetSHA1Hash(new string[] { File.ReadAllText(enterpriseFilePath) }, out enterpriseHash);
                log.LogDebug($"Enterprise file name and hash: {enterpriseFilePath} | {enterpriseHash}");
            }
            catch (Exception exe)
            {
                log.LogError(exe, $"Unable to get file hash on enterprise default script: {enterpriseFilePath}");
            }

            bool match = localHash == enterpriseHash;
            if (!match)
            {
                log.LogDebug($"File mismatch found: {enterpriseHash} <--> {localHash}");
            }

            return match;

        }
        internal static bool CopyEnterpriseToLocal(string localFilePath, string enterpriseFilePath)
        {
            if (!File.Exists(enterpriseFilePath))
            {
                log.LogWarning($"Unable to move enterprise file to local. Enterprise file does not exist: {enterpriseFilePath}");
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
                    log.LogError(exe, $"Unable to delete local file at: {localFilePath}");
                    return false;
                }
            }

            try
            {
                File.Copy(enterpriseFilePath, localFilePath, true);
                log.LogDebug($"Copied enterprise file to local path:  '{enterpriseFilePath}' --> '{localFilePath}'");
                return true;
            }
            catch(Exception exe)
            {
                log.LogError(exe, $"Unable to move enterprise file '{enterpriseFilePath}' to local file '{localFilePath}");
                return false;
            }
        }
        internal static DefaultScriptRegistryFile GetApplicableDefaultScriptReg(List<DefaultScriptRegistryFile> defaultScriptRegs, List<string> groupMemberships)
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
                    log.LogInformation($"No DefaultScriptRegistryFile matches found for groups: {String.Join("; ", groupMemberships.ToArray())}");
                    return null;
                }
                else
                {
                    DefaultScriptRegistryFile tmp = myDefaults.ToList()[0].defReg;
                    log.LogDebug($"Matched default script registry: {tmp.FileName} to {myDefaults.ToList()[0].grp}");

                    return tmp;
                }
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Error matching DefaultScriptRegistryFile to group memberships");
                return null;
            }

        }
    }
}
