using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.IO.Compression;
using System.Xml.Linq;
using SqlSync.SqlBuild.Models;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Identity.Client;

#nullable enable

namespace SqlSync.SqlBuild
{
    /// <summary>
    /// POCO-based XML serializer/deserializer for SqlSyncBuildDataModel compatible with legacy SqlSyncBuildData DataSet XML.
    /// </summary>
    public static class SqlSyncBuildDataXmlSerializer
    {
        private static readonly XNamespace Ns = "http://schemas.mckechney.com/SqlSyncBuildProject.xsd";

        public static SqlSyncBuildDataModel Load(string path)
        {
            using var stream = File.OpenRead(path);
            if (LooksLikeZip(stream))
            {
                stream.Position = 0;
                using var archive = new ZipArchive(stream, ZipArchiveMode.Read, leaveOpen: true);
                var entry = archive.GetEntry(XmlFileNames.MainProjectFile)
                           ?? archive.Entries.FirstOrDefault(e => e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));
                if (entry == null)
                    throw new InvalidOperationException($"Zip package '{path}' does not contain a project XML file.");
                using var entryStream = entry.Open();
                return Load(XDocument.Load(entryStream));
            }

            stream.Position = 0;
            return Load(XDocument.Load(stream));
        }

        private static bool LooksLikeZip(Stream stream)
        {
            if (!stream.CanSeek) return false;
            var pos = stream.Position;
            Span<byte> header = stackalloc byte[4];
            var read = stream.Read(header);
            stream.Position = pos;
            return read >= 2 && header[0] == 0x50 && header[1] == 0x4B; // 'PK'
        }

        private class Scripts
        {
            public List<Script> ScriptRows { get; } = new();
        }
        private class Builds
        {
            public List<Build> BuildRows { get; } = new();
        }
        public static SqlSyncBuildDataModel Load(XDocument doc)
        {
            if (doc.Root is null)
                throw new InvalidOperationException("SqlSyncBuildData XML has no root element.");


            var ns = doc.Root.GetDefaultNamespace();
            var projects = new List<SqlSyncBuildProject>();
            var scriptsTable = new List<Scripts>();
            var scriptRows = new List<Script>();
            var buildsTable = new List<Builds>();
            var buildRows = new List<Build>();
            var scriptRuns = new List<ScriptRun>();
            var committedScripts = new List<CommittedScript>();
            var codeReviews = new List<CodeReview>();

            int nextProjectId = 0;
            int nextScriptsId = 0;
            int nextBuildsId = 0;
            int nextBuildId = 0;

            foreach (var projElement in doc.Root.Elements(ns + "SqlSyncBuildProject"))
            {
                var projectId = nextProjectId++;
                var project = new SqlSyncBuildProject(
                    SqlSyncBuildProjectId: projectId,
                    ProjectName: (string?)projElement.Attribute("ProjectName"),
                    ScriptTagRequired: ParseBoolOrNull((string?)projElement.Attribute("ScriptTagRequired")));
                projects.Add(project);

                // Scripts container(s)
                var scriptsContainers = projElement.Elements(ns + "Scripts").ToList();
                if (scriptsContainers.Count == 0)
                {
                    // dataset would still have a Scripts table row (auto id) even if empty; mimic by creating one empty scripts row
                    var scriptsId = nextScriptsId++;
                    scriptsTable.Add(new Scripts());
                }
                else
                {
                    foreach (var scriptsContainer in scriptsContainers)
                    {
                        var scriptsId = nextScriptsId++;
                        scriptsTable.Add(new Scripts());
                        foreach (var s in scriptsContainer.Elements(ns + "Script"))
                        {
                            scriptRows.Add(ParseScript(s, scriptsId));
                        }
                    }
                }

                // Builds container(s)
                var buildsContainers = projElement.Elements(ns + "Builds").ToList();
                if (buildsContainers.Count == 0)
                {
                    // dataset would still have a Builds row; mimic
                    var buildsId = nextBuildsId++;
                    buildsTable.Add(new Builds());
                }
                else
                {
                    foreach (var buildsContainer in buildsContainers)
                    {
                        var buildsId = nextBuildsId++;
                        buildsTable.Add(new Builds());
                        foreach (var b in buildsContainer.Elements(ns + "Build"))
                        {
                            var buildId = nextBuildId++;
                            buildRows.Add(ParseBuild(b, buildsId, buildId));
                            foreach (var sr in b.Elements(ns + "ScriptRun"))
                            {
                                scriptRuns.Add(ParseScriptRun(sr, buildId.ToString()));
                            }
                        }
                    }
                }

                // CommittedScript entries
                foreach (var cs in projElement.Elements(ns + "CommittedScript"))
                {
                    committedScripts.Add(ParseCommittedScript(cs, projectId));
                }
            }

            // CodeReview entries at root level
            foreach (var cr in doc.Root.Elements(ns + "CodeReview"))
            {
                codeReviews.Add(ParseCodeReview(cr));
            }

            return new SqlSyncBuildDataModel(projects, scriptRows, buildRows, scriptRuns, committedScripts, codeReviews);
        }

        private static Script ParseScript(XElement el, int scriptsId)
        {
            return new Script(
                FileName: (string?)el.Attribute("FileName"),
                BuildOrder: ParseDoubleOrNull((string?)el.Attribute("BuildOrder")),
                Description: (string?)el.Attribute("Description"),
                RollBackOnError: ParseBoolOrNull((string?)el.Attribute("RollBackOnError")),
                CausesBuildFailure: ParseBoolOrNull((string?)el.Attribute("CausesBuildFailure")),
                DateAdded: ParseDateTimeOrNull((string?)el.Attribute("DateAdded")),
                ScriptId: (string?)el.Attribute("ScriptId"),
                Database: (string?)el.Attribute("Database"),
                StripTransactionText: ParseBoolOrNull((string?)el.Attribute("StripTransactionText")),
                AllowMultipleRuns: ParseBoolOrNull((string?)el.Attribute("AllowMultipleRuns")),
                AddedBy: (string?)el.Attribute("AddedBy"),
                ScriptTimeOut: ParseIntOrNull((string?)el.Attribute("ScriptTimeOut")),
                DateModified: ParseDateTimeOrNull((string?)el.Attribute("DateModified")),
                ModifiedBy: (string?)el.Attribute("ModifiedBy"),
                   Tag: (string?)el.Attribute("Tag"));
        }

        private static Build ParseBuild(XElement el, int buildsId, int buildId)
        {
            var finalStat = Enum.TryParse<BuildItemStatus>((string?)el.Attribute("FinalStatus"), out var finalStatus) ? finalStatus : BuildItemStatus.Unknown;
            return new Build(
                Name: (string?)el.Attribute("Name"),
                BuildType: (string?)el.Attribute("BuildType"),
                BuildStart: ParseDateTimeOrNull((string?)el.Attribute("BuildStart")),
                BuildEnd: ParseDateTimeOrNull((string?)el.Attribute("BuildEnd")),
                ServerName: (string?)el.Attribute("ServerName"),
                FinalStatus: finalStatus,
                BuildId: (string?)el.Attribute("BuildId"),
                UserId: (string?)el.Attribute("UserId"));
        }

        private static ScriptRun ParseScriptRun(XElement el, string buildId)
        {
            return new ScriptRun(
                FileHash: (string?)el.Element(Ns + "FileHash"),
                Results: (string?)el.Element(Ns + "Results"),
                FileName: (string?)el.Attribute("FileName"),
                RunOrder: ParseDoubleOrNull((string?)el.Attribute("RunOrder")),
                RunStart: ParseDateTimeOrNull((string?)el.Attribute("RunStart")),
                RunEnd: ParseDateTimeOrNull((string?)el.Attribute("RunEnd")),
                Success: ParseBoolOrNull((string?)el.Attribute("Success")),
                Database: (string?)el.Attribute("Database"),
                ScriptRunId: (string?)el.Attribute("ScriptRunId"),
                BuildId: buildId);
        }

        private static CommittedScript ParseCommittedScript(XElement el, int projectId)
        {
            return new CommittedScript(
                ScriptId: (string?)el.Attribute("ScriptId"),
                ServerName: (string?)el.Attribute("ServerName"),
                CommittedDate: ParseDateTimeOrNull((string?)el.Attribute("CommittedDate")),
                AllowScriptBlock: ParseBoolOrNull((string?)el.Attribute("AllowScriptBlock")),
                ScriptHash: (string?)el.Attribute("ScriptHash"),
                SqlSyncBuildProjectId: projectId);
        }

        private static CodeReview ParseCodeReview(XElement el)
        {
            return new CodeReview(
                CodeReviewId: ParseGuidOrNull((string?)el.Element(Ns + "CodeReviewId")),
                ScriptId: (string?)el.Element(Ns + "ScriptId"),
                ReviewDate: ParseDateTimeOrNull((string?)el.Element(Ns + "ReviewDate")),
                ReviewBy: (string?)el.Element(Ns + "ReviewBy"),
                ReviewStatus: ParseShortOrNull((string?)el.Element(Ns + "ReviewStatus")),
                Comment: (string?)el.Element(Ns + "Comment"),
                ReviewNumber: (string?)el.Element(Ns + "ReviewNumber"),
                CheckSum: (string?)el.Element(Ns + "CheckSum"),
                ValidationKey: (string?)el.Element(Ns + "ValidationKey"));
        }

        public static void Save(string path, SqlSyncBuildDataModel model)
        {
            var doc = BuildDocument(model);
            using var writer = XmlWriter.Create(path, new XmlWriterSettings
            {
                Indent = true,
                Encoding = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true),
            });
            doc.Save(writer);
        }

        public static XDocument BuildDocument(SqlSyncBuildDataModel model)
        {
            var root = new XElement(Ns + "SqlSyncBuildData");
            // Project tree
            foreach (var proj in model.SqlSyncBuildProject)
            {
                var projEl = new XElement(Ns + "SqlSyncBuildProject");
                if (proj.ProjectName is not null) projEl.SetAttributeValue("ProjectName", proj.ProjectName);
                if (proj.ScriptTagRequired.HasValue) projEl.SetAttributeValue("ScriptTagRequired", FormatBool(proj.ScriptTagRequired.Value));

                var scriptsGroups = model.Script.ToList();
                if (scriptsGroups.Count == 0)
                {
                    projEl.Add(new XElement(Ns + "Scripts"));
                }
                else
                {
                    foreach (var sg in scriptsGroups)
                    {
                        var scriptsEl = new XElement(Ns + "Scripts");
                        var scripts = model.Script.ToList();
                        foreach (var s in scripts)
                        {
                            var scriptEl = new XElement(Ns + "Script");
                            SetAttr(scriptEl, "FileName", s.FileName);
                            SetAttr(scriptEl, "BuildOrder", s.BuildOrder);
                            SetAttr(scriptEl, "Description", s.Description);
                            SetAttr(scriptEl, "RollBackOnError", s.RollBackOnError);
                            SetAttr(scriptEl, "CausesBuildFailure", s.CausesBuildFailure);
                            SetAttr(scriptEl, "DateAdded", s.DateAdded);
                            SetAttr(scriptEl, "ScriptId", s.ScriptId);
                            SetAttr(scriptEl, "Database", s.Database);
                            SetAttr(scriptEl, "StripTransactionText", s.StripTransactionText);
                            SetAttr(scriptEl, "AllowMultipleRuns", s.AllowMultipleRuns);
                            SetAttr(scriptEl, "AddedBy", s.AddedBy);
                            SetAttr(scriptEl, "ScriptTimeOut", s.ScriptTimeOut);
                            SetAttr(scriptEl, "DateModified", s.DateModified);
                            SetAttr(scriptEl, "ModifiedBy", s.ModifiedBy);
                            SetAttr(scriptEl, "Tag", s.Tag);
                            scriptsEl.Add(scriptEl);
                        }
                        projEl.Add(scriptsEl);
                    }
                }

                var buildGroups = model.Build.GroupBy(b => b.BuildId).ToList();
                var buildsEl = new XElement(Ns + "Builds");

                foreach (var bg in buildGroups)
                {
                    var builds = bg.ToList().OrderBy(b => b.BuildStart).ToList();
                    foreach (var b in builds)
                    {
                        var buildEl = new XElement(Ns + "Build");
                        SetAttr(buildEl, "Name", b.Name);
                        SetAttr(buildEl, "BuildType", b.BuildType);
                        SetAttr(buildEl, "BuildStart", b.BuildStart);
                        SetAttr(buildEl, "BuildEnd", b.BuildEnd);
                        SetAttr(buildEl, "ServerName", b.ServerName);
                        SetAttr(buildEl, "FinalStatus", b.FinalStatus.ToString());
                        SetAttr(buildEl, "BuildId", b.BuildId);
                        SetAttr(buildEl, "UserId", b.UserId);

                        var runs = model.ScriptRun.Where(sr => sr.BuildId == b.BuildId).ToList();
                        foreach (var sr in runs)
                        {
                            var srEl = new XElement(Ns + "ScriptRun");
                            SetAttr(srEl, "FileName", sr.FileName);
                            SetAttr(srEl, "RunOrder", sr.RunOrder);
                            SetAttr(srEl, "RunStart", sr.RunStart);
                            SetAttr(srEl, "RunEnd", sr.RunEnd);
                            SetAttr(srEl, "Success", sr.Success);
                            SetAttr(srEl, "Database", sr.Database);
                            SetAttr(srEl, "ScriptRunId", sr.ScriptRunId);
                            if (sr.FileHash is not null) srEl.Add(new XElement(Ns + "FileHash", sr.FileHash));
                            if (sr.Results is not null) srEl.Add(new XElement(Ns + "Results", sr.Results));
                            buildEl.Add(srEl);
                        }

                        buildsEl.Add(buildEl);
                    }
                    projEl.Add(buildsEl);
                }


                var committed = model.CommittedScript.Where(c => c.SqlSyncBuildProjectId == proj.SqlSyncBuildProjectId).ToList();
                foreach (var c in committed)
                {
                    var csEl = new XElement(Ns + "CommittedScript");
                    SetAttr(csEl, "ScriptId", c.ScriptId);
                    SetAttr(csEl, "ServerName", c.ServerName);
                    SetAttr(csEl, "CommittedDate", c.CommittedDate);
                    SetAttr(csEl, "AllowScriptBlock", c.AllowScriptBlock);
                    SetAttr(csEl, "ScriptHash", c.ScriptHash);
                    projEl.Add(csEl);
                }

                root.Add(projEl);
            }

            foreach (var cr in model.CodeReview)
            {
                var crEl = new XElement(Ns + "CodeReview");
                if (cr.CodeReviewId.HasValue) crEl.Add(new XElement(Ns + "CodeReviewId", cr.CodeReviewId.Value.ToString("D")));
                if (cr.ScriptId is not null) crEl.Add(new XElement(Ns + "ScriptId", cr.ScriptId));
                if (cr.ReviewDate.HasValue) crEl.Add(new XElement(Ns + "ReviewDate", FormatDate(cr.ReviewDate.Value)));
                if (cr.ReviewBy is not null) crEl.Add(new XElement(Ns + "ReviewBy", cr.ReviewBy));
                if (cr.ReviewStatus.HasValue) crEl.Add(new XElement(Ns + "ReviewStatus", cr.ReviewStatus.Value.ToString(CultureInfo.InvariantCulture)));
                if (cr.Comment is not null) crEl.Add(new XElement(Ns + "Comment", cr.Comment));
                if (cr.ReviewNumber is not null) crEl.Add(new XElement(Ns + "ReviewNumber", cr.ReviewNumber));
                if (cr.CheckSum is not null) crEl.Add(new XElement(Ns + "CheckSum", cr.CheckSum));
                if (cr.ValidationKey is not null) crEl.Add(new XElement(Ns + "ValidationKey", cr.ValidationKey));
                root.Add(crEl);
            }

            var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root);
            return doc;
        }

        private static bool? ParseBoolOrNull(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (bool.TryParse(value, out var b)) return b;
            return null;
        }

        private static int? ParseIntOrNull(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (int.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n)) return n;
            return null;
        }

        private static short? ParseShortOrNull(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (short.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var n)) return n;
            return null;
        }

        private static double? ParseDoubleOrNull(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (double.TryParse(value, NumberStyles.Float | NumberStyles.AllowThousands, CultureInfo.InvariantCulture, out var n)) return n;
            return null;
        }

        private static DateTime? ParseDateTimeOrNull(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            try { return XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.RoundtripKind); }
            catch { return null; }
        }

        private static Guid? ParseGuidOrNull(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            if (Guid.TryParse(value, out var g)) return g;
            return null;
        }

        private static void SetAttr(XElement el, string name, string? value)
        {
            if (value is not null)
                el.SetAttributeValue(name, value);
        }

        private static void SetAttr(XElement el, string name, bool? value)
        {
            if (value.HasValue)
                el.SetAttributeValue(name, FormatBool(value.Value));
        }

        private static void SetAttr(XElement el, string name, int? value)
        {
            if (value.HasValue)
                el.SetAttributeValue(name, value.Value.ToString(CultureInfo.InvariantCulture));
        }

        private static void SetAttr(XElement el, string name, double? value)
        {
            if (value.HasValue)
                el.SetAttributeValue(name, value.Value.ToString(CultureInfo.InvariantCulture));
        }

        private static void SetAttr(XElement el, string name, DateTime? value)
        {
            if (value.HasValue)
                el.SetAttributeValue(name, FormatDate(value.Value));
        }

        private static string FormatBool(bool value) => value ? "true" : "false";

        private static string FormatDate(DateTime value) => XmlConvert.ToString(value, XmlDateTimeSerializationMode.RoundtripKind);
    }
}
