using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Model;
using EvoX.Model.PIM;
using EvoX.Controller.Commands.Atomic;
using EvoX.Controller.Commands.Atomic.PIM;

namespace EvoX.Controller.Commands.Atomic.PIM.MacroWrappers
{
    [PublicCommand("Delete PIM attribute", PublicCommandAttribute.EPulicCommandCategory.PIM_atomic)]
    public class cmdDeletePIMAttribute : MacroCommand
    {
        [PublicArgument("Deleted attribute", typeof(PIMAttribute))]
        [Scope(ScopeAttribute.EScope.PIMAttribute)]
        public Guid AttributeGuid { get; set; }

        public cmdDeletePIMAttribute() { }

        public cmdDeletePIMAttribute(Controller c)
            : base(c) { }

        public void Set(Guid attributeGuid)
        {
            AttributeGuid = attributeGuid;
            
        }

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdDeletePIMAttribute(Controller, AttributeGuid));
        }
    }
}
