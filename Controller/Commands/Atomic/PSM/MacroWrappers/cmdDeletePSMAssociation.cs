using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Atomic;
using Exolutio.Controller.Commands.Atomic.PSM;

namespace Exolutio.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Delete PSM association", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdDeletePSMAssociation : WrapperCommand
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

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdDeletePSMAssociation(Controller, AssociationGuid));
        }

    }
}
