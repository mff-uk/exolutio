using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM.MacroWrappers
{
    /// <summary>
    /// Atomic operation that updates the class' final property
    /// </summary>
    [PublicCommand("Update PSM class final property", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdUpdatePSMClassFinal : MacroCommand
    {
        [PublicArgument("PSM Class", typeof(PSMClass))]
        [Scope(ScopeAttribute.EScope.PSMClass)]
        public Guid ClassGuid { get; set; }

        [PublicArgument("Final", ModifiedPropertyName = "Final")]
        public bool Final { get; set; }

        public cmdUpdatePSMClassFinal() { }

        public cmdUpdatePSMClassFinal(Controller c)
            : base(c) { }

        public void Set(Guid psmClass, bool final)
        {
            ClassGuid = psmClass;
            Final = final;
            
        }

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdUpdatePSMClassFinal(Controller, ClassGuid, Final));
        }

    }
}
