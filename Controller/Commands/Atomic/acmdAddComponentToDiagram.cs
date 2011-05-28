using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic
{
    public class acmdAddComponentToDiagram : StackedCommand
    {
        [PublicArgument("Component", typeof(Component))]
        public Guid ComponentGuid { get; set; }

        [PublicArgument("Diagram")] 
        public Guid DiagramGuid { get; set; }
        
        public acmdAddComponentToDiagram()
        {
            
        }

        public acmdAddComponentToDiagram(Controller c, Guid componentGuid, Guid diagramGuid)
            : base(c)
        {
            ComponentGuid = componentGuid;
            DiagramGuid = diagramGuid;
        }

        public override bool CanExecute()
        {
            Component component = Project.TranslateComponent<Component>(ComponentGuid);
            Diagram diagram = Project.TranslateComponent<Diagram>(DiagramGuid);

            return !diagram.Components.Contains(component) && diagram.Schema == component.Schema;
        }
        
        internal override void CommandOperation()
        {
            Component component = Project.TranslateComponent<Component>(ComponentGuid);
            Diagram diagram = Project.TranslateComponent<Diagram>(DiagramGuid);
            diagram.Components.Add(component);
            Report = new CommandReport(CommandReports.COMPONENT_ADDED_TO_DIAGRAM, component, diagram);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            Component component = Project.TranslateComponent<Component>(ComponentGuid);
            Diagram diagram = Project.TranslateComponent<Diagram>(DiagramGuid);
            diagram.Components.Remove(component);
            return OperationResult.OK;
        }
    }
}
