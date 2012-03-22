using System;
using Exolutio.Model.PIM;
using Exolutio.Model;
using System.Linq;
using Exolutio.Model.PSM;
using System.Collections.Generic;
using Exolutio.Controller.Commands.Complex.PSM;
using Exolutio.Controller.Commands.Atomic.PSM;

namespace Exolutio.Controller.Commands.Atomic.PIM
{
    internal class acmdGeneralizePIMAssociationEnd : AtomicCommand
    {
        Guid associationEndGuid, oldClassGuid;
        int index;

        public acmdGeneralizePIMAssociationEnd(Controller c, Guid pimAttributeGuid)
            : base(c)
        {
            associationEndGuid = pimAttributeGuid;
        }

        public override bool CanExecute()
        {
            if (associationEndGuid == Guid.Empty)
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }
            PIMAssociationEnd associationEnd = Project.TranslateComponent<PIMAssociationEnd>(associationEndGuid);
            PIMClass oldclass = associationEnd.PIMClass;
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
            PIMAssociationEnd pimAssociationEnd = Project.TranslateComponent<PIMAssociationEnd>(associationEndGuid);
            PIMClass oldClass = pimAssociationEnd.PIMClass;
            oldClassGuid = oldClass;
            PIMClass newClass = oldClass.GeneralizationAsSpecific.General;
            index = oldClass.PIMAssociationEnds.IndexOf(pimAssociationEnd);
            Report = new CommandReport("{0} generalized from {1} to {2}.", pimAssociationEnd, oldClass, newClass);

            oldClass.PIMAssociationEnds.Remove(pimAssociationEnd);
            pimAssociationEnd.PIMClass = newClass;
            newClass.PIMAssociationEnds.Add(pimAssociationEnd);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PIMAssociationEnd pimAssociationEnd = Project.TranslateComponent<PIMAssociationEnd>(associationEndGuid);
            PIMClass oldClass = Project.TranslateComponent<PIMClass>(oldClassGuid);
            PIMClass newClass = oldClass.GeneralizationAsSpecific.General;

            newClass.PIMAssociationEnds.Remove(pimAssociationEnd);
            pimAssociationEnd.PIMClass = oldClass;
            oldClass.PIMAssociationEnds.Insert(pimAssociationEnd, index);
            return OperationResult.OK;
        }

        internal override PropagationMacroCommand PrePropagation()
        {
            PIMAssociationEnd pimAssociationEnd = Project.TranslateComponent<PIMAssociationEnd>(associationEndGuid);
            PIMClass oldPIMClass = pimAssociationEnd.PIMClass;
            PIMClass newPIMClass = oldPIMClass.GeneralizationAsSpecific.General;
            
            //TODO: what about psm associations in the other direction? Create inheritance?
            IEnumerable<PSMAssociation> psmAssociations = pimAssociationEnd.PIMAssociation.GetInterpretedComponents().Cast<PSMAssociation>();
            List<PSMAssociation> psmAssociationsNormalDirection = psmAssociations.Where(a => a.InterpretedAssociationEnd != pimAssociationEnd).ToList();
            List<PSMAssociation> psmAssociationsOppositeDirection = psmAssociations.Where(a => a.InterpretedAssociationEnd == pimAssociationEnd).ToList();

            PropagationMacroCommand command = new PropagationMacroCommand(Controller) { CheckFirstOnlyInCanExecute = true };
            command.Report = new CommandReport("Pre-propagation (generalize PIM association end)");

            foreach (PSMAssociation a in psmAssociationsNormalDirection)
            {
                IEnumerable<PSMClass> generals = a.Parent.NearestInterpretedClass().GetGeneralClasses();
                if (generals.Any(c => c.Interpretation == newPIMClass))
                {
                    if (a.Parent.Interpretation == null)
                    {
                        //classa neinterpret, mozna i s impl.inheritance
                        command.Commands.Add(new cmdReconnectPSMAssociation(Controller) { AssociationGuid = a, NewParentGuid = a.Parent.NearestInterpretedClass() });
                    }

                    //whether the class uses implicit inheritance or not (a.PSMClass.Interpretation == oldPIMClass) we move it to the PSMClass, whose interpretation is newPIMClass
                    command.Commands.Add(new cmdGeneralizePSMAssociation(Controller) { AssociationGuid = a, PSMClassGuid = generals.First(c => c.Interpretation == newPIMClass) });
                }
            }
            
            foreach (PSMAssociation a in psmAssociationsOppositeDirection)
            {
                Guid newPSMClassGuid = Guid.NewGuid();
                Guid newPSMAssociationGuid = Guid.NewGuid();

                command.Commands.Add(new acmdNewPSMClass(Controller, a.PSMSchema) { ClassGuid = newPSMClassGuid });
                command.Commands.Add(new acmdRenameComponent(Controller, newPSMClassGuid, newPIMClass.Name));
                command.Commands.Add(new acmdSetPSMClassInterpretation(Controller, newPSMClassGuid, newPIMClass));
                command.Commands.Add(new acmdNewPSMAssociation(Controller, a.Parent, newPSMClassGuid, a.PSMSchema) { AssociationGuid = newPSMAssociationGuid });
                command.Commands.Add(new acmdSetPSMAssociationInterpretation(Controller, newPSMAssociationGuid, a.InterpretedAssociationEnd, a.Interpretation) { ForceExecute = true });
                                
                command.Commands.Add(new cmdUpdatePSMAssociation(Controller) { AssociationGuid = newPSMAssociationGuid, Name = a.Name, Lower = a.Lower, Upper = a.Upper });
                command.Commands.Add(new cmdDeletePSMAssociation(Controller) { AssociationGuid = a });
                if ((a.Child as PSMClass).GeneralizationAsSpecific != null)
                {
                    command.Commands.Add(new acmdNewPSMGeneralization(Controller, (a.Child as PSMClass).GeneralizationAsSpecific.General, newPSMClassGuid, a.PSMSchema));
                    command.Commands.Add(new acmdDeletePSMGeneralization(Controller, (a.Child as PSMClass).GeneralizationAsSpecific));
                }
                command.Commands.Add(new acmdNewPSMGeneralization(Controller, newPSMClassGuid, a.Child, a.PSMSchema));
            }

            return command;              
        }
    }
}
