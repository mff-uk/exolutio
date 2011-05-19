using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Model.PIM;
using EvoX.Model;
using EvoX.Model.PSM;
using EvoX.Controller.Commands.Atomic.PSM;

namespace EvoX.Controller.Commands.Atomic.PIM.MacroWrappers
{
    [PublicCommand("Update PIM association end cardinality", PublicCommandAttribute.EPulicCommandCategory.PIM_atomic)]
    public class cmdUpdatePIMAssociationEndCardinality : MacroCommand
    {
        [PublicArgument("PIMAssociationEnd", typeof(PIMAssociationEnd))]
        [Scope(ScopeAttribute.EScope.PIMAssociationEnd)]
        public Guid ComponentGuid { get; set; }

        [PublicArgument("New lower", ModifiedPropertyName = "Lower")]
        public uint NewLower { get; set; }

        [PublicArgument("New upper", ModifiedPropertyName = "Upper")]
        public UnlimitedInt NewUpper { get; set; }

        public cmdUpdatePIMAssociationEndCardinality()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdUpdatePIMAssociationEndCardinality(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid cardinalityOwnerGuid, uint lower, UnlimitedInt upper)
        {
            ComponentGuid = cardinalityOwnerGuid;
            NewLower = lower;
            NewUpper = upper;
        }

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdUpdatePIMAssociationEndCardinality(Controller, ComponentGuid, NewLower, NewUpper));
        }

        public override bool CanExecute()
        {
            return ComponentGuid != Guid.Empty;
        }
        
    }
}
