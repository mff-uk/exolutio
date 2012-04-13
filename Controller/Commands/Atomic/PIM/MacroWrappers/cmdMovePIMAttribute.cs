using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Controller.Commands.Reflection;
using Exolutio.Model.PIM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PIM.MacroWrappers
{
    [PublicCommand("Move PIM Attribute to neighboring PIM Class", PublicCommandAttribute.EPulicCommandCategory.PIM_atomic)]
    public class cmdMovePIMAttribute : WrapperCommand
    {
        [PublicArgument("PIMAttribute", typeof(PIMAttribute))]
        [Scope(ScopeAttribute.EScope.PIMAttribute)]
        public Guid AttributeGuid { get; set; }

        [PublicArgument("Target PIM Class", typeof(PIMClass), ModifiedPropertyName = "PIMClass")]
        [ConsistentWith("AttributeGuid", PIMAttributeNeighboringClassParameterConsistency.Key)]
        public Guid TargetClassGuid { get; set; }

        public cmdMovePIMAttribute() { }
        public cmdMovePIMAttribute(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }
        
        public void Set(Guid pimAttributeGuid, Guid pimClassGuid)
        {
            TargetClassGuid = pimClassGuid;
            AttributeGuid = pimAttributeGuid;
        }

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdMovePIMAttribute(Controller, AttributeGuid, TargetClassGuid));
            base.GenerateSubCommands();
        }
    }
}
