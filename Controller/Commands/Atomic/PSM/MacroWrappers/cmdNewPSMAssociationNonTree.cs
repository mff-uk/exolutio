﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Controller.Commands.Reflection;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Create new PSM association (Non tree)", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdNewPSMAssociationNonTree : WrapperCommand
    {
        [PublicArgument("Schema", typeof(PSMSchema))]
        [Scope(ScopeAttribute.EScope.PSMSchema)]
        public Guid SchemaGuid { get; set; }

        [PublicArgument("Parent class or content model", typeof(PSMAssociationMember))]
        [ConsistentWith("SchemaGuid", PSMSchemaComponentParameterConsistency.Key)]
        public Guid ParentGuid { get; set; }

        [PublicArgument("Child class or content model", typeof(PSMAssociationMember))]
        [ConsistentWith("SchemaGuid", PSMSchemaComponentParameterConsistency.Key)]
        public Guid ChildGuid { get; set; }

        private Guid associationGuid = Guid.Empty;

        /// <summary>
        /// If set before execution, creates a new association with this GUID.
        /// After execution contains GUID of the created association.
        /// </summary>
        [GeneratedIDArgument("AssociationGuid", typeof(PSMAssociation))]
        public Guid AssociationGuid
        {
            get { return associationGuid; }
            set
            {
                if (!Executed) associationGuid = value;
                else throw new ExolutioCommandException("Cannot set AssociationGuid after command execution.", this);
            }
        }

        public cmdNewPSMAssociationNonTree() { }

        public cmdNewPSMAssociationNonTree(Controller c)
            : base(c) { }
        
        public void Set(Guid psmParentGuid, Guid psmChildGuid, Guid psmSchemaGuid)
        {
            SchemaGuid = psmSchemaGuid;
            ParentGuid = psmParentGuid;
            ChildGuid = psmChildGuid;
        }

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdNewPSMAssociationNonTree(Controller, ParentGuid, ChildGuid, SchemaGuid) { AssociationGuid = AssociationGuid } );
        }

    }
}
