using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Atomic;
using Exolutio.Controller.Commands.Atomic.PSM;

namespace Exolutio.Controller.Commands.Complex.PSM
{
    [PublicCommand("Update PSM association (complex)", PublicCommandAttribute.EPulicCommandCategory.PSM_complex)]
    public class cmdUpdatePSMAssociation : ComposedCommand
    {
        [PublicArgument("Name", ModifiedPropertyName = "Name")]
        public string Name { get; set; }

        [PublicArgument("Lower", ModifiedPropertyName = "Lower")]
        public uint Lower { get; set; }

        [PublicArgument("Upper", ModifiedPropertyName = "Upper")]
        public UnlimitedInt Upper { get; set; }

        [Scope(ScopeAttribute.EScope.PSMAssociation)]
        [PublicArgument("Updated association", typeof(PSMAssociation))]
        public Guid AssociationGuid { get; set; }

        public Guid InterpretedAssociation { get; set; }

        public cmdUpdatePSMAssociation()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdUpdatePSMAssociation(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid associationGuid, string name, uint lower, UnlimitedInt upper)
        {
            AssociationGuid = associationGuid;
            Name = name;
            Lower = lower;
            Upper = upper;
        }

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdRenameComponent(Controller, AssociationGuid, Name) { Propagate = false });
            Commands.Add(new acmdUpdatePSMAssociationCardinality(Controller, AssociationGuid, Lower, Upper) { Propagate = false });
            if (InterpretedAssociation != Guid.Empty)
            {
                Commands.Add(new acmdSetPSMAssociationInterpretation(Controller, AssociationGuid, InterpretedAssociation) { Propagate = false });
            }
        }

        public override bool CanExecute()
        {
            return base.CanExecute();
        }

        internal override void CommandOperation()
        {
            base.CommandOperation();
            Report = new CommandReport(CommandReports.COMPLEX_UPDATE_PSM_ASSOCIATION);
        }
    }
}
