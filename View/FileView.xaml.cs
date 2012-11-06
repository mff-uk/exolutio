using System;
using System.Collections.Generic;
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
using System.Xml.Schema;
using Exolutio.SupportingClasses;
using ICSharpCode.AvalonEdit.Folding;
using ICSharpCode.AvalonEdit.Highlighting;
using Microsoft.Win32;

namespace Exolutio.View
{
    /// <summary>
    /// Interaction logic for FileView.xaml
    /// </summary>
    public partial class FileView : UserControl
    {
        public FileView()
        {
            InitializeComponent();
        }

        private bool allowEdit;
        public bool AllowEdit
        {
            get { return allowEdit; }
            set { allowEdit = value; }
        }

        private EDisplayedFileType displayedFileType;

        public EDisplayedFileType DisplayedFileType
        {
            get { return displayedFileType; }
            set { displayedFileType = value; }
        }

        public void DisplayFile(EDisplayedFileType displayedFileType, string fileContents)
        {
            DisplayedFileType = displayedFileType;
            FileContents = fileContents;
            tbDocument.WordWrap = true;
            Init();
        }

        public void DisplayFile(EDisplayedFileType displayedFileType, Stream fileContents)
        {
            using (StreamReader sr = new StreamReader(fileContents))
            {
                string contents = sr.ReadToEnd();
                DisplayFile(displayedFileType, contents);
            }
        }

        private XmlFoldingStrategy foldingStrategy;
        private FoldingManager foldingManager;

        //private IList<LogMessage> logMessages;

        //public IList<LogMessage> LogMessages
        //{
        //    get
        //    {
        //        return logMessages;
        //    }
        //    set
        //    {
        //        logMessages = value;
        //        LogMessage.ImageGetter = ImageGetter;
        //        gridLog.ItemsSource = logMessages.OrderBy(message => message.Severity == LogMessage.ESeverity.Error ? 0 : 1);
        //        int countw = logMessages.Count(e => e.Severity == LogMessage.ESeverity.Warning);
        //        int counte = logMessages.Count(e => e.Severity == LogMessage.ESeverity.Error);
        //        if (countw > 0 && counte > 0)
        //            expander1.Header = String.Format("Document created with {0} errors and {1} warnings", counte, countw);
        //        else if (countw > 0)
        //            expander1.Header = String.Format("Document created with {0} warnings", countw);
        //        else if (counte > 0)
        //            expander1.Header = String.Format("Document created with {0} errors", counte);
        //        else
        //            expander1.Header = "Document created successfully";
        //    }
        //}

        //public PSMDiagram Diagram { get; set; }

        //private static object ImageGetter(LogMessage message)
        //{
        //    if (message.Severity == LogMessage.ESeverity.Error)
        //        return ContextMenuIcon.GetContextIcon("error_button").Source;
        //    else
        //        return ContextMenuIcon.GetContextIcon("Warning").Source;
        //}

        public string FileContents
        {
            get { return tbDocument.Text; }
            set { 
                tbDocument.Clear();
                tbDocument.Text = value; }
        }

        public bool WordWrap
        {
            get { return tbDocument.WordWrap; }
            set { tbDocument.WordWrap = value; }
        }

        public void Init()
        {
            if (DisplayedFileType.IsAmong(EDisplayedFileType.XML, EDisplayedFileType.XSD, EDisplayedFileType.RNG, EDisplayedFileType.SCH) && 
                (tbDocument.SyntaxHighlighting == null || tbDocument.SyntaxHighlighting.Name != "XML"))
            {
                tbDocument.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance.GetDefinition("XML");
                if (foldingStrategy == null)
                {
                    foldingManager = FoldingManager.Install(tbDocument.TextArea);
                    foldingStrategy = new XmlFoldingStrategy();
                }
            }

            if (DisplayedFileType == EDisplayedFileType.RNC &&
                (tbDocument.SyntaxHighlighting == null || tbDocument.SyntaxHighlighting.Name != "RNC"))
            {
                System.Xml.XmlReader reader;
                using (StringReader sr = new StringReader(Properties.Resources.RNC))
                {
                    reader = new System.Xml.XmlTextReader(sr);
                    tbDocument.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, HighlightingManager.Instance);                    
                }
            }

            tbDocument.ShowLineNumbers = true;
            UpdateFolding();
        }


        private void UpdateFolding()
        {
            if (foldingStrategy != null && !string.IsNullOrEmpty(FileContents))
                foldingStrategy.UpdateFoldings(foldingManager, tbDocument.Document);            

        }

       
        private void tbDocument_TextChanged(object sender, EventArgs e)
        {
            UpdateFolding();
        }

        private void tbDocument_KeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Add && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                tbDocument.FontSize *= 1.1;
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Subtract && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                tbDocument.FontSize *= 0.9;
                e.Handled = true;
                return;
            }

            if (e.Key == Key.Multiply && Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                tbDocument.FontSize = 12;
                e.Handled = true;
                return;
            }
        }       
    }
}
