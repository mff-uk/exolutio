using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Controller.Commands.Reflection;
using Exolutio.Model.PSM;
using Exolutio.Model.PIM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Set PSM attribute's interpretation", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdSetPSMAttributeInterpretation : WrapperCommand
    {
        [PublicArgument("PSM attribute", typeof(PSMAttribute))]
        [Scope(ScopeAttribute.EScope.PSMAttribute)]
        public Guid PSMComponentGuid { get; set; }

        [PublicArgument("Interpretation", typeof(PIMAttribute), AllowNullInput = true, ModifiedPropertyName = "Interpretation")]
        [ConsistentWith("PSMComponentGuid", InterpretationConsistency.Key)]
        public Guid PIMComponentGuid { get; set; }

        public cmdSetPSMAttributeInterpretation() { }

        public cmdSetPSMAttributeInterpretation(Controller c)
            : base(c) { }

        public void Set(Guid interpretedPSMComponentGuid, Guid pimInterpretationGuid)
        {
            PSMComponentGuid = interpretedPSMComponentGuid;
            PIMComponentGuid = pimInterpretationGuid;
            
        }

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdSetPSMAttributeInterpretation(Controller, PSMComponentGuid, PIMComponentGuid));
        }
    }
}
