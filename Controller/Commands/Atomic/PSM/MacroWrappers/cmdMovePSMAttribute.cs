using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Model.PSM;
using EvoX.Model;

namespace EvoX.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Move PSM attribute (no propagation yet)", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdMovePSMAttribute : MacroCommand
    {
        [PublicArgument("Attribute", typeof(PSMAttribute))]
        [Scope(ScopeAttribute.EScope.PSMAttribute)]
        public Guid AttributeGuid { get; set; }

        [PublicArgument("Target PSM class", typeof(PSMClass), ModifiedPropertyName = "PSMClass")]
        public Guid NewClassGuid { get; set; }

        public cmdMovePSMAttribute() { }

        public cmdMovePSMAttribute(Controller c)
            : base(c) { }
        
        public void Set(Guid psmAttributeGuid, Guid psmClassGuid)
        {
            NewClassGuid = psmClassGuid;
            AttributeGuid = psmAttributeGuid;
            
        }

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdMovePSMAttribute(Controller, AttributeGuid, NewClassGuid));
        }
    }
}
