using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Atomic.PSM;

namespace Exolutio.Controller.Commands.Atomic.PIM
{
    internal class acmdUpdatePIMClassFinal : AtomicCommand
    {
        Guid classGuid;
        bool newFinal, oldFinal;

        public acmdUpdatePIMClassFinal(Controller c, Guid pimClassGuid, bool final)
            : base(c)
        {
            classGuid = pimClassGuid;
            newFinal = final;
        }
        
        public override bool CanExecute()
        {
            if (classGuid == Guid.Empty)
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }
            PIMClass pimClass = Project.TranslateComponent<PIMClass>(classGuid);
            if (pimClass.GeneralizationsAsGeneral.Count > 0)
            {
                ErrorDescription = CommandErrors.CMDERR_CANNOT_SET_FINAL_GENERALIZATIIONS_EXIST;
                return false;
            }
            return true;
        }
        
        internal override void CommandOperation()
        {
            PIMClass pimClass = Project.TranslateComponent<PIMClass>(classGuid);
            oldFinal = pimClass.Final;
            pimClass.Final = newFinal;
            Report = new CommandReport(CommandReports.PIM_CLASS_FINAL_CHANGED, pimClass, oldFinal, pimClass.Final);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PIMClass pimClass = Project.TranslateComponent<PIMClass>(classGuid);
                
            pimClass.Final = oldFinal;
            return OperationResult.OK;
        }

        /*internal override PropagationMacroCommand PrePropagation()
        {
            List<PSMAttribute> list = Project.TranslateComponent<PIMAttribute>(attributeGuid).GetInterpretedComponents().Cast<PSMAttribute>().ToList<PSMAttribute>();
            if (list.Count == 0) return null;

            PropagationMacroCommand command = new PropagationMacroCommand(Controller);
            command.Report = new CommandReport("Pre-propagation (update PIM attribute type)");

            foreach (PSMAttribute a in list)
            {
                acmdUpdatePSMAttributeType d = new acmdUpdatePSMAttributeType(Controller, a, newTypeGuid) { Propagate = false };
                command.Commands.Add(d);
            }

            return command;
        }*/

    }
}
