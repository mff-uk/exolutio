using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Model.PSM;
using EvoX.Model;

namespace EvoX.Controller.Commands.Atomic.PSM
{
    public class acmdReconnectPSMAssociation : StackedCommand
    {
        Guid associationGuid, newClassGuid, oldClassGuid;
        int index;

        public acmdReconnectPSMAssociation(Controller c, Guid psmAssociationGuid, Guid psmClassGuid)
            : base(c)
        {
            newClassGuid = psmClassGuid;
            associationGuid = psmAssociationGuid;
        }

        public override bool CanExecute()
        {
            if (!(newClassGuid != Guid.Empty
                && Project.VerifyComponentType<PSMClass>(newClassGuid)
                && associationGuid != Guid.Empty
                && Project.VerifyComponentType<PSMAssociation>(associationGuid)
                && Project.TranslateComponent<PSMAssociation>(associationGuid).Parent is PSMClass))
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }

            PSMClass newClass = Project.TranslateComponent<PSMClass>(newClassGuid);
            PSMClass oldClass = Project.TranslateComponent<PSMAssociation>(associationGuid).Parent as PSMClass;

            //the two classes connected by an association (atomic operation)
            if (newClass.ParentAssociation != null && newClass.ParentAssociation.Parent == oldClass) return true;
            if (oldClass.ParentAssociation != null && oldClass.ParentAssociation.Parent == newClass) return true;

            if (newClass.RepresentedClass == oldClass || oldClass.RepresentedClass == newClass) return true;

            ErrorDescription = CommandErrors.CMDERR_NO_COMMON_ASSOCIATION_OR_REPR;
            return false;

        }
        
        internal override void CommandOperation()
        {
            PSMAssociation psmAssociation = Project.TranslateComponent<PSMAssociation>(associationGuid);
            PSMClass oldClass = psmAssociation.Parent as PSMClass;
            oldClassGuid = oldClass;
            PSMClass newClass = Project.TranslateComponent<PSMClass>(newClassGuid);

            index = oldClass.ChildPSMAssociations.IndexOf(psmAssociation);
            oldClass.ChildPSMAssociations.Remove(psmAssociation);
            psmAssociation.Parent = newClass;
            newClass.ChildPSMAssociations.Add(psmAssociation);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PSMAssociation psmAssociation = Project.TranslateComponent<PSMAssociation>(associationGuid);
            PSMClass oldClass = Project.TranslateComponent<PSMClass>(oldClassGuid);
            PSMClass newClass = Project.TranslateComponent<PSMClass>(newClassGuid);

            newClass.ChildPSMAssociations.Remove(psmAssociation);
            psmAssociation.Parent = oldClass;
            oldClass.ChildPSMAssociations.Insert(psmAssociation, index);
            return OperationResult.OK;
        }
    }
}
