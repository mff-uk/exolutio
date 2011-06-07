using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Atomic;
using Exolutio.Controller.Commands.Atomic.PIM;

namespace Exolutio.Controller.Commands.Complex.PIM
{
    [PublicCommand("Split PIM association (complex)", PublicCommandAttribute.EPulicCommandCategory.PIM_complex)]
    public class cmdSplitPIMAssociation : MacroCommand
    {
        [PublicArgument("PIM association", typeof(PIMAssociation))]
        [Scope(ScopeAttribute.EScope.PIMAssociation)]
        public Guid PIMAssociationGuid { get; set; }

        public IEnumerable<Tuple<Guid, Guid, Guid>> NewGuids { get; set; }

        private uint cnt = 2;
        [PublicArgument("Count")]
        public uint Count { get { return cnt; } set { cnt = value; } }

        public cmdSplitPIMAssociation()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdSplitPIMAssociation(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid pimAssociationGuid, IEnumerable<Tuple<Guid, Guid, Guid>> newGuids)
        {
            PIMAssociationGuid = pimAssociationGuid;
            NewGuids = newGuids;
        }

        public void Set(Guid pimAssociationGuid, uint count)
        {
            PIMAssociationGuid = pimAssociationGuid;
            Count = count;
        }

        protected override void GenerateSubCommands()
        {
            if (NewGuids == null)
            {
                List<Tuple<Guid, Guid, Guid>> guids = new List<Tuple<Guid, Guid, Guid>>();
                for (int i = 0; i < Count; i++)
                {
                    guids.Add(new Tuple<Guid, Guid, Guid>(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid()));
                }
                NewGuids = guids;
            }

            PIMAssociation original = Project.TranslateComponent<PIMAssociation>(PIMAssociationGuid);
            PIMAssociationEnd originalEnd1 = original.PIMAssociationEnds.First();
            PIMAssociationEnd originalEnd2 = original.PIMAssociationEnds.Last();

            int counter = 0;
            foreach (Tuple<Guid, Guid, Guid> t in NewGuids)
            {
                counter++;
                Guid newAssociationGuid = t.Item1;
                Guid newAssociationEnd1Guid = t.Item2;
                Guid newAssociationEnd2Guid = t.Item2;

                Commands.Add(new acmdNewPIMAssociation(Controller, originalEnd1.PIMClass, newAssociationEnd1Guid, originalEnd2.PIMClass, newAssociationEnd2Guid, original.PIMSchema) { AssociationGuid = newAssociationGuid });
                Commands.Add(new acmdRenameComponent(Controller, newAssociationGuid, original.Name + counter));
                Commands.Add(new acmdRenameComponent(Controller, newAssociationEnd1Guid, originalEnd1.Name));
                Commands.Add(new acmdRenameComponent(Controller, newAssociationEnd2Guid, originalEnd2.Name));
                Commands.Add(new acmdUpdatePIMAssociationEndCardinality(Controller, newAssociationEnd1Guid, originalEnd1.Lower, originalEnd1.Upper));
                Commands.Add(new acmdUpdatePIMAssociationEndCardinality(Controller, newAssociationEnd2Guid, originalEnd2.Lower, originalEnd2.Upper));

                foreach (PIMDiagram d in Project.SingleVersion.PIMDiagrams)
                {
                    if (d.PIMComponents.Contains(original))
                    {
                        Commands.Add(new acmdAddComponentToDiagram(Controller, newAssociationGuid, d));
                    }
                }
            }
            Commands.Add(new acmdSynchroPIMAssociations(Controller) { X1 = Enumerable.Repeat(original.ID, 1).ToList(), X2 = NewGuids.Select(t => t.Item1).ToList() });
            Commands.Add(new cmdDeletePIMAssociation(Controller) { AssociationGuid = original });
        }

        public override bool CanExecute()
        {
            if (PIMAssociationGuid == null) return false;
            if (Count < 2 && NewGuids == null) return false;
            return base.CanExecute();
        }

        internal override void CommandOperation()
        {
            base.CommandOperation();
            Report = new CommandReport(CommandReports.COMPLEX_SPLIT_PSM_ASSOCIATION);
        }

    }
}
