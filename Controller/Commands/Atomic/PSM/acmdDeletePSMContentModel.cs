using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Model.PSM;
using EvoX.Model;

namespace EvoX.Controller.Commands.Atomic.PSM
{
    /// <summary>
    /// Atomic operation that deletes the content model and moves its associations to a parent PSMAssociationMember
    /// </summary>
    public class acmdDeletePSMContentModel : StackedCommand
    {
        private Guid schemaGuid;

        private Guid cmodelGuid;

        private PSMContentModelType type;

        private int rootIndex = -1;
        
        public acmdDeletePSMContentModel(Controller c, Guid psmContentModelGuid)
            : base(c)
        {
            cmodelGuid = psmContentModelGuid;
            schemaGuid = Project.TranslateComponent<PSMContentModel>(cmodelGuid).PSMSchema;
        }

        public override bool CanExecute()
        {
            if (!(schemaGuid != Guid.Empty
                && Project.VerifyComponentType<PSMSchema>(schemaGuid)
                && cmodelGuid != Guid.Empty
                && Project.VerifyComponentType<PSMContentModel>(cmodelGuid)))
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }

            PSMContentModel cm = Project.TranslateComponent<PSMContentModel>(cmodelGuid);
            if (cm.ChildPSMAssociations.Count > 0)
            {
                ErrorDescription = CommandErrors.CMDERR_CONTENT_MODEL_NOT_EMPTY;
                return false;
            }

            return true;
        }
        
        internal override void CommandOperation()
        {
            PSMSchema s = Project.TranslateComponent<PSMSchema>(schemaGuid);
            PSMContentModel cm = Project.TranslateComponent<PSMContentModel>(cmodelGuid);
            type = cm.Type;

            rootIndex = s.Roots.Remove(cm);

            Project.TranslateComponent<PSMSchema>(schemaGuid).PSMContentModels.Remove(cm);
            Project.mappingDictionary.Remove(cm);
            Report = new CommandReport(CommandReports.PSM_component_deleted, cm);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PSMSchema s = Project.TranslateComponent<PSMSchema>(schemaGuid);
            PSMContentModel cm = new PSMContentModel(Project, cmodelGuid, s, rootIndex);
            cm.Type = type;

            //s.RegisterPSMRoot(cm, rootIndex); //Already done in the constructor

            return OperationResult.OK;
        }
    }
}
