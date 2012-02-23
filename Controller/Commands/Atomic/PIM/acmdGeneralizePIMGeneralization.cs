using System;
using Exolutio.Model.PIM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PIM
{
    internal class acmdGeneralizePIMGeneralization : AtomicCommand
    {
        Guid generalizationGuid, oldClassGuid;
        int index;

        public acmdGeneralizePIMGeneralization(Controller c, Guid pimGeneralizationGuid)
            : base(c)
        {
            generalizationGuid = pimGeneralizationGuid;
        }

        public override bool CanExecute()
        {
            if (generalizationGuid == Guid.Empty)
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }
            PIMGeneralization generalization = Project.TranslateComponent<PIMGeneralization>(generalizationGuid);
            PIMClass oldclass = generalization.General;
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
            PIMGeneralization pimGeneralization = Project.TranslateComponent<PIMGeneralization>(generalizationGuid);
            PIMClass oldClass = pimGeneralization.General;
            oldClassGuid = oldClass;
            PIMClass newClass = oldClass.GeneralizationAsSpecific.General;
            index = oldClass.GeneralizationsAsGeneral.IndexOf(pimGeneralization);
            Report = new CommandReport("{0} generalized from {1} to {2}.", pimGeneralization, oldClass, newClass);

            oldClass.GeneralizationsAsGeneral.Remove(pimGeneralization);
            pimGeneralization.General = newClass;
            newClass.GeneralizationsAsGeneral.Add(pimGeneralization);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PIMGeneralization pimGeneralization = Project.TranslateComponent<PIMGeneralization>(generalizationGuid);
            PIMClass oldClass = Project.TranslateComponent<PIMClass>(oldClassGuid);
            PIMClass newClass = oldClass.GeneralizationAsSpecific.General;

            newClass.GeneralizationsAsGeneral.Remove(pimGeneralization);
            pimGeneralization.General = oldClass;
            oldClass.GeneralizationsAsGeneral.Insert(pimGeneralization, index);
            return OperationResult.OK;
        }

        /*internal override PropagationMacroCommand PrePropagation()
        {
            
        }*/
    }
}
