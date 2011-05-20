using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Model;
using EvoX.Model.PSM;
using EvoX.Controller.Commands.Atomic;
using EvoX.Controller.Commands.Atomic.PSM;

namespace EvoX.Controller.Commands.Complex.PSM
{
    [PublicCommand("Delete PSM schema (complex)", PublicCommandAttribute.EPulicCommandCategory.PSM_complex)]
    public class cmdDeletePSMSchema : MacroCommand
    {
        [PublicArgument("Deleted PSM schema", typeof(PSMSchema))]
        [Scope(ScopeAttribute.EScope.PSMSchema)]
        public Guid SchemaGuid { get; set; }
        
        public cmdDeletePSMSchema()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdDeletePSMSchema(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid schemaGuid)
        {
            SchemaGuid = schemaGuid;
            
        }

        protected override void GenerateSubCommands()
        {
            PSMSchema schema = Project.TranslateComponent<PSMSchema>(SchemaGuid);
            foreach (PSMClass c in schema.PSMClasses)
            {
                foreach (PSMClass r in c.Representants)
                {
                    acmdSetRepresentedClass cr = new acmdSetRepresentedClass(Controller, r, Guid.Empty);
                    Commands.Add(cr);
                }
                cmdDeletePSMClassAndParent dc = new cmdDeletePSMClassAndParent(Controller);
                dc.Set(c);
                Commands.Add(dc);
            }
            foreach (PSMContentModel c in schema.PSMContentModels)
            {
                acmdDeletePSMContentModel dc = new acmdDeletePSMContentModel(Controller, c);
                Commands.Add(dc);
            }

            PSMDiagram psmDiagram = schema.ProjectVersion.PSMDiagrams.FirstOrDefault(d => d.PSMSchema == schema);
            if (psmDiagram != null)
            {
                Commands.Add(new acmdDeletePSMDiagram(Controller) { DiagramGuid = psmDiagram.ID, SchemaGuid = SchemaGuid });
            }

            Commands.Add(new acmdDeletePSMSchema(Controller, schema));
        }

        public override bool CanExecute()
        {
            if (SchemaGuid == Guid.Empty) return false;
            return base.CanExecute();
        }

        internal override void CommandOperation()
        {
            base.CommandOperation();
            Report = new CommandReport(CommandReports.COMPLEX_DELETE_PSM_SCHEMA);
        }
    }
}
