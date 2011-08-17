using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Controller.Commands.Atomic.PIM;
using System.Diagnostics;

namespace Exolutio.Controller.Commands.Atomic.PSM
{
    internal class acmdReconnectPSMAssociation : AtomicCommand
    {
        Guid associationGuid, newParentGuid, oldParentGuid;
        int index;
        List<Tuple<Guid, int>> schemaIndexes;

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

            PSMAssociation psmAssociation = Project.TranslateComponent<PSMAssociation>(associationGuid);
            PSMAssociationMember newParent = Project.TranslateComponent<PSMAssociationMember>(newParentGuid);
            PSMAssociationMember oldParent = Project.TranslateComponent<PSMAssociation>(associationGuid).Parent as PSMAssociationMember;

            if (newParent == psmAssociation.Child) return false;
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
            if (oldParent.PSMSchema != newParent.PSMSchema)
            {
                schemaIndexes = new List<Tuple<Guid, int>>();
                foreach (PSMComponent c in psmAssociation.GetPSMChildComponentsRecursive(true, true))
                {
                    if (c is PSMClass)
                    {
                        schemaIndexes.Add(Tuple.Create<Guid, int>(c, oldParent.PSMSchema.PSMClasses.Remove(c as PSMClass)));
                        newParent.PSMSchema.PSMClasses.Add(c as PSMClass);
                    }
                    else if (c is PSMAssociation)
                    {
                        schemaIndexes.Add(Tuple.Create<Guid, int>(c, oldParent.PSMSchema.PSMAssociations.Remove(c as PSMAssociation)));
                        newParent.PSMSchema.PSMAssociations.Add(c as PSMAssociation);
                    }
                    else if (c is PSMContentModel)
                    {
                        schemaIndexes.Add(Tuple.Create<Guid, int>(c, oldParent.PSMSchema.PSMContentModels.Remove(c as PSMContentModel)));
                        newParent.PSMSchema.PSMContentModels.Add(c as PSMContentModel);
                    }
                    else if (c is PSMAttribute)
                    {
                        schemaIndexes.Add(Tuple.Create<Guid, int>(c, oldParent.PSMSchema.PSMAttributes.Remove(c as PSMAttribute)));
                        newParent.PSMSchema.PSMAttributes.Add(c as PSMAttribute);
                    }
                    else
                    {
                        Debug.Assert(false, "Unknown type");
                    }
                }
            }
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
            if (oldParent.PSMSchema != newParent.PSMSchema)
            {
                foreach (Tuple<Guid, int> t in schemaIndexes.Reverse<Tuple<Guid, int>>())
                {
                    PSMComponent c = Project.TranslateComponent<PSMComponent>(t.Item1);
                    if (c is PSMClass)
                    {
                        newParent.PSMSchema.PSMClasses.Remove(c as PSMClass);
                        oldParent.PSMSchema.PSMClasses.Insert(c as PSMClass, t.Item2);
                    }
                    else if (c is PSMAssociation)
                    {
                        newParent.PSMSchema.PSMAssociations.Remove(c as PSMAssociation);
                        oldParent.PSMSchema.PSMAssociations.Insert(c as PSMAssociation, t.Item2);
                    }
                    else if (c is PSMContentModel)
                    {
                        newParent.PSMSchema.PSMContentModels.Remove(c as PSMContentModel);
                        oldParent.PSMSchema.PSMContentModels.Insert(c as PSMContentModel, t.Item2);
                    }
                    else if (c is PSMAttribute)
                    {
                        newParent.PSMSchema.PSMAttributes.Remove(c as PSMAttribute);
                        oldParent.PSMSchema.PSMAttributes.Insert(c as PSMAttribute, t.Item2);
                    }
                    else
                    {
                        Debug.Assert(false, "Unknown type");
                    }
                }
            }
            psmAssociation.Parent = oldParent;
            oldParent.ChildPSMAssociations.Insert(psmAssociation, index);
            return OperationResult.OK;
        }
        internal override PropagationMacroCommand PostPropagation()
        {
            PSMAssociation psmAssociation = Project.TranslateComponent<PSMAssociation>(associationGuid);
            PSMAssociationMember source = Project.TranslateComponent<PSMAssociationMember>(oldParentGuid);
            PSMAssociationMember target = Project.TranslateComponent<PSMAssociationMember>(newParentGuid);

            PSMClass oldIntContext = (source is PSMClass && (source as PSMClass).Interpretation != null)? (source as PSMClass) : source.NearestInterpretedParentClass();
            PSMClass newIntContext = (target is PSMClass && (target as PSMClass).Interpretation != null) ? (target as PSMClass) : target.NearestInterpretedParentClass();

            if (oldIntContext == null) return null;
            if (newIntContext != null && oldIntContext.Interpretation == newIntContext.Interpretation) return null;

            PropagationMacroCommand command = new PropagationMacroCommand(Controller) { CheckFirstOnlyInCanExecute = true };
            command.Report = new CommandReport("Post-propagation (reconnect parent PSM association end)");

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
            if (psmAssociation.Interpretation == null)
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
            else
            {
                if (newIntContext == null)
                {
                    command.Commands.Add(new acmdSetPSMAssociationInterpretation(Controller, psmAssociation, Guid.Empty));
                }
                else
                {
                    PIMAssociationEnd e = (psmAssociation.Interpretation as PIMAssociation).PIMAssociationEnds.First(ae => ae.PIMClass == oldIntContext.Interpretation);
                    command.Commands.Add(new acmdMoveAssociationEnd(Controller, e, newIntContext.Interpretation) { PropagateSource = psmAssociation });
                }
            }
            
            return command;
        }
    }
}
