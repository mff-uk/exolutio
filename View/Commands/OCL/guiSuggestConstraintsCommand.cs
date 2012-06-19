using System.Collections.Generic;
using Exolutio.Model.OCL.AST;
using Exolutio.Model.OCL.ConstraintConversion;
using Exolutio.Model.PSM;
using Exolutio.ResourceLibrary;

namespace Exolutio.View.Commands.OCL
{
    public class guiSuggestConstraintsCommand : guiActiveDiagramCommand
    {
        public override void Execute(object parameter = null)
        {
            if (Current.ActiveDiagram != null && Current.ActiveDiagram is PSMDiagram)
            {
                ConstraintsSuggestor suggestor = new ConstraintsSuggestor();
                PSMSchema psmSchema = (PSMSchema) Current.ActiveDiagram.Schema;
                IList<ClassifierConstraintBlock> constraints = suggestor.FindSuitableConstraints(Current.ProjectVersion.PIMSchema, psmSchema);
                SuggestConstraintsWindow w = new SuggestConstraintsWindow();
                w.PSMSchema = psmSchema;
                w.Show();
                w.DisplayConstraints(constraints);
            }
        }

        public override string Text
        {
            get { return "Suggest constraints"; }
        }

        public override string ScreenTipText
        {
            get { return "Examine the OCL constraints defined in PIM and suggest those that can be translated to PSM constraints for this schema." ; }
        }

        public override bool CanExecute(object parameter)
        {
            return Current.ActiveDiagram != null && Current.ActiveDiagram is PSMDiagram;
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.documents_new); }
        }
    }
}