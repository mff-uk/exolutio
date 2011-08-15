using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Controller.Commands.Atomic;
using Exolutio.Controller.Commands.Atomic.PIM;

namespace Exolutio.Controller.Commands.Atomic.PIM.MacroWrappers
{
    [Obsolete("Atomic operation wrapper deprecated by an appropriate complex operation")]
    //[PublicCommand("Delete PIM attribute", PublicCommandAttribute.EPulicCommandCategory.PIM_atomic)]
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
