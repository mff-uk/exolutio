using System;
using System.Linq;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Controller.Commands.Atomic;
using Exolutio.Controller.Commands.Atomic.PIM;

namespace Exolutio.Controller.Commands.Complex.PIM
{
    [PublicCommand("Split PIM attribute (complex)", PublicCommandAttribute.EPulicCommandCategory.PSM_complex)]
    public class cmdSplitPIMAttribute : MacroCommand
    {
        [PublicArgument("PIM attribute", typeof(PIMAttribute))]
        [Scope(ScopeAttribute.EScope.PIMAttribute)]
        public Guid PIMAttributeGuid { get; set; }

        public Guid NewAttributeGuid { get; set; }
        
        public cmdSplitPIMAttribute()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdSplitPIMAttribute(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid psmAttributeGuid, Guid newAttributeGuid)
        {

            PIMAttributeGuid = psmAttributeGuid;
            NewAttributeGuid = newAttributeGuid;
        }

        protected override void GenerateSubCommands()
        {
            if (NewAttributeGuid == Guid.Empty) NewAttributeGuid = Guid.NewGuid();
            PIMAttribute original = Project.TranslateComponent<PIMAttribute>(PIMAttributeGuid);
            Commands.Add(new acmdNewPIMAttribute(Controller, original.PIMClass, original.PIMSchema) { AttributeGuid = NewAttributeGuid });
            Commands.Add(new acmdRenameComponent(Controller, NewAttributeGuid, original.Name + "2"));
            Commands.Add(new acmdUpdatePIMAttributeCardinality(Controller, NewAttributeGuid, original.Lower, original.Upper));
            Commands.Add(new acmdUpdatePIMAttributeType(Controller, NewAttributeGuid, original.AttributeType));
            Commands.Add(new acmdSynchroPIMAttributes(Controller) { X1 = Enumerable.Repeat(original.ID, 1).ToList(), X2 = Enumerable.Repeat(NewAttributeGuid, 1).ToList() });
        }

        public override bool CanExecute()
        {
            if (PIMAttributeGuid == null) return false;
            return base.CanExecute();
        }

        internal override void CommandOperation()
        {
            base.CommandOperation();
            Report = new CommandReport(CommandReports.COMPLEX_SPLIT_PIM_ATTRIBUTE);
        }
        
    }
}
