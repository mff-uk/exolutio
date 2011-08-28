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
    [PublicCommand("Set PSM association's interpretation", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdSetPSMAssociationInterpretation : MacroCommand
    {
        [PublicArgument("PSM association", typeof(PSMAssociation))]
        [Scope(ScopeAttribute.EScope.PSMAssociation)]
        public Guid PSMComponentGuid { get; set; }

        [PublicArgument("Interpretation", typeof(PIMAssociation), AllowNullInput = true, ModifiedPropertyName = "Interpretation")]
        [ConsistentWith("PSMComponentGuid", InterpretationConsistency.Key)]
        public Guid PIMComponentGuid { get; set; }

        [PublicArgument("Child association end", typeof(PIMAssociationEnd))]
        [ConsistentWith("PIMComponentGuid", PIMAssociationAssociationEndParameterConsistency.Key)]
        public Guid ChildAssociationEnd { get; set; }

        public cmdSetPSMAssociationInterpretation() { }

        public cmdSetPSMAssociationInterpretation(Controller c)
            : base(c) { }

        public void Set(Guid interpretedPSMComponentGuid, Guid childAssociationEnd, Guid pimInterpretationGuid)
        {
            PSMComponentGuid = interpretedPSMComponentGuid;
            PIMComponentGuid = pimInterpretationGuid;
            ChildAssociationEnd = childAssociationEnd;
        }

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdSetPSMAssociationInterpretation(Controller, PSMComponentGuid, ChildAssociationEnd, PIMComponentGuid));
        }
    }
}
