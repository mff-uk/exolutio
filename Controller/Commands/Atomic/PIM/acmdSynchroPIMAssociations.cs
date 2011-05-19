using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Model.PIM;
using EvoX.Model;

namespace EvoX.Controller.Commands.Atomic.PIM
{
    public class acmdSynchroPIMAssociations : StackedCommand
    {
        public List<Guid> X1 = new List<Guid>();
        public List<Guid> X2 = new List<Guid>();

        public acmdSynchroPIMAssociations(Controller c)
            : base(c) { }

        public override bool CanExecute()
        {
            List<PIMAssociation> aX1 = X1.Select<Guid, PIMAssociation>(G => Project.TranslateComponent<PIMAssociation>(G)).ToList<PIMAssociation>();
            List<PIMAssociation> aX2 = X2.Select<Guid, PIMAssociation>(G => Project.TranslateComponent<PIMAssociation>(G)).ToList<PIMAssociation>();
            if (aX1.Count == 0 && aX2.Count == 0) return false;
            PIMClass pimClass1 = aX1.Count == 0 ? aX2[0].PIMClasses[0] : aX1[0].PIMClasses[0];
            PIMClass pimClass2 = aX1.Count == 0 ? aX2[0].PIMClasses[1] : aX1[0].PIMClasses[1];

            IEnumerable<PIMAssociation> intersect = pimClass1.PIMAssociationEnds.Select<PIMAssociationEnd, PIMAssociation>(e => e.PIMAssociation)
                .Intersect<PIMAssociation>(
                pimClass2.PIMAssociationEnds.Select<PIMAssociationEnd, PIMAssociation>(e => e.PIMAssociation));

            if (
                !aX1.All<PIMAssociation>(a => intersect.Contains<PIMAssociation>(a))
                ||
                !aX2.All<PIMAssociation>(a => intersect.Contains<PIMAssociation>(a))
               )
            {
                ErrorDescription = CommandErrors.CMDERR_SYNCHRO_PIM_ASSOC_NOT_SUBSET;
                return false;
            }
            return true;
        }
        
        internal override void CommandOperation()
        {
            Report = new CommandReport();
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            return OperationResult.OK;
        }
    }
}
