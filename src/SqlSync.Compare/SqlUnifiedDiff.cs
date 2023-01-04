using Algorithm.Diff;
using SqlSync.SqlBuild;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
namespace SqlSync.Compare
{
    /// <summary>
    /// Summary description for UnifiedDiff.
    /// </summary>
    public class SqlUnifiedDiff
    {
        private string extractPathLeftFile;
        private string extractPathRightFile;
        public SqlUnifiedDiff()
        {

        }

        public void GetUnifiedDiff(ref SqlSync.SqlBuild.SqlSyncBuildData leftBuildData, string leftTempFilePath, string rightBuildZipPath, out List<FileCompareResults> onlyInLeft, out List<FileCompareResults> onlyInRight, out List<FileCompareResults> modified, out string rightFileTempDirectory)
        {
            extractPathLeftFile = leftTempFilePath;
            string tmpDir = System.IO.Path.GetTempPath();
            extractPathRightFile = tmpDir + @"SqlsyncCompare-" + System.Guid.NewGuid().ToString().Replace("-", "");
            Directory.CreateDirectory(extractPathRightFile);
            rightFileTempDirectory = extractPathRightFile;
            ZipHelper.UnpackZipPackage(extractPathRightFile, rightBuildZipPath, false);
            SqlSync.SqlBuild.SqlSyncBuildData rightBuildData = new SqlSyncBuildData();
            rightBuildData.ReadXml(Path.Combine(extractPathRightFile, XmlFileNames.MainProjectFile));
            //TODO: wrap load failure. 

            List<FileCompareResults> filesOnlyInLeft = GetFileListDiff(leftBuildData, rightBuildData, true);
            List<FileCompareResults> filesOnlyInRight = GetFileListDiff(rightBuildData, leftBuildData, false);
            List<FileCompareResults> commonFiles = GetCommonFileList(leftBuildData, rightBuildData);

            modified = ProcessUnifiedDiff(extractPathLeftFile, extractPathRightFile, commonFiles);
            onlyInRight = GetFileContents(extractPathRightFile, filesOnlyInRight);
            onlyInLeft = GetFileContents(extractPathLeftFile, filesOnlyInLeft);

        }
        public List<FileCompareResults> GetFileContents(string basePath, List<FileCompareResults> fileList)
        {
            //string fileName;
            //string contents;
            List<FileCompareResults> tmpLst = new List<FileCompareResults>();
            for (int i = 0; i < fileList.Count; i++)
            {
                string leftFullPath = (fileList[i].LeftScriptRow != null) ? Path.Combine(basePath, fileList[i].LeftScriptRow.FileName) : "";
                string rightFullPath = (fileList[i].RightScriptRow != null) ? Path.Combine(basePath, fileList[i].RightScriptRow.FileName) : "";

                if (fileList[i].LeftScriptRow != null && File.Exists(leftFullPath))
                {
                    fileList[i].LeftScriptText = File.ReadAllText(leftFullPath);
                }

                if (fileList[i].RightScriptRow != null && File.Exists(rightFullPath))
                {
                    fileList[i].RightSciptText = File.ReadAllText(rightFullPath);
                }

                tmpLst.Add(fileList[i]);
            }
            return tmpLst;
        }





        private List<FileCompareResults> ProcessUnifiedDiff(string leftPath, string rightPath, List<FileCompareResults> commonFiles)
        {

            string fileName;
            string leftFile;
            string rightFile;
            List<FileCompareResults> lst = new List<FileCompareResults>();

            for (int j = 0; j < commonFiles.Count; j++)
            {
                StringBuilder sb = new StringBuilder();
                StringWriter sw = new StringWriter(sb);
                fileName = commonFiles[j].LeftScriptRow.FileName;
                leftFile = Path.Combine(leftPath, fileName);
                rightFile = Path.Combine(rightPath, fileName);
                commonFiles[j].LeftScriptPath = leftFile;
                commonFiles[j].RightScriptPath = rightFile;

                lst.Add(ProcessUnifiedDiff(commonFiles[j]));
            }

            return lst;

        }
        public static FileCompareResults ProcessUnifiedDiff(FileCompareResults fileData)
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            string leftFile = fileData.LeftScriptPath;
            string rightFile = fileData.RightScriptPath;
            string[] leftContents = new string[0];
            string[] rightContents = new string[0];
            if (File.Exists(rightFile))
            {
                rightContents = File.OpenText(rightFile).ReadToEnd().Split('\n');
                for (int i = 0; i < rightContents.Length; i++)
                    rightContents[i] = rightContents[i].TrimEnd(); // trim the end because the diff blocks a a WriteLine, don't want extra \r\n's
            }

            if (File.Exists(leftFile))
            {
                leftContents = File.OpenText(leftFile).ReadToEnd().Split('\n');
                for (int i = 0; i < leftContents.Length; i++)
                    leftContents[i] = leftContents[i].TrimEnd(); // trim the end because the diff blocks a a WriteLine, don't want extra \r\n's
            }

            //Get the diff
            UnifiedDiff.WriteUnifiedDiff(leftContents, leftFile, rightContents, rightFile, sw, 500, false, false);

            //If there is not a real diff, just add the file contents, otherwise, add the unified diff text
            if (sb.ToString().Trim().Split('\r').Length < 3)
                fileData.LeftScriptText = String.Join("\r\n", leftContents);
            else
            {
                fileData.UnifiedDiffText = sb.ToString();
                fileData.LeftScriptText = String.Join("\r\n", leftContents);
                fileData.RightSciptText = String.Join("\r\n", rightContents);
            }

            return fileData;
        }


        public void CleanUpTempFiles(bool cleanLeftFile, bool cleanRightFile)
        {
            //Clean-up
            try
            {
                if (Directory.Exists(extractPathLeftFile) && cleanLeftFile)
                    Directory.Delete(extractPathLeftFile, true);

                if (Directory.Exists(extractPathRightFile) && cleanRightFile)
                    Directory.Delete(extractPathRightFile, true);
            }
            catch { }

        }

        #region .: File List Processing :.
        private Hashtable GetFileList(string dir)
        {
            Hashtable fileDic = new Hashtable();
            string[] files = Directory.GetFiles(dir);
            for (int i = 0; i < files.Length; i++)
                if (!files[i].ToLower().EndsWith(".log") && !files[i].ToLower().EndsWith(".xml"))
                    fileDic.Add(Path.GetFileName(files[i]), files[i]);

            return fileDic;
        }

        private List<FileCompareResults> GetFileListDiff(SqlSyncBuildData master, SqlSyncBuildData child, bool masterIsLeftFile)
        {
            bool foundScript = false;
            List<FileCompareResults> results = new List<FileCompareResults>();
            foreach (SqlSyncBuildData.ScriptRow masterRow in master.Script)
            {
                foreach (SqlSyncBuildData.ScriptRow childRow in child.Script)
                {
                    if (masterRow.FileName.Trim().ToLower() == childRow.FileName.Trim().ToLower())
                    {
                        foundScript = true;
                        break;
                    }
                }
                if (!foundScript)
                {
                    FileCompareResults result = new FileCompareResults();
                    if (masterIsLeftFile)
                    {
                        result.LeftScriptRow = masterRow;
                    }
                    else
                    {
                        result.RightScriptRow = masterRow;
                    }

                    results.Add(result);
                }

                foundScript = false;
            }
            return results;
        }
        private List<FileCompareResults> GetCommonFileList(SqlSyncBuildData left, SqlSyncBuildData right)
        {
            //bool foundScript = false;
            List<FileCompareResults> results = new List<FileCompareResults>();
            foreach (SqlSyncBuildData.ScriptRow leftRow in left.Script)
            {
                foreach (SqlSyncBuildData.ScriptRow rightRow in right.Script)
                {
                    if (leftRow.FileName.Trim().ToLower() == rightRow.FileName.Trim().ToLower())
                    {
                        FileCompareResults result = new FileCompareResults();
                        result.LeftScriptRow = leftRow;
                        result.RightScriptRow = rightRow; //Only file name, not full path yet
                        results.Add(result);
                    }
                }
            }
            return results;
        }


        #endregion

    }
}
