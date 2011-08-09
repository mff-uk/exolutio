using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic
{
    public class acmdNewAttributeType : StackedCommand
    {
        public Guid AttributeTypeGuid { get; set; }

        public string Name { get; set; }

        public Guid BaseTypeGuid;

        public string XSDDefinition { get; set; }

        public Guid ProjectVersionGuid { get; set; }

        public bool IsSealed { get; set; }

        //optional
        public Guid PSMSchemaGuid { get; set; }

        public acmdNewAttributeType(Controller c)
            : base(c)
        {

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

        public override bool CanExecute()
        {
            return true; 
        }

        public void Set(Guid projectVersionGuid, string name, string xsdDefinition, bool isSealed, Guid baseType)
        {
            ProjectVersionGuid = projectVersionGuid;
            Name = name;
            XSDDefinition = xsdDefinition;
            IsSealed = isSealed;
            BaseTypeGuid = baseType;
        }

        internal override void CommandOperation()
        {
            if (AttributeTypeGuid == Guid.Empty)
            {
                AttributeTypeGuid = Guid.NewGuid();
            }

            AttributeType a = new AttributeType(Project, AttributeTypeGuid);
            ProjectVersion pv = Project.TranslateComponent<ProjectVersion>(ProjectVersionGuid);
            a.SetProjectVersion(pv);
            a.BaseType = BaseTypeGuid != Guid.Empty ? Project.TranslateComponent<AttributeType>(BaseTypeGuid) : null;
            a.Name = Name;
            a.IsSealed = IsSealed;
            a.XSDDefinition = XSDDefinition;
            if (PSMSchemaGuid != Guid.Empty)
            {
                PSMSchema psmSchema = Project.TranslateComponent<PSMSchema>(PSMSchemaGuid);
                a.Schema = psmSchema;
                psmSchema.PSMSchemaDefinedTypes.Add(a);
            }
            else
            {
                pv.PIMAttributeTypes.Add(a);
            }

            Report = new CommandReport("'{0}' created. ");
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            AttributeType a = Project.TranslateComponent<AttributeType>(AttributeTypeGuid);
            if (PSMSchemaGuid != Guid.Empty)
            {
                PSMSchema psmSchema = Project.TranslateComponent<PSMSchema>(PSMSchemaGuid);
                psmSchema.PSMSchemaDefinedTypes.Remove(a);
            }
            else
            {
                a.ProjectVersion.PIMAttributeTypes.Remove(a);
            }

            Project.mappingDictionary.Remove(AttributeTypeGuid);
            return OperationResult.OK;
        }
    }
}
