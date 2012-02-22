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
    [PublicCommand("Set PSM classes interpretation", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdSetPSMClassInterpretation : MacroCommand
    {
        [PublicArgument("PSM component", typeof(PSMClass))]
        [Scope(ScopeAttribute.EScope.PSMClass)]
        public Guid PSMComponentGuid { get; set; }

        [PublicArgument("Interpretation", typeof(PIMClass), AllowNullInput = true, ModifiedPropertyName = "Interpretation")]
        [ConsistentWith("PSMComponentGuid", InterpretationConsistency.Key)]
        public Guid PIMComponentGuid { get; set; }

        public cmdSetPSMClassInterpretation() { }

        public cmdSetPSMClassInterpretation(Controller c)
            : base(c) { }

        public void Set(Guid interpretedPSMComponentGuid, Guid pimInterpretationGuid)
        {
            PSMComponentGuid = interpretedPSMComponentGuid;
            PIMComponentGuid = pimInterpretationGuid;
            
        }

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdSetPSMClassInterpretation(Controller, PSMComponentGuid, PIMComponentGuid));
        }
    }
}
