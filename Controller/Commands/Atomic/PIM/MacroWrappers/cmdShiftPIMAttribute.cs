using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PIM.MacroWrappers
{
    [PublicCommand("Shift PIM attribute", PublicCommandAttribute.EPulicCommandCategory.PIM_atomic)]
    public class cmdShiftPIMAttribute : MacroCommand
    {
        [PublicArgument("Attribute", typeof(PIMAttribute))]
        [Scope(ScopeAttribute.EScope.PIMAttribute)]
        public Guid AttributeGuid { get; set; }

        [PublicArgument("Shift up", SuggestedValue = true)]
        public bool Up { get; set; }

        public cmdShiftPIMAttribute() { }

        public cmdShiftPIMAttribute(Controller c)
            : base(c) { }

        public void Set(Guid psmAttributeGuid, bool shiftUp)
        {
            AttributeGuid = psmAttributeGuid;
            Up = shiftUp;
        }

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdShiftPIMAttribute(Controller, AttributeGuid, Up));
        }

    }
}
