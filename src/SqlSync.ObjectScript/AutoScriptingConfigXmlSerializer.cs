using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using SqlSync.ObjectScript.Models;

#nullable enable

namespace SqlSync.ObjectScript
{
    /// <summary>
    /// POCO-based XML serializer/deserializer for AutoScriptingConfigModel compatible with legacy DataSet XML.
    /// </summary>
    public static class AutoScriptingConfigXmlSerializer
    {
        private static readonly XNamespace Ns = "http://www.mckechney.com/AutoScriptingConfig.xsd";

        public static AutoScriptingConfigModel Load(string path)
        {
            using var stream = File.OpenRead(path);
            var doc = XDocument.Load(stream);
            return Load(doc);
        }

        public static AutoScriptingConfigModel Load(XDocument doc)
        {
            if (doc.Root is null)
                throw new InvalidOperationException("AutoScriptingConfig XML has no root element.");

            var ns = doc.Root.GetDefaultNamespace();
            var autoScriptingList = new List<AutoScripting>();
            var dbConfigList = new List<DatabaseScriptConfig>();
            var postActionList = new List<PostScriptingAction>();

            int nextAutoId = 0;

            foreach (var auto in doc.Root.Elements(ns + "AutoScripting"))
            {
                var autoId = nextAutoId++;
                var autoRec = new AutoScripting(
                    AllowManualSelection: ParseBoolOrNull((string?)auto.Attribute("AllowManualSelection")),
                    IncludeFileHeaders: ParseBoolOrNull((string?)auto.Attribute("IncludeFileHeaders")),
                    DeletePreExistingFiles: ParseBoolOrNull((string?)auto.Attribute("DeletePreExistingFiles")),
                    ZipScripts: ParseBoolOrNull((string?)auto.Attribute("ZipScripts")),
                    AutoScripting_Id: autoId);
                autoScriptingList.Add(autoRec);

                var dbConfigEl = auto.Element(ns + "DatabaseScriptConfig");
                if (dbConfigEl is not null)
                {
                    var dbCfg = new DatabaseScriptConfig(
                        ServerName: (string?)dbConfigEl.Attribute("ServerName"),
                        DatabaseName: (string?)dbConfigEl.Attribute("DatabaseName"),
                        UserName: (string?)dbConfigEl.Attribute("UserName"),
                        Password: (string?)dbConfigEl.Attribute("Password"),
                        AuthenticationType: (string?)dbConfigEl.Attribute("AuthenticationType"),
                        ScriptToPath: (string?)dbConfigEl.Element(ns + "ScriptToPath") ?? string.Empty,
                        AutoScripting_Id: autoId);
                    dbConfigList.Add(dbCfg);
                }

                var postActionsEl = auto.Element(ns + "PostScriptingAction");
                if (postActionsEl is not null)
                {
                    var pa = new PostScriptingAction(
                        Name: (string?)postActionsEl.Attribute("Name"),
                        Command: (string?)postActionsEl.Attribute("Command"),
                        Arguments: (string?)postActionsEl.Attribute("Arguments"),
                        AutoScripting_Id: autoId);
                    postActionList.Add(pa);
                }
            }

            return new AutoScriptingConfigModel(autoScriptingList, dbConfigList, postActionList);
        }

        public static void Save(string path, AutoScriptingConfigModel model)
        {
            var doc = BuildDocument(model);
            using var writer = XmlWriter.Create(path, new XmlWriterSettings
            {
                Indent = true,
                Encoding = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true),
            });
            doc.Save(writer);
        }

        public static XDocument BuildDocument(AutoScriptingConfigModel model)
        {
            var root = new XElement(Ns + "AutoScriptingConfig");

            foreach (var auto in model.AutoScripting)
            {
                var autoEl = new XElement(Ns + "AutoScripting");
                SetAttr(autoEl, "AllowManualSelection", auto.AllowManualSelection);
                SetAttr(autoEl, "IncludeFileHeaders", auto.IncludeFileHeaders);
                SetAttr(autoEl, "DeletePreExistingFiles", auto.DeletePreExistingFiles);
                SetAttr(autoEl, "ZipScripts", auto.ZipScripts);

                var dbCfg = model.DatabaseScriptConfig.FirstOrDefault(d => d.AutoScripting_Id == auto.AutoScripting_Id);
                if (dbCfg is not null)
                {
                    var dbEl = new XElement(Ns + "DatabaseScriptConfig");
                    SetAttr(dbEl, "ServerName", dbCfg.ServerName);
                    SetAttr(dbEl, "DatabaseName", dbCfg.DatabaseName);
                    SetAttr(dbEl, "UserName", dbCfg.UserName);
                    SetAttr(dbEl, "Password", dbCfg.Password);
                    SetAttr(dbEl, "AuthenticationType", dbCfg.AuthenticationType);
                    dbEl.Add(new XElement(Ns + "ScriptToPath", dbCfg.ScriptToPath ?? string.Empty));
                    autoEl.Add(dbEl);
                }

                var pa = model.PostScriptingAction.FirstOrDefault(p => p.AutoScripting_Id == auto.AutoScripting_Id);
                if (pa is not null)
                {
                    var paEl = new XElement(Ns + "PostScriptingAction");
                    SetAttr(paEl, "Name", pa.Name);
                    SetAttr(paEl, "Command", pa.Command);
                    SetAttr(paEl, "Arguments", pa.Arguments);
                    autoEl.Add(paEl);
                }

                root.Add(autoEl);
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

        private static void SetAttr(XElement el, string name, string? value)
        {
            if (value is not null)
                el.SetAttributeValue(name, value);
        }

        private static void SetAttr(XElement el, string name, bool? value)
        {
            if (value.HasValue)
                el.SetAttributeValue(name, value.Value ? "true" : "false");
        }
    }
}
