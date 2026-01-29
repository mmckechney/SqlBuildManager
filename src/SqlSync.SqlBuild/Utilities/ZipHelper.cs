using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;

namespace SqlSync.SqlBuild
{
    /// <summary>
    /// Summary description for ZipHelper.
    /// </summary>
    public class ZipHelper
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
                string tempName = Path.Combine(Path.GetDirectoryName(zipFileName), @"~" + retryCount.ToString() + "~" + Path.GetFileName(zipFileName));
                using (ZipArchive newFile = ZipFile.Open(tempName, ZipArchiveMode.Create))
                {
                    foreach (string file in fullPathFilesToZip)
                    {
                        if (File.Exists(file) == false)
                            continue;
//TODO: raise a zip failure more aggressively?
                        try
                        {
                            newFile.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Fastest);
                        }
                        catch (IOException ioex)
                        {
                            // Skip locked files (e.g., log files held by another process)
                            log.LogWarning(ioex, $"Unable to add file {file} to zip package; skipping.");
                            continue;
                        }
                        catch (UnauthorizedAccessException uaex)
                        {
                            log.LogWarning(uaex, $"Unable to add file {file} to zip package due to access; skipping.");
                            continue;
                        }

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
                    log.LogError(e, $"Unable to add file {zipFileName} to zip package");
                    throw;
                }
            }
            return true;
        }

        private static bool CreateZipPackage(string[] filesToZip, string basePath, string zipFileName, bool keepPathInfo, int retryCount)
        {

            List<string> fullPathFiles = new List<string>();
            foreach (string file in filesToZip)
            {
                fullPathFiles.Add(Path.Combine(basePath, file));
            }
            return CreateZipPackage(fullPathFiles, zipFileName, keepPathInfo, retryCount);
        }

        public static bool UnpackZipPackage(string destinationDir, string zipFileName, bool overwriteExistingProjectFiles)
        {
            try
            {
                string fileUnzipFullName;
                log.LogDebug($"Unzipping {zipFileName} to folder: {destinationDir}");
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

                        if (!System.IO.File.Exists(fileUnzipFullName) || overwriteExistingProjectFiles)
                        {
                            file.ExtractToFile(fileUnzipFullName, overwriteExistingProjectFiles);
                        }
                    }
                }
                return true;
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Unable to unzip package file");
                return false;
            }
        }

        public static async Task<bool> UnpackZipPackageAsync(string destinationDir, string zipFileName, bool overwriteExistingProjectFiles, CancellationToken cancellationToken = default)
        {
            try
            {
                string fileUnzipFullName;
                log.LogDebug($"Unzipping {zipFileName} to folder: {destinationDir}");
                if (!Directory.Exists(destinationDir))
                {
                    Directory.CreateDirectory(destinationDir);
                }
                using (ZipArchive archive = ZipFile.Open(zipFileName, ZipArchiveMode.Read))
                {
                    foreach (ZipArchiveEntry file in archive.Entries)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        fileUnzipFullName = Path.Combine(destinationDir, file.Name);
                        if (!File.Exists(fileUnzipFullName) || overwriteExistingProjectFiles)
                        {
                            await using var entryStream = file.Open();
                            await using var outStream = new FileStream(fileUnzipFullName, FileMode.Create, FileAccess.Write, FileShare.None, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
                            await entryStream.CopyToAsync(outStream, 81920, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }
                return true;
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Unable to unzip package file async");
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
                        log.LogDebug($"Adding files '{file}' to package zip file '{filesToZip}'");
                        modFile.CreateEntryFromFile(file, Path.GetFileName(file), CompressionLevel.Fastest);
                    }

                }
                return true;
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Error adding files to package zip");
                return false;
            }


        }

        public static async Task<bool> AppendZipPackageAsync(string[] filesToZip, string basePath, string zipFileName, bool keepPathInfo, CancellationToken cancellationToken = default)
        {
            try
            {
                using (ZipArchive modFile = ZipFile.Open(zipFileName, ZipArchiveMode.Update))
                {
                    foreach (var file in filesToZip)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var fullPath = Path.Combine(basePath, file);
                        if (!File.Exists(fullPath)) continue;
                        log.LogDebug($"Adding file '{file}' to package zip file '{zipFileName}'");
                        var entry = modFile.CreateEntry(Path.GetFileName(file), CompressionLevel.Fastest);
                        await using var entryStream = entry.Open();
                        await using var fileStream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
                        await fileStream.CopyToAsync(entryStream, 81920, cancellationToken).ConfigureAwait(false);
                    }
                }
                return true;
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Unable to append to zip package async");
                return false;
            }
        }

        public static async Task<bool> CreateZipPackageAsync(List<string> fullPathFilesToZip, string zipFileName, bool keepPathInfo, int retryCount = 0, CancellationToken cancellationToken = default)
        {
            try
            {
                string tempName = Path.Combine(Path.GetDirectoryName(zipFileName), @"~" + retryCount.ToString() + "~" + Path.GetFileName(zipFileName));
                await using (var zipFs = new FileStream(tempName, FileMode.Create, FileAccess.ReadWrite, FileShare.None, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan))
                using (var archive = new ZipArchive(zipFs, ZipArchiveMode.Create, leaveOpen: false))
                {
                    foreach (string file in fullPathFilesToZip)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        if (!File.Exists(file)) continue;
                        try
                        {
                            var entry = archive.CreateEntry(Path.GetFileName(file), CompressionLevel.Fastest);
                            await using var entryStream = entry.Open();
                            await using var fileStream = new FileStream(file, FileMode.Open, FileAccess.Read, FileShare.Read, 81920, FileOptions.Asynchronous | FileOptions.SequentialScan);
                            await fileStream.CopyToAsync(entryStream, 81920, cancellationToken).ConfigureAwait(false);
                        }
                        catch (IOException ioex)
                        {
                            log.LogWarning(ioex, $"Unable to add file {file} to zip package; skipping.");
                            continue;
                        }
                        catch (UnauthorizedAccessException uaex)
                        {
                            log.LogWarning(uaex, $"Unable to add file {file} to zip package due to access; skipping.");
                            continue;
                        }
                    }
                }
                if (File.Exists(zipFileName)) File.Delete(zipFileName);
                File.Move(tempName, zipFileName);
            }
            catch (Exception e)
            {
                if (retryCount < 5)
                {
                    await Task.Delay(100, cancellationToken).ConfigureAwait(false);
                    return await CreateZipPackageAsync(fullPathFilesToZip, zipFileName, keepPathInfo, retryCount + 1, cancellationToken).ConfigureAwait(false);
                }
                log.LogError(e, $"Unable to add file {zipFileName} to zip package");
                throw;
            }
            return true;
        }

        public static Task<bool> CreateZipPackageAsync(string[] filesToZip, string basePath, string zipFileName, bool keepPathInfo, CancellationToken cancellationToken = default)
        {
            List<string> fullPathFiles = new List<string>();
            foreach (string file in filesToZip)
            {
                fullPathFiles.Add(Path.Combine(basePath, file));
            }
            return CreateZipPackageAsync(fullPathFiles, zipFileName, keepPathInfo, 0, cancellationToken);
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
            using (Stream stream = (Stream)File.Open(sourceFileName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
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

