using Exolutio.ResourceLibrary;
using System.Linq;
using System.Collections.Generic;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Controller.Commands.Complex.PIM;

namespace Exolutio.View.Commands.PIM
{
    public class guiSplitPIMAssociation : guiSelectionDependentCommand
    {
        public uint Count = 2;
        
        public override bool CanExecute(object parameter)
        {
            if (!(Current.ActiveDiagram is PIMDiagram)) return false;

            IEnumerable<PIMAssociation> selectedAssociations = Current.ActiveDiagramView.GetSelectedComponents()
                .Where(c => c is PIMAssociation).Cast<PIMAssociation>();

            return selectedAssociations.Count() > 0;

        }

        public override void Execute(object parameter)
        {
            IEnumerable<PIMAssociation> selectedAssociations = Current.ActiveDiagramView.GetSelectedComponents()
                .Where(c => c is PIMAssociation).Cast<PIMAssociation>();

            MacroCommand command = new MacroCommand(Current.Controller);
            foreach (PIMAssociation a in selectedAssociations)
            {
                command.Commands.Add(new cmdSplitPIMAssociation(Current.Controller) { PIMAssociationGuid = a, Count = Count });
            }
            command.Execute();
        }

        public override string Text
        {
            get
            {
                return Count.ToString();
            }
        }

        public override string ScreenTipText
        {
            get { return "Split PIM associations into " + Count; }
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get
            {
                return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.split_pim_assoc);
            }
        }
    }
}