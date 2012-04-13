using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.MacroWrappers
{
    public class cmdDeleteAttributeType : WrapperCommand
    {
        public Guid AttributeTypeGuid { get; set; }

        public Guid OwnerPSMSchemaGuid { get; set; }

        public cmdDeleteAttributeType()
        {
            CheckFirstOnlyInCanExecute = true;
        }
        public cmdDeleteAttributeType(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid attributeTypeGuid)
        {
            AttributeTypeGuid = attributeTypeGuid;
        }

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdDeleteAttributeType(Controller, OwnerPSMSchemaGuid, AttributeTypeGuid));
        }


    }
}
