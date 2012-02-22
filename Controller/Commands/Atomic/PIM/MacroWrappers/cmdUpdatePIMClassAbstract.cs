using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PIM.MacroWrappers
{
    /// <summary>
    /// Atomic operation that updates the class' abstract property
    /// </summary>
    [PublicCommand("Update PIM class abstract property", PublicCommandAttribute.EPulicCommandCategory.PIM_atomic)]
    public class cmdUpdatePIMClassAbstract : MacroCommand
    {
        [PublicArgument("PIM Class", typeof(PIMClass))]
        [Scope(ScopeAttribute.EScope.PIMClass)]
        public Guid ClassGuid { get; set; }

        [PublicArgument("Abstract", ModifiedPropertyName = "Abstract")]
        public bool Abstract { get; set; }

        public cmdUpdatePIMClassAbstract() { }

        public cmdUpdatePIMClassAbstract(Controller c)
            : base(c) { }

        public void Set(Guid pimClass, bool @abstract)
        {
            ClassGuid = pimClass;
            Abstract = @abstract;
            
        }

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdUpdatePIMClassAbstract(Controller, ClassGuid, Abstract));
        }

    }
}
