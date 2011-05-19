using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Model;
using EvoX.Model.PIM;
using EvoX.Controller.Commands.Atomic;
using EvoX.Controller.Commands.Atomic.PIM;

namespace EvoX.Controller.Commands.Complex.PIM
{
    [PublicCommand("Delete PIM attribute (complex)", PublicCommandAttribute.EPulicCommandCategory.PIM_complex)]
    public class cmdDeletePIMAttribute : MacroCommand
    {
        [PublicArgument("Deleted attribute", typeof(PIMAttribute))]
        [Scope(ScopeAttribute.EScope.PIMAttribute)]
        public Guid AttributeGuid { get; set; }
        
        public cmdDeletePIMAttribute()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdDeletePIMAttribute(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid attributeGuid)
        {
            AttributeGuid = attributeGuid;
            
        }

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdRenameComponent(Controller, AttributeGuid, "") { Propagate = false });
            Commands.Add(new acmdUpdatePIMAttributeCardinality(Controller, AttributeGuid, 1, 1) { Propagate = false });
            Commands.Add(new acmdUpdatePIMAttributeType(Controller, AttributeGuid, Guid.Empty) { Propagate = false });
            Commands.Add(new acmdDeletePIMAttribute(Controller, AttributeGuid));
        }

        public override bool CanExecute()
        {
            if (AttributeGuid == Guid.Empty) return false;
            return base.CanExecute();
        }
        
    }
}
