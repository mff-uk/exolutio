using System;
using System.Linq;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Controller.Commands.Atomic;
using Exolutio.Controller.Commands.Atomic.PIM;
using System.Collections.Generic;

namespace Exolutio.Controller.Commands.Complex.PIM
{
    [PublicCommand("Split PIM attribute (complex)", PublicCommandAttribute.EPulicCommandCategory.PIM_complex)]
    public class cmdSplitPIMAttribute : ComposedCommand
    {
        [PublicArgument("PIM attribute", typeof(PIMAttribute))]
        [Scope(ScopeAttribute.EScope.PIMAttribute)]
        public Guid PIMAttributeGuid { get; set; }

        public IEnumerable<Guid> NewGuids { get; set; }

        private uint cnt = 2;
        [PublicArgument("Count")]
        public uint Count { get { return cnt; } set { cnt = value; } }
        
        public cmdSplitPIMAttribute()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdSplitPIMAttribute(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid psmAttributeGuid, IEnumerable<Guid> newGuids)
        {
            PIMAttributeGuid = psmAttributeGuid;
            NewGuids = newGuids;
        }

        public void Set(Guid pimAttributeGuid, uint count)
        {
            PIMAttributeGuid = pimAttributeGuid;
            Count = count;
        }

        internal override void GenerateSubCommands()
        {
            if (NewGuids == null)
            {
                List<Guid> guids = new List<Guid>();
                for (int i = 0; i < Count; i++)
                {
                    guids.Add(Guid.NewGuid());
                }
                NewGuids = guids;
            }
            PIMAttribute original = Project.TranslateComponent<PIMAttribute>(PIMAttributeGuid);
            uint counter = 0;
            foreach (Guid newAttributeGuid in NewGuids)
            {
                counter++;
                Commands.Add(new acmdNewPIMAttribute(Controller, original.PIMClass, original.PIMSchema) { AttributeGuid = newAttributeGuid });
                Commands.Add(new acmdRenameComponent(Controller, newAttributeGuid, original.Name + counter));
                Commands.Add(new acmdUpdatePIMAttributeCardinality(Controller, newAttributeGuid, original.Lower, original.Upper));
                Commands.Add(new acmdUpdatePIMAttributeType(Controller, newAttributeGuid, original.AttributeType));
            }
            Commands.Add(new acmdSynchroPIMAttributes(Controller) { X1 = Enumerable.Repeat(original.ID, 1).ToList(), X2 = NewGuids.ToList() });
            Commands.Add(new cmdDeletePIMAttribute(Controller) { AttributeGuid = original });
        }

        public override bool CanExecute()
        {
            if (PIMAttributeGuid == null) return false;
            if (Count < 2 && NewGuids == null) return false;
            return base.CanExecute();
        }

        internal override void CommandOperation()
        {
            base.CommandOperation();
            Report = new CommandReport(CommandReports.COMPLEX_SPLIT_PIM_ATTRIBUTE);
        }
        
    }
}
