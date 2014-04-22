using System;
using System.IO;
using ICSharpCode.SharpZipLib;
using ICSharpCode.SharpZipLib.Checksums;
using ICSharpCode.SharpZipLib.Zip;
using ICSharpCode.SharpZipLib.Zip.Compression;
using ICSharpCode.SharpZipLib.Zip.Compression.Streams;
using System.Text;
using System.Collections.Generic;
using log4net;
using System.Threading;
using System.Threading.Tasks;
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

        #region Unused methods
        public static string GetFile(string zipFileName,string fileName)
		{
			UnicodeEncoding uniEncoding = new UnicodeEncoding();
			StringBuilder sb = new StringBuilder();
			System.IO.Stream inputStream = null;
			try
			{
				ICSharpCode.SharpZipLib.Zip.ZipFile myZipFile = new ICSharpCode.SharpZipLib.Zip.ZipFile(zipFileName);
				ICSharpCode.SharpZipLib.Zip.ZipEntry entry = myZipFile.GetEntry(Path.GetFileName(fileName));
				inputStream = myZipFile.GetInputStream(entry);
				int size = 2048;
				byte[] data = new byte[2048];
				while (true) 
				{
					size = inputStream.Read(data, 0, data.Length);
					if (size > 0) 
					{
						sb.Append(new ASCIIEncoding().GetString(data, 0, size));
					} 
					else 
					{
						break;
					}
				}
			}
			finally
			{
				if(inputStream != null)
					inputStream.Close();
			}
			return sb.ToString();
		}
		public static bool AddToZip(string zipFileName, string fileToZip)
		{
			if(File.Exists(fileToZip) == false)
			{
				return false;
			}

			Crc32 crc = new Crc32();
			ZipOutputStream s;
			if(File.Exists(zipFileName))
			{
				s = new ZipOutputStream(File.OpenWrite(zipFileName));
			}
			else
			{
				s = new ZipOutputStream(File.Create(zipFileName));
			}
		
			s.SetLevel(6); // 0 - store only to 9 - means best compression
			FileStream fs = File.OpenRead(fileToZip);
			
			byte[] buffer = new byte[fs.Length];
			fs.Read(buffer, 0, buffer.Length);
			ZipEntry entry = new ZipEntry(fileToZip);
			
			entry.DateTime = DateTime.Now;
			// set Size and the crc, because the information
			// about the size and crc should be stored in the header
			// if it is not set it is automatically written in the footer.
			// (in this case size == crc == -1 in the header)
			// Some ZIP programs have problems with zip files that don't store
			// the size and crc in the header.
			entry.Size = fs.Length;
			fs.Close();
			
			crc.Reset();
			crc.Update(buffer);
			
			entry.Crc  = crc.Value;
			
			s.PutNextEntry(entry);
			
			s.Write(buffer, 0, buffer.Length);
			
		
			s.Finish();
			s.Close();
			return true;
		}
		public static void ZipFile(string FileToZip, string ZipedFile)
		{
			if (! System.IO.File.Exists(FileToZip)) 
			{
				throw new System.IO.FileNotFoundException("The specified file " + FileToZip + " could not be found. Zipping aborderd");
			}
		
			System.IO.FileStream StreamToZip = new System.IO.FileStream(FileToZip,System.IO.FileMode.Open , System.IO.FileAccess.Read);
			System.IO.FileStream ZipFile;
			if(File.Exists(ZipedFile))
			{
				ZipFile = System.IO.File.OpenWrite(ZipedFile);
			}
			else
			{
				ZipFile = System.IO.File.Create(ZipedFile);
			}
			ZipFile.Seek(ZipFile.Length-1,SeekOrigin.End);
			ZipOutputStream ZipStream = new ZipOutputStream(ZipFile);
			ZipEntry ZipEntry = new ZipEntry(Path.GetFileName(FileToZip));
			ZipStream.PutNextEntry(ZipEntry);
			ZipStream.SetLevel(6);
			byte[] buffer = new byte[2048];
			System.Int32 size =StreamToZip.Read(buffer,0,buffer.Length);
			ZipStream.Write(buffer,0,size);
			try 
			{
				while (size < StreamToZip.Length) 
				{
					int sizeRead =StreamToZip.Read(buffer,0,buffer.Length);
					ZipStream.Write(buffer,0,sizeRead);
					size += sizeRead;
				}
			} 
			catch(System.Exception ex)
			{
				throw ex;
			}
			ZipStream.Finish();
			ZipStream.Close();
			StreamToZip.Close();
        }
        #endregion

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
            //var task = Task.Factory.StartNew(() =>
            // {
                 try
                 {
                     Crc32 crc = new Crc32();
                     string tempName = Path.GetDirectoryName(zipFileName) + @"\~" + retryCount.ToString() + "~" + Path.GetFileName(zipFileName);
                     ZipOutputStream s = new ZipOutputStream(File.Create(tempName));

                     s.SetLevel(5); // 0 - store only to 9 - means best compression

                     foreach (string file in fullPathFilesToZip)
                     {
                         if (File.Exists(file) == false)
                             continue;

                         FileStream fs = File.OpenRead(file);

                         byte[] buffer = new byte[fs.Length];
                         fs.Read(buffer, 0, buffer.Length);
                         ZipEntry entry;
                         if (keepPathInfo)
                             entry = new ZipEntry(file);
                         else
                             entry = new ZipEntry(Path.GetFileName(file));

                         entry.DateTime = DateTime.Now;

                         // set Size and the crc, because the information
                         // about the size and crc should be stored in the header
                         // if it is not set it is automatically written in the footer.
                         // (in this case size == crc == -1 in the header)
                         // Some ZIP programs have problems with zip files that don't store
                         // the size and crc in the header.
                         entry.Size = fs.Length;
                         fs.Close();

                         crc.Reset();
                         crc.Update(buffer);

                         entry.Crc = crc.Value;

                         s.PutNextEntry(entry);

                         s.Write(buffer, 0, buffer.Length);

                     }

                     s.Finish();
                     s.Close();
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
             //});
            //return true;
        }
        /// <summary>
        /// For some operations, there seems to be some race condition to save the Zip file. 
        /// Until that can be found, we'll try a recursive call to rememdy the problem.
        /// </summary>
        /// <param name="filesToZip"></param>
        /// <param name="basePath"></param>
        /// <param name="zipFileName"></param>
        /// <param name="keepPathInfo"></param>
        /// <param name="retryCount"></param>
        /// <returns></returns>
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
            //try
            //{
            //    Crc32 crc = new Crc32();
            //    string tempName = Path.GetDirectoryName(zipFileName) + @"\~" + retryCount.ToString() + "~"+Path.GetFileName(zipFileName);
            //    ZipOutputStream s = new ZipOutputStream(File.Create(tempName));

            //    s.SetLevel(5); // 0 - store only to 9 - means best compression

            //    foreach (string file in filesToZip)
            //    {
            //        if (File.Exists(basePath + file) == false)
            //            continue;

            //        FileStream fs = File.OpenRead(basePath + file);

            //        byte[] buffer = new byte[fs.Length];
            //        fs.Read(buffer, 0, buffer.Length);
            //        ZipEntry entry;
            //        if (keepPathInfo)
            //            entry = new ZipEntry(file);
            //        else
            //            entry = new ZipEntry(Path.GetFileName(file));

            //        entry.DateTime = DateTime.Now;

            //        // set Size and the crc, because the information
            //        // about the size and crc should be stored in the header
            //        // if it is not set it is automatically written in the footer.
            //        // (in this case size == crc == -1 in the header)
            //        // Some ZIP programs have problems with zip files that don't store
            //        // the size and crc in the header.
            //        entry.Size = fs.Length;
            //        fs.Close();

            //        crc.Reset();
            //        crc.Update(buffer);

            //        entry.Crc = crc.Value;

            //        s.PutNextEntry(entry);

            //        s.Write(buffer, 0, buffer.Length);

            //    }

            //    s.Finish();
            //    s.Close();
            //    if (File.Exists(zipFileName))
            //    {
            //        File.Delete(zipFileName);
            //    }
            //    File.Move(tempName, zipFileName);
            //}
            //catch (Exception e)
            //{
            //    if (retryCount < 5)
            //    {
            //        System.Threading.Thread.Sleep(100);
            //        return CreateZipPackage(filesToZip, basePath, zipFileName, keepPathInfo, retryCount + 1);
            //    }
            //    else
            //    {
            //        throw e;
            //    }
            //}
            //return true;
        }
		public static bool UnpackZipPackage(string destinationDir, string zipFileName)
		{
            log.DebugFormat("Unzipping {0} to folder: {1}", zipFileName, destinationDir);
			try
			{
                using (ZipInputStream s = new ZipInputStream(File.OpenRead(zipFileName)))
                {

                    ZipEntry theEntry;
                    while ((theEntry = s.GetNextEntry()) != null)
                    {
                        string fileName = Path.GetFileName(theEntry.Name);
                        if (fileName != String.Empty)
                        {
                            FileStream streamWriter = File.Create(destinationDir + @"\" + Path.GetFileName(theEntry.Name));
                            int size = 2048;
                            byte[] data = new byte[2048];
                            while (true)
                            {
                                size = s.Read(data, 0, data.Length);
                                if (size > 0)
                                {
                                    streamWriter.Write(data, 0, size);
                                }
                                else
                                {
                                    break;
                                }
                            }

                            streamWriter.Close();
                        }
                    }
                    s.Close();
                }
				return true;
			}
			catch(Exception e)
			{

				string error = e.ToString();
                log.ErrorFormat("Unable to UnZip package at {0} to the folder {1}.\r\nError message: {2}",zipFileName,destinationDir ,error);
				return false;
			}
			
		}
		public static bool AppendZipPackage(string[] filesToZip, string basePath, string zipFileName,bool keepPathInfo)
		{
			string tempPath = System.IO.Path.GetTempPath() +@"\" +System.Guid.NewGuid().ToString();
			try
			{
				System.Collections.ArrayList fullList = new System.Collections.ArrayList();
				if(File.Exists(zipFileName))
				{
					Directory.CreateDirectory(tempPath);
					if(UnpackZipPackage(tempPath,zipFileName))
						fullList.AddRange(Directory.GetFiles(tempPath));
					else
						return false;
				}

				for(int i=0;i<filesToZip.Length;i++)
					fullList.Add(basePath+filesToZip[i]);

				string[] arrFullList = new string[fullList.Count];
				fullList.CopyTo(arrFullList);

				return CreateZipPackage(arrFullList,string.Empty,zipFileName,keepPathInfo);
			}
			finally
			{
				if(Directory.Exists(tempPath))
					Directory.Delete(tempPath,true);
			}
		}
	}
}
