using System;
using Exolutio.Model.PIM;
using Exolutio.Model;
using Exolutio.Model.PSM;
using System.Collections.Generic;
using System.Linq;
using Exolutio.Controller.Commands.Atomic.PSM;
using System.Diagnostics;
using Exolutio.Controller.Commands.Complex.PSM;

namespace Exolutio.Controller.Commands.Complex.PSM
{
    [PublicCommand("Generalize PSM attribute (complex)", PublicCommandAttribute.EPulicCommandCategory.PSM_complex)]
    public class cmdGeneralizePSMAttribute : ComposedCommand
    {
        [PublicArgument("Attribute", typeof(PSMAttribute))]
        [Scope(ScopeAttribute.EScope.PSMAttribute)]
        public Guid AttributeGuid { get; set; }

        [PublicArgument("Target General PSM Class", typeof(PSMClass))]
        public Guid PSMClassGuid { get; set; }

        public cmdGeneralizePSMAttribute(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdGeneralizePSMAttribute()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid attributeGuid, Guid targetGeneralPSMClass)
        {
            AttributeGuid = attributeGuid;
            PSMClassGuid = targetGeneralPSMClass;
        }

        public override bool CanExecute()
        {
            if (AttributeGuid == Guid.Empty || PSMClassGuid == Guid.Empty)
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }
            
            PSMAttribute attribute = Project.TranslateComponent<PSMAttribute>(AttributeGuid);
            PSMClass oldclass = attribute.PSMClass;
            PSMClass newclass = Project.TranslateComponent<PSMClass>(PSMClassGuid);

            if (!oldclass.GetGeneralClasses().Contains(newclass))
            {
                ErrorDescription = CommandErrors.CMDERR_CLASS_NOT_GENERAL;
                return false;
            }

            return true;
        }

        internal override void GenerateSubCommands()
        {
            PSMAttribute attribute = Project.TranslateComponent<PSMAttribute>(AttributeGuid);
            PSMClass oldclass = attribute.PSMClass;
            PSMClass newclass = Project.TranslateComponent<PSMClass>(PSMClassGuid);
            List<PSMGeneralization> path = oldclass.GetGeneralizationPathTo(newclass);
            for (int i = 0; i < path.Count; i++)
            {
                Commands.Add(new acmdGeneralizePSMAttribute(Controller, attribute) { Propagate = Propagate });
            }
        }        
    }
}
