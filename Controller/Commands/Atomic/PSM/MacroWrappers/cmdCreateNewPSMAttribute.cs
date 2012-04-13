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
    [PublicCommand("Create new PSM attribute", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdCreateNewPSMAttribute : WrapperCommand
    {
        [PublicArgument("PSMClass", typeof(PSMClass))]
        [Scope(ScopeAttribute.EScope.PSMClass)]
        public Guid PSMClassGuid { get; set; }

        [GeneratedIDArgument("AttributeGuid", typeof(PSMAttribute))]
        public Guid AttributeGuid { get; set; }
        
        public cmdCreateNewPSMAttribute() { }

        public cmdCreateNewPSMAttribute(Controller c)
            : base(c) { }

        public void Set(Guid psmClassGuid)
        {
            PSMClassGuid = psmClassGuid;
            
        }

        internal override void GenerateSubCommands()
        {
            if (AttributeGuid == Guid.Empty) AttributeGuid = Guid.NewGuid();
            Commands.Add(new acmdNewPSMAttribute(Controller, PSMClassGuid, Project.TranslateComponent<PSMClass>(PSMClassGuid).Schema) { AttributeGuid = AttributeGuid });
        }

    }
}
