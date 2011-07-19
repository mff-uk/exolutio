using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Complex.PSM;
using Exolutio.Controller.Commands.Atomic.PSM;
using System.Collections.ObjectModel;

namespace Exolutio.Controller.Commands.Atomic.PIM
{
    public class acmdSynchroPIMAssociations : StackedCommand
    {
        public List<Guid> X1 = new List<Guid>();
        public List<Guid> X2 = new List<Guid>();

        public acmdSynchroPIMAssociations(Controller c)
            : base(c) { }

        public override bool CanExecute()
        {
            ReadOnlyCollection<PIMAssociation> aX1 = Project.TranslateComponentCollection<PIMAssociation>(X1);
            ReadOnlyCollection<PIMAssociation> aX2 = Project.TranslateComponentCollection<PIMAssociation>(X2);
            if (aX1.Count == 0 || aX2.Count == 0) return false;
            PIMClass pimClass1 = aX2[0].PIMClasses[0];
            PIMClass pimClass2 = aX2[0].PIMClasses[1];

            IEnumerable<PIMAssociation> intersect = pimClass1.PIMAssociationEnds.Select<PIMAssociationEnd, PIMAssociation>(e => e.PIMAssociation)
                .Intersect<PIMAssociation>(
                pimClass2.PIMAssociationEnds.Select<PIMAssociationEnd, PIMAssociation>(e => e.PIMAssociation));

            if (
                !aX1.All<PIMAssociation>(a => intersect.Contains<PIMAssociation>(a))
                ||
                !aX2.All<PIMAssociation>(a => intersect.Contains<PIMAssociation>(a))
               )
            {
                ErrorDescription = CommandErrors.CMDERR_SYNCHRO_PIM_ASSOC_NOT_SUBSET;
                return false;
            }
            return true;
        }
        
        internal override void CommandOperation()
        {
            Report = new CommandReport(CommandReports.PIM_ASSOC_SYNCHRO, 
                String.Concat(Project.TranslateComponentCollection<PIMAssociation>(X1).Select<PIMAssociation, String>(a => a.ToString() + " ")),
                String.Concat(Project.TranslateComponentCollection<PIMAssociation>(X2).Select<PIMAssociation, String>(a => a.ToString() + " ")));
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            return OperationResult.OK;
        }

        internal override PropagationMacroCommand PrePropagation()
        {
            IEnumerable<PIMAssociation> aX1 = Project.TranslateComponentCollection<PIMAssociation>(X1);
            IEnumerable<PIMAssociation> aX2 = Project.TranslateComponentCollection<PIMAssociation>(X2);
            if (aX1.Count() == 0 || aX2.Count() == 0 || aX1.Union(aX2).Count() == 1) return null;

            PropagationMacroCommand command = new PropagationMacroCommand(Controller) { CheckFirstOnlyInCanExecute = true };
            command.Report = new CommandReport("Pre-propagation (synchronize PIM association sets)");
            PIMClass pimClass1 = aX2.First().PIMClasses.First();
            PIMClass pimClass2 = aX2.First().PIMClasses.Last();

            //Twice... X1 => X2, X2 => X1
            for (int i = 0; i < 2; i++)
            {
                IEnumerable<PSMClass> allPSMClasses = pimClass1.GetInterpretedComponents().Union(pimClass2.GetInterpretedComponents()).Cast<PSMClass>();
                //Selects psmClasses affected by the synchronization (those which have aX1 counterpart present)
                List<PSMClass> psmClasses = new List<PSMClass>();
                foreach (PSMClass c in allPSMClasses)
                {
                    IEnumerable<PSMAssociation> assocs = c.GetContextPSMAssociations();
                    if (assocs.Union(c.ParentAssociation == null
                                ? Enumerable.Empty<PSMAssociation>()
                                : Enumerable.Repeat(c.ParentAssociation, 1))
                               .Where(a => a.Interpretation != null)
                               .Select(psma => psma.Interpretation as PIMAssociation)
                               .Intersect(aX1)
                               .OrderBy(k => k.ID)
                               .SequenceEqual(aX1.OrderBy(j => j.ID)))
                        psmClasses.Add(c);
                }

                //Elimination of SRs where C is affected and SR is a SR of C
                bool found = true;
                while (found)
                {
                    found = false;
                    List<PSMClass> list = new List<PSMClass>();
                    foreach (PSMClass psmClass in psmClasses)
                    {
                        if (psmClass.RepresentedClass != null 
                            && psmClasses.Contains(psmClass.RepresentedClass)
                            && !psmClass.GetContextPSMAssociations(true)
                                    .Union(psmClass.ParentAssociation == null
                                        ? Enumerable.Empty<PSMAssociation>()
                                        : Enumerable.Repeat(psmClass.ParentAssociation, 1))
                                   .Where(a => a.Interpretation != null)
                                   .Select(psma => psma.Interpretation as PIMAssociation)
                                   .Intersect(aX1)
                                   .OrderBy(k => k.ID)
                                   .SequenceEqual(aX1.OrderBy(j => j.ID)))
                        {
                            found = true;
                        }
                        else list.Add(psmClass);
                    }
                    psmClasses = list;
                }

                foreach (PSMClass psmClass in psmClasses)
                {
                    IEnumerable<PSMAssociation> parentEnum =
                        psmClass.ParentAssociation == null
                        ? Enumerable.Empty<PSMAssociation>()
                        : Enumerable.Repeat(psmClass.ParentAssociation, 1);

                    IEnumerable<Tuple<PSMAssociation, IEnumerable<ModelIterator.MoveStep>>> associationsAndPaths = psmClass.GetContextPSMAssociationsWithPaths();
                    IEnumerable<PSMAssociation> associations = associationsAndPaths.Select(t => t.Item1).Union(parentEnum);
                    IEnumerable<PSMAssociation> interpretedAssociations = associations.Where(a => a.Interpretation != null);
                    IEnumerable<PSMAssociation> responsibleAssociations = interpretedAssociations.Where(a => aX1.Contains(a.Interpretation as PIMAssociation));
                    IEnumerable<PIMAssociation> interpretations = interpretedAssociations.Select(a => a.Interpretation as PIMAssociation);
                    IEnumerable<PIMAssociation> associationsToPropagate = aX2.Where(a => !interpretations.Contains(a));

                    IEnumerable<PSMAssociation> psmAssociationsToMoveList = interpretedAssociations.Where(a => a.Parent != psmClass && a.Child != psmClass && aX1.Contains((PIMAssociation)a.Interpretation) || aX2.Contains((PIMAssociation)a.Interpretation));
                    Dictionary<PSMAssociation, IEnumerable<ModelIterator.MoveStep>> psmAssociationsToMove = new Dictionary<PSMAssociation, IEnumerable<ModelIterator.MoveStep>>();
                    foreach (PSMAssociation t in psmAssociationsToMoveList)
                    {
                        Tuple<PSMAssociation, IEnumerable<ModelIterator.MoveStep>> tuple = associationsAndPaths.Single(tup => tup.Item1 == t);
                        psmAssociationsToMove.Add(tuple.Item1, tuple.Item2);
                    }

                    List<Guid> newAssociationsGuid = new List<Guid>();
                    foreach (PIMAssociation a in associationsToPropagate)
                    {
                        Guid classGuid = Guid.NewGuid();
                        command.Commands.Add(new acmdNewPSMClass(Controller, psmClass.PSMSchema) { ClassGuid = classGuid });
                        command.Commands.Add(new acmdRenameComponent(Controller, classGuid, psmClass.Interpretation == pimClass1 ? pimClass2.Name : pimClass1.Name));
                        command.Commands.Add(new acmdSetPSMClassInterpretation(Controller, classGuid, psmClass.Interpretation == pimClass1 ? pimClass2 : pimClass1));

                        Guid assocGuid = Guid.NewGuid();
                        command.Commands.Add(new acmdNewPSMAssociation(Controller, psmClass, classGuid, psmClass.PSMSchema) { AssociationGuid = assocGuid });
                        command.Commands.Add(new acmdRenameComponent(Controller, assocGuid, a.Name));
                        PIMAssociationEnd e = a.PIMAssociationEnds.First(ae => ae.PIMClass != psmClass.Interpretation);
                        command.Commands.Add(new acmdUpdatePSMAssociationCardinality(Controller, assocGuid, e.Lower, e.Upper));
                        command.Commands.Add(new acmdSetPSMAssociationInterpretation(Controller, assocGuid, a));
                        newAssociationsGuid.Add(assocGuid);
                    }

                    foreach (KeyValuePair<PSMAssociation, IEnumerable<ModelIterator.MoveStep>> kvp in psmAssociationsToMove)
                    {
                        foreach (ModelIterator.MoveStep s in kvp.Value)
                        {
                            if (s.StepType == ModelIterator.MoveStep.MoveStepType.None) continue;
                            command.Commands.Add(new acmdReconnectPSMAssociation(Controller, kvp.Key, s.StepTarget) { Propagate = false });
                        }
                    }

                    IEnumerable<Guid> synchroGroup1 = responsibleAssociations.Select<PSMAssociation, Guid>(a => a);
                    IEnumerable<Guid> synchroGroup2 = associations.Where(a => aX2.Contains((PIMAssociation)a.Interpretation)).Select<PSMAssociation, Guid>(a => a).Union(newAssociationsGuid);

                    command.Commands.Add(new acmdSynchroPSMAssociations(Controller) { X1 = synchroGroup1.ToList(), X2 = synchroGroup2.ToList(), Propagate = false });

                    foreach (KeyValuePair<PSMAssociation, IEnumerable<ModelIterator.MoveStep>> kvp in psmAssociationsToMove)
                    {
                        bool first = true;
                        foreach (ModelIterator.MoveStep s in kvp.Value.Reverse<ModelIterator.MoveStep>())
                        {
                            if (first) first = false;
                            else command.Commands.Add(new acmdReconnectPSMAssociation(Controller, kvp.Key, s.StepTarget) { Propagate = false });
                        }
                    }

                }

                //Swap the two lists and do it again
                IEnumerable<PIMAssociation> temp = aX1;
                aX1 = aX2;
                aX2 = temp;
            }

            return command;
        }
    }
}
