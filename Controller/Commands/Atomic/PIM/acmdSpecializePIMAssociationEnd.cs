using System;
using Exolutio.Model.PIM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PIM
{
    internal class acmdSpecializePIMAssociationEnd : AtomicCommand
    {
        Guid associationEndGuid, generalClassGuid, specialClassGuid;
        int index;

        public acmdSpecializePIMAssociationEnd(Controller c, Guid pimAssociationEndGuid, Guid specialGuid)
            : base(c)
        {
            associationEndGuid = pimAssociationEndGuid;
            specialClassGuid = specialGuid;
        }

        public override bool CanExecute()
        {
            if (associationEndGuid == Guid.Empty || specialClassGuid == Guid.Empty)
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }
            PIMAssociationEnd associationEnd = Project.TranslateComponent<PIMAssociationEnd>(associationEndGuid);
            PIMClass oldclass = associationEnd.PIMClass;
            PIMClass newclass = Project.TranslateComponent<PIMClass>(specialClassGuid);
            if (newclass.GeneralizationAsSpecific == null || newclass.GeneralizationAsSpecific.General != oldclass)
            {
                ErrorDescription = CommandErrors.CMDERR_INVALID_SPECIALIZATION;
                return false;
            }
            return true;
        }
        
        internal override void CommandOperation()
        {
            PIMAssociationEnd pimAssociationEnd = Project.TranslateComponent<PIMAssociationEnd>(associationEndGuid);
            PIMClass generalClass = pimAssociationEnd.PIMClass;
            generalClassGuid = generalClass;
            PIMClass specialClass = Project.TranslateComponent<PIMClass>(specialClassGuid);
            index = generalClass.PIMAssociationEnds.IndexOf(pimAssociationEnd);
            Report = new CommandReport("{0} specialized from {1} to {2}.", pimAssociationEnd, generalClass, specialClass);

            generalClass.PIMAssociationEnds.Remove(pimAssociationEnd);
            pimAssociationEnd.PIMClass = specialClass;
            specialClass.PIMAssociationEnds.Add(pimAssociationEnd);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PIMAssociationEnd pimAssociationEnd = Project.TranslateComponent<PIMAssociationEnd>(associationEndGuid);
            PIMClass generalClass = Project.TranslateComponent<PIMClass>(generalClassGuid);
            PIMClass specialClass = Project.TranslateComponent<PIMClass>(specialClassGuid);

            specialClass.PIMAssociationEnds.Remove(pimAssociationEnd);
            pimAssociationEnd.PIMClass = generalClass;
            generalClass.PIMAssociationEnds.Insert(pimAssociationEnd, index);
            return OperationResult.OK;
        }

        /*internal override PropagationMacroCommand PrePropagation()
        {
            
        }*/
    }
}
