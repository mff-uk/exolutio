using System;
using Exolutio.Model.PIM;
using Exolutio.Model;
using Exolutio.Model.PSM;
using System.Collections.Generic;
using System.Linq;
using Exolutio.Controller.Commands.Complex.PSM;
using Exolutio.Controller.Commands.Atomic.PSM;

namespace Exolutio.Controller.Commands.Atomic.PIM
{
    internal class acmdSpecializePIMAssociationEnd : AtomicCommand
    {
        Guid associationEndGuid, generalClassGuid, specialClassGuid;
        int index;

        public acmdSpecializePIMAssociationEnd(Controller c, Guid pimAssociationEndGuid, Guid specialGuid)
            : base(c)
        {
            associationEndGuid = pimAssociationEndGuid;
            specialClassGuid = specialGuid;
        }

        public override bool CanExecute()
        {
            if (associationEndGuid == Guid.Empty || specialClassGuid == Guid.Empty)
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }
            PIMAssociationEnd associationEnd = Project.TranslateComponent<PIMAssociationEnd>(associationEndGuid);
            PIMClass oldclass = associationEnd.PIMClass;
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
            PIMAssociationEnd pimAssociationEnd = Project.TranslateComponent<PIMAssociationEnd>(associationEndGuid);
            PIMClass generalClass = pimAssociationEnd.PIMClass;
            generalClassGuid = generalClass;
            PIMClass specialClass = Project.TranslateComponent<PIMClass>(specialClassGuid);
            index = generalClass.PIMAssociationEnds.IndexOf(pimAssociationEnd);
            Report = new CommandReport("{0} specialized from {1} to {2}.", pimAssociationEnd, generalClass, specialClass);

            generalClass.PIMAssociationEnds.Remove(pimAssociationEnd);
            pimAssociationEnd.PIMClass = specialClass;
            specialClass.PIMAssociationEnds.Add(pimAssociationEnd);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PIMAssociationEnd pimAssociationEnd = Project.TranslateComponent<PIMAssociationEnd>(associationEndGuid);
            PIMClass generalClass = Project.TranslateComponent<PIMClass>(generalClassGuid);
            PIMClass specialClass = Project.TranslateComponent<PIMClass>(specialClassGuid);

            specialClass.PIMAssociationEnds.Remove(pimAssociationEnd);
            pimAssociationEnd.PIMClass = generalClass;
            generalClass.PIMAssociationEnds.Insert(pimAssociationEnd, index);
            return OperationResult.OK;
        }

        internal override PropagationMacroCommand PrePropagation()
        {
            PIMAssociationEnd pimAssociationEnd = Project.TranslateComponent<PIMAssociationEnd>(associationEndGuid);
            PIMClass generalPIMClass = pimAssociationEnd.PIMClass;
            PIMClass specialPIMClass = Project.TranslateComponent<PIMClass>(specialClassGuid);

            IEnumerable<PSMAssociation> psmAssociations = pimAssociationEnd.PIMAssociation.GetInterpretedComponents().Cast<PSMAssociation>();
            List<PSMAssociation> psmAssociationsNormalDirection = psmAssociations.Where(a => a.InterpretedAssociationEnd != pimAssociationEnd).ToList();
            List<PSMAssociation> psmAssociationsOppositeDirection = psmAssociations.Where(a => a.InterpretedAssociationEnd == pimAssociationEnd).ToList();

            PropagationMacroCommand command = new PropagationMacroCommand(Controller) { CheckFirstOnlyInCanExecute = true };
            command.Report = new CommandReport("Pre-propagation (specialize PIM association end)");

            foreach (PSMAssociation a in psmAssociationsNormalDirection)
            {
                if (a.Parent.Interpretation == null)
                {
                    //class without interpretation, maybe including impl.inheritance
                    command.Commands.Add(new cmdReconnectPSMAssociation(Controller) { AssociationGuid = a, NewParentGuid = a.Parent.NearestInterpretedClass() });
                }

                IEnumerable<Tuple<PSMClass, IEnumerable<PSMClass>>> paths = a.Parent.NearestInterpretedClass().GetSpecialClassesWithPaths();
                if (paths.Any(p => p.Item1.Interpretation == specialPIMClass))
                //1) there is special PIMClass counterpart -> move there
                {
                    IEnumerable<PSMClass> path = paths.First(p => p.Item1.Interpretation == specialPIMClass).Item2;
                    foreach (PSMClass c in path)
                    {
                        command.Commands.Add(new acmdSpecializePSMAssociation(Controller, a, c));
                    }
                }
                else
                //2) There is none... create. TODO: FIX: multiple attributes in one class => multiple classes
                {
                    Guid newClassGuid = Guid.NewGuid();
                    command.Commands.Add(new acmdNewPSMClass(Controller, a.PSMSchema) { ClassGuid = newClassGuid });
                    command.Commands.Add(new acmdRenameComponent(Controller, newClassGuid, specialPIMClass.Name));
                    command.Commands.Add(new acmdSetPSMClassInterpretation(Controller, newClassGuid, specialPIMClass));
                    command.Commands.Add(new acmdNewPSMGeneralization(Controller, a.Parent.NearestInterpretedClass(), newClassGuid, a.PSMSchema));
                    command.Commands.Add(new acmdSpecializePSMAssociation(Controller, a, newClassGuid));
                }
            }

            foreach (PSMAssociation a in psmAssociationsOppositeDirection)
            {
                command.Commands.Add(new cmdDeletePSMAssociation(Controller) { AssociationGuid = a });
            }

            return command;                 
        }
    }
}
