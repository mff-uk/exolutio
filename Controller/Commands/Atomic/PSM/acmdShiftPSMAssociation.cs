using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Model.PSM;
using EvoX.Model;

namespace EvoX.Controller.Commands.Atomic.PSM
{
    public class acmdShiftPSMAssociation : StackedCommand
    {
        Guid associationGuid = Guid.Empty;
        bool left = true;

        public acmdShiftPSMAssociation(Controller c, Guid psmAssociationGuid, bool shiftLeft)
            : base(c)
        {
            associationGuid = psmAssociationGuid;
            left = shiftLeft;
        }

        public override bool CanExecute()
        {
            return Project.TranslateComponent<PSMAssociation>(associationGuid).Parent.ChildPSMAssociations.Count > 1;
        }

        internal override void CommandOperation()
        {
            PSMAssociation a = Project.TranslateComponent<PSMAssociation>(associationGuid);
            PSMAssociationMember parent = a.Parent;
            int index = parent.ChildPSMAssociations.IndexOf(a);
            int count = parent.ChildPSMAssociations.Count;

            if (left) index = (index + count - 1) % count;
            else index = (index + 1) % count;
            parent.ChildPSMAssociations.Remove(a);
            parent.ChildPSMAssociations.Insert(a, index);

            Report = new CommandReport(CommandReports.PSM_ASSOC_SHIFT, a, left ? "left" : "right");
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PSMAssociation a = Project.TranslateComponent<PSMAssociation>(associationGuid);
            PSMAssociationMember parent = a.Parent;
            int index = parent.ChildPSMAssociations.IndexOf(a);
            int count = parent.ChildPSMAssociations.Count;

            if (left) index = (index + 1) % count;
            else index = (index - 1) % count;
            parent.ChildPSMAssociations.Remove(a);
            parent.ChildPSMAssociations.Insert(a, index);
            return OperationResult.OK;
        }
    }
}
