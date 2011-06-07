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
    public class acmdSetRepresentedClass : StackedCommand
    {
        Guid representant, represented, oldRepresented;

        public acmdSetRepresentedClass(Controller c, Guid representantPSMClass, Guid representedPSMClass)
            : base(c)
        {
            represented = representedPSMClass;
            representant = representantPSMClass;
        }

        public override bool CanExecute()
        {
            if (!(representant != Guid.Empty
                && Project.VerifyComponentType<PSMClass>(representant)))
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }

            if (represented == Guid.Empty) return true;

            PSMClass representantClass = Project.TranslateComponent<PSMClass>(representant);
            PSMClass representedClass = Project.TranslateComponent<PSMClass>(represented);

            if (representantClass.Interpretation != representedClass.Interpretation)
            {
                ErrorDescription = CommandErrors.CMDERR_NOT_SAME_INTERPRETATION;
                return false;
            }

            if (representedClass.RepresentedClass == representantClass || representedClass == representantClass)
            {
                ErrorDescription = CommandErrors.CMDERR_CYCLIC_REPR;
                return false;
            }

            PSMClass representedNIC = representedClass.NearestInterpretedClass();
            PSMClass representantNIC = representantClass.NearestInterpretedClass();
            if (representedNIC == null && representantNIC == null) return true;
            if (representedNIC == null || representantNIC == null)
            {
                ErrorDescription = CommandErrors.CMDERR_REPR_DIFFERENT_CONTEXT;
                return false;
            }
            if (representedNIC.Interpretation == representantNIC.Interpretation) return true;
            else
            {
                ErrorDescription = CommandErrors.CMDERR_REPR_DIFFERENT_CONTEXT;
                return false;
            }
        }
        
        internal override void CommandOperation()
        {
            PSMClass representantClass = Project.TranslateComponent<PSMClass>(representant);
            oldRepresented = representantClass.RepresentedClass;
            if (represented == Guid.Empty)
            {
                representantClass.RepresentedClass = null;
            }
            else
            {
                PSMClass representedClass = Project.TranslateComponent<PSMClass>(represented);
                representantClass.RepresentedClass = representedClass;
            }
            PSMClass old = oldRepresented == Guid.Empty ? null : Project.TranslateComponent<PSMClass>(oldRepresented);
            Report = new CommandReport(CommandReports.STRUCTURAL_REPRESENTANT_CHANGED, representantClass, old, representantClass.RepresentedClass);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PSMClass representantClass = Project.TranslateComponent<PSMClass>(representant);
            if (oldRepresented == Guid.Empty)
            {
                representantClass.RepresentedClass = null;
            }
            else
            {
                PSMClass oldRepresentedClass = Project.TranslateComponent<PSMClass>(oldRepresented);
                representantClass.RepresentedClass = oldRepresentedClass;
            }
            return OperationResult.OK;
        }
    }
}
