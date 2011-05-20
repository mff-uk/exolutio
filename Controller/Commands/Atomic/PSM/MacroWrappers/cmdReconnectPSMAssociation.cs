using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Model.PSM;
using EvoX.Model;

namespace EvoX.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Reconnect parent PSM association end (no propagation yet)", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdReconnectPSMAssociation : MacroCommand
    {
        [PublicArgument("Association", typeof(PSMAssociation))]
        [Scope(ScopeAttribute.EScope.PSMAssociation)]
        public Guid AssociationGuid { get; set; }

        [PublicArgument("New parent (PSM Association Member)", typeof(PSMAssociationMember))]
        public Guid NewParentGuid { get; set; }

        public cmdReconnectPSMAssociation() { }        
        
        public cmdReconnectPSMAssociation(Controller c)
            : base(c) { }

        public void Set(Guid psmAssociationGuid, Guid parentGuid)
        {
            NewParentGuid = parentGuid;
            AssociationGuid = psmAssociationGuid;
            
        }

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdReconnectPSMAssociation(Controller, AssociationGuid, NewParentGuid));
        }
    }
}
