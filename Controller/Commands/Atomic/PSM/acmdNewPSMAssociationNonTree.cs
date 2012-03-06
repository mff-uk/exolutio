using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM
{
    internal class acmdNewPSMAssociationNonTree : AtomicCommand
    {
        private Guid schemaGuid;

        private Guid associationGuid = Guid.Empty;

        private Guid parentGuid;

        private Guid childGuid;

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
                else throw new ExolutioCommandException("Cannot set AssociationGuid after command execution.", this);
            }
        }

        public acmdNewPSMAssociationNonTree(Controller c, Guid psmParentGuid, Guid psmChildGuid, Guid psmSchemaGuid)
            : base(c)
        {
            schemaGuid = psmSchemaGuid;
            parentGuid = psmParentGuid;
            childGuid = psmChildGuid;
        }

        public override bool CanExecute()
        {
            if (parentGuid == Guid.Empty || childGuid == Guid.Empty || schemaGuid == Guid.Empty) return false;
            if (!Project.VerifyComponentType<PSMAssociationMember>(parentGuid)
                || !Project.VerifyComponentType<PSMAssociationMember>(childGuid)
                || !Project.VerifyComponentType<PSMSchema>(schemaGuid)) return false;
            PSMAssociationMember parent = Project.TranslateComponent<PSMAssociationMember>(parentGuid);
            PSMAssociationMember child = Project.TranslateComponent<PSMAssociationMember>(childGuid);
            
            // this is allowed for non-tree associations: 
            //if (child.ParentAssociation != null)
            //{
            //    ErrorDescription = CommandErrors.CMDERR_PARENT_ASSOCIATION_EXISTS;
            //    return false;
            //}

            /*if (child.Interpretation != null)
            {
                if (parent is PSMClass)
                {
                    if ((parent as PSMClass).NearestInterpretedClass() != null) return false;
                }
                else if (parent.NearestInterpretedParentClass() != null)
                {
                    return false;
                }
            }*/
            
            return true;
        }

        internal override void CommandOperation()
        {
            if (AssociationGuid == Guid.Empty) AssociationGuid = Guid.NewGuid();
            PSMAssociationMember child = Project.TranslateComponent<PSMAssociationMember>(childGuid);
            
            //if (child.PSMSchema.Roots.Contains(child))
            //{
            //    rootIndex = child.PSMSchema.Roots.Remove(child);
            //}

            PSMAssociation psmAssociation = new PSMAssociation(
                Project,
                AssociationGuid,
                Project.TranslateComponent<PSMAssociationMember>(parentGuid),
                child,
                Project.TranslateComponent<PSMSchema>(schemaGuid),
                setChildParentAssociation:false
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
            
            s.PSMAssociations.Remove(a);
            Project.mappingDictionary.Remove(a);
            
            return OperationResult.OK;
        }
    }
}
