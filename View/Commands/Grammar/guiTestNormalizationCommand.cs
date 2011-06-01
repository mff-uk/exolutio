using System;
using Exolutio.Controller.Commands;
using Exolutio.Dialogs;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.ResourceLibrary;

namespace Exolutio.View.Commands.Grammar
{
    public class guiTestNormalizationCommand : guiActiveDiagramCommand
    {
        public IReportDisplay ReportDisplay { get; set; }

        public override void Execute(object parameter)
        {
            if (Current.ActiveDiagram != null && Current.ActiveDiagram.Schema is PSMSchema)
            {
                ModelVerifier verifier = new ModelVerifier();

                if (!verifier.TestSchemaNormalized((PSMSchema)Current.ActiveDiagram.Schema))
                {
                    ExolutioMessageBox.Show("Normalization", "Schema is not normalized", "Check command log window for details.");
                    Current.MainWindow.DisplayLog(verifier.Log, true);
                }
                else
                {
                    ExolutioMessageBox.Show("Normalization", "Normalization passed", "Schema is normalized");
                    Current.MainWindow.DisplayReport(new CommandReport("Schema is normalized. "), true);
                }
            }
        }

        public override string Text
        {
            get { return "Test normalization"; }
        }

        public override string ScreenTipText
        {
            get { return "Test whether PSM schema is normalized"; }
        }

        public override bool CanExecute(object parameter)
        {
            return Current.ActiveDiagram != null && Current.ActiveDiagram is PSMDiagram;
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.component_preferences); }
        }

    }
}