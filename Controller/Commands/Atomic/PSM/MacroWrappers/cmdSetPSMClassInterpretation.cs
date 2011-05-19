using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Controller.Commands.Reflection;
using EvoX.Model.PSM;
using EvoX.Model.PIM;
using EvoX.Model;

namespace EvoX.Controller.Commands.Atomic.PSM.MacroWrappers
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

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdSetPSMClassInterpretation(Controller, PSMComponentGuid, PIMComponentGuid));
        }
    }
}
