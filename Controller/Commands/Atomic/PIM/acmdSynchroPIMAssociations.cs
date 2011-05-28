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

        internal override MacroCommand PrePropagation()
        {
            IEnumerable<PIMAssociation> aX1 = Project.TranslateComponentCollection<PIMAssociation>(X1);
            IEnumerable<PIMAssociation> aX2 = Project.TranslateComponentCollection<PIMAssociation>(X2);
            if (aX1.Count() == 0 || aX2.Count() == 0 || aX1.Union(aX2).Count() == 1) return null;
            
            MacroCommand command = new MacroCommand(Controller) { CheckFirstOnlyInCanExecute = true };
            command.Report = new CommandReport("Pre-propagation (synchronize PIM association sets)");
            PIMClass pimClass1 = aX2.First().PIMClasses.First();
            PIMClass pimClass2 = aX2.First().PIMClasses.Last();

            //Twice... X1 => X2, X2 => X1
            for (int i = 0; i < 2; i++)
            {
                //Selects psmClasses affected by the synchronization (those which have aX1 counterpart present)
                IEnumerable<PSMClass> psmClasses = 
                     pimClass1.GetInterpretedComponents()
                    .Union(pimClass2.GetInterpretedComponents())
                    .Cast<PSMClass>()
                    .Where(c => c.UnInterpretedSubClasses()
                        .SelectMany<PSMClass, PSMAssociation>(cl => cl.ChildPSMAssociations)
                        .Union(c.ChildPSMAssociations)
                        .Union(c.ParentAssociation == null
                                ? Enumerable.Empty<PSMAssociation>()
                                : Enumerable.Repeat(c.ParentAssociation, 1))
                        .Where(a => a.Interpretation != null)
                        .Select(psma => psma.Interpretation as PIMAssociation)
                        .Intersect(aX1)
                        .OrderBy(k => k.ID)
                        .SequenceEqual(aX1.OrderBy(j => j.ID))
                        );

                foreach (PSMClass psmClass in psmClasses)
                {
                    IEnumerable<PSMAssociation> psmAssociationsInSubClasses = psmClass.UnInterpretedSubClasses()
                        .SelectMany<PSMClass, PSMAssociation>(c => c.ChildPSMAssociations);

                    IEnumerable<PSMAssociation> parentEnum = 
                        psmClass.ParentAssociation == null
                        ? Enumerable.Empty<PSMAssociation>() 
                        : Enumerable.Repeat(psmClass.ParentAssociation, 1);
                    
                    IEnumerable<PIMAssociation> interpretations = psmAssociationsInSubClasses
                        .Union(psmClass.ChildPSMAssociations)
                        .Union(parentEnum)
                        .Where(a => a.Interpretation != null)
                        .Select<PSMAssociation, PIMAssociation>(a => a.Interpretation as PIMAssociation);
                    IEnumerable<PIMAssociation> associationsToPropagate = aX2.Where<PIMAssociation>(a => !interpretations.Contains(a));

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

                    IEnumerable<PSMAssociation> psmAssociationsToMove = psmAssociationsInSubClasses.Where(a => aX1.Contains((PIMAssociation)a.Interpretation) || aX2.Contains((PIMAssociation)a.Interpretation));

                    foreach (PSMAssociation a in psmAssociationsToMove)
                    {
                        command.Commands.Add(new cmdReconnectPSMAssociation(Controller) { AssociationGuid = a, NewParentGuid = psmClass, Propagate = false });
                    }

                    IEnumerable<Guid> synchroGroup1 = psmAssociationsInSubClasses.Union(psmClass.ChildPSMAssociations).Union(parentEnum).Where(a => aX1.Contains((PIMAssociation)a.Interpretation)).Select<PSMAssociation, Guid>(a => a);
                    IEnumerable<Guid> synchroGroup2 = psmAssociationsInSubClasses.Union(psmClass.ChildPSMAssociations).Union(parentEnum).Where(a => aX2.Contains((PIMAssociation)a.Interpretation)).Select<PSMAssociation, Guid>(a => a).Union(newAssociationsGuid);

                    command.Commands.Add(new acmdSynchroPSMAssociations(Controller) { X1 = synchroGroup1.ToList(), X2 = synchroGroup2.ToList(), Propagate = false });

                    foreach (PSMAssociation a in psmAssociationsToMove)
                    {
                        command.Commands.Add(new cmdReconnectPSMAssociation(Controller) { AssociationGuid = a, NewParentGuid = a.Parent, Propagate = false });
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
