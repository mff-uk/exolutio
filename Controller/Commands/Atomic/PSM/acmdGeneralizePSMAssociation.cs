using System;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM
{
    internal class acmdGeneralizePSMAssociation : AtomicCommand
    {
        Guid associationGuid, oldParentGuid;
        int index;

        public acmdGeneralizePSMAssociation(Controller c, Guid psmAssociationGuid)
            : base(c)
        {
            associationGuid = psmAssociationGuid;
        }

        public override bool CanExecute()
        {
            if (associationGuid == Guid.Empty)
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }
            PSMAssociation association = Project.TranslateComponent<PSMAssociation>(associationGuid);
            PSMClass oldclass = association.Parent as PSMClass;
            if (oldclass == null)
            {
                ErrorDescription = CommandErrors.CMDERR_PARENT_NOT_PSMCLASS;
                return false;
            }
            PSMClass newclass = oldclass.GeneralizationAsSpecific == null ? null : oldclass.GeneralizationAsSpecific.General;
            if (newclass == null)
            {
                ErrorDescription = CommandErrors.CMDERR_NO_GENERALIZATION;
                return false;
            }
            return true;
        }
        
        internal override void CommandOperation()
        {
            PSMAssociation psmAssociation = Project.TranslateComponent<PSMAssociation>(associationGuid);
            PSMClass oldClass = psmAssociation.Parent as PSMClass;
            oldParentGuid = oldClass;
            PSMClass newClass = oldClass.GeneralizationAsSpecific.General;
            index = oldClass.ChildPSMAssociations.IndexOf(psmAssociation);
            Report = new CommandReport("{0} generalized from {1} to {2}.", psmAssociation, oldClass, newClass);

            oldClass.ChildPSMAssociations.Remove(psmAssociation);
            psmAssociation.Parent = newClass;
            newClass.ChildPSMAssociations.Add(psmAssociation);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PSMAssociation psmAssociation = Project.TranslateComponent<PSMAssociation>(associationGuid);
            PSMClass oldClass = Project.TranslateComponent<PSMClass>(oldParentGuid);
            PSMClass newClass = oldClass.GeneralizationAsSpecific.General;

            newClass.ChildPSMAssociations.Remove(psmAssociation);
            psmAssociation.Parent = oldClass;
            oldClass.ChildPSMAssociations.Insert(psmAssociation, index);
            return OperationResult.OK;
        }

        /*internal override PropagationMacroCommand PrePropagation()
        {
            
        }*/
    }
}
