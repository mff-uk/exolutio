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
    [PublicCommand("Delete PSM generalization", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdDeletePSMGeneralization : WrapperCommand
    {
        [PublicArgument("Deleted generalization", typeof(PSMGeneralization))]
        [Scope(ScopeAttribute.EScope.PSMGeneralization)]
        public Guid GeneralizationGuid { get; set; }

        public cmdDeletePSMGeneralization() { }

        public cmdDeletePSMGeneralization(Controller c)
            : base(c) { }

        public void Set(Guid generalizationGuid)
        {
            GeneralizationGuid = generalizationGuid;

        }

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdDeletePSMGeneralization(Controller, GeneralizationGuid));
        }
    }
}
