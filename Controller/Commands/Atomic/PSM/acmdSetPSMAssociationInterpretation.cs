using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Controller.Commands.Reflection;
using EvoX.Model.PSM;
using EvoX.Model.PIM;
using EvoX.Model;

namespace EvoX.Controller.Commands.Atomic.PSM
{
    public class acmdSetPSMAssociationInterpretation : acmdSetInterpretation
    {
        public acmdSetPSMAssociationInterpretation(Controller c, Guid interpretedPSMComponentGuid, Guid pimInterpretationGuid)
            : base(c, interpretedPSMComponentGuid, pimInterpretationGuid)
        { }

        public override bool CanExecute()
        {
            if (!(PSMComponentGuid != Guid.Empty
                && PIMComponentGuid == Guid.Empty || (Project.VerifyComponentType<PSMAssociation>(PSMComponentGuid) && Project.VerifyComponentType<PIMAssociation>(PIMComponentGuid))
                ))
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }

            if (PIMComponentGuid == Guid.Empty) return true;

            PIMAssociation pimAssoc = Project.TranslateComponent<PIMAssociation>(PIMComponentGuid);
            PSMAssociation psmAssoc = Project.TranslateComponent<PSMAssociation>(PSMComponentGuid);
            PSMClass child = psmAssoc.Child as PSMClass;

            if (child == null)
            {
                ErrorDescription = CommandErrors.CMDERR_CANNOT_SET_INTERPRETATION_CHILD_NOT_CLASS;
                return false;
            }

            PSMClass intclass = psmAssoc.NearestInterpretedClass();
            if (intclass == null)
            {
                ErrorDescription = CommandErrors.CMDERR_CANNOT_SET_INTERPRETATION_NO_INTCLASS;
                return false;
            }

            if (!(child.Interpretation as PIMClass).GetAssociationsWith(intclass.Interpretation as PIMClass).Contains<PIMAssociation>(pimAssoc))
            {
                ErrorDescription = CommandErrors.CMDERR_NO_COMMON_INTERPRETED_ASSOCIATION;
                return false;
            }
            return true;            
        }
    }
}
