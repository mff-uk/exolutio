using System;
using Exolutio.Model.PSM;

namespace Exolutio.Controller.Commands.Atomic.PSM
{
    internal class acmdUpdatePSMAttributeDefaultValue : AtomicCommand
    {
        Guid attributeGuid;
        string newDefaultValue, oldDefaultValue;

        public acmdUpdatePSMAttributeDefaultValue(Controller c, Guid psmAttributeGuid, string defaultValue)
            : base(c)
        {
            attributeGuid = psmAttributeGuid;
            newDefaultValue = defaultValue;
        }

        public override bool CanExecute()
        {
            return attributeGuid != Guid.Empty
                   && Project.VerifyComponentType<PSMAttribute>(attributeGuid);
        }

        internal override void CommandOperation()
        {
            PSMAttribute psmAttribute = Project.TranslateComponent<PSMAttribute>(attributeGuid);
            oldDefaultValue = psmAttribute.DefaultValue;
            psmAttribute.DefaultValue = newDefaultValue;

            Report = new CommandReport("Default value of '{0}' changed from '{1}' to '{2}'.", psmAttribute, oldDefaultValue, newDefaultValue);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PSMAttribute psmAttribute = Project.TranslateComponent<PSMAttribute>(attributeGuid);
            psmAttribute.DefaultValue = oldDefaultValue;
            return OperationResult.OK;
        }
    }
}