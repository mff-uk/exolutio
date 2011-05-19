﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Model.PSM;
using EvoX.Model;

namespace EvoX.Controller.Commands.Atomic.PSM
{
    public class acmdMovePSMAttribute : StackedCommand
    {
        Guid attributeGuid, newClassGuid, oldClassGuid;
        int index;

        public acmdMovePSMAttribute(Controller c, Guid psmAttributeGuid, Guid psmClassGuid)
            : base(c)
        {
            newClassGuid = psmClassGuid;
            attributeGuid = psmAttributeGuid;
        }

        public override bool CanExecute()
        {
            if (!(newClassGuid != Guid.Empty
                && Project.VerifyComponentType<PSMClass>(newClassGuid)
                && attributeGuid != Guid.Empty
                && Project.VerifyComponentType<PSMAttribute>(attributeGuid)))
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }

            PSMClass newClass = Project.TranslateComponent<PSMClass>(newClassGuid);
            PSMClass oldClass = Project.TranslateComponent<PSMAttribute>(attributeGuid).PSMClass;

            //the two classes connected by an association path across containers (atomic operation)
            if (newClass.NearestParentClass() == oldClass) return true;
            if (oldClass.NearestParentClass() == newClass) return true;

            if (newClass.RepresentedClass == oldClass || oldClass.RepresentedClass == newClass) return true;

            ErrorDescription = CommandErrors.CMDERR_NO_COMMON_ASSOCIATION_OR_REPR;
            return false;
        }
        
        internal override void CommandOperation()
        {
            PSMAttribute psmAttribute = Project.TranslateComponent<PSMAttribute>(attributeGuid);
            PSMClass oldClass = psmAttribute.PSMClass;
            oldClassGuid = oldClass;
            PSMClass newClass = Project.TranslateComponent<PSMClass>(newClassGuid);
            index = oldClass.PSMAttributes.IndexOf(psmAttribute);

            oldClass.PSMAttributes.Remove(psmAttribute);
            psmAttribute.PSMClass = newClass;
            newClass.PSMAttributes.Add(psmAttribute);
            Report = new CommandReport(CommandReports.COMPONENT_MOVED, psmAttribute, oldClass, newClass);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PSMAttribute psmAttribute = Project.TranslateComponent<PSMAttribute>(attributeGuid);
            PSMClass oldClass = Project.TranslateComponent<PSMClass>(oldClassGuid);
            PSMClass newClass = Project.TranslateComponent<PSMClass>(newClassGuid);

            newClass.PSMAttributes.Remove(psmAttribute);
            psmAttribute.PSMClass = oldClass;
            oldClass.PSMAttributes.Insert(psmAttribute, index);
            return OperationResult.OK;
        }
    }
}
