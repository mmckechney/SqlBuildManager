using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.IO.Compression;
using System.Xml.Linq;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Identity.Client;
using System.Threading;
using System.Threading.Tasks;

#nullable enable

namespace SqlSync.SqlBuild.Models
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

        public static async Task<SqlSyncBuildDataModel> LoadAsync(string path, CancellationToken cancellationToken = default)
        {
            await using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read, bufferSize: 4096, useAsync: true);
            var buffer = new byte[stream.Length];
            await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken).ConfigureAwait(false);
            
            using var memStream = new MemoryStream(buffer);
            if (LooksLikeZip(memStream))
            {
                memStream.Position = 0;
                using var archive = new ZipArchive(memStream, ZipArchiveMode.Read, leaveOpen: true);
                var entry = archive.GetEntry(XmlFileNames.MainProjectFile)
                           ?? archive.Entries.FirstOrDefault(e => e.FullName.EndsWith(".xml", StringComparison.OrdinalIgnoreCase));
                if (entry == null)
                    throw new InvalidOperationException($"Zip package '{path}' does not contain a project XML file.");
                using var entryStream = entry.Open();
                return Load(XDocument.Load(entryStream));
            }

            memStream.Position = 0;
            return Load(XDocument.Load(memStream));
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

            int nextProjectId = 0;
            int nextScriptsId = 0;
            int nextBuildsId = 0;
            int nextBuildId = 0;

            foreach (var projElement in doc.Root.Elements(ns + "SqlSyncBuildProject"))
            {
                var projectId = nextProjectId++;
                var project = new SqlSyncBuildProject(
                    sqlSyncBuildProjectId: projectId,
                    projectName: (string?)projElement.Attribute("ProjectName"),
                    scriptTagRequired: ParseBoolOrNull((string?)projElement.Attribute("ScriptTagRequired")));
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

            return new SqlSyncBuildDataModel(projects, scriptRows, buildRows, scriptRuns, committedScripts);
        }

        private static Script ParseScript(XElement el, int scriptsId)
        {
            return new Script(
                fileName: (string?)el.Attribute("FileName"),
                buildOrder: ParseDoubleOrNull((string?)el.Attribute("BuildOrder")),
                description: (string?)el.Attribute("Description"),
                rollBackOnError: ParseBoolOrNull((string?)el.Attribute("RollBackOnError")),
                causesBuildFailure: ParseBoolOrNull((string?)el.Attribute("CausesBuildFailure")),
                dateAdded: ParseDateTimeOrNull((string?)el.Attribute("DateAdded")),
                scriptId: (string?)el.Attribute("ScriptId"),
                database: (string?)el.Attribute("Database"),
                stripTransactionText: ParseBoolOrNull((string?)el.Attribute("StripTransactionText")),
                allowMultipleRuns: ParseBoolOrNull((string?)el.Attribute("AllowMultipleRuns")),
                addedBy: (string?)el.Attribute("AddedBy"),
                scriptTimeOut: ParseIntOrNull((string?)el.Attribute("ScriptTimeOut")),
                dateModified: ParseDateTimeOrNull((string?)el.Attribute("DateModified")),
                modifiedBy: (string?)el.Attribute("ModifiedBy"),
                tag: (string?)el.Attribute("Tag"));
        }

        private static Build ParseBuild(XElement el, int buildsId, int buildId)
        {
            var finalStat = Enum.TryParse<BuildItemStatus>((string?)el.Attribute("FinalStatus"), out var finalStatus) ? finalStatus : BuildItemStatus.Unknown;
            return new Build(
                name: (string?)el.Attribute("Name"),
                buildType: (string?)el.Attribute("BuildType"),
                buildStart: ParseDateTimeOrNull((string?)el.Attribute("BuildStart")),
                buildEnd: ParseDateTimeOrNull((string?)el.Attribute("BuildEnd")),
                serverName: (string?)el.Attribute("ServerName"),
                finalStatus: finalStatus,
                buildId: (string?)el.Attribute("BuildId"),
                userId: (string?)el.Attribute("UserId"));
        }

        private static ScriptRun ParseScriptRun(XElement el, string buildId)
        {
            return new ScriptRun(
                fileHash: (string?)el.Element(Ns + "FileHash"),
                results: (string?)el.Element(Ns + "Results"),
                fileName: (string?)el.Attribute("FileName"),
                runOrder: ParseDoubleOrNull((string?)el.Attribute("RunOrder")),
                runStart: ParseDateTimeOrNull((string?)el.Attribute("RunStart")),
                runEnd: ParseDateTimeOrNull((string?)el.Attribute("RunEnd")),
                success: ParseBoolOrNull((string?)el.Attribute("Success")),
                database: (string?)el.Attribute("Database"),
                scriptRunId: (string?)el.Attribute("ScriptRunId"),
                buildId: buildId);
        }

        private static CommittedScript ParseCommittedScript(XElement el, int projectId)
        {
            return new CommittedScript(
                scriptId: (string?)el.Attribute("ScriptId"),
                serverName: (string?)el.Attribute("ServerName"),
                committedDate: ParseDateTimeOrNull((string?)el.Attribute("CommittedDate")),
                allowScriptBlock: ParseBoolOrNull((string?)el.Attribute("AllowScriptBlock")),
                scriptHash: (string?)el.Attribute("ScriptHash"),
                sqlSyncBuildProjectId: projectId);
        }

        public static async Task SaveAsync(string path, SqlSyncBuildDataModel model)
        {
            await Task.Run(() =>
            {
                var doc = BuildDocument(model);
                using var writer = XmlWriter.Create(path, new XmlWriterSettings
                {
                    Indent = true,
                    Encoding = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true),
                });
                doc.Save(writer);
            });
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
                    if (scripts.Count > 0 || true) // Always add Scripts element
                    {
                        projEl.Add(scriptsEl);
                    }
                    
                }


                var buildsEl = new XElement(Ns + "Builds");

                    var builds = model.Build.ToList().OrderBy(b => b.BuildStart).ToList();
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
