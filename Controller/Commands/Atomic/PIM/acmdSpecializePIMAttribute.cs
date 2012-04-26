using System;
using Exolutio.Model.PIM;
using Exolutio.Model;
using System.Collections.Generic;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Complex.PSM;
using System.Linq;
using Exolutio.Controller.Commands.Atomic.PSM;

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

        internal override PropagationMacroCommand PrePropagation()
        {
            PIMAttribute pimAttribute = Project.TranslateComponent<PIMAttribute>(attributeGuid);
            PIMClass specialPIMClass = Project.TranslateComponent<PIMClass>(specialClassGuid);
            PIMClass generalPIMClass = pimAttribute.PIMClass;
            List<PSMAttribute> psmAttributes = pimAttribute.GetInterpretedComponents().Cast<PSMAttribute>().ToList();

            PropagationMacroCommand command = new PropagationMacroCommand(Controller) { CheckFirstOnlyInCanExecute = true };
            command.Report = new CommandReport("Pre-propagation (specialize PIM attribute)");

            foreach (PSMAttribute a in psmAttributes)
            {
                IEnumerable<Tuple<PSMClass, IEnumerable<PSMClass>>> paths = a.PSMClass.GetSpecialClassesWithPaths();
                if (paths.Any(p => p.Item1.Interpretation == specialPIMClass))
                //1) there is special PIMClass counterpart -> move there
                {
                    IEnumerable<PSMClass> path = paths.First(p => p.Item1.Interpretation == specialPIMClass).Item2;
                    foreach (PSMClass c in path)
                    {
                        command.Commands.Add(new acmdSpecializePSMAttribute(Controller, a, c));
                    }
                }
                else
                //2) There is none... create. TODO: FIX: multiple attributes in one class => multiple classes
                {
                    Guid newClassGuid = Guid.NewGuid();
                    command.Commands.Add(new acmdNewPSMClass(Controller, a.PSMSchema) { ClassGuid = newClassGuid });
                    command.Commands.Add(new acmdRenameComponent(Controller, newClassGuid, specialPIMClass.Name));
                    command.Commands.Add(new acmdSetPSMClassInterpretation(Controller, newClassGuid, specialPIMClass));
                    command.Commands.Add(new acmdNewPSMGeneralization(Controller, a.PSMClass, newClassGuid, a.PSMSchema));
                    command.Commands.Add(new acmdSpecializePSMAttribute(Controller, a, newClassGuid));
                }
            }

            return command;  
        }
    }
}
