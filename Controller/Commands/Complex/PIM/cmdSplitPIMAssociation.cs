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

        public Guid NewAssociationGuid { get; set; }

        [PublicArgument("Diagram", typeof(Diagram), AllowNullInput = true)]
        public Guid DiagramGuid { get; set; }

        public cmdSplitPIMAssociation()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdSplitPIMAssociation(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid psmAssociationGuid, Guid newAssociationGuid)
        {
            PIMAssociationGuid = psmAssociationGuid;
            NewAssociationGuid = newAssociationGuid;
        }

        protected override void GenerateSubCommands()
        {
            if (NewAssociationGuid == Guid.Empty) NewAssociationGuid = Guid.NewGuid();
            Guid NewAssociationEnd1Guid = Guid.NewGuid();
            Guid NewAssociationEnd2Guid = Guid.NewGuid();

            PIMAssociation original = Project.TranslateComponent<PIMAssociation>(PIMAssociationGuid);

            PIMAssociationEnd originalEnd1 = original.PIMAssociationEnds.First();
            PIMAssociationEnd originalEnd2 = original.PIMAssociationEnds.Last();

            Commands.Add(new acmdNewPIMAssociation(Controller, originalEnd1.PIMClass, NewAssociationEnd1Guid, originalEnd2.PIMClass, NewAssociationEnd2Guid, original.PIMSchema) { AssociationGuid = NewAssociationGuid });
            if (DiagramGuid != Guid.Empty) Commands.Add(new acmdAddComponentToDiagram(Controller, NewAssociationGuid, DiagramGuid));
            Commands.Add(new acmdRenameComponent(Controller, NewAssociationGuid, original.Name + "2"));
            Commands.Add(new acmdRenameComponent(Controller, NewAssociationEnd1Guid, originalEnd1.Name));
            Commands.Add(new acmdRenameComponent(Controller, NewAssociationEnd2Guid, originalEnd2.Name));
            Commands.Add(new acmdUpdatePIMAssociationEndCardinality(Controller, NewAssociationEnd1Guid, originalEnd1.Lower, originalEnd1.Upper));
            Commands.Add(new acmdUpdatePIMAssociationEndCardinality(Controller, NewAssociationEnd2Guid, originalEnd2.Lower, originalEnd2.Upper));
            Commands.Add(new acmdSynchroPIMAssociations(Controller) { X1 = Enumerable.Repeat(original.ID, 1).ToList(), X2 = Enumerable.Repeat(NewAssociationGuid, 1).ToList() });
        }

        public override bool CanExecute()
        {
            if (PIMAssociationGuid == null) return false;
            return base.CanExecute();
        }

        internal override void CommandOperation()
        {
            base.CommandOperation();
            Report = new CommandReport(CommandReports.COMPLEX_SPLIT_PSM_ASSOCIATION);
        }

    }
}
