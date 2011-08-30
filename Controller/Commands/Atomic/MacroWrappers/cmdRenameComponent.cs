using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Atomic.PSM;

namespace Exolutio.Controller.Commands.Atomic.MacroWrappers
{
    [PublicCommand("Rename component", PublicCommandAttribute.EPulicCommandCategory.Common_atomic)]
    public class cmdRenameComponent : MacroCommand
    {
        [PublicArgument("Component", typeof(Component))]
        [Scope(ScopeAttribute.EScope.PIMAssociation | ScopeAttribute.EScope.PIMClass | ScopeAttribute.EScope.PIMAttribute | ScopeAttribute.EScope.PIMAssociationEnd
             | ScopeAttribute.EScope.PSMAttribute | ScopeAttribute.EScope.PSMClass | ScopeAttribute.EScope.PSMAssociation | ScopeAttribute.EScope.PSMSchemaClass)]
        public Guid ComponentGuid { get; set; }

        [PublicArgument("New name", AllowNullInput = true, ModifiedPropertyName = "Name")]
        public string NewName { get; set; }

        public cmdRenameComponent()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdRenameComponent(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid componentGuid, string newName)
        {
            NewName = newName;
            ComponentGuid = componentGuid;
        }

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdRenameComponent(Controller, ComponentGuid, NewName));
        }
    }
}
