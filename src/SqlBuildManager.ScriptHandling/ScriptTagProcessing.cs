using Microsoft.Extensions.Logging;
using SqlBuildManager.Interfaces.ScriptHandling.Tags;
using SqlSync.SqlBuild;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
namespace SqlBuildManager.ScriptHandling
{
    public class ScriptTagProcessing
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        #region .: Methods to infer script tags :.
        /// <summary>
        /// Used to infer script tags for an entire build package
        /// </summary>
        /// <param name="buildData" >The SqlSyncBuildData object for the current package</param>
        /// <param name="projectPath">The path that the SBM is unpacked to</param>
        /// <param name="regexFormats">The Regex formats that will be used to try to extract script tags</param>
        /// <param name="source">The enum designating where to get the tag from</param>
        /// <returns>Boolean as to whether the inference worked</returns>
        public static bool InferScriptTags(ref SqlSyncBuildData buildData, string projectPath, List<string> regexFormats, TagInferenceSource source)
        {
            bool atLeastOneUpdated = false;
            string tmpTag = string.Empty;
            foreach (SqlSyncBuildData.ScriptRow row in buildData.Script)
            {
                tmpTag = InferScriptTag(source, regexFormats, row.FileName, projectPath);
                if (tmpTag.Length > 0)
                {
                    row.Tag = tmpTag;
                    atLeastOneUpdated = true;
                }
            }

            if (atLeastOneUpdated)
                buildData.AcceptChanges();

            return atLeastOneUpdated;

        }
        internal static string InferScriptTagFromFileName(string scriptFileName, List<string> regexFormats)
        {
            foreach (string regexFormat in regexFormats)
            {
                Regex tmpReg = new Regex(regexFormat, RegexOptions.IgnoreCase);
                if (tmpReg.Match(scriptFileName).Success)
                    return CleanTag(tmpReg.Match(scriptFileName).Value);
            }
            return string.Empty;
        }

        /// <summary>
        /// Cleans out any special characters (spaces, dashes and number signs)
        /// </summary>
        /// <param name="tag">The tag that has been extracted</param>
        /// <returns>The modified tag string</returns>
        private static string CleanTag(string tag)
        {
            return tag.Trim().Replace(" ", "").Replace("-", "").Replace("#", "");
        }
        /// <summary>
        /// Extracts a tag matching the specified regular expression from the script
        /// </summary>
        /// <param name="scriptContents">The text of the script</param>
        /// <param name="regexFormats">List of regular expression formats</param>
        /// <returns>The tag string found, empty string if not found</returns>
        internal static string InferScriptTagFromFileContents(string scriptContents, List<string> regexFormats)
        {
            if (regexFormats == null || regexFormats.Count == 0)
                return string.Empty;

            Match tmpLastMatch = null;
            foreach (string reg in regexFormats)
            {
                Regex tmpRegex = new Regex(reg, RegexOptions.IgnoreCase);
                MatchCollection tmpFileContents = tmpRegex.Matches(scriptContents);
                foreach (Match contentMatch in tmpFileContents)
                {
                    if (ScriptHandlingHelper.IsInLargeCommentHeader(scriptContents, contentMatch.Index))
                    {
                        if (tmpLastMatch == null || contentMatch.Index > tmpLastMatch.Index)
                            tmpLastMatch = contentMatch;
                    }
                }

            }
            if (tmpLastMatch == null)
                return string.Empty;
            else
                return CleanTag(tmpLastMatch.Value);
        }

        /// <summary>
        /// Gets a script tag from the text of the script or from the path name
        /// </summary>
        /// <param name="scriptPathAndName">The full name and path of the script</param>
        /// <param name="scriptContents">The text of the script</param>
        /// <param name="regexFormats">The regular expression formats used to extract the teg</param>
        /// <param name="source">The enum designating where to get the tag from</param>
        /// <returns>The extracted script tag or empty string if not found</returns>
        public static string InferScriptTag(string scriptPathAndName, string scriptContents, List<string> regexFormats, TagInferenceSource source)
        {
            if (regexFormats == null)
                return string.Empty;

            if (source == TagInferenceSource.None)
                return string.Empty;


            string fileContentTag = InferScriptTagFromFileContents(scriptContents, regexFormats);

            string fileNameTag = InferScriptTagFromFileName(scriptPathAndName, regexFormats);


            //Not able to get anything...
            if (fileContentTag.Length == 0 && fileNameTag.Length == 0)
                return string.Empty;

            if (fileNameTag.Length > 0 && source == TagInferenceSource.ScriptName)
                return fileNameTag;

            if (fileContentTag.Length > 0 && source == TagInferenceSource.ScriptText)
                return fileContentTag;

            if (source == TagInferenceSource.TextOverName)
            {
                if (fileContentTag.Length > 0)
                    return fileContentTag;
                else if (fileNameTag.Length > 0)
                    return fileNameTag;
            }

            if (source == TagInferenceSource.NameOverText)
            {
                if (fileNameTag.Length > 0)
                    return fileNameTag;
                else if (fileContentTag.Length > 0)
                    return fileContentTag;
            }

            return string.Empty;
        }
        /// <summary>
        /// Gets the script tag from a file name or file contents
        /// </summary>
        /// <param name="source">The enum designating where to get the tag from</param>>
        /// <param name="regexFormats">The regular expression formats used to extract the teg</param>
        /// <param name="scriptName">The name of the file</param>
        /// <param name="scriptPath">The path to the file</param>
        /// <returns>The extracted script tag or empty string if not found</returns>
        public static string InferScriptTag(TagInferenceSource source, List<string> regexFormats, string scriptName, string scriptPath)
        {
            log.LogDebug($"InferScriptTag: TagInferenceSource for {scriptName} is {Enum.GetName(typeof(TagInferenceSource), source)}");
            if (source == TagInferenceSource.None)
                return string.Empty;

            if (regexFormats == null)
            {
                log.LogWarning($"InferScriptTag: RegularExpression formats is null when processing {scriptName}");
                return string.Empty;
            }

            try
            {
                scriptName = Path.GetFileName(scriptName);

                string tmpTag;

                //Do this easier check first to avoid opening the file is possible...
                if (source == TagInferenceSource.ScriptName || source == TagInferenceSource.NameOverText)
                {
                    tmpTag = InferScriptTagFromFileName(scriptName, regexFormats);
                    if (tmpTag.Length > 0)
                    {
                        return tmpTag;
                    }

                }


                //If we get here, we will need to get the file contents...
                if (!File.Exists(Path.Combine(scriptPath, scriptName)))
                {
                    log.LogWarning($"Unable to find file for Script Tag Inference for file {scriptName} in path {scriptPath}");
                    return string.Empty;
                }

                string contents = File.ReadAllText(Path.Combine(scriptPath, scriptName));
                tmpTag = InferScriptTag(scriptName, contents, regexFormats, source);
                if (tmpTag.Length > 0)
                {
                    return tmpTag;
                }

            }
            catch (Exception exe)
            {
                log.LogError(exe, $"Error Inferring Script Tag for file {scriptName} in path {scriptPath}.");
            }
            return string.Empty;
        }
        #endregion
    }
}
