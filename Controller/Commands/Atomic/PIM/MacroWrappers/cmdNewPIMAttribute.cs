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
    [PublicCommand("Create new PIM attribute", PublicCommandAttribute.EPulicCommandCategory.PIM_atomic)]
    public class cmdNewPIMAttribute : WrapperCommand
    {
        [PublicArgument("PIMClass", typeof(PIMClass))]
        [Scope(ScopeAttribute.EScope.PIMClass)]
        public Guid PIMClassGuid { get; set; }

        [GeneratedIDArgument("AttributeGuid", typeof(PIMAttribute))]
        public Guid AttributeGuid { get; set; }
        
        public cmdNewPIMAttribute() {}

        public cmdNewPIMAttribute(Controller c)
            : base(c) {}

        public void Set(Guid pimClassGuid)
        {
            PIMClassGuid = pimClassGuid;
            
        }

        internal override void GenerateSubCommands()
        {
            if (AttributeGuid == Guid.Empty) AttributeGuid = Guid.NewGuid();
            Commands.Add(new acmdNewPIMAttribute(Controller, PIMClassGuid, Project.TranslateComponent<PIMClass>(PIMClassGuid).Schema) { AttributeGuid = AttributeGuid });
        }

    }
}
