using System;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM
{
    internal class acmdSpecializePSMAssociation : AtomicCommand
    {
        Guid associationGuid, generalClassGuid, specialClassGuid;
        int index;

        public acmdSpecializePSMAssociation(Controller c, Guid psmAssociationGuid, Guid specialGuid)
            : base(c)
        {
            associationGuid = psmAssociationGuid;
            specialClassGuid = specialGuid;
        }

        public override bool CanExecute()
        {
            if (associationGuid == Guid.Empty || specialClassGuid == Guid.Empty)
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
            PSMClass newclass = Project.TranslateComponent<PSMClass>(specialClassGuid);
            if (newclass.GeneralizationAsSpecific == null || newclass.GeneralizationAsSpecific.General != oldclass)
            {
                ErrorDescription = CommandErrors.CMDERR_INVALID_SPECIALIZATION;
                return false;
            }
            return true;
        }
        
        internal override void CommandOperation()
        {
            PSMAssociation psmAssociation = Project.TranslateComponent<PSMAssociation>(associationGuid);
            PSMClass generalClass = psmAssociation.Parent as PSMClass;
            generalClassGuid = generalClass;
            PSMClass specialClass = Project.TranslateComponent<PSMClass>(specialClassGuid);
            index = generalClass.ChildPSMAssociations.IndexOf(psmAssociation);
            Report = new CommandReport("{0} specialized from {1} to {2}.", psmAssociation, generalClass, specialClass);

            generalClass.ChildPSMAssociations.Remove(psmAssociation);
            psmAssociation.Parent = specialClass;
            specialClass.ChildPSMAssociations.Add(psmAssociation);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PSMAssociation psmAssociation = Project.TranslateComponent<PSMAssociation>(associationGuid);
            PSMClass generalClass = Project.TranslateComponent<PSMClass>(generalClassGuid);
            PSMClass specialClass = Project.TranslateComponent<PSMClass>(specialClassGuid);

            specialClass.ChildPSMAssociations.Remove(psmAssociation);
            psmAssociation.Parent = generalClass;
            generalClass.ChildPSMAssociations.Insert(psmAssociation, index);
            return OperationResult.OK;
        }

        /*internal override PropagationMacroCommand PrePropagation()
        {
            
        }*/
    }
}
