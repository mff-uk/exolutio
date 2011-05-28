using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Create new PSM schema", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdNewPSMSchema : MacroCommand
    {
        private Guid schemaGuid;

        private Guid schemaClassGuid;
        
        private Guid diagramGuid;


        /// <summary>
        /// If set before execution, creates a new schema with this GUID.
        /// After execution contains GUID of the created schema.
        /// </summary>
        public Guid SchemaGuid
        {
            get { return schemaGuid; }
            set
            {
                if (!Executed) schemaGuid = value;
                else throw new ExolutioCommandException("Cannot set SchemaGuid after command execution.", this);
            }
        }

        /// <summary>
        /// If set before execution, creates a new diagram with this GUID.
        /// After execution contains GUID of the created diagram.
        /// </summary>
        public Guid DiagramGuid
        {
            get { return diagramGuid; }
            set
            {
                if (!Executed) diagramGuid = value;
                else throw new ExolutioCommandException("Cannot set DiagramGuid after command execution.", this);
            }
        }

        /// <summary>
        /// If set before execution, creates a new schemaClass with this GUID.
        /// After execution contains GUID of the created schemaClass.
        /// </summary>
        public Guid SchemaClassGuid
        {
            get { return schemaClassGuid; }
            set
            {
                if (!Executed) schemaClassGuid = value;
                else throw new ExolutioCommandException("Cannot set SchemaClassGuid after command execution.", this);
            }
        }

        public cmdNewPSMSchema() { }
        
        public cmdNewPSMSchema(Controller c)
            : base(c) { }

        protected override void GenerateSubCommands()
        {
            if (SchemaGuid == Guid.Empty) SchemaGuid = Guid.NewGuid();
            Commands.Add(new acmdNewPSMSchema(Controller) { SchemaGuid = SchemaGuid, SchemaClassGuid = SchemaClassGuid });
            Commands.Add(new acmdNewPSMDiagram(Controller) { SchemaGuid = SchemaGuid, DiagramGuid = DiagramGuid });
        }

    }
}
