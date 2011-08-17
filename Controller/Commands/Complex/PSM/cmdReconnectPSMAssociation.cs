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
    [PublicCommand("Reconnect PSM association (complex)", PublicCommandAttribute.EPulicCommandCategory.PSM_complex)]
    public class cmdReconnectPSMAssociation : ComposedCommand
    {
        [PublicArgument("Association", typeof(PSMAssociation))]
        [Scope(ScopeAttribute.EScope.PSMAssociation)]
        public Guid AssociationGuid { get; set; }

        [PublicArgument("New parent (PSM Association Member)", typeof(PSMAssociationMember))]
        public Guid NewParentGuid { get; set; }

        public cmdReconnectPSMAssociation()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdReconnectPSMAssociation(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid psmAssociationGuid, Guid parentGuid)
        {
            NewParentGuid = parentGuid;
            AssociationGuid = psmAssociationGuid;
        }

        protected override void GenerateSubCommands()
        {
            PSMAssociation association = Project.TranslateComponent<PSMAssociation>(AssociationGuid);
            PSMAssociationMember source = association.Parent;
            PSMAssociationMember target = Project.TranslateComponent<PSMAssociationMember>(NewParentGuid);

            if ((target is PSMClass) && (source is PSMClass)
                    && ((target as PSMClass).RepresentedClass == source
                    || (source as PSMClass).RepresentedClass == target))
            {
                Commands.Add(new acmdReconnectPSMAssociation(Controller, AssociationGuid, target) { Propagate = Propagate });
            }
            else
            {
                List<PSMAssociationMember> intermediateMembers = new List<PSMAssociationMember>();
                PSMAssociationMember common = source.GetNearestCommonAncestorAssociationMember(target);
                Debug.Assert(common != null, "Common Ancestor Class Null");

                if (common != source)
                {
                    //move up to common PSMAssociationMember
                    PSMAssociationMember parent = source.ParentAssociation.Parent;
                    while (parent != common)
                    {
                        intermediateMembers.Add(parent);
                        Debug.Assert(parent.ParentAssociation != null, "Did not find common Association Member");
                        parent = parent.ParentAssociation.Parent;
                    }
                    intermediateMembers.Add(common);
                }

                if (common.IsDescendantFrom(target))
                {
                    //move up
                    PSMAssociationMember parent = common.ParentAssociation.Parent;
                    while (parent != target)
                    {
                        intermediateMembers.Add(parent);
                        Debug.Assert(parent.ParentAssociation != null, "Did not find common Association Member");
                        parent = parent.ParentAssociation.Parent;
                    }
                    intermediateMembers.Add(target);
                }
                else if (target.IsDescendantFrom(common))
                {
                    //move down
                    List<PSMAssociationMember> intermediateMembers2 = new List<PSMAssociationMember>();
                    intermediateMembers2.Add(target);
                    PSMAssociationMember parent = target.ParentAssociation.Parent;
                    while (parent != common)
                    {
                        intermediateMembers2.Add(parent);
                        Debug.Assert(parent.ParentAssociation != null, "Did not find common Association Member");
                        parent = parent.ParentAssociation.Parent;
                    }
                    intermediateMembers2.Reverse();
                    intermediateMembers.AddRange(intermediateMembers2);
                }
                else if (target == common)
                {
                    //nothing
                }
                else
                {
                    Debug.Assert(false, "error - common association member not reachable?");
                }

                foreach (PSMAssociationMember psmAssociationMember in intermediateMembers)
                {
                    Commands.Add(new acmdReconnectPSMAssociation(Controller, AssociationGuid, psmAssociationMember) { Propagate = Propagate });
                }
            }
        }

        public override bool CanExecute()
        {
            PSMAssociation association = Project.TranslateComponent<PSMAssociation>(AssociationGuid);
            PSMAssociationMember target = Project.TranslateComponent<PSMAssociationMember>(NewParentGuid);
            PSMAssociationMember source = association.Parent;
            if ((target is PSMClass) && (source is PSMClass)
                    && ((target as PSMClass).RepresentedClass == source
                    || (source as PSMClass).RepresentedClass == target)) return true;
            
            if (source.GetNearestCommonAncestorAssociationMember(target) == null)
            {
                ErrorDescription = CommandErrors.CMDERR_NO_COMMON_ANCESTOR_ASSOC_MEMBER;
                return false;
            }
            return true;
        }

        internal override void CommandOperation()
        {
            base.CommandOperation();
            Report = new CommandReport(CommandReports.COMPLEX_RECONNECT_PSM_ASSOCIATION);
        }
    }
}
