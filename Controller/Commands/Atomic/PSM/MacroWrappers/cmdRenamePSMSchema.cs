using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Atomic.PSM;

namespace Exolutio.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Rename PSM schema", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdRenamePSMSchema : MacroCommand
    {
        [PublicArgument("Schema", typeof(PSMSchema))]
        [Scope(ScopeAttribute.EScope.PSMSchema)]
        public Guid SchemaGuid { get; set; }

        [PublicArgument("New name", ModifiedPropertyName = "Name")]
        public string NewName { get; set; }

        public cmdRenamePSMSchema()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdRenamePSMSchema(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid psmSchemaGuid, string newName)
        {
            SchemaGuid = psmSchemaGuid;
            NewName = newName;
        }

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdRenamePSMSchema(Controller, SchemaGuid, NewName));
        }
    }
}
