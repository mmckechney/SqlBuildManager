using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using SqlSync.SqlBuild.Models;

#nullable enable

namespace SqlSync.SqlBuild
{
    /// <summary>
    /// POCO-based XML serializer/deserializer for ServerConnectConfigModel compatible with legacy DataSet XML.
    /// </summary>
    public static class ServerConnectConfigXmlSerializer
    {
        private static readonly XNamespace Ns = "http://schemas.mckechney.com/SqlSyncConfiguration.xsd";

        public static ServerConnectConfigModel Load(string path)
        {
            using var stream = File.OpenRead(path);
            var doc = XDocument.Load(stream);
            return Load(doc);
        }

        public static ServerConnectConfigModel Load(XDocument doc)
        {
            if (doc.Root is null)
                throw new InvalidOperationException("ServerConnectConfig XML has no root element.");

            var ns = doc.Root.GetDefaultNamespace();
            var serverConfigs = new List<ServerConfiguration>();
            var lastProgramUpdateChecks = new List<LastProgramUpdateCheck>();
            var lastDirectories = new List<LastDirectory>();

            foreach (var sc in doc.Root.Elements(ns + "ServerConfiguration"))
            {
                var name = (string?)sc.Attribute("Name") ?? string.Empty;
                var lastAccessed = ParseDate((string?)sc.Attribute("LastAccessed")) ?? DateTime.MinValue;
                var userName = (string?)sc.Element(ns + "UserName");
                var password = (string?)sc.Element(ns + "Password");
                var authType = (string?)sc.Element(ns + "AuthenticationType");
                serverConfigs.Add(new ServerConfiguration(name, lastAccessed, userName, password, authType));
            }

            foreach (var lpuc in doc.Root.Elements(ns + "LastProgramUpdateCheck"))
            {
                var checkTime = ParseDate((string?)lpuc.Attribute("CheckTime"));
                lastProgramUpdateChecks.Add(new LastProgramUpdateCheck(checkTime));
            }

            foreach (var ld in doc.Root.Elements(ns + "LastDirectory"))
            {
                var componentName = (string?)ld.Attribute("ComponentName") ?? string.Empty;
                var directory = (string?)ld.Attribute("Directory") ?? string.Empty;
                lastDirectories.Add(new LastDirectory(componentName, directory));
            }

            return new ServerConnectConfigModel(serverConfigs, lastProgramUpdateChecks, lastDirectories);
        }

        public static void Save(string path, ServerConnectConfigModel model)
        {
            var doc = BuildDocument(model);
            using var writer = XmlWriter.Create(path, new XmlWriterSettings
            {
                Indent = true,
                Encoding = new System.Text.UTF8Encoding(encoderShouldEmitUTF8Identifier: true),
            });
            doc.Save(writer);
        }

        public static XDocument BuildDocument(ServerConnectConfigModel model)
        {
            var root = new XElement(Ns + "ServerConnectConfig");

            foreach (var sc in model.ServerConfiguration)
            {
                var scEl = new XElement(Ns + "ServerConfiguration");
                scEl.SetAttributeValue("Name", sc.Name ?? string.Empty);
                scEl.SetAttributeValue("LastAccessed", FormatDate(sc.LastAccessed));
                if (sc.UserName is not null) scEl.Add(new XElement(Ns + "UserName", sc.UserName));
                if (sc.Password is not null) scEl.Add(new XElement(Ns + "Password", sc.Password));
                if (sc.AuthenticationType is not null) scEl.Add(new XElement(Ns + "AuthenticationType", sc.AuthenticationType));
                root.Add(scEl);
            }

            foreach (var lpuc in model.LastProgramUpdateCheck)
            {
                var el = new XElement(Ns + "LastProgramUpdateCheck");
                if (lpuc.CheckTime.HasValue)
                {
                    el.SetAttributeValue("CheckTime", FormatDate(lpuc.CheckTime.Value));
                }
                root.Add(el);
            }

            foreach (var ld in model.LastDirectory)
            {
                var el = new XElement(Ns + "LastDirectory");
                el.SetAttributeValue("ComponentName", ld.ComponentName ?? string.Empty);
                el.SetAttributeValue("Directory", ld.Directory ?? string.Empty);
                root.Add(el);
            }

            var doc = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), root);
            return doc;
        }

        private static DateTime? ParseDate(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            try { return XmlConvert.ToDateTime(value, XmlDateTimeSerializationMode.RoundtripKind); }
            catch { return null; }
        }

        private static string FormatDate(DateTime value) => XmlConvert.ToString(value, XmlDateTimeSerializationMode.RoundtripKind);
    }
}
