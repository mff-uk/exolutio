using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;
using Exolutio.Model.PIM;

namespace Exolutio.Controller.Commands.Atomic.PSM
{
    internal class acmdDeletePSMAssociation : AtomicCommand
    {
        private Guid schemaGuid;

        private Guid associationGuid;

        private Guid interpretation;

        private Guid parentGuid = Guid.Empty;

        private Guid childGuid = Guid.Empty;

        int index;

        public acmdDeletePSMAssociation(Controller c, Guid psmAssociation)
            : base(c)
        {
            associationGuid = psmAssociation;
            schemaGuid = Project.TranslateComponent<PSMAssociation>(associationGuid).PSMSchema;
        }

        public override bool CanExecute()
        {
            if (!(schemaGuid != Guid.Empty
                && Project.VerifyComponentType<PSMSchema>(schemaGuid)
                && associationGuid != Guid.Empty
                && Project.VerifyComponentType<PSMAssociation>(associationGuid)))
            {
                ErrorDescription = CommandErrors.CMDERR_INPUT_TYPE_MISMATCH;
                return false;
            }

            PSMAssociation a = Project.TranslateComponent<PSMAssociation>(associationGuid);

            if (a.Interpretation != null) return true;
            
            if (a.Child != null && a.Child is PSMClass && (a.Child as PSMClass).Interpretation != null) return true;

            PSMAssociationMember c = a.Child; //Uninterpreted child

            IEnumerable<PSMClass> unInterpretedSubClasses = c.UnInterpretedSubClasses(true);
            //PSM attributes within the uninterpreted PSM Class subtree cannot have interpretations
            if (!unInterpretedSubClasses
                  .SelectMany<PSMClass, PSMAttribute>(cl => cl.PSMAttributes)
                  .All<PSMAttribute>(at => at.Interpretation == null)
                )
            {
                ErrorDescription = CommandErrors.CMDERR_UNINTERPRETED_SUBCLASS_ATTRIBUTES_INTERPRETED;
                return false;
            }

            //PSM associations within the uninterpreted PSM Class subtree cannot have interpretations
            if (!unInterpretedSubClasses
                  .Select<PSMClass, PSMAssociation>(cl => cl.ParentAssociation)
                  .All<PSMAssociation>(assoc => assoc.Interpretation == null)
                )
            {
                ErrorDescription = CommandErrors.CMDERR_UNINTERPRETED_SUBCLASS_ASSOCIATIONS_INTERPRETED;
                return false;
            }
            
            return true;
        }

        internal override void CommandOperation()
        {
            PSMAssociation a = Project.TranslateComponent<PSMAssociation>(associationGuid);
            PSMSchema s = Project.TranslateComponent<PSMSchema>(schemaGuid);

            if (a.Parent != null)
            {
                index = a.Parent.ChildPSMAssociations.IndexOf(a);
                a.Parent.ChildPSMAssociations.Remove(a);
                parentGuid = a.Parent;
            }
            //else throw new ExolutioCommandException("Deleted association " + a.ToString() + " had null Parent", this);
            if (a.Child != null)
            {
                a.Child.ParentAssociation = null;
                childGuid = a.Child;
                s.Roots.Add(a.Child);
            }
            //else throw new ExolutioCommandException("Deleted association " + a.ToString() + " had null Child", this);

            interpretation = a.Interpretation;
            s.PSMAssociations.Remove(a);
            Project.mappingDictionary.Remove(a);
            Report = new CommandReport(CommandReports.PSM_component_deleted, a);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            if (childGuid != Guid.Empty)
            {
                PSMAssociationMember child = Project.TranslateComponent<PSMAssociationMember>(childGuid);
                child.PSMSchema.Roots.Remove(child);
            }
            new PSMAssociation(
                Project,
                associationGuid,
                parentGuid == Guid.Empty ? null : Project.TranslateComponent<PSMAssociationMember>(parentGuid),
                childGuid == Guid.Empty ? null : Project.TranslateComponent<PSMAssociationMember>(childGuid),
                index,
                Project.TranslateComponent<PSMSchema>(schemaGuid)
                ) { Interpretation = interpretation == Guid.Empty ? null : Project.TranslateComponent<PIMComponent>(interpretation) };

            return OperationResult.OK;
        }
    }
}
