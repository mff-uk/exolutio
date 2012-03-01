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
    internal class acmdUpdatePIMClassAbstract : AtomicCommand
    {
        Guid classGuid;
        bool newAbstract, oldAbstract;

        public acmdUpdatePIMClassAbstract(Controller c, Guid pimClassGuid, bool @abstract)
            : base(c)
        {
            classGuid = pimClassGuid;
            newAbstract = @abstract;
        }

        public override bool CanExecute()
        {
            if (classGuid == Guid.Empty || !Project.VerifyComponentType<PIMClass>(classGuid))
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }
            return true;
        }
        
        internal override void CommandOperation()
        {
            PIMClass pimClass = Project.TranslateComponent<PIMClass>(classGuid);
            oldAbstract = pimClass.Abstract;
            pimClass.Abstract = newAbstract;
            Report = new CommandReport(CommandReports.PIM_CLASS_ABSTRACT_CHANGED, pimClass, oldAbstract, pimClass.Abstract);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PIMClass pimClass = Project.TranslateComponent<PIMClass>(classGuid);
                
            pimClass.Abstract = oldAbstract;
            return OperationResult.OK;
        }

        internal override PropagationMacroCommand PrePropagation()
        {
            PIMClass pimClass = Project.TranslateComponent<PIMClass>(classGuid);
            List<PSMClass> list = pimClass.GetInterpretedComponents().Cast<PSMClass>().ToList<PSMClass>();
            if (list.Count == 0) return null;

            PropagationMacroCommand command = new PropagationMacroCommand(Controller);
            command.Report = new CommandReport("Pre-propagation (update PIM class abstract property)");

            foreach (PSMClass psmClass in list)
            {
                acmdUpdatePSMClassAbstract cmd = new acmdUpdatePSMClassAbstract(Controller, psmClass, newAbstract) { Propagate = false };
                command.Commands.Add(cmd);
            }

            return command;
        }

    }
}
