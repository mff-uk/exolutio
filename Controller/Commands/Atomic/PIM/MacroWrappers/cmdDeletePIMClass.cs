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
    [PublicCommand("Delete PIM class", PublicCommandAttribute.EPulicCommandCategory.PIM_atomic)]
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

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdDeletePIMClass(Controller, ClassGuid));
        }        
    }
}
