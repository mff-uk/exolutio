using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PIM.MacroWrappers
{
    [PublicCommand("Specialize PIM association end (atomic)", PublicCommandAttribute.EPulicCommandCategory.PIM_atomic)]
    public class cmdSpecializePIMAssociationEnd : WrapperCommand
    {
        [PublicArgument("Association End", typeof(PIMAssociationEnd))]
        [Scope(ScopeAttribute.EScope.PIMAssociationEnd)]
        public Guid AssociationEndGuid { get; set; }

        [PublicArgument("Target special PIM class", typeof(PIMClass), ModifiedPropertyName = "PIMClass")]
        public Guid SpecialClassGuid { get; set; }

        public cmdSpecializePIMAssociationEnd() { }

        public cmdSpecializePIMAssociationEnd(Controller c)
            : base(c) { }

        public void Set(Guid associationEndGuid, Guid specialPIMClassGuid)
        {
            SpecialClassGuid = specialPIMClassGuid;
            AssociationEndGuid = associationEndGuid;
        }

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdSpecializePIMAssociationEnd(Controller, AssociationEndGuid, SpecialClassGuid));
        }
    }
}
