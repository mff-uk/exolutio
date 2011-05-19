using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Model.PSM;
using EvoX.Model;

namespace EvoX.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Delete PSM schema", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdDeletePSMSchema : MacroCommand
    {
        [PublicArgument("PSMSchema", typeof(PSMSchema))]
        [Scope(ScopeAttribute.EScope.PSMSchema)]
        public Guid SchemaGuid { get; set; }

        public cmdDeletePSMSchema() { }

        public cmdDeletePSMSchema(Controller c)
            : base(c) { }

        public void Set(Guid psmSchemaGuid)
        {
            SchemaGuid = psmSchemaGuid;
            
        }

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdDeletePSMSchema(Controller, SchemaGuid));
        }
    }
}
