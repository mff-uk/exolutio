using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Exolutio.Controller.Commands;
using Exolutio.Controller.Commands.Reflection;
using Exolutio.Model.PIM;
using Exolutio.Model;
using Exolutio.Model.PSM;
using Exolutio.Controller.Commands.Atomic.PSM;
using Exolutio.Controller.Commands.Complex.PSM;

namespace Exolutio.Controller.Commands.Atomic.PIM.MacroWrappers
{
    [PublicCommand("Synchronize two PIMAssociation sets", PublicCommandAttribute.EPulicCommandCategory.PIM_atomic)]
    public class cmdSynchroPIMAssociations : MacroCommand
    {
        private List<Guid> x1 = new List<Guid>();
        
        private List<Guid> x2 = new List<Guid>();

        public cmdSynchroPIMAssociations() { }

        public cmdSynchroPIMAssociations(Controller c)
            : base(c) { }

        
        //[PublicArgument("PIM schema", typeof(PIMSchema))]
        //[Scope(ScopeAttribute.EScope.PIMDiagram)]
        //public Guid PIMSchemaGuid { get; set; }
        
        [PublicArgument("First set", typeof(PIMAssociation))]
        //[ConsistentWith("PIMSchemaGuid", PIMSchemaComponentParameterConsistency.Key)]
        public List<Guid> X1
        {
            get { return x1; }
            set { x1 = value; }
        }

        [PublicArgument("Second set", typeof(PIMAssociation))]
        //[ConsistentWith("PIMSchemaGuid", PIMSchemaComponentParameterConsistency.Key)]
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

        internal override void GenerateSubCommands()
        {
            Commands.Add(new acmdSynchroPIMAssociations(Controller) { X1 = x1, X2 = x2 });
        }

    }
}
