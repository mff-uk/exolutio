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
    internal class acmdSetPSMAssociationInterpretation : acmdSetInterpretation
    {
        public acmdSetPSMAssociationInterpretation(Controller c, Guid interpretedPSMComponentGuid, Guid pimInterpretationGuid)
            : base(c, interpretedPSMComponentGuid, pimInterpretationGuid)
        { }

        public override bool CanExecute()
        {
            if (ForceExecute) return true;
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

            if (child.Interpretation == null)
            {
                ErrorDescription = CommandErrors.CMDERR_CANNOT_SET_INTERPRETATION_CMDERR_CANNOT_SET_INTERPRETATION_CHILD_NOT_INTERPRETED;
                return false;
            }

            PSMClass intclass = psmAssoc.NearestInterpretedClass();
            if (intclass == null)
            {
                ErrorDescription = CommandErrors.CMDERR_CANNOT_SET_INTERPRETATION_NO_INTCLASS;
                return false;
            }

            if (!(child.Interpretation as PIMClass).GetAssociationsWithIncludeInherited(intclass.Interpretation as PIMClass).Contains<PIMAssociation>(pimAssoc))
            {
                ErrorDescription = CommandErrors.CMDERR_NO_COMMON_INTERPRETED_ASSOCIATION;
                return false;
            }
            return true;            
        }

        internal override void CommandOperation()
        {
            PSMAssociation c = Project.TranslateComponent<PSMAssociation>(PSMComponentGuid);
            PIMAssociation oldInterpretation = c.Interpretation as PIMAssociation;
            if (c.UsedGeneralizations.Count > 0) oldUsedGeneralizations.AddRange(c.UsedGeneralizations.Select(g => g.ID));
            if (c.Interpretation == null) oldPimComponentGuid = Guid.Empty;
            else oldPimComponentGuid = c.Interpretation;
            if (PIMComponentGuid != Guid.Empty)
            {
                c.Interpretation = Project.TranslateComponent<PIMAssociation>(PIMComponentGuid);
                
                //TODO: Work with ordered image to get it easily
                //c.UsedGeneralizations = (c.NearestInterpretedClass().Interpretation as PIMClass).GetGeneralizationPathTo((c.Interpretation as PIMAttribute).PIMClass).Select(x => x.ID).ToList();
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
