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
    internal class acmdSetPSMClassInterpretation : acmdSetInterpretation
    {
        internal acmdSetPSMClassInterpretation(Controller c, Guid interpretedPSMComponentGuid, Guid pimInterpretationGuid)
            : base(c, interpretedPSMComponentGuid, pimInterpretationGuid)
        { }

        public override bool CanExecute()
        {
            if (!(PSMComponentGuid != Guid.Empty
                && PIMComponentGuid == Guid.Empty || (Project.VerifyComponentType<PSMClass>(PSMComponentGuid) && Project.VerifyComponentType<PIMClass>(PIMComponentGuid))
                ))
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }

            //PSMClass does not have representants when interpretation is to be changed
            PSMClass c = Project.TranslateComponent<PSMClass>(PSMComponentGuid);
            if (c.Representants.Count > 0)
            {
                ErrorDescription = CommandErrors.CMDERR_CLASS_IS_REPRESENTED;
                return false;
            }

            //PSM attributes within the uninterpreted PSM Class subtree cannot have interpretations
            if (!c.UnInterpretedSubClasses()
                  .SelectMany<PSMClass, PSMAttribute>(cl => cl.PSMAttributes)
                  .Union<PSMAttribute>(c.PSMAttributes)
                  .All<PSMAttribute>(a => a.Interpretation == null)
                )
            {
                ErrorDescription = CommandErrors.CMDERR_UNINTERPRETED_SUBCLASS_ATTRIBUTES_INTERPRETED;
                return false;
            }

            //PSM associations within the uninterpreted PSM Class subtree cannot have interpretations
            if (!(c.UnInterpretedSubClasses()
                  .Select<PSMClass, PSMAssociation>(cl => cl.ParentAssociation)
                  .All<PSMAssociation>(a => a.Interpretation == null)
                 && (c.ParentAssociation == null || c.ParentAssociation.Interpretation == null)))
            {
                ErrorDescription = CommandErrors.CMDERR_UNINTERPRETED_SUBCLASS_ASSOCIATIONS_INTERPRETED;
                return false;
            }

            return true;
        }
    }
}
