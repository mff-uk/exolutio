using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Reconnect parent PSM association end (atomic)", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
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
