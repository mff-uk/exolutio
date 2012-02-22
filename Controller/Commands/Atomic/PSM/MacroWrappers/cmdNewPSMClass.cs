using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Model.PSM;
using Exolutio.Model;

namespace Exolutio.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Create new PSM class", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdNewPSMClass : MacroCommand
    {
        [PublicArgument("Schema", typeof(PSMSchema))]
        [Scope(ScopeAttribute.EScope.PSMSchema)]
        public Guid SchemaGuid { get; set; }

        private Guid classGuid = Guid.Empty;

        /// <summary>
        /// If set before execution, creates a new class with this GUID.
        /// After execution contains GUID of the created class.
        /// </summary>
        public Guid ClassGuid
        {
            get { return classGuid; }
            set
            {
                if (!Executed) classGuid = value;
                else throw new ExolutioCommandException("Cannot set ClassGuid after command execution.", this);
            }
        }

        public cmdNewPSMClass()
        {

        }

        public cmdNewPSMClass(Controller c)
            : base(c)
        {
            
        }

        public void Set(Guid psmSchemaGuid)
        {
            SchemaGuid = psmSchemaGuid;
            
        }

        internal override void GenerateSubCommands()
        {
            if (ClassGuid == Guid.Empty) ClassGuid = Guid.NewGuid();
            Commands.Add(new acmdNewPSMClass(Controller, SchemaGuid) { ClassGuid = ClassGuid });
        }
    }
}
