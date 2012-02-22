using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Atomic;
using Exolutio.Controller.Commands.Atomic.PSM;

namespace Exolutio.Controller.Commands.Complex.PSM
{
    [PublicCommand("Delete PSM attribute (complex)", PublicCommandAttribute.EPulicCommandCategory.PSM_complex)]
    public class cmdDeletePSMAttribute : ComposedCommand
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

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdRenameComponent(Controller, AttributeGuid, "") { Propagate = false });
            Commands.Add(new acmdUpdatePSMAttributeCardinality(Controller, AttributeGuid, 1, 1) { Propagate = false });
            Commands.Add(new acmdUpdatePSMAttributeType(Controller, AttributeGuid, Guid.Empty) { Propagate = false });
            Commands.Add(new acmdUpdatePSMAttributeXForm(Controller, AttributeGuid, true) { Propagate = false });
            Commands.Add(new acmdSetPSMAttributeInterpretation(Controller, AttributeGuid, Guid.Empty) { Propagate = false });
            Commands.Add(new acmdDeletePSMAttribute(Controller, AttributeGuid));
        }

        public override bool CanExecute()
        {
            if (AttributeGuid == Guid.Empty) return false;
            return base.CanExecute();
        }

        internal override void CommandOperation()
        {
            base.CommandOperation();
            Report = new CommandReport(CommandReports.COMPLEX_DELETE_PSM_ATTRIBUTE);
        }

    }
}
