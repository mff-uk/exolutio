using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Model.PIM;
using EvoX.Model;
using EvoX.Model.PSM;
using EvoX.Controller.Commands.Atomic.PSM;

namespace EvoX.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Update PSM attribute XML form", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdUpdatePSMAttributeXForm : MacroCommand
    {
        [PublicArgument("Attribute", typeof(PSMAttribute))]
        [Scope(ScopeAttribute.EScope.PSMAttribute)]
        public Guid AttributeGuid { get; set; }

        [PublicArgument("Represents XML element", ModifiedPropertyName = "Element")]
        public bool NewForm { get; set; }

        public cmdUpdatePSMAttributeXForm()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdUpdatePSMAttributeXForm(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid psmAttributeGuid, bool element)
        {
            AttributeGuid = psmAttributeGuid;
            NewForm = element;
            
        }

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdUpdatePSMAttributeXForm(Controller, AttributeGuid, NewForm));
        }
        
        public override bool CanExecute()
        {
            return AttributeGuid != Guid.Empty;
        }
    }
}
