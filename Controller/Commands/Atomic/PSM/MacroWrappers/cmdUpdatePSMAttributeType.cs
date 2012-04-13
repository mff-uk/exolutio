using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Atomic.PSM;

namespace Exolutio.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Update PSM attribute type", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdUpdatePSMAttributeType : WrapperCommand
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

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdUpdatePSMAttributeType(Controller, attributeGuid, newTypeGuid));
        }
        
        public override bool CanExecute()
        {
            return attributeGuid != Guid.Empty;
        }
    }
}
