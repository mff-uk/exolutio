using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Controller.Commands.Reflection;
using EvoX.Model.PIM;
using EvoX.Model;

namespace EvoX.Controller.Commands.Atomic.PIM
{
    [PublicCommand("Move PIM Attribute to neighboring PIM Class", PublicCommandAttribute.EPulicCommandCategory.PIM_atomic)]
    public class cmdMovePIMAttribute : MacroCommand
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

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdMovePIMAttribute(Controller, AttributeGuid, TargetClassGuid));
            base.GenerateSubCommands();
        }
    }
}
