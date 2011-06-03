using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Atomic.PSM;

namespace Exolutio.Controller.Commands.Atomic.PIM
{
    public class acmdUpdatePIMAttributeCardinality : StackedCommand
    {
        Guid ComponentGuid;
        uint oldLower, newLower;
        UnlimitedInt oldUpper, newUpper;

        public acmdUpdatePIMAttributeCardinality(Controller c, Guid cardinalityOwnerGuid, uint lower, UnlimitedInt upper)
            : base(c)
        {
            ComponentGuid = cardinalityOwnerGuid;
            newLower = lower;
            newUpper = upper;
        }

        public override bool CanExecute()
        {
            return ComponentGuid != Guid.Empty && Project.TranslateComponent<Component>(ComponentGuid) is IHasCardinality;
        }
        
        internal override void CommandOperation()
        {
            IHasCardinality owner = Project.TranslateComponent<Component>(ComponentGuid) as IHasCardinality;
            string oldCardinality = owner.CardinalityString;
            oldLower = owner.Lower;
            oldUpper = owner.Upper;
            owner.Lower = newLower;
            owner.Upper = newUpper;
            Report = new CommandReport(CommandReports.CARDINALITY_CHANGED, owner, oldCardinality, owner.CardinalityString);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            try
            {
                IHasCardinality owner = Project.TranslateComponent<Component>(ComponentGuid) as IHasCardinality;
                owner.Lower = oldLower;
                owner.Upper = oldUpper;
                return OperationResult.OK;
            }
            catch
            {
                return OperationResult.Failed;
            }
        }

        /*internal override PropagationMacroCommand PrePropagation()
        {
            List<PSMAttribute> list = Project.TranslateComponent<PIMAttribute>(ComponentGuid).GetInterpretedComponents().Cast<PSMAttribute>().ToList<PSMAttribute>();
            if (list.Count == 0) return null;

            PropagationMacroCommand command = new PropagationMacroCommand(Controller);
            command.Report = new CommandReport("Pre-propagation (update PIM attribute cardinality)");

            foreach (PSMAttribute a in list)
            {
                acmdUpdatePSMAttributeCardinality d = new acmdUpdatePSMAttributeCardinality(Controller, a, newLower, newUpper) { Propagate = false };
                command.Commands.Add(d);
            }

            return command;
        }*/
    }
}
