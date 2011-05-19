using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Model.PSM;
using EvoX.Model;

namespace EvoX.Controller.Commands.Atomic.PSM
{
    public class acmdSynchroPSMAssociations : StackedCommand
    {
        public List<Guid> X1 = new List<Guid>();
        public List<Guid> X2 = new List<Guid>();

        public acmdSynchroPSMAssociations(Controller c)
            : base(c) { }

        public override bool CanExecute()
        {
            IEnumerable<PSMAssociation> aX1 = X1.Select<Guid, PSMAssociation>(G => Project.TranslateComponent<PSMAssociation>(G));
            IEnumerable<PSMAssociation> aX2 = X2.Select<Guid, PSMAssociation>(G => Project.TranslateComponent<PSMAssociation>(G));
            if (aX1.Count() == 0 && aX2.Count() == 0)
            {
                ErrorDescription = CommandErrors.CMDERR_CANNOT_SYNCHRO_EMPTY_SETS;
                return false;
            }
            
            /*PSMClass psmClass = aX1.Count() == 0 ? aX2[0].PSMClass : aX1[0].PSMClass;
            if (!aX1.All<PSMAttribute>(a => a.PSMClass == psmClass) || !aX2.All<PSMAttribute>(a => a.PSMClass == psmClass))
            {
                ErrorDescription = CommandErrors.CMDERR_CANNOT_SYNCHRO_ATTS_DIFFERENT_CLASSES;
                return false;
            }*/
            return true;
        }
       
    }
}
