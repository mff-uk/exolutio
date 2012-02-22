using System;
using Exolutio.Model.PIM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PIM
{
    internal class acmdSpecializePIMAttribute : AtomicCommand
    {
        Guid attributeGuid, generalClassGuid, specialClassGuid;
        int index;

        public acmdSpecializePIMAttribute(Controller c, Guid pimAttributeGuid, Guid specialGuid)
            : base(c)
        {
            attributeGuid = pimAttributeGuid;
            specialClassGuid = specialGuid;
        }

        public override bool CanExecute()
        {
            if (attributeGuid == Guid.Empty || specialClassGuid == Guid.Empty)
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }
            PIMAttribute attribute = Project.TranslateComponent<PIMAttribute>(attributeGuid);
            PIMClass oldclass = attribute.PIMClass;
            PIMClass newclass = Project.TranslateComponent<PIMClass>(specialClassGuid);
            if (newclass.GeneralizationAsSpecific == null || newclass.GeneralizationAsSpecific.General != oldclass)
            {
                ErrorDescription = CommandErrors.CMDERR_INVALID_SPECIALIZATION;
                return false;
            }
            return true;
        }
        
        internal override void CommandOperation()
        {
            PIMAttribute pimAttribute = Project.TranslateComponent<PIMAttribute>(attributeGuid);
            PIMClass generalClass = pimAttribute.PIMClass;
            generalClassGuid = generalClass;
            PIMClass specialClass = Project.TranslateComponent<PIMClass>(specialClassGuid);
            index = generalClass.PIMAttributes.IndexOf(pimAttribute);
            Report = new CommandReport("{0} specialized from {1} to {2}.", pimAttribute, generalClass, specialClass);

            generalClass.PIMAttributes.Remove(pimAttribute);
            pimAttribute.PIMClass = specialClass;
            specialClass.PIMAttributes.Add(pimAttribute);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PIMAttribute pimAttribute = Project.TranslateComponent<PIMAttribute>(attributeGuid);
            PIMClass generalClass = Project.TranslateComponent<PIMClass>(generalClassGuid);
            PIMClass specialClass = Project.TranslateComponent<PIMClass>(specialClassGuid);

            specialClass.PIMAttributes.Remove(pimAttribute);
            pimAttribute.PIMClass = generalClass;
            generalClass.PIMAttributes.Insert(pimAttribute, index);
            return OperationResult.OK;
        }

        /*internal override PropagationMacroCommand PrePropagation()
        {
            
        }*/
    }
}
