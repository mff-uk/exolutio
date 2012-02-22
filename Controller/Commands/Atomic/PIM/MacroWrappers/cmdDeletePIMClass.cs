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
    [Obsolete("Atomic operation wrapper deprecated by an appropriate complex operation")]
    //[PublicCommand("Delete PIM class", PublicCommandAttribute.EPulicCommandCategory.PIM_atomic)]
    public class cmdDeletePIMClass : MacroCommand
    {
        [PublicArgument("Deleted class", typeof(PIMClass))]
        [Scope(ScopeAttribute.EScope.PIMClass)]
        public Guid ClassGuid { get; set; }
        
        public cmdDeletePIMClass() {}

        public cmdDeletePIMClass(Controller c)
            : base(c) {}

        public void Set(Guid classGuid)
        {
            ClassGuid = classGuid;
        }

        internal override void GenerateSubCommands()
        {
            Commands.AddRange(acmdRemoveComponentFromDiagram.CreateCommandsToRemoveFromAllDiagrams(Controller, ClassGuid));
            Commands.Add(new acmdDeletePIMClass(Controller, ClassGuid));
        }        
    }
}
