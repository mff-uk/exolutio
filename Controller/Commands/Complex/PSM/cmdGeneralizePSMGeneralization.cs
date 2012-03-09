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
    [PublicCommand("Generalize PSM generalization (complex)", PublicCommandAttribute.EPulicCommandCategory.PSM_complex)]
    public class cmdGeneralizePSMGeneralization : ComposedCommand
    {
        [PublicArgument("Generalization", typeof(PSMGeneralization))]
        [Scope(ScopeAttribute.EScope.PSMGeneralization)]
        public Guid GeneralizationGuid { get; set; }

        [PublicArgument("Target General PSM Class", typeof(PSMClass))]
        public Guid PSMClassGuid { get; set; }

        public cmdGeneralizePSMGeneralization(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdGeneralizePSMGeneralization()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid generalizationGuid, Guid targetGeneralPSMClass)
        {
            GeneralizationGuid = generalizationGuid;
            PSMClassGuid = targetGeneralPSMClass;
        }

        public override bool CanExecute()
        {
            if (GeneralizationGuid == Guid.Empty || PSMClassGuid == Guid.Empty)
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }
            
            PSMGeneralization generalization = Project.TranslateComponent<PSMGeneralization>(GeneralizationGuid);
            PSMClass oldclass = generalization.General;
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
            PSMGeneralization generalization = Project.TranslateComponent<PSMGeneralization>(GeneralizationGuid);
            PSMClass oldclass = generalization.General;
            PSMClass newclass = Project.TranslateComponent<PSMClass>(PSMClassGuid);
            List<PSMGeneralization> path = oldclass.GetGeneralizationPathTo(newclass);
            for (int i = 0; i < path.Count; i++)
            {
                Commands.Add(new acmdGeneralizePSMGeneralization(Controller, generalization) { Propagate = Propagate });
            }
        }        
    }
}
