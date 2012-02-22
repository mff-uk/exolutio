using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Controller.Commands.Atomic;
using Exolutio.Controller.Commands.Atomic.PIM;

namespace Exolutio.Controller.Commands.Complex.PIM
{
    [PublicCommand("Create new PIM association (complex)", PublicCommandAttribute.EPulicCommandCategory.PIM_complex)]
    public class cmdCreateNewPIMAssociation : ComposedCommand, ICommandWithDiagramParameter
    {
        public Guid SchemaGuid { get; set; }

        public Guid AssociationGuid { get; set; }
        
        [Scope(ScopeAttribute.EScope.PIMDiagram)]
        [PublicArgument("Diagram", typeof(Diagram), AllowNullInput = true)]
        public Guid DiagramGuid { get; set; }

        [PublicArgument("Name", SuggestedValue = "newAssociation")]
        public string Name { get; set; }

        [PublicArgument("PIMClass1", typeof(PIMClass))]
        public Guid PIMClassGuid1 { get; set; }

        [PublicArgument("Lower1", SuggestedValue = 1)]
        public uint Lower1 { get; set; }
        
        [PublicArgument("Upper1", SuggestedValue = 1)]
        public UnlimitedInt Upper1 { get; set; }

        [PublicArgument("Role1", SuggestedValue = "newRole")]
        public string Role1 { get; set; }

        [PublicArgument("PIMClass2", typeof(PIMClass))]
        public Guid PIMClassGuid2 { get; set; }

        [PublicArgument("Lower2", SuggestedValue = 1)]
        public uint Lower2 { get; set; }

        [PublicArgument("Upper2", SuggestedValue = 1)]
        public UnlimitedInt Upper2 { get; set; }

        [PublicArgument("Role2", SuggestedValue = "newRole")]
        public string Role2 { get; set; }

        public cmdCreateNewPIMAssociation()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdCreateNewPIMAssociation(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(string name, Guid pimClassGuid1, uint lower1, UnlimitedInt upper1, string role1, Guid pimClassGuid2, uint lower2, UnlimitedInt upper2, string role2)
        {
            Name = name;
            PIMClassGuid1 = pimClassGuid1;
            Lower1 = lower1;
            Upper1 = upper1;
            Role1 = role1;
            PIMClassGuid2 = pimClassGuid2;
            Lower2 = lower2;
            Upper2 = upper2;
            Role2 = role2;
            
        }

        internal override void GenerateSubCommands()
        {
            if (AssociationGuid == Guid.Empty) AssociationGuid = Guid.NewGuid();
            Guid AssociationEnd1Guid = Guid.NewGuid();
            Guid AssociationEnd2Guid = Guid.NewGuid();
            
            Commands.Add(new acmdNewPIMAssociation(Controller, PIMClassGuid1, AssociationEnd1Guid, PIMClassGuid2, AssociationEnd2Guid, SchemaGuid) { AssociationGuid = AssociationGuid, Propagate = false });
            Commands.Add(new acmdRenameComponent(Controller, AssociationGuid, Name) { Propagate = false });

            Commands.Add(new acmdRenameComponent(Controller, AssociationEnd1Guid, Role1) { Propagate = false });
            Commands.Add(new acmdUpdatePIMAttributeCardinality(Controller, AssociationEnd1Guid, Lower1, Upper1) { Propagate = false });

            Commands.Add(new acmdRenameComponent(Controller, AssociationEnd2Guid, Role2) { Propagate = false });
            Commands.Add(new acmdUpdatePIMAttributeCardinality(Controller, AssociationEnd2Guid, Lower2, Upper2) { Propagate = false });
            
            if (DiagramGuid != Guid.Empty)
            {
                Commands.Add(new acmdAddComponentToDiagram(Controller, AssociationGuid, DiagramGuid));
            }
        }

        public override bool CanExecute()
        {
            if (PIMClassGuid1 == Guid.Empty || PIMClassGuid2 == Guid.Empty) return false;
            return base.CanExecute();
        }

        internal override void CommandOperation()
        {
            base.CommandOperation();
            Report = new CommandReport(CommandReports.COMPLEX_NEW_PIM_ASSOC);
        }
    }
}
