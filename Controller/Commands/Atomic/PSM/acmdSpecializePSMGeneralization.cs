using System;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM
{
    internal class acmdSpecializePSMGeneralization : AtomicCommand
    {
        Guid generalizationGuid, generalClassGuid, specialClassGuid;
        int index;

        public acmdSpecializePSMGeneralization(Controller c, Guid psmGeneralizationGuid, Guid specialGuid)
            : base(c)
        {
            generalizationGuid = psmGeneralizationGuid;
            specialClassGuid = specialGuid;
        }

        public override bool CanExecute()
        {
            if (generalizationGuid == Guid.Empty || specialClassGuid == Guid.Empty)
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }
            PSMGeneralization pimGeneralization = Project.TranslateComponent<PSMGeneralization>(generalizationGuid);
            PSMClass oldclass = pimGeneralization.General;
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
            PSMGeneralization psmGeneralization = Project.TranslateComponent<PSMGeneralization>(generalizationGuid);
            PSMClass generalClass = psmGeneralization.General;
            generalClassGuid = generalClass;
            PSMClass specialClass = Project.TranslateComponent<PSMClass>(specialClassGuid);
            index = generalClass.GeneralizationsAsGeneral.IndexOf(psmGeneralization);
            Report = new CommandReport("{0} specialized from {1} to {2}.", psmGeneralization, generalClass, specialClass);

            generalClass.GeneralizationsAsGeneral.Remove(psmGeneralization);
            psmGeneralization.General = specialClass;
            specialClass.GeneralizationsAsGeneral.Add(psmGeneralization);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PSMGeneralization psmGeneralization = Project.TranslateComponent<PSMGeneralization>(generalizationGuid);
            PSMClass generalClass = Project.TranslateComponent<PSMClass>(generalClassGuid);
            PSMClass specialClass = Project.TranslateComponent<PSMClass>(specialClassGuid);

            specialClass.GeneralizationsAsGeneral.Remove(psmGeneralization);
            psmGeneralization.General = generalClass;
            generalClass.GeneralizationsAsGeneral.Insert(psmGeneralization, index);
            return OperationResult.OK;
        }

        /*internal override PropagationMacroCommand PrePropagation()
        {
            
        }*/
    }
}
