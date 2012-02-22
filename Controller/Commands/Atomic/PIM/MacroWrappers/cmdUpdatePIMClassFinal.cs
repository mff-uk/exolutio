using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Model;
using Exolutio.Controller.Commands.Atomic.PIM;

namespace Exolutio.Controller.Commands.Atomic.PIM.MacroWrappers
{
    /// <summary>
    /// Atomic operation that updates the class' final property
    /// </summary>
    [PublicCommand("Update PIM class final property", PublicCommandAttribute.EPulicCommandCategory.PIM_atomic)]
    public class cmdUpdatePIMClassFinal : MacroCommand
    {
        [PublicArgument("PIM Class", typeof(PIMClass))]
        [Scope(ScopeAttribute.EScope.PIMClass)]
        public Guid ClassGuid { get; set; }

        [PublicArgument("Final", ModifiedPropertyName = "Final")]
        public bool Final { get; set; }

        public cmdUpdatePIMClassFinal() { }

        public cmdUpdatePIMClassFinal(Controller c)
            : base(c) { }

        public void Set(Guid psmClass, bool final)
        {
            ClassGuid = psmClass;
            Final = final;
            
        }

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdUpdatePIMClassFinal(Controller, ClassGuid, Final));
        }

    }
}
