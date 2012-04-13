using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Generalize PSM attribute (atomic)", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdGeneralizePSMAttribute : WrapperCommand
    {
        [PublicArgument("Attribute", typeof(PSMAttribute))]
        [Scope(ScopeAttribute.EScope.PSMAttribute)]
        public Guid AttributeGuid { get; set; }

        public cmdGeneralizePSMAttribute() { }

        public cmdGeneralizePSMAttribute(Controller c)
            : base(c) { }
        
        public void Set(Guid psmAttributeGuid)
        {
            AttributeGuid = psmAttributeGuid;
        }

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdGeneralizePSMAttribute(Controller, AttributeGuid));
        }
    }
}
