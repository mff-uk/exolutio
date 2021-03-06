﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Xml.Linq;
using System.Xml.Schema;
using Exolutio.DataGenerator;
using Exolutio.Dialogs;
using Exolutio.Model.PSM;
using Exolutio.Model.PSM.Grammar.XSDTranslation;
using Exolutio.Revalidation;
using Exolutio.Revalidation.Changes;
using Exolutio.Revalidation.XSLT;
using Exolutio.SupportingClasses.XML;
using ICSharpCode.AvalonEdit.Folding;
using Microsoft.Win32;
using System.Xml;
using System.Xml.Xsl;
using System.Configuration;

namespace Exolutio.View.Commands
{
    /// <summary>
    /// Interaction logic for XsltTestWindow.xaml
    /// </summary>
    public partial class XsltTestWindow
    {
		private static string BASE_DIR = @"d:\Development\Exolutio\Tests\OCLAdaptation\Cases\";

        readonly IEnumerable<string> fileNames;
        
        public XsltTestWindow()
        {
            InitializeComponent();

            foldingManager = FoldingManager.Install(tbOldDoc.TextArea);
            foldingManager = FoldingManager.Install(tbNewDoc.TextArea);
            foldingManager = FoldingManager.Install(tbXslt.TextArea);

            tbOldDoc.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("XML");
            tbOldDoc.ShowLineNumbers = true;

            tbNewDoc.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("XML");
            tbNewDoc.ShowLineNumbers = true;

            tbXslt.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("XML");
            tbXslt.ShowLineNumbers = true;

            foldingStrategy = new XmlFoldingStrategy();

            if (Directory.Exists(BASE_DIR))
            {
                FileInfo[] fileInfos = (new DirectoryInfo(BASE_DIR)).GetFiles("*.xslt");

                fileNames = fileInfos.Select(a => a.Name);
                cbXsltList.ItemsSource = fileNames;

                if (fileNames.Contains("tmp.xslt"))
                    OpenXslt("tmp.xslt");
            }

#if DEBUG
#else 
            bTestOutputCreation.Visibility = Visibility.Collapsed;
            bSaveRef.Visibility = Visibility.Collapsed;
            pXSLT.Visibility = Visibility.Collapsed;
#endif
        }

        private XsltTestWindow(DetectedChangeInstancesSet changeInstances)
            : this()
        {
            ChangeInstances = changeInstances; 
        }

        private PSMSchema schemaVersion1;
        protected PSMSchema SchemaVersion1
        {
            get { return schemaVersion1; }
            set
            {
                schemaVersion1 = value;
#if AUTOCREATE_SAMPLES
                CreateSampleDocument();
#endif
                if (fileNames != null)
                {
                    if (fileNames.Contains(schemaVersion1.Caption + ".xslt"))
                    {
                        cbXsltList.SelectedItem = schemaVersion1.Caption + ".xslt";
                        OpenXslt(cbXsltList.SelectedItem.ToString());
                    }
                    if (fileNames.Contains(schemaVersion1.Project.Name + ".xslt"))
                    {
                        cbXsltList.SelectedItem = schemaVersion1.Project.Name + ".xslt";
                        OpenXslt(cbXsltList.SelectedItem.ToString());
                    }
                    if (fileNames.Contains(Path.GetFileNameWithoutExtension(schemaVersion1.Project.ProjectFile.FullName) + ".xslt"))
                    {
                        cbXsltList.SelectedItem = Path.GetFileNameWithoutExtension(schemaVersion1.Project.ProjectFile.FullName) + ".xslt";
                        OpenXslt(cbXsltList.SelectedItem.ToString());
                    }
                }
            }
        }

        private PSMSchema schemaVersion2;

        protected PSMSchema SchemaVersion2
        {
            get { return schemaVersion2; }
            set 
            { 
                schemaVersion2 = value;
#if AUTOCREATE_SAMPLES
                CreateSampleNewDocument();
#endif 
            }
        }

        private void CreateSampleNewDocument()
        {
            SampleDataGenerator sampleDataGenerator = new SampleDataGenerator();
            sampleDataGenerator.GenerateComments = false;
            XDocument sampleDoc = sampleDataGenerator.Translate(schemaVersion2);
            tbNewDoc.Text = XDocumentToString(sampleDoc);
            UpdateFolding();
        }

		const string SAVE_DIR = @"d:\Development\Exolutio\XSLTTest\";
		const string SAVE_DOCUMENT = @"d:\Development\Exolutio\XSLTTest\LastInput.xml";

		public const string SAVE_STYLESHEET = @"d:\Development\Exolutio\XSLTTest\LastStylesheet.xslt";

        public static string XDocumentToString(XDocument doc)
        {
            StringBuilder sb = new StringBuilder();
            using (TextWriter tw = new StringWriter(sb))
            {
                doc.Save(tw);
            }
            return sb.ToString();
        }


        private void CreateSampleDocument()
        {
            SampleDataGenerator sampleDataGenerator = new SampleDataGenerator();
            sampleDataGenerator.GenerateComments = false;
            XDocument sampleDoc = sampleDataGenerator.Translate(schemaVersion1, bSchemaAware.IsChecked == true ? "LastSchema.xsd" : null);

            string documentString = XDocumentToString(sampleDoc);
            tbOldDoc.Text = documentString;

            SaveOutput(documentString);
            UpdateFolding();
        }

        private static void SaveOutput(string sampleDoc)
        {
            string text = sampleDoc;
            //int si = sampleDoc.IndexOf("xmlns:xsi=\"");
            //int ei = sampleDoc.IndexOf("\"", si + "xmlns:xsi=\"".Length) + 1;
            //string text = si != -1 ? sampleDoc.Remove(si, ei - si) : sampleDoc;
            //si = text.IndexOf("xmlns=\"");
            //if (si >= 0)
            //{
            //    ei = text.IndexOf("\"", si + "xmlns=\"".Length) + 1;
            //    string xmlns = text.Substring(si, ei - si);
            //    text = text.Remove(si, ei - si);
            //}
            XmlDocumentHelper.SaveInUtf8(text.Replace("utf-16", "utf-8"), SAVE_DOCUMENT);
        }

        public static bool? ShowDialog(DetectedChangeInstancesSet changeInstances, PSMSchema activeDiagramOldVersion, PSMSchema activeDiagramNewVersion)
        {
            XsltTestWindow xsltTestWindow = new XsltTestWindow(changeInstances);
            xsltTestWindow.SchemaVersion1 = activeDiagramOldVersion;
            xsltTestWindow.SchemaVersion2 = activeDiagramNewVersion;
            
            //return xsltTestWindow.ShowDialog();
            xsltTestWindow.Show();
            return true;
        }

        protected DetectedChangeInstancesSet ChangeInstances { get; private set; }

        private readonly XmlFoldingStrategy foldingStrategy;
        private readonly FoldingManager foldingManager;

        private void UpdateFolding()
        {
            try
            {
                if (foldingStrategy != null)
                {
                    if (!string.IsNullOrEmpty(tbOldDoc.Text))
                        foldingStrategy.UpdateFoldings(foldingManager, tbOldDoc.Document);
                    if (!string.IsNullOrEmpty(tbNewDoc.Text))
                        foldingStrategy.UpdateFoldings(foldingManager, tbNewDoc.Document);
                    if (!string.IsNullOrEmpty(tbXslt.Text))
                        foldingStrategy.UpdateFoldings(foldingManager, tbXslt.Document);
                }
            }
            catch (Exception)
            {

            }
        }

        private void bAnotherSample_Click(object sender, RoutedEventArgs e)
        {
            CreateSampleDocument();
        }

        private void bTransform_Click(object sender, RoutedEventArgs e)
        {
            Transform();
        }

        private void Transform()
        {
            if (string.IsNullOrEmpty(tbOldDoc.Text) || string.IsNullOrEmpty(tbXslt.Text))
            {
                return;
            }

            string outDoc = XsltProcessing.Transform(tbOldDoc.Text, tbXslt.Text, BASE_DIR, SchemaVersion1, bSchemaAware.IsChecked == true);
            tbNewDoc.Text = outDoc;

            int si = outDoc.IndexOf("xmlns:xsi=\"");
            int ei; 
            string text = outDoc; 
            if (si >= 0)
            {
                ei = outDoc.IndexOf("\"", si + "xmlns:xsi=\"".Length) + 1;
                text = outDoc.Remove(si, ei - si);
            }
            si = text.IndexOf("xmlns=\"");
            if (si >= 0)
            {
                ei = text.IndexOf("\"", si + "xmlns=\"".Length) + 1;
                string xmlns = text.Substring(si, ei - si);
                text = text.Remove(si, ei - si);
            }

            if (Environment.MachineName.Contains("TRUPIK"))
            {
                XmlDocumentHelper.SaveInUtf8(text.Replace("utf-16", "utf-8"), SAVE_DOCUMENT.Replace("LastInput", "LastOutput"));
            }
        }

        

        private void xmlEdit_TextChanged(object sender, EventArgs e)
        {
            UpdateFolding();
        }

        private void Save_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog
                                                {
                CheckFileExists = false,
                DefaultExt = "xslt",
                Filter = "XSLT Transformations|*.xslt"
            };

            string path = System.IO.Path.GetDirectoryName(schemaVersion2.Project.ProjectFile.FullName) +
                //"\\" + System.IO.Path.GetFileNameWithoutExtension(schemaVersion2.Project.ProjectFile.FullName) + "-out" + 
                "\\SavedStylesheet.xslt";

            string xsdpath = System.IO.Path.GetDirectoryName(schemaVersion2.Project.ProjectFile.FullName) +
                "\\LastSchema.xsd";
            
            XsdSchemaGenerator schemaGenerator = new XsdSchemaGenerator();
            schemaGenerator.Initialize(SchemaVersion1);
            schemaGenerator.GenerateXSDStructure();
            XDocument xmlSchemaDocument = schemaGenerator.GetXsd();

            if (path.Contains("Tests\\OCLAdaptation"))
            {
                if (!Directory.Exists(System.IO.Path.GetDirectoryName(path)))
                {
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                }
                XmlDocumentHelper.SaveInUtf8(tbXslt.Text, path);
                xmlSchemaDocument.SaveInUtf8(xsdpath);
                ExolutioMessageBox.Show("Saved", "Saved", string.Empty);
            }
            else if (saveFileDialog.ShowDialog() == true)
            {
                XmlDocumentHelper.SaveInUtf8(tbXslt.Text, saveFileDialog.FileName);
                xmlSchemaDocument.SaveInUtf8(xsdpath);
                ExolutioMessageBox.Show("Saved", "Saved", string.Empty);
            }
            
        }

        private void Load_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
                                                {
                                                    CheckFileExists = true,
                                                    DefaultExt = "xslt",
                                                    Filter = "XSLT Transformations|*.xslt"
                                                };

            if (openFileDialog.ShowDialog() == true)
            {
                OpenXslt(openFileDialog.FileName);
            }
        }

        private void OpenXslt(string filename)
        {
            if (File.Exists(filename))
                tbXslt.Text = File.ReadAllText(filename);
            else 
                tbXslt.Text = File.ReadAllText(BASE_DIR + filename);
            UpdateFolding();
        }

       private void bXsltFromChanges_Click(object sender, RoutedEventArgs e)
        {
            XsltAdaptationScriptGenerator xsltTemplateGenerator = new XsltAdaptationScriptGenerator();
            string xslt = null;
            
            xsltTemplateGenerator.Initialize(SchemaVersion1, SchemaVersion2, ChangeInstances);
            xsltTemplateGenerator.SchemaAware = bSchemaAware.IsChecked == true; 
            xsltTemplateGenerator.GenerateTransformationStructure();

            XDocument revalidationStylesheet = xsltTemplateGenerator.GetAdaptationTransformation();
            StringBuilder sb = new StringBuilder();
            using (TextWriter tw = new StringWriter(sb))
            {
                revalidationStylesheet.Save(tw);
            }
            xslt = sb.ToString();

            tbXslt.Text = xslt;
            if (Environment.MachineName.Contains("TRUPIK"))
            {
                XmlDocumentHelper.SaveInUtf8(xslt, SAVE_STYLESHEET);
            }

           string dir = Path.GetDirectoryName(schemaVersion2.Project.ProjectFile.FullName);
                //+ "\\" + Path.GetFileNameWithoutExtension(schemaVersion2.Project.ProjectFile.FullName) + "-out"
                
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            
            if (Environment.MachineName.Contains("TRUPIK"))
            {
                XmlDocumentHelper.SaveInUtf8(xslt, dir + "\\LastStylesheet.xslt");
            }

            UpdateFolding();
        }

        

        private void bValidateOld_Click(object sender, RoutedEventArgs e)
        {
            XsdValidator validator = new XsdValidator();
            bool valid = validator.ValidateDocument(schemaVersion1, tbOldDoc.Text);
            if (valid)
            {
                ExolutioMessageBox.Show("Validation", "Valid", "The old version is valid.");
            }
            else
            {
                ExolutioErrorMsgBox.Show("ERROR - The old version is not valid!", validator.ErrorMessage);
            }
        }

        private void bValidateNew_Click(object sender, RoutedEventArgs e)
        {
            XsdValidator validator = new XsdValidator();
            bool valid = validator.ValidateDocument(schemaVersion2, tbNewDoc.Text);
            if (valid)
            {
                ExolutioMessageBox.Show("Validation", "Valid", "The new version is valid.");
            }
            else
            {
                ExolutioErrorMsgBox.Show("ERROR - The new version is not valid!", validator.ErrorMessage);
            }
        }

        private void SaveRef_Click(object sender, RoutedEventArgs e)
        {
            SaveRef(null);
        }

        private void SaveRef(string desiredName)
        {
            string name = Path.GetFileNameWithoutExtension(schemaVersion2.Project.ProjectFile.FullName);
            string dir = Path.GetDirectoryName(schemaVersion2.Project.ProjectFile.FullName);

            string inputDir = dir; // +"\\" + name + "-in";
            string outputDir = dir; // + "\\" + name + "-out";

            if (!Directory.Exists(inputDir))
                Directory.CreateDirectory(inputDir);

            if (!Directory.Exists(outputDir))
                Directory.CreateDirectory(outputDir);
            string s;
            if (string.IsNullOrEmpty(desiredName))
            {
                string ts = DateTime.Now.ToShortDateString() + " " + DateTime.Now.ToLongTimeString().Replace(":", "-");
                s = string.Format("{0} test {1}", name, ts);
            }
            else
            {
                s = desiredName;
            }
            string filenameIn = string.Format("{0}-in.xml", s);
            string filenameRef = string.Format("{0}-out.xml", s);

            XmlDocumentHelper.SaveInUtf8(tbOldDoc.Text, inputDir + "\\" + filenameIn);
            XmlDocumentHelper.SaveInUtf8(tbNewDoc.Text, outputDir + "\\" + filenameRef);
        }

        private void SaveRefCust_Click(object sender, RoutedEventArgs e)
        {
            string desiredName;
            if (ExolutioInputBox.Show("FileName: ", out desiredName) == true)
            {
                SaveRef(desiredName);
            }
        }

        private void bTestOutputCreation_Click(object sender, RoutedEventArgs e)
        {
            CreateSampleDocument();

            XsdValidator validator = new XsdValidator { };
            
            if (!validator.ValidateDocument(schemaVersion1, tbOldDoc.Text))
                throw new Exception("Old document invalid");

            Transform();

            validator = new XsdValidator {  };
            if (!validator.ValidateDocument(schemaVersion2, tbNewDoc.Text))
                throw new Exception("New document invalid");

            SaveRef(null);

            MessageBox.Show("Created OK", "Created OK", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void bGenerateOutput_Click(object sender, RoutedEventArgs e)
        {
            CreateSampleNewDocument();
            SaveOutput(tbNewDoc.Text);
        }

        private void SchemaAware_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
