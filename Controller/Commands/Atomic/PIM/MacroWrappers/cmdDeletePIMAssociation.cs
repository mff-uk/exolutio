using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Controller.Commands.Atomic;
using Exolutio.Controller.Commands.Atomic.PIM;

namespace Exolutio.Controller.Commands.Atomic.PIM.MacroWrappers
{
    [Obsolete("Atomic operation wrapper deprecated by an appropriate complex operation")]
    //[PublicCommand("Delete PIM association", PublicCommandAttribute.EPulicCommandCategory.PIM_atomic)]
    public class cmdDeletePIMAssociation : MacroCommand
    {
        [PublicArgument("Deleted association", typeof(PIMAssociation))]
        [Scope(ScopeAttribute.EScope.PIMAssociation)]
        public Guid AssociationGuid { get; set; }
        
        public cmdDeletePIMAssociation()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdDeletePIMAssociation(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid associationGuid)
        {
            AssociationGuid = associationGuid;
            
        }

        protected override void GenerateSubCommands()
        {
            Commands.AddRange(acmdRemoveComponentFromDiagram.CreateCommandsToRemoveFromAllDiagrams(Controller, AssociationGuid));
            Commands.Add(new acmdDeletePIMAssociation(Controller, AssociationGuid));
        }        
    }
}
