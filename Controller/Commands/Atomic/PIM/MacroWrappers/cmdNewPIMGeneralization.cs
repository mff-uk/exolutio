using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model;
using Exolutio.Model.PIM;
using Exolutio.Controller.Commands.Atomic;
using Exolutio.Controller.Commands.Atomic.PIM;

namespace Exolutio.Controller.Commands.Atomic.PIM.MacroWrappers
{
    [PublicCommand("Create new PIM generalization", PublicCommandAttribute.EPulicCommandCategory.PIM_atomic)]
    public class cmdNewPIMGeneralization : WrapperCommand, ICommandWithDiagramParameter
    {
        [GeneratedIDArgument("GeneralizationGuid", typeof(PIMGeneralization))]
        public Guid GeneralizationGuid { get; set; }

        [PublicArgument("Schema", typeof(Schema), CreateControlInEditors = false, AllowNullInput = true)]
        public Guid SchemaGuid { get; set; }
        
        [Scope(ScopeAttribute.EScope.PIMDiagram)]
        [PublicArgument("Diagram", typeof(Diagram), AllowNullInput = true)]
        public Guid DiagramGuid { get; set; }

        [PublicArgument("General PIM Class", typeof(PIMClass))]
        public Guid GeneralClass { get; set; }

        [PublicArgument("Specific PIM Class", typeof(PIMClass))]
        public Guid SpecificClass { get; set; }

        public cmdNewPIMGeneralization() 
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdNewPIMGeneralization(Controller c)
            : base(c) 
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public void Set(Guid generalClass, Guid specificClass, Guid schemaGuid)
        {
            GeneralClass = generalClass;
            SpecificClass = specificClass;
            SchemaGuid = schemaGuid;
            
        }

        internal override void GenerateSubCommands()
        {
            if (GeneralizationGuid == Guid.Empty) GeneralizationGuid = Guid.NewGuid();
            Commands.Add(new acmdNewPIMGeneralization(Controller, GeneralClass, SpecificClass, SchemaGuid) { GeneralizationGuid = GeneralizationGuid, Propagate = false });
            if (DiagramGuid != Guid.Empty)
            {
                Commands.Add(new acmdAddComponentToDiagram(Controller, GeneralizationGuid, DiagramGuid));
            }
        }

    }
}
