using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic
{
    internal class acmdRemoveComponentFromDiagram : StackedCommand
    {
        [PublicArgument("Component", typeof(Component))]
        public Guid ComponentGuid { get; set; }

        [PublicArgument("Diagram")] 
        public Guid DiagramGuid { get; set; }
        
        public acmdRemoveComponentFromDiagram()
        {
            
        }

        public acmdRemoveComponentFromDiagram(Controller c, Guid componentGuid, Guid diagramGuid)
            : base(c)
        {
            ComponentGuid = componentGuid;
            DiagramGuid = diagramGuid;
        }

        public override bool CanExecute()
        {
            Component component = Project.TranslateComponent<Component>(ComponentGuid);
            Diagram diagram = Project.TranslateComponent<Diagram>(DiagramGuid);

            return diagram.Components.Contains(component) && diagram.Schema == component.Schema;
        }
        
        internal override void CommandOperation()
        {
            Component component = Project.TranslateComponent<Component>(ComponentGuid);
            Diagram diagram = Project.TranslateComponent<Diagram>(DiagramGuid);
            diagram.Components.Remove(component);
            Report = new CommandReport(CommandReports.COMPONENT_REMOVED_FROM_DIAGRAM, component, diagram);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            Component component = Project.TranslateComponent<Component>(ComponentGuid);
            Diagram diagram = Project.TranslateComponent<Diagram>(DiagramGuid);
            diagram.Components.Add(component);
            return OperationResult.OK;
        }

        public static IEnumerable<CommandBase> CreateCommandsToRemoveFromAllDiagrams(Controller controller, Guid deletedComponentGuid)
        {
            Component deletedComponent = (Component) controller.Project.TranslateComponent(deletedComponentGuid);
            List<CommandBase> result = new List<CommandBase>();
            foreach (Diagram diagram in deletedComponent.ProjectVersion.Diagrams)
            {
                if (diagram.Components.Contains(deletedComponent))
                {
                    result.Add(new acmdRemoveComponentFromDiagram(controller, deletedComponentGuid, diagram));
                }
            }
            return result;
        }
    }
}