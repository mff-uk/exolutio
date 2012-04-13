using System;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Controller.Commands.Atomic;
using Exolutio.Controller.Commands.Atomic.PSM;
using Exolutio.Model.PSM;

namespace Exolutio.Controller.Commands.Complex.PIM
{
    [PublicCommand("Derive new PSM root (complex)", PublicCommandAttribute.EPulicCommandCategory.PIM_complex)]
    public class cmdDerivePSMRoot : ComposedCommand
    {
        [PublicArgument("PSM schema", typeof(PSMSchema), AllowNullInput = true)]
        public Guid SchemaGuid { get; set; }

        [PublicArgument("PIM Class", typeof(PIMClass))]
        [Scope(ScopeAttribute.EScope.PIMClass)]
        public Guid PIMClassGuid { get; set; }

        [GeneratedIDArgument("PSMSchemaClassGuid", typeof(PSMSchemaClass))]
        public Guid PSMSchemaClassGuid { get; set; }

        [GeneratedIDArgument("RootGuid", typeof(PSMAssociationMember))]
        public Guid RootGuid { get; set; }

        [GeneratedIDArgument("AssociationGuid", typeof(PIMAssociation))]
        public Guid AssociationGuid { get; set; }

        public cmdDerivePSMRoot()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdDerivePSMRoot(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid pimClassGuid, Guid schemaGuid)
        {
            PIMClassGuid = pimClassGuid;
            SchemaGuid = schemaGuid;
            
        }

        internal override void GenerateSubCommands()
        {
            if (RootGuid == Guid.Empty) RootGuid = Guid.NewGuid();
            if (AssociationGuid == Guid.Empty) AssociationGuid = Guid.NewGuid();
            if (SchemaGuid == Guid.Empty)
            {
                if (PSMSchemaClassGuid == Guid.Empty) PSMSchemaClassGuid = Guid.NewGuid();
                if (SchemaGuid == Guid.Empty) SchemaGuid = Guid.NewGuid();
                Commands.Add(new acmdNewPSMSchema(Controller) { SchemaGuid = SchemaGuid, SchemaClassGuid = PSMSchemaClassGuid });
                Commands.Add(new acmdNewPSMDiagram(Controller) { SchemaGuid = SchemaGuid });
            }
            else
            {
                PSMSchemaClassGuid = Project.TranslateComponent<PSMSchema>(SchemaGuid).PSMSchemaClass;
            }
            PIMClass source = Project.TranslateComponent<PIMClass>(PIMClassGuid);
            Commands.Add(new acmdNewPSMClass(Controller, SchemaGuid) { ClassGuid = RootGuid });
            Commands.Add(new acmdRenameComponent(Controller, RootGuid, source.Name));
            Commands.Add(new acmdSetPSMClassInterpretation(Controller, RootGuid, PIMClassGuid));
            Commands.Add(new acmdNewPSMAssociation(Controller, PSMSchemaClassGuid, RootGuid, SchemaGuid) { AssociationGuid = AssociationGuid });
            Commands.Add(new acmdRenameComponent(Controller, AssociationGuid, source.Name));
        }

        public override bool CanExecute()
        {
            return base.CanExecute();
        }

        internal override void CommandOperation()
        {
            base.CommandOperation();
            Report = new CommandReport(CommandReports.COMPLEX_DERIVE_NEW_ROOT);
        }

    }
}
