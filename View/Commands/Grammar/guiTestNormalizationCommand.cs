using System;
using Exolutio.Controller.Commands;
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
                    if (ReportDisplay != null)
                    {
                        ReportDisplay.DisplayedReport = null;
                        ReportDisplay.DisplayedLog = verifier.Log;
                        ReportDisplay.Update();
                    }
                }
                else
                {
                    if (ReportDisplay != null)
                    {
                        ReportDisplay.DisplayedLog = null;
                        ReportDisplay.DisplayedReport = new CommandReport("Schema is normalized. ");
                        ReportDisplay.Update();
                    }
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