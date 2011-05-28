using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PIM.MacroWrappers
{
    [PublicCommand("Move PIM association end to neighboring PIM Class", PublicCommandAttribute.EPulicCommandCategory.PIM_atomic)]
    public class cmdMoveAssociationEnd : MacroCommand
    {
        [PublicArgument("PIMAssociationEnd", typeof(PIMAssociationEnd))]
        [Scope(ScopeAttribute.EScope.PIMAssociationEnd)]
        public Guid AssociationEndGuid { get; set; }

        [PublicArgument("New class", typeof(PIMClass), ModifiedPropertyName = "PIMAssociation")]
        public Guid NewClassGuid { get; set; }

        public cmdMoveAssociationEnd() { }
        
        public cmdMoveAssociationEnd(Controller c)
            : base(c) { }

        public void Set(Guid pimAssociationEndGuid, Guid pimClassGuid)
        {
            NewClassGuid = pimClassGuid;
            AssociationEndGuid = pimAssociationEndGuid;
            
        }

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdMoveAssociationEnd(Controller, AssociationEndGuid, NewClassGuid));
        }

    }
}
