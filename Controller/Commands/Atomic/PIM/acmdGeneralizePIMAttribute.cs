using System;
using Exolutio.Model.PIM;
using Exolutio.Model;
using Exolutio.Model.PSM;
using System.Collections.Generic;
using System.Linq;
using Exolutio.Controller.Commands.Complex.PSM;

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

        internal override PropagationMacroCommand PrePropagation()
        {
            PIMAttribute pimAttribute = Project.TranslateComponent<PIMAttribute>(attributeGuid);
            PIMClass oldPIMClass = pimAttribute.PIMClass;
            PIMClass newPIMClass = oldPIMClass.GeneralizationAsSpecific.General;
            List<PSMAttribute> psmAttributes = pimAttribute.GetInterpretedComponents().Cast<PSMAttribute>().ToList();

            PropagationMacroCommand command = new PropagationMacroCommand(Controller) { CheckFirstOnlyInCanExecute = true };
            command.Report = new CommandReport("Pre-propagation (generalize PIM attribute)");

            foreach (PSMAttribute a in psmAttributes)
            {
                IEnumerable<PSMClass> generals = a.PSMClass.NearestInterpretedClass().GetGeneralClasses();
                if (generals.Any(c => c.Interpretation == newPIMClass))
                {
                    if (a.PSMClass.Interpretation == null)
                    {
                        //classa neinterpret, mozna i s impl.inheritance
                        command.Commands.Add(new cmdMovePSMAttribute(Controller) { AttributeGuid = a, ClassGuid = a.PSMClass.NearestInterpretedClass() });
                    }

                    //whether the class uses implicit inheritance or not (a.PSMClass.Interpretation == oldPIMClass) we move it to the PSMClass, whose interpretation is newPIMClass
                    command.Commands.Add(new cmdGeneralizePSMAttribute(Controller) { AttributeGuid = a, PSMClassGuid = generals.First(c => c.Interpretation == newPIMClass) });
                }
            }

            return command;            
        }
    }
}
