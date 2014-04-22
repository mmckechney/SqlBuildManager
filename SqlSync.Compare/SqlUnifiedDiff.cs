using System;
using SqlSync.SqlBuild;
using Algorithm.Diff;
using System.IO;
using System.Collections;
using System.Collections.Specialized;
using System.Text;
using System.Collections.Generic;
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

        public void GetUnifiedDiff(ref SqlSync.SqlBuild.SqlSyncBuildData leftBuildData,string leftTempFilePath, string rightBuildZipPath, out List<FileCompareResults> onlyInLeft, out List<FileCompareResults> onlyInRight, out List<FileCompareResults> modified, out string rightFileTempDirectory)
        {
            this.extractPathLeftFile = leftTempFilePath;
            string tmpDir = System.IO.Path.GetTempPath();
            this.extractPathRightFile = tmpDir + @"SqlsyncCompare-" + System.Guid.NewGuid().ToString().Replace("-", "") + @"\";
            Directory.CreateDirectory(this.extractPathRightFile);
            rightFileTempDirectory = this.extractPathRightFile;
            ZipHelper.UnpackZipPackage(this.extractPathRightFile, rightBuildZipPath);
            SqlSync.SqlBuild.SqlSyncBuildData rightBuildData = new SqlSyncBuildData();
            rightBuildData.ReadXml(this.extractPathRightFile + XmlFileNames.MainProjectFile);
            //TODO: wrap load failure. 

            List<FileCompareResults> filesOnlyInLeft = GetFileListDiff(leftBuildData, rightBuildData, true);
            List<FileCompareResults> filesOnlyInRight = GetFileListDiff(rightBuildData, leftBuildData, false);
            List<FileCompareResults> commonFiles = GetCommonFileList(leftBuildData, rightBuildData);

            modified = ProcessUnifiedDiff(this.extractPathLeftFile, this.extractPathRightFile, commonFiles);
            onlyInRight = GetFileContents(this.extractPathRightFile, filesOnlyInRight);
            onlyInLeft = GetFileContents(this.extractPathLeftFile, filesOnlyInLeft);

        }
        public List<FileCompareResults> GetFileContents(string basePath, List<FileCompareResults> fileList)
        {
            //string fileName;
            //string contents;
            List<FileCompareResults> tmpLst = new List<FileCompareResults>();
            for (int i = 0; i < fileList.Count; i++)
            {
                if (fileList[i].LeftScriptRow != null && File.Exists(basePath + fileList[i].LeftScriptRow.FileName))
                    fileList[i].LeftScriptText = File.ReadAllText(basePath + fileList[i].LeftScriptRow.FileName);

                if (fileList[i].RightScriptRow != null && File.Exists(basePath + fileList[i].RightScriptRow.FileName))
                    fileList[i].RightSciptText = File.ReadAllText(basePath + fileList[i].RightScriptRow.FileName);

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
                leftFile = leftPath + fileName;
                rightFile = rightPath + fileName;
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
                if (Directory.Exists(this.extractPathLeftFile) && cleanLeftFile)
                    Directory.Delete(this.extractPathLeftFile, true);

                if (Directory.Exists(this.extractPathRightFile) && cleanRightFile)
                    Directory.Delete(this.extractPathRightFile, true);
            }
            catch { }

        }
		
        #region .: File List Processing :.
		private Hashtable GetFileList(string dir)
		{
			Hashtable fileDic = new Hashtable();
			string[] files = Directory.GetFiles(dir);
			for(int i=0;i<files.Length;i++)
				if(!files[i].ToLower().EndsWith(".log") && !files[i].ToLower().EndsWith(".xml"))
					fileDic.Add(Path.GetFileName(files[i]),files[i]);

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
		
        //#region .: HTML Processing :.
        //public string GetHTMLDiff(string oldBuildFile, string newBuildFile, bool includeCSS)
        //{
        //    Hashtable added;
        //    Hashtable removed;
        //    Hashtable modified;
        //    GetUnifiedDiff(oldBuildFile, newBuildFile, out added, out removed, out modified);
        //    string html = HTMLTransformDiff(added, removed, modified);
        //    if (includeCSS)
        //    {
        //        string css = new SqlSync.Compare.ResourceHelper().GetFromResources("SqlSync.Compare.Default.css");
        //        return "<STYLE  type=\"text/css\">\r\n" + css + "\r\n</STYLE>\r\n" + html;
        //    }
        //    else
        //        return html;
        //}
        //private string HTMLTransformDiff(Hashtable added, Hashtable removed, Hashtable modified)
        //{
        //    StringBuilder sb = new StringBuilder();
        //    sb.Append("<DIV id=patch>\r\n");
        //    IDictionaryEnumerator enumer = added.GetEnumerator();
        //    while (enumer.MoveNext())
        //    {
        //        sb.Append(BuildHTMLFileDiff(enumer.Value.ToString(), enumer.Key.ToString(), "Added") + "\r\n");
        //    }

        //    enumer = modified.GetEnumerator();
        //    while (enumer.MoveNext())
        //    {
        //        sb.Append(BuildHTMLFileDiff(enumer.Value.ToString(), enumer.Key.ToString(), "Modified") + "\r\n");
        //    }

        //    enumer = removed.GetEnumerator();
        //    while (enumer.MoveNext())
        //    {
        //        sb.Append(BuildHTMLFileDiff(enumer.Value.ToString(), enumer.Key.ToString(), "Deleted") + "\r\n");
        //    }

        //    sb.Append("\r\n</DIV>");
        //    return sb.ToString();
        //}
        //private string FileNameAnchor(string input)
        //{
        //    input = input.Replace(" ", "");
        //    input = input.Replace("/", "");
        //    input = input.Replace(".", "");
        //    input = input.Replace("Modified:", "");
        //    input = input.Replace("Added:", "");
        //    input = input.Replace("Deleted:", "");
        //    return input.Trim();
        //}
        //private string BuildHTMLFileDiff(string unifiedDiffFormat,string fileName, string changeType)
        //{
        //    string[] lines = unifiedDiffFormat.Trim().Split('\n');
        //    if(lines.Length == 2)
        //        return string.Empty;

        //    StringBuilder diffLines = new StringBuilder();
        //    string template = SqlSync.Compare.Templates.FileDiffHTML;
        //    template = template.Replace("#fileDesc#", SqlSync.Compare.Templates.fileDesc.Replace("#fileDesc#", changeType+": "+fileName));
        //    template = template.Replace("#anchor#", FileNameAnchor(fileName));

        //    for (int i = 0; i < lines.Length; i++)
        //    {
        //        if (lines[i].StartsWith("---"))
        //        {
        //            //template = template.Replace("#prePost#", SqlSync.Compare.Templates.prePostVersion.Replace("#prePost#", lines[i].TrimEnd() + "<BR/>" + lines[i + 1].TrimEnd()));
        //            template = template.Replace("#prePost#", "");
        //            i++;
        //        }
        //        else if (lines[i].StartsWith("@@"))
        //            diffLines.Append("\r\n" + SqlSync.Compare.Templates.linesIndex.Replace("#index#", System.Web.HttpUtility.HtmlEncode(lines[i].TrimEnd())));
        //        else if (lines[i].StartsWith("+"))
        //            diffLines.Append(SqlSync.Compare.Templates.addLine.Replace("#line#", System.Web.HttpUtility.HtmlEncode(lines[i].TrimEnd())));
        //        else if (lines[i].StartsWith("-"))
        //            diffLines.Append(SqlSync.Compare.Templates.deleteLine.Replace("#line#", System.Web.HttpUtility.HtmlEncode(lines[i].TrimEnd())));
        //        else if (lines[i].StartsWith(" ") || lines[i].Trim().Length == 0)
        //            diffLines.Append(SqlSync.Compare.Templates.contextLine.Replace("#line#", System.Web.HttpUtility.HtmlEncode(lines[i].TrimEnd())));
        //        else if (lines[i].StartsWith("=="))
        //            continue;
        //        else
        //        {
        //            break;
        //        }
        //    }
        //    template = template.Replace("#changeLines#", diffLines.ToString());
        //    template = template.Replace("#prePost#", ""); // just in case (for deleted files)

        //    template = template.Replace("</DEL><DEL>", "\r\n");
        //    template = template.Replace("</INS><INS>", "\r\n");
        //    template = template.Replace("<SPAN class=cx></SPAN>", "");

        //    return template;
        //}
        //    #endregion
        

	}
}
