using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM
{
    public class acmdNewPSMContentModel : StackedCommand
    {
        private Guid schemaGuid;

        private Guid cmodelGuid = Guid.Empty;

        private PSMContentModelType type;
        
        /// <summary>
        /// If set before execution, creates a new content model with this GUID.
        /// After execution contains GUID of the created content model.
        /// </summary>
        public Guid ContentModelGuid
        {
            get { return cmodelGuid; }
            set
            {
                if (!Executed) cmodelGuid = value;
                else throw new ExolutioCommandException("Cannot set ContentModelGuid after command execution.", this);
            }
        }

        public acmdNewPSMContentModel(Controller c, PSMContentModelType contentModelType, Guid psmSchemaGuid)
            : base(c)
        {
            schemaGuid = psmSchemaGuid;
            type = contentModelType;
        }

        public override bool CanExecute()
        {
            return schemaGuid != Guid.Empty
                && Project.VerifyComponentType<PSMSchema>(schemaGuid);
        }
        
        internal override void CommandOperation()
        {
            if (ContentModelGuid == Guid.Empty) ContentModelGuid = Guid.NewGuid();

            PSMSchema s = Project.TranslateComponent<PSMSchema>(schemaGuid);
            PSMContentModel cm = new PSMContentModel(Project, ContentModelGuid, s);
            cm.Type = type;

            Report = new CommandReport(CommandReports.PSM_component_added, cm);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PSMSchema s = Project.TranslateComponent<PSMSchema>(schemaGuid);
            PSMContentModel cm = Project.TranslateComponent<PSMContentModel>(ContentModelGuid);

            Project.TranslateComponent<PSMSchema>(schemaGuid).Roots.Remove(cm);
            Project.TranslateComponent<PSMSchema>(schemaGuid).PSMContentModels.Remove(cm);
            Project.mappingDictionary.Remove(cm);
            return OperationResult.OK;
        }
    }
}
