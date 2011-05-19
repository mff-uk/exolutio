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
    [PublicCommand("Set PSM association's interpretation", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdSetPSMAssociationInterpretation : MacroCommand
    {
        [PublicArgument("PSM association", typeof(PSMAssociation))]
        [Scope(ScopeAttribute.EScope.PSMAssociation)]
        public Guid PSMComponentGuid { get; set; }

        [PublicArgument("Interpretation", typeof(PIMAssociation), AllowNullInput = true, ModifiedPropertyName = "Interpretation")]
        [ConsistentWith("PSMComponentGuid", InterpretationConsistency.Key)]
        public Guid PIMComponentGuid { get; set; }

        public cmdSetPSMAssociationInterpretation() { }

        public cmdSetPSMAssociationInterpretation(Controller c)
            : base(c) { }

        public void Set(Guid interpretedPSMComponentGuid, Guid pimInterpretationGuid)
        {
            PSMComponentGuid = interpretedPSMComponentGuid;
            PIMComponentGuid = pimInterpretationGuid;
            
        }

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdSetPSMAssociationInterpretation(Controller, PSMComponentGuid, PIMComponentGuid));
        }
    }
}
