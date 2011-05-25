using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Model.PIM;
using EvoX.Model;
using EvoX.Model.PSM;
using EvoX.Controller.Commands.Atomic.PSM;
using EvoX.Controller.Commands.Complex.PSM;
using EvoX.Controller.Commands.Reflection;

namespace EvoX.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Synchronize two PSMAssociation sets", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdSynchroPSMAssociations : MacroCommand
    {
        private List<Guid> x1 = new List<Guid>();
        
        private List<Guid> x2 = new List<Guid>();

        public cmdSynchroPSMAssociations() { }

        public cmdSynchroPSMAssociations(Controller c)
            : base(c) { }

        [PublicArgument("PSM Schema", typeof(PSMSchema))]
        [Scope(ScopeAttribute.EScope.PSMSchema)]
        public Guid PSMSchemaGuid { get; set; }

        [PublicArgument("First set", typeof(PSMAssociation))]
        [ConsistentWith("PSMSchemaGuid", PSMSchemaComponentParameterConsistency.Key)]
        public List<Guid> X1
        {
            get { return x1; }
            set { x1 = value; }
        }

        [PublicArgument("Second set", typeof(PSMAssociation))]
        [ConsistentWith("PSMSchemaGuid", PSMSchemaComponentParameterConsistency.Key)]
        public List<Guid> X2
        {
            get { return x2; }
            set { x2 = value; }
        }

        public void Set(IEnumerable<Guid> x1, IEnumerable<Guid> x2)
        {
            this.x1 = x1.ToList<Guid>();
            this.x2 = x2.ToList<Guid>();
            
        }

        protected override void GenerateSubCommands()
        {
            Commands.Add(new acmdSynchroPSMAssociations(Controller) { X1 = x1, X2 = x2 });
        }

    }
}
