using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Atomic.PSM;

namespace Exolutio.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Update PSM association cardinality (with propagation)", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdUpdatePSMAssociationCardinality : WrapperCommand
    {
        [PublicArgument("PSMAssociation", typeof(PSMAssociation))]
        [Scope(ScopeAttribute.EScope.PSMAssociation)]
        public Guid ComponentGuid { get; set; }

        [PublicArgument("New lower", ModifiedPropertyName = "Lower")]
        public uint NewLower { get; set; }

        [PublicArgument("New upper", ModifiedPropertyName = "Upper")]
        public UnlimitedInt NewUpper { get; set; }

        public cmdUpdatePSMAssociationCardinality()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdUpdatePSMAssociationCardinality(Controller c)
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

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdUpdatePSMAssociationCardinality(Controller, ComponentGuid, NewLower, NewUpper));
        }

        public override bool CanExecute()
        {
            return ComponentGuid != Guid.Empty;
        }
        
    }
}
