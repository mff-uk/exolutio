using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Atomic.PSM;

namespace Exolutio.Controller.Commands.Atomic.PSM
{
    internal class acmdUpdatePSMClassAbstract : AtomicCommand
    {
        Guid classGuid;
        bool newAbstract, oldAbstract;

        public acmdUpdatePSMClassAbstract(Controller c, Guid psmClassGuid, bool @abstract)
            : base(c)
        {
            classGuid = psmClassGuid;
            newAbstract = @abstract;
        }

        public override bool CanExecute()
        {
            return classGuid != Guid.Empty
                && Project.VerifyComponentType<PSMClass>(classGuid);
        }
        
        internal override void CommandOperation()
        {
            PSMClass psmClass = Project.TranslateComponent<PSMClass>(classGuid);
            oldAbstract = psmClass.Abstract;
            psmClass.Abstract = newAbstract;
            Report = new CommandReport(CommandReports.PSM_CLASS_ABSTRACT_CHANGED, psmClass, oldAbstract, psmClass.Abstract);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PSMClass psmClass = Project.TranslateComponent<PSMClass>(classGuid);
                
            psmClass.Abstract = oldAbstract;
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
