using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Controller.Commands.Reflection;
using EvoX.Model.PSM;
using EvoX.Model;

namespace EvoX.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Create new PSM association", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdNewPSMAssociation : MacroCommand
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
        public Guid AssociationGuid
        {
            get { return associationGuid; }
            set
            {
                if (!Executed) associationGuid = value;
                else throw new EvoXCommandException("Cannot set AssociationGuid after command execution.", this);
            }
        }

        public cmdNewPSMAssociation() { }

        public cmdNewPSMAssociation(Controller c)
            : base(c) { }
        
        public void Set(Guid psmParentGuid, Guid psmChildGuid, Guid psmSchemaGuid)
        {
            SchemaGuid = psmSchemaGuid;
            ParentGuid = psmParentGuid;
            ChildGuid = psmChildGuid;
        }

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdNewPSMAssociation(Controller, ParentGuid, ChildGuid, SchemaGuid));
        }

    }
}
