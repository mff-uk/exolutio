using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Controller.Commands.Reflection;
using Exolutio.Model.PSM;
using Exolutio.Model.PIM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Set PSM class as structural representative", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdSetRepresentedClass : MacroCommand
    {
        [PublicArgument("Representant PSM class", typeof(PSMClass))]
        [Scope(ScopeAttribute.EScope.PSMClass)]
        public Guid Representant { get; set; }

        [PublicArgument("Represented PSM class", typeof(PSMClass), AllowNullInput = true, ModifiedPropertyName = "RepresentedClass")]
        public Guid Represented { get; set; }

        public cmdSetRepresentedClass() { }

        public cmdSetRepresentedClass(Controller c)
            : base(c) { }

        public void Set(Guid representantPSMClass, Guid representedPSMClass)
        {
            Represented = representedPSMClass;
            Representant = representantPSMClass;
            
        }

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdSetRepresentedClass(Controller, Representant, Represented));
        }
    }
}
