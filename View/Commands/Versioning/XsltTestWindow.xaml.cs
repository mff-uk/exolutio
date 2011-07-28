//#define SAVE_DOC_FOR_TEST
//#define AUTOCREATE_SAMPLES
using System;
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
using Exolutio.Revalidation;
using Exolutio.Revalidation.Changes;
using Exolutio.Revalidation.XSLT;
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
        private static string BASE_DIR = @"D:\Programování\XCase\Test\Evolution\";

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

#if DEBUG
#if SAVE_DOC_FOR_TEST
        const string SAVE_DIR = @"D:\Programování\EVOXSVN\XSLTTest\";
        const string SAVE_DOCUMENT = @"D:\Programování\EVOXSVN\XSLTTest\LastInput.xml";
        public const string SAVE_STYLESHEET = @"D:\Programování\EVOXSVN\XSLTTest\LastStylesheet.xslt";
#endif 
#endif 
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
            XDocument sampleDoc = sampleDataGenerator.Translate(schemaVersion1);

            string documentString = XDocumentToString(sampleDoc);
            tbOldDoc.Text = documentString;

            SaveOutput(documentString);
            UpdateFolding();
        }

        private static void SaveOutput(string sampleDoc)
        {
#if DEBUG
#if SAVE_DOC_FOR_TEST
            int si = sampleDoc.IndexOf("xmlns:xsi=\"");
            int ei = sampleDoc.IndexOf("\"", si + "xmlns:xsi=\"".Length) + 1;
            string text = si != -1 ? sampleDoc.Remove(si, ei - si) : sampleDoc;
            si = text.IndexOf("xmlns=\"");
            ei = text.IndexOf("\"", si + "xmlns=\"".Length) + 1;
            string xmlns = text.Substring(si, ei - si);
            text = text.Remove(si, ei - si);
            File.WriteAllText(SAVE_DOCUMENT, text.Replace("utf-16", "utf-8"), Encoding.UTF8);
#endif
#endif
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CreateSampleDocument();
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Transform();
        }

        private void Transform()
        {
            if (string.IsNullOrEmpty(tbOldDoc.Text) || string.IsNullOrEmpty(tbXslt.Text))
            {
                return;
            }

            string outDoc = XsltProcessing.Transform(tbOldDoc.Text, tbXslt.Text, BASE_DIR);
            tbNewDoc.Text = outDoc;

#if DEBUG
#if SAVE_DOC_FOR_TEST
            int si = outDoc.IndexOf("xmlns:xsi=\"");
            int ei = outDoc.IndexOf("\"", si + "xmlns:xsi=\"".Length) + 1;
            string text = outDoc.Remove(si, ei - si);
            si = text.IndexOf("xmlns=\"");
            ei = text.IndexOf("\"", si + "xmlns=\"".Length) + 1;
            string xmlns = text.Substring(si, ei - si);
            text = text.Remove(si, ei - si);
            File.WriteAllText(SAVE_DOCUMENT.Replace("LastInput", "LastOutput"), text.Replace("utf-16", "utf-8"), Encoding.UTF8);
#endif
#endif
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

            string path = System.IO.Path.GetDirectoryName(schemaVersion2.Project.ProjectFile.FullName) + "\\" +
                System.IO.Path.GetFileNameWithoutExtension(schemaVersion2.Project.ProjectFile.FullName) + "-out" + "\\last-template.xslt";

            if (path.Contains("Test\\Evolution"))
            {
                if (!Directory.Exists(System.IO.Path.GetDirectoryName(path)))
                {
                    Directory.CreateDirectory(System.IO.Path.GetDirectoryName(path));
                }
                File.WriteAllText(path, tbXslt.Text);
            }
            else if (saveFileDialog.ShowDialog() == true)
            {
                File.WriteAllText(saveFileDialog.FileName, tbXslt.Text);
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

        private void bXsltBasic_Click(object sender, RoutedEventArgs e)
        {
            tbXslt.Text = String.Empty; // Evolution.Stylesheets.Stylesheets.XSLT_EMPTY;
        }

        private void cbXsltList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            string file = cbXsltList.SelectedValue.ToString();

            string xsltString = File.ReadAllText(BASE_DIR + file);
            tbXslt.Text = xsltString;
            UpdateFolding();
        }
        
        private void bXsltFromChanges_Click(object sender, RoutedEventArgs e)
        {
            XsltRevalidationScriptGenerator xsltTemplateGenerator = new XsltRevalidationScriptGenerator();
            string xslt = null;
            
            xsltTemplateGenerator.Initialize(SchemaVersion1, SchemaVersion2, ChangeInstances);
            xsltTemplateGenerator.GenerateTemplateStructure();

            XDocument revalidationStylesheet = xsltTemplateGenerator.GetRevalidationStylesheet();
            StringBuilder sb = new StringBuilder();
            using (TextWriter tw = new StringWriter(sb))
            {
                revalidationStylesheet.Save(tw);
            }
            xslt = sb.ToString();

            tbXslt.Text = xslt;
            #if DEBUG
            #if SAVE_DOC_FOR_TEST
            File.WriteAllText(SAVE_STYLESHEET, xslt);
            string dir = Path.GetDirectoryName(schemaVersion2.Project.ProjectFile.FullName) + "\\" + Path.GetFileNameWithoutExtension(schemaVersion2.Project.ProjectFile.FullName) + "-out";
            if (!Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            File.WriteAllText(dir + "\\last-stylesheet.xslt", xslt);
            #endif
            #endif
            UpdateFolding();
        }

        

        private void bValidateOld_Click(object sender, RoutedEventArgs e)
        {
            DocumentValidator validator = new DocumentValidator();
            validator.ValidateDocument(schemaVersion1, tbOldDoc.Text);
        }

        private void bValidateNew_Click(object sender, RoutedEventArgs e)
        {
            DocumentValidator validator = new DocumentValidator();
            validator.ValidateDocument(schemaVersion2, tbNewDoc.Text);
        }

        private void SaveRef_Click(object sender, RoutedEventArgs e)
        {
            SaveRef(null);
        }

        private void SaveRef(string desiredName)
        {
            string name = Path.GetFileNameWithoutExtension(schemaVersion2.Project.ProjectFile.FullName);
            string dir = Path.GetDirectoryName(schemaVersion2.Project.ProjectFile.FullName);

            string inputDir = dir + "\\" + name + "-in";
            string outputDir = dir + "\\" + name + "-out";

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
            string filenameIn = string.Format("{0}.xml", s);
            string filenameRef = string.Format("{0}-reference.xml", s);

            File.WriteAllText(inputDir + "\\" + filenameIn, tbOldDoc.Text);
            File.WriteAllText(outputDir + "\\" + filenameRef, tbNewDoc.Text);
        }

        private void SaveRefCust_Click(object sender, RoutedEventArgs e)
        {
            string desiredName;
            if (ExolutioInputBox.Show("FileName: ", out desiredName) == true)
            {
                SaveRef(desiredName);
            }
        }

        private void TestOutputCreation_Click(object sender, RoutedEventArgs e)
        {
            CreateSampleDocument();

            DocumentValidator validator = new DocumentValidator { SilentMode = true };
            
            if (!validator.ValidateDocument(schemaVersion1, tbOldDoc.Text))
                throw new Exception("Old document invalid");

            Transform();

            validator = new DocumentValidator { SilentMode = true };
            if (!validator.ValidateDocument(schemaVersion2, tbNewDoc.Text))
                throw new Exception("New document invalid");

            SaveRef(null);

            MessageBox.Show("Created OK", "Created OK", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void GenerateOutput_Click(object sender, RoutedEventArgs e)
        {
            CreateSampleNewDocument();
            SaveOutput(tbNewDoc.Text);
        }
    }
}
