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
    [PublicCommand("Generalize PSM association (complex)", PublicCommandAttribute.EPulicCommandCategory.PSM_complex)]
    public class cmdGeneralizePSMAssociation : ComposedCommand
    {
        [PublicArgument("Association", typeof(PSMAssociation))]
        [Scope(ScopeAttribute.EScope.PSMAssociation)]
        public Guid AssociationGuid { get; set; }

        [PublicArgument("Target General PSM Class", typeof(PSMClass))]
        public Guid PSMClassGuid { get; set; }

        public cmdGeneralizePSMAssociation(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdGeneralizePSMAssociation()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid associationGuid, Guid targetGeneralPSMClass)
        {
            AssociationGuid = associationGuid;
            PSMClassGuid = targetGeneralPSMClass;
        }

        public override bool CanExecute()
        {
            if (AssociationGuid == Guid.Empty || PSMClassGuid == Guid.Empty)
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }
            
            PSMAssociation association = Project.TranslateComponent<PSMAssociation>(AssociationGuid);
            PSMClass oldclass = association.Parent as PSMClass;
            if (oldclass == null)
            {
                ErrorDescription = CommandErrors.CMDERR_PARENT_NOT_PSMCLASS;
                return false;
            }

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
            PSMAssociation association = Project.TranslateComponent<PSMAssociation>(AssociationGuid);
            PSMClass oldclass = association.Parent as PSMClass;
            PSMClass newclass = Project.TranslateComponent<PSMClass>(PSMClassGuid);
            List<PSMGeneralization> path = oldclass.GetGeneralizationPathTo(newclass);
            for (int i = 0; i < path.Count; i++)
            {
                Commands.Add(new acmdGeneralizePSMAssociation(Controller, association) { Propagate = Propagate });
            }
        }        
    }
}
