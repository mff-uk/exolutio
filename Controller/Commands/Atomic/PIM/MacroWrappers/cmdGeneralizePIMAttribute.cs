using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PIM.MacroWrappers
{
    [PublicCommand("Generalize PIM attribute (atomic)", PublicCommandAttribute.EPulicCommandCategory.PIM_atomic)]
    public class cmdGeneralizePIMAttribute : WrapperCommand
    {
        [PublicArgument("Attribute", typeof(PIMAttribute))]
        [Scope(ScopeAttribute.EScope.PIMAttribute)]
        public Guid AttributeGuid { get; set; }

        public cmdGeneralizePIMAttribute() { }

        public cmdGeneralizePIMAttribute(Controller c)
            : base(c) { }
        
        public void Set(Guid pimAttributeGuid)
        {
            AttributeGuid = pimAttributeGuid;
        }

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdGeneralizePIMAttribute(Controller, AttributeGuid));
        }
    }
}
