using System;
using System.Xml.Schema;
using System.Xml;
namespace SqlSync.Validator
{
	/// <summary>
	/// Summary description for SchemaValidator.
	/// </summary>

	public class SchemaValidator
	{
		bool isValid;
		//private XmlSchemaCollection schemaCache;

		public SchemaValidator()
		{
			

		}
        private string validityErrorMessage;

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
		private void CheckValidity(object sender, ValidationEventArgs args)
		{
            this.validityErrorMessage += args.Message + "\r\n";
			this.isValid = false;
		}

		public bool ValidateAgainstSchema(string fileName, string schemaFileName, string schemaNamespace)
		{
			this.isValid = true;
			//Check to see if this file matches the set schema
			XmlTextReader reader = new XmlTextReader(fileName);
			XmlValidatingReader validator = new XmlValidatingReader(reader);
			validator.ValidationEventHandler += new ValidationEventHandler(CheckValidity);
			validator.ValidationType = ValidationType.Schema;
			try
			{
				validator.Schemas.Add(GetSchema(schemaNamespace,schemaFileName) );
				while(validator.Read())
				{
                    //Break out once it is determined the Xml is invalid
                    if (!this.isValid)
                        return this.isValid;
				}
				
			}
			catch(Exception exe)
			{
                string error = exe.ToString();
				isValid = false;
			}
			finally
			{
				validator.Close();
			}

			return this.isValid;
		}
		/// <summary>
		/// Loads the generator XSD schema 
		/// </summary>
		/// <param name="schemaLocation"></param>
		/// <returns></returns>
		private XmlSchemaCollection GetSchema(string schemaNamespace, string schemaLocation)
		{
			try
			{
				XmlSchemaCollection schemaCache = new XmlSchemaCollection();
				schemaCache.Add(schemaNamespace,schemaLocation);

				return schemaCache;
			}
			catch
			{
				return null;
			}
		}


	}
}
