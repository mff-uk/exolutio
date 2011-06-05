using System;
using Exolutio.Dialogs;
using Exolutio.ResourceLibrary;
using Exolutio.Model.PSM;
using System.Linq;
using System.Collections.Generic;
using Exolutio.Controller.Commands.Complex.PSM;
using Exolutio.Controller.Commands;
using Exolutio.Controller.Commands.Atomic.PIM;
using Exolutio.Model.PIM;
using Exolutio.Controller.Commands.Complex.PIM;
using Exolutio.Controller.Commands.Atomic.PIM.MacroWrappers;

namespace Exolutio.View.Commands.PIM
{
    public class guiPIMDelete : guiSelectionDependentCommand
    {
        public override bool CanExecute(object parameter)
        {
            if (!(Current.ActiveDiagram is PIMDiagram)) return false;

            IEnumerable<PIMComponent> selectedPIMComponents = Current.ActiveDiagramView.GetSelectedComponents()
                .Where(c => c is PIMComponent).Cast<PIMComponent>();
            return selectedPIMComponents.Count() > 0;
        }

        public override void Execute(object parameter)
        {
            IEnumerable<PIMAssociation> selectedAssociations = Current.ActiveDiagramView.GetSelectedComponents()
                .Where(c => c is PIMAssociation).Cast<PIMAssociation>();
            IEnumerable<PIMAttribute> selectedAttributes = Current.ActiveDiagramView.GetSelectedComponents()
                .Where(c => c is PIMAttribute).Cast<PIMAttribute>();
            IEnumerable<PIMClass> selectedClasses = Current.ActiveDiagramView.GetSelectedComponents()
                .Where(c => c is PIMClass).Cast<PIMClass>();

            MacroCommand macro = new MacroCommand(Current.Controller);
            foreach (PIMAssociation a in selectedAssociations)
            {
                macro.Commands.Add(new Exolutio.Controller.Commands.Complex.PIM.cmdDeletePIMAssociation(Current.Controller) { AssociationGuid = a });
            }
            foreach (PIMAttribute a in selectedAttributes)
            {
                macro.Commands.Add(new Exolutio.Controller.Commands.Complex.PIM.cmdDeletePIMAttribute(Current.Controller) { AttributeGuid = a });
            }
            foreach (PIMClass c in selectedClasses)
            {
                macro.Commands.Add(new Exolutio.Controller.Commands.Complex.PIM.cmdDeletePIMClass(Current.Controller) { ClassGuid = c });
            }
            macro.Execute();
        }

        public override string Text
        {
            get
            {
                return "Delete";
            }
        }

        public override string ScreenTipText
        {
            get { return "Delete selected components"; }
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get { return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.delete); }
        }

    }
}