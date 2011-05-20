using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Model;
using EvoX.Model.PIM;
using EvoX.Controller.Commands.Atomic;
using EvoX.Controller.Commands.Atomic.PIM;

namespace EvoX.Controller.Commands.Complex.PIM
{
    [PublicCommand("Create new PIM class (complex)", PublicCommandAttribute.EPulicCommandCategory.PIM_complex)]
    public class cmdCreateNewPIMClass : MacroCommand, ICommandWithDiagramParameter
    {
        public Guid SchemaGuid { get; set; }

        [Scope(ScopeAttribute.EScope.PIMDiagram)]
        [PublicArgument("Diagram", typeof(Diagram), AllowNullInput = true)]
        public Guid DiagramGuid { get; set; }

        [PublicArgument("Name", SuggestedValue = "Class")]
        public string Name { get; set; }

        public Guid ClassGuid { get; set; }
        
        public cmdCreateNewPIMClass()
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdCreateNewPIMClass(Controller c)
            : base(c)
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(string name, Guid schemaGuid)
        {
            Name = name;
            SchemaGuid = schemaGuid;
            
        }

        protected override void GenerateSubCommands()
        {
            ClassGuid = Guid.NewGuid();
            Commands.Add(new acmdNewPIMClass(Controller, SchemaGuid) { ClassGuid = ClassGuid });
            Commands.Add(new acmdRenameComponent(Controller, ClassGuid, Name));
            if (DiagramGuid != Guid.Empty)
            {
                Commands.Add(new acmdAddComponentToDiagram(Controller, ClassGuid, DiagramGuid));
            }
        }

        public override bool CanExecute()
        {
            if (Name == null || SchemaGuid == Guid.Empty) return false;
            return base.CanExecute();
        }

        internal override void CommandOperation()
        {
            base.CommandOperation();
            Report = new CommandReport(CommandReports.COMPLEX_NEW_PIM_CLASS);
        }

    }
}
