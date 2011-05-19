using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using EvoX.Controller.Commands;
using EvoX.Model;
using EvoX.Model.PSM;
using EvoX.Controller.Commands.Atomic.PSM;
using EvoX.Controller.Commands.Complex.PSM;

namespace EvoX.Controller.Commands.Atomic.PSM.MacroWrappers
{
    [PublicCommand("Synchronize two PSMAttribute sets", PublicCommandAttribute.EPulicCommandCategory.PSM_atomic)]
    public class cmdSynchroPSMAttributes : MacroCommand
    {
        private List<Guid> x1 = new List<Guid>();
         
        private List<Guid> x2 = new List<Guid>();

        public cmdSynchroPSMAttributes() { }

        public cmdSynchroPSMAttributes(Controller c)
            : base(c) { }

        [PublicArgument("First set", typeof(PSMAttribute))]
        public List<Guid> X1
        {
            get { return x1; }
            set { x1 = value; }
        }

        [PublicArgument("Second set", typeof(PSMAttribute))]
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
            Commands.Add(new acmdSynchroPSMAttributes(Controller) { X1 = x1, X2 = x2 });
        }

    }
}
