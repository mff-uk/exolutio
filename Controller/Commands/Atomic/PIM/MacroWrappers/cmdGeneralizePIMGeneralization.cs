using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PIM.MacroWrappers
{
    [PublicCommand("Generalize PIM generalization (atomic)", PublicCommandAttribute.EPulicCommandCategory.PIM_atomic)]
    public class cmdGeneralizePIMGeneralization : WrapperCommand
    {
        [PublicArgument("Generalization", typeof(PIMGeneralization))]
        [Scope(ScopeAttribute.EScope.PIMGeneralization)]
        public Guid GeneralizationGuid { get; set; }

        public cmdGeneralizePIMGeneralization() { }

        public cmdGeneralizePIMGeneralization(Controller c)
            : base(c) { }
        
        public void Set(Guid pimGeneralizationGuid)
        {
            GeneralizationGuid = pimGeneralizationGuid;
        }

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdGeneralizePIMGeneralization(Controller, GeneralizationGuid));
        }
    }
}
