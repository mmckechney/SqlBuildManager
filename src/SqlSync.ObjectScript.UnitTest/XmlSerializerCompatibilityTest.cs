using System.Xml.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.ObjectScript;

namespace SqlSync.ObjectScript.UnitTest
{
    [TestClass]
    public class XmlSerializerCompatibilityTest
    {
        private const string SampleAutoScriptingXml = @"<?xml version=""1.0""?>
<AutoScriptingConfig xmlns=""http://www.mckechney.com/AutoScriptingConfig.xsd"">
  <AutoScripting AllowManualSelection=""false"" IncludeFileHeaders=""true"" DeletePreExistingFiles=""true"" ZipScripts=""true"">
    <DatabaseScriptConfig ServerName=""srv"" DatabaseName=""db"" UserName=""u"" Password=""p"" AuthenticationType=""WindowsAuthentication"">
      <ScriptToPath>./scripts</ScriptToPath>
    </DatabaseScriptConfig>
    <PostScriptingAction Name=""echo"" Command=""cmd"" Arguments=""/c echo hi"" />
  </AutoScripting>
</AutoScriptingConfig>";

        [TestMethod]
        public void AutoScriptingConfigXmlSerializer_Roundtrip()
        {
            var doc = XDocument.Parse(SampleAutoScriptingXml);
            var model = AutoScriptingConfigXmlSerializer.Load(doc);
            Assert.AreEqual(1, model.AutoScripting.Count);
            Assert.AreEqual(false, model.AutoScripting[0].AllowManualSelection);
            Assert.AreEqual("srv", model.DatabaseScriptConfig[0].ServerName);
            Assert.AreEqual("./scripts", model.DatabaseScriptConfig[0].ScriptToPath);
            Assert.AreEqual("echo", model.PostScriptingAction[0].Name);

            var outDoc = AutoScriptingConfigXmlSerializer.BuildDocument(model);
            var ns = (XNamespace)"http://www.mckechney.com/AutoScriptingConfig.xsd";
            var auto = outDoc.Root!.Element(ns + "AutoScripting")!;
            Assert.AreEqual("true", (string?)auto.Attribute("IncludeFileHeaders"));
            var db = auto.Element(ns + "DatabaseScriptConfig")!;
            Assert.AreEqual("srv", (string?)db.Attribute("ServerName"));
            Assert.AreEqual("WindowsAuthentication", (string?)db.Attribute("AuthenticationType"));
        }
    }
}
