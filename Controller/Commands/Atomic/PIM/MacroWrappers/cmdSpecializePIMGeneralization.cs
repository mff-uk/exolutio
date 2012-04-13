using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PIM.MacroWrappers
{
    [PublicCommand("Specialize PIM generalization (atomic)", PublicCommandAttribute.EPulicCommandCategory.PIM_atomic)]
    public class cmdSpecializePIMGeneralization : WrapperCommand
    {
        [PublicArgument("Generalization", typeof(PIMGeneralization))]
        [Scope(ScopeAttribute.EScope.PIMGeneralization)]
        public Guid GeneralizationGuid { get; set; }

        [PublicArgument("Target special PIM class", typeof(PIMClass), ModifiedPropertyName = "PIMClass")]
        public Guid SpecialClassGuid { get; set; }

        public cmdSpecializePIMGeneralization() { }

        public cmdSpecializePIMGeneralization(Controller c)
            : base(c) { }

        public void Set(Guid pimGeneralizationGuid, Guid specialPIMClassGuid)
        {
            SpecialClassGuid = specialPIMClassGuid;
            GeneralizationGuid = pimGeneralizationGuid;
        }

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdSpecializePIMGeneralization(Controller, GeneralizationGuid, SpecialClassGuid));
        }
    }
}
