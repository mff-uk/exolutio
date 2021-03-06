﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Atomic.PSM;

namespace Exolutio.Controller.Commands.Atomic.PIM.MacroWrappers
{
    [PublicCommand("Update PIM attribute type", PublicCommandAttribute.EPulicCommandCategory.PIM_atomic)]
    public class cmdUpdatePIMAttributeType : WrapperCommand
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

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdUpdatePIMAttributeType(Controller, attributeGuid, newTypeGuid));
        }
        
        public override bool CanExecute()
        {
            return attributeGuid != Guid.Empty;
        }
    }
}
