using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Schema;
using Exolutio.Dialogs;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Model.PSM.Grammar;
using Exolutio.Model.PSM.Grammar.XSDTranslation;
using Exolutio.ResourceLibrary;
using Exolutio.SupportingClasses;
using Exolutio.View;
using Microsoft.Win32;
using Label = Exolutio.ViewToolkit.Label;

namespace Exolutio.WPFClient
{
    /// <summary>
    /// Interaction logic for FileTab.xaml
    /// </summary>
    public partial class FileTab : IFilePresenterTab
    {
        public FileTab()
        {
            InitializeComponent();
        }

        public string FileName { get; set; }

        public PSMSchema ValidationSchema { get; set; }

        public void DisplayFile(EDisplayedFileType displayedFileType, Stream fileContents, string fileName = null, ILog log = null, PSMSchema validationSchema = null)
        {
            fileView.DisplayFile(displayedFileType, fileContents);
            
            FileName = fileName;
            Title = fileName;

            if (log != null)
            {
                LogMessages = log; 
            }

            ValidationSchema = validationSchema;
            ShowHideRelevantButtons();
        }

        public void DisplayFile(EDisplayedFileType displayedFileType, string fileContents, string fileName = null, ILog log = null, PSMSchema validationSchema = null)
        {
            fileView.DisplayFile(displayedFileType, fileContents);
            
            FileName = fileName;
            Title = fileName;

            if (log != null)
            {
                LogMessages = log;
            }

            ValidationSchema = validationSchema;
            ShowHideRelevantButtons();
        }

        private void ShowHideRelevantButtons()
        {
            bValidateXMLSchema.Visibility = fileView.DisplayedFileType == EDisplayedFileType.XSD ? Visibility.Visible : Visibility.Collapsed;
            bValidateSchematronSchema.Visibility = fileView.DisplayedFileType == EDisplayedFileType.SCH ? Visibility.Visible : Visibility.Collapsed;
            bValidateAgainstSchema.Visibility = ValidationSchema != null ? Visibility.Visible : Visibility.Collapsed;
        }

        public string GetDocumentText()
        {
            return fileView.FileContents;
        }

        public void SetDocumentText(string text)
        {
            fileView.FileContents = text;
        }

        public void bSave_Click(object sender, RoutedEventArgs e)
        {
            string defaultExt;
            string filter;
            if (fileView.DisplayedFileType == EDisplayedFileType.XSD)
            {
                defaultExt = "xsd";
                filter = "XML Schema files (*.xsd)|*.xsd|XML files (*.xml)|*.xml|All files (*.*)|*.*";
            }
            else if (fileView.DisplayedFileType == EDisplayedFileType.SCH)
            {
                defaultExt = "sch";
                filter = "Schematron Schema files (*.sch)|*.sch|XML files (*.xml)|*.xml|All files (*.*)|*.*";
            }
            else if (fileView.DisplayedFileType == EDisplayedFileType.XML)
            {
                defaultExt = "xml";
                filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            }
            else
            {
                defaultExt = null;
                filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            }

            SaveFileDialog sfd = new SaveFileDialog
            {
                AddExtension = true,
                DefaultExt = defaultExt,
                FileName = FileName,
                Filter = filter
            };
            if (sfd.ShowDialog() == true)
            {
                File.WriteAllText(sfd.FileName, GetDocumentText(), Encoding.UTF8);
            }
        }

        #region log

        private static object ImageGetter(ILogMessage message)
        {
            if (message.Severity == ELogMessageSeverity.Error)
                return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.error_button);
            else
                return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.Warning);
        }

        private ILog logMessages;

        public ILog LogMessages
        {
            get
            {
                return logMessages;
            }
            set
            {
                logMessages = value;
                LogMessage.ImageGetter = ImageGetter;
                gridLog.ItemsSource = logMessages.OrderBy(message => message.Severity == ELogMessageSeverity.Error ? 0 : 1);
                int countw = logMessages.Count(e => e.Severity == ELogMessageSeverity.Warning);
                int counte = logMessages.Count(e => e.Severity == ELogMessageSeverity.Error);
                if (countw > 0 && counte > 0)
                    expander1.Header = String.Format("Task completed with {0} errors and {1} warnings", counte, countw);
                else if (countw > 0)
                    expander1.Header = String.Format("Task completed with {0} warnings", countw);
                else if (counte > 0)
                    expander1.Header = String.Format("Task completed with {0} errors", counte);
                else
                    expander1.Header = "Task completed successfully";
            }
        }

        #endregion 

        #region schema validation

        private bool IsSchemaValid = true;

        private void bValidateSchema_Click(object sender, RoutedEventArgs e)
        {
            XmlSchema schema;
            /*ProjectTranslator projectTranslator = null;
            bool diagramWithReferences = Diagram.DiagramReferences.Count() > 0;
            if (diagramWithReferences)
            {
                projectTranslator = new ProjectTranslator { Project = MainWindow.CurrentProject };
                projectTranslator.SaveSchemas(null);
            }*/
            try
            {
                /*if (diagramWithReferences)
                {
                    FileStream f = new FileStream(projectTranslator.SchemaFiles[Diagram], FileMode.Open);
                    schema = XmlSchema.Read(f, ValidationCallBack);
                    f.Close();
                }
                else
                {*/
                schema = XmlSchema.Read(new StringReader(GetDocumentText()), null);
                //}
                IsSchemaValid = true;

            }
            catch (XmlSchemaException ex)
            {
                string message = string.Format("{0} Position:[{1},{2}] object: {3}", ex.Message, ex.LineNumber,
                                               ex.LinePosition, ex.SourceSchemaObject);
                Debug.WriteLine(message);
                MessageBox.Show(message);
                return;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                MessageBox.Show(ex.Message);
                return;
            }

            XmlSchemaSet schemaSet = new XmlSchemaSet();
            schemaSet.ValidationEventHandler += ValidationCallBack;
            schemaSet.Add(schema);
            schemaSet.Compile();

            if (IsSchemaValid)
            {
                MessageBox.Show("XSD is valid");
            }
        }

        private void ValidationCallBack(object sender, ValidationEventArgs e)
        {
            IsSchemaValid = false;
            Debug.WriteLine(e.Message);
            MessageBox.Show(e.Message);
        }

        #endregion

        #region validation against schema

        private bool isValidAgainstSchema;
        private bool abortValidationAgainstSchema;

        private void bValidateAgainstPSMSchema_Click(object sender, RoutedEventArgs e)
        {
            XsdSchemaGenerator g = new XsdSchemaGenerator();
            g.Initialize(ValidationSchema);
            g.GenerateXSDStructure();
            XDocument xDocument = g.GetXsd();
            ValidateDocumentAgainstSchema(xDocument);
        }

        private void ValidateDocumentAgainstSchema(XDocument schemaXSD)
        {
            XmlReader xmlfile = null;
            XmlReader schemaReader = null;
            MemoryStream _msSchemaText = null;
            isValidAgainstSchema = true;
            abortValidationAgainstSchema = false;
            try
            {
                _msSchemaText = new MemoryStream();
                string tmp = System.IO.Path.GetTempFileName();
                schemaXSD.Save(tmp);
                schemaReader = new XmlTextReader(File.OpenRead(tmp));
                XmlSchema schema = XmlSchema.Read(schemaReader, schemaSettings_ValidationEventHandler);
                XmlReaderSettings schemaSettings = new XmlReaderSettings();
                schemaSettings.Schemas.Add(schema);
                foreach (XmlSchema s in schemaSettings.Schemas.Schemas())
                {
                    StringBuilder sb = new StringBuilder();
                    StringWriter sw = new StringWriter(sb);
                    s.Write(sw);
                    sw.Flush();
                }

                schemaSettings.ValidationType = ValidationType.Schema;
                schemaSettings.ValidationEventHandler += schemaSettings_ValidationEventHandler;

                try
                {
                    xmlfile = XmlReader.Create(new StringReader(fileView.FileContents), schemaSettings);
                }
                catch (XmlSchemaValidationException ex)
                {
                    isValidAgainstSchema = false;
                    ExolutioErrorMsgBox.Show("Invalid schema",
                                             string.Format("Validation can not continue - schema is invalid. \r\n\r\n{0}",
                                                           ex.Message));
                    return;
                }

                if (isValidAgainstSchema)
                {
                    while (xmlfile.Read() && !abortValidationAgainstSchema)
                    {
                    }
                }
            }
            catch (XmlSchemaValidationException ex)
            {
                isValidAgainstSchema = false;
                ExolutioErrorMsgBox.Show("Invalid document",
                                         string.Format("{0} \r\n\r\nValidation can not continue.", ex.Message));
            }
            catch (Exception ex)
            {
                isValidAgainstSchema = false;
                ExolutioErrorMsgBox.Show("Invalid document",
                                         string.Format("{0} \r\n\r\nValidation can not continue.", ex.Message));
            }
            finally
            {
                if (xmlfile != null) xmlfile.Close();
                if (schemaReader != null) schemaReader.Close();
                if (_msSchemaText != null) _msSchemaText.Dispose();
            }

            if (isValidAgainstSchema)
            {
                ExolutioMessageBox.Show("Validation successful", "Document is valid",
                                        "Validation against the XML schema passed successfuly. ");
            }
        }

        void schemaSettings_ValidationEventHandler(object sender, ValidationEventArgs e)
        {
            string location = string.Empty;
            if (e.Exception != null)
            {
                location = string.Format("\r\n\rLine number: {0} position {1}", e.Exception.LineNumber, e.Exception.LinePosition);
            }

            MessageBoxResult result = ExolutioYesNoBox.Show("Invalid document", string.Format("{0}{1}\r\n\rContinue validation?", e.Message, location));

            if (result == MessageBoxResult.No)
                abortValidationAgainstSchema = true;
            isValidAgainstSchema = false;
        }

        #endregion

        public void CreateAdditionalActionsButtons(FilePresenterButton[] additionalActions)
        {
            foreach (FilePresenterButton filePresenterButton in additionalActions)
            {
                Button b = new Button();
                StackPanel stackPanel = new StackPanel();
                stackPanel.Orientation = Orientation.Horizontal;
                b.Content = stackPanel;
                stackPanel.Children.Add(new Image() {Source = filePresenterButton.Icon});
                stackPanel.Children.Add(new System.Windows.Controls.Label() {Content = filePresenterButton.Text});
                b.Click += delegate { filePresenterButton.UpdateFileContentAction(this); };
                MainToolBar.Items.Add(b);
            }
        }

        private void bValidateSchematronSchema_Click(object sender, RoutedEventArgs e)
        {
            XDocument schematronXSD;
            using (StringReader sr = new StringReader(Exolutio.Model.PSM.Grammar.Properties.Resources.schematronXSD))
            {
                schematronXSD = XDocument.Load(sr);
            }
            //XNamespace xsdNS = "http://www.w3.org/2001/XMLSchema";
            //string tmp = System.IO.Path.GetTempFileName();
            //System.IO.File.WriteAllText(tmp, Exolutio.Model.PSM.Grammar.Properties.Resources.xmlNamespaceXSD);
            //schematronXSD.Element(xsdNS + "schema").Element(xsdNS + "import").Attribute("schemaLocation").Value = System.IO.Path.GetFileName(tmp); 
            ValidateDocumentAgainstSchema(schematronXSD);
        }
    }
}
