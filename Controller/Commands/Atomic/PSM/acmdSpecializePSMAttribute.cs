using System;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM
{
    internal class acmdSpecializePSMAttribute : AtomicCommand
    {
        Guid attributeGuid, generalClassGuid, specialClassGuid;
        int index;

        public acmdSpecializePSMAttribute(Controller c, Guid psmAttributeGuid, Guid specialGuid)
            : base(c)
        {
            attributeGuid = psmAttributeGuid;
            specialClassGuid = specialGuid;
        }

        public override bool CanExecute()
        {
            if (attributeGuid == Guid.Empty || specialClassGuid == Guid.Empty)
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }
            PSMAttribute attribute = Project.TranslateComponent<PSMAttribute>(attributeGuid);
            PSMClass oldclass = attribute.PSMClass;
            PSMClass newclass = Project.TranslateComponent<PSMClass>(specialClassGuid);
            if (newclass.GeneralizationAsSpecific == null || newclass.GeneralizationAsSpecific.General != oldclass)
            {
                ErrorDescription = CommandErrors.CMDERR_INVALID_SPECIALIZATION;
                return false;
            }
            return true;
        }
        
        internal override void CommandOperation()
        {
            PSMAttribute pimAttribute = Project.TranslateComponent<PSMAttribute>(attributeGuid);
            PSMClass generalClass = pimAttribute.PSMClass;
            generalClassGuid = generalClass;
            PSMClass specialClass = Project.TranslateComponent<PSMClass>(specialClassGuid);
            index = generalClass.PSMAttributes.IndexOf(pimAttribute);
            Report = new CommandReport("{0} specialized from {1} to {2}.", pimAttribute, generalClass, specialClass);

            generalClass.PSMAttributes.Remove(pimAttribute);
            pimAttribute.PSMClass = specialClass;
            specialClass.PSMAttributes.Add(pimAttribute);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PSMAttribute pimAttribute = Project.TranslateComponent<PSMAttribute>(attributeGuid);
            PSMClass generalClass = Project.TranslateComponent<PSMClass>(generalClassGuid);
            PSMClass specialClass = Project.TranslateComponent<PSMClass>(specialClassGuid);

            specialClass.PSMAttributes.Remove(pimAttribute);
            pimAttribute.PSMClass = generalClass;
            generalClass.PSMAttributes.Insert(pimAttribute, index);
            return OperationResult.OK;
        }

        /*internal override PropagationMacroCommand PrePropagation()
        {
            
        }*/
    }
}
