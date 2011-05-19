using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Model;
using EvoX.Model.PSM;

namespace EvoX.Controller.Commands.Atomic.PSM
{
    public class acmdDeletePSMClass : StackedCommand
    {
        Guid deletedClassGuid, schemaGuid;

        int rootIndex = -1;

        public acmdDeletePSMClass(Controller c, Guid psmClassGuid)
            : base(c)
        {
            deletedClassGuid = psmClassGuid;
            schemaGuid = Project.TranslateComponent<PSMClass>(deletedClassGuid).PSMSchema;
        }

        public override bool CanExecute()
        {
            if (!(deletedClassGuid != Guid.Empty
                && schemaGuid != Guid.Empty
                && Project.VerifyComponentType<PSMSchema>(schemaGuid)
                && Project.VerifyComponentType<PSMClass>(deletedClassGuid)))
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }

            PSMClass c = Project.TranslateComponent<PSMClass>(deletedClassGuid);

            if (c.ChildPSMAssociations.Count > 0)
            {
                ErrorDescription = CommandErrors.CMDERR_CLASS_HAS_ASSOCIATIONS;
                return false;
            }

            if (c.PSMAttributes.Count > 0)
            {
                ErrorDescription = CommandErrors.CMDERR_CLASS_HAS_ATTRIBUTES;
                return false;
            }

            if (c.Representants.Count > 0)
            {
                ErrorDescription = CommandErrors.CMDERR_CLASS_IS_REPRESENTED;
                return false;
            }

            return true;
        }

        internal override void CommandOperation()
        {
            PSMClass c = Project.TranslateComponent<PSMClass>(deletedClassGuid);
            PSMSchema schema = Project.TranslateComponent<PSMSchema>(schemaGuid);
            rootIndex = schema.Roots.Remove(c);
            schema.PSMClasses.Remove(c);
            Project.mappingDictionary.Remove(deletedClassGuid);
            Report = new CommandReport(CommandReports.PSM_component_deleted, c);
        }
        
        internal override OperationResult UndoOperation()
        {
            PSMSchema schema = Project.TranslateComponent<PSMSchema>(schemaGuid);
            PSMClass c = new PSMClass(Project, deletedClassGuid, schema, rootIndex);
            //schema.RegisterPSMRoot(c, rootIndex); //Already done in new PSMClass
            return OperationResult.OK;
        }
    }
}
