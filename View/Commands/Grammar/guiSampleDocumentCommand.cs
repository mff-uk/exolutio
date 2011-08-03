using System;
using System.Xml;
using System.Xml.Linq;
using Exolutio.DataGenerator;
using Exolutio.Dialogs;
using Exolutio.Model.PSM;
using Exolutio.ResourceLibrary;
using Exolutio.SupportingClasses;
using Exolutio.View.Commands.Grammar;

namespace Exolutio.View.Commands
{
    public class guiSampleDocumentCommand : guiActiveDiagramCommand
    {
        public override bool CanExecute(object parameter)
        {
            return Current.ActiveDiagram != null && Current.ActiveDiagram is PSMDiagram;
        }

        public override void Execute(object parameter)
        {
            SampleDataGenerator g = new SampleDataGenerator();
            XDocument xmlDocument = g.Translate((PSMSchema) Current.ActiveDiagram.Schema);
            FilePresenterButton[] additionalButtons = new [] { new FilePresenterButton() { Text = "Generate another file", Icon = ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.xmlIcon), UpdateFileContentAction = GenerateAnotherFile} };
            Current.MainWindow.FilePresenter.DisplayFile(xmlDocument, EDisplayedFileType.XML, Current.ActiveDiagram.Caption + "_sample.xml", g.Log, (PSMSchema) Current.ActiveDiagram.Schema, additionalButtons);


        }

        private void GenerateAnotherFile(IFilePresenterTab filetab)
        {
            SampleDataGenerator g = new SampleDataGenerator();
            XDocument xmlDocument = g.Translate(filetab.ValidationSchema);
            filetab.SetDocumentText(xmlDocument.ToString());
        }

        public override string Text
        {
            get { return "Sample XML"; }
            set
            {
                base.Text = value;
            }
        }

        public override string ScreenTipText
        {
            get { return "Create sample XML file for the PSM schema "; }
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.xmlIcon); }
        }
    }
}