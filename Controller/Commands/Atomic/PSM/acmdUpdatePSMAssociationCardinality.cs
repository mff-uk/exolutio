﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Model.PIM;
using EvoX.Model;
using EvoX.Model.PSM;
using EvoX.Controller.Commands.Atomic.PSM;
using EvoX.Controller.Commands.Atomic.PIM;

namespace EvoX.Controller.Commands.Atomic.PSM
{
    public class acmdUpdatePSMAssociationCardinality : StackedCommand
    {
        public Guid componentGuid;
        private uint oldLower, newLower;
        private UnlimitedInt oldUpper, newUpper;

        public acmdUpdatePSMAssociationCardinality(Controller c, Guid cardinalityOwnerGuid, uint lower, UnlimitedInt upper)
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
            IHasCardinality owner = Project.TranslateComponent<Component>(componentGuid) as IHasCardinality;
            owner.Lower = oldLower;
            owner.Upper = oldUpper;
            return OperationResult.OK;
        }

        internal override MacroCommand PostPropagation()
        {
            PSMAssociation association = Project.TranslateComponent<PSMAssociation>(componentGuid);
            PIMAssociation interpretation = association.Interpretation as PIMAssociation;
            if (interpretation == null) return null;

            PIMAssociationEnd e = ((association.Child as PSMClass).Interpretation as PIMClass).PIMAssociationEnds.First<PIMAssociationEnd>(end => end.PIMAssociation == interpretation);

            MacroCommand command = new MacroCommand(Controller);

            acmdUpdatePIMAssociationEndCardinality d = new acmdUpdatePIMAssociationEndCardinality(Controller, e, newLower, newUpper) { Propagate = true };
            command.Commands.Add(d);

            return command;
        }
    }
}
