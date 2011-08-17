using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic
{
    internal class acmdDeleteAttributeType : AtomicCommand
    {
        public Guid AttributeTypeGuid { get; set; }

        public Guid OwnerPSMSchemaGuid { get; set; }

        private bool ProjectVersionType
        {
            get { return OwnerPSMSchemaGuid == Guid.Empty; }
        }

        public acmdDeleteAttributeType()
        {
            
        }

        private AttributeType attributeType
        {
            get { return Controller.Project.TranslateComponent<AttributeType>(AttributeTypeGuid); }
        }

        public acmdDeleteAttributeType(Controller c, Guid ownerPSMSchemaGuid, Guid attributeTypeGuid)
            : base(c)
        {
            OwnerPSMSchemaGuid = ownerPSMSchemaGuid;
            AttributeTypeGuid = attributeTypeGuid;
        }

        public override bool CanExecute()
        {
            if (AttributeTypeGuid == Guid.Empty)
            {
                return false;
            }

            if (ProjectVersionType)
            {
                ProjectVersion projectVersion = attributeType.ProjectVersion;
                PIMAttribute usingAttribute = projectVersion.PIMSchema.PIMAttributes.FirstOrDefault(a => a.AttributeType == attributeType);
                if (usingAttribute != null)
                {
                    ErrorDescription = string.Format("'{0}' can not be deleted, because it is used by '{1}'.", attributeType, usingAttribute);
                    return false; 
                }

                AttributeType usingAttributeType = projectVersion.PIMAttributeTypes.FirstOrDefault(a => a.BaseType == attributeType);
                if (usingAttributeType != null)
                {
                    ErrorDescription = string.Format("'{0}' can not be deleted, because it is used by '{1}'.", attributeType, usingAttributeType);
                    return false; 
                }
            }
            else
            {
                PSMSchema psmSchema = Controller.Project.TranslateComponent<PSMSchema>(OwnerPSMSchemaGuid);
                PSMAttribute usingAttribute = psmSchema.PSMAttributes.FirstOrDefault(a => a.AttributeType == attributeType);
                if (usingAttribute != null)
                {
                    ErrorDescription = string.Format("'{0}' can not be deleted, because it is used by '{1}'.", attributeType, usingAttribute);
                    return false;
                }

                AttributeType usingAttributeType = psmSchema.PSMSchemaDefinedTypes.FirstOrDefault(a => a.BaseType == attributeType);
                if (usingAttributeType != null)
                {
                    ErrorDescription = string.Format("'{0}' can not be deleted, because it is used by '{1}'.", attributeType, usingAttributeType);
                    return false;
                }
            }

            return true; 
        }

        private string oldName;
        private string oldXSD;
        private Guid oldBaseType;
        private Guid oldSchema;
        private Guid oldProjectVersionGuid;
        private bool oldIsSealed;

        internal override void CommandOperation()
        {
            oldName = attributeType.Name;
            oldBaseType = attributeType.BaseType;
            oldXSD = attributeType.XSDDefinition;
            oldSchema = attributeType.Schema;
            oldProjectVersionGuid = attributeType.ProjectVersion;
            oldIsSealed = attributeType.IsSealed;

            if (ProjectVersionType)
            {
                attributeType.ProjectVersion.PIMAttributeTypes.Remove(attributeType);
            }
            else
            {
                PSMSchema psmSchema = Project.TranslateComponent<PSMSchema>(OwnerPSMSchemaGuid);
                psmSchema.PSMSchemaDefinedTypes.Remove(attributeType);
            }

            Project.mappingDictionary.Remove(attributeType);

            Report = new CommandReport("'{0}' deleted.", attributeType);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            AttributeType t = new AttributeType(Project, AttributeTypeGuid);
            ProjectVersion projectVersion = Controller.Project.TranslateComponent<ProjectVersion>(oldProjectVersionGuid);
            t.SetProjectVersion(projectVersion);
            t.XSDDefinition = oldXSD;
            t.BaseType = Project.TranslateComponent<AttributeType>(oldBaseType);
            t.Name = oldName;
            t.IsSealed = oldIsSealed;
            if (oldSchema != Guid.Empty)
            {
                PSMSchema psmSchema = Project.TranslateComponent<PSMSchema>(OwnerPSMSchemaGuid);
                t.Schema = psmSchema;
                psmSchema.PSMSchemaDefinedTypes.Add(t);
            }
            else
            {
                projectVersion.PIMAttributeTypes.Add(t);
            } 
            return OperationResult.OK;
        }
    }
}
