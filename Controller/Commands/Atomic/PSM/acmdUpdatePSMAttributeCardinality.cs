using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Model.PSM;
using EvoX.Model;
using EvoX.Model.PIM;
using EvoX.Controller.Commands.Atomic.PIM;

namespace EvoX.Controller.Commands.Atomic.PSM
{
    public class acmdUpdatePSMAttributeCardinality : StackedCommand
    {
        public Guid componentGuid;
        private uint oldLower, newLower;
        private UnlimitedInt oldUpper, newUpper;

        public acmdUpdatePSMAttributeCardinality(Controller c, Guid cardinalityOwnerGuid, uint lower, UnlimitedInt upper)
            : base(c)
        {
            componentGuid = cardinalityOwnerGuid;
            newLower = lower;
            newUpper = upper;
        }

        public override bool CanExecute()
        {
            return componentGuid != Guid.Empty && Project.TranslateComponent<Component>(componentGuid) is IHasCardinality;
        }
        
        internal override void CommandOperation()
        {
            IHasCardinality owner = Project.TranslateComponent<Component>(componentGuid) as IHasCardinality;
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
                IHasCardinality owner = Project.TranslateComponent<Component>(componentGuid) as IHasCardinality;
                owner.Lower = oldLower;
                owner.Upper = oldUpper;
                return OperationResult.OK;
            }
            catch
            {
                return OperationResult.Failed;
            }
        }
        
        internal override MacroCommand PostPropagation()
        {
            PSMAttribute attribute = Project.TranslateComponent<PSMAttribute>(componentGuid);

            if (attribute.Interpretation == null) return null;

            PIMAttribute interpretation = attribute.Interpretation as PIMAttribute;

            MacroCommand command = new MacroCommand(Controller);

            acmdUpdatePIMAttributeCardinality d = new acmdUpdatePIMAttributeCardinality(Controller, interpretation, newLower, newUpper) { Propagate = true };
            command.Commands.Add(d);

            return command;
        }
    }
}
