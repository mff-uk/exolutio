using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Controller.Commands.Reflection;
using EvoX.Model.PIM;
using EvoX.Model;
using EvoX.Model.PSM;
using EvoX.Controller.Commands.Atomic.PSM;
using EvoX.Controller.Commands.Complex.PSM;

namespace EvoX.Controller.Commands.Atomic.PIM.MacroWrappers
{
    [PublicCommand("Synchronize two PIMAttribute sets", PublicCommandAttribute.EPulicCommandCategory.PIM_atomic)]
    public class cmdSynchroPIMAttributes : MacroCommand
    {
        private List<Guid> x1 = new List<Guid>();
        
        private List<Guid> x2 = new List<Guid>();

        public cmdSynchroPIMAttributes() { }

        public cmdSynchroPIMAttributes(Controller c)
            : base(c) { }
        
        [PublicArgument("Class of the synchronized attributes", typeof(PIMClass))]
        [Scope(ScopeAttribute.EScope.PIMClass)]
        public Guid ClassGuid { get; set; }

        [PublicArgument("First set", typeof(PIMAttribute))]
        [ConsistentWith("ClassGuid", PIMClassAttributeParameterConsistency.Key)]
        public List<Guid> X1
        {
            get { return x1; }
            set { x1 = value; }
        }

        [PublicArgument("Second set", typeof(PIMAttribute))]
        [ConsistentWith("ClassGuid", PIMClassAttributeParameterConsistency.Key)]
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
            Commands.Add(new acmdSynchroPIMAttributes(Controller) { X1 = x1, X2 = x2 });
        }

    }
}
