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

            if (representedClass.RepresentedClass == representantClass)
            {
                ErrorDescription = CommandErrors.CMDERR_CYCLIC_REPR;
                return false;
            }
            return true;
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
            Report = new CommandReport(CommandReports.STRUCTURAL_REPRESENTANT_CHANGED, representantClass, Project.TranslateComponent<PSMClass>(oldRepresented), representantClass.RepresentedClass);
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
