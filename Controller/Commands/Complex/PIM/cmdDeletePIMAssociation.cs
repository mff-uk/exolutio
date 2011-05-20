using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Model;
using EvoX.Model.PIM;
using EvoX.Controller.Commands.Atomic;
using EvoX.Controller.Commands.Atomic.PIM;

namespace EvoX.Controller.Commands.Complex.PIM
{
    [PublicCommand("Delete PIM association (complex)", PublicCommandAttribute.EPulicCommandCategory.PIM_complex)]
    public class cmdDeletePIMAssociation : MacroCommand
    {
        [PublicArgument("Deleted association", typeof(PIMAssociation))]
        [Scope(ScopeAttribute.EScope.PIMAssociation)]
        public Guid AssociationGuid { get; set; }
        
        public cmdDeletePIMAssociation()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdDeletePIMAssociation(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid associationGuid)
        {
            AssociationGuid = associationGuid;
            
        }

        protected override void GenerateSubCommands()
        {
            PIMAssociation a = Project.TranslateComponent<PIMAssociation>(AssociationGuid);
            foreach (PIMAssociationEnd e in a.PIMAssociationEnds)
            {
                Commands.Add(new acmdRenameComponent(Controller, e, "") { Propagate = false });
                Commands.Add(new acmdUpdatePIMAssociationEndCardinality(Controller, e, 1, 1) { Propagate = false });
            }
            Commands.Add(new acmdRenameComponent(Controller, a, "") { Propagate = false });
            Commands.Add(new acmdDeletePIMAssociation(Controller, a));
        }

        public override bool CanExecute()
        {
            if (AssociationGuid == Guid.Empty) return false;
            return base.CanExecute();
        }

        internal override void CommandOperation()
        {
            base.CommandOperation();
            Report = new CommandReport(CommandReports.COMPLEX_DELETE_PIM_ASSOC);
        }

    }
}
