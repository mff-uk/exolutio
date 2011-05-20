using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Model;
using EvoX.Model.PSM;
using EvoX.Controller.Commands.Atomic;
using EvoX.Controller.Commands.Atomic.PSM;

namespace EvoX.Controller.Commands.Complex.PSM
{
    public class cmdDeletePSMAssociationRecursive : MacroCommand
    {
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

            Commands.Add(new acmdUpdatePSMAssociationCardinality(Controller, association, 1, 1) { Propagate = false });
            Commands.Add(new acmdRenameComponent(Controller, association, "") { Propagate = false });
            if (association.Child != null)
            {
                cmdDeletePSMAssociationMember dam = new cmdDeletePSMAssociationMember(Controller);
                dam.Set(association.Child);
                Commands.Add(dam);
            }
            if (!(association.Child is PSMContentModel))
                Commands.Add(new acmdDeletePSMAssociation(Controller, association));
            
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
