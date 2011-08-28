using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Controller.Commands.Reflection;
using Exolutio.Model.PSM;
using Exolutio.Model.PIM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM
{
    internal class acmdSetPSMAttributeInterpretation : acmdSetInterpretation
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

            if (pimClass1 != pimClass2 && !pimClass2.GetGeneralClasses().Contains(pimClass1))
            {
                ErrorDescription = CommandErrors.CMDERR_CANNOT_SET_INTERPRETATION_CLASSES_DONT_MATCH;
                return false;
            }
            return true;
        }
        
        internal override void CommandOperation()
        {
            PSMAttribute c = Project.TranslateComponent<PSMAttribute>(PSMComponentGuid);
            PIMAttribute oldInterpretation = c.Interpretation as PIMAttribute;
            if (c.UsedGeneralizations.Count > 0) oldUsedGeneralizations.AddRange(c.UsedGeneralizations.Select(g => g.ID));
            if (c.Interpretation == null) oldPimComponentGuid = Guid.Empty;
            else oldPimComponentGuid = c.Interpretation;
            if (PIMComponentGuid != Guid.Empty)
            {
                c.Interpretation = Project.TranslateComponent<PIMAttribute>(PIMComponentGuid);
                c.UsedGeneralizations.Clear();
                foreach (PIMGeneralization g in (c.NearestInterpretedClass().Interpretation as PIMClass).GetGeneralizationPathTo((c.Interpretation as PIMAttribute).PIMClass))
                {
                    c.UsedGeneralizations.Add(g);
                }
            }
            else
            {
                c.Interpretation = null;
                c.UsedGeneralizations.Clear();
            }
            Report = new CommandReport(CommandReports.SET_INTERPRETATION, c, oldInterpretation, c.Interpretation);
        }
    }
}
