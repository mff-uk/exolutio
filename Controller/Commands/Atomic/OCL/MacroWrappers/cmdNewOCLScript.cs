using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Controller.Commands.Atomic;
using Exolutio.Controller.Commands.Atomic.PIM;
using Exolutio.Model.PSM;

namespace Exolutio.Controller.Commands.Atomic.PIM.MacroWrappers
{
    //[PublicCommand("Create new OCL script", PublicCommandAttribute.EPulicCommandCategory.PIM_atomic)]
    public class cmdCreateNewOCLScript : MacroCommand, ICommandWithDiagramParameter
    {
        //[PublicArgument("Schema", typeof(PSMSchema))]
        [Scope(ScopeAttribute.EScope.PSMSchema)]
        public Guid SchemaGuid { get; set; }

        [Scope(ScopeAttribute.EScope.PIMDiagram)]
        [PublicArgument("Diagram", typeof(Diagram), AllowNullInput = true)]
        public Guid DiagramGuid { get; set; }

        private Guid oclScriptGuid = Guid.Empty;
        
        /// <summary>
        /// If set before execution, creates a new class with this GUID.
        /// After execution contains GUID of the created class.
        /// </summary>
        public Guid OCLScriptGuid
        {
            get { return oclScriptGuid; }
            set
            {
                if (!Executed) oclScriptGuid = value;
                else throw new ExolutioCommandException("Cannot set ClassGuid after command execution.", this);
            }
        }
        
        public cmdCreateNewOCLScript() { }

        public cmdCreateNewOCLScript(Controller c)
            : base(c) { }

        public void Set(Guid schemaGuid)
        {
            SchemaGuid = schemaGuid;
        }

        protected override void GenerateSubCommands()
        {
            if (oclScriptGuid == Guid.Empty) oclScriptGuid = Guid.NewGuid();
            Commands.Add(new acmdNewOCLScript(Controller, SchemaGuid) { OCLScriptGuid = oclScriptGuid });
            if (DiagramGuid != Guid.Empty)
            {
                //Commands.Add(new acmdAddComponentToDiagram(Controller, OCLScriptGuid, DiagramGuid));
            }
        }
    }
}
