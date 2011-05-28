using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Create new PSM content model", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdNewPSMContentModel : MacroCommand
    {
        [PublicArgument("Schema", typeof(PSMSchema))]
        [Scope(ScopeAttribute.EScope.PSMSchema)]
        public Guid SchemaGuid { get; set; }

        private Guid cmodelGuid = Guid.Empty;

        [PublicArgument("Type")]
        public PSMContentModelType Type { get; set; }
        
        /// <summary>
        /// If set before execution, creates a new content model with this GUID.
        /// After execution contains GUID of the created content model.
        /// </summary>
        public Guid ContentModelGuid
        {
            get { return cmodelGuid; }
            set
            {
                if (!Executed) cmodelGuid = value;
                else throw new ExolutioCommandException("Cannot set ContentModelGuid after command execution.", this);
            }
        }

        public cmdNewPSMContentModel() { }

        public cmdNewPSMContentModel(Controller c)
            : base(c) { }

        public void Set(PSMContentModelType contentModelType, Guid psmSchemaGuid)
        {
            SchemaGuid = psmSchemaGuid;
            Type = contentModelType;
            
        }

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdNewPSMContentModel(Controller, Type, SchemaGuid));
        }
    }
}
