using System;
using System.Linq;
using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild;

namespace SqlSync.SqlBuild.UnitTest
{
    [TestClass]
    public class XmlSerializerCompatibilityTest
    {
        private const string SampleBuildProjectXml = @"<?xml version=""1.0"" standalone=""yes""?>
      <SqlSyncBuildData xmlns=""http://schemas.mckechney.com/SqlSyncBuildProject.xsd"">
        <SqlSyncBuildProject ProjectName="""" ScriptTagRequired=""true"">
    <Scripts>
      <Script FileName=""Simple Select.sql"" BuildOrder=""1"" Description="""" RollBackOnError=""true"" CausesBuildFailure=""true"" DateAdded=""2010-04-09T11:35:33.7776612-04:00"" ScriptId=""6f2f7b92-7ea8-4240-be14-327f443949d7"" Database=""SqlBuildTest"" StripTransactionText=""true"" AllowMultipleRuns=""true"" AddedBy=""mmckechn"" ScriptTimeOut=""500"" DateModified=""2010-04-09T11:41:53.9493776-04:00"" ModifiedBy=""mmckechn"" Tag=""default"" />
    </Scripts>
    <Builds />
  </SqlSyncBuildProject>
</SqlSyncBuildData>";

        private const string SampleServerConfigXml = @"<?xml version=""1.0"" standalone=""yes""?>
      <ServerConnectConfig xmlns=""http://schemas.mckechney.com/SqlSyncConfiguration.xsd"">
        <ServerConfiguration Name=""local"" LastAccessed=""2024-01-01T00:00:00Z"">
    <UserName>u</UserName>
    <Password>p</Password>
    <AuthenticationType>WindowsAuthentication</AuthenticationType>
  </ServerConfiguration>
  <LastProgramUpdateCheck CheckTime=""2024-01-02T00:00:00Z"" />
  <LastDirectory ComponentName=""comp"" Directory=""C:\temp"" />
</ServerConnectConfig>";

        [TestMethod]
        public void SqlSyncBuildDataXmlSerializer_Load_ParsesLegacyXml()
        {
            var doc = XDocument.Parse(SampleBuildProjectXml);
            var model = SqlSyncBuildDataXmlSerializer.Load(doc);

            Assert.AreEqual(1, model.SqlSyncBuildProject.Count);
            Assert.AreEqual(true, model.SqlSyncBuildProject.Single().ScriptTagRequired);
            Assert.AreEqual(1, model.Scripts.Count);
            Assert.AreEqual(1, model.Script.Count);
            var script = model.Script.Single();
            Assert.AreEqual("Simple Select.sql", script.FileName);
            Assert.AreEqual("SqlBuildTest", script.Database);
        }

        [TestMethod]
        public void SqlSyncBuildDataXmlSerializer_Save_WritesExpectedElements()
        {
            var doc = XDocument.Parse(SampleBuildProjectXml);
            var model = SqlSyncBuildDataXmlSerializer.Load(doc);
            var outDoc = SqlSyncBuildDataXmlSerializer.BuildDocument(model);

            var ns = (XNamespace)"http://schemas.mckechney.com/SqlSyncBuildProject.xsd";
            Assert.AreEqual("SqlSyncBuildData", outDoc.Root!.Name.LocalName);
            Assert.AreEqual(ns, outDoc.Root.Name.Namespace);
            var script = outDoc.Root.Element(ns + "SqlSyncBuildProject")!
                .Element(ns + "Scripts")!
                .Element(ns + "Script")!;
            Assert.AreEqual("Simple Select.sql", (string?)script.Attribute("FileName"));
            Assert.AreEqual("SqlBuildTest", (string?)script.Attribute("Database"));
        }

        [TestMethod]
        public void ServerConnectConfigXmlSerializer_Roundtrip()
        {
            var doc = XDocument.Parse(SampleServerConfigXml);
            var model = ServerConnectConfigXmlSerializer.Load(doc);

            Assert.AreEqual(1, model.ServerConfiguration.Count);
            Assert.AreEqual("local", model.ServerConfiguration[0].Name);
            Assert.AreEqual("u", model.ServerConfiguration[0].UserName);
            Assert.AreEqual("p", model.ServerConfiguration[0].Password);
            Assert.AreEqual("WindowsAuthentication", model.ServerConfiguration[0].AuthenticationType);
            Assert.AreEqual(1, model.LastProgramUpdateCheck.Count);
            Assert.AreEqual(1, model.LastDirectory.Count);

            var outDoc = ServerConnectConfigXmlSerializer.BuildDocument(model);
            var ns = (XNamespace)"http://schemas.mckechney.com/SqlSyncConfiguration.xsd";
            var sc = outDoc.Root!.Element(ns + "ServerConfiguration")!;
            Assert.AreEqual("local", (string?)sc.Attribute("Name"));
            Assert.AreEqual("u", (string?)sc.Element(ns + "UserName"));
        }
    }
}
