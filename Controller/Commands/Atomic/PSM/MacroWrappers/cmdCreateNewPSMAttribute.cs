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
    [PublicCommand("Create new PSM attribute", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdCreateNewPSMAttribute : MacroCommand
    {
        [PublicArgument("PSMClass", typeof(PSMClass))]
        [Scope(ScopeAttribute.EScope.PSMClass)]
        public Guid PSMClassGuid { get; set; }

        public Guid AttributeGuid { get; set; }
        
        public cmdCreateNewPSMAttribute() { }

        public cmdCreateNewPSMAttribute(Controller c)
            : base(c) { }

        public void Set(Guid psmClassGuid)
        {
            PSMClassGuid = psmClassGuid;
            
        }

        protected override void GenerateSubCommands()
        {
            if (AttributeGuid == Guid.Empty) AttributeGuid = Guid.NewGuid();
            Commands.Add(new acmdNewPSMAttribute(Controller, PSMClassGuid, Project.TranslateComponent<PSMClass>(PSMClassGuid).Schema) { AttributeGuid = AttributeGuid });
        }

    }
}
