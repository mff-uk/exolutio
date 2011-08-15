using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM
{
    internal class acmdDeletePSMDiagram : StackedCommand
    {
        private Guid schemaGuid;

        public Guid SchemaGuid
        {
            get { return schemaGuid; }
            set { schemaGuid = value; }
        }

        private Guid diagramGuid;

        public Guid DiagramGuid
        {
            get { return diagramGuid; }
            set
            {
                diagramGuid = value;
            }
        }


        public acmdDeletePSMDiagram(Controller c)
            : base(c)
        { }

        public override bool CanExecute()
        {
            return true;
        }
        
        internal override void CommandOperation()
        {
            PSMDiagram psmDiagram = Project.TranslateComponent<PSMDiagram>(DiagramGuid);
            psmDiagram.ProjectVersion.PSMDiagrams.RemoveChecked(psmDiagram);
            Project.mappingDictionary.Remove(DiagramGuid);
            Report = new CommandReport(CommandReports.PSM_diagram_removed, psmDiagram);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PSMSchema psmSchema = Project.TranslateComponent<PSMSchema>(SchemaGuid);
            PSMDiagram psmDiagram = new PSMDiagram(Project, DiagramGuid);
            psmDiagram.Schema = psmSchema;
            psmDiagram.LoadSchemaToDiagram(psmSchema);
            psmSchema.ProjectVersion.PSMDiagrams.Add(psmDiagram);
            return OperationResult.OK;
        }
    }
}
