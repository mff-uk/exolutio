using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Controller.Commands.Atomic;
using Exolutio.Controller.Commands.Atomic.PIM;
using Exolutio.Model.PSM;

namespace Exolutio.Controller.Commands.Atomic.PIM.MacroWrappers
{
    [PublicCommand("Create new PIM association", PublicCommandAttribute.EPulicCommandCategory.PIM_atomic)]
    public class cmdNewPIMAssociation : WrapperCommand, ICommandWithDiagramParameter
    {
        [GeneratedIDArgument("AssociationGuid", typeof(PIMAssociation))]
        public Guid AssociationGuid { get; set; }

        [GeneratedIDArgument("AssociationEnd1Guid", typeof(PIMAssociationEnd))]
        public Guid AssociationEnd1Guid { get; set; }

        [GeneratedIDArgument("AssociationEnd2Guid", typeof(PIMAssociationEnd))]
        public Guid AssociationEnd2Guid { get; set; }

        [PublicArgument("Schema", typeof(Schema), CreateControlInEditors = false, AllowNullInput = true)]
        public Guid SchemaGuid { get; set; }
        
        [Scope(ScopeAttribute.EScope.PIMDiagram)]
        [PublicArgument("Diagram", typeof(Diagram), AllowNullInput = true)]
        public Guid DiagramGuid { get; set; }

        [PublicArgument("PIM Class 1", typeof(PIMClass))]
        public Guid PIMClassGuid1 { get; set; }

        [PublicArgument("PIM Class 2", typeof(PIMClass))]
        public Guid PIMClassGuid2 { get; set; }

        public cmdNewPIMAssociation() 
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdNewPIMAssociation(Controller c)
            : base(c) 
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid pimClassGuid1, Guid pimClassGuid2, Guid schemaGuid)
        {
            PIMClassGuid1 = pimClassGuid1;
            PIMClassGuid2 = pimClassGuid2;
            SchemaGuid = schemaGuid;
        }

        internal override void GenerateSubCommands()
        {
            if (AssociationGuid == Guid.Empty) AssociationGuid = Guid.NewGuid();
            if (AssociationEnd1Guid == Guid.Empty) AssociationEnd1Guid = Guid.NewGuid();
            if (AssociationEnd2Guid == Guid.Empty) AssociationEnd2Guid = Guid.NewGuid();    
            Commands.Add(new acmdNewPIMAssociation(Controller, PIMClassGuid1, AssociationEnd1Guid, PIMClassGuid2, AssociationEnd2Guid, SchemaGuid) { AssociationGuid = AssociationGuid, Propagate = false });
            if (DiagramGuid != Guid.Empty)
            {
                Commands.Add(new acmdAddComponentToDiagram(Controller, AssociationGuid, DiagramGuid));
            }
            acmdRenameComponent r1 = new acmdRenameComponent(Controller, AssociationEnd1Guid, null);
            acmdRenameComponent r2 = new acmdRenameComponent(Controller, AssociationEnd2Guid, null);
            Commands.Add(r1);
            Commands.Add(r2);
        }

    }
}
