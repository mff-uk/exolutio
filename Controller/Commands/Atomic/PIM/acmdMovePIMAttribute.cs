using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Model;
using Exolutio.Model.PSM;
using System.Diagnostics;
using Exolutio.Controller.Commands.Complex.PSM;
using Exolutio.Controller.Commands.Atomic.PSM;

namespace Exolutio.Controller.Commands.Atomic.PIM
{
    public class acmdMovePIMAttribute : StackedCommand
    {
        Guid attributeGuid, newClassGuid, oldClassGuid;
        int index;

        public acmdMovePIMAttribute(Controller c, Guid pimAttributeGuid, Guid pimClassGuid)
            : base(c)
        {
            newClassGuid = pimClassGuid;
            attributeGuid = pimAttributeGuid;
        }

        public override bool CanExecute()
        {
            if (newClassGuid == Guid.Empty || attributeGuid == Guid.Empty)
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }
            PIMClass newclass = Project.TranslateComponent<PIMClass>(newClassGuid);
            PIMAttribute attribute = Project.TranslateComponent<PIMAttribute>(attributeGuid);
            PIMClass oldclass = attribute.PIMClass;
            if (newclass.GetAssociationsWith(oldclass).Count<PIMAssociation>() == 0)
            {
                ErrorDescription = CommandErrors.CMDERR_NO_COMMON_ASSOCIATION;
                return false;
            }
            return true;
        }
        
        internal override void CommandOperation()
        {
            PIMAttribute pimAttribute = Project.TranslateComponent<PIMAttribute>(attributeGuid);
            PIMClass oldClass = pimAttribute.PIMClass;
            oldClassGuid = oldClass;
            PIMClass newClass = Project.TranslateComponent<PIMClass>(newClassGuid);
            index = oldClass.PIMAttributes.IndexOf(pimAttribute);
            Report = new CommandReport("{0} moved from {1} to {2}.", pimAttribute, oldClass, newClass);

            oldClass.PIMAttributes.Remove(pimAttribute);
            pimAttribute.PIMClass = newClass;
            newClass.PIMAttributes.Add(pimAttribute);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PIMAttribute pimAttribute = Project.TranslateComponent<PIMAttribute>(attributeGuid);
            PIMClass oldClass = Project.TranslateComponent<PIMClass>(oldClassGuid);
            PIMClass newClass = Project.TranslateComponent<PIMClass>(newClassGuid);

            newClass.PIMAttributes.Remove(pimAttribute);
            pimAttribute.PIMClass = oldClass;
            oldClass.PIMAttributes.Insert(pimAttribute, index);
            return OperationResult.OK;
        }

        internal override MacroCommand PrePropagation()
        {
            MacroCommand command = new MacroCommand(Controller);
            command.Report = new CommandReport("Pre-propagation (move PIM attribute)");

            PIMAttribute attribute = Project.TranslateComponent<PIMAttribute>(attributeGuid);
            PIMClass targetClass = Project.TranslateComponent<PIMClass>(newClassGuid);
            PIMClass sourceClass = attribute.PIMClass;

            IEnumerable<PIMAssociation> pimAssociations = targetClass.GetAssociationsWith(sourceClass);
            IEnumerable<PSMAttribute> interpretedAttributes = attribute.GetInterpretedComponents().Cast<PSMAttribute>().Where(a => a.ID != PropagateSource);

            foreach (PSMAttribute psmAttribute in interpretedAttributes)
            {
                PSMClass intclass = psmAttribute.PSMClass.NearestInterpretedClass();
                Debug.Assert(intclass.Interpretation == sourceClass, "Intclass != sourceclass");

                bool found = false;
                PSMAssociation parentAssociation = intclass.ParentAssociation;

                foreach (PIMAssociation association in pimAssociations)
                {
                    if (parentAssociation != null && parentAssociation.Interpretation == association)
                    {
                        //moving the attribute up in PSM
                        found = true;

                        cmdCreateNewPSMAttribute c2 = new cmdCreateNewPSMAttribute(Controller);
                        Guid attrGuid2 = Guid.NewGuid();
                        c2.AttributeGuid = attrGuid2;
                        c2.Set(intclass, psmAttribute.AttributeType, psmAttribute.Name, psmAttribute.Lower, psmAttribute.Upper, psmAttribute.Element);
                        command.Commands.Add(c2);

                        if (psmAttribute.PSMClass != intclass)
                        {
                            command.Commands.Add(new cmdMovePSMAttribute(Controller) { AttributeGuid = psmAttribute, ClassGuid = intclass, Propagate = false });
                        }

                        acmdSynchroPSMAttributes s2 = new acmdSynchroPSMAttributes(Controller) { Propagate = false };
                        s2.X1.Add(psmAttribute);
                        s2.X2.Add(attrGuid2);
                        command.Commands.Add(s2);

                        if (psmAttribute.PSMClass != intclass)
                        {
                            command.Commands.Add(new cmdMovePSMAttribute(Controller) { AttributeGuid = psmAttribute, ClassGuid = psmAttribute.PSMClass, Propagate = false });
                        }
                        
                        acmdSetInterpretation i2 = new acmdSetPSMAttributeInterpretation(Controller, attrGuid2, attribute);
                        command.Commands.Add(i2);

                        cmdMovePSMAttribute m2 = new cmdMovePSMAttribute(Controller) { Propagate = false };
                        m2.Set(attrGuid2, intclass.NearestInterpretedParentClass());
                        command.Commands.Add(m2);
                    }

                    //select nearest interpreted child PSM classes, whose parent association's interpretation is the PIM association through which we are moving the attribute
                    IEnumerable<PSMClass> children = intclass.InterpretedSubClasses().Where<PSMClass>(pc => pc.Interpretation == targetClass && pc.ParentAssociation.Interpretation == association);
                    foreach (PSMClass child in children)
                    {
                        found = true;
                        cmdCreateNewPSMAttribute c = new cmdCreateNewPSMAttribute(Controller);
                        Guid attrGuid = Guid.NewGuid();
                        c.AttributeGuid = attrGuid;
                        c.Set(intclass, psmAttribute.AttributeType, psmAttribute.Name, psmAttribute.Lower, psmAttribute.Upper, psmAttribute.Element);
                        command.Commands.Add(c);

                        if (psmAttribute.PSMClass != intclass)
                        {
                            command.Commands.Add(new cmdMovePSMAttribute(Controller) { AttributeGuid = psmAttribute, ClassGuid = intclass, Propagate = false });
                        }

                        acmdSynchroPSMAttributes s2 = new acmdSynchroPSMAttributes(Controller) { Propagate = false };
                        s2.X1.Add(psmAttribute);
                        s2.X2.Add(attrGuid);
                        command.Commands.Add(s2);

                        if (psmAttribute.PSMClass != intclass)
                        {
                            command.Commands.Add(new cmdMovePSMAttribute(Controller) { AttributeGuid = psmAttribute, ClassGuid = psmAttribute.PSMClass, Propagate = false });
                        }

                        acmdSetInterpretation i = new acmdSetPSMAttributeInterpretation(Controller, attrGuid, attribute);
                        command.Commands.Add(i);

                        cmdMovePSMAttribute m = new cmdMovePSMAttribute(Controller) { Propagate = false };
                        m.Set(attrGuid, child);
                        command.Commands.Add(m);
                    }

                    if (!found)
                    {
                        cmdCreateNewPSMAttribute c = new cmdCreateNewPSMAttribute(Controller);
                        Guid attrGuid = Guid.NewGuid();
                        c.AttributeGuid = attrGuid;
                        c.Set(/*intclass*/psmAttribute.PSMClass, psmAttribute.AttributeType, psmAttribute.Name, psmAttribute.Lower, psmAttribute.Upper, psmAttribute.Element);
                        command.Commands.Add(c);

                        /*if (psmAttribute.PSMClass != intclass)
                        {
                            cmdMovePSMAttribute m1 = new cmdMovePSMAttribute(Controller) { Propagate = false };
                            m1.Set(psmAttribute, intclass);
                            command.Commands.Add(m1);
                        }*/

                        acmdSynchroPSMAttributes s = new acmdSynchroPSMAttributes(Controller) { Propagate = false };
                        s.X1.Add(psmAttribute);
                        s.X2.Add(attrGuid);
                        command.Commands.Add(s);

                        /*if (psmAttribute.PSMClass != intclass)
                        {
                            cmdMovePSMAttribute m2 = new cmdMovePSMAttribute(Controller) { Propagate = false };
                            m2.Set(psmAttribute, psmAttribute.PSMClass);
                            command.Commands.Add(m2);
                        }*/

                        acmdSetInterpretation i = new acmdSetPSMAttributeInterpretation(Controller, attrGuid, attribute);
                        command.Commands.Add(i);

                        //create psmassoc, class
                        Guid ncGuid = Guid.NewGuid();
                        acmdNewPSMClass nc = new acmdNewPSMClass(Controller, psmAttribute.PSMSchema) { ClassGuid = ncGuid };
                        command.Commands.Add(nc);

                        acmdRenameComponent rc = new acmdRenameComponent(Controller, ncGuid, targetClass.Name);
                        command.Commands.Add(rc);

                        acmdSetInterpretation ic = new acmdSetPSMClassInterpretation(Controller, ncGuid, targetClass);
                        command.Commands.Add(ic);

                        Guid naGuid = Guid.NewGuid();

                        acmdNewPSMAssociation na = new acmdNewPSMAssociation(Controller, /*intclass*/ psmAttribute.PSMClass, ncGuid, psmAttribute.PSMSchema) { AssociationGuid = naGuid };
                        command.Commands.Add(na);

                        acmdRenameComponent ra = new acmdRenameComponent(Controller, naGuid, association.Name);
                        command.Commands.Add(ra);

                        PIMAssociationEnd e = targetClass.PIMAssociationEnds.Single<PIMAssociationEnd>(aend => aend.PIMAssociation == association);
                        acmdUpdatePSMAssociationCardinality carda = new acmdUpdatePSMAssociationCardinality(Controller, naGuid, e.Lower, e.Upper) { Propagate = false };
                        command.Commands.Add(carda);

                        acmdSetInterpretation ia = new acmdSetPSMAssociationInterpretation(Controller, naGuid, association);
                        command.Commands.Add(ia);

                        acmdMovePSMAttribute m = new acmdMovePSMAttribute(Controller, attrGuid, ncGuid) { Propagate = false };
                        command.Commands.Add(m);
                    }
                }
                //delete attribute
                cmdDeletePSMAttribute d = new cmdDeletePSMAttribute(Controller) { Propagate = false };
                d.Set(psmAttribute);
                command.Commands.Add(d);
            }

            command.CheckFirstOnlyInCanExecute = true;
            return command;
        }
    }
}
