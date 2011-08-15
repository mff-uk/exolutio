using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM
{
    internal class acmdUpdatePSMAttributeType : StackedCommand
    {
        Guid attributeGuid, newTypeGuid, oldTypeGuid;
        public acmdUpdatePSMAttributeType(Controller c, Guid psmAttributeGuid, Guid typeGuid)
            : base(c)
        {
            attributeGuid = psmAttributeGuid;
            newTypeGuid = typeGuid;
        }

        public override bool CanExecute()
        {
            return attributeGuid != Guid.Empty
                && Project.VerifyComponentType<PSMAttribute>(attributeGuid);
        }
        
        internal override void CommandOperation()
        {
            PSMAttribute psmAttribute = Project.TranslateComponent<PSMAttribute>(attributeGuid);
            AttributeType type = psmAttribute.AttributeType;
            oldTypeGuid = psmAttribute.AttributeType;
            if (newTypeGuid != Guid.Empty)
                psmAttribute.AttributeType = Project.TranslateComponent<AttributeType>(newTypeGuid);
            else psmAttribute.AttributeType = null;
            Report = new CommandReport(CommandReports.TYPED_CHANGED, psmAttribute, type, psmAttribute.AttributeType);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PSMAttribute psmAttribute = Project.TranslateComponent<PSMAttribute>(attributeGuid);
            if (oldTypeGuid != Guid.Empty)
                psmAttribute.AttributeType = Project.TranslateComponent<AttributeType>(oldTypeGuid);
            else psmAttribute.AttributeType = null;
            return OperationResult.OK;
        }
    }
}
