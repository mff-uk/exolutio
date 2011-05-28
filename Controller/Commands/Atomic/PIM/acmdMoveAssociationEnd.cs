using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Model;
using Exolutio.Controller.Commands.Complex.PSM;
using Exolutio.Controller.Commands.Atomic.PSM;
using Exolutio.Model.PSM;
using System.Diagnostics;

namespace Exolutio.Controller.Commands.Atomic.PIM
{
    public class acmdMoveAssociationEnd : StackedCommand
    {
        Guid associationEndGuid, newClassGuid, oldClassGuid;

        public acmdMoveAssociationEnd(Controller c, Guid pimAssociationEndGuid, Guid pimClassGuid)
            : base(c)
        {
            newClassGuid = pimClassGuid;
            associationEndGuid = pimAssociationEndGuid;
        }

        public override bool CanExecute()
        {
            if (!(newClassGuid != Guid.Empty
                && Project.VerifyComponentType<PIMClass>(newClassGuid)
                && associationEndGuid != Guid.Empty
                && Project.VerifyComponentType<PIMAssociationEnd>(associationEndGuid)))
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }

            PIMClass newclass = Project.TranslateComponent<PIMClass>(newClassGuid);
            PIMClass oldclass = Project.TranslateComponent<PIMAssociationEnd>(associationEndGuid).PIMClass;

            if (newclass.GetAssociationsWith(oldclass).Count<PIMAssociation>() == 0)
            {
                ErrorDescription = CommandErrors.CMDERR_NO_COMMON_ASSOCIATION;
                return false;
            }
            return true;
        }
        
        internal override void CommandOperation()
        {
            PIMAssociationEnd pimAssociationEnd = Project.TranslateComponent<PIMAssociationEnd>(associationEndGuid);
            PIMClass oldClass = pimAssociationEnd.PIMClass;
            oldClassGuid = oldClass;
            PIMClass newClass = Project.TranslateComponent<PIMClass>(newClassGuid);
            Report = new CommandReport("{0} moved from {1} to {2}.", pimAssociationEnd, oldClass, newClass);

            oldClass.PIMAssociationEnds.Remove(pimAssociationEnd);
            pimAssociationEnd.PIMClass = newClass;
            newClass.PIMAssociationEnds.Add(pimAssociationEnd);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PIMAssociationEnd pimAssociationEnd = Project.TranslateComponent<PIMAssociationEnd>(associationEndGuid);
            PIMClass oldClass = Project.TranslateComponent<PIMClass>(oldClassGuid);
            PIMClass newClass = Project.TranslateComponent<PIMClass>(newClassGuid);

            newClass.PIMAssociationEnds.Remove(pimAssociationEnd);
            pimAssociationEnd.PIMClass = oldClass;
            oldClass.PIMAssociationEnds.Add(pimAssociationEnd);
            return OperationResult.OK;
        }

        internal override MacroCommand PrePropagation()
        {
            MacroCommand command = new MacroCommand(Controller);
            command.Report = new CommandReport("Pre-propagation (reconnect PIM association end)");

            PIMAssociationEnd assocEnd = Project.TranslateComponent<PIMAssociationEnd>(associationEndGuid);
            PIMClass targetClass = Project.TranslateComponent<PIMClass>(newClassGuid);
            PIMClass sourceClass = assocEnd.PIMClass;

            IEnumerable<PIMAssociation> pimAssociations = targetClass.GetAssociationsWith(sourceClass);
            IEnumerable<PSMAssociation> interpretedAssociations = assocEnd.PIMAssociation.GetInterpretedComponents().Cast<PSMAssociation>().Where(a => a.ID != PropagateSource);

            foreach (PSMAssociation psmAssociation in interpretedAssociations)
            {
                if (psmAssociation.NearestInterpretedClass().Interpretation == sourceClass)
                    // C1 => C2, C1 (intclass) source, C2 (its descendant) target //I(R')=R^(E2)
                {
                    //C'u
                    PSMClass intclass = psmAssociation.NearestInterpretedClass();
                    
                    //C', it is a PSMClass because psmAssociation has interpretation
                    PSMClass child = psmAssociation.Child as PSMClass;

                    Debug.Assert(intclass.Interpretation == sourceClass, "Intclass != sourceclass");

                    bool found = false;
                    PSMAssociation parentAssociation = intclass.ParentAssociation;

                    foreach (PIMAssociation association in pimAssociations)
                    {
                        if (parentAssociation != null && parentAssociation.Interpretation == association)
                        {
                            //moving the association up in PSM
                            found = true;

                            Guid classGuid2 = Guid.NewGuid();
                            Guid assocGuid2 = Guid.NewGuid();

                            command.Commands.Add(new acmdNewPSMClass(Controller, psmAssociation.Schema) {ClassGuid = classGuid2} );
                            command.Commands.Add(new acmdRenameComponent(Controller, classGuid2, child.Name));
                            command.Commands.Add(new acmdSetPSMClassInterpretation(Controller, classGuid2, child.Interpretation));
                            command.Commands.Add(new acmdSetRepresentedClass(Controller, classGuid2, child));
                            command.Commands.Add(new acmdNewPSMAssociation(Controller, parentAssociation.NearestInterpretedClass(), classGuid2, psmAssociation.Schema) { AssociationGuid = assocGuid2 });
                            command.Commands.Add(new acmdRenameComponent(Controller, assocGuid2, psmAssociation.Name) { Propagate = false });
                            command.Commands.Add(new acmdUpdatePSMAssociationCardinality(Controller, assocGuid2, psmAssociation.Lower, psmAssociation.Upper) { Propagate = false });
                            command.Commands.Add(new acmdSetPSMAssociationInterpretation(Controller, assocGuid2, psmAssociation.Interpretation));

                            acmdSynchroPSMAssociations s = new acmdSynchroPSMAssociations(Controller) { Propagate = false };
                            s.X1.Add(psmAssociation);
                            s.X2.Add(assocGuid2);
                            command.Commands.Add(s);

                            cmdReconnectPSMAssociation r = new cmdReconnectPSMAssociation(Controller) { Propagate = false };
                            r.Set(assocGuid2, parentAssociation.NearestInterpretedClass());
                            command.Commands.Add(r);
                        }

                        //select nearest interpreted child PSM classes, whose parent association's interpretation is the PIM association through which we are moving the association
                        IEnumerable<PSMClass> children = intclass.InterpretedSubClasses().Where(pc => pc.Interpretation == targetClass && pc.ParentAssociation.Interpretation == association);
                        foreach (PSMClass childClass in children)
                        {
                            found = true;
                            Guid classGuid2 = Guid.NewGuid();
                            Guid assocGuid2 = Guid.NewGuid();

                            command.Commands.Add(new acmdNewPSMClass(Controller, psmAssociation.Schema) { ClassGuid = classGuid2 });
                            command.Commands.Add(new acmdRenameComponent(Controller, classGuid2, child.Name) { Propagate = false });
                            command.Commands.Add(new acmdSetPSMClassInterpretation(Controller, classGuid2, child.Interpretation));
                            command.Commands.Add(new acmdSetRepresentedClass(Controller, classGuid2, child));
                            command.Commands.Add(new acmdNewPSMAssociation(Controller, childClass, classGuid2, psmAssociation.Schema) { AssociationGuid = assocGuid2 });
                            command.Commands.Add(new acmdRenameComponent(Controller, assocGuid2, psmAssociation.Name) { Propagate = false });
                            command.Commands.Add(new acmdUpdatePSMAssociationCardinality(Controller, assocGuid2, psmAssociation.Lower, psmAssociation.Upper) { Propagate = false });
                            command.Commands.Add(new acmdSetPSMAssociationInterpretation(Controller, assocGuid2, psmAssociation.Interpretation) { ForceExecute = true });

                            acmdSynchroPSMAssociations s = new acmdSynchroPSMAssociations(Controller) { Propagate = false };
                            s.X1.Add(psmAssociation);
                            s.X2.Add(assocGuid2);
                            command.Commands.Add(s);

                            cmdReconnectPSMAssociation r = new cmdReconnectPSMAssociation(Controller) { Propagate = false };
                            r.Set(assocGuid2, childClass);
                            command.Commands.Add(r);
                        }

                        if (!found)
                        {
                            Guid classGuid = Guid.NewGuid();
                            Guid assocGuid = Guid.NewGuid();

                            command.Commands.Add(new acmdNewPSMClass(Controller, psmAssociation.Schema) { ClassGuid = classGuid });
                            command.Commands.Add(new acmdRenameComponent(Controller, classGuid, targetClass.Name) { Propagate = false });
                            command.Commands.Add(new acmdSetPSMClassInterpretation(Controller, classGuid, targetClass));
                            command.Commands.Add(new acmdNewPSMAssociation(Controller, parentAssociation.NearestInterpretedClass(), classGuid, psmAssociation.Schema) { AssociationGuid = assocGuid });
                            command.Commands.Add(new acmdRenameComponent(Controller, assocGuid, association.Name) { Propagate = false });
                            PIMAssociationEnd e = targetClass.PIMAssociationEnds.Single<PIMAssociationEnd>(aend => aend.PIMAssociation == association);
                            command.Commands.Add(new acmdUpdatePSMAssociationCardinality(Controller, assocGuid, e.Lower, e.Upper) { Propagate = false });
                            command.Commands.Add(new acmdSetPSMAssociationInterpretation(Controller, assocGuid, association) { ForceExecute = true });

                            Guid classGuid2 = Guid.NewGuid();
                            Guid assocGuid2 = Guid.NewGuid();

                            command.Commands.Add(new acmdNewPSMClass(Controller, psmAssociation.Schema) { ClassGuid = classGuid2 });
                            command.Commands.Add(new acmdRenameComponent(Controller, classGuid2, child.Name) { Propagate = false });
                            command.Commands.Add(new acmdSetPSMClassInterpretation(Controller, classGuid2, child.Interpretation));
                            command.Commands.Add(new acmdSetRepresentedClass(Controller, classGuid2, child));
                            command.Commands.Add(new acmdNewPSMAssociation(Controller, classGuid, classGuid2, psmAssociation.Schema) { AssociationGuid = assocGuid2 });
                            command.Commands.Add(new acmdRenameComponent(Controller, assocGuid2, psmAssociation.Name) { Propagate = false });
                            command.Commands.Add(new acmdUpdatePSMAssociationCardinality(Controller, assocGuid2, psmAssociation.Lower, psmAssociation.Upper) { Propagate = false });
                            command.Commands.Add(new acmdSetPSMAssociationInterpretation(Controller, assocGuid2, psmAssociation.Interpretation) { ForceExecute = true });

                            acmdSynchroPSMAssociations s = new acmdSynchroPSMAssociations(Controller) { Propagate = false };
                            s.X1.Add(psmAssociation);
                            s.X2.Add(assocGuid2);
                            command.Commands.Add(s);

                            cmdReconnectPSMAssociation r = new cmdReconnectPSMAssociation(Controller) { Propagate = false };
                            r.Set(assocGuid2, classGuid);
                            command.Commands.Add(r);
                        }
                    }
                    //delete association
                    cmdDeletePSMAssociation d = new cmdDeletePSMAssociation(Controller) { Propagate = false };
                    d.Set(psmAssociation);
                    command.Commands.Add(d);
                }
                else
                // C1 => C2, C2 source, C1 target
                {
                    //C'
                    PSMClass intclass = psmAssociation.NearestInterpretedClass();

                    //C'u, it is a PSMClass because psmAssociation has interpretation
                    PSMClass child = psmAssociation.Child as PSMClass;

                    Debug.Assert(child.Interpretation == sourceClass, "Child != sourceclass");

                    foreach (PIMAssociation association in pimAssociations)
                    {
                        //The case that C'v would be an ancestor of intclass does not apply here
                        
                        //select nearest interpreted child PSM classes, whose parent association's interpretation is the PIM association through which we are moving the association
                        Dictionary<Guid, Guid> children = new Dictionary<Guid, Guid>();
                        foreach (PSMClass ch in intclass.InterpretedSubClasses().Where(pc => pc.Interpretation == sourceClass && pc.ParentAssociation.Interpretation == association))
                        {
                            children.Add(ch, ch.ParentAssociation);
                        }
                        if (children.Count() == 0)
                        {
                            Guid classGuid = Guid.NewGuid();
                            Guid assocGuid = Guid.NewGuid();

                            command.Commands.Add(new acmdNewPSMClass(Controller, psmAssociation.Schema) { ClassGuid = classGuid });
                            command.Commands.Add(new acmdRenameComponent(Controller, classGuid, targetClass.Name) { Propagate = false });
                            command.Commands.Add(new acmdSetPSMClassInterpretation(Controller, classGuid, targetClass));
                            command.Commands.Add(new acmdNewPSMAssociation(Controller, psmAssociation.Child, classGuid, psmAssociation.Schema) { AssociationGuid = assocGuid });
                            command.Commands.Add(new acmdRenameComponent(Controller, assocGuid, association.Name) { Propagate = false });
                            PIMAssociationEnd e = targetClass.PIMAssociationEnds.Single(aend => aend.PIMAssociation == association);
                            command.Commands.Add(new acmdUpdatePSMAssociationCardinality(Controller, assocGuid, e.Lower, e.Upper) { Propagate = false });
                            command.Commands.Add(new acmdSetPSMAssociationInterpretation(Controller, assocGuid, association) { ForceExecute = true });
                            children.Add(classGuid, assocGuid);
                        }

                        foreach (KeyValuePair<Guid, Guid> p in children)
                        {
                            
                            Guid classGuid = Guid.NewGuid();
                            Guid assocGuid = Guid.NewGuid();

                            //C'_
                            command.Commands.Add(new acmdNewPSMClass(Controller, psmAssociation.Schema) { ClassGuid = classGuid });
                            command.Commands.Add(new acmdRenameComponent(Controller, classGuid, intclass.Name) { Propagate = false });
                            command.Commands.Add(new acmdSetPSMClassInterpretation(Controller, classGuid, intclass.Interpretation));
                            //command.Commands.Add(new acmdSetRepresentedClass(Controller, classGuid, intclass));
                            //R'_
                            command.Commands.Add(new acmdNewPSMAssociation(Controller, child, classGuid, psmAssociation.Schema) { AssociationGuid = assocGuid });
                            command.Commands.Add(new acmdRenameComponent(Controller, assocGuid, psmAssociation.Name) { Propagate = false });
                            PIMAssociationEnd e1 = (psmAssociation.Interpretation as PIMAssociation).PIMAssociationEnds.Single(aend => aend.PIMClass == intclass.Interpretation);
                            command.Commands.Add(new acmdUpdatePSMAssociationCardinality(Controller, assocGuid, e1.Lower, e1.Upper) { Propagate = false });
                            command.Commands.Add(new acmdSetPSMAssociationInterpretation(Controller, assocGuid, psmAssociation.Interpretation) { ForceExecute = true });
                            acmdSynchroPSMAssociations s = new acmdSynchroPSMAssociations(Controller);
                            s.X1.Add(psmAssociation);
                            s.X2.Add(assocGuid);
                            command.Commands.Add(s);

                            cmdReconnectPSMAssociation r = new cmdReconnectPSMAssociation(Controller);
                            r.Set(assocGuid, p.Key);
                            command.Commands.Add(r);

                            Guid classGuid3 = Guid.NewGuid();
                            Guid assocGuid3 = Guid.NewGuid();

                            //Cu'^_
                            command.Commands.Add(new acmdNewPSMClass(Controller, psmAssociation.Schema) { ClassGuid = classGuid3 });
                            command.Commands.Add(new acmdRenameComponent(Controller, classGuid3, child.Name) { Propagate = false });
                            command.Commands.Add(new acmdSetPSMClassInterpretation(Controller, classGuid3, child.Interpretation));
                            command.Commands.Add(new acmdSetRepresentedClass(Controller, classGuid3, child) { Propagate = false });

                            //Rx'^_
                            command.Commands.Add(new acmdNewPSMAssociation(Controller, p.Key, classGuid3, psmAssociation.PSMSchema) { AssociationGuid = assocGuid3 });
                            command.Commands.Add(new acmdRenameComponent(Controller, assocGuid3, psmAssociation.Name) { Propagate = false });
                            PIMAssociationEnd e2 = association.PIMAssociationEnds.Single(aend => aend.PIMClass == sourceClass);
                            command.Commands.Add(new acmdUpdatePSMAssociationCardinality(Controller, assocGuid3, e2.Lower, e2.Upper) { Propagate = false });
                            command.Commands.Add(new acmdSetPSMAssociationInterpretation(Controller, assocGuid3, association) { ForceExecute = true });

                            acmdSynchroPSMAssociations s2 = new acmdSynchroPSMAssociations(Controller) { Propagate = false };
                            s2.X1.Add(p.Value);
                            s2.X2.Add(assocGuid3);
                            command.Commands.Add(s2);

                            cmdDeletePSMAssociation d0 = new cmdDeletePSMAssociation(Controller) { Propagate = false };
                            d0.Set(p.Value);
                            command.Commands.Add(d0);

                            //R'^_
                            Guid assocGuid4 = Guid.NewGuid();
                            command.Commands.Add(new acmdNewPSMAssociation(Controller, intclass, p.Key, psmAssociation.PSMSchema) { AssociationGuid = assocGuid4 });
                            command.Commands.Add(new acmdRenameComponent(Controller, assocGuid4, psmAssociation.Name) { Propagate = false });
                            command.Commands.Add(new acmdUpdatePSMAssociationCardinality(Controller, assocGuid4, assocEnd.Lower, assocEnd.Upper) { Propagate = false });
                            command.Commands.Add(new acmdSetPSMAssociationInterpretation(Controller, assocGuid4, psmAssociation.Interpretation) { ForceExecute = true });

                            acmdSynchroPSMAssociations s3 = new acmdSynchroPSMAssociations(Controller) { Propagate = false };
                            s3.X1.Add(assocGuid);
                            s3.X2.Add(assocGuid4);
                            command.Commands.Add(s3);

                            cmdDeletePSMAssociation d2 = new cmdDeletePSMAssociation(Controller) { Propagate = false };
                            d2.Set(assocGuid);
                            command.Commands.Add(d2);

                            cmdDeletePSMClass d3 = new cmdDeletePSMClass(Controller) { Propagate = false };
                            d3.Set(classGuid);
                            command.Commands.Add(d3);
                        }
                    }
                    //delete association
                    cmdDeletePSMAssociation d = new cmdDeletePSMAssociation(Controller) { Propagate = false };
                    d.Set(psmAssociation);
                    command.Commands.Add(d);
                }
            }

            command.CheckFirstOnlyInCanExecute = true;
            return command;
        }
    }
}
