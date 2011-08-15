using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Controller.Commands.Atomic;
using Exolutio.Controller.Commands.Atomic.PIM;

namespace Exolutio.Controller.Commands.Atomic.PIM.MacroWrappers
{
    [PublicCommand("Delete PIM generalization", PublicCommandAttribute.EPulicCommandCategory.PIM_atomic)]
    public class cmdDeletePIMGeneralization : MacroCommand
    {
        [PublicArgument("Deleted generalization", typeof(PIMGeneralization))]
        [Scope(ScopeAttribute.EScope.PIMGeneralization)]
        public Guid GeneralizationGuid { get; set; }

        public cmdDeletePIMGeneralization() { }

        public cmdDeletePIMGeneralization(Controller c)
            : base(c) { }

        public void Set(Guid generalizationGuid)
        {
            GeneralizationGuid = generalizationGuid;

        }

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdDeletePIMGeneralization(Controller, GeneralizationGuid));
        }
    }
}
