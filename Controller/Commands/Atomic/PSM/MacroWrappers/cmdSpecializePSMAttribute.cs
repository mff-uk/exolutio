using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Specialize PSM attribute (atomic)", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdSpecializePSMAttribute : WrapperCommand
    {
        [PublicArgument("Attribute", typeof(PSMAttribute))]
        [Scope(ScopeAttribute.EScope.PSMAttribute)]
        public Guid AttributeGuid { get; set; }

        [PublicArgument("Target special PSM class", typeof(PSMClass), ModifiedPropertyName = "PSMClass")]
        public Guid SpecialClassGuid { get; set; }

        public cmdSpecializePSMAttribute() { }

        public cmdSpecializePSMAttribute(Controller c)
            : base(c) { }
        
        public void Set(Guid psmAttributeGuid, Guid specialPSMClassGuid)
        {
            SpecialClassGuid = specialPSMClassGuid;
            AttributeGuid = psmAttributeGuid;
        }

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdSpecializePSMAttribute(Controller, AttributeGuid, SpecialClassGuid));
        }
    }
}
