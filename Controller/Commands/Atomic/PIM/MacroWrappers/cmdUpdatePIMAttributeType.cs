using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Model.PIM;
using EvoX.Model;
using EvoX.Model.PSM;
using EvoX.Controller.Commands.Atomic.PSM;

namespace EvoX.Controller.Commands.Atomic.PIM.MacroWrappers
{
    [PublicCommand("Update PIM attribute type", PublicCommandAttribute.EPulicCommandCategory.PIM_atomic)]
    public class cmdUpdatePIMAttributeType : MacroCommand
    {
        [PublicArgument("Attribute", typeof (PIMAttribute))]
        [Scope(ScopeAttribute.EScope.PIMAttribute)]
        public Guid attributeGuid { get; set; }

        [PublicArgument("New attribute type", typeof(AttributeType), AllowNullInput = true, ModifiedPropertyName = "AttributeType")]
        public Guid newTypeGuid { get; set; }

        public cmdUpdatePIMAttributeType()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdUpdatePIMAttributeType(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid pimAttributeGuid, Guid typeGuid)
        {
            attributeGuid = pimAttributeGuid;
            newTypeGuid = typeGuid;
            
        }

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdUpdatePIMAttributeType(Controller, attributeGuid, newTypeGuid));
        }
        
        public override bool CanExecute()
        {
            return attributeGuid != Guid.Empty;
        }
    }
}
