using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Atomic;
using Exolutio.Controller.Commands.Atomic.PSM;

namespace Exolutio.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Delete PSM attribute", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdDeletePSMAttribute : WrapperCommand
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

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdDeletePSMAttribute(Controller, AttributeGuid));
        }
    }
}
