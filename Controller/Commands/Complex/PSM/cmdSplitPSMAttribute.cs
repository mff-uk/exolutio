using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Atomic;
using Exolutio.Controller.Commands.Atomic.PSM;

namespace Exolutio.Controller.Commands.Complex.PSM
{
    [PublicCommand("Split PSM attribute (complex)", PublicCommandAttribute.EPulicCommandCategory.PSM_complex)]
    public class cmdSplitPSMAttribute : MacroCommand
    {
        [PublicArgument("PSM attribute", typeof(PSMAttribute))]
        [Scope(ScopeAttribute.EScope.PSMAttribute)]
        public Guid PSMAttributeGuid { get; set; }

        public Guid NewAttributeGuid { get; set; }
        
        public cmdSplitPSMAttribute()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdSplitPSMAttribute(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid psmAttributeGuid, Guid newAttributeGuid)
        {

            PSMAttributeGuid = psmAttributeGuid;
            NewAttributeGuid = newAttributeGuid;
        }

        protected override void GenerateSubCommands()
        {
            if (NewAttributeGuid == Guid.Empty) NewAttributeGuid = Guid.NewGuid();
            PSMAttribute original = Project.TranslateComponent<PSMAttribute>(PSMAttributeGuid);
            Commands.Add(new acmdNewPSMAttribute(Controller, original.PSMClass, original.PSMSchema) { AttributeGuid = NewAttributeGuid });
            Commands.Add(new acmdRenameComponent(Controller, NewAttributeGuid, original.Name + "2"));
            Commands.Add(new acmdUpdatePSMAttributeCardinality(Controller, NewAttributeGuid, original.Lower, original.Upper));
            Commands.Add(new acmdUpdatePSMAttributeType(Controller, NewAttributeGuid, original.AttributeType));
            Commands.Add(new acmdUpdatePSMAttributeXForm(Controller, NewAttributeGuid, original.Element));
            Commands.Add(new acmdSynchroPSMAttributes(Controller) { X1 = Enumerable.Repeat(original.ID, 1).ToList(), X2 = Enumerable.Repeat(NewAttributeGuid, 1).ToList() });
        }

        public override bool CanExecute()
        {
            if (PSMAttributeGuid == null) return false;
            return base.CanExecute();
        }

        internal override void CommandOperation()
        {
            base.CommandOperation();
            Report = new CommandReport(CommandReports.COMPLEX_SPLIT_PSM_ATTRIBUTE);
        }
        
    }
}
