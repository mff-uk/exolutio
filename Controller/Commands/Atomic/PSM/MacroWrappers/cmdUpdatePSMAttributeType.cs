using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Model.PIM;
using EvoX.Model;
using EvoX.Model.PSM;
using EvoX.Controller.Commands.Atomic.PSM;

namespace EvoX.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Update PSM attribute type", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdUpdatePSMAttributeType : MacroCommand
    {
        [PublicArgument("Attribute", typeof (PSMAttribute))]
        [Scope(ScopeAttribute.EScope.PSMAttribute)]
        public Guid attributeGuid { get; set; }

        [PublicArgument("New attribute type", typeof(AttributeType), AllowNullInput = true, ModifiedPropertyName = "AttributeType")]
        public Guid newTypeGuid { get; set; }

        public cmdUpdatePSMAttributeType()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdUpdatePSMAttributeType(Controller c)
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
            Commands.Add(new acmdUpdatePSMAttributeType(Controller, attributeGuid, newTypeGuid));
        }
        
        public override bool CanExecute()
        {
            return attributeGuid != Guid.Empty;
        }
    }
}
