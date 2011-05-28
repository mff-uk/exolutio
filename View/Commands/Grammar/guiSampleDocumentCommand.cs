using System;
using System.Xml;
using System.Xml.Linq;
using Exolutio.DataGenerator;
using Exolutio.Dialogs;
using Exolutio.Model.PSM;
using Exolutio.ResourceLibrary;
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
            Current.MainWindow.FilePresenter.DisplaySampleFile(xmlDocument);

#if SILVERLIGHT

            HelpBox helpBox = new HelpBox();
            Current.MainWindow.FloatingWindowHost.Add(helpBox);
            helpBox.ShowModal();
#else
#endif
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