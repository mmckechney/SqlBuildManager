using log4net;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;

namespace SqlSync.SqlBuild
{
    /// <summary>
    /// Summary description for ZipHelper.
    /// </summary>
    public class ZipHelper
	{
        private static ILog log = LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

		public ZipHelper()
		{

        }

        public static bool CreateZipPackage(string[] filesToZip, string basePath, string zipFileName,bool keepPathInfo)
		{
            return CreateZipPackage(filesToZip, basePath, zipFileName, keepPathInfo, 0);
		}
		public static bool CreateZipPackage(string[] filesToZip, string basePath, string zipFileName)
		{
			return CreateZipPackage(filesToZip,basePath,zipFileName,true);
		}
       
        public static bool CreateZipPackage(List<string> fullPathFilesToZip, string zipFileName, bool keepPathInfo, int retryCount)
        {

            try
            {
                string tempName = Path.GetDirectoryName(zipFileName) + @"\~" + retryCount.ToString() + "~" + Path.GetFileName(zipFileName);
                using (ZipArchive newFile = ZipFile.Open(tempName, ZipArchiveMode.Create))
                {
                    foreach (string file in fullPathFilesToZip)
                    {
                        if (File.Exists(file) == false)
                            continue;

                        newFile.CreateEntryFromFile(file, Path.GetFileName(file));

                    }
                }
                if (File.Exists(zipFileName))
                {
                    File.Delete(zipFileName);
                }
                File.Move(tempName, zipFileName);
            }
            catch (Exception e)
            {
                if (retryCount < 5)
                {
                    System.Threading.Thread.Sleep(100);
                    return CreateZipPackage(fullPathFilesToZip, zipFileName, keepPathInfo, retryCount + 1);
                }
                else
                {
                    log.Error(String.Format("Unable to add file {0} to zip package", zipFileName), e);
                    throw e;
                }
            }
            return true;
        }
            
        private static bool CreateZipPackage(string[] filesToZip, string basePath, string zipFileName, bool keepPathInfo, int retryCount)
        {
            if (basePath.Trim().Length > 0 && !basePath.EndsWith(@"\"))
                basePath = basePath + @"\";

            List<string> fullPathFiles = new List<string>();
            foreach (string file in filesToZip)
            {
                fullPathFiles.Add(basePath + file);
            }
            return  CreateZipPackage(fullPathFiles, zipFileName, keepPathInfo, retryCount);
        }

        public static bool UnpackZipPackage(string destinationDir, string zipFileName)
        {
            try
            {
                string fileUnzipFullName;
                log.DebugFormat("Unzipping {0} to folder: {1}", zipFileName, destinationDir);
                if (!Directory.Exists(destinationDir))
                {
                    Directory.CreateDirectory(destinationDir);
                }
                using (ZipArchive archive = ZipFile.Open(zipFileName,ZipArchiveMode.Read))
                {
                    //Loops through each file in the zip file
                    foreach (ZipArchiveEntry file in archive.Entries)
                    {
                        //Identifies the destination file name and path
                        fileUnzipFullName = Path.Combine(destinationDir, file.Name);

                        if (!System.IO.File.Exists(fileUnzipFullName))
                        {
                            file.ExtractToFile(fileUnzipFullName);
                        }
                    }
                }
                return true;
            }
            catch (Exception exe)
            {
                log.Error("Unable to unzip package file", exe);
                return false;
            }
        }

        public static bool AppendZipPackage(string[] filesToZip, string basePath, string zipFileName, bool keepPathInfo)
        {
            try
            {
                using (ZipArchive modFile = ZipFile.Open(zipFileName, ZipArchiveMode.Update))
                {
                    foreach (var file in filesToZip)
                    {
                        log.DebugFormat("Adding files '{0}' to package zip file '{1}'", file, filesToZip);
                        modFile.CreateEntryFromFile(file, Path.GetFileName(file));
                    }

                }
                return true;
            }
            catch (Exception exe)
            {
                log.Error("Error adding files to package zip", exe);
                return false;
            }

           
        }
    }
}
