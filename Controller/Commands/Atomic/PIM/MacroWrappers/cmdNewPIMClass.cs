using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Model;
using EvoX.Model.PIM;
using EvoX.Controller.Commands.Atomic;
using EvoX.Controller.Commands.Atomic.PIM;

namespace EvoX.Controller.Commands.Atomic.PIM.MacroWrappers
{
    [PublicCommand("Create new PIM class", PublicCommandAttribute.EPulicCommandCategory.PIM_atomic)]
    public class cmdCreateNewPIMClass : MacroCommand, ICommandWithDiagramParameter
    {
        public Guid SchemaGuid { get; set; }

        [Scope(ScopeAttribute.EScope.PIMDiagram)]
        [PublicArgument("Diagram", typeof(Diagram), AllowNullInput = true)]
        public Guid DiagramGuid { get; set; }

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
                else throw new EvoXCommandException("Cannot set ClassGuid after command execution.", this);
            }
        }
        
        public cmdCreateNewPIMClass() 
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdCreateNewPIMClass(Controller c)
            : base(c) 
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid schemaGuid)
        {
            SchemaGuid = schemaGuid;
            
        }

        protected override void GenerateSubCommands()
        {
            if (classGuid == Guid.Empty) classGuid = Guid.NewGuid();
            Commands.Add(new acmdNewPIMClass(Controller, SchemaGuid) { ClassGuid = classGuid });
            if (DiagramGuid != Guid.Empty)
            {
                Commands.Add(new acmdAddComponentToDiagram(Controller, ClassGuid, DiagramGuid));
            }
            
        }

    }
}
