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
    [PublicCommand("Delete PSM association (recursive)", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdDeletePSMAssociationRecursive : ComposedCommand
    {
        [PublicArgument("Deleted PSM association", typeof(PSMAssociation))]
        [Scope(ScopeAttribute.EScope.PSMAssociation)]
        public Guid AssociationGuid { get; set; }
        
        public cmdDeletePSMAssociationRecursive()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdDeletePSMAssociationRecursive(Controller c)
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
            PSMAssociation association = Project.TranslateComponent<PSMAssociation>(AssociationGuid);
            Commands.Add(new cmdDeletePSMAssociation(Controller) { AssociationGuid = association });
            if (association.Child != null)
            {
                cmdDeletePSMAssociationMemberRecursive dam = new cmdDeletePSMAssociationMemberRecursive(Controller);
                dam.Set(association.Child);
                Commands.Add(dam);
            }
        }

        public override bool CanExecute()
        {
            if (AssociationGuid == Guid.Empty) return false;
            return base.CanExecute();
        }

        internal override void CommandOperation()
        {
            base.CommandOperation();
            Report = new CommandReport(CommandReports.COMPLEX_DELETE_PSM_ASSOC_RECURSIVE);
        }

    }
}
