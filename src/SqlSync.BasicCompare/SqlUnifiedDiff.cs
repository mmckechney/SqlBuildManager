namespace SqlSync.BasicCompare
{
    using Algorithm.Diff;
    using SqlSync.SqlBuild;
    using System;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Web;

    public class SqlUnifiedDiff
    {
        private string extractPathNewFile;
        private string extractPathOldFile;

        private string BuildHTMLFileDiff(string unifiedDiffFormat, string fileName, string changeType)
        {
            string[] textArray = unifiedDiffFormat.Trim().Split(new char[] { '\n' });
            if (textArray.Length == 2)
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder();
            string text = "<A id=#anchor#></A>\r\n<DIV class=modfile>\r\n#fileDesc#\r\n<PRE class=diff>\r\n#prePost#\r\n<SPAN>\r\n#changeLines#\r\n</SPAN></PRE></DIV>\r\n<a href=#pagetop class=toplink>Return to Top</a>";
            text = text.Replace("#fileDesc#", "<H4>#fileDesc#</H4>".Replace("#fileDesc#", changeType + ": " + fileName)).Replace("#anchor#", FileNameAnchor(fileName));
            for (int i = 0; i < textArray.Length; i++)
            {
                if (textArray[i].StartsWith("---"))
                {
                    text = text.Replace("#prePost#", "");
                    i++;
                }
                else if (textArray[i].StartsWith("@@"))
                {
                    builder.Append("\r\n" + "<SPAN class=lines>#index#</SPAN>".Replace("#index#", HttpUtility.HtmlEncode(textArray[i].TrimEnd(new char[0]))));
                }
                else if (textArray[i].StartsWith("+"))
                {
                    builder.Append("<INS>#line#</INS>".Replace("#line#", HttpUtility.HtmlEncode(textArray[i].TrimEnd(new char[0]))));
                }
                else if (textArray[i].StartsWith("-"))
                {
                    builder.Append("<DEL>#line#</DEL>".Replace("#line#", HttpUtility.HtmlEncode(textArray[i].TrimEnd(new char[0]))));
                }
                else if (textArray[i].StartsWith(" ") || (textArray[i].Trim().Length == 0))
                {
                    builder.Append("<SPAN class=cx>#line#</SPAN>".Replace("#line#", HttpUtility.HtmlEncode(textArray[i].TrimEnd(new char[0]))));
                }
                else if (!textArray[i].StartsWith("=="))
                {
                    break;
                }
            }
            return text.Replace("#changeLines#", builder.ToString()).Replace("#prePost#", "").Replace("</DEL><DEL>", "\r\n").Replace("</INS><INS>", "\r\n").Replace("<SPAN class=cx></SPAN>", "");
        }

        private string BuildRtfFileDiff(string unifiedDiffFormat, string fileName, string changeType)
        {
            string[] textArray = unifiedDiffFormat.Trim().Split(new char[] { '\n' });
            if (textArray.Length == 2)
            {
                return string.Empty;
            }
            StringBuilder builder = new StringBuilder();
            builder.Append(@"\cf1\b\f0\fs22 #File Name#\par".Replace("#File Name#", changeType + ": " + fileName));
            for (int i = 0; i < textArray.Length; i++)
            {
                if (textArray[i].StartsWith("---"))
                {
                    i++;
                }
                else if (textArray[i].StartsWith("@@"))
                {
                    builder.Append(@"\cf2\b0\fs16 #Lines Effected#\par".Replace("#Lines Effected#", textArray[i].TrimEnd(new char[0])));
                }
                else if (textArray[i].StartsWith("+"))
                {
                    builder.Append(@"\cf4 #Added#\par".Replace("#Added#", textArray[i].TrimEnd(new char[0])));
                }
                else if (textArray[i].StartsWith("-"))
                {
                    builder.Append(@"\cf5 #Deleted#\par".Replace("#Deleted#", textArray[i].TrimEnd(new char[0])));
                }
                else if (textArray[i].StartsWith(" ") || (textArray[i].Trim().Length == 0))
                {
                    builder.Append(@"\cf3\fs18 #Reference Lines#\par".Replace("#Reference Lines#", textArray[i].TrimEnd(new char[0])));
                }
                else if (!textArray[i].StartsWith("=="))
                {
                    break;
                }
            }
            return builder.ToString();
        }

        private string FileNameAnchor(string input)
        {
            input = input.Replace(" ", "");
            input = input.Replace("/", "");
            input = input.Replace(".", "");
            input = input.Replace("Modified:", "");
            input = input.Replace("Added:", "");
            input = input.Replace("Deleted:", "");
            return input.Trim();
        }

        private Hashtable GetCommonFileList(Hashtable master, Hashtable child)
        {
            Hashtable hashtable = new Hashtable();
            IDictionaryEnumerator enumerator = master.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (child.ContainsKey(enumerator.Key))
                {
                    hashtable.Add(enumerator.Key, enumerator.Value);
                }
            }
            return hashtable;
        }

        private Hashtable GetFileList(string dir)
        {
            Hashtable hashtable = new Hashtable();
            string[] files = Directory.GetFiles(dir);
            for (int i = 0; i < files.Length; i++)
            {
                if (!files[i].ToLower().EndsWith(".log") && !files[i].ToLower().EndsWith(".xml"))
                {
                    hashtable.Add(Path.GetFileName(files[i]), files[i]);
                }
            }
            return hashtable;
        }

        private Hashtable GetFileListDiff(Hashtable master, Hashtable child)
        {
            Hashtable hashtable = new Hashtable();
            IDictionaryEnumerator enumerator = master.GetEnumerator();
            while (enumerator.MoveNext())
            {
                if (!child.ContainsKey(enumerator.Key))
                {
                    hashtable.Add(Path.GetFileName(enumerator.Key.ToString()), enumerator.Value);
                }
            }
            return hashtable;
        }

        public string GetHTMLDiff(string oldBuildFile, string newBuildFile, bool includeCSS)
        {
            Hashtable added;
            Hashtable removed;
            Hashtable modified;
            GetUnifiedDiff(oldBuildFile, newBuildFile, out added, out removed, out modified);
            string text = HTMLTransformDiff(added, removed, modified);
            if (includeCSS)
            {
                string fromResources = new ResourceHelper().GetFromResources("SQLSync.Compare.Default.css");
                return ("<STYLE  type=\"text/css\">\r\n" + fromResources + "\r\n</STYLE>\r\n" + text);
            }
            return text.Trim();
        }

        public string GetRtfDiff(string oldBuildFile, string newBuildFile)
        {
            Hashtable added;
            Hashtable removed;
            Hashtable modified;
            GetUnifiedDiff(oldBuildFile, newBuildFile, out added, out removed, out modified);
            return RtfTransformDiff(added, removed, modified);
        }

        public void GetUnifiedDiff(string oldBuildFile, string newBuildFile, out Hashtable added, out Hashtable removed, out Hashtable modified)
        {
            string tempPath = Path.GetTempPath();
            extractPathOldFile = tempPath + "SqlsyncCompare-" + Guid.NewGuid().ToString().Replace("-", "");
            Directory.CreateDirectory(extractPathOldFile);
            extractPathNewFile = tempPath + "SqlsyncCompare-" + Guid.NewGuid().ToString().Replace("-", "");
            Directory.CreateDirectory(extractPathNewFile);
            ZipHelper.UnpackZipPackage(extractPathOldFile, oldBuildFile, false);
            ZipHelper.UnpackZipPackage(extractPathNewFile, newBuildFile, false);
            Hashtable child = GetFileList(extractPathOldFile);
            Hashtable master = GetFileList(extractPathNewFile);
            Hashtable commonFiles = GetFileListDiff(master, child);
            Hashtable fileListDiff = GetFileListDiff(child, master);
            Hashtable commonFileList = GetCommonFileList(master, child);
            modified = ProcessUnifiedDiff(extractPathOldFile, extractPathNewFile, commonFileList);
            added = ProcessUnifiedDiff(extractPathOldFile, extractPathNewFile, commonFiles);
            removed = ProcessUnifiedDiff(extractPathOldFile, extractPathNewFile, fileListDiff);
            try
            {
                if (Directory.Exists(extractPathOldFile))
                {
                    Directory.Delete(extractPathOldFile, true);
                }
                if (Directory.Exists(extractPathNewFile))
                {
                    Directory.Delete(extractPathNewFile, true);
                }
            }
            catch
            {
            }
        }

        private string HTMLTransformDiff(Hashtable added, Hashtable removed, Hashtable modified)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("<DIV id=patch>\r\n");
            IDictionaryEnumerator enumerator = added.GetEnumerator();
            while (enumerator.MoveNext())
            {
                builder.Append(BuildHTMLFileDiff(enumerator.Value.ToString(), enumerator.Key.ToString(), "Added").Trim() + "\r\n");
            }
            enumerator = modified.GetEnumerator();
            while (enumerator.MoveNext())
            {
                builder.Append(BuildHTMLFileDiff(enumerator.Value.ToString(), enumerator.Key.ToString(), "Modified").Trim() + "\r\n");
            }
            enumerator = removed.GetEnumerator();
            while (enumerator.MoveNext())
            {
                builder.Append(BuildHTMLFileDiff(enumerator.Value.ToString(), enumerator.Key.ToString(), "Deleted").Trim() + "\r\n");
            }

            return builder.ToString().Trim() + "\r\n</DIV>";
        }

        private Hashtable ProcessUnifiedDiff(string oldPath, string newPath, Hashtable commonFiles)
        {
            Hashtable hashtable = new Hashtable();
            IDictionaryEnumerator enumerator = commonFiles.GetEnumerator();
            while (enumerator.MoveNext())
            {
                StringBuilder sb = new StringBuilder();
                StringWriter writer = new StringWriter(sb);
                string key = enumerator.Key.ToString();
                string path = Path.Combine(oldPath, key);
                string text3 = Path.Combine(newPath + key);
                string[] leftLines = new string[0];
                string[] rightLines = new string[0];
                if (File.Exists(text3))
                {
                    rightLines = File.OpenText(text3).ReadToEnd().Split(new char[] { '\n' });
                }
                if (File.Exists(path))
                {
                    leftLines = File.OpenText(path).ReadToEnd().Split(new char[] { '\n' });
                }
                UnifiedDiff.WriteUnifiedDiff(leftLines, path, rightLines, text3, writer, 1, false, false);
                hashtable.Add(key, sb.ToString());
            }
            return hashtable;
        }

        private string RtfTransformDiff(Hashtable added, Hashtable removed, Hashtable modified)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append("{\\rtf1\\fbidis\\ansi\\ansicpg1252\\deff0\\deflang1033{\\fonttbl{\\f0\\fswiss\\fprq2\\fcharset0 Arial;}{\\f1\\fswiss\\fprq2\\fcharset0 Verdana;}}\r\n{\\colortbl ;\\red51\\green51\\blue153;\\red153\\green153\\blue153;\\red0\\green0\\blue0;\\red0\\green128\\blue0;\\red251\\green87\\blue126;}\r\n\\viewkind4\\uc1\\pard\\ltrpar");
            IDictionaryEnumerator enumerator = added.GetEnumerator();
            while (enumerator.MoveNext())
            {
                builder.Append(BuildRtfFileDiff(enumerator.Value.ToString(), enumerator.Key.ToString(), "Added") + @"\par");
            }
            enumerator = modified.GetEnumerator();
            while (enumerator.MoveNext())
            {
                builder.Append(BuildRtfFileDiff(enumerator.Value.ToString(), enumerator.Key.ToString(), "Modified") + @"\par");
            }
            enumerator = removed.GetEnumerator();
            while (enumerator.MoveNext())
            {
                builder.Append(BuildRtfFileDiff(enumerator.Value.ToString(), enumerator.Key.ToString(), "Deleted") + @"\par");
            }
            builder.Append("}");
            return builder.ToString();
        }
    }
}
