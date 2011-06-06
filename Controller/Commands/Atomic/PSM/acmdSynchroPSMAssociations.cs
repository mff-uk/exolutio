using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Controller.Commands.Complex.PIM;
using System.Collections.ObjectModel;
using Exolutio.Controller.Commands.Atomic.PIM;
using Exolutio.SupportingClasses;

namespace Exolutio.Controller.Commands.Atomic.PSM
{
    public class acmdSynchroPSMAssociations : StackedCommand
    {
        public List<Guid> X1 = new List<Guid>();
        public List<Guid> X2 = new List<Guid>();

        public acmdSynchroPSMAssociations(Controller c)
            : base(c) { }

        private bool getAmAndInterpretation(ReadOnlyCollection<PSMAssociation> aX1, ReadOnlyCollection<PSMAssociation> aX2, out PIMClass interpretation, out PSMClass am)
        {
            List<PSMClass> d = new List<PSMClass>();
            am = null;
            interpretation = null;
            IEnumerable<PSMAssociation> union = aX1.Union(aX2);

            if (union.Count() == 1)
            {
                return true;
            }
            else foreach (PSMAssociation a in union)
            {
                if (a.Parent is PSMClass && d.Contains((PSMClass)a.Parent))
                {
                    if (am == null) am = a.Parent as PSMClass;
                    else if (am != a.Parent) return false;
/*                    if (a.Child.Interpretation != null)
                    {
                        if (interpretation == null) interpretation = a.Child.Interpretation as PIMClass;
                        else if (interpretation != a.Child.Interpretation) return false;
                    }
*/                }
                if (a.Child is PSMClass && d.Contains((PSMClass)a.Child))
                {
                    if (am == null) am = a.Child as PSMClass;
                    else if (am != a.Child) return false;
/*                    if (a.Parent.Interpretation != null)
                    {
                        if (interpretation == null) interpretation = a.Parent.Interpretation as PIMClass;
                        else if (interpretation != a.Parent.Interpretation) return false;
                    }
*/                }
                if (!(a.Parent is PSMAssociationMember && a.Child is PSMClass)) return false;
                if (a.Parent is PSMClass) d.Add(a.Parent as PSMClass);
                d.Add(a.Child as PSMClass);
            }
            PSMClass c = am;
            d.RemoveAll(a => a == c);
            c = d.FirstOrDefault(cl => cl.Interpretation != null);
            interpretation = c == null ? null : c.Interpretation as PIMClass;
            PIMClass i = interpretation as PIMClass;
            if (d.Any(cl => cl.Interpretation != null && (cl.Interpretation as PIMClass) != i)) return false;
            if (am == null) return false;
            else return true;
        }
        
        public override bool CanExecute()
        {
            ReadOnlyCollection<PSMAssociation> aX1 = Project.TranslateComponentCollection<PSMAssociation>(X1);
            ReadOnlyCollection<PSMAssociation> aX2 = Project.TranslateComponentCollection<PSMAssociation>(X2);
            if (aX1.Count == 0 || aX2.Count == 0)
            {
                ErrorDescription = CommandErrors.CMDERR_CANNOT_SYNCHRO_EMPTY_SETS;
                return false;
            }

            PSMClass am;
            PIMClass interpretation;
            if (!getAmAndInterpretation(aX1, aX2, out interpretation, out am))
            {
                ErrorDescription = CommandErrors.CMDERR_CANNOT_SYNCHRO_PSM_ASSOC;
                return false;
            }

            return true;
        }

        internal override void CommandOperation()
        {
            Report = new CommandReport(CommandReports.PSM_ASSOC_SYNCHRO,
                String.Concat(Project.TranslateComponentCollection<PSMAssociation>(X1).Select<PSMAssociation, String>(a => a.ToString() + " ")),
                String.Concat(Project.TranslateComponentCollection<PSMAssociation>(X2).Select<PSMAssociation, String>(a => a.ToString() + " ")));
        }

        internal override PropagationMacroCommand PostPropagation()
        {
            ReadOnlyCollection<PSMAssociation> aX1 = Project.TranslateComponentCollection<PSMAssociation>(X1);
            ReadOnlyCollection<PSMAssociation> aX2 = Project.TranslateComponentCollection<PSMAssociation>(X2);
            PSMClass C1_;
            PIMClass C1;
            PIMClass C2;

            if (aX1.Count == 0 || aX2.Count == 0 || aX1.Union(aX2).Count() == 1) return null;
            if (!getAmAndInterpretation(aX1, aX2, out C2, out C1_)) return null;
            if (aX1.Any(assoc => assoc.Interpretation == null) && aX2.Any(assoc => assoc.Interpretation == null)) return null;
            C1 = C1_.Interpretation as PIMClass;

            PropagationMacroCommand command = new PropagationMacroCommand(Controller) { CheckFirstOnlyInCanExecute = true };
            command.Report = new CommandReport("Post-propagation (synchronize PSM association sets)");

            //Twice... X1 => X2, X2 => X1
            for (int i = 0; i < 2; i++)
            {
                if (aX1.All(assoc => assoc.Interpretation != null))
                {
                    IEnumerable<PIMAssociation> interpretations1 = aX1.Select<PSMAssociation, PIMAssociation>(a => a.Interpretation as PIMAssociation);
                    IEnumerable<PIMAssociation> interpretations2 = aX2.Where(a => a.Interpretation != null).Select<PSMAssociation, PIMAssociation>(a => a.Interpretation as PIMAssociation);
                    IEnumerable<PSMAssociation> associationsToPropagate = aX2.Where(a => a.Interpretation == null);
                    List<Guid> newAssociations = new List<Guid>();

                    foreach (PSMAssociation a in associationsToPropagate)
                    {
                        PSMClass C2_ = a.Parent.Interpretation == C1 ? a.Child as PSMClass : a.Parent as PSMClass;

                        Guid pimClassGuid = C2;
                        if (C2_.Interpretation == null)
                        {
                            pimClassGuid = Guid.NewGuid();
                            cmdCreateNewPIMClass c = new cmdCreateNewPIMClass(Controller) { ClassGuid = pimClassGuid };
                            c.Set(C2_.Name, C1.PIMSchema);
                            command.Commands.Add(c);

                            acmdSetInterpretation cmdi = new acmdSetPSMClassInterpretation(Controller, C2_, pimClassGuid);
                            command.Commands.Add(cmdi);
                        }

                        //else - but all C2 should be the same class

                        Guid assocGuid = Guid.NewGuid();
                        Guid assocEnd1Guid = Guid.NewGuid();
                        Guid assocEnd2Guid = Guid.NewGuid();

                        command.Commands.Add(new acmdNewPIMAssociation(Controller, C1, assocEnd1Guid, pimClassGuid, assocEnd2Guid, C1.PIMSchema) { AssociationGuid = assocGuid });
                        foreach (PIMDiagram d in Project.SingleVersion.PIMDiagrams.Where(d => d.PIMComponents.Contains(C1)))
                        {
                            command.Commands.Add(new acmdAddComponentToDiagram(Controller, assocGuid, d));
                        }
                            
                        command.Commands.Add(new acmdRenameComponent(Controller, assocGuid, a.Name));
                        command.Commands.Add(new acmdUpdatePIMAssociationEndCardinality(Controller, assocEnd2Guid, a.Lower, a.Upper));
                        command.Commands.Add(new acmdSetPSMAssociationInterpretation(Controller, a, assocGuid));
                        newAssociations.Add(assocGuid);
                    }

                    IEnumerable<PIMAssociation> associations = C1.GetAssociationsWith(C2);
                    IEnumerable<Guid> synchroGroup1 = associations.Where(a => interpretations1.Contains(a)).Select<PIMAssociation, Guid>(g => g);
                    IEnumerable<Guid> synchroGroup2 = associations.Where(a => interpretations2.Contains(a)).Select<PIMAssociation, Guid>(g => g).Union(newAssociations);

                    //We could somehow add PropagateSource here...
                    command.Commands.Add(new acmdSynchroPIMAssociations(Controller) { X1 = synchroGroup1.ToList(), X2 = synchroGroup2.ToList() });
                }
                //Swap the two lists and do it again
                ReadOnlyCollection<PSMAssociation> temp = aX1;
                aX1 = aX2;
                aX2 = temp;
            }
            return command;
        }
    }
}
