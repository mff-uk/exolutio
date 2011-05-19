using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Model;
using EvoX.Model.PSM;
using EvoX.Controller.Commands.Atomic;
using EvoX.Controller.Commands.Atomic.PSM;

namespace EvoX.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Delete PSM attribute", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdDeletePSMAttribute : MacroCommand
    {
        [PublicArgument("Deleted PSM attribute", typeof(PSMAttribute))]
        [Scope(ScopeAttribute.EScope.PSMAttribute)]
        public Guid AttributeGuid { get; set; }

        public cmdDeletePSMAttribute() { }

        public cmdDeletePSMAttribute(Controller c)
            : base(c) { }

        public void Set(Guid attributeGuid)
        {
            AttributeGuid = attributeGuid;
            
        }

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdDeletePSMAttribute(Controller, AttributeGuid));
        }
    }
}
