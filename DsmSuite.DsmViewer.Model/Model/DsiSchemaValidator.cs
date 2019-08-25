using System;
using System.Xml;
using System.Xml.Schema;

namespace DsmSuite.DsmViewer.Model.Model
{
    public class DsiSchemaValidator
    {
        private bool _validationOk = true;

        public bool Validate(String filename)
        {
            XmlSchemaSet schemaSet = new XmlSchemaSet();
            schemaSet.Add("urn:dsi-schema", "dsi.xsd");

            XmlSchema compiledSchema = null;

            foreach (XmlSchema schema in schemaSet.Schemas())
            {
                compiledSchema = schema;
            }

            if (compiledSchema != null)
            {
                XmlReaderSettings settings = new XmlReaderSettings();
                settings.Schemas.Add(compiledSchema);
                settings.ValidationEventHandler += ValidationCallBack;
                settings.ValidationType = ValidationType.Schema;

                XmlReader vreader = XmlReader.Create(filename, settings);

                while (vreader.Read())
                {
                }
                vreader.Close();
            }

            return _validationOk;
        }

        private void ValidationCallBack(object sender, ValidationEventArgs args)
        {
            _validationOk = false;
            if (args.Severity == XmlSeverityType.Warning)
            {
                Console.WriteLine("\tWarning: Matching schema not found.  No validation occurred." + args.Message);
            }
            else
            {
                Console.WriteLine("\tValidation error: " + args.Message);
            }
        }
    }
}
