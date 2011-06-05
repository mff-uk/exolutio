using System;
using Exolutio.Dialogs;
using Exolutio.ResourceLibrary;
using Exolutio.Model.PSM;
using System.Linq;
using System.Collections.Generic;
using Exolutio.Controller.Commands.Complex.PSM;
using Exolutio.Controller.Commands;
using Exolutio.Controller.Commands.Atomic.PSM;
using System.Diagnostics;

namespace Exolutio.View.Commands.PSM
{
    public class guiDeleteSubtree : guiSelectionDependentCommand
    {
        public override bool CanExecute(object parameter)
        {
            if (!(Current.ActiveDiagram is PSMDiagram)) return false;

            if (Current.ActiveDiagramView.IsSelectedComponentOfType(typeof(PSMSchemaClass))
                || Current.ActiveDiagramView.IsSelectedComponentOfType(typeof(PSMAttribute))) return false;

            if (Current.ActiveDiagramView.GetSelectedComponents().Count() != 1) return false;

            return true;

        }

        public override void Execute(object parameter)
        {
            PSMComponent component = Current.ActiveDiagramView.GetSingleSelectedComponentOrNull() as PSMComponent;

            if (!(component is PSMAssociation)) component = (component as PSMAssociationMember).ParentAssociation;
                cmdDeletePSMAssociationRecursive da = new cmdDeletePSMAssociationRecursive(Current.Controller) { AssociationGuid = component };
            da.Execute();
        }

        public override string Text
        {
            get
            {
                return "Delete subtree";
            }
        }

        public override string ScreenTipText
        {
            get { return "Delete PSM subtree"; }
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get
            {
                return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.delete);
            }
        }
    }
}