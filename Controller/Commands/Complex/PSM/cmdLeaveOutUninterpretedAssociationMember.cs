using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Atomic;
using Exolutio.Controller.Commands.Atomic.PSM;
using System.Diagnostics;

namespace Exolutio.Controller.Commands.Complex.PSM
{
    /// <summary>
    /// Leaves out content model or uninterpreted class
    /// </summary>
    [PublicCommand("Leave out uninterpreted association member (complex)", PublicCommandAttribute.EPulicCommandCategory.PSM_complex)]
    public class cmdLeaveOutUninterpretedAssociationMember : ComposedCommand
    {
        [PublicArgument("Left out PSM content model or uninterpreted class", typeof(PSMAssociationMember))]
        [Scope(ScopeAttribute.EScope.PSMContentModel | ScopeAttribute.EScope.PSMClass)]
        public Guid AssociationMemberGuid { get; set; }

        public cmdLeaveOutUninterpretedAssociationMember()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdLeaveOutUninterpretedAssociationMember(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid amGuid)
        {
            AssociationMemberGuid = amGuid;
            
        }

        protected override void GenerateSubCommands()
        {
            PSMAssociationMember psmAssociationMember = Project.TranslateComponent<PSMAssociationMember>(AssociationMemberGuid);
            if (psmAssociationMember.ParentAssociation != null)
            {
                foreach (PSMAssociation a in psmAssociationMember.ChildPSMAssociations)
                {
                    Commands.Add(new acmdReconnectPSMAssociation(Controller, a, psmAssociationMember.ParentAssociation.Parent));
                }
                if (psmAssociationMember is PSMClass)
                {
                    PSMClass parent = psmAssociationMember.NearestParentClass();
                    if (parent == null)
                    {
                        foreach (PSMAttribute a in (psmAssociationMember as PSMClass).PSMAttributes)
                        {
                            Commands.Add(new cmdDeletePSMAttribute(Controller) { AttributeGuid = a });
                        }
                    }
                    else
                    {
                        foreach (PSMAttribute a in (psmAssociationMember as PSMClass).PSMAttributes)
                        {
                            Commands.Add(new acmdMovePSMAttribute(Controller, a, parent));
                        }
                    }
                }
                Commands.Add(new cmdDeletePSMAssociation(Controller) { AssociationGuid = psmAssociationMember.ParentAssociation });
            }
            if (psmAssociationMember is PSMClass)
                Commands.Add(new cmdDeleteRootPSMClass(Controller) { ClassGuid = psmAssociationMember });
            else if (psmAssociationMember is PSMContentModel)
                Commands.Add(new cmdDeleteRootPSMContentModel(Controller) { ContentModelGuid = psmAssociationMember });
            else
                Debug.Assert(false, "Unknown Association Member Type");
        }

        public override bool CanExecute()
        {
            if (AssociationMemberGuid == Guid.Empty) return false;
            PSMAssociationMember associationMember = Project.TranslateComponent<PSMAssociationMember>(AssociationMemberGuid);
            if (associationMember.ParentAssociation != null && associationMember.ParentAssociation.Interpretation != null) return false;
            if (associationMember is PSMClass && (associationMember as PSMClass).Interpretation != null) return false;
            if (associationMember is PSMSchemaClass) return false;
            return base.CanExecute();
        }

        internal override void CommandOperation()
        {
            base.CommandOperation();
            Report = new CommandReport(CommandReports.COMPLEX_LEAVE_OUT_UNINT_AM);
        }

    }
}
