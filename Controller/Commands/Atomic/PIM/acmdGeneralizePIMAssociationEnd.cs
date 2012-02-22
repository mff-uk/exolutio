using System;
using Exolutio.Model.PIM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PIM
{
    internal class acmdGeneralizePIMAssociationEnd : AtomicCommand
    {
        Guid associationEndGuid, oldClassGuid;
        int index;

        public acmdGeneralizePIMAssociationEnd(Controller c, Guid pimAttributeGuid)
            : base(c)
        {
            associationEndGuid = pimAttributeGuid;
        }

        public override bool CanExecute()
        {
            if (associationEndGuid == Guid.Empty)
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }
            PIMAssociationEnd associationEnd = Project.TranslateComponent<PIMAssociationEnd>(associationEndGuid);
            PIMClass oldclass = associationEnd.PIMClass;
            PIMClass newclass = oldclass.GeneralizationAsSpecific == null ? null : oldclass.GeneralizationAsSpecific.General;
            if (newclass == null)
            {
                ErrorDescription = CommandErrors.CMDERR_NO_GENERALIZATION;
                return false;
            }
            return true;
        }
        
        internal override void CommandOperation()
        {
            PIMAssociationEnd pimAssociationEnd = Project.TranslateComponent<PIMAssociationEnd>(associationEndGuid);
            PIMClass oldClass = pimAssociationEnd.PIMClass;
            oldClassGuid = oldClass;
            PIMClass newClass = oldClass.GeneralizationAsSpecific.General;
            index = oldClass.PIMAssociationEnds.IndexOf(pimAssociationEnd);
            Report = new CommandReport("{0} generalized from {1} to {2}.", pimAssociationEnd, oldClass, newClass);

            oldClass.PIMAssociationEnds.Remove(pimAssociationEnd);
            pimAssociationEnd.PIMClass = newClass;
            newClass.PIMAssociationEnds.Add(pimAssociationEnd);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PIMAssociationEnd pimAssociationEnd = Project.TranslateComponent<PIMAssociationEnd>(associationEndGuid);
            PIMClass oldClass = Project.TranslateComponent<PIMClass>(oldClassGuid);
            PIMClass newClass = oldClass.GeneralizationAsSpecific.General;

            newClass.PIMAssociationEnds.Remove(pimAssociationEnd);
            pimAssociationEnd.PIMClass = oldClass;
            oldClass.PIMAssociationEnds.Insert(pimAssociationEnd, index);
            return OperationResult.OK;
        }

        /*internal override PropagationMacroCommand PrePropagation()
        {
            
        }*/
    }
}
