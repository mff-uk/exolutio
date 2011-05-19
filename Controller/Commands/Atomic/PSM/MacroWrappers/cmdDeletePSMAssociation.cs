using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Model;
using EvoX.Model.PSM;
using EvoX.Controller.Commands.Atomic;
using EvoX.Controller.Commands.Atomic.PSM;

namespace EvoX.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Delete PSM association", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdDeletePSMAssociation : MacroCommand
    {
        [PublicArgument("Deleted PSM association", typeof(PSMAssociation))]
        [Scope(ScopeAttribute.EScope.PSMAssociation)]
        public Guid AssociationGuid { get; set; }

        public cmdDeletePSMAssociation() { }

        public cmdDeletePSMAssociation(Controller c)
            : base(c) { }

        public void Set(Guid associationGuid)
        {
            AssociationGuid = associationGuid;
            
        }

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdDeletePSMAssociation(Controller, AssociationGuid));
        }

    }
}
