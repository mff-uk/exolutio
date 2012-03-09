using System;
using Exolutio.Model.PIM;
using Exolutio.Model;
using Exolutio.Model.PSM;
using System.Collections.Generic;
using System.Linq;
using Exolutio.Controller.Commands.Atomic.PSM;
using System.Diagnostics;
using Exolutio.Controller.Commands.Complex.PSM;

namespace Exolutio.Controller.Commands.Atomic.PIM
{
    internal class acmdGeneralizePIMGeneralization : AtomicCommand
    {
        Guid generalizationGuid, oldClassGuid;
        int index;

        public acmdGeneralizePIMGeneralization(Controller c, Guid pimGeneralizationGuid)
            : base(c)
        {
            generalizationGuid = pimGeneralizationGuid;
        }

        public override bool CanExecute()
        {
            if (generalizationGuid == Guid.Empty)
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }
            PIMGeneralization generalization = Project.TranslateComponent<PIMGeneralization>(generalizationGuid);
            PIMClass oldclass = generalization.General;
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
            PIMGeneralization pimGeneralization = Project.TranslateComponent<PIMGeneralization>(generalizationGuid);
            PIMClass oldClass = pimGeneralization.General;
            oldClassGuid = oldClass;
            PIMClass newClass = oldClass.GeneralizationAsSpecific.General;
            index = oldClass.GeneralizationsAsGeneral.IndexOf(pimGeneralization);
            Report = new CommandReport("{0} generalized from {1} to {2}.", pimGeneralization, oldClass, newClass);

            oldClass.GeneralizationsAsGeneral.Remove(pimGeneralization);
            pimGeneralization.General = newClass;
            newClass.GeneralizationsAsGeneral.Add(pimGeneralization);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PIMGeneralization pimGeneralization = Project.TranslateComponent<PIMGeneralization>(generalizationGuid);
            PIMClass oldClass = Project.TranslateComponent<PIMClass>(oldClassGuid);
            PIMClass newClass = oldClass.GeneralizationAsSpecific.General;

            newClass.GeneralizationsAsGeneral.Remove(pimGeneralization);
            pimGeneralization.General = oldClass;
            oldClass.GeneralizationsAsGeneral.Insert(pimGeneralization, index);
            return OperationResult.OK;
        }

        internal override PropagationMacroCommand PrePropagation()
        {
            PIMGeneralization pimGeneralization = Project.TranslateComponent<PIMGeneralization>(generalizationGuid);
            PIMClass pimSpecificClass = pimGeneralization.Specific;
            PIMClass oldPIMGeneral = pimGeneralization.General;
            PIMClass newPIMGeneral = oldPIMGeneral.GeneralizationAsSpecific.General;
            List<PSMClass> classesToCheck = pimSpecificClass.GetSpecificClasses(true).SelectMany(c => c.GetInterpretedComponents().Cast<PSMClass>()).ToList();
            List<PSMClass> psmSpecificClasses = pimSpecificClass.GetInterpretedComponents().Cast<PSMClass>().ToList();

            /* ALTERNATIVE SOLUTION - TO BE THOUGHT THROUGH - MOVE AS MANY ATTS AND ASSOCS AS POSSIBLE - DOES IT MAKE SENSE?
             * What about those in inheritance subtree??? Must go through every PSM class that has interpretation in the inheritance hierarchy subtree
             * 
             * 
             * 1) Is there a need to move atts and assocs? No - OK
             *  1a) Yes - where? Is there the "old general class"?
             *   1aa) Yes - There
             *   1ab) No - Is there superparent?
             *    1aba) Yes - Create old parent and there and specialize the subtree under it
             *    1abb) No - create parent, move "bad" atts and assocs, delete generalization.
             * 2) Now assocs and attrs are OK. Has inh. parent same interpretation or is there no generalization? Yes - nothing
             *  2a) No - is it old parent? Yes - generalize
             *   2aa) No - Nothing - the generalization does not affect the current PSM class.
             */

            PropagationMacroCommand command = new PropagationMacroCommand(Controller) { CheckFirstOnlyInCanExecute = true };
            command.Report = new CommandReport("Pre-propagation (generalize PIM generalization)");

            //DELETE ATTS AND ASSOCS THAT VIOLATE INTERPRETATION AFTER GEN PIM GEN
            foreach (PSMClass c in classesToCheck)
            {
                //parent association (just check it...), it should not be involved...
                Debug.Assert(c.ParentAssociation == null || c.ParentAssociation.Interpretation == null || c.Interpretation == (c.ParentAssociation.Interpretation as PIMAssociation)
                                                              .PIMAssociationEnds
                                                              .Single(e => e != c.ParentAssociation.InterpretedAssociationEnd));

                List<PSMAttribute> attributesLost = c.GetContextPSMAttributes(true).Where
                   (a => a.Interpretation != null && (a.Interpretation as PIMAttribute).PIMClass == pimGeneralization.General).ToList();

                List<PSMAssociation> associationsLost = c.GetContextPSMAssociations(true).Where
                    (a => a.Interpretation != null && a.InterpretedAssociationEnd.PIMClass == pimGeneralization.General).ToList();

                foreach (PSMAttribute att in attributesLost)
                {
                    if (att.PSMClass != c) command.Commands.Add(new cmdMovePSMAttribute(Controller) { AttributeGuid = att, ClassGuid = c });
                    command.Commands.Add(new cmdDeletePSMAttribute(Controller) { AttributeGuid = att });
                }

                foreach (PSMAssociation assoc in associationsLost)
                {
                    if (assoc.Parent != c) command.Commands.Add(new cmdReconnectPSMAssociation(Controller) { AssociationGuid = assoc, NewParentGuid = c });
                    command.Commands.Add(new cmdDeletePSMAssociation(Controller) { AssociationGuid = assoc });

                }                
            }
            
            foreach (PSMClass c in psmSpecificClasses)
            {
                if (c.GeneralizationAsSpecific != null && c.GeneralizationAsSpecific.General.Interpretation != c.Interpretation)
                {
                    if (c.GeneralizationAsSpecific.General.Interpretation == oldPIMGeneral)
                    {
                        command.Commands.Add(new acmdGeneralizePSMGeneralization(Controller, c.GeneralizationAsSpecific) { Propagate = false });
                    }
                }
            }

            return command;
        }
    }
}
