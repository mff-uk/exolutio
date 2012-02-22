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
    /// Atomic operation that updates the class' abstract property
    /// </summary>
    [PublicCommand("Update PSM class abstract property", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdUpdatePSMClassAbstract : MacroCommand
    {
        [PublicArgument("PSM Class", typeof(PSMClass))]
        [Scope(ScopeAttribute.EScope.PSMClass)]
        public Guid ClassGuid { get; set; }

        [PublicArgument("Abstract", ModifiedPropertyName = "Abstract")]
        public bool Abstract { get; set; }

        public cmdUpdatePSMClassAbstract() { }

        public cmdUpdatePSMClassAbstract(Controller c)
            : base(c) { }

        public void Set(Guid psmClass, bool @abstract)
        {
            ClassGuid = psmClass;
            Abstract = @abstract;
            
        }

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdUpdatePSMClassAbstract(Controller, ClassGuid, Abstract));
        }

    }
}
