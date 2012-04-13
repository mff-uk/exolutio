using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Atomic;
using Exolutio.Controller.Commands.Atomic.PSM;
using Exolutio.Controller.Commands.Reflection;

namespace Exolutio.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Create new PSM generalization", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdNewPSMGeneralization : MacroCommand
    {
        [GeneratedIDArgument("GeneralizationGuid", typeof(PSMGeneralization))]
        public Guid GeneralizationGuid { get; set; }

        [PublicArgument("Schema", typeof(PSMSchema))]
        [Scope(ScopeAttribute.EScope.PSMSchema)]
        public Guid SchemaGuid { get; set; }
        
        [PublicArgument("General PSM Class", typeof(PSMClass))]
        [ConsistentWith("SchemaGuid", PSMSchemaComponentParameterConsistency.Key)]
        public Guid GeneralClass { get; set; }

        [PublicArgument("Specific PSM Class", typeof(PSMClass))]
        [ConsistentWith("SchemaGuid", PSMSchemaComponentParameterConsistency.Key)]
        public Guid SpecificClass { get; set; }

        public cmdNewPSMGeneralization() 
        {
            CheckFirstOnlyInCanExecute = true;
        }

        public cmdNewPSMGeneralization(Controller c)
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
            Commands.Add(new acmdNewPSMGeneralization(Controller, GeneralClass, SpecificClass, SchemaGuid) { GeneralizationGuid = GeneralizationGuid, Propagate = false });
        }
    }
}
