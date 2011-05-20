using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Controller.Commands.Reflection;
using EvoX.Model.PSM;
using EvoX.Model.PIM;
using EvoX.Model;

namespace EvoX.Controller.Commands.Atomic.PSM
{
    public abstract class acmdSetInterpretation : StackedCommand
    {
        protected Guid PSMComponentGuid;

        protected Guid PIMComponentGuid;

        private Guid oldPimComponentGuid;

        public acmdSetInterpretation(Controller c, Guid interpretedPSMComponentGuid, Guid pimInterpretationGuid)
            : base(c)
        {
            PSMComponentGuid = interpretedPSMComponentGuid;
            PIMComponentGuid = pimInterpretationGuid;
        }

        internal override void CommandOperation()
        {
            PSMComponent c = Project.TranslateComponent<PSMComponent>(PSMComponentGuid);
            PIMComponent oldInterpretation = c.Interpretation;
            if (c.Interpretation == null) oldPimComponentGuid = Guid.Empty;
            else oldPimComponentGuid = c.Interpretation;
            if (PIMComponentGuid != Guid.Empty)
                c.Interpretation = Project.TranslateComponent<PIMComponent>(PIMComponentGuid);
            else c.Interpretation = null;
            Report = new CommandReport(CommandReports.SET_INTERPRETATION, c, oldInterpretation, c.Interpretation);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PSMComponent c = Project.TranslateComponent<PSMComponent>(PSMComponentGuid);
            if (oldPimComponentGuid == Guid.Empty) c.Interpretation = null;
            else c.Interpretation = Project.TranslateComponent<PIMComponent>(oldPimComponentGuid);
            return OperationResult.OK;
        }
    }
}
