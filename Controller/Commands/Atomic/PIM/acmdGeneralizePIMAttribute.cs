using System;
using Exolutio.Model.PIM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PIM
{
    internal class acmdGeneralizePIMAttribute : AtomicCommand
    {
        Guid attributeGuid, oldClassGuid;
        int index;

        public acmdGeneralizePIMAttribute(Controller c, Guid pimAttributeGuid)
            : base(c)
        {
            attributeGuid = pimAttributeGuid;
        }

        public override bool CanExecute()
        {
            if (attributeGuid == Guid.Empty)
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }
            PIMAttribute attribute = Project.TranslateComponent<PIMAttribute>(attributeGuid);
            PIMClass oldclass = attribute.PIMClass;
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
            PIMAttribute pimAttribute = Project.TranslateComponent<PIMAttribute>(attributeGuid);
            PIMClass oldClass = pimAttribute.PIMClass;
            oldClassGuid = oldClass;
            PIMClass newClass = oldClass.GeneralizationAsSpecific.General;
            index = oldClass.PIMAttributes.IndexOf(pimAttribute);
            Report = new CommandReport("{0} generalized from {1} to {2}.", pimAttribute, oldClass, newClass);

            oldClass.PIMAttributes.Remove(pimAttribute);
            pimAttribute.PIMClass = newClass;
            newClass.PIMAttributes.Add(pimAttribute);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PIMAttribute pimAttribute = Project.TranslateComponent<PIMAttribute>(attributeGuid);
            PIMClass oldClass = Project.TranslateComponent<PIMClass>(oldClassGuid);
            PIMClass newClass = oldClass.GeneralizationAsSpecific.General;

            newClass.PIMAttributes.Remove(pimAttribute);
            pimAttribute.PIMClass = oldClass;
            oldClass.PIMAttributes.Insert(pimAttribute, index);
            return OperationResult.OK;
        }

        /*internal override PropagationMacroCommand PrePropagation()
        {
            
        }*/
    }
}
