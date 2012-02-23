using System;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM
{
    internal class acmdGeneralizePSMGeneralization : AtomicCommand
    {
        Guid generalizationGuid, oldClassGuid;
        int index;

        public acmdGeneralizePSMGeneralization(Controller c, Guid psmGeneralizationGuid)
            : base(c)
        {
            generalizationGuid = psmGeneralizationGuid;
        }

        public override bool CanExecute()
        {
            if (generalizationGuid == Guid.Empty)
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }
            PSMGeneralization generalization = Project.TranslateComponent<PSMGeneralization>(generalizationGuid);
            PSMClass oldclass = generalization.General;
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
            PSMGeneralization psmGeneralization = Project.TranslateComponent<PSMGeneralization>(generalizationGuid);
            PSMClass oldClass = psmGeneralization.General;
            oldClassGuid = oldClass;
            PSMClass newClass = oldClass.GeneralizationAsSpecific.General;
            index = oldClass.GeneralizationsAsGeneral.IndexOf(psmGeneralization);
            Report = new CommandReport("{0} generalized from {1} to {2}.", psmGeneralization, oldClass, newClass);

            oldClass.GeneralizationsAsGeneral.Remove(psmGeneralization);
            psmGeneralization.General = newClass;
            newClass.GeneralizationsAsGeneral.Add(psmGeneralization);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PSMGeneralization psmGeneralization = Project.TranslateComponent<PSMGeneralization>(generalizationGuid);
            PSMClass oldClass = Project.TranslateComponent<PSMClass>(oldClassGuid);
            PSMClass newClass = oldClass.GeneralizationAsSpecific.General;

            newClass.GeneralizationsAsGeneral.Remove(psmGeneralization);
            psmGeneralization.General = oldClass;
            oldClass.GeneralizationsAsGeneral.Insert(psmGeneralization, index);
            return OperationResult.OK;
        }

        /*internal override PropagationMacroCommand PrePropagation()
        {
            
        }*/
    }
}
