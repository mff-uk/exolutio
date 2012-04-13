using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Specialize PSM association (atomic)", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdSpecializePSMAssociation : WrapperCommand
    {
        [PublicArgument("Association", typeof(PSMAssociation))]
        [Scope(ScopeAttribute.EScope.PSMAssociation)]
        public Guid AssociationGuid { get; set; }

        [PublicArgument("Target special PSM class", typeof(PSMClass), ModifiedPropertyName = "Parent")]
        public Guid SpecialClassGuid { get; set; }

        public cmdSpecializePSMAssociation() { }

        public cmdSpecializePSMAssociation(Controller c)
            : base(c) { }

        public void Set(Guid associationGuid, Guid specialPSMClassGuid)
        {
            SpecialClassGuid = specialPSMClassGuid;
            AssociationGuid = associationGuid;
        }

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdSpecializePSMAssociation(Controller, AssociationGuid, SpecialClassGuid));
        }
    }
}
