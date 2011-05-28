using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Controller.Commands.Atomic.PIM;

namespace Exolutio.Controller.Commands.Atomic.PSM
{
    public class acmdReconnectPSMAssociation : StackedCommand
    {
        Guid associationGuid, newParentGuid, oldParentGuid;
        int index;

        public acmdReconnectPSMAssociation(Controller c, Guid psmAssociationGuid, Guid parentGuid)
            : base(c)
        {
            newParentGuid = parentGuid;
            associationGuid = psmAssociationGuid;
        }

        public override bool CanExecute()
        {
            if (!(newParentGuid != Guid.Empty
                && Project.VerifyComponentType<PSMAssociationMember>(newParentGuid)
                && associationGuid != Guid.Empty
                && Project.VerifyComponentType<PSMAssociation>(associationGuid)
                && Project.TranslateComponent<PSMAssociation>(associationGuid).Parent is PSMAssociationMember))
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }

            PSMAssociationMember newParent = Project.TranslateComponent<PSMAssociationMember>(newParentGuid);
            PSMAssociationMember oldParent = Project.TranslateComponent<PSMAssociation>(associationGuid).Parent as PSMAssociationMember;

            //the two parents connected by an association (atomic operation)
            if (newParent.ParentAssociation != null && newParent.ParentAssociation.Parent == oldParent) return true;
            if (oldParent.ParentAssociation != null && oldParent.ParentAssociation.Parent == newParent) return true;

            if ((newParent is PSMClass) && (oldParent is PSMClass) 
                && ((newParent as PSMClass).RepresentedClass == oldParent 
                    || (oldParent as PSMClass).RepresentedClass == newParent)) return true;

            ErrorDescription = CommandErrors.CMDERR_NO_COMMON_ASSOCIATION_OR_REPR;
            return false;

        }
        
        internal override void CommandOperation()
        {
            PSMAssociation psmAssociation = Project.TranslateComponent<PSMAssociation>(associationGuid);
            PSMAssociationMember oldParent = psmAssociation.Parent as PSMAssociationMember;
            oldParentGuid = oldParent;
            PSMAssociationMember newParent = Project.TranslateComponent<PSMAssociationMember>(newParentGuid);

            index = oldParent.ChildPSMAssociations.IndexOf(psmAssociation);
            oldParent.ChildPSMAssociations.Remove(psmAssociation);
            psmAssociation.Parent = newParent;
            newParent.ChildPSMAssociations.Add(psmAssociation);

            Report = new CommandReport(CommandReports.MOVE_PSM_ASSOCIATION, psmAssociation, oldParent, newParent);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PSMAssociation psmAssociation = Project.TranslateComponent<PSMAssociation>(associationGuid);
            PSMAssociationMember oldParent = Project.TranslateComponent<PSMAssociationMember>(oldParentGuid);
            PSMAssociationMember newParent = Project.TranslateComponent<PSMAssociationMember>(newParentGuid);

            newParent.ChildPSMAssociations.Remove(psmAssociation);
            psmAssociation.Parent = oldParent;
            oldParent.ChildPSMAssociations.Insert(psmAssociation, index);
            return OperationResult.OK;
        }
        internal override MacroCommand PostPropagation()
        {
            PSMAssociation psmAssociation = Project.TranslateComponent<PSMAssociation>(associationGuid);
            PSMAssociationMember source = Project.TranslateComponent<PSMAssociationMember>(oldParentGuid);
            PSMAssociationMember target = Project.TranslateComponent<PSMAssociationMember>(newParentGuid);

            PSMClass oldIntContext = (source is PSMClass && (source as PSMClass).Interpretation != null)? (source as PSMClass) : source.NearestInterpretedParentClass();
            PSMClass newIntContext = (target is PSMClass && (target as PSMClass).Interpretation != null) ? (target as PSMClass) : target.NearestInterpretedParentClass();

            if (oldIntContext == null) return null;
            if (oldIntContext.Interpretation == newIntContext.Interpretation) return null;

            MacroCommand command = new MacroCommand(Controller) { CheckFirstOnlyInCanExecute = true };
            command.Report = new CommandReport("Post-propagation (move PSM attribute)");

            if (psmAssociation.Interpretation == null)
            {
                if (newIntContext == null)
                {
                    foreach (PSMAttribute a in psmAssociation.Child.UnInterpretedSubClasses(true)
                        .SelectMany<PSMClass, PSMAttribute>(c => c.PSMAttributes)
                        .Where(a => a.Interpretation != null))
                    {
                        command.Commands.Add(new acmdSetPSMAttributeInterpretation(Controller, a, Guid.Empty));
                    }
                    foreach (PSMAssociation a in psmAssociation.Child.UnInterpretedSubClasses(true)
                        .SelectMany<PSMClass, PSMAssociation>(c => c.ChildPSMAssociations)
                        .Where(a => a.Interpretation != null))
                    {
                        command.Commands.Add(new acmdSetPSMAssociationInterpretation(Controller, a, Guid.Empty));
                    }
                }
                else
                {
                    foreach (PSMAttribute a in psmAssociation.Child.UnInterpretedSubClasses(true)
                        .SelectMany<PSMClass, PSMAttribute>(c => c.PSMAttributes)
                        .Where(a => a.Interpretation != null))
                    {
                        command.Commands.Add(new acmdMovePIMAttribute(Controller, a.Interpretation, newIntContext.Interpretation) { PropagateSource = a });
                    }
                    foreach (PSMAssociation a in psmAssociation.Child.UnInterpretedSubClasses(true)
                        .SelectMany<PSMClass, PSMAssociation>(c => c.ChildPSMAssociations)
                        .Where(a => a.Interpretation != null))
                    {
                        PIMAssociationEnd e = (a.Interpretation as PIMAssociation).PIMAssociationEnds.First(ae => ae.PIMClass == oldIntContext.Interpretation);
                        command.Commands.Add(new acmdMoveAssociationEnd(Controller, e, newIntContext.Interpretation) { PropagateSource = a });
                    }
                }
            }
            else
            {
                PIMAssociationEnd e = (psmAssociation.Interpretation as PIMAssociation).PIMAssociationEnds.First(ae => ae.PIMClass == oldIntContext.Interpretation);
                command.Commands.Add(new acmdMoveAssociationEnd(Controller, e, newIntContext.Interpretation) { PropagateSource = psmAssociation });
            }
            
            return command;
        }
    }
}
