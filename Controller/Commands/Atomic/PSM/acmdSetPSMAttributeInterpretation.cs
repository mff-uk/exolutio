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
    public class acmdSetPSMAttributeInterpretation : acmdSetInterpretation
    {
        public acmdSetPSMAttributeInterpretation(Controller c, Guid interpretedPSMComponentGuid, Guid pimInterpretationGuid)
            : base(c, interpretedPSMComponentGuid, pimInterpretationGuid)
        { }

        public override bool CanExecute()
        {
            if (!(PSMComponentGuid != Guid.Empty
                && PIMComponentGuid == Guid.Empty || (Project.VerifyComponentType<PSMAttribute>(PSMComponentGuid) && Project.VerifyComponentType<PIMAttribute>(PIMComponentGuid))
                ))
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }

            if (PIMComponentGuid == Guid.Empty) return true;

            PIMClass pimClass1 = Project.TranslateComponent<PIMAttribute>(PIMComponentGuid).PIMClass;
            PSMClass niClass = Project.TranslateComponent<PSMAttribute>(PSMComponentGuid).NearestInterpretedClass();
            if (niClass == null)
            {
                ErrorDescription = CommandErrors.CMDERR_CANNOT_SET_INTERPRETATION_NO_INTCLASS;
                return false;
            }
            PIMClass pimClass2 = niClass.Interpretation as PIMClass;

            if (pimClass1 != pimClass2)
            {
                ErrorDescription = CommandErrors.CMDERR_CANNOT_SET_INTERPRETATION_CLASSES_DONT_MATCH;
                return false;
            }
            return true;
        }
    }
}
