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
    [PublicCommand("Delete PSM class", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdDeletePSMClass : MacroCommand
    {
        [PublicArgument("PSMClass", typeof(PSMClass))]
        [Scope(ScopeAttribute.EScope.PSMClass)]
        public Guid ClassGuid { get; set; }

        public cmdDeletePSMClass() { }

        public cmdDeletePSMClass(Controller c)
            : base(c) { }

        public void Set(Guid classGuid)
        {
            ClassGuid = classGuid;
            
        }

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdDeletePSMClass(Controller, ClassGuid));
        }
    }
}
