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
    public class acmdUpdatePIMAttributeType : StackedCommand
    {
        Guid attributeGuid, newTypeGuid, oldTypeGuid;

        public acmdUpdatePIMAttributeType(Controller c, Guid pimAttributeGuid, Guid typeGuid)
            : base(c)
        {
            attributeGuid = pimAttributeGuid;
            newTypeGuid = typeGuid;
        }

        public override bool CanExecute()
        {
            return attributeGuid != Guid.Empty
                && Project.VerifyComponentType<PIMAttribute>(attributeGuid);
        }
        
        internal override void CommandOperation()
        {
            PIMAttribute pimAttribute = Project.TranslateComponent<PIMAttribute>(attributeGuid);
            AttributeType oldType = pimAttribute.AttributeType;
            oldTypeGuid = oldType;
            if (newTypeGuid != Guid.Empty)
                pimAttribute.AttributeType = Project.TranslateComponent<AttributeType>(newTypeGuid);
            else pimAttribute.AttributeType = null;
            Report = new CommandReport(CommandReports.TYPED_CHANGED, pimAttribute, oldType, pimAttribute.AttributeType);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PIMAttribute pimAttribute = Project.TranslateComponent<PIMAttribute>(attributeGuid);
                
            pimAttribute.AttributeType = oldTypeGuid == Guid.Empty ? null : Project.TranslateComponent<AttributeType>(oldTypeGuid);
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
