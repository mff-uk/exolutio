using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Model.PIM;
using EvoX.Model;

namespace EvoX.Controller.Commands.Atomic.PIM
{
    public class acmdMoveAssociationEnd : StackedCommand
    {
        Guid associationEndGuid, newClassGuid, oldClassGuid;

        public acmdMoveAssociationEnd(Controller c, Guid pimAssociationEndGuid, Guid pimClassGuid)
            : base(c)
        {
            newClassGuid = pimClassGuid;
            associationEndGuid = pimAssociationEndGuid;
        }

        public override bool CanExecute()
        {
            if (!(newClassGuid != Guid.Empty
                && Project.VerifyComponentType<PIMClass>(newClassGuid)
                && associationEndGuid != Guid.Empty
                && Project.VerifyComponentType<PIMAssociationEnd>(associationEndGuid)))
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }

            PIMClass newclass = Project.TranslateComponent<PIMClass>(newClassGuid);
            PIMClass oldclass = Project.TranslateComponent<PIMAssociationEnd>(associationEndGuid).PIMClass;

            if (newclass.GetAssociationsWith(oldclass).Count<PIMAssociation>() == 0)
            {
                ErrorDescription = CommandErrors.CMDERR_NO_COMMON_ASSOCIATION;
                return false;
            }
            return true;
        }
        
        internal override void CommandOperation()
        {
            PIMAssociationEnd pimAssociationEnd = Project.TranslateComponent<PIMAssociationEnd>(associationEndGuid);
            PIMClass oldClass = pimAssociationEnd.PIMClass;
            oldClassGuid = oldClass;
            PIMClass newClass = Project.TranslateComponent<PIMClass>(newClassGuid);
            Report = new CommandReport("{0} moved from {1} to {2}.", pimAssociationEnd, oldClass, newClass);

            oldClass.PIMAssociationEnds.Remove(pimAssociationEnd);
            pimAssociationEnd.PIMClass = newClass;
            newClass.PIMAssociationEnds.Remove(pimAssociationEnd);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PIMAssociationEnd pimAssociationEnd = Project.TranslateComponent<PIMAssociationEnd>(associationEndGuid);
            PIMClass oldClass = Project.TranslateComponent<PIMClass>(oldClassGuid);
            PIMClass newClass = Project.TranslateComponent<PIMClass>(newClassGuid);

            newClass.PIMAssociationEnds.Remove(pimAssociationEnd);
            pimAssociationEnd.PIMClass = oldClass;
            oldClass.PIMAssociationEnds.Remove(pimAssociationEnd);
            return OperationResult.OK;
        }
    }
}
