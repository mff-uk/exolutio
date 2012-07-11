using System;
using System.Text;
using System.Xml.Linq;
using System.Xml.Schema;
using System.Xml;
using System.IO;

namespace Exolutio.Model.PSM.Grammar.XSDTranslation
{
    public class XsdValidator
    {
        private bool isValid;
        private bool abort;
        public string ErrorMessage { get; set; }
        
        public bool ValidateDocument(PSMSchema psmSchema, string xmltext)
        {
            XsdSchemaGenerator schemaGenerator = new XsdSchemaGenerator();
            schemaGenerator.Initialize(psmSchema);
            schemaGenerator.GenerateXSDStructure();
            XDocument schemaXSD = schemaGenerator.GetXsd();

            XmlReader xmlfile = null;
            XmlReader schemaReader = null;
            MemoryStream _msSchemaText = null;
            isValid = true;
            abort = false;
            try
            {
                _msSchemaText = new MemoryStream();
                schemaXSD.Save(_msSchemaText);
                _msSchemaText.Position = 0;
                schemaReader = new XmlTextReader(_msSchemaText);
                XmlSchema schema = XmlSchema.Read(schemaReader, schemaSettings_ValidationEventHandler);
                //schema.TargetNamespace = diagram.Project.XMLNamespaceOrDefaultNamespace;

                XmlReaderSettings schemaSettings = new XmlReaderSettings();
                schemaSettings.Schemas.Add(schema);
                schemaSettings.ValidationType = ValidationType.Schema;
                schemaSettings.ValidationEventHandler += schemaSettings_ValidationEventHandler;
                schemaSettings.ValidationFlags |= XmlSchemaValidationFlags.ReportValidationWarnings;
                try
                {
                    xmlfile = XmlReader.Create(new StringReader(xmltext), schemaSettings);
                }
                catch (XmlSchemaValidationException ex)
                {
                    isValid = false;
                    ErrorMessage = string.Format("Validation can not continue - schema is invalid. \r\n\r\n{0}", ex.Message);
                    return false;
                }

                if (isValid)
                {
                    while (xmlfile.Read() && !abort)
                    {
                    }
                }
            }
            catch (XmlSchemaValidationException ex)
            {
                isValid = false;
                ErrorMessage = string.Format("{0} \r\n\r\nValidation can not continue.", ex.Message);
            }
            catch (Exception ex)
            {
                isValid = false;
                ErrorMessage = string.Format("{0} \r\n\r\nValidation can not continue.", ex.Message);
            }
            finally
            {
                if (xmlfile != null) xmlfile.Close();
                if (schemaReader != null) schemaReader.Close();
                if (_msSchemaText != null) _msSchemaText.Dispose();
            }

            if (isValid)
            {
                //ok
            }

            return isValid;
        }

        void schemaSettings_ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            string location = string.Empty;
            if (e.Exception != null)
            {
                location = string.Format("\r\n\rLine number: {0} position {1}", e.Exception.LineNumber,
                                         e.Exception.LinePosition);
            }

            abort = true;
            ErrorMessage = string.Format("{0}{1}\r\n\r", e.Message, location);
            isValid = false;
        }
    }
}