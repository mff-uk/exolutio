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
    public class cmdUpdateAttributeType : WrapperCommand
    {
        public Guid AttributeTypeGuid { get; set; }

        public string NewName { get; set; }

        public string NewXSDDefinition { get; set; }

        public bool NewIsSealed { get; set; }

        public Guid NewBaseType { get; set; }

        public cmdUpdateAttributeType()
        {
            CheckFirstOnlyInCanExecute = true;
        }
        public cmdUpdateAttributeType(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid attributeTypeGuid, string name, string xsdDefinition, bool isSealed, Guid baseType)
        {
            AttributeTypeGuid = attributeTypeGuid;
            NewName = name;
            NewXSDDefinition = xsdDefinition;
            NewIsSealed = isSealed;
            NewBaseType = baseType;
        }


        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdUpdateAttributeType(Controller, AttributeTypeGuid, NewName, NewXSDDefinition, NewIsSealed, NewBaseType));
        }


    }
}
