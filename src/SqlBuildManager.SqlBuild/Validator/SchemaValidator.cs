using System;
using System.Xml;
using System.Xml.Schema;

namespace SqlBuildManager.SqlBuild.Validator
{
    /// <summary>
    /// Summary description for SchemaValidator.
    /// </summary>

    public class SchemaValidator
    {
        bool isValid;

        public SchemaValidator()
        {


        }
        private string validityErrorMessage = string.Empty;

        public string ValidityErrorMessage
        {
            get { return validityErrorMessage; }
            set { validityErrorMessage = value; }
        }
        private System.Text.StringBuilder sb = new System.Text.StringBuilder();
        /// <summary>
        /// Sets the global boolean to false due to an error in validation
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void CheckValidity(object? sender, ValidationEventArgs args)
        {
            validityErrorMessage += args.Message + "\r\n";
            isValid = false;
        }

        public bool ValidateAgainstSchema(string fileName, string schemaFileName, string schemaNamespace)
        {
            isValid = true;

            XmlReaderSettings settings = new XmlReaderSettings();
            // SEC-003: explicitly prohibit DTD processing and disable external entity resolution.
            settings.DtdProcessing = DtdProcessing.Prohibit;
            settings.XmlResolver = null;
            settings.ValidationType = ValidationType.Schema;
            settings.Schemas.Add(GetSchema(schemaNamespace, schemaFileName));
            settings.Schemas.Compile();
            settings.ValidationEventHandler += new ValidationEventHandler(CheckValidity);
            try
            {
                using (XmlReader validator = XmlReader.Create(fileName, settings))
                {
                    while (validator.Read())
                    {
                        //Break out once it is determined the Xml is invalid
                        if (!isValid)
                            return isValid;
                    }
                }
            }
            catch (Exception exe)
            {
                string error = exe.ToString();
                isValid = false;
            }

            return isValid;
        }
        /// <summary>
        /// Loads the generator XSD schema 
        /// </summary>
        /// <param name="schemaLocation"></param>
        /// <returns></returns>
        private XmlSchemaSet GetSchema(string schemaNamespace, string schemaLocation)
        {
            try
            {
                XmlSchemaSet schemaCache = new XmlSchemaSet();
                schemaCache.Add(schemaNamespace, schemaLocation);

                return schemaCache;
            }
            catch
            {
                return null!;
            }
        }


    }
}
