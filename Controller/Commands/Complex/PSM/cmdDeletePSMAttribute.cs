using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Model;
using EvoX.Model.PSM;
using EvoX.Controller.Commands.Atomic;
using EvoX.Controller.Commands.Atomic.PSM;

namespace EvoX.Controller.Commands.Complex.PSM
{
    [PublicCommand("Delete PSM attribute (complex)", PublicCommandAttribute.EPulicCommandCategory.PSM_complex)]
    public class cmdDeletePSMAttribute : MacroCommand
    {
        [PublicArgument("Deleted PSM attribute", typeof(PSMAttribute))]
        [Scope(ScopeAttribute.EScope.PSMAttribute)]
        public Guid AttributeGuid { get; set; }
        
        public cmdDeletePSMAttribute()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdDeletePSMAttribute(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid attributeGuid)
        {
            AttributeGuid = attributeGuid;
            
        }

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdRenameComponent(Controller, AttributeGuid, "") { Propagate = false });
            Commands.Add(new acmdUpdatePSMAttributeCardinality(Controller, AttributeGuid, 1, 1) { Propagate = false });
            Commands.Add(new acmdUpdatePSMAttributeType(Controller, AttributeGuid, Guid.Empty) { Propagate = false });
            Commands.Add(new acmdSetPSMAttributeInterpretation(Controller, AttributeGuid, Guid.Empty) { Propagate = false });
            Commands.Add(new acmdDeletePSMAttribute(Controller, AttributeGuid));
        }

        public override bool CanExecute()
        {
            if (AttributeGuid == Guid.Empty) return false;
            return base.CanExecute();
        }
        
    }
}
