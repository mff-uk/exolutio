using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Generalize PSM association (atomic)", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdGeneralizePSMAssociation : WrapperCommand
    {
        [PublicArgument("Association", typeof(PSMAssociation))]
        [Scope(ScopeAttribute.EScope.PSMAssociation)]
        public Guid AssociationGuid { get; set; }

        public cmdGeneralizePSMAssociation() { }

        public cmdGeneralizePSMAssociation(Controller c)
            : base(c) { }
        
        public void Set(Guid associationEndGuid)
        {
            AssociationGuid = associationEndGuid;
        }

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdGeneralizePSMAssociation(Controller, AssociationGuid));
        }
    }
}
