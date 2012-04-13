using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Specialize PSM generalization (atomic)", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdSpecializePSMGeneralization : WrapperCommand
    {
        [PublicArgument("Generalization", typeof(PSMGeneralization))]
        [Scope(ScopeAttribute.EScope.PSMGeneralization)]
        public Guid GeneralizationGuid { get; set; }

        [PublicArgument("Target special PIM class", typeof(PSMClass), ModifiedPropertyName = "PSMClass")]
        public Guid SpecialClassGuid { get; set; }

        public cmdSpecializePSMGeneralization() { }

        public cmdSpecializePSMGeneralization(Controller c)
            : base(c) { }

        public void Set(Guid psmGeneralizationGuid, Guid specialPSMClassGuid)
        {
            SpecialClassGuid = specialPSMClassGuid;
            GeneralizationGuid = psmGeneralizationGuid;
        }

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdSpecializePSMGeneralization(Controller, GeneralizationGuid, SpecialClassGuid));
        }
    }
}
