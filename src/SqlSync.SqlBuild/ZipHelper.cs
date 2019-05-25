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

        public static bool CreateZipPackage(string[] filesToZip, string basePath, string zipFileName, bool keepPathInfo)
        {
            return CreateZipPackage(filesToZip, basePath, zipFileName, keepPathInfo, 0);
        }
        public static bool CreateZipPackage(string[] filesToZip, string basePath, string zipFileName)
        {
            return CreateZipPackage(filesToZip, basePath, zipFileName, true);
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

                        newFile.CreateEntryFromFile(file, Path.GetFileName(file),CompressionLevel.Fastest);

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
            return CreateZipPackage(fullPathFiles, zipFileName, keepPathInfo, retryCount);
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
                using (ZipArchive archive = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
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
                        modFile.CreateEntryFromFile(file, Path.GetFileName(file),CompressionLevel.Fastest);
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
    public static class ZipFile
    {
        public static ZipArchive Open(string archiveFileName, ZipArchiveMode mode)
        {
            FileMode mode1;
            FileAccess access;
            FileShare share;
            switch (mode)
            {
                case ZipArchiveMode.Read:
                    mode1 = FileMode.Open;
                    access = FileAccess.Read;
                    share = FileShare.Read;
                    break;
                case ZipArchiveMode.Create:
                    mode1 = FileMode.CreateNew;
                    access = FileAccess.Write;
                    share = FileShare.None;
                    break;
                case ZipArchiveMode.Update:
                    mode1 = FileMode.OpenOrCreate;
                    access = FileAccess.ReadWrite;
                    share = FileShare.None;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(mode));
            }
            FileStream fileStream = (FileStream)null;
            try
            {
                fileStream = File.Open(archiveFileName, mode1, access, share);
                return new ZipArchive((Stream)fileStream, mode, false, null);
            }
            catch
            {
                fileStream?.Dispose();
                throw;
            }
        }
    }

    public static class ZipFileExtensions
    {
        public static ZipArchiveEntry CreateEntryFromFile(this ZipArchive destination, string sourceFileName, string entryName, CompressionLevel compressionLevel)
        {
            return ZipFileExtensions.DoCreateEntryFromFile(destination, sourceFileName, entryName, new CompressionLevel?(compressionLevel));
        }
        internal static ZipArchiveEntry DoCreateEntryFromFile(ZipArchive destination, string sourceFileName, string entryName, CompressionLevel? compressionLevel)
        {
            if (destination == null)
                throw new ArgumentNullException(nameof(destination));
            if (sourceFileName == null)
                throw new ArgumentNullException(nameof(sourceFileName));
            if (entryName == null)
                throw new ArgumentNullException(nameof(entryName));
            using (Stream stream = (Stream)File.Open(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                ZipArchiveEntry zipArchiveEntry = compressionLevel.HasValue ? destination.CreateEntry(entryName, compressionLevel.Value) : destination.CreateEntry(entryName);
                DateTime dateTime = File.GetLastWriteTime(sourceFileName);
                if (dateTime.Year < 1980 || dateTime.Year > 2107)
                    dateTime = new DateTime(1980, 1, 1, 0, 0, 0);
                zipArchiveEntry.LastWriteTime = (DateTimeOffset)dateTime;
                using (Stream destination1 = zipArchiveEntry.Open())
                    stream.CopyTo(destination1);
                return zipArchiveEntry;
            }
        }

        public static void ExtractToFile(this ZipArchiveEntry source, string destinationFileName)
        {
            source.ExtractToFile(destinationFileName, false);
        }

        public static void ExtractToFile(this ZipArchiveEntry source, string destinationFileName, bool overwrite)
        {
            if (source == null)
                throw new ArgumentNullException(nameof(source));
            if (destinationFileName == null)
                throw new ArgumentNullException(nameof(destinationFileName));
            FileMode mode = overwrite ? FileMode.Create : FileMode.CreateNew;
            using (Stream destination = (Stream)File.Open(destinationFileName, mode, FileAccess.Write, FileShare.None))
            {
                using (Stream stream = source.Open())
                    stream.CopyTo(destination);
            }
            File.SetLastWriteTime(destinationFileName, source.LastWriteTime.DateTime);
        }
    }
}

