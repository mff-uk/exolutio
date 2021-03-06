using Exolutio.ResourceLibrary;
using Exolutio.Model.PSM;
using System.Linq;
using System.Collections.Generic;
using Exolutio.Controller.Commands.Complex.PSM;
using Exolutio.Controller.Commands;

namespace Exolutio.View.Commands.PSM
{
    public class guiSplitPSMAssociation : guiSelectionDependentCommand
    {
        public uint Count = 2;
        
        public override bool CanExecute(object parameter)
        {
            if (!(Current.ActiveDiagram is PSMDiagram)) return false;

            IEnumerable<PSMAssociation> selectedAssociations = Current.ActiveDiagramView.GetSelectedComponents()
                .Where(c => c is PSMAssociation).Cast<PSMAssociation>();

            return selectedAssociations.Count() > 0 && selectedAssociations.All(a => a.Child is PSMClass);

        }

        public override void Execute(object parameter)
        {
            IEnumerable<PSMAssociation> selectedAssociations = Current.ActiveDiagramView.GetSelectedComponents()
                .Where(c => c is PSMAssociation).Cast<PSMAssociation>();

            MacroCommand command = new MacroCommand(Current.Controller);
            foreach (PSMAssociation a in selectedAssociations)
            {
                command.Commands.Add(new cmdSplitPSMAssociation(Current.Controller) { PSMAssociationGuid = a, Count = Count});
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
            get { return "Split PSM associations into " + Count; }
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get
            {
                return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.split_psm_assoc);
            }
        }
    }
}