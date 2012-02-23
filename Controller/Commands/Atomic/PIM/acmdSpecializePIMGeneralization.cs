using System;
using Exolutio.Model.PIM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PIM
{
    internal class acmdSpecializePIMGeneralization : AtomicCommand
    {
        Guid generalizationGuid, generalClassGuid, specialClassGuid;
        int index;

        public acmdSpecializePIMGeneralization(Controller c, Guid pimGeneralizationGuid, Guid specialGuid)
            : base(c)
        {
            generalizationGuid = pimGeneralizationGuid;
            specialClassGuid = specialGuid;
        }

        public override bool CanExecute()
        {
            if (generalizationGuid == Guid.Empty || specialClassGuid == Guid.Empty)
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }
            PIMGeneralization pimGeneralization = Project.TranslateComponent<PIMGeneralization>(generalizationGuid);
            PIMClass oldclass = pimGeneralization.General;
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
            PIMGeneralization pimGeneralization = Project.TranslateComponent<PIMGeneralization>(generalizationGuid);
            PIMClass generalClass = pimGeneralization.General;
            generalClassGuid = generalClass;
            PIMClass specialClass = Project.TranslateComponent<PIMClass>(specialClassGuid);
            index = generalClass.GeneralizationsAsGeneral.IndexOf(pimGeneralization);
            Report = new CommandReport("{0} specialized from {1} to {2}.", pimGeneralization, generalClass, specialClass);

            generalClass.GeneralizationsAsGeneral.Remove(pimGeneralization);
            pimGeneralization.General = specialClass;
            specialClass.GeneralizationsAsGeneral.Add(pimGeneralization);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PIMGeneralization pimGeneralization = Project.TranslateComponent<PIMGeneralization>(generalizationGuid);
            PIMClass generalClass = Project.TranslateComponent<PIMClass>(generalClassGuid);
            PIMClass specialClass = Project.TranslateComponent<PIMClass>(specialClassGuid);

            specialClass.GeneralizationsAsGeneral.Remove(pimGeneralization);
            pimGeneralization.General = generalClass;
            generalClass.GeneralizationsAsGeneral.Insert(pimGeneralization, index);
            return OperationResult.OK;
        }

        /*internal override PropagationMacroCommand PrePropagation()
        {
            
        }*/
    }
}
