using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM
{
    internal class acmdDeletePSMSchema : AtomicCommand
    {
        private Guid schemaGuid;

        private Guid schemaClassGuid;

        private string Name;

        private int index;

        public acmdDeletePSMSchema(Controller c, Guid psmSchemaGuid)
            : base(c)
        {
            schemaGuid = psmSchemaGuid;
        }

        public override bool CanExecute()
        {
            if (!(schemaGuid != Guid.Empty && Project.VerifyComponentType<PSMSchema>(schemaGuid)))
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }
            PSMSchema s = Project.TranslateComponent<PSMSchema>(schemaGuid);
            PSMSchemaClass sC = s.PSMSchemaClass;
            if (sC.ChildPSMAssociations.Count > 0)
            {
                ErrorDescription = CommandErrors.CMDERR_CLASS_HAS_ASSOCIATIONS;
                return false;
            }
            if (s.Roots.Count > 1)
            {
                ErrorDescription = CommandErrors.CMDERR_ROOTS_PRESENT;
                return false;
            }

            return true;
        }
        
        internal override void CommandOperation()
        {
            PSMSchema s = Project.TranslateComponent<PSMSchema>(schemaGuid);
            string report = s.ToString();
            schemaClassGuid = s.PSMSchemaClass;
            Name = s.PSMSchemaClass.Name;
            s.UnRegisterPSMSchemaClass(s.PSMSchemaClass);
            index = Project.LatestVersion.PSMSchemas.Remove(s);
            Project.mappingDictionary.Remove(schemaClassGuid);
            Project.mappingDictionary.Remove(schemaGuid);
            Report = new CommandReport(CommandReports.PSM_component_deleted, report);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PSMSchema schema = new PSMSchema(Project, schemaGuid);
            new PSMSchemaClass(Project, schemaClassGuid, schema) { Name = Name };
            Project.LatestVersion.PSMSchemas.Insert(schema, index);
            return OperationResult.OK;
        }
    }
}
