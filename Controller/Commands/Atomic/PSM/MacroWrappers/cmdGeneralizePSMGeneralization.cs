using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Generalize PSM generalization (atomic)", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdGeneralizePSMGeneralization : MacroCommand
    {
        [PublicArgument("Generalization", typeof(PSMGeneralization))]
        [Scope(ScopeAttribute.EScope.PSMGeneralization)]
        public Guid GeneralizationGuid { get; set; }

        public cmdGeneralizePSMGeneralization() { }

        public cmdGeneralizePSMGeneralization(Controller c)
            : base(c) { }
        
        public void Set(Guid psmGeneralizationGuid)
        {
            GeneralizationGuid = psmGeneralizationGuid;
        }

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdGeneralizePSMGeneralization(Controller, GeneralizationGuid));
        }
    }
}
