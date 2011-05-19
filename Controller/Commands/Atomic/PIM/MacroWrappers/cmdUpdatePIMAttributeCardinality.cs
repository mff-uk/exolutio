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
    [PublicCommand("Update PIM attribute cardinality", PublicCommandAttribute.EPulicCommandCategory.PIM_atomic)]
    public class cmdUpdatePIMAttributeCardinality : MacroCommand
    {
        [PublicArgument("PIMAttribute", typeof(PIMAttribute))]
        [Scope(ScopeAttribute.EScope.PIMAttribute)]
        public Guid ComponentGuid { get; set; }

        [PublicArgument("New lower", ModifiedPropertyName = "Lower")]
        public uint NewLower { get; set; }

        [PublicArgument("New upper", ModifiedPropertyName = "Upper")]
        public UnlimitedInt NewUpper { get; set; }

        public cmdUpdatePIMAttributeCardinality()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdUpdatePIMAttributeCardinality(Controller c)
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
            Commands.Add(new acmdUpdatePIMAttributeCardinality(Controller, ComponentGuid, NewLower, NewUpper));
        }

        public override bool CanExecute()
        {
            return ComponentGuid != Guid.Empty;
        }
        
    }
}
