using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic
{
    [PublicCommand("Rename component", PublicCommandAttribute.EPulicCommandCategory.Common_atomic)]
    public class acmdRenameComponent : StackedCommand
    {
        [PublicArgument("Component", typeof(Component))]
        [Scope(ScopeAttribute.EScope.PIMAssociation | ScopeAttribute.EScope.PIMClass | ScopeAttribute.EScope.PIMAttribute
             | ScopeAttribute.EScope.PSMAttribute | ScopeAttribute.EScope.PSMClass | ScopeAttribute.EScope.PSMAssociation | ScopeAttribute.EScope.PSMSchemaClass)]
        public Guid NamedComponentGuid { get; set; }

        [PublicArgument("New name", AllowNullInput = true, ModifiedPropertyName = "Name")] //null is allowed to enable removing association name
        public string NewName { get; set; }
        
        private string oldname;

        public acmdRenameComponent()
        {
            
        }

        public acmdRenameComponent(Controller c, Guid componentGuid, string name )
            : base(c)
        {
            NamedComponentGuid = componentGuid;
            NewName = name;
        }

        public override bool CanExecute()
        {
            return NamedComponentGuid != Guid.Empty;
        }
        
        internal override void CommandOperation()
        {
            Component component = Project.TranslateComponent<Component>(NamedComponentGuid);
            oldname = component.Name;
            component.Name = NewName;
            Report = new CommandReport(CommandReports.COMPONENT_RENAMED, component, oldname, NewName);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            Component component = Project.TranslateComponent<Component>(NamedComponentGuid);
            component.Name = oldname;
            return OperationResult.OK;
        }
    }
}
