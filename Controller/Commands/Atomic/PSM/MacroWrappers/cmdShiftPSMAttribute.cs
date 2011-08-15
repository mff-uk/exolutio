using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Shift PSM attribute", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdShiftPSMAttribute : MacroCommand
    {
        [PublicArgument("Attribute", typeof(PSMAttribute))]
        [Scope(ScopeAttribute.EScope.PSMAttribute)]
        public Guid AttributeGuid { get; set; }

        [PublicArgument("Shift up", SuggestedValue = true)]
        public bool Up { get; set; }

        public cmdShiftPSMAttribute() { }

        public cmdShiftPSMAttribute(Controller c)
            : base(c) { }

        public void Set(Guid psmAttributeGuid, bool shiftUp)
        {
            AttributeGuid = psmAttributeGuid;
            Up = shiftUp;
        }

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdShiftPSMAttribute(Controller, AttributeGuid, Up));
        }

    }
}
