using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM
{
    internal class acmdShiftPSMRoot : AtomicCommand
    {
        Guid associationMemberGuid = Guid.Empty;
        bool left = true;

        public acmdShiftPSMRoot(Controller c, Guid psmAssociationMember, bool shiftLeft)
            : base(c)
        {
            associationMemberGuid = psmAssociationMember;
            left = shiftLeft;
        }

        public override bool CanExecute()
        {
            return Project.TranslateComponent<PSMAssociationMember>(associationMemberGuid).ParentAssociation == null;
        }
        
        internal override void CommandOperation()
        {
            PSMAssociationMember root = Project.TranslateComponent<PSMAssociationMember>(associationMemberGuid);
            int index = root.PSMSchema.Roots.IndexOf(root);
            int count = root.PSMSchema.Roots.Count;

            if (left) index = (index + count - 1) % count;
            else index = (index + 1) % count;
            root.PSMSchema.Roots.Remove(root);
            root.PSMSchema.Roots.Insert(root, index);

            Report = new CommandReport(CommandReports.SHIFT_PSM_ROOT, root, left ? "left" : "right");
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PSMAssociationMember root = Project.TranslateComponent<PSMAssociationMember>(associationMemberGuid);
            int index = root.PSMSchema.Roots.IndexOf(root);
            int count = root.PSMSchema.Roots.Count;

            if (left) index = (index + 1) % count;
            else index = (index - 1) % count;
            root.PSMSchema.Roots.Remove(root);
            root.PSMSchema.Roots.Insert(root, index);
            return OperationResult.OK;
        }
    }
}
