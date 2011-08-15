using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM
{
    /// <summary>
    /// Atomic operation that updates the content model's type
    /// </summary>
    internal class acmdUpdatePSMContentModel : StackedCommand
    {
        private Guid cmodelGuid;
        private PSMContentModelType oldtype, newtype;

        public acmdUpdatePSMContentModel(Controller c, Guid psmContentModelGuid, PSMContentModelType type)
            : base(c)
        {
            cmodelGuid = psmContentModelGuid;
            newtype = type;
        }

        public override bool CanExecute()
        {
            if (!(cmodelGuid != Guid.Empty
                && Project.VerifyComponentType<PSMContentModel>(cmodelGuid)))
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }
            return true;
        }
        
        internal override void CommandOperation()
        {
            PSMContentModel cm = Project.TranslateComponent<PSMContentModel>(cmodelGuid);
            oldtype = cm.Type;
            cm.Type = newtype;
            Report = new CommandReport(CommandReports.PSM_component_deleted, cm);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PSMContentModel cm = Project.TranslateComponent<PSMContentModel>(cmodelGuid);
            cm.Type = oldtype;
            return OperationResult.OK;
        }
    }
}
