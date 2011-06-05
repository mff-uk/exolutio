using System;
using Exolutio.Dialogs;
using Exolutio.ResourceLibrary;
using Exolutio.Model.PSM;
using System.Linq;
using System.Collections.Generic;
using Exolutio.Controller.Commands.Complex.PSM;
using Exolutio.Controller.Commands;
using Exolutio.Controller.Commands.Atomic.PSM;

namespace Exolutio.View.Commands.PSM
{
    public class guiLeaveOutUnintAM : guiSelectionDependentCommand
    {
        public override bool CanExecute(object parameter)
        {
            if (!(Current.ActiveDiagram is PSMDiagram)) return false;

            if (!Current.ActiveDiagramView.IsSelectedComponentOfType(typeof(PSMAssociationMember))) return false;

            PSMAssociationMember associationMember = Current.ActiveDiagramView.GetSingleSelectedComponentOrNull() as PSMAssociationMember;

            if (associationMember.ParentAssociation != null && associationMember.ParentAssociation.Interpretation != null) return false;
            if (associationMember is PSMClass && (associationMember as PSMClass).Interpretation != null) return false;
            if (associationMember is PSMSchemaClass) return false;
            return true;

        }

        public override void Execute(object parameter)
        {
            PSMAssociationMember associationMember = Current.ActiveDiagramView.GetSingleSelectedComponentOrNull() as PSMAssociationMember;
            cmdLeaveOutUninterpretedAssociationMember command = new cmdLeaveOutUninterpretedAssociationMember(Current.Controller);
            command.Set(associationMember);
            command.Execute();
        }

        public override string Text
        {
            get
            {
                return "Leave out";
            }
        }

        public override string ScreenTipText
        {
            get { return "Leave out content model or uninterpreted class"; }
        }

        public override System.Windows.Media.ImageSource Icon
        {
            get
            {
                return ExolutioResourceNames.GetResourceImageSource(ExolutioResourceNames.RemoveContainer);
            }
        }
    }
}