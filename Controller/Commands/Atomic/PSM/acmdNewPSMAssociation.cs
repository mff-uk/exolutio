using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Model.PSM;
using EvoX.Model;

namespace EvoX.Controller.Commands.Atomic.PSM
{
    public class acmdNewPSMAssociation : StackedCommand
    {
        private Guid schemaGuid;

        private Guid associationGuid = Guid.Empty;

        private Guid parentGuid;

        private Guid childGuid;

        private int rootIndex = -1;

        /// <summary>
        /// If set before execution, creates a new association with this GUID.
        /// After execution contains GUID of the created association.
        /// </summary>
        public Guid AssociationGuid
        {
            get { return associationGuid; }
            set
            {
                if (!Executed) associationGuid = value;
                else throw new EvoXCommandException("Cannot set AssociationGuid after command execution.", this);
            }
        }

        public acmdNewPSMAssociation(Controller c, Guid psmParentGuid, Guid psmChildGuid, Guid psmSchemaGuid)
            : base(c)
        {
            schemaGuid = psmSchemaGuid;
            parentGuid = psmParentGuid;
            childGuid = psmChildGuid;
        }

        public override bool CanExecute()
        {
            return parentGuid != Guid.Empty
                && Project.VerifyComponentType<PSMAssociationMember>(parentGuid)
                && childGuid != Guid.Empty
                && Project.VerifyComponentType<PSMAssociationMember>(childGuid)
                && schemaGuid != Guid.Empty
                && Project.VerifyComponentType<PSMSchema>(schemaGuid);
        }

        internal override void CommandOperation()
        {
            if (AssociationGuid == Guid.Empty) AssociationGuid = Guid.NewGuid();
            PSMAssociationMember child = Project.TranslateComponent<PSMAssociationMember>(childGuid);
            if (child.PSMSchema.Roots.Contains(child))
            {
                rootIndex = child.PSMSchema.Roots.Remove(child);
            }

            PSMAssociation psmAssociation = new PSMAssociation(
                Project,
                AssociationGuid,
                Project.TranslateComponent<PSMAssociationMember>(parentGuid),
                child,
                Project.TranslateComponent<PSMSchema>(schemaGuid)
                );
            Report = new CommandReport(CommandReports.PSM_component_added, psmAssociation);
        }

        internal override CommandBase.OperationResult UndoOperation()
        {
            PSMAssociation a = Project.TranslateComponent<PSMAssociation>(AssociationGuid);
            PSMSchema s = Project.TranslateComponent<PSMSchema>(schemaGuid);
                
            if (a.Parent != null)
            {
                a.Parent.ChildPSMAssociations.Remove(a);
            }
            if (a.Child != null) a.Child.ParentAssociation = null;

            s.PSMAssociations.Remove(a);
            Project.mappingDictionary.Remove(a);

            s.Roots.Insert(Project.TranslateComponent<PSMAssociationMember>(childGuid), rootIndex);

            return OperationResult.OK;
        }
    }
}
