using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Shift PSM root", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdShiftPSMRoot : MacroCommand
    {
        [PublicArgument("PSM root", typeof(PSMAssociationMember))]
        [Scope(ScopeAttribute.EScope.PSMClass | ScopeAttribute.EScope.PSMContentModel | ScopeAttribute.EScope.PSMSchemaClass)]
        public Guid RootGuid { get; set; }

        [PublicArgument("Shift left", SuggestedValue = true)]
        public bool Left { get; set; }

        public cmdShiftPSMRoot() { }

        public cmdShiftPSMRoot(Controller c)
            : base(c) { }

        public void Set(Guid psmRootGuid, bool shiftLeft)
        {
            RootGuid = psmRootGuid;
            Left = shiftLeft;
            
        }

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdShiftPSMRoot(Controller, RootGuid, Left));
        }

    }
}
