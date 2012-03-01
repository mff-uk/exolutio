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
            List<PSMClass> psmSpecificClasses = pimSpecificClass.GetInterpretedComponents().Cast<PSMClass>().ToList();
            if (psmSpecificClasses.Count == 0) return null;

            PropagationMacroCommand command = new PropagationMacroCommand(Controller) { CheckFirstOnlyInCanExecute = true };
            command.Report = new CommandReport("Pre-propagation (generalize PIM generalization)");

            foreach (PSMClass c in psmSpecificClasses)
            {
                //TODO: parent association (just check it...), it should not be involved...
                Debug.Assert(c.ParentAssociation == null || c.ParentAssociation.Interpretation == null || c.Interpretation == (c.ParentAssociation.Interpretation as PIMAssociation)
                                                              .PIMAssociationEnds
                                                              .Single(e => e != c.ParentAssociation.InterpretedAssociationEnd));

                List<PSMAttribute> attributesLost = c.GetContextPSMAttributes(true).Where
                    (a => a.Interpretation != null && (a.Interpretation as PIMAttribute).PIMClass == (c.Interpretation as PIMClass).GeneralizationAsSpecific.General).ToList();

                List<PSMAssociation> associationsLost = c.GetContextPSMAssociations(true).Where
                    (a => a.Interpretation != null && a.InterpretedAssociationEnd.PIMClass == (c.Interpretation as PIMClass).GeneralizationAsSpecific.General).ToList();

                if (c.GeneralizationAsSpecific != null)
                {
                    bool moveToNewClass = attributesLost.Count > 0 || associationsLost.Count > 0;

                    PSMClass newGeneralPSMClass = c.GeneralizationAsSpecific.General.GeneralizationAsSpecific == null ? null : c.GeneralizationAsSpecific.General.GeneralizationAsSpecific.General;

                    Guid newClassGuid;
                    if (moveToNewClass)
                    {
                        //is there the class to move to?
                        if (c.GeneralizationAsSpecific.General.Interpretation != pimGeneralization.General)
                        {
                            newClassGuid = Guid.NewGuid();
                            Guid newGeneralizationGuid = Guid.NewGuid();
                            PIMClass pimGeneralClass = pimSpecificClass.GeneralizationAsSpecific.General;
                            command.Commands.Add(new acmdNewPSMClass(Controller, c.Schema) { ClassGuid = newClassGuid });
                            command.Commands.Add(new acmdRenameComponent(Controller, newClassGuid, pimGeneralClass.Name));
                            command.Commands.Add(new acmdSetPSMClassInterpretation(Controller, newClassGuid, pimGeneralClass));
                            command.Commands.Add(new acmdNewPSMGeneralization(Controller, c.GeneralizationAsSpecific.General, newClassGuid, c.Schema) { GeneralizationGuid = newGeneralizationGuid });
                            command.Commands.Add(new acmdSpecializePSMGeneralization(Controller, c.GeneralizationAsSpecific, newClassGuid));
                        }

                        //TODO: Now this has to be generalized for the whole intcontext....
                        foreach (PSMAttribute att in attributesLost)
                        {
                            if (att.PSMClass != c) command.Commands.Add(new cmdMovePSMAttribute(Controller) { AttributeGuid = att, ClassGuid = c });
                            command.Commands.Add(new acmdGeneralizePSMAttribute(Controller, att));
                        }

                        foreach (PSMAssociation assoc in associationsLost)
                        {
                            if (assoc.Parent != c) command.Commands.Add(new cmdReconnectPSMAssociation(Controller) { AssociationGuid = assoc, NewParentGuid = c });
                            command.Commands.Add(new acmdGeneralizePSMAssociation(Controller, assoc));

                        }
                    }
                    if (newGeneralPSMClass != null)
                    {
                        command.Commands.Add(new acmdGeneralizePSMGeneralization(Controller, c.GeneralizationAsSpecific));
                    }
                    else
                    {
                        command.Commands.Add(new acmdDeletePSMGeneralization(Controller, c.GeneralizationAsSpecific));
                    }
                }   
                else
                {
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

            }

            return command;
        }
    }
}
