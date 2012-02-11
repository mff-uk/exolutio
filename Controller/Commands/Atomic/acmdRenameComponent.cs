using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic
{
    //[PublicCommand("Rename component", PublicCommandAttribute.EPulicCommandCategory.Common_atomic)]
    internal class acmdRenameComponent : AtomicCommand
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

        internal override PropagationMacroCommand PostPropagation()
        {
            PropagationMacroCommand command = new PropagationMacroCommand(Controller) { CheckFirstOnlyInCanExecute = true };
            ExolutioObject component = Project.TranslateComponent(NamedComponentGuid);

            if (component is PIMComponent)
            {
                PIMComponent pimComponent = (PIMComponent)component;
                foreach (PSMComponent psmComponent in pimComponent.GetInterpretedComponents())
                {
                    if (psmComponent.IsNamed && psmComponent.Name == oldname)
                    {
                        command.Commands.Add(new acmdRenameComponent(Controller, psmComponent, NewName));
                    }
                }
            }

            return command;
        }
    }
}
