using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Shift PSM association", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdShiftPSMAssociation : MacroCommand
    {
        [PublicArgument("Association", typeof(PSMAssociation))]
        [Scope(ScopeAttribute.EScope.PSMAssociation)]
        public Guid AssociationGuid { get; set; }

        [PublicArgument("Shift left", SuggestedValue = true)]
        public bool Left { get; set; }

        public cmdShiftPSMAssociation() { }
        
        public cmdShiftPSMAssociation(Controller c)
            : base(c) { }

        public void Set(Guid psmAssociationGuid, bool shiftLeft)
        {
            AssociationGuid = psmAssociationGuid;
            Left = shiftLeft;
            
        }

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdShiftPSMAssociation(Controller, AssociationGuid, Left));
        }

    }
}
