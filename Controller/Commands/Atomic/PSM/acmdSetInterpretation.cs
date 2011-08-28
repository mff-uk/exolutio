using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Controller.Commands.Reflection;
using Exolutio.Model.PSM;
using Exolutio.Model.PIM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM
{
    internal abstract class acmdSetInterpretation : AtomicCommand
    {
        protected Guid PSMComponentGuid;

        protected Guid PIMComponentGuid;

        protected Guid oldPimComponentGuid;

        protected List<Guid> oldUsedGeneralizations = new List<Guid>();

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
            
            c.UsedGeneralizations.Clear();
            foreach (Guid g in oldUsedGeneralizations)
            {
                c.UsedGeneralizations.AddAsGuidSilent(g);
            }
            return OperationResult.OK;
        }
    }
}
