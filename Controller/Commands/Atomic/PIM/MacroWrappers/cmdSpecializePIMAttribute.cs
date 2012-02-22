using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PIM.MacroWrappers
{
    [PublicCommand("Specialize PIM attribute (atomic)", PublicCommandAttribute.EPulicCommandCategory.PIM_atomic)]
    public class cmdSpecializePIMAttribute : MacroCommand
    {
        [PublicArgument("Attribute", typeof(PIMAttribute))]
        [Scope(ScopeAttribute.EScope.PIMAttribute)]
        public Guid AttributeGuid { get; set; }

        [PublicArgument("Target special PIM class", typeof(PIMClass), ModifiedPropertyName = "PIMClass")]
        public Guid SpecialClassGuid { get; set; }

        public cmdSpecializePIMAttribute() { }

        public cmdSpecializePIMAttribute(Controller c)
            : base(c) { }

        public void Set(Guid psmAttributeGuid, Guid specialPIMClassGuid)
        {
            SpecialClassGuid = specialPIMClassGuid;
            AttributeGuid = psmAttributeGuid;
        }

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdSpecializePIMAttribute(Controller, AttributeGuid, SpecialClassGuid));
        }
    }
}
