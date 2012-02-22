using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Move PSM attribute (atomic)", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
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

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdMovePSMAttribute(Controller, AttributeGuid, NewClassGuid));
        }
    }
}
