using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Model.PSM;
using EvoX.Model;

namespace EvoX.Controller.Commands.Atomic.PSM
{
    public class acmdNewPSMDiagram : StackedCommand
    {
        private Guid schemaGuid;

        private Guid diagramGuid;

        public Guid SchemaGuid
        {
            get { return schemaGuid; }
            set
            {
                schemaGuid = value;
            }
        }

        /// <summary>
        /// If set before execution, creates a new diagram with this GUID.
        /// After execution contains GUID of the created diagram.
        /// </summary>
        public Guid DiagramGuid
        {
            get { return diagramGuid; }
            set
            {
                if (!Executed) diagramGuid = value;
                else throw new EvoXCommandException("Cannot set DiagramGuid after command execution.", this);
            }
        }


        public acmdNewPSMDiagram(Controller c)
            : base(c)
        { }

        public override bool CanExecute()
        {
            return true;
        }
        
        internal override void CommandOperation()
        {
            if (DiagramGuid == Guid.Empty) DiagramGuid = Guid.NewGuid();

            PSMSchema psmSchema = Project.TranslateComponent<PSMSchema>(schemaGuid);
            PSMDiagram diagram = new PSMDiagram(Project, DiagramGuid);
            diagram.LoadSchemaToDiagram(psmSchema);
            psmSchema.ProjectVersion.PSMDiagrams.Add(diagram);
            Report = new CommandReport(CommandReports.PSM_diagram_added, psmSchema);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PSMSchema s = Project.TranslateComponent<PSMSchema>(SchemaGuid);
            PSMDiagram psmDiagram = Project.TranslateComponent<PSMDiagram>(diagramGuid);
            s.ProjectVersion.PSMDiagrams.Remove(psmDiagram);
            return OperationResult.OK;
        }
    }
}
