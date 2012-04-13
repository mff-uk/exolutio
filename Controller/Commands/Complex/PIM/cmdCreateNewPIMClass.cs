using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Controller.Commands.Atomic;
using Exolutio.Controller.Commands.Atomic.PIM;

namespace Exolutio.Controller.Commands.Complex.PIM
{
    [PublicCommand("Create new PIM class (complex)", PublicCommandAttribute.EPulicCommandCategory.PIM_complex)]
    public class cmdCreateNewPIMClass : ComposedCommand, ICommandWithDiagramParameter
    {
        [PublicArgument("Schema", typeof(Schema), CreateControlInEditors = false, AllowNullInput = true)]
        public Guid SchemaGuid { get; set; }

        [Scope(ScopeAttribute.EScope.PIMDiagram)]
        [PublicArgument("Diagram", typeof(Diagram), AllowNullInput = true)]
        public Guid DiagramGuid { get; set; }

        [PublicArgument("Name", SuggestedValue = "Class")]
        public string Name { get; set; }

        [GeneratedIDArgument("ClassGuid", typeof(PIMClass))]
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

        internal override void GenerateSubCommands()
        {
            if (ClassGuid == Guid.Empty) ClassGuid = Guid.NewGuid();
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
