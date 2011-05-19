using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Model;
using EvoX.Model.PSM;
using EvoX.Controller.Commands.Atomic;
using EvoX.Controller.Commands.Atomic.PSM;

namespace EvoX.Controller.Commands.Atomic.PSM.MacroWrappers
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

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdDeletePSMClass(Controller, ClassGuid));
        }
    }
}
