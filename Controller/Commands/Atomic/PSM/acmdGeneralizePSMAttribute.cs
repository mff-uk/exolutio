using System;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM
{
    internal class acmdGeneralizePSMAttribute : AtomicCommand
    {
        Guid attributeGuid, oldClassGuid;
        int index;

        public acmdGeneralizePSMAttribute(Controller c, Guid psmAttributeGuid)
            : base(c)
        {
            attributeGuid = psmAttributeGuid;
        }

        public override bool CanExecute()
        {
            if (attributeGuid == Guid.Empty)
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }
            PSMAttribute attribute = Project.TranslateComponent<PSMAttribute>(attributeGuid);
            PSMClass oldclass = attribute.PSMClass;
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
            PSMAttribute psmAttribute = Project.TranslateComponent<PSMAttribute>(attributeGuid);
            PSMClass oldClass = psmAttribute.PSMClass;
            oldClassGuid = oldClass;
            PSMClass newClass = oldClass.GeneralizationAsSpecific.General;
            index = oldClass.PSMAttributes.IndexOf(psmAttribute);
            Report = new CommandReport("{0} generalized from {1} to {2}.", psmAttribute, oldClass, newClass);

            oldClass.PSMAttributes.Remove(psmAttribute);
            psmAttribute.PSMClass = newClass;
            newClass.PSMAttributes.Add(psmAttribute);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PSMAttribute psmAttribute = Project.TranslateComponent<PSMAttribute>(attributeGuid);
            PSMClass oldClass = Project.TranslateComponent<PSMClass>(oldClassGuid);
            PSMClass newClass = oldClass.GeneralizationAsSpecific.General;

            newClass.PSMAttributes.Remove(psmAttribute);
            psmAttribute.PSMClass = oldClass;
            oldClass.PSMAttributes.Insert(psmAttribute, index);
            return OperationResult.OK;
        }

        /*internal override PropagationMacroCommand PrePropagation()
        {
            
        }*/
    }
}
