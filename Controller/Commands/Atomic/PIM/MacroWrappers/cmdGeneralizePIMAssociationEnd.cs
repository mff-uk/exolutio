using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PIM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PIM.MacroWrappers
{
    [PublicCommand("Generalize PIM association end (atomic)", PublicCommandAttribute.EPulicCommandCategory.PIM_atomic)]
    public class cmdGeneralizePIMAssociationEnd : WrapperCommand
    {
        [PublicArgument("Association end", typeof(PIMAssociationEnd))]
        [Scope(ScopeAttribute.EScope.PIMAssociationEnd)]
        public Guid AssociationEndGuid { get; set; }

        public cmdGeneralizePIMAssociationEnd() { }

        public cmdGeneralizePIMAssociationEnd(Controller c)
            : base(c) { }
        
        public void Set(Guid associationEndGuid)
        {
            AssociationEndGuid = associationEndGuid;
        }

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdGeneralizePIMAssociationEnd(Controller, AssociationEndGuid));
        }
    }
}
