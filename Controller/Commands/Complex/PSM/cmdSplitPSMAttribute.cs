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
    public class cmdSplitPSMAttribute : ComposedCommand
    {
        [PublicArgument("PSM attribute", typeof(PSMAttribute))]
        [Scope(ScopeAttribute.EScope.PSMAttribute)]
        public Guid PSMAttributeGuid { get; set; }

        public IEnumerable<Guid> NewGuids { get; set; }

        private uint cnt = 2;
        [PublicArgument("Count")]
        public uint Count { get { return cnt; } set { cnt = value; } }

        public cmdSplitPSMAttribute()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdSplitPSMAttribute(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid psmAttributeGuid, IEnumerable<Guid> newGuids)
        {
            PSMAttributeGuid = psmAttributeGuid;
            NewGuids = newGuids;
        }

        public void Set(Guid psmAttributeGuid, uint count)
        {
            PSMAttributeGuid = psmAttributeGuid;
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
            
            PSMAttribute original = Project.TranslateComponent<PSMAttribute>(PSMAttributeGuid);

            uint counter = 0;
            foreach (Guid newAttributeGuid in NewGuids)
            {
                counter++;
                Commands.Add(new acmdNewPSMAttribute(Controller, original.PSMClass, original.PSMSchema) { AttributeGuid = newAttributeGuid });
                Commands.Add(new acmdRenameComponent(Controller, newAttributeGuid, original.Name + counter));
                Commands.Add(new acmdUpdatePSMAttributeCardinality(Controller, newAttributeGuid, original.Lower, original.Upper));
                Commands.Add(new acmdUpdatePSMAttributeType(Controller, newAttributeGuid, original.AttributeType));
                Commands.Add(new acmdUpdatePSMAttributeXForm(Controller, newAttributeGuid, original.Element));
            }
            Commands.Add(new acmdSynchroPSMAttributes(Controller) { X1 = Enumerable.Repeat(original.ID, 1).ToList(), X2 = NewGuids.ToList() });
            Commands.Add(new cmdDeletePSMAttribute(Controller) { AttributeGuid = original });
        }

        public override bool CanExecute()
        {
            if (PSMAttributeGuid == null) return false;
            if (Count < 2 && NewGuids == null) return false;
            return base.CanExecute();
        }

        internal override void CommandOperation()
        {
            base.CommandOperation();
            Report = new CommandReport(CommandReports.COMPLEX_SPLIT_PSM_ATTRIBUTE);
        }
        
    }
}
