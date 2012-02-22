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
    public class cmdNewAttributeType : MacroCommand
    {
        public Guid AttributeTypeGuid { get; set; }

        public string Name { get; set; }

        public Guid BaseTypeGuid;

        public string XSDDefinition { get; set; }

        public Guid ProjectVersionGuid { get; set; }

        public bool IsSealed { get; set; }

        //optional
        public Guid PSMSchemaGuid { get; set; }

        public cmdNewAttributeType()
        {
            CheckFirstOnlyInCanExecute = true;
        }
        public cmdNewAttributeType(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid projectVersionGuid, Guid attributeTypeGuid, Guid baseAttributeType, Guid psmSchemaGuid, string name, string xsdImplementation, bool isSealed)
        {
            AttributeTypeGuid = attributeTypeGuid;
            BaseTypeGuid = baseAttributeType;
            Name = name;
            XSDDefinition = xsdImplementation;
            PSMSchemaGuid = psmSchemaGuid;
            ProjectVersionGuid = projectVersionGuid;
            IsSealed = isSealed;
        }

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdNewAttributeType(Controller, ProjectVersionGuid, PSMSchemaGuid, Name, XSDDefinition, IsSealed, BaseTypeGuid));
        }


    }
}
